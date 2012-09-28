// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections.Concurrent;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     This class wraps another <see cref="IDbDependencyResolver" /> such that the resolutions
    ///     made by that resolver are cached in a thread-safe manner.
    /// </summary>
    internal class CachingDependencyResolver : IDbDependencyResolver
    {
        private readonly IDbDependencyResolver _underlyingResolver;

        private readonly ConcurrentDictionary<Tuple<Type, object>, object> _resolvedDependencies
            = new ConcurrentDictionary<Tuple<Type, object>, object>();

        public CachingDependencyResolver(IDbDependencyResolver underlyingResolver)
        {
            Contract.Requires(underlyingResolver != null);

            _underlyingResolver = underlyingResolver;
        }

        public virtual object GetService(Type type, object key)
        {
            return _resolvedDependencies.GetOrAdd(
                Tuple.Create(type, key),
                k => _underlyingResolver.GetService(type, key));
        }
    }
}
