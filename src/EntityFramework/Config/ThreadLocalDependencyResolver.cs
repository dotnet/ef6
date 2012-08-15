// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Diagnostics.Contracts;
    using System.Threading;
    
    public sealed class ThreadLocalDependencyResolver<T> : IDbDependencyResolver, IDisposable
    {
        private readonly ThreadLocal<T> _threadLocal;
        private readonly string _name;

        public ThreadLocalDependencyResolver(Func<T> valueFactory)
            : this(valueFactory, null)
        {
            Contract.Requires(valueFactory != null);
        }

        public ThreadLocalDependencyResolver(Func<T> valueFactory, string name)
        {
            Contract.Requires(valueFactory != null);

            _threadLocal = new ThreadLocal<T>(valueFactory);
            _name = name;
        }

        /// <inheritdoc />
        public object GetService(Type type, string name)
        {
            return type == typeof(T)
                   && (_name == null || name == _name)
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
