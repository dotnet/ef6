// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.ConfigFile;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Resolves dependencies from a config file.
    /// </summary>
    internal class AppConfigDependencyResolver : IDbDependencyResolver
    {
        private readonly AppConfig _appConfig;
        private readonly InternalConfiguration _internalConfiguration;

        private readonly ConcurrentDictionary<Tuple<Type, object>, Func<object>> _serviceFactories
            = new ConcurrentDictionary<Tuple<Type, object>, Func<object>>();

        private readonly Dictionary<string, DbProviderServices> _providerFactories
            = new Dictionary<string, DbProviderServices>();

        private bool _providersLoaded;

        private readonly ProviderServicesFactory _providerServicesFactory;

        /// <summary>
        ///     For testing.
        /// </summary>
        public AppConfigDependencyResolver()
        {
        }

        public AppConfigDependencyResolver(
            AppConfig appConfig, 
            InternalConfiguration internalConfiguration, 
            ProviderServicesFactory providerServicesFactory = null)
        {
            DebugCheck.NotNull(appConfig);

            _appConfig = appConfig;
            _internalConfiguration = internalConfiguration;
            _providerServicesFactory = providerServicesFactory ?? new ProviderServicesFactory();
        }

        public virtual object GetService(Type type, object key)
        {
            return _serviceFactories.GetOrAdd(
                Tuple.Create(type, key),
                t => GetServiceFactory(type, key as string))();
        }

        public virtual Func<object> GetServiceFactory(Type type, string name)
        {
            if (!_providersLoaded)
            {
                lock (_providerFactories)
                {
                    if (!_providersLoaded)
                    {
                        LoadAllDbProviderServices();
                        _providersLoaded = true;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                if (type == typeof(DbProviderServices))
                {
                    DbProviderServices providerFactory;
                    _providerFactories.TryGetValue(name, out providerFactory);
                    return () => providerFactory;
                }

                if (type == typeof(MigrationSqlGenerator))
                {
                    return _appConfig.Providers.TryGetMigrationSqlGeneratorFactory(name);
                }
            }

            if (type == typeof(IDbConnectionFactory))
            {
                // This is convoluted to avoid breaking changes from EF5. The behavior is:
                // 1. If the app has already set the Database.DefaultConnectionFactory property, then
                //    whatever it is set to should be returned.
                // 2. If not, but an connection factory was set in app.config, then set the
                //    DefaultConnectionFactory property to the one from the app.config so that in
                //    the future it will always be used, unless...
                // 3. The app later changes the DefaultConnectionFactory property in which case
                //    the later one will be used instead of the one from app.config
                // Note that this means that the app.config and DefaultConnectionFactory will override
                // any other resolver in the chain (since this class is at the top of the chain)
                // unless IDbConfiguration was used to add an overriding resolver.
                if (!Database.DefaultConnectionFactoryChanged)
                {
                    var connectionFactory = _appConfig.TryGetDefaultConnectionFactory();
                    if (connectionFactory != null)
                    {
#pragma warning disable 612,618
                        Database.DefaultConnectionFactory = connectionFactory;
#pragma warning restore 612,618
                    }
                }

                return () => Database.DefaultConnectionFactoryChanged ? Database.SetDefaultConnectionFactory : null;
            }

            var contextType = type.TryGetElementType(typeof(IDatabaseInitializer<>));
            if (contextType != null)
            {
                var initializer = _appConfig.Initializers.TryGetInitializer(contextType);
                return () => initializer;
            }

            if (type == typeof(DbSpatialServices))
            {
                var connectionFactory = _appConfig.Providers.TryGetSpatialProvider();
                return () => connectionFactory;
            }

            return () => null;
        }

        private void LoadAllDbProviderServices()
        {
            var providers = _appConfig.Providers.GetAllDbProviderServices().ToList();

            if (providers.All(p => p.InvariantName != "System.Data.SqlClient"))
            {
                // If no SQL Server provider is registered, then make sure the SQL Server provider is available
                // by convention (if it can be loaded) as it would have been in previous versions of EF.
                RegisterSqlServerProvider();
            }

            var defaultName = _appConfig.Providers.DefaultInvariantName;
            if (defaultName != null && providers.All(p => p.InvariantName != defaultName))
            {
                throw new InvalidOperationException(Strings.EF6Providers_DefaultNotFound(defaultName));
            }

            providers.Where(p => p.InvariantName != defaultName).Each(RegisterProvider);

            // Make sure default is added last so it will resolve dependencies before others.
            providers.Where(p => p.InvariantName == defaultName).Each(RegisterProvider);
        }

        private void RegisterProvider(ProviderElement providerElement)
        {
            DebugCheck.NotNull(providerElement);

            var provider = _providerServicesFactory.GetInstance(providerElement.ProviderTypeName, providerElement.InvariantName);
            _providerFactories[providerElement.InvariantName] = provider;
            _internalConfiguration.AddSecondaryResolver(provider);
        }

        private void RegisterSqlServerProvider()
        {
            var provider = _providerServicesFactory
                .TryGetInstance("System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer");

            if (provider != null)
            {
                // This provider goes just above the root resolver so that any other provider registered in code
                // still takes precedence.
                _internalConfiguration.AddSecondaryResolver(
                    new SingletonDependencyResolver<DbProviderServices>(provider, "System.Data.SqlClient"));
                _internalConfiguration.AddSecondaryResolver(provider);
            }
        }
    }
}
