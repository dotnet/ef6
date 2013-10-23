// Copyright (c) Microsoft Corporation.  All rights reserved.

using EnvDTE;
using Microsoft.VisualStudio.TestTools.Common;
using Microsoft.VisualStudio.TestTools.Execution;
using Microsoft.VisualStudio.TestTools.TestAdapter;
using Microsoft.VisualStudio.TestTools.VsIdeTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Thread = System.Threading.Thread;

[module: SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase",
    Scope = "namespace", Target = "Microsoft.VisualStudio.TestTools.HostAdapters.VsIde")] // Be consistent with VS Interop Assemblies.
namespace Microsoft.VisualStudio.TestTools.HostAdapters.VsIde
{
    /// <summary>
    /// Vs Ide Host Adapter: Agent side.
    /// This wraps ITestAdapter and looks like ITestAdapter for the Agent.
    /// Internally it delegates to original test adapter hosted by Visual Studio IDE.
    /// 
    /// Tweaks:
    /// Registry: HKCU\SOFTWARE\Microsoft\VisualStudio\VersionMajor.VersionMinor\EnterpriseTools\QualityTools\HostAdapters\VS IDE\
    /// - RestartVsOneTime (DWORD): 
    ///   If set to 1, HA will restart VS BEFORE next test, then set the value of this to 0, so this works only 1 time, like auto-reset event.
    /// - RegistryHiveOverride (string): overrides Run Config hive setting, also can be used for running with attribute
    /// </summary>
    internal class VsIdeHostAdapter : ILoadTestAdapter, ITimeoutTestAdapter
    {
        #region Consts
        internal const string Name = "VS IDE";
        #endregion

        #region Private Data
        private IRunContext m_runContext;
        private TestRunConfiguration m_runConfig;
        private string m_workingDir;
        private HostAdapterHostSide m_hostSide; // Can be ITestAdapter type.
        private object m_hostSideLock = new object();
        private IVsIdeTestHostAddin m_testHostAddin;
        private IChannel m_clientChannel;
        private IChannel m_serverChannel;
        private VisualStudioIde m_vsIde;
        private IVsIdeHostDebugger m_hostSession;
        private string m_vsRegistryHive;    // This is like <version> or <version>Exp.
        private string m_additionalCommandLine;
        private bool m_isHostSideDirty;     // Means: there was at least 1 tests that used the IDE.
        private RetryMessageFilter m_comMessageFilter;
        private static TimeSpan s_addinWaitTimeout = TimeSpan.FromMilliseconds(RegistrySettings.BaseTimeout * 270); // For how long time to poll VS to load plugins.
        private object m_loadRunLock = new object();
        #endregion

        #region Constructor
        /// <summary>
        /// This is called by Agent and should not have any parameters.
        /// </summary>
        public VsIdeHostAdapter()
        {
        }
        #endregion

        #region IBaseAdapter, ITestAdapter, ILoadTestAdapter
        /// <summary>
        /// ITestAdapter method: called to initialize run context for this adapter.
        /// </summary>
        /// <param name="runContext">The run context to be used for this run</param>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]  // Base param name is incorrect.
        void ITestAdapter.Initialize(IRunContext runContext)
        {
            Debug.Assert(runContext != null);

            m_runContext = runContext;
            m_runConfig = m_runContext.RunConfig.TestRun.RunConfiguration;
            m_workingDir = m_runContext.RunContextVariables.GetStringValue("TestDeploymentDir");

            Debug.Assert(m_runConfig != null);
            Debug.Assert(!string.IsNullOrEmpty(m_workingDir));

            SetupChannels();

            // TODO: consider more reliable mechanism for hadling VS being busy. Com message filter is only for STA/same thread.
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                // Install COM message filter to retry COM calls when VS IDE is busy, e.g. when getting the addin from VS IDE.
                // This prevents RPC_E_CALL_REJECTED error when VS IDE is busy.
                try
                {
                    m_comMessageFilter = new RetryMessageFilter();
                }
                catch (COMException ex)
                {
                    string message = "Failed to create COM filter (ignorable): " + ex.ToString();
                    Debug.Fail(message);
                    SendResult(ex.Message, TestOutcome.Warning, false);
                    // Ignore the exception, continue without the filter.
                }
            }
            else
            {
                TraceMessage("COM message filter is disabled because it can be registered only in STA apartment, and current apartment is " +
                    Thread.CurrentThread.GetApartmentState().ToString());
            }

            InitHostSession();
        }

        /// <summary>
        /// IBaseAdapter method: called to execute a test.
        /// </summary>
        /// <param name="testElement">The test object to run</param>
        /// <param name="testContext">The Test conext for this test invocation</param>
        void IBaseAdapter.Run(ITestElement testElement, ITestContext testContext)
        {
            PrePostHostCodeExecution(testElement, testContext, ExecutionType.PreHost);
            CheckRestartVs();
            DateTime testStartTime = DateTime.Now;
            try
            {
                ((ITestAdapter)m_hostSide).Run(testElement, testContext);
                Trace.WriteLine("RUN method completed.....");

                if (m_runContext.RunConfig.TestRun.Result.Outcome != TestOutcome.Passed &&
                    m_runContext.RunConfig.TestRun.Result.Outcome != TestOutcome.PassedButRunAborted)
                    TakeScreenShot("ScreenShot_" + m_runContext.RunConfig.TestRun.Result.Outcome.ToString() + "_" + testElement.Name);

            }
            catch (RemotingException remotingException)
            {
                AbortSingleTestResult("Remoting Exception in IBaseAdapter.Run(): " + remotingException.Message, testElement, testStartTime);
            }

            m_isHostSideDirty = true;
            
            PrePostHostCodeExecution(testElement, testContext, ExecutionType.PostHost);
        }

        /// <summary>
        /// ILoadTestAdapter method: called to execute a test. May be called in parallel with other tests.
        /// </summary>
        /// <param name="testElement">The test object to run</param>
        /// <param name="testContext">The Test conext for this test invocation</param>
        void ILoadTestAdapter.LoadRun(ITestElement testElement, ITestContext testContext)
        {
            // We want to be able to run under a load test, but at least for now we don't support actually producing load for
            // more than one user (one test at a time). So, we enforce that with a lock. The load test should still be
            // configured to use only one user in the scenario in which this test is included.
            lock (m_loadRunLock)
            {
                ((IBaseAdapter)this).Run(testElement, testContext);
            }
        }

        /// <summary>
        /// ITimeoutTestAdapter method: called when a test times out, to clean up the test
        /// </summary>
        /// <param name="test">The test that timed out</param>
        void ITimeoutTestAdapter.TestTimeout(ITestElement test)
        {
            TakeScreenShot("ScreenShot_Timeout_" + test.Name);
            try
            {
                ((ITimeoutTestAdapter)m_hostSide).TestTimeout(test);
            }
            catch (RemotingException remotingException)
            {
                SendResult("Remoting Exception in ITimeoutTestAdapter.TestTimeout(): " + remotingException.Message, TestOutcome.Warning);
            }
        }

        /// <summary>
        /// IBaseAdapter method: called when the test run is complete.
        /// </summary>
        void IBaseAdapter.Cleanup()
        {
            try
            {
                CleanupHostSide();
                CleanupHostSession();

                // Uninstall COM message filter.
                if (m_comMessageFilter != null)
                {
                    m_comMessageFilter.Dispose();
                    m_comMessageFilter = null;
                }
            }
            finally
            {
                CleanupChannels();
            }
        }

        /// <summary>
        /// IBaseAdapter method: called when the user stops the test run.
        /// </summary>
        void IBaseAdapter.StopTestRun()
        {
            try
            {
                ((ITestAdapter)HostSide).StopTestRun();
            }
            catch (RemotingException remotingException)
            {
                SendResult("Remoting Exception in IBaseAdapter.StopTestRun(): " + remotingException.Message, TestOutcome.Warning);
            }
        }

        /// <summary>
        /// IBaseAdapter method: called when the test run is aborted.
        /// </summary>
        void IBaseAdapter.AbortTestRun()
        {
            TakeScreenShot("ScreenShot_Aborted");
            try
            {
                ((ITestAdapter)HostSide).AbortTestRun();
            }
            catch (RemotingException remotingException)
            {
                SendResult("Remoting Exception in IBaseAdapter.AbortTestRun(): " + remotingException.Message, TestOutcome.Warning);
            }
        }

        /// <summary>
        /// IBaseAdapter method: called when the user pauses the test run.
        /// </summary>
        void IBaseAdapter.PauseTestRun()
        {
            try
            {
                ((ITestAdapter)HostSide).PauseTestRun();
            }
            catch (RemotingException remotingException)
            {
                SendResult("Remoting Exception in IBaseAdapter.PauseTestRun(): " + remotingException.Message, TestOutcome.Warning);
            }
        }

        /// <summary>
        /// IBaseAdapter method: called when the user resumes a paused test run.
        /// </summary>
        void IBaseAdapter.ResumeTestRun()
        {
            try
            {
                ((ITestAdapter)HostSide).ResumeTestRun();
            }
            catch (RemotingException remotingException)
            {
                SendResult("Remoting Exception in IBaseAdapter.ResumeTestRun(): " + remotingException.Message, TestOutcome.Warning);
            }
        }

        /// <summary>
        /// ITestAdapter method: called when a message is sent from the UI or the controller.
        /// </summary>
        /// <param name="obj">The message object</param>
        void ITestAdapter.ReceiveMessage(object obj)
        {
            try
            {
                ((ITestAdapter)HostSide).ReceiveMessage(obj);
            }
            catch (RemotingException remotingException)
            {
                SendResult("Remoting Exception in IBaseAdapter.RecieveMessage(): " + remotingException.Message, TestOutcome.Warning);
            }
        }

        /// <summary>
        /// ITestAdapter method: called just before the test run finishes and
        /// gives the adapter a chance to do any clean-up.
        /// </summary>
        /// <param name="runContext">The run context for this run</param>
        void ITestAdapter.PreTestRunFinished(IRunContext runContext)
        {
            try
            {
                ((ITestAdapter)HostSide).PreTestRunFinished(runContext);
            }
            catch (RemotingException remotingException)
            {
                SendResult("Remoting Exception in IBaseAdapter.PreTestRunFinished(): " + remotingException.Message, TestOutcome.Inconclusive);
            }
        }
        #endregion

        #region Private
        private HostAdapterHostSide HostSide
        {
            get
            {
                return m_hostSide;
            }
        }

        /// <summary>
        /// Creates VS IDE and HostSide inside it.
        /// This can be called multiple times in the run if specified to restart VS between tests.
        /// </summary>
        private void InitHostSide()
        {
            Debug.Assert(m_runContext != null);
            try
            {
                m_vsRegistryHive = GetRegistryHive();
                m_additionalCommandLine = GetAdditionalCommandLine();

                lock (m_hostSideLock)
                {
                    // Get the "host side" of the host adapter.
                    CreateHostSide();

                    try
                    {
                        // Call Initialize for the host side.
                        ((ITestAdapter)HostSide).Initialize(m_runContext);
                    }
                    catch (RemotingException remotingException)
                    {
                        SendResult("Remoting Exception in InitHostSide(): " + remotingException.Message, TestOutcome.Warning);
                    }
                    // If test run was started under debugger, attach debugger.
                    CheckAttachDebugger();
                }
            }
            catch (Exception ex)
            {
                // Do some error diagnostics.
                TestRunTextResultMessage textMessage = new TestRunTextResultMessage(
                                            Environment.MachineName,
                                            m_runContext.RunConfig.TestRun.Id,
                                            ex.GetType() + ": " + ex.Message,
                                            TestMessageKind.TextMessage);
                m_runContext.ResultSink.AddResult(textMessage);
                m_runContext.ResultSink.AddResult(new RunStateEvent(m_runContext.RunConfig.TestRun.Id,
                                                                    RunState.Aborting,
                                                                    Environment.MachineName));
                throw;
            }

            m_isHostSideDirty = false;
        }

        /// <summary>
        /// Determine which registry hive to use:
        ///     If override value is set, use it, don't use anything else.
        ///     Else If using RunConfig, get it from RunConfig
        ///     Else get it from environment.
        /// </summary>
        /// <returns></returns>
        private string GetRegistryHive()
        {
            // We get registry hive each time we initialize host side, i.e. it can be changed in between tests.
            string overrideHiveValue = RegistrySettings.RegistryHiveOverride;
            if (!string.IsNullOrEmpty(overrideHiveValue))
            {
                return overrideHiveValue;
            }

            // Note that Run Config Data can be null, e.g. when executing using HostType attribute.
            TestRunConfiguration runConfig = m_runContext.RunConfig.TestRun.RunConfiguration;
            VsIdeHostRunConfigData runConfigHostData = runConfig.HostData[Name] as VsIdeHostRunConfigData;
            if (runConfigHostData != null)
            {
                return runConfigHostData.RegistryHive;
            }

            return null;    // VsIde will figure out and use default.
        }

        private string GetAdditionalCommandLine()
        {
            TestRunConfiguration runConfig = m_runContext.RunConfig.TestRun.RunConfiguration;
            VsIdeHostRunConfigData runConfigHostData = runConfig.HostData[Name] as VsIdeHostRunConfigData;
            if (runConfigHostData != null)
            {
                return runConfigHostData.AdditionalCommandLineArguments;
            }
            return null;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void CreateHostSide()
        {
            // Note: we already have try-catch-Debug.Fail in Initialize that calls this method.
            // Note: registryHive can be null when using HostType attribute. In this case we'll use default hive.
            Debug.Assert(!string.IsNullOrEmpty(m_workingDir));
            // Note: registryHive can be null when using the attribute. That's OK, VsIde will figure out.
            Debug.Assert(!string.IsNullOrEmpty(m_workingDir));
            Debug.Assert(m_hostSide == null, "HA.CreateHostSide: m_hostSide should be null (ignorable).");
            Debug.Assert(m_vsIde == null, "HA.CreateHostSide: m_vsIde should be null (ignorable).");

            // Start devenv.
            m_vsIde = new VisualStudioIde(new VsIdeStartupInfo(m_vsRegistryHive, m_additionalCommandLine, m_workingDir));
            m_vsIde.ErrorHandler += HostProcessErrorHandler;

            bool success = InvokeWithRetry(
                delegate
                {
                    m_vsIde.Dte.MainWindow.Visible = true;    // TestRunConfig for this host type could have an option to set main window to visible.
                },
                s_addinWaitTimeout
            );

            if (!success)
            {
                throw new VsIdeTestHostException(Resources.TimedOutCommunicatingToIde);
            }

            m_hostSide = GetHostSideFromAddin();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private HostAdapterHostSide GetHostSideFromAddin()
        {
            // Find the Addin.
            // Note: After VS starts addin needs some time to load, so we try a few times.
            IVsIdeTestHostAddin addinLookingFor = null;

            InvokeWithRetry(
                delegate
                {
                    foreach (AddIn addin in m_vsIde.Dte.AddIns)
                    {
                        // Note: the name of the addin comes from XML file that registers the addin.
                        if (addin.Name == "VsIdeTestHost(EF)")
                        {
                            addinLookingFor = (IVsIdeTestHostAddin)addin.Object;
                            break;
                        }
                    }
                },
                delegate
                {
                    return addinLookingFor != null;
                },
                s_addinWaitTimeout
            );

            if (addinLookingFor == null)
            {
                Debug.Fail("HA.GetHostSideFromAddin: timed out but could not get the addin from VS.");
                throw new VsIdeTestHostException(Resources.TimedOutGettingAddin);
            }

            m_testHostAddin = addinLookingFor;
            HostAdapterHostSide hostSide = m_testHostAddin.GetHostSide(); // This can be casted to ITestAdapter.
            Debug.Assert(hostSide != null);

            return hostSide;
        }

        private void CheckAttachDebugger()
        {
            Debug.Assert(m_runConfig != null);

            if (m_runConfig.IsExecutedUnderDebugger)
            {
                Debug.Assert(m_hostSession != null);
                Debug.Assert(m_vsIde != null);

                // Ask Test Runner IDE to attach debugger to Host IDE.
                m_hostSession.AttachDebugger(m_vsIde.Process.Id);
            }
        }

        /// <summary>
        /// If run under debugger, attaches to the host session of test runner IDE.
        /// </summary>
        private void InitHostSession()
        {
            Debug.Assert(m_hostSession == null, "HA.InitHostSession: m_hostSession should be null!");
            Debug.Assert(m_runConfig != null);

            if (m_runConfig.IsExecutedUnderDebugger)
            {
                // First check if there is Host Data in Run Config.
                // There is not host data when using HostType attribute (not using Run Config).
                string sessionId = null;
                VsIdeHostRunConfigData hostData = m_runConfig.HostData[Name] as VsIdeHostRunConfigData;
                if (hostData != null)
                {
                    sessionId = hostData.SessionId;
                }
                else
                {
                    try
                    {
                        // Try connecting by parent process id.
                        // This is not very reliable because Windows does not really track parent-child process relationship.
                        int ppid = ProcessUtil.GetParentProcessId(System.Diagnostics.Process.GetCurrentProcess().Id);
                        sessionId = VsIdeHostSession.Prefix + ppid.ToString(CultureInfo.InvariantCulture);
                    }
                    catch (COMException ex)
                    {
                        m_runContext.ResultSink.AddResult(new TestRunTextResultMessage(Environment.MachineName, m_runContext.RunConfig.TestRun.Id,
                            "Warning: " + ex.GetType() + ": " + ex.Message, TestMessageKind.TextMessage));
                    }
                }

                Debug.Assert(m_hostSession == null, "HA.InitHostSession: m_hostSession should be null!");

                if (!string.IsNullOrEmpty(sessionId))
                {
                    // Now get the IDE runner session, the Uri is ipc://ServerPortName/AppName.
                    string uri = string.Format(
                        CultureInfo.InvariantCulture, "ipc://{0}/{1}",
                        sessionId,
                        VsIdeHostSession.RemoteObjectName);

                    m_hostSession = (IVsIdeHostDebugger)Activator.GetObject(typeof(IVsIdeHostDebugger), uri);

                }
                else
                {
                    m_runContext.ResultSink.AddResult(new TestRunTextResultMessage(Environment.MachineName, m_runContext.RunConfig.TestRun.Id,
                        "Warning: failed to get session id for debugger VS IDE, debugging child VS will be disabled.", TestMessageKind.TextMessage));
                }
            }
        }

        /// <summary>
        /// This should not throw.
        /// We do the following steps. Each step is important and must be done whenever previous step throws or not.
        /// - Call HostSide.Cleanup.
        /// - Ask Runner IDE to detach debugger (if attached).
        /// - Dispose m_vsIde.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void CleanupHostSide()
        {
            if (!m_isHostSideDirty)
                return;

            lock (m_hostSideLock)
            {
                //Debug.Assert(HostSide != null);

                if (HostSide != null)
                {
                    try
                    {
                        ((ITestAdapter)HostSide).Cleanup();
                    }
                    catch (Exception ex)    // We don't know what this can throw in advance.
                    {
                        SendResult(Resources.FailedToCallTACleanup(ex), TestOutcome.Warning);
                    }
                }

                // m_runConfig can be null if cleanup is called too early, i.e. before we comleted init.
                if (m_runConfig != null && m_runConfig.IsExecutedUnderDebugger)
                {
                    // Detach debugger for VS we are going to shut down.
                    //Debug.Assert(m_hostSession != null);
                    if (m_hostSession != null)
                    {
                        try
                        {
                            // Detach debugger but keep HostSession alive for possible attach to restarted VS.
                            m_hostSession.DetachDebugger(m_vsIde.Process.Id);
                        }
                        catch (Exception ex)
                        {
                            SendResult(Resources.FailedToDetachDebugger(ex), TestOutcome.Warning);
                        }
                    }
                }

                if (m_vsIde != null)
                {
                    try
                    {
                        m_vsIde.Dispose();
                    }
                    catch (Exception ex)
                    {
                        SendResult(Resources.ErrorShuttingDownVS(ex), TestOutcome.Warning);
                    }
                }
                m_vsIde = null;
                m_hostSide = null;
                m_isHostSideDirty = false;
            }
        }

        private bool HostExists()
        {
            return m_vsIde != null && HostSide != null;
        }

        private bool ShouldRestartVs()
        {
            return !HostExists() || (RegistrySettings.RestartVsBetweenTests > 0 && m_isHostSideDirty);
        }

        private void CleanupHostSession()
        {
            // m_runConfig can be null if cleanup is called too early, i.e. before we comleted init.
            if (m_runConfig != null && m_runConfig.IsExecutedUnderDebugger)
            {
                Debug.Assert(m_hostSession != null);
                try
                {
                    m_hostSession.DetachDebugger();         // Detach from all attached.
                }
                finally
                {
                    m_hostSession = null;
                }
            }
        }

        private void CheckRestartVs()
        {
            lock (m_hostSideLock)
            {
                if (ShouldRestartVs())
                {
                    // This is optimization: if nobody used this instance of VS, do not restart it.
                    CleanupHostSide();
                    InitHostSide();
                }
            }
        }

        private void SetupChannels()
        {
            BinaryServerFormatterSinkProvider   serverProvider      = new BinaryServerFormatterSinkProvider();
            Hashtable                           properties          = new Hashtable();
            string                              channelPrefix       = "EqtVsIdeHostAdapter_" + Guid.NewGuid().ToString();
            string                              serverChannelName   = channelPrefix + "_ServerChannel";
            string                              clientChannelName   = channelPrefix + "_ClientChannel";

            // Server channel is required for callbacks from client side.
            // Actually it is not required when running from vstesthost as vstesthost already sets up the channels
            // but since we have /noisolation (~ no vstesthost) mode we need to create this channel. 
            // RunConfig.IsExecutedOutOfProc exists but it's internal so we cannot say if we are mstest-inproc or not.
            serverProvider.TypeFilterLevel = TypeFilterLevel.Full;  // Enable remoting objects as arguments.

            // portName:        Must be different from Clients Port.
            // authorizedGroup: Default IpcChannel security is allow for all users who can authorize this machine.
            properties["name"]              = serverChannelName;
            properties["portName"]          = serverChannelName;           
            properties["authorizedGroup"]   = WindowsIdentity.GetCurrent().Name;

            m_serverChannel = new IpcServerChannel(properties, serverProvider);
            ChannelServices.RegisterChannel(m_serverChannel, false);

            m_clientChannel = new IpcClientChannel(clientChannelName, new BinaryClientFormatterSinkProvider());
            ChannelServices.RegisterChannel(m_clientChannel, false);
        }

        private void CleanupChannels()
        {
            if (m_clientChannel != null)
            {
                ChannelServices.UnregisterChannel(m_clientChannel);
                m_clientChannel = null;
            }
            if (m_serverChannel != null)
            {
                ChannelServices.UnregisterChannel(m_serverChannel);
                m_serverChannel = null;
            }
        }

        private void HostProcessErrorHandler(string errorMessage, TestOutcome outcome, bool abortTestRun)
        {
            Debug.Assert(m_runContext != null);
            SendResult(errorMessage, outcome, abortTestRun);
        }

        private void SendResult(string messageText, TestOutcome outcome)
        {
            SendResult(messageText, outcome, false);
        }

        /// <summary>
        /// Sends run level message to the result sink.
        /// </summary>
        /// <param name="message">Text for the message.</param>
        /// <param name="outcome">Outcome for the message. Affects test run outcome.</param>
        /// <param name="abortTestRun">If true, we use TMK.Panic, otherwise TMK.TextMessage</param>
        private void SendResult(string messageText, TestOutcome outcome, bool abortTestRun)
        {
            TakeScreenShot("ScreenShot_TestRunAborted");

            Debug.Assert(!abortTestRun || outcome == TestOutcome.Error,
                "HA.SendResult: When abortTestRun = true, Outcome should be Error.");

            String events = GetEvents(m_runContext.RunConfig.TestRun.Created, DateTime.Now, true);
            String abortMessage = String.Empty;

            if (events != String.Empty)
            {
                abortMessage = String.Format("{0}{1}{2}", messageText, Environment.NewLine, events);
            }
            else
            {
                abortMessage = messageText;
            }

            TestRunTextResultMessage message = new TestRunTextResultMessage(
                Environment.MachineName,
                m_runContext.RunConfig.TestRun.Id,
                abortMessage,
                TestMessageKind.TextMessage);
            message.Outcome = outcome;

            m_runContext.ResultSink.AddResult(message);

            if (abortTestRun)
            {
                m_runContext.ResultSink.AddResult(new RunStateEvent(m_runContext.RunConfig.TestRun.Id,
                                                                RunState.Aborting,
                                                                Environment.MachineName));
            }
        }

        private void AbortSingleTestResult(String message, ITestElement testElement, DateTime startTime)
        {
            TakeScreenShot("ScreenShot_SingleTestAborted");
            String events = GetEvents(startTime, DateTime.Now);
            String abortMessage = String.Format("{0}{1}{2}", message, Environment.NewLine, events);

            TextTestResultMessage result 
                = new TextTestResultMessage(
                    m_runContext.RunConfig.TestRun.Id, 
                    testElement, 
                    abortMessage);

            TestResultAggregation testResult 
                = new TestResultAggregation(
                    Environment.MachineName, 
                    m_runContext.RunConfig.TestRun.Id, 
                    testElement);

            testResult.Outcome = TestOutcome.Aborted;

            m_runContext.ResultSink.AddResult(result);
            m_runContext.ResultSink.AddResult(testResult);
        }

        private String GetEvents(DateTime startTime, DateTime endTime, Boolean lastEventOnly = false)
        {
            StringBuilder events = new StringBuilder();

            try
            {
                String query = "<QueryList>"
                             + "<Query Id=\"0\" Path=\"Application\">"
                             + "<Select Path=\"Application\">*[System[Provider[@Name='.NET Runtime'] and (EventID=1026) "
                             +" and TimeCreated[@SystemTime&gt;='" 
                             + XmlConvert.ToString(startTime, XmlDateTimeSerializationMode.Utc)
                             + "' and @SystemTime&lt;='" 
                             + XmlConvert.ToString(endTime, XmlDateTimeSerializationMode.Utc) 
                             + "']"
                             +"]]</Select>"
                             + "</Query>"
                             + "</QueryList>";

                EventLogQuery eventLogQuery = new EventLogQuery("Application", PathType.LogName, query);

                if (lastEventOnly)
                {
                    eventLogQuery.ReverseDirection = true;
                }
                
                using (EventLogReader logReader = new EventLogReader(eventLogQuery))
                {
                    if (lastEventOnly)
                    {
                        EventRecord eventLogEntry = logReader.ReadEvent();
                        WriteCrashReport(events, eventLogEntry);
                    }
                    else
                    {

                        for (EventRecord    eventLogEntry = logReader.ReadEvent();
                                            eventLogEntry != null;
                                            eventLogEntry = logReader.ReadEvent())
                        {
                            WriteCrashReport(events, eventLogEntry);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                events.Append("Error Reading from EventLog. Message:" + ex.Message + " " + ex.StackTrace);
            }

            return events.ToString();
        }

        /// <summary>
        /// Takes an event record, and writes the properties into a string builder.
        /// This assumes that the event is a .Net Runtime Provider EventID 1026, which only has one property
        /// that is a Crash Report for Visual Studio.
        /// </summary>
        /// <param name="events">StringBuilder to write the properties to.</param>
        /// <param name="eventLogEntry">EventRecord to extract properties from</param>
        private void WriteCrashReport(StringBuilder events, EventRecord eventLogEntry)
        {
            if (eventLogEntry == null)
                return;

            // We assume that this .Net Runtime Provider EventID 1026, which only has one property
            EventProperty eventProperty = eventLogEntry.Properties.FirstOrDefault();
            if (eventProperty != null)
            {
                int stringBuildInitialLength = events.Length;

                if (eventLogEntry.TimeCreated.HasValue)
                {
                    events.Append("Event Log Time: ");
                    events.AppendLine(eventLogEntry.TimeCreated.Value.ToLocalTime().ToString());
                }
                
                String crashReport = eventProperty.Value.ToString();
                
                crashReport = crashReport.Replace(" at ",               Environment.NewLine + " at ");
                crashReport = crashReport.Replace("Framework Version:", Environment.NewLine + "Framework Version:");
                crashReport = crashReport.Replace("Description:",       Environment.NewLine + "Description:");
                crashReport = crashReport.Replace("Exception Info:",    Environment.NewLine + "Exception Info:" + Environment.NewLine);
                crashReport = crashReport.Replace("Stack:",             Environment.NewLine + "Stack:");

                events.AppendLine(crashReport);
            }
        }

        private delegate bool BoolMethodInvoker();

        /// <summary>
        /// See the other overload for details.
        /// </summary>
        private bool InvokeWithRetry(MethodInvoker invoker, TimeSpan timeout)
        {
            return InvokeWithRetry(invoker, null, timeout);
        }

        /// <summary>
        /// Invokes specified invoker inside a loop with try-catch/everything and retries until reaches timeout or until each of the following is true:
        /// - The invoker does not throw exception.
        /// - If the additional condition delegate is specified, it must return true.
        /// </summary>
        /// <param name="invoker">The method to invoke</param>
        /// <param name="stopCondition">If specified, to break from the loop, it must be true. The condition delegate must not throw.</param>
        /// <param name="timeout">Timeout to stop retries after.</param>
        /// <returns>True on success, false on timeout.</returns>
        private bool InvokeWithRetry(MethodInvoker invoker, BoolMethodInvoker stopCondition, TimeSpan timeout)
        {
            Debug.Assert(invoker != null);

            Stopwatch timer = Stopwatch.StartNew();
            bool hasTimedOut = true;
            try
            {
                do
                {
                    bool isExceptionThrown = false;
                    try
                    {
                        invoker.Invoke();
                    }
                    catch
                    {
                        isExceptionThrown = true;
                    }

                    // If there was no exception, also check for stop condition.
                    // Note: condition.Invoke() must not throw. If it throws, we also throw from here.
                    if (!isExceptionThrown &&
                        (stopCondition == null || stopCondition.Invoke()))
                    {
                        hasTimedOut = false;
                        break;
                    }

                    Thread.Sleep(RegistrySettings.BaseSleepDuration);   // This is to prevent 100% CPU consumption.
                } while (timer.Elapsed < timeout);
            }
            finally
            {
                timer.Stop();
            }

            return !hasTimedOut;
        }

        private void TraceMessage(string message)
        {
            Debug.Assert(message != null);

            Trace.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "[pid={0,4}, tid={1,2}, {2:yyyy}/{2:MM}/{2:dd} {2:HH}:{2:mm}:{2:ss}.{2:fff}] {3}",
                System.Diagnostics.Process.GetCurrentProcess().Id,
                System.Threading.Thread.CurrentThread.ManagedThreadId,
                DateTime.Now, message));
        }

        private void PrePostHostCodeExecution(ITestElement testElement, ITestContext testContext, ExecutionType executionType)
        {
            Assembly            testAssembly        = null;
            Type                testType            = null;
            List<MethodInfo>    preExecuteMethods   = null;
            String              currentMethod       = String.Empty;
            String              testClassName       = String.Empty;
            object              testObject          = null;
            object[]            parameters          = new object[] { };

            try
            {
                testAssembly        = Assembly.LoadFrom(testElement.Storage);
                testClassName       = GetTestClassName(testElement);
                testType            = testAssembly.GetType(testClassName);
                preExecuteMethods   = GetExecutionMethods(testType, executionType);

                if (preExecuteMethods.Count > 0)
                {
                    if (HostExists()) CleanupHostSide();

                    testObject = Activator.CreateInstance(testType);

                    foreach (var preExecuteMethod in preExecuteMethods)
                    {
                        currentMethod = preExecuteMethod.Name;
                        preExecuteMethod.Invoke(testObject, parameters);
                    }
                }
            }
            catch (Exception e)
            {
                String method = String.Empty;

                if (currentMethod != String.Empty)
                {
                    method = " in Method: " + currentMethod;
                }
                else
                {
                    method = "before method execution";
                }

                HostProcessErrorHandler(
                    String.Format("Pre-Host Code Execution Failed {0}. Error: {1}.", method, e.ToString())
                    , TestOutcome.Warning
                    , false);
            }

        }

        private List<MethodInfo> GetExecutionMethods(Type testType, ExecutionType executionType)
        {
            List<MethodInfo> methods = new List<MethodInfo>();

            Type executionAttribute = null;

            switch (executionType)
            {
                case ExecutionType.PreHost:
                    executionAttribute = typeof(VsIdePreHostExecutionMethod);
                    break;
                case ExecutionType.PostHost:
                    executionAttribute = typeof(VsIdePostHostExecutionMethod);
                    break;
                default:
                    throw new ArgumentException("Unknow Execution Type.");
            }

            foreach (var m in testType.GetMethods())
            {
                bool hasCustomAttribute = m.GetCustomAttributes(executionAttribute, true).Length >= 1;
                
                if (hasCustomAttribute
                    && !m.IsAbstract
                    && m.IsStatic
                    && m.GetParameters().Length == 0)
                {
                    methods.Add(m);
                }
                else if (hasCustomAttribute)
                {
                    HostProcessErrorHandler(
                        String.Format("Method {0} is attributed with {1} but cannot be run Pre/Post Host. "
                                    + "Methods must be Static and have no parameters."
                                    , m.ToString()
                                    , executionType.ToString())
                        , TestOutcome.Warning
                        , false);
                }
            }

            return methods;
        }

        private void TakeScreenShot(string screenshotName)
        {
            try
            {
                string filename = Environment.CurrentDirectory.ToString() + "\\" + screenshotName + "_" + DateTime.UtcNow.Ticks + ".jpeg";
                Trace.WriteLine("ScreenShot captured: {0}", filename);
                Size screenSize = Screen.PrimaryScreen.Bounds.Size;
                Bitmap btm = new Bitmap(screenSize.Width, screenSize.Height);
                using (Graphics g = Graphics.FromImage(btm))
                {
                    g.CopyFromScreen(new Point(0, 0), new Point(0, 0), screenSize);
                }
                btm.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                btm.Dispose();
            }
            catch (Exception ex)
            {
                //Eat all exceptions as we do not want failure of this method to affect the test run
                Trace.WriteLine(ex.Message);
            }
        }

        private String GetTestClassName(ITestElement element)
        {
            List<String> names = new List<string>(element.HumanReadableId.Split(new char[] { '.' }));
            String className = String.Empty;

            if (names.Count > 0 && names[names.Count - 1].Equals(element.Name))
                names.RemoveAt(names.Count - 1);

            foreach (String name in names)
            {
                if (className != String.Empty && !className.EndsWith("."))
                {
                    className += ".";
                }

                className += name;
            }

            return className;
        }

        private enum ExecutionType
        {
            PreHost,
            PostHost
        }

        #endregion
    }
}
