// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.TestAdapter;
using Microsoft.VisualStudio.TestTools.Common;
using Microsoft.VisualStudio.TestTools.Execution;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Security.Permissions;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.VsIdeTesting;

namespace Microsoft.VisualStudio.TestTools.HostAdapters.VsIde
{
    /// <summary>
    /// This is MarshalByRefObject wrapper for TA, since not all TAs could be MBR.
    /// </summary>
    /// <remarks>
    /// This class is public. We use explicit interface implementations to expose less data to public.
    /// </remarks>
    public sealed class HostAdapterHostSide : MarshalByRefObject, ITestAdapter, ITimeoutTestAdapter
    {
        #region Constants
        /// <summary>
        /// Amount of time to wait for the Run method thread to abort
        /// </summary>
        private static readonly int ThreadAbortTimeout = RegistrySettings.BaseTimeout;
        #endregion

        private Dictionary<string, ITestAdapter> m_adapters = new Dictionary<string, ITestAdapter>();
        private IRunContext m_runContext;
        private IServiceProvider m_serviceProvider;
        private Thread m_runThread;
        private object m_runThreadLock = new object();

        /// <summary>
        /// Disabled costructor. Nobody is supposed to call it.
        /// </summary>
        private HostAdapterHostSide()
        {
            Debug.Fail("HostAdapterHostSide.HostAdapterHostSide(): nobody should call this.");
        }

        /// <summary>
        /// Constructor. Called by Addin.
        /// </summary>
        /// <param name="serviceProvider">VS Service provider.</param>
        internal HostAdapterHostSide(IServiceProvider serviceProvider)
        {
            Debug.Assert(serviceProvider != null, "HostAdapterHostSide.HostAdapterHostSide: serverProvider is null!");
            m_serviceProvider = serviceProvider;

            // Initialize the Framework.
            VsIdeTestHostContext.ServiceProvider = m_serviceProvider;
            UIThreadInvoker.Initialize();
        }

        /// <summary>
        /// Get real TA.
        /// </summary>
        private ITestAdapter GetTestAdapter(ITestElement test)
        {
            Debug.Assert(test != null, "Internal error: test is null!");
            Debug.Assert(!string.IsNullOrEmpty(test.Adapter), "Internal error: test.Adapter is null or empty!");

            ITestAdapter realTestAdapter = null;
            bool containsAdapter = m_adapters.TryGetValue(test.Adapter, out realTestAdapter);
            if (!containsAdapter)
            {
                realTestAdapter = (ITestAdapter)Activator.CreateInstance(Type.GetType(test.Adapter), new Object[] { });

                // Initialize was delayed to be run from the Run method.
                realTestAdapter.Initialize(m_runContext);

                m_adapters.Add(test.Adapter, realTestAdapter);
            }

            return realTestAdapter;
        }

        #region IBaseAdapter, ITestAdapter
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
        void ITestAdapter.Initialize(IRunContext runContext)
        {
            // We delay inner TAs initialization until Run method because we don't know which test type this is going to be.
            m_runContext = runContext;

            TestRunConfiguration runConfig = m_runContext.RunConfig.TestRun.RunConfiguration;
            Debug.Assert(runConfig != null);
            VsIdeHostRunConfigData runConfigHostData = runConfig.HostData[VsIdeHostAdapter.Name] as VsIdeHostRunConfigData;
            if (runConfigHostData != null)
            {
                VsIdeTestHostContext.AdditionalTestData = runConfigHostData.AdditionalTestData;
            }
        }

        void IBaseAdapter.Run(ITestElement testElement, ITestContext testContext)
        {
            Thread myThread = Thread.CurrentThread;
            try
            {
                // Make sure to abort the previous test's Run thread if it isn't done yet - this could happen if a test times out
                Thread threadToAbort;
                lock (m_runThreadLock)
                {
                    threadToAbort = m_runThread;
                    m_runThread = myThread;
                }
                AbortThread(threadToAbort);

                //Register configuration proxy to merge test configuration with VS
                ConfigurationProxy.Register(testElement.Storage);

                ITestAdapter realAdapter = GetTestAdapter(testElement);
                realAdapter.Run(testElement, testContext);
            }
            catch (ThreadAbortException)
            {
                // The agent-side adapter's thread that called this method may have already been aborted due to a timeout, so
                // don't send this exception back
                Thread.ResetAbort();
            }
            finally
            {
                lock (m_runThreadLock)
                {
                    if (m_runThread != null && m_runThread.ManagedThreadId == myThread.ManagedThreadId)
                    {
                        m_runThread = null;
                    }
                }
            }
        }

        private static void AbortThread(Thread thread)
        {
            if (thread == null)
            {
                return;
            }

            try
            {
                SafeThreadAbort(thread, ThreadAbortTimeout);
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
        }

        void ITimeoutTestAdapter.TestTimeout(ITestElement test)
        {
            ITimeoutTestAdapter realTimeoutAdapter = GetTestAdapter(test) as ITimeoutTestAdapter;
            if (realTimeoutAdapter != null)
            {
                realTimeoutAdapter.TestTimeout(test);
            }
        }

        void IBaseAdapter.Cleanup()
        {
            foreach (ITestAdapter testAdapter in m_adapters.Values)
            {
                testAdapter.Cleanup();
            }
            m_adapters.Clear();
        }

        void IBaseAdapter.StopTestRun()
        {
            foreach (ITestAdapter testAdapter in m_adapters.Values)
            {
                testAdapter.StopTestRun();
            }
        }

        void IBaseAdapter.AbortTestRun()
        {
            foreach (ITestAdapter testAdapter in m_adapters.Values)
            {
                testAdapter.AbortTestRun();
            }
        }

        void IBaseAdapter.PauseTestRun()
        {
            foreach (ITestAdapter testAdapter in m_adapters.Values)
            {
                testAdapter.PauseTestRun();
            }
        }

        void IBaseAdapter.ResumeTestRun()
        {
            foreach (ITestAdapter testAdapter in m_adapters.Values)
            {
                testAdapter.ResumeTestRun();
            }
        }

        void ITestAdapter.ReceiveMessage(object obj)
        {
            foreach (ITestAdapter testAdapter in m_adapters.Values)
            {
                testAdapter.ReceiveMessage(obj);
            }
        }

        void ITestAdapter.PreTestRunFinished(IRunContext runContext)
        {
            // Abort the thread of the last test if it timed out, so that the test will be cleaned up before the test run is cleaned up
            Thread runThread;
            lock (m_runThreadLock)
            {
                runThread = m_runThread;
            }
            AbortThread(runThread);

            foreach (ITestAdapter testAdapter in m_adapters.Values)
            {
                testAdapter.PreTestRunFinished(runContext);
            }
        }
        #endregion

        #region Other methods
        // NOTE: this method is copied from Microsoft.VisualStudio.TestTools.Common.ThreadHelper. This assembly can't be a
        // friend of Common since this assembly is not signed.
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static bool SafeThreadAbort(Thread thread, int timeout)
        {
            Trace.WriteLine("ThreadHelper.SafeThreadAbort: starting...");

            // When there's a Sleep in the finally block of a thread, the call
            // to Thread.Abort will stop processing until the sleep finishes. Thread.Abort
            // does not take a timeout argument, so we create another thread that
            // will kill the target thread, and call Join on it instead.

            // Note that this is expensive (we create a new thread), so use it sparingly.

            try
            {
                // We cannot use QueueUserWorkItem because we need to call Thread.Join.
                Thread helperThread = new Thread(delegate()
                {
                    thread.Abort();
                    thread.Join(timeout);
                });

                helperThread.Name = "SafeThreadAbort thread killer";
                helperThread.IsBackground = true;
                helperThread.Start();

                // Give the thread a chance to abort
                bool isHelperThreadFinished = helperThread.Join(timeout);
                if (!isHelperThreadFinished)
                {
                    // Well, we tried. Unfortunately this leaves the target thread and killer thread running.
                    Trace.TraceError("ThreadHelper: Timed out aborting the target thread!");
                    return false;
                }
                else
                {
                    Trace.WriteLine("ThreadHelper.SafeThreadAbort: target thread was killed successfully.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("ThreadHelper.SafeThreadAbort: Exception while killing target thread: " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Attempt to run an arbitrary method on the host side on the current thread.
        /// Note: Ideal for ad hoc code execution outside the context of an MSTest TestMethod
        /// </summary>
        public object RunNonTestMethod(String typeName, String methodName, ArrayList args, String assemblyHintPath = null)
        {
            #region Arg Checks
            Debug.Assert(typeName != null, "typeName is null");
            Debug.Assert(methodName != null, "methodName is null");
            #endregion

            // Load the assembly
            Assembly assembly = Assembly.GetExecutingAssembly();
            if (assemblyHintPath != null)
                assembly = Assembly.LoadFrom(assemblyHintPath);

            // Find the type
            Type targetType = assembly.GetType(typeName);
            if (targetType == null)
            {
                throw new InvalidOperationException(
                    String.Format("Unable to locate type {0} in assembly {1}", typeName, assembly.FullName));
            }
                
            // Find the method
            MethodInfo targetMethod = targetType.GetMethod(methodName);
            if (targetMethod == null)
            {
                throw new InvalidOperationException(
                    String.Format("Unable to locate method {0} in type {1}", methodName, typeName));
            }

            // Invoke, instantiating the type if non-static
            object target = null;
            if (!targetMethod.IsStatic)
                target = Activator.CreateInstance(targetType);

            return targetMethod.Invoke(target, args.ToArray());
        }

        #endregion

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
