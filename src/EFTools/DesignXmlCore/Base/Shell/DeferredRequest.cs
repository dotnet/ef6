// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Shell
{
    using System;
    using System.Diagnostics;
    using System.Windows.Threading;

    /// <summary>
    ///     This class gives a UI component the ability to defer some processing.  This can be helpful
    ///     for some UI situations where, for example, you want an event handler to finish processing before
    ///     you kick off some dependent processing.
    ///     This class uses a DispatcherOperation and Dispatcher.  The Dispatcher maintains a prioritized
    ///     queue of work items for a specific thread, so this deferred request will run on the same thread
    ///     as the caller.
    /// </summary>
    internal sealed class DeferredRequest : IDisposable
    {
        internal delegate void Callback(object arg);

        private DispatcherOperation _operation;
        private Callback _callback;

        internal DeferredRequest(Callback callback)
        {
            Debug.Assert(callback != null);
            _callback = callback;
        }

        public void Dispose()
        {
            Cancel();
            _callback = null;
        }

        /// <summary>
        ///     Returns true is there is a pending DispatcherOperation
        /// </summary>
        internal bool IsPending
        {
            get
            {
                return (_operation != null &&
                        _operation.Status == DispatcherOperationStatus.Pending);
            }
        }

        /// <summary>
        ///     Cancels a pending DispatcherOperation
        /// </summary>
        internal void Cancel()
        {
            if (IsPending)
            {
                _operation.Abort();
            }
            _operation = null;
        }

        /// <summary>
        ///     Requests that a new DispatcherOperation be placed on the Dispatcher queue.  The
        ///     callback will receive a null in its arg parameter.
        /// </summary>
        internal void Request()
        {
            Request(null);
        }

        /// <summary>
        ///     Requests that a new DispatcherOperation be placed on the Dispatcher queue
        /// </summary>
        /// <param name="arg">The object to send the callback in its arg parameter.</param>
        internal void Request(object arg)
        {
            if (_operation != null)
            {
                Cancel();
            }

            _operation = Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new DispatcherOperationCallback(DispatcherOperation),
                arg);
        }

        /// <summary>
        ///     Calls the callback method immediately, sending null to its arg parameter.
        /// </summary>
        internal void ExecuteNow()
        {
            ExecuteNow(null);
        }

        /// <summary>
        ///     Calls the callback method immediately.
        /// </summary>
        /// <param name="arg">The object to send the callback in its arg parameter.</param>
        internal void ExecuteNow(object arg)
        {
            if (_operation != null)
            {
                Cancel();
            }
            _callback(arg);
        }

        /// <summary>
        ///     Dispatcher method that matches the System.Windows.Threading.DispatcherOperationCallback
        ///     delegate.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private object DispatcherOperation(object arg)
        {
            Debug.Assert(
                _operation != null &&
                _operation.Status == DispatcherOperationStatus.Executing, "This should only be called by an executing DispatcherOperation");

            try
            {
                _callback(arg);
            }
            finally
            {
                _operation = null;
            }

            return null;
        }
    }
}
