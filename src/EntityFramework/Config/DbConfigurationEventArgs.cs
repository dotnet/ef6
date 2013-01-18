// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Event arguments passed to <see cref="DbConfiguration.OnLockingConfiguration"/> event handlers.
    /// </summary>
    public class DbConfigurationEventArgs : EventArgs
    {
        private readonly InternalConfiguration _internalConfiguration;

        internal DbConfigurationEventArgs(InternalConfiguration configuration)
        {
            DebugCheck.NotNull(configuration);

            _internalConfiguration = configuration;
        }

        /// <summary>
        ///     Returns a snapshot of the <see cref="IDbDependencyResolver" /> that is about to be locked.
        ///     Use the GetService methods on this object to get services that have been registered.
        /// </summary>
        public IDbDependencyResolver ResolverSnapshot
        {
            get { return _internalConfiguration.ResolverSnapshot; }
        }

        /// <summary>
        ///     Call this method to add a <see cref="IDbDependencyResolver" /> instance to the Chain of
        ///     Responsibility of resolvers that are used to resolve dependencies needed by the Entity Framework.
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
        public void AddDependencyResolver(IDbDependencyResolver resolver, bool overrideConfigFile)
        {
            Check.NotNull(resolver, "resolver");

            _internalConfiguration.CheckNotLocked("AddDependencyResolver");
            _internalConfiguration.AddDependencyResolver(resolver, overrideConfigFile);
        }

        /// <summary>
        ///     Adds a wrapping resolver to the configuration that is about to be locked. A wrapping
        ///     resolver is a resolver that incepts a service would have been returned by the resolver
        ///     chain and wraps or replaces it with another service of the same type.
        /// </summary>
        /// <typeparam name="TService">The type of service to wrap.</typeparam>
        /// <param name="wrapService">A delegate that takes the unwrapped service and key and returns the wrapped service.</param>
        public void WrapService<TService>(Func<TService, object, TService> serviceWrapper)
        {
            Check.NotNull(serviceWrapper, "serviceWrapper");

            AddDependencyResolver(
                new WrappingDependencyResolver<TService>(ResolverSnapshot, serviceWrapper),
                overrideConfigFile: true);
        }
    }
}
