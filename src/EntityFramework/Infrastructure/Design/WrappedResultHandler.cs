// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Design
{
    // <summary>
    // Wraps a handler. If the handler does not implement a contract, calling its
    // operations will result in a no-op.
    // </summary>
    internal class WrappedResultHandler : IResultHandler
    {
        private readonly IResultHandler _handler;
        private readonly IResultHandler2 _handler2;

        public WrappedResultHandler(object handler)
        {
#if NET45 || NET40
            var handlerBase = handler as HandlerBase
                ?? new ForwardingProxy<HandlerBase>(handler).GetTransparentProxy();

            _handler = handler as IResultHandler
                ?? (handlerBase.ImplementsContract(typeof(IResultHandler).FullName)
                    ? new ForwardingProxy<IResultHandler>(handler).GetTransparentProxy()
                    : null);

            _handler2 = handler as IResultHandler2
                ?? (handlerBase.ImplementsContract(typeof(IResultHandler2).FullName)
                    ? new ForwardingProxy<IResultHandler2>(handler).GetTransparentProxy()
                    : null);
#else
            _handler = handler as IResultHandler;
            _handler2 = handler as IResultHandler2;
#endif
        }

        public void SetResult(object value)
        {
            if (_handler != null)
            {
                _handler.SetResult(value);
            }
        }

        public bool SetError(string type, string message, string stackTrace)
        {
            if (_handler2 == null)
            {
                return false;
            }

            _handler2.SetError(type, message, stackTrace);

            return true;
        }
    }
}
