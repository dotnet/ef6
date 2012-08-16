// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Diagnostics.Contracts;
    using System.Threading;

    public sealed class ThreadLocalDependencyResolver<T> : IDbDependencyResolver, IDisposable
    {
        private readonly ThreadLocal<T> _threadLocal;
        private readonly object _key;

        public ThreadLocalDependencyResolver(Func<T> valueFactory)
            : this(valueFactory, null)
        {
            Contract.Requires(valueFactory != null);
        }

        public ThreadLocalDependencyResolver(Func<T> valueFactory, object key)
        {
            Contract.Requires(valueFactory != null);

            _threadLocal = new ThreadLocal<T>(valueFactory);
            _key = key;
        }

        /// <inheritdoc />
        public object GetService(Type type, object key)
        {
            return ((type == typeof(T))
                    && (_key == null || key == _key))
                       ? (object)_threadLocal.Value
                       : null;
        }

        /// <inheritdoc />
        public void Release(object service)
        {
        }

        public void Dispose()
        {
            _threadLocal.Dispose();
        }
    }
}
