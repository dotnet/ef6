// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Spatial;
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

            _internalConfiguration.CheckNotLocked("AddDependencyResolver");
            _internalConfiguration.AddDependencyResolver(resolver);
        }

        /// <summary>
        ///     Attempts to locate and return an instance of a given service.
        /// </summary>
        /// <typeparam name="TService"> The service contract type. </typeparam>
        /// <returns> The resolved dependency, which must be an instance of the given contract type, or null if the dependency could not be resolved. </returns>
        public static TService GetService<TService>()
        {
            return GetService<TService>(null);
        }

        /// <summary>
        ///     Attempts to locate and return an instance of a given service with a given key.
        /// </summary>
        /// <typeparam name="TService"> The service contract type. </typeparam>
        /// <param name="key"> The optional key used to resolve the target service. </param>
        /// <returns> The resolved dependency, which must be an instance of the given contract type, or null if the dependency could not be resolved. </returns>
        public static TService GetService<TService>(object key)
        {
            return InternalConfiguration.Instance.GetService<TService>(key);
        }

        /// <summary>
        ///     Gets the <see cref="IDbDependencyResolver" /> that is being used to resolve service
        ///     dependencies in the Entity Framework.
        /// </summary>
        public static IDbDependencyResolver DependencyResolver
        {
            get { return InternalConfiguration.Instance.DependencyResolver; }
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to register
        ///     an Entity Framework provider.
        /// </summary>
        /// <remarks>
        ///     This method is provided as a convenient and discoverable way to add configuration to the entity framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="DbProviderServices" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolved backed by an Inversion-of-Control container.
        /// </remarks>
        /// <param name="providerInvariantName"> The ADO.NET provider invariant name indicating the type of ADO.NET connection for which this provider will be used. </param>
        /// <param name="provider"> The provider instance. </param>
        [CLSCompliant(false)]
        protected internal void AddProvider(string providerInvariantName, DbProviderServices provider)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(providerInvariantName));
            Contract.Requires(provider != null);

            _internalConfiguration.CheckNotLocked("AddProvider");
            _internalConfiguration.RegisterSingleton(provider, providerInvariantName);
        }

        /// <summary>
        ///     Sets the <see cref="IDbConnectionFactory" /> that is used to create connections by convention if no other
        ///     connection string or connection is given to or can be discovered by <see cref="DbContext" />.
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to change
        ///     the default connection factory being used.
        /// </summary>
        /// <remarks>
        ///     This method is provided as a convenient and discoverable way to add configuration to the entity framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="IDbConnectionFactory" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolved backed by an Inversion-of-Control container.
        /// </remarks>
        /// <param name="connectionFactory"> The connection factory. </param>
        protected internal void SetDefaultConnectionFactory(IDbConnectionFactory connectionFactory)
        {
            Contract.Requires(connectionFactory != null);

            _internalConfiguration.CheckNotLocked("SetDefaultConnectionFactory");
            _internalConfiguration.RegisterSingleton(connectionFactory, null);
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to 
        ///     set the database initializer to use for the given context type.  The database initializer is called when a
        ///     the given <see cref="DbContext" /> type is used to access a database for the first time.
        ///     The default strategy for Code First contexts is an instance of <see cref="CreateDatabaseIfNotExists{TContext}" />.
        /// </summary>
        /// <remarks>
        ///     Calling this method is equivalent to calling <see cref="Database.SetInitializer{TContext}" />.
        ///     This method is provided as a convenient and discoverable way to add configuration to the entity framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="IDatabaseInitializer{TContext}" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolved backed by an Inversion-of-Control container.
        /// </remarks>
        /// <typeparam name="TContext"> The type of the context. </typeparam>
        /// <param name="initializer"> The initializer to use, or null to disable initialization for the given context type. </param>
        protected internal void SetDatabaseInitializer<TContext>(IDatabaseInitializer<TContext> initializer) where TContext : DbContext
        {
            _internalConfiguration.CheckNotLocked("SetDatabaseInitializer");
            _internalConfiguration.RegisterSingleton(initializer ?? new NullDatabaseInitializer<TContext>(), null);
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to add a
        ///     <see cref="MigrationSqlGenerator" /> for use with the provider represented by the given invariant name.
        /// </summary>
        /// <remarks>
        ///     This method is typically used by providers to register an associated SQL generator for Code First Migrations.
        ///     It is different from setting the generator in the <see cref="DbMigrationsConfiguration" /> because it allows
        ///     EF to use the Migrations pipeline to create a database even when there is no Migrations configuration in the project
        ///     and/or Migrations are not being explicitly used.
        /// 
        ///     This method is provided as a convenient and discoverable way to add configuration to the entity framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="MigrationSqlGenerator" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolved backed by an Inversion-of-Control container.
        /// </remarks>
        /// <param name="providerInvariantName"> The invariant name of the ADO.NET provider for which this generator should be used. </param>
        /// <param name="sqlGenerator"> A delegate that returns a new instance of the SQL generator each time it is called. </param>
        protected internal void AddMigrationSqlGenerator(string providerInvariantName, Func<MigrationSqlGenerator> sqlGenerator)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(providerInvariantName));
            Contract.Requires(sqlGenerator != null);

            _internalConfiguration.CheckNotLocked("AddMigrationSqlGenerator");
            _internalConfiguration.AddDependencyResolver(
                new TransientDependencyResolver<MigrationSqlGenerator>(sqlGenerator, providerInvariantName));
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to set
        ///     an implementation of <see cref="IManifestTokenService" /> which allows provider manifest tokens to
        ///     be obtained from connections without necessarily opening the connection.
        /// </summary>
        /// <remarks>
        ///     This method is provided as a convenient and discoverable way to add configuration to the entity framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="IManifestTokenService" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolved backed by an Inversion-of-Control container.
        /// </remarks>
        /// <param name="service"> The manifest token service. </param>
        protected internal void SetManifestTokenService(IManifestTokenService service)
        {
            Contract.Requires(service != null);

            _internalConfiguration.CheckNotLocked("SetManifestTokenService");
            _internalConfiguration.RegisterSingleton(service, null);
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to set
        ///     an implementation of <see cref="IDbProviderFactoryService" /> which allows a <see cref="DbProviderFactory" />
        ///     to be obtained from a <see cref="DbConnection" /> in cases where the default implementation is not
        ///     sufficient.
        /// </summary>
        /// <remarks>
        ///     This method is provided as a convenient and discoverable way to add configuration to the entity framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="IDbProviderFactoryService" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolved backed by an Inversion-of-Control container.
        /// </remarks>
        /// <param name="providerFactoryService"> The provider factory service. </param>
        protected internal void SetProviderFactoryService(IDbProviderFactoryService providerFactoryService)
        {
            Contract.Requires(providerFactoryService != null);

            _internalConfiguration.CheckNotLocked("SetProviderFactoryService");
            _internalConfiguration.RegisterSingleton(providerFactoryService, null);
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to set
        ///     an implementation of <see cref="IDbModelCacheKeyFactory" /> which allows the key used to cache the
        ///     model behind a <see cref="DbContext" /> to be changed.
        /// </summary>
        /// <remarks>
        ///     This method is provided as a convenient and discoverable way to add configuration to the entity framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="IDbModelCacheKeyFactory" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolved backed by an Inversion-of-Control container.
        /// </remarks>
        /// <param name="keyFactory"> The key factory. </param>
        protected internal void SetModelCacheKeyFactory(IDbModelCacheKeyFactory keyFactory)
        {
            Contract.Requires(keyFactory != null);

            _internalConfiguration.CheckNotLocked("SetModelCacheKeyFactory");
            _internalConfiguration.RegisterSingleton(keyFactory, null);
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to set
        ///     an implementation of <see cref="IHistoryContextFactory" /> which allows for configuration of the
        ///     internal Migrations <see cref="HistoryContext" /> for a given <see cref="DbMigrationsConfiguration" />.
        /// </summary>
        /// <remarks>
        ///     This method is provided as a convenient and discoverable way to add configuration to the entity framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="IHistoryContextFactory" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolved backed by an Inversion-of-Control container.
        /// </remarks>
        /// <param name="historyContextFactory"> The <see cref="HistoryContext" /> factory. </param>
        /// <typeparam name="TMigrationsConfiguration"> The <see cref="DbMigrationsConfiguration" /> that this factory will apply to. </typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        protected internal void SetHistoryContextFactory<TMigrationsConfiguration>(IHistoryContextFactory historyContextFactory)
            where TMigrationsConfiguration : DbMigrationsConfiguration
        {
            Contract.Requires(historyContextFactory != null);

            _internalConfiguration.CheckNotLocked("SetHistoryContextFactory");
            _internalConfiguration.RegisterSingleton(historyContextFactory, typeof(TMigrationsConfiguration));
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to set
        ///     an implementation of <see cref="DbSpatialServices" /> which will be used whenever a spatial provider is
        ///     required. Normally the spatial provider is obtained from the EF provider's <see cref="DbProviderServices" />
        ///     implementation, but this can be overridden using this method. This also allows stand-alone instances of 
        ///     <see cref="DbGeometry" /> and <see cref="DbGeography" /> to be created using the correct provider.
        ///     Note that only one spatial provider can be set in this way; it is not possible to set different spatial providers
        ///     for different EF/ADO.NET providers.
        /// </summary>
        /// <remarks>
        ///     This method is provided as a convenient and discoverable way to add configuration to the entity framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="IDbModelCacheKeyFactory" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolved backed by an Inversion-of-Control container.
        /// </remarks>
        /// <param name="keyFactory"> The key factory. </param>
        protected internal void SetSpatialProvider(DbSpatialServices spatialProvider)
        {
            Contract.Requires(spatialProvider != null);

            _internalConfiguration.CheckNotLocked("SetSpatialProvider");
            _internalConfiguration.RegisterSingleton(spatialProvider, null);
        }

        internal virtual InternalConfiguration InternalConfiguration
        {
            get { return _internalConfiguration; }
        }
    }
}
