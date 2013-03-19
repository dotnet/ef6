// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping.ViewGeneration;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Pluralization;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

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
            DebugCheck.NotNull(internalConfiguration);

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
            Check.NotNull(configuration, "configuration");

            InternalConfiguration.Instance = configuration.InternalConfiguration;
        }

        /// <summary>
        ///     Occurs during EF initialization after the DbConfiguration has been constructed but just before
        ///     it is locked ready for use. Use this event to inspect and/or override services that have been
        ///     registered before the configuration is locked. Note that this event should be used carefully
        ///     since it may prevent tooling from discovering the same configuration that is used at runtime.
        /// </summary>
        /// <remarks>
        ///     Handlers can only be added before EF starts to use the configuration and so handlers should
        ///     generally be added as part of application initialization. Do not access the DbConfiguration
        ///     static methods inside the handler; instead use the the members of <see cref="DbConfigurationEventArgs" />
        ///     to get current services and/or add overrides.
        /// </remarks>
        public static event EventHandler<DbConfigurationEventArgs> OnLockingConfiguration
        {
            add
            {
                Check.NotNull(value, "value");

                DbConfigurationManager.Instance.AddOnLockingHandler(value);
            }
            remove
            {
                Check.NotNull(value, "value");

                DbConfigurationManager.Instance.RemoveOnLockingHandler(value);
            }
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
            Check.NotNull(resolver, "resolver");

            _internalConfiguration.CheckNotLocked("AddDependencyResolver");
            _internalConfiguration.AddDependencyResolver(resolver, overrideConfigFile: false);
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
        ///     This method is provided as a convenient and discoverable way to add configuration to the Entity Framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="DbProviderServices" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolver backed by an Inversion-of-Control container.
        /// </remarks>
        /// <param name="providerInvariantName"> The ADO.NET provider invariant name indicating the type of ADO.NET connection for which this provider will be used. </param>
        /// <param name="provider"> The provider instance. </param>
        [CLSCompliant(false)]
        protected internal void AddDbProviderServices(string providerInvariantName, DbProviderServices provider)
        {
            Check.NotEmpty(providerInvariantName, "providerInvariantName");
            Check.NotNull(provider, "provider");

            _internalConfiguration.CheckNotLocked("AddDbProviderServices");
            _internalConfiguration.RegisterSingleton(provider, providerInvariantName);
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to register
        ///     an Entity Framework provider.
        /// </summary>
        /// <remarks>
        ///     The giver provider type should have a <see cref="DbProviderNameAttribute" /> applied to it.
        ///     This method is provided as a convenient and discoverable way to add configuration to the Entity Framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="DbProviderServices" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolver backed by an Inversion-of-Control container.
        /// </remarks>
        /// <param name="provider"> The provider instance. </param>
        [CLSCompliant(false)]
        protected internal void AddDbProviderServices(DbProviderServices provider)
        {
            foreach (var providerInvariantNameAttribute in DbProviderNameAttribute.GetFromType(provider.GetType()))
            {
                AddDbProviderServices(providerInvariantNameAttribute.Name, provider);
            }
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to register
        ///     an ADO.NET provider.
        /// </summary>
        /// <remarks>
        ///     This method is provided as a convenient and discoverable way to add configuration to the Entity Framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolvers for
        ///     <see cref="DbProviderFactory" /> and <see cref="IProviderInvariantName" />. This means that, if desired,
        ///     the same functionality can be achieved using a custom resolver or a resolver backed by an
        ///     Inversion-of-Control container.
        /// </remarks>
        /// <param name="providerInvariantName"> The ADO.NET provider invariant name indicating the type of ADO.NET connection for which this provider will be used. </param>
        /// <param name="providerFactory"> The provider instance. </param>
        [CLSCompliant(false)]
        protected internal void AddDbProviderFactory(string providerInvariantName, DbProviderFactory providerFactory)
        {
            Check.NotEmpty(providerInvariantName, "providerInvariantName");
            Check.NotNull(providerFactory, "providerFactory");

            _internalConfiguration.CheckNotLocked("AddDbProviderFactory");
            _internalConfiguration.RegisterSingleton(providerFactory, providerInvariantName);
            _internalConfiguration.AddDependencyResolver(new InvariantNameResolver(providerFactory, providerInvariantName));
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to add an
        ///     <see cref="IExecutionStrategy" /> for use with the associated provider.
        /// </summary>
        /// <remarks>
        ///     The <typeparamref name="T" /> type should have a <see cref="DbProviderNameAttribute" /> applied to it.
        ///     This method is provided as a convenient and discoverable way to add configuration to the Entity Framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="IExecutionStrategy" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolver backed by an Inversion-of-Control container.
        /// </remarks>
        /// <typeparam name="T">
        ///     The type that implements <see cref="IExecutionStrategy" />.
        /// </typeparam>
        /// <param name="getExecutionStrategy"> A function that returns a new instance of an execution strategy. </param>
        protected internal void AddExecutionStrategy<T>(Func<T> getExecutionStrategy)
            where T : IExecutionStrategy
        {
            Check.NotNull(getExecutionStrategy, "getExecutionStrategy");

            _internalConfiguration.CheckNotLocked("AddExecutionStrategy");

            foreach (var providerInvariantNameAttribute in DbProviderNameAttribute.GetFromType(typeof(T)))
            {
                _internalConfiguration.AddDependencyResolver(
                    new ExecutionStrategyResolver<T>(providerInvariantNameAttribute.Name, /*serverName:*/ null, getExecutionStrategy));
            }
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to add an
        ///     <see cref="IExecutionStrategy" /> for use with the associated provider for the specified server name.
        /// </summary>
        /// <remarks>
        ///     The <typeparamref name="T" /> type should have a <see cref="DbProviderNameAttribute" /> applied to it.
        ///     This method is provided as a convenient and discoverable way to add configuration to the Entity Framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="IExecutionStrategy" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolver backed by an Inversion-of-Control container.
        /// </remarks>
        /// <typeparam name="T">
        ///     The type that implements <see cref="IExecutionStrategy" />.
        /// </typeparam>
        /// <param name="getExecutionStrategy"> A function that returns a new instance of an execution strategy. </param>
        /// <param name="serverName"> A string that will be matched against the server name in the connection string. </param>
        protected internal void AddExecutionStrategy<T>(Func<T> getExecutionStrategy, string serverName)
            where T : IExecutionStrategy
        {
            Check.NotEmpty(serverName, "serverName");
            Check.NotNull(getExecutionStrategy, "getExecutionStrategy");

            _internalConfiguration.CheckNotLocked("AddExecutionStrategy");
            foreach (var providerInvariantNameAttribute in DbProviderNameAttribute.GetFromType(typeof(T)))
            {
                _internalConfiguration.AddDependencyResolver(
                    new ExecutionStrategyResolver<T>(providerInvariantNameAttribute.Name, serverName, getExecutionStrategy));
            }
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to add an
        ///     <see cref="IExecutionStrategy" /> for use with the provider represented by the given invariant name.
        /// </summary>
        /// <remarks>
        ///     This method is provided as a convenient and discoverable way to add configuration to the Entity Framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="IExecutionStrategy" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolver backed by an Inversion-of-Control container.
        /// </remarks>
        /// <param name="providerInvariantName"> The ADO.NET provider invariant name indicating the type of ADO.NET connection for which this execution strategy will be used. </param>
        /// <param name="getExecutionStrategy"> A function that returns a new instance of an execution strategy. </param>
        protected internal void AddExecutionStrategy(string providerInvariantName, Func<IExecutionStrategy> getExecutionStrategy)
        {
            Check.NotEmpty(providerInvariantName, "providerInvariantName");
            Check.NotNull(getExecutionStrategy, "getExecutionStrategy");

            _internalConfiguration.CheckNotLocked("AddExecutionStrategy");
            _internalConfiguration.AddDependencyResolver(
                new ExecutionStrategyResolver<IExecutionStrategy>(providerInvariantName, /*serverName:*/ null, getExecutionStrategy));
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to add an
        ///     <see cref="IExecutionStrategy" /> for use with the provider represented by the given invariant name and for a given server name.
        /// </summary>
        /// <remarks>
        ///     This method is provided as a convenient and discoverable way to add configuration to the Entity Framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="IExecutionStrategy" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolver backed by an Inversion-of-Control container.
        /// </remarks>
        /// <param name="providerInvariantName"> The ADO.NET provider invariant name indicating the type of ADO.NET connection for which this execution strategy will be used. </param>
        /// <param name="getExecutionStrategy"> A function that returns a new instance of an execution strategy. </param>
        /// <param name="serverName"> A string that will be matched against the server name in the connection string. </param>
        protected internal void AddExecutionStrategy(
            string providerInvariantName, Func<IExecutionStrategy> getExecutionStrategy, string serverName)
        {
            Check.NotEmpty(providerInvariantName, "providerInvariantName");
            Check.NotEmpty(serverName, "serverName");
            Check.NotNull(getExecutionStrategy, "getExecutionStrategy");

            _internalConfiguration.CheckNotLocked("AddExecutionStrategy");
            _internalConfiguration.AddDependencyResolver(
                new ExecutionStrategyResolver<IExecutionStrategy>(providerInvariantName, serverName, getExecutionStrategy));
        }

        /// <summary>
        ///     Sets the <see cref="IDbConnectionFactory" /> that is used to create connections by convention if no other
        ///     connection string or connection is given to or can be discovered by <see cref="DbContext" />.
        ///     Note that a default connection factory is set in the app.config or web.config file whenever the
        ///     EntityFramework NuGet package is installed. As for all config file settings, the default connection factory
        ///     set in the config file will take precedence over any setting made with this method. Therefore the setting
        ///     must be removed from the config file before calling this method will have any effect.
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to change
        ///     the default connection factory being used.
        /// </summary>
        /// <remarks>
        ///     This method is provided as a convenient and discoverable way to add configuration to the Entity Framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="IDbConnectionFactory" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolver backed by an Inversion-of-Control container.
        /// </remarks>
        /// <param name="connectionFactory"> The connection factory. </param>
        protected internal void SetDefaultConnectionFactory(IDbConnectionFactory connectionFactory)
        {
            Check.NotNull(connectionFactory, "connectionFactory");

            _internalConfiguration.CheckNotLocked("SetDefaultConnectionFactory");
            _internalConfiguration.RegisterSingleton(connectionFactory, null);
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to
        ///     set the pluralization service.
        /// </summary>
        /// <param name="pluralizationService"> The pluralization service to use. </param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pluralization")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "pluralization")]
        protected internal void SetPluralizationService(IPluralizationService pluralizationService)
        {
            Check.NotNull(pluralizationService, "pluralizationService");

            _internalConfiguration.CheckNotLocked("SetPluralizationService");
            _internalConfiguration.RegisterSingleton(pluralizationService, null);
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to
        ///     set the database initializer to use for the given context type.  The database initializer is called when a
        ///     the given <see cref="DbContext" /> type is used to access a database for the first time.
        ///     The default strategy for Code First contexts is an instance of <see cref="CreateDatabaseIfNotExists{TContext}" />.
        /// </summary>
        /// <remarks>
        ///     Calling this method is equivalent to calling <see cref="Database.SetInitializer{TContext}" />.
        ///     This method is provided as a convenient and discoverable way to add configuration to the Entity Framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="IDatabaseInitializer{TContext}" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolver backed by an Inversion-of-Control container.
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
        ///     This method is provided as a convenient and discoverable way to add configuration to the Entity Framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="MigrationSqlGenerator" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolver backed by an Inversion-of-Control container.
        /// </remarks>
        /// <param name="providerInvariantName"> The invariant name of the ADO.NET provider for which this generator should be used. </param>
        /// <param name="sqlGenerator"> A delegate that returns a new instance of the SQL generator each time it is called. </param>
        protected internal void AddMigrationSqlGenerator(string providerInvariantName, Func<MigrationSqlGenerator> sqlGenerator)
        {
            Check.NotEmpty(providerInvariantName, "providerInvariantName");
            Check.NotNull(sqlGenerator, "sqlGenerator");

            _internalConfiguration.CheckNotLocked("AddMigrationSqlGenerator");
            _internalConfiguration.AddDependencyResolver(
                new TransientDependencyResolver<MigrationSqlGenerator>(sqlGenerator, providerInvariantName));
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to add a
        ///     <see cref="MigrationSqlGenerator" /> for use with the associated provider.
        /// </summary>
        /// <remarks>
        ///     The <typeparamref name="T" /> type should have a <see cref="DbProviderNameAttribute" /> applied to it.
        ///     This method is typically used by providers to register an associated SQL generator for Code First Migrations.
        ///     It is different from setting the generator in the <see cref="DbMigrationsConfiguration" /> because it allows
        ///     EF to use the Migrations pipeline to create a database even when there is no Migrations configuration in the project
        ///     and/or Migrations are not being explicitly used.
        ///     This method is provided as a convenient and discoverable way to add configuration to the Entity Framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="MigrationSqlGenerator" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolver backed by an Inversion-of-Control container.
        /// </remarks>
        /// <typeparam name="T">
        ///     The type that implements <see cref="MigrationSqlGenerator" />.
        /// </typeparam>
        /// <param name="sqlGenerator"> A delegate that returns a new instance of the SQL generator each time it is called. </param>
        protected internal void AddMigrationSqlGenerator<T>(Func<T> sqlGenerator)
            where T : MigrationSqlGenerator
        {
            foreach (var providerInvariantNameAttribute in DbProviderNameAttribute.GetFromType(typeof(T)))
            {
                AddMigrationSqlGenerator(providerInvariantNameAttribute.Name, sqlGenerator);
            }
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to set
        ///     an implementation of <see cref="IManifestTokenService" /> which allows provider manifest tokens to
        ///     be obtained from connections without necessarily opening the connection.
        /// </summary>
        /// <remarks>
        ///     This method is provided as a convenient and discoverable way to add configuration to the Entity Framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="IManifestTokenService" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolver backed by an Inversion-of-Control container.
        /// </remarks>
        /// <param name="service"> The manifest token service. </param>
        protected internal void SetManifestTokenService(IManifestTokenService service)
        {
            Check.NotNull(service, "service");

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
        ///     This method is provided as a convenient and discoverable way to add configuration to the Entity Framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="IDbProviderFactoryService" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolver backed by an Inversion-of-Control container.
        /// </remarks>
        /// <param name="providerFactoryService"> The provider factory service. </param>
        protected internal void SetProviderFactoryService(IDbProviderFactoryService providerFactoryService)
        {
            Check.NotNull(providerFactoryService, "providerFactoryService");

            _internalConfiguration.CheckNotLocked("SetProviderFactoryService");
            _internalConfiguration.RegisterSingleton(providerFactoryService, null);
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to set
        ///     an implementation of <see cref="IDbModelCacheKeyFactory" /> which allows the key used to cache the
        ///     model behind a <see cref="DbContext" /> to be changed.
        /// </summary>
        /// <remarks>
        ///     This method is provided as a convenient and discoverable way to add configuration to the Entity Framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="IDbModelCacheKeyFactory" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolver backed by an Inversion-of-Control container.
        /// </remarks>
        /// <param name="keyFactory"> The key factory. </param>
        protected internal void SetModelCacheKeyFactory(IDbModelCacheKeyFactory keyFactory)
        {
            Check.NotNull(keyFactory, "keyFactory");

            _internalConfiguration.CheckNotLocked("SetModelCacheKeyFactory");
            _internalConfiguration.RegisterSingleton(keyFactory, null);
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to set
        ///     an implementation of <see cref="IHistoryContextFactory" /> which allows for configuration of the
        ///     internal Migrations <see cref="HistoryContext" /> for a given <see cref="DbMigrationsConfiguration" />.
        /// </summary>
        /// <remarks>
        ///     This method is provided as a convenient and discoverable way to add configuration to the Entity Framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="IHistoryContextFactory" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolver backed by an Inversion-of-Control container.
        /// </remarks>
        /// <param name="historyContextFactory">
        ///     The <see cref="HistoryContext" /> factory.
        /// </param>
        /// <typeparam name="TMigrationsConfiguration">
        ///     The <see cref="DbMigrationsConfiguration" /> that this factory will apply to.
        /// </typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        protected internal void SetHistoryContextFactory<TMigrationsConfiguration>(IHistoryContextFactory historyContextFactory)
            where TMigrationsConfiguration : DbMigrationsConfiguration
        {
            Check.NotNull(historyContextFactory, "historyContextFactory");

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
        ///     This method is provided as a convenient and discoverable way to add configuration to the Entity Framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="IDbModelCacheKeyFactory" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolver backed by an Inversion-of-Control container.
        /// </remarks>
        /// <param name="spatialProvider"> The spatial provider. </param>
        protected internal void SetSpatialProvider(DbSpatialServices spatialProvider)
        {
            Check.NotNull(spatialProvider, "spatialProvider");

            _internalConfiguration.CheckNotLocked("SetSpatialProvider");
            _internalConfiguration.RegisterSingleton(spatialProvider, null);
        }

        /// <summary>
        ///     Call this method from the constructor of a class derived from <see cref="DbConfiguration" /> to set
        ///     an implementation of <see cref="IViewAssemblyCache" /> which will be used to find and cache the list
        ///     of assemblies that contain pre-generated views.
        /// </summary>
        /// <remarks>
        ///     This method is provided as a convenient and discoverable way to add configuration to the Entity Framework.
        ///     Internally it works in the same way as using AddDependencyResolver to add an appropriate resolver for
        ///     <see cref="IViewAssemblyCache" />. This means that, if desired, the same functionality can be achieved using
        ///     a custom resolver or a resolver backed by an Inversion-of-Control container.
        /// </remarks>
        /// <param name="cache"> The cache implementation to use. </param>
        protected internal void SetViewAssemblyCache(IViewAssemblyCache cache)
        {
            Check.NotNull(cache, "cache");

            _internalConfiguration.CheckNotLocked("SetViewAssemblyCache");
            _internalConfiguration.RegisterSingleton(cache, null);
        }

        internal virtual InternalConfiguration InternalConfiguration
        {
            get { return _internalConfiguration; }
        }
    }
}
