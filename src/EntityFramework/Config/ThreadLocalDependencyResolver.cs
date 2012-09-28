// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Diagnostics.Contracts;
    using System.Threading;

    /// <summary>
    ///     Implements <see cref="IDbDependencyResolver" /> to resolve a dependency such that it returns
    ///     a per-thread instance.
    /// </summary>
    /// <typeparam name="T"> The type that defines the contract for the dependency that will be resolved. </typeparam>
    /// <remarks>
    ///     This class is immutable such that instances can be accessed by multiple threads at the same time.
    /// </remarks>
    public sealed class ThreadLocalDependencyResolver<T> : IDbDependencyResolver, IDisposable
        where T : class
    {
        private readonly ThreadLocal<T> _threadLocal;
        private readonly object _key;

        /// <summary>
        ///     Constructs a new resolver that will return the given instance for the contract type
        ///     regardless of the key passed to the Get method.
        /// </summary>
        /// <param name="valueFactory"> The <see cref="T:System.Func{T}" /> invoked to produce a new per-thread instance of the target service. </param>
        public ThreadLocalDependencyResolver(Func<T> valueFactory)
            : this(valueFactory, null)
        {
            Contract.Requires(valueFactory != null);
        }

        /// <summary>
        ///     Constructs a new resolver that will return the given instance for the contract type
        ///     if the given key matches exactly the key passed to the Get method.
        /// </summary>
        /// <param name="valueFactory"> The <see cref="T:System.Func{T}" /> invoked to produce a new per-thread instance of the target service. </param>
        /// <param name="key"> Optionally, the key of the dependency to be resolved. This may be null for dependencies that are not differentiated by key. </param>
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
                    && (_key == null || Equals(key, _key)))
                       ? _threadLocal.Value
                       : null;
        }

        public void Dispose()
        {
            _threadLocal.Dispose();
        }
    }
}
