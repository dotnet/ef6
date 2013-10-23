// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Text.RegularExpressions;
using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TestTools.Common;
using Microsoft.Win32;
using Process = System.Diagnostics.Process; // Ambiguos with EnvDte.Process.
using VsipErrorHandler = Microsoft.VisualStudio.ErrorHandler;

namespace Microsoft.VisualStudio.TestTools.HostAdapters.VsIde
{
    /// <summary>
    /// Used to start VS.
    /// </summary>
    public class VsIdeStartupInfo
    {
        // TODO: refactor classes so that we don't keep registry hive + additional command line args + something else
        //       on each level (runconfig data, startup info, adapter, vs ide class), to make addining new parameters easier in future.

        private string m_registryHive;
        private string m_workingDirectory;
        private string m_additionalCommandLineArgs;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="registryHive">Can be empty in which case we'll auto figure out by taking the current non-suffixed hive.</param>
        /// <param name="additionalCommandLineArgs">
        /// Command line arguments in addition to ones imposed by the hive parameter. 
        /// Does not override hive but adds to it.</param>
        /// <param name="workingDirectory">Working directory for devenv.exe process.</param>
        public VsIdeStartupInfo(string registryHive, string additionalCommandLineArgs, string workingDirectory)
        {
            // Note: registryHive can be null when using the attribute. That's OK, VsIde will figure out.
            Debug.Assert(!string.IsNullOrEmpty(workingDirectory));

            m_registryHive = registryHive;
            m_additionalCommandLineArgs = additionalCommandLineArgs;
            m_workingDirectory = workingDirectory;
        }

        /// <summary>
        /// Hive name under Microsoft.VisualStudio, like 8.0Exp.
        /// </summary>
        internal string RegistryHive
        {
            get { return m_registryHive; }
            set { m_registryHive = value; } 
        }

        /// <summary>
        /// Working directory for devenv.exe process.
        /// </summary>
        internal string WorkingDirectory
        {
            get { return m_workingDirectory; }
        }

        /// <summary>
        /// Command line arguments in addition to ones imposed by the hive parameter. 
        /// These do not override arguments imposed by the RegistryHive property but add to it.
        /// </summary>
        internal string AdditionalCommandLineArguments
        {
            get { return m_additionalCommandLineArgs; }
        }
    }

    internal delegate void VsIdeHostErrorHandler(string errorText, TestOutcome outcome, bool abortTestRun);

    /// <summary>
    /// This wraps Visual Studio DTE.
    /// </summary>
    public class VisualStudioIde : IDisposable
    {
        #region Private
        private const string BaseProgId = "VisualStudio.DTE";

        // How long to wait for IDE to appear in ROT.
        private static TimeSpan s_ideStartupTimeout = TimeSpan.FromMilliseconds(RegistrySettings.BaseTimeout * 600);
        // How long to wait before killing devenv.exe after Dispose() is called. During this time VS can e.g. save buffers to disk.
        private static TimeSpan s_ideExitTimeout = TimeSpan.FromMilliseconds(RegistrySettings.BaseTimeout * 5);
        // Timeout to wait while VS rejects calls.
        private static TimeSpan s_rejectedCallTimeout = TimeSpan.FromMilliseconds(RegistrySettings.BaseTimeout * 30);

        // HRESULT for "call rejected by callee" error
        private const int CallRejectedByCalleeErrorCode = -2147418111;
        private const int RpcServerUnavailableErrorCode = -2147023174;

        private DTE m_dte;
        private System.Diagnostics.Process m_process;
        private object m_cleanupLock = new object();
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor. Starts new instance of VS IDE.
        /// </summary>
        public VisualStudioIde(VsIdeStartupInfo info)
        {
            Debug.Assert(info != null);

            StartNewInstance(info);
        }

        ~VisualStudioIde()
        {
            Dispose(false);
        }
        #endregion

        #region Properties
        [CLSCompliant(false)]
        public DTE Dte
        {
            get { return m_dte; }
        }

        public Process Process
        {
            get { return m_process; }
        }

        internal event VsIdeHostErrorHandler ErrorHandler;
        #endregion

        #region Private
        // Create a DevEnv process
        private void StartNewInstance(VsIdeStartupInfo info)
        {
            Debug.Assert(info != null);
            Debug.Assert(m_process == null, "VisualStudioIde.StartNewInstance: m_process should be null!");

            if (string.IsNullOrEmpty(info.RegistryHive))
            {
                info.RegistryHive = VSRegistry.GetDefaultVersion();
                if (string.IsNullOrEmpty(info.RegistryHive))
                {
                    // Please no Debug.Assert. This is a valid case.
                    throw new VsIdeTestHostException(Resources.CannotFindVSInstallation(info.RegistryHive));
                }
            }

            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            if (info.WorkingDirectory != null)
            {
                process.StartInfo.WorkingDirectory = info.WorkingDirectory;
            }

            process.StartInfo.FileName = VSRegistry.GetVSLocation(info.RegistryHive);
            Debug.Assert(!string.IsNullOrEmpty(process.StartInfo.FileName));

            // Note that this needs to be partial (not $-terminated) as we partially match/replace.
            Regex versionRegex = new Regex(@"^[0-9]+\.[0-9]+");

            string hiveVersion = versionRegex.Match(info.RegistryHive).Value;
            string hiveSuffix = versionRegex.Replace(info.RegistryHive, string.Empty);

            if (!string.IsNullOrEmpty(hiveSuffix))
            {
                process.StartInfo.Arguments = "/RootSuffix " + hiveSuffix;
            }

            if (!string.IsNullOrEmpty(info.AdditionalCommandLineArguments))
            {
                if (!string.IsNullOrEmpty(process.StartInfo.Arguments))
                {
                    process.StartInfo.Arguments += " ";
                }
                process.StartInfo.Arguments += info.AdditionalCommandLineArguments;
            }

            process.Exited += new EventHandler(ProcessExited);
            process.EnableRaisingEvents = true;

            if (!process.Start())
            {
                throw new VsIdeTestHostException(Resources.FailedToStartVSProcess);
            }

            m_process = process;

            string progId = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", VisualStudioIde.BaseProgId, hiveVersion);
            m_dte = GetDteFromRot(progId, m_process.Id);
            Debug.Assert(m_dte != null);
        }

        private static DTE GetDteFromRot(string progId, int processId)
        {
            Debug.Assert(!string.IsNullOrEmpty(progId));

            EnvDTE.DTE dte;
            string moniker = string.Format(CultureInfo.InvariantCulture, "!{0}:{1}", progId, processId);

            // It takes some time after process started to register in ROT.
            Stopwatch sw = Stopwatch.StartNew();
            do
            {
                dte = GetDteFromRot(moniker);
                if (dte != null)
                {
                    break;
                }
                System.Threading.Thread.Sleep(RegistrySettings.BaseSleepDuration * 2);
            } while (sw.Elapsed < s_ideStartupTimeout);

            if (dte == null)
            {
                throw new VsIdeTestHostException(String.Format(CultureInfo.InvariantCulture, Resources.TimedOutGettingDteFromRot, progId));
            }
            return dte;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static DTE GetDteFromRot(string monikerName)
        {
            Debug.Assert(!string.IsNullOrEmpty(monikerName));

            IRunningObjectTable rot;
            IEnumMoniker monikerEnumerator;
            object dte = null;
            try
            {
                int result = NativeMethods.GetRunningObjectTable(0, out rot);
                if (result != NativeMethods.S_OK)
                {
                    Marshal.ThrowExceptionForHR(result);
                }

                rot.EnumRunning(out monikerEnumerator);
                VsipErrorHandler.ThrowOnFailure(monikerEnumerator.Reset());

                uint fetched = 0;
                IMoniker[] moniker = new IMoniker[1];
                while (monikerEnumerator.Next(1, moniker, out fetched) == 0)
                {
                    IBindCtx bindingContext;
                    result = NativeMethods.CreateBindCtx(0, out bindingContext);
                    if (result != NativeMethods.S_OK)
                    {
                        Marshal.ThrowExceptionForHR(result);
                    }

                    string name;
                    try
                    {
                        moniker[0].GetDisplayName(bindingContext, null, out name);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Some processes will throw an exception 
                        // when trying to access them. We will just
                        // skip those.
                        continue;
                    }

                    if (name == monikerName)
                    {
                        object returnObject;
                        rot.GetObject(moniker[0], out returnObject);
                        dte = (object)returnObject;
                        break;
                    }
                }
            }
            catch
            {
                return null;
            }

            return (DTE)dte;
        }

        private void ProcessExited(object sender, EventArgs args)
        {
            lock (m_cleanupLock)
            {
                m_process.EnableRaisingEvents = false;
                m_process.Exited -= new EventHandler(ProcessExited);

                if (ErrorHandler != null)
                {
                    ErrorHandler(Resources.VSExitedUnexpectedly, TestOutcome.Error, false);
                }
            }
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Dispose(bool disposingNotFinalizing)
        {
            if (!disposingNotFinalizing)
            {
                // When called from finalizer, just clean up. Don't lock. Don't throw.
                KillProcess();
            }
            else
            {
                lock (m_cleanupLock)
                {
                    if (m_process.EnableRaisingEvents)
                    {
                        m_process.EnableRaisingEvents = false;
                        m_process.Exited -= new EventHandler(ProcessExited);
                    }

                    try
                    {
                        if (m_dte != null)
                        {
                            // Visual Studio sometimes rejects the call to Quit() so we need to retry it.
                            Stopwatch sw = Stopwatch.StartNew();
                            bool timedOut = true;
                            do
                            {
                                try
                                {
                                    m_dte.Quit();
                                    timedOut = false;
                                    break;
                                }
                                catch (COMException ex)
                                {
                                    if (ex.ErrorCode == CallRejectedByCalleeErrorCode)
                                    {
                                        System.Threading.Thread.Sleep(RegistrySettings.BaseSleepDuration * 2);
                                    }
                                    else if (ex.InnerException is RemotingException)
                                    {
                                        //Failed to communicate with the VS IDE instance. Continuing.
                                        //This should Kill the Process if its still running.
                                        timedOut = false;
                                        break;
                                    }
                                    else
                                    {
                                        // Some unexpected failure.
                                        // E.g. you get RpcServerUnavailableErrorCode if for some reason devenv is not available anymore -- maybe
                                        // the process has died or killed by the user. In other cases you may get rpc call failed.
                                        // In these cases devenv is quitted unexpectedly, test run should have already reported error.
                                        // If it doesn't, since this will throw a invalidoperationexception below, trun will fail. 
                                        //Debug.Assert(!RegistrySettings.VerboseAssertionsEnabled, ex.ToString());
                                        throw;
                                    }
                                }
                                catch (InvalidComObjectException)
                                {
                                    // This Exception happens after the test result has been reported. 
                                    timedOut = false;
                                    break;
                                }
                            } while (sw.Elapsed < s_rejectedCallTimeout);

                            if (timedOut)
                            {
                                throw new VsIdeTestHostException(Resources.TimedOutWaitingDteQuit);
                            }
                        }
                    }
                    finally
                    {
                        KillProcess();
                    }
                }
            }
        }

        private void KillProcess()
        {
            if (m_process != null)
            {
                // wait for the specified time for the IDE to exit.  
                // If it hasn't, kill the process so we can proceed to the next test.
                Stopwatch sw = Stopwatch.StartNew();
                while (!m_process.HasExited && (sw.Elapsed < s_ideExitTimeout))
                {
                    System.Threading.Thread.Sleep(RegistrySettings.BaseSleepDuration);
                }

                if (!m_process.HasExited)
                {
                    m_process.Kill();
                }

                m_process = null;
            }
        }
        #endregion
    }
}
