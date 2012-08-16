// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Implements <see cref="IDbDependencyResolver" /> to resolve a dependency such that it always returns
    ///     the same instance and does nothing on Release.
    /// </summary>
    /// <typeparam name="T"> The type that defines the contract for the dependency that will be resolved. </typeparam>
    /// <remarks>
    ///     This class is immutable such that instances can be accessed by multiple threads at the same time.
    /// </remarks>
    public class SingletonDependencyResolver<T> : IDbDependencyResolver
    {
        private readonly T _singletonInstance;
        private readonly object _key;

        /// <summary>
        ///     Constructs a new resolver that will return the given instance for the contract type
        ///     regardless of the name passed to the Get method.
        /// </summary>
        /// <param name="singletonInstance"> The instance to return. </param>
        public SingletonDependencyResolver(T singletonInstance)
            : this(singletonInstance, null)
        {
        }

        /// <summary>
        ///     Constructs a new resolver that will return the given instance for the contract type
        ///     if the given name matches exactly the name passed to the Get method.
        /// </summary>
        /// <param name="singletonInstance"> The instance to return. </param>
        /// <para> The name of the dependency to resolve. </para>
        public SingletonDependencyResolver(T singletonInstance, object key)
        {
            Contract.Requires(singletonInstance != null);

            _singletonInstance = singletonInstance;
            _key = key;
        }

        /// <inheritdoc />
        public object GetService(Type type, object key)
        {
            return ((type == typeof(T))
                    && (_key == null || key == _key))
                       ? (object)_singletonInstance
                       : null;
        }

        /// <inheritdoc />
        public void Release(object service)
        {
        }
    }
}
