// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     This class wraps another <see cref="IDbDependencyResolver" /> such that the resolutions
    ///     made by that resolver are cached in a thread-safe manner.
    /// </summary>
    internal class CachingDependencyResolver : IDbDependencyResolver
    {
        private readonly IDbDependencyResolver _underlyingResolver;

        private readonly ConcurrentDictionary<Tuple<Type, object>, object> _resolvedDependencies
            = new ConcurrentDictionary<Tuple<Type, object>, object>();

        private readonly ConcurrentDictionary<Tuple<Type, object>, IEnumerable<object>> _resolvedAllDependencies
            = new ConcurrentDictionary<Tuple<Type, object>, IEnumerable<object>>();


        public CachingDependencyResolver(IDbDependencyResolver underlyingResolver)
        {
            DebugCheck.NotNull(underlyingResolver);

            _underlyingResolver = underlyingResolver;
        }

        public virtual object GetService(Type type, object key)
        {
            return _resolvedDependencies.GetOrAdd(
                Tuple.Create(type, key),
                k => _underlyingResolver.GetService(type, key));
        }

        public IEnumerable<object> GetServices(Type type, object key)
        {
            return _resolvedAllDependencies.GetOrAdd(
                Tuple.Create(type, key),
                k => _underlyingResolver.GetServices(type, key));
        }
    }
}
