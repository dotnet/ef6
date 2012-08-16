// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Implements a Composite pattern for <see cref="IDbDependencyResolver" /> such that if the first
    ///     resolver can't resolve the dependency then the second resolver will be used.
    /// </summary>
    internal class CompositeResolver<TFirst, TSecond> : IDbDependencyResolver
        where TFirst : IDbDependencyResolver
        where TSecond : IDbDependencyResolver
    {
        // DbConfiguration depends on this class being immutable
        private readonly TFirst _firstResolver;
        private readonly TSecond _secondResolver;

        public CompositeResolver(TFirst firstResolver, TSecond secondResolver)
        {
            Contract.Requires(firstResolver != null);
            Contract.Requires(secondResolver != null);

            _firstResolver = firstResolver;
            _secondResolver = secondResolver;
        }

        public TFirst First
        {
            get { return _firstResolver; }
        }

        public TSecond Second
        {
            get { return _secondResolver; }
        }

        public virtual object GetService(Type type, object key)
        {
            return _firstResolver.GetService(type, key) ?? _secondResolver.GetService(type, key);
        }

        public virtual void Release(object service)
        {
            _firstResolver.Release(service);
            _secondResolver.Release(service);
        }
    }
}
