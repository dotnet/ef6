// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Config
{
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Extension methods to call the <see cref="IDbDependencyResolver.GetService"/> method using
    /// a generic type parameter and/or no name.
    /// </summary>
    public static class IDbDependencyResolverExtensions
    {
        /// <summary>
        /// Calls <see cref="IDbDependencyResolver.GetService"/> passing the generic type of the method and the given
        /// name as arguments.
        /// </summary>
        /// <typeparam name="T">The contract type to resolve.</typeparam>
        /// <param name="resolver">The resolver to use.</param>
        /// <param name="name">The name of the dependency to resolve.</param>
        /// <returns>The resolved dependency, or null if the resolver could not resolve it.</returns>
        public static T GetService<T>(this IDbDependencyResolver resolver, string name)
        {
            Contract.Requires(resolver != null);

            return (T)resolver.GetService(typeof(T), name);
        }

        /// <summary>
        /// Calls <see cref="IDbDependencyResolver.GetService"/> passing the generic type of the method as
        /// the type argument and null for the name argument.
        /// </summary>
        /// <typeparam name="T">The contract type to resolve.</typeparam>
        /// <param name="resolver">The resolver to use.</param>
        /// <returns>The resolved dependency, or null if the resolver could not resolve it.</returns>
        public static T GetService<T>(this IDbDependencyResolver resolver)
        {
            Contract.Requires(resolver != null);

            return (T)resolver.GetService(typeof(T), null);
        }

        /// <summary>
        /// Calls <see cref="IDbDependencyResolver.GetService"/> passing the given type argument and using
        /// null for the name argument.
        /// </summary>
        /// <param name="type">The contract type to resolve.</param>
        /// <param name="resolver">The resolver to use.</param>
        /// <returns>The resolved dependency, or null if the resolver could not resolve it.</returns>
        public static object GetService(this IDbDependencyResolver resolver, Type type)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(type != null);

            return resolver.GetService(type, null);
        }
    }
}
