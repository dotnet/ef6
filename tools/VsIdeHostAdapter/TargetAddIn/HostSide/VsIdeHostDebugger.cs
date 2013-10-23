// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using EnvDTE;

namespace Microsoft.VisualStudio.TestTools.HostAdapters.VsIde
{
    internal interface IVsIdeHostDebugger
    {
        void AttachDebugger(int processId);
        void DetachDebugger(int processId);
        void DetachDebugger();  // Detach from all attached by Attach. 
    }

    internal class VsIdeHostDebugger : MarshalByRefObject, IVsIdeHostDebugger
    {
        private DTE m_dte;
        /// <summary>
        /// Maps PID onto EnvDTE.Process.
        /// </summary>
        private Dictionary<int, EnvDTE.Process> m_attachedProcesses = new Dictionary<int, EnvDTE.Process>();

        public VsIdeHostDebugger(DTE dte)
        {
            Debug.Assert(dte != null);
            m_dte = dte;
        }

        public void AttachDebugger(int processId)
        {
            EnvDTE.Process process = GetDteProcess(processId);

            try
            {
                process.Attach();
            }
            catch (Exception e)
            {
                Debug.Fail("Exception: " + e.ToString());
                throw;
            }

            if (!m_attachedProcesses.ContainsKey(processId))
            {
                m_attachedProcesses.Add(processId, process);
            }
            else
            {
                Debug.Fail("AttachDebugger: was already attached to process with pid=" + processId.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Detach from previously attached process.
        /// </summary>
        public void DetachDebugger(int processId)
        {
            EnvDTE.Process process = m_attachedProcesses[processId];
            try
            {
                process.Detach(false); // waitForBreakOnEnd = false.
            }
            finally
            {
                // If there's some problem, do not try removing next time.
                m_attachedProcesses.Remove(processId);
            }
        }

        /// <summary>
        /// Detach from all attached processes. Ignore exceptions.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void DetachDebugger()
        {
            foreach (KeyValuePair<int, EnvDTE.Process> pair in m_attachedProcesses)
            {
                try
                {
                    pair.Value.Detach(false); // waitForBreakOnEnd = false.
                }
                catch (Exception ex)
                {
                    Debug.Fail("HostSide.VsDebugger.DetachDebugger: " + ex.ToString());
                    // Ignore the exception.
                }
            }

            m_attachedProcesses.Clear();
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        private EnvDTE.Process GetDteProcess(int processId)
        {
            foreach (EnvDTE.Process process in m_dte.Debugger.LocalProcesses)
            {
                if (process.ProcessID == processId)
                {
                    return process;
                }
            }

            Debug.Fail(string.Format(CultureInfo.InvariantCulture, 
                "Attach failed since process with id {0} was not found.", processId));
            throw new ArgumentException(Resources.CannotFindProcess, "processId");
        }
    }
}
