// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Implements <see cref="IDbDependencyResolver" /> to resolve a dependency such that it always returns
    ///     a new instance.
    /// </summary>
    /// <typeparam name="T"> The type that defines the contract for the dependency that will be resolved. </typeparam>
    /// <remarks>
    ///     This class is immutable such that instances can be accessed by multiple threads at the same time.
    /// </remarks>
    public class TransientDependencyResolver<T> : IDbDependencyResolver
        where T : class
    {
        private readonly Func<T> _activator;
        private readonly object _key;

        /// <summary>
        ///     Constructs a new resolver that will return the given instance for the contract type.
        /// </summary>
        /// <param name="activator">
        ///     The <see cref="T:System.Func{T}" /> invoked to produce a new transient instance of the target service.
        /// </param>
        public TransientDependencyResolver(Func<T> activator)
            : this(activator, null)
        {
            Check.NotNull(activator, "activator");
        }

        /// <summary>
        ///     Constructs a new resolver that will return the given instance for the contract type
        ///     if the given key matches exactly the key passed to the Get method.
        /// </summary>
        /// <param name="activator">
        ///     The <see cref="T:System.Func{T}" /> invoked to produce a new transient instance of the target service.
        /// </param>
        /// <param name="key"> Optionally, the key of the dependency to be resolved. This may be null for dependencies that are not differentiated by key. </param>
        public TransientDependencyResolver(Func<T> activator, object key)
        {
            Check.NotNull(activator, "activator");

            _activator = activator;
            _key = key;
        }

        /// <inheritdoc />
        public object GetService(Type type, object key)
        {
            return ((type == typeof(T))
                    && (_key == null || Equals(key, _key)))
                       ? _activator()
                       : null;
        }

        /// <inheritdoc />
        public IEnumerable<object> GetServices(Type type, object key)
        {
            return this.GetServiceAsServices(type, key);
        }
    }
}
