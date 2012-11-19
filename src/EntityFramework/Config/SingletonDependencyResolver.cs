// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Implements <see cref="IDbDependencyResolver" /> to resolve a dependency such that it always returns
    ///     the same instance.
    /// </summary>
    /// <typeparam name="T"> The type that defines the contract for the dependency that will be resolved. </typeparam>
    /// <remarks>
    ///     This class is immutable such that instances can be accessed by multiple threads at the same time.
    /// </remarks>
    public class SingletonDependencyResolver<T> : IDbDependencyResolver
        where T : class
    {
        private readonly T _singletonInstance;
        private readonly object _key;

        /// <summary>
        ///     Constructs a new resolver that will return the given instance for the contract type
        ///     regardless of the key passed to the Get method.
        /// </summary>
        /// <param name="singletonInstance"> The instance to return. </param>
        public SingletonDependencyResolver(T singletonInstance)
            : this(singletonInstance, null)
        {
        }

        /// <summary>
        ///     Constructs a new resolver that will return the given instance for the contract type
        ///     if the given key matches exactly the key passed to the Get method.
        /// </summary>
        /// <param name="singletonInstance"> The instance to return. </param>
        /// <param name="key"> Optionally, the key of the dependency to be resolved. This may be null for dependencies that are not differentiated by key. </param>
        public SingletonDependencyResolver(T singletonInstance, object key)
        {
            Check.NotNull(singletonInstance, "singletonInstance");

            _singletonInstance = singletonInstance;
            _key = key;
        }

        /// <inheritdoc />
        public object GetService(Type type, object key)
        {
            return ((type == typeof(T))
                    && (_key == null || Equals(key, _key)))
                       ? _singletonInstance
                       : null;
        }
    }
}
