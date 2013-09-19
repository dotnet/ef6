// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    /// Extension methods to call the <see cref="IDbDependencyResolver.GetService" /> method using
    /// a generic type parameter and/or no name.
    /// </summary>
    public static class DbDependencyResolverExtensions
    {
        /// <summary>
        /// Calls <see cref="IDbDependencyResolver.GetService" /> passing the generic type of the method and the given
        /// name as arguments.
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
        /// Calls <see cref="IDbDependencyResolver.GetService" /> passing the generic type of the method as
        /// the type argument and null for the name argument.
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
        /// Calls <see cref="IDbDependencyResolver.GetService" /> passing the given type argument and using
        /// null for the name argument.
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

        /// <summary>
        /// Calls <see cref="IDbDependencyResolver.GetServices" /> passing the generic type of the method and the given
        /// name as arguments.
        /// </summary>
        /// <typeparam name="T"> The contract type to resolve. </typeparam>
        /// <param name="resolver"> The resolver to use. </param>
        /// <param name="key"> The key of the dependency to resolve. </param>
        /// <returns> All resolved dependencies, or an <see cref="IEnumerable{T}"/> if no services are resolved.</returns>
        public static IEnumerable<T> GetServices<T>(this IDbDependencyResolver resolver, object key)
        {
            Check.NotNull(resolver, "resolver");

            return resolver.GetServices(typeof(T), key).OfType<T>();
        }

        /// <summary>
        /// Calls <see cref="IDbDependencyResolver.GetServices" /> passing the generic type of the method as
        /// the type argument and null for the name argument.
        /// </summary>
        /// <typeparam name="T"> The contract type to resolve. </typeparam>
        /// <param name="resolver"> The resolver to use. </param>
        /// <returns> All resolved dependencies, or an <see cref="IEnumerable{T}"/> if no services are resolved.</returns>
        public static IEnumerable<T> GetServices<T>(this IDbDependencyResolver resolver)
        {
            Check.NotNull(resolver, "resolver");

            return resolver.GetServices(typeof(T), null).OfType<T>();
        }

        /// <summary>
        /// Calls <see cref="IDbDependencyResolver.GetServices" /> passing the given type argument and using
        /// null for the name argument.
        /// </summary>
        /// <param name="resolver"> The resolver to use. </param>
        /// <param name="type"> The contract type to resolve. </param>
        /// <returns> All resolved dependencies, or an <see cref="IEnumerable{Object}"/> if no services are resolved.</returns>
        public static IEnumerable<object> GetServices(this IDbDependencyResolver resolver, Type type)
        {
            Check.NotNull(resolver, "resolver");
            Check.NotNull(type, "type");

            return resolver.GetServices(type, null);
        }

        /// <summary>
        /// This is a helper method that can be used in an <see cref="IDbDependencyResolver.GetServices"/> implementation 
        /// such that an empty list is returned if the <see cref="IDbDependencyResolver.GetService"/> returns null
        /// and a list of one element is returned if GetService returns one element.
        /// </summary>
        /// <param name="resolver">The resolver.</param>
        /// <param name="type"> The contract type to resolve. </param>
        /// <param name="key"> The key of the dependency to resolve. </param>
        /// <returns>A list of either zero or one elements.</returns>
        internal static IEnumerable<object> GetServiceAsServices(this IDbDependencyResolver resolver, Type type, object key)
        {
            DebugCheck.NotNull(resolver);

            var service = resolver.GetService(type, key);
            return service == null ? Enumerable.Empty<object>() : new[] { service };
        }
    }
}
