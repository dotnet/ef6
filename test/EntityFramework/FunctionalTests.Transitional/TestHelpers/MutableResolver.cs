// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Config;

    /// <summary>
    ///     A resolver that allows to add dependency resolvers at runtime.
    /// </summary>
    /// <remarks>
    ///     This class isn't thread-safe as the tests using it aren't expected to be run in parallel.
    /// </remarks>
    public class MutableResolver : IDbDependencyResolver
    {
        private static readonly Dictionary<Type, Func<object, object>> _resolvers = new Dictionary<Type, Func<object, object>>();
        private static readonly MutableResolver _instance = new MutableResolver();

        private MutableResolver()
        {
        }

        /// <inheritdoc/>
        public object GetService(Type type, object key)
        {
            Func<object, object> resolver;
            if (_resolvers.TryGetValue(type, out resolver))
            {
                return resolver(key);
            }

            return null;
        }

        public static MutableResolver Instance
        {
            get { return _instance; }
        }

        /// <summary>
        ///     Adds or replaces a resolver for a dependency of type <typeparamref name="TResolver"/>.
        /// </summary>
        /// <remarks>
        ///     Remember to call <see cref="ClearResolvers"/> from a <c>finally</c> block after using this method.
        /// </remarks>
        /// <typeparam name="TResolver">The type of dependency to resolve.</typeparam>
        /// <param name="resolver">A delegate that takes a key object and returns a dependency instance.</param>
        public static void AddResolver<TResolver>(Func<object, object> resolver)
        {
            _resolvers.Add(typeof(TResolver), resolver);
        }

        /// <summary>
        ///     Removes all added resolvers.
        /// </summary>
        public static void ClearResolvers()
        {
            _resolvers.Clear();
        }
    }
}
