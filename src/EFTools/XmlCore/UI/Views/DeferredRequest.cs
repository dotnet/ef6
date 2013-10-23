// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views
{
    using System;
    using System.Diagnostics;
    using System.Windows.Threading;

    internal sealed class DeferredRequest : IDisposable
    {
        internal delegate void Callback(object arg);

        private DispatcherOperation _operation;
        private Callback _callback;

        internal DeferredRequest(Callback callback)
        {
            _callback = callback;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Cancel();
            _callback = null;
        }

        #endregion

        internal bool IsPending
        {
            get { return _operation != null; }
        }

        internal void Cancel()
        {
            if (_operation != null)
            {
                Debug.Assert(_operation.Status == DispatcherOperationStatus.Pending);
                _operation.Abort();
                _operation = null;
            }
        }

        internal void Request()
        {
            Request(null);
        }

        internal void Request(object arg)
        {
            if (_operation == null)
            {
                Debug.Assert(_callback != null);

                _operation =
                    Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Loaded,
                        new DispatcherOperationCallback(DispatcherOperation), arg);
            }
        }

        internal void ExecuteNow()
        {
            ExecuteNow(null);
        }

        internal void ExecuteNow(object arg)
        {
            if (_operation != null)
            {
                Cancel();
            }
            _callback(arg);
        }

        private object DispatcherOperation(object arg)
        {
            try
            {
                Debug.Assert(_operation != null && _operation.Status == DispatcherOperationStatus.Executing);
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
