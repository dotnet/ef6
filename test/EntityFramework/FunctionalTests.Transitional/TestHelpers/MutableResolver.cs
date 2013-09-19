// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Reflection;

    /// <summary>
    /// A resolver that allows to add dependency resolvers at runtime.
    /// </summary>
    /// <remarks>
    /// This class isn't thread-safe as the tests using it aren't expected to be run in parallel.
    /// </remarks>
    public class MutableResolver : IDbDependencyResolver
    {
        private static readonly Dictionary<Type, Func<object, object>> _resolvers = new Dictionary<Type, Func<object, object>>();
        private static readonly MutableResolver _instance = new MutableResolver();

        private static readonly FieldInfo _executionStrategyFactoriesField = typeof(DbProviderServices).GetField("_executionStrategyFactories", BindingFlags.NonPublic | BindingFlags.Static);

        private MutableResolver()
        {
        }

        /// <inheritdoc />
        public object GetService(Type type, object key)
        {
            Func<object, object> resolver;
            if (_resolvers.TryGetValue(type, out resolver))
            {
                return resolver(key);
            }

            return null;
        }

        public IEnumerable<object> GetServices(Type type, object key)
        {
            return this.GetServiceAsServices(type, key);
        }

        public static MutableResolver Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Adds or replaces a resolver for a dependency of type <typeparamref name="TResolver" />.
        /// </summary>
        /// <remarks>
        /// Remember to call <see cref="ClearResolvers" /> from a <c>finally</c> block or Dispose method after using this method.
        /// </remarks>
        /// <typeparam name="TResolver">The type of dependency to resolve.</typeparam>
        /// <param name="resolver">A delegate that takes a key object and returns a dependency instance.</param>
        public static void AddResolver<TResolver>(Func<object, object> resolver)
        {
            if (typeof(TResolver) == typeof(Func<IDbExecutionStrategy>))
            {
                ClearCache();
            }
            _resolvers[typeof(TResolver)] = resolver;
        }

        /// <summary>
        /// Adds or replaces a resolver for a dependency of type <typeparamref name="TResolver" />.
        /// </summary>
        /// <remarks>
        /// Remember to call <see cref="ClearResolvers" /> from a <c>finally</c> block or Dispose method after using this method.
        /// </remarks>
        public static void AddResolver<TResolver>(IDbDependencyResolver resolver)
        {
            _resolvers[typeof(TResolver)] = k => resolver.GetService<TResolver>(k);
        }

        /// <summary>
        /// Removes all added resolvers.
        /// </summary>
        public static void ClearResolvers()
        {
            _resolvers.Clear();

            ClearCache();
        }

        private static void ClearCache()
        {
            var executionStrategyFactories = (ConcurrentDictionary<ExecutionStrategyKey, Func<IDbExecutionStrategy>>)_executionStrategyFactoriesField.GetValue(null);
            executionStrategyFactories.Clear();
        }
    }
}
