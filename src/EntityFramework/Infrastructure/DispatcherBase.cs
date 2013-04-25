// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    /// <summary>
    ///     Base class for dispatchers.
    /// </summary>
    /// <typeparam name="TInterceptor">
    ///     The type of <see cref="IDbInterceptor" /> to dispatch to.
    /// </typeparam>
    public class DispatcherBase<TInterceptor>
        where TInterceptor : class, IDbInterceptor
    {
        private readonly InternalDispatcher<TInterceptor> _internalDispatcher = new InternalDispatcher<TInterceptor>();

        internal DispatcherBase()
        {
        }

        internal InternalDispatcher<TInterceptor> InternalDispatcher
        {
            get { return _internalDispatcher; }
        }
    }
}
