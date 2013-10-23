// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using SERVERCALL = Microsoft.VisualStudio.OLE.Interop.SERVERCALL;
using PENDINGMSG = Microsoft.VisualStudio.OLE.Interop.PENDINGMSG;

namespace Microsoft.VisualStudio.TestTools.HostAdapters.VsIde
{
    /// <summary>
    /// Redefine the interface as Primary Interop Assembly methods return uint (bug?) and we need to return -1.
    /// Refer to http://msdn.microsoft.com/library/en-us/com/html/e12d48c0-5033-47a8-bdcd-e94c49857248.asp
    /// </summary>
    [ComImport()]
    [Guid("00000016-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMessageFilter
    {
        [PreserveSig]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "InComing")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dw", Justification = "COM interface, parameters are named using Hungarian notation for consistency")]
        int HandleInComingCall(int dwCallType, IntPtr handleTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo);

        [PreserveSig]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dw", Justification = "COM interface, parameters are named using Hungarian notation for consistency")]
        int RetryRejectedCall(IntPtr handleTaskCallee, int dwTickCount, int dwRejectType);

        [PreserveSig]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dw", Justification = "COM interface, parameters are named using Hungarian notation for consistency")]
        int MessagePending(IntPtr handleTaskCallee, int dwTickCount, int dwPendingType);
    }

    /// <summary>
    /// COM message filter class to prevent RPC_E_CALL_REJECTED error while DTE is busy.
    /// The filter is used by COM to handle incoming/outgoing messages while waiting for response from a synchonous call.
    /// </summary>
    [ComVisible(true)]
    internal class RetryMessageFilter : IMessageFilter, IDisposable
    {
        private const int RetryCall = 99;
        private const int CancelCall = -1;

        private IMessageFilter m_oldFilter;

        public RetryMessageFilter()
        {
            // Register the filter.
            int result = NativeMethods.CoRegisterMessageFilter(this, out m_oldFilter);
            if (result != NativeMethods.S_OK)
            {
                throw new VsIdeTestHostException(Resources.FailedToRegisterMessageFilter);
            }
        }

        #region IDisposable implementation
        ~RetryMessageFilter()
        {
            Dispose();
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.TestTools.HostAdapters.VsIde.NativeMethods.CoRegisterMessageFilter(Microsoft.VisualStudio.TestTools.HostAdapters.VsIde.IMessageFilter,Microsoft.VisualStudio.TestTools.HostAdapters.VsIde.IMessageFilter@)", Justification = "It's a relatively minor issue if we fail to unregister the message filter, and we don't want to throw in Dispose")]
        public void Dispose()
        {
            // Unregister the filter.
            IMessageFilter ourFilter;
            NativeMethods.CoRegisterMessageFilter(m_oldFilter, out ourFilter);

            GC.SuppressFinalize(this);
        }
        #endregion

        #region IMessageFilter members
        /// <summary>
        /// Provides an abolity to filter or reject incoming calls (or callbacks) to an object or a process. 
        /// Called by COM prior to each method invocation originating outside the current process. 
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "InComing")]
        public int HandleInComingCall(int dwCallType, IntPtr handleTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo)
        {
            // Let current process try process the call.
            return (int)SERVERCALL.SERVERCALL_ISHANDLED;
        }

        /// <summary>
        /// An ability to choose to retry or cancel the outgoing call or switch to the task specified by threadIdCallee.
        /// Called by COM immediately after receiving SERVERCALL_RETRYLATER or SERVERCALL_REJECTED
        /// from the IMessageFilter::HandleIncomingCall method on the callee's IMessageFilter interface.
        /// </summary>
        /// -1: The call should be canceled. COM then returns RPC_E_CALL_REJECTED from the original method call. 
        /// 0..99: The call is to be retried immediately. 
        /// 100 and above: COM will wait for this many milliseconds and then retry the call.
        /// </returns>
        public int RetryRejectedCall(IntPtr threadIdCallee, int dwTickCount, int dwRejectType)
        {
            if (dwRejectType == (int)SERVERCALL.SERVERCALL_RETRYLATER)
            {
                // The server called by this process is busy. Ask COM to retry the outgoing call.
                return RetryCall;
            }
            else
            {
                // Ask COM to cancel the call and return RPC_E_CALL_REJECTED from the original method call. 
                return CancelCall;
            }
        }

        /// <summary>
        /// Called by COM when a Windows message appears in a COM application's message queue 
        /// while the application is waiting for a reply to an outgoing remote call. 
        /// </summary>
        /// <returns>
        /// Tell COM whether: to process the message without interrupting the call, 
        /// to continue waiting, or to cancel the operation. 
        /// </returns>
        public int MessagePending(IntPtr handleTaskCallee, int dwTickCount, int dwPendingType)
        {
            // Continue waiting for the reply, and do not dispatch the message unless it is a task-switching or window-activation message. 
            // A subsequent message will trigger another call to IMessageFilter::MessagePending. 
            return (int)PENDINGMSG.PENDINGMSG_WAITDEFPROCESS;
        }
        #endregion
    }
}
