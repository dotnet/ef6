// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Extension methods to call the <see cref="IDbDependencyResolver.GetService" /> method using
    ///     a generic type parameter and/or no name.
    /// </summary>
    public static class IDbDependencyResolverExtensions
    {
        /// <summary>
        ///     Calls <see cref="IDbDependencyResolver.GetService" /> passing the generic type of the method and the given
        ///     name as arguments.
        /// </summary>
        /// <typeparam name="T"> The contract type to resolve. </typeparam>
        /// <param name="resolver"> The resolver to use. </param>
        /// <param name="key"> The key of the dependency to resolve. </param>
        /// <returns> The resolved dependency, or null if the resolver could not resolve it. </returns>
        public static T GetService<T>(this IDbDependencyResolver resolver, object key)
        {
            Check.NotNull(resolver, "resolver");

            return (T)resolver.GetService(typeof(T), key);
        }

        /// <summary>
        ///     Calls <see cref="IDbDependencyResolver.GetService" /> passing the generic type of the method as
        ///     the type argument and null for the name argument.
        /// </summary>
        /// <typeparam name="T"> The contract type to resolve. </typeparam>
        /// <param name="resolver"> The resolver to use. </param>
        /// <returns> The resolved dependency, or null if the resolver could not resolve it. </returns>
        public static T GetService<T>(this IDbDependencyResolver resolver)
        {
            Check.NotNull(resolver, "resolver");

            return (T)resolver.GetService(typeof(T), null);
        }

        /// <summary>
        ///     Calls <see cref="IDbDependencyResolver.GetService" /> passing the given type argument and using
        ///     null for the name argument.
        /// </summary>
        /// <param name="resolver"> The resolver to use. </param>
        /// <param name="type"> The contract type to resolve. </param>
        /// <returns> The resolved dependency, or null if the resolver could not resolve it. </returns>
        public static object GetService(this IDbDependencyResolver resolver, Type type)
        {
            Check.NotNull(resolver, "resolver");
            Check.NotNull(type, "type");

            return resolver.GetService(type, null);
        }
    }
}
