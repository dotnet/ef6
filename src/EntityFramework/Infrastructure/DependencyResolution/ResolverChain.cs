// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    /// Chain-of-Responsibility implementation for <see cref="IDbDependencyResolver" /> instances.
    /// </summary>
    /// <remarks>
    /// When GetService is called each resolver added to the chain is called in turn until one
    /// returns a non-null value. If all resolvers in the chain return null, then GetService
    /// returns null. Resolvers are called in the reverse order to which they are added so that
    /// the most recently added resolvers get a chance to resolve first.
    /// This class is thread-safe.
    /// </remarks>
    internal class ResolverChain : IDbDependencyResolver
    {
        // DbConfiguration depends on this class being thread safe
        private readonly ConcurrentStack<IDbDependencyResolver> _resolvers = new ConcurrentStack<IDbDependencyResolver>();
        private volatile IDbDependencyResolver[] _resolversSnapshot = new IDbDependencyResolver[0];

        /// <summary>
        /// Adds a new resolver to the top of the chain.
        /// </summary>
        /// <param name="resolver">The resolver to add.</param>
        public virtual void Add(IDbDependencyResolver resolver)
        {
            Check.NotNull(resolver, "resolver");

            // The idea here is that Add and GetService must all be thread-safe, but
            // Add is only called infrequently. Therefore each time Add is called a snapshot is taken
            // of the stack that can then be enumerated without needing to make a snapshot
            // every time the enumeration is asked for, which is the normal behavior for the concurrent
            // collections.
            _resolvers.Push(resolver);
            _resolversSnapshot = _resolvers.ToArray();
        }

        /// <summary>
        /// Gets the resolvers in the chain in the order that they will be called to
        /// resolve a dependency.
        /// </summary>
        public virtual IEnumerable<IDbDependencyResolver> Resolvers
        {
            get { return _resolversSnapshot.Reverse(); }
        }

        /// <summary>
        /// Calls GetService on each resolver in the chain in turn and returns the first non-null value
        /// or returns null if all GetService calls return null. Resolvers are called in the reverse order
        /// to which they are added so that the most recently added resolvers get a chance to resolve first.
        /// </summary>
        /// <param name="type">The type of service to resolve.</param>
        /// <param name="key">
        /// An optional key value which may be used to determine the service instance to create.
        /// </param>
        /// <returns>The resolved service, or null if no resolver in the chain could resolve the service.</returns>
        public virtual object GetService(Type type, object key)
        {
            return _resolversSnapshot
                .Select(r => r.GetService(type, key))
                .FirstOrDefault(s => s != null);
        }

        /// <summary>
        /// Calls GetServices with the given type and key on each resolver in the chain and concatenates all
        /// the results into a single enumeration.
        /// </summary>
        /// <param name="type">The type of service to resolve.</param>
        /// <param name="key">
        /// An optional key value which may be used to determine the service instance to create.
        /// </param>
        /// <returns>All the resolved services, or an empty enumeration if no resolver in the chain could resolve the service.</returns>
        public virtual IEnumerable<object> GetServices(Type type, object key)
        {
            return _resolversSnapshot.SelectMany(r => r.GetServices(type, key));
        }
    }
}
