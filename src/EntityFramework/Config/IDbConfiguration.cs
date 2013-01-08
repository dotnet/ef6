// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    /// <summary>
    /// A view over <see cref="DbConfiguration"/> usually used in <see cref="DbConfiguration.OnLockingConfiguration"/>
    /// event handlers.
    /// </summary>
    public interface IDbConfiguration
    {
        /// <summary>
        ///     Call this method to add a <see cref="IDbDependencyResolver" /> instance to the Chain of
        ///     Responsibility of resolvers that are used to resolve dependencies needed by the Entity Framework.
        ///     This method is usually accessed from within an OnLockingConfiguration event handler.
        /// </summary>
        /// <remarks>
        ///     Resolvers are asked to resolve dependencies in reverse order from which they are added. This means
        ///     that a resolver can be added to override resolution of a dependency that would already have been
        ///     resolved in a different way.
        ///     The only exception to this is that any dependency registered in the application's config file
        ///     will always be used in preference to using a dependency resolver added here, unless the
        ///     overrideConfigFile is set to true in which case the resolver added here will also override config
        ///     file settings.
        /// </remarks>
        /// <param name="resolver"> The resolver to add. </param>
        /// <param name="overrideConfigFile">If true, then the resolver added will take precedence over settings in the config file.</param>
        void AddDependencyResolver(IDbDependencyResolver resolver, bool overrideConfigFile);

        /// <summary>
        ///     Returns the <see cref="IDbDependencyResolver" /> that is about to be locked.
        ///     Use the GetService methods on this object to get services that have been registered.
        ///     This property is usually accessed from within an OnLockingConfiguration event handler.
        /// </summary>
        IDbDependencyResolver DependencyResolver { get; }
    }
}
