// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Design
{
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Wraps a handler. If the handler does not implement a contract, calling its
    /// operations will result in a no-op.
    /// </summary>
    internal class WrappedHandler : IResultHandler
    {
        private readonly IResultHandler _resultHandler;

        public WrappedHandler(object handler)
        {
            DebugCheck.NotNull(handler);

            var handlerBase = handler as HandlerBase
                ?? new ForwardingProxy<HandlerBase>(handler).GetTransparentProxy();

            _resultHandler = handler as IResultHandler
                ?? (handlerBase.ImplementsContract(typeof(IResultHandler).FullName)
                    ? new ForwardingProxy<IResultHandler>(handler).GetTransparentProxy()
                    : null);
        }

        public void SetResult(object value)
        {
            if (_resultHandler != null)
            {
                _resultHandler.SetResult(value);
            }
        }
    }
}
