// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Design
{
    // <summary>
    // Wraps a handler. If the handler does not implement a contract, calling its
    // operations will result in a no-op.
    // </summary>
    internal class WrappedReportHandler : IReportHandler
    {
        private readonly IReportHandler _handler;

        public WrappedReportHandler(object handler)
        {
#if NET45 || NET40
            if (handler != null)
            {
                var handlerBase = handler as HandlerBase
                    ?? new ForwardingProxy<HandlerBase>(handler).GetTransparentProxy();

                _handler = handler as IReportHandler
                    ?? (handlerBase.ImplementsContract(typeof(IReportHandler).FullName)
                        ? new ForwardingProxy<IReportHandler>(handler).GetTransparentProxy()
                        : null);
            }
#else
            _handler = handler as IReportHandler;
#endif
        }

        public void OnError(string message)
            => _handler?.OnError(message);

        public void OnInformation(string message)
            => _handler?.OnInformation(message);

        public void OnVerbose(string message)
            => _handler?.OnVerbose(message);

        public void OnWarning(string message)
            => _handler?.OnWarning(message);
    }
}
