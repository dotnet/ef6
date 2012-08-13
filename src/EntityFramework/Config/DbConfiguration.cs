// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     A class derived from this class can be placed in the same assembly as a class derived from
    ///     <see cref="DbContext" /> to define Entity Framework configuration for an application.
    ///     Configuration is set by calling protected methods and setting protected properties of this
    ///     class in the constructor of your derived type.
    ///     The type to use can also be registered in the config file of the application.
    ///     See http://go.microsoft.com/fwlink/?LinkId=260883 for more information about Entity Framework configuration.
    /// </summary>
    [SuppressMessage("Microsoft.Contracts", "CC1036",
        Justification = "Due to a bug in code contracts IsNullOrWhiteSpace isn't recognized as pure.")]
    public class DbConfiguration
    {
        private readonly InternalConfiguration _internalConfiguration;

        /// <summary>
        ///     Any class derived from <see cref="DbConfiguration" /> must have a public parameterless constructor
        ///     and that constructor should call this constructor.
        /// </summary>
        protected internal DbConfiguration()
            : this(new InternalConfiguration())
        {
            _internalConfiguration.Owner = this;
        }

        internal DbConfiguration(InternalConfiguration internalConfiguration)
        {
            Contract.Requires(internalConfiguration != null);

            _internalConfiguration = internalConfiguration;
            _internalConfiguration.Owner = this;
        }

        /// <summary>
        ///     The Singleton instance of <see cref="DbConfiguration" /> for this app domain. This can be
        ///     set at application start before any Entity Framework features have been used and afterwards
        ///     should be treated as read-only.
        /// </summary>
        public static void SetConfiguration(DbConfiguration configuration)
        {
            Contract.Requires(configuration != null);

            InternalConfiguration.Instance = configuration.InternalConfiguration;
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to
        ///     add a <see cref="IDbDependencyResolver" /> instance to the Chain of Responsibility of resolvers that
        ///     are used to resolve dependencies needed by the Entity Framework.
        /// </summary>
        /// <remarks>
        ///     Resolvers are asked to resolve dependencies in reverse order from which they are added. This means
        ///     that a resolver can be added to override resolution of a dependency that would already have been
        ///     resolved in a different way.
        ///     The only exception to this is that any dependency registered in the application's config file
        ///     will always be used in preference to using a dependency resolver added here.
        /// </remarks>
        /// <param name="resolver"> The resolver to add. </param>
        protected internal void AddDependencyResolver(IDbDependencyResolver resolver)
        {
            Contract.Requires(resolver != null);

            _internalConfiguration.AddDependencyResolver(resolver);
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to register
        ///     an Entity Framework provider.
        /// </summary>
        /// <param name="providerInvariantName"> The ADO.NET provider invariant name indicating the type of ADO.NET connection for which this provider will be used. </param>
        /// <param name="provider"> The provider instance. </param>
        [CLSCompliant(false)]
        protected internal void AddProvider(string providerInvariantName, DbProviderServices provider)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(providerInvariantName));
            Contract.Requires(provider != null);

            _internalConfiguration.AddProvider(providerInvariantName, provider);
        }

        /// <summary>
        ///     Gets the Entity Framework provider that has been registered for use with ADO.NET connections that are
        ///     identified by the given ADO.NET provider invariant name.
        /// </summary>
        /// <param name="providerInvariantName"> The provider invariant name. </param>
        /// <returns> The registered provider. </returns>
        [CLSCompliant(false)]
        public static DbProviderServices GetProvider(string providerInvariantName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(providerInvariantName));

            return InternalConfiguration.Instance.GetProvider(providerInvariantName);
        }

        /// <summary>
        ///     Sets the <see cref="IDbConnectionFactory" /> that is used to create connections by convention if no other
        ///     connection string or connection is given to or can be discovered by <see cref="DbContext" />.
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to change
        ///     the default connection factory being used.
        /// </summary>
        protected internal void SetDefaultConnectionFactory(IDbConnectionFactory connectionFactory)
        {
            Contract.Requires(connectionFactory != null);

            _internalConfiguration.DefaultConnectionFactory = connectionFactory;
        }

        /// <summary>
        ///     The <see cref="IDbConnectionFactory" /> that is used to create connections by convention if no other
        ///     connection string or connection is given to or can be discovered by <see cref="DbContext" />.
        /// </summary>
        public static IDbConnectionFactory DefaultConnectionFactory
        {
            get { return InternalConfiguration.Instance.DefaultConnectionFactory; }
        }

        /// <summary>
        ///     Gets the <see cref="IDbDependencyResolver" /> that is being used to resolve service
        ///     dependencies in the Entity Framework.
        /// </summary>
        public static IDbDependencyResolver DependencyResolver
        {
            get { return InternalConfiguration.Instance.DependencyResolver; }
        }

        internal virtual InternalConfiguration InternalConfiguration
        {
            get { return _internalConfiguration; }
        }
    }
}
