// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Implements a Composite pattern for <see cref="IDbDependencyResolver" /> such that if the first
    ///     resolver can't resolve the dependency then the second resolver will be used.
    /// </summary>
    internal class CompositeResolver<TFirst, TSecond> : IDbDependencyResolver
        where TFirst : class, IDbDependencyResolver
        where TSecond : class, IDbDependencyResolver
    {
        // DbConfiguration depends on this class being immutable
        private readonly TFirst _firstResolver;
        private readonly TSecond _secondResolver;

        public CompositeResolver(TFirst firstResolver, TSecond secondResolver)
        {
            DebugCheck.NotNull(firstResolver);
            DebugCheck.NotNull(secondResolver);

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

        public IEnumerable<object> GetServices(Type type, object key)
        {
            return _firstResolver.GetServices(type, key).Concat(_secondResolver.GetServices(type, key));
        }
    }
}
