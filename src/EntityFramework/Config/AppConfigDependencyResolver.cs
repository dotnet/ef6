// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections.Concurrent;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Resolves dependencies from a config file.
    /// </summary>
    internal class AppConfigDependencyResolver : IDbDependencyResolver
    {
        private readonly AppConfig _appConfig;

        private readonly ConcurrentDictionary<Tuple<Type, object>, Func<object>> _serviceFactories
            = new ConcurrentDictionary<Tuple<Type, object>, Func<object>>();

        public AppConfigDependencyResolver(AppConfig appConfig)
        {
            DebugCheck.NotNull(appConfig);

            _appConfig = appConfig;
        }

        public virtual object GetService(Type type, object key)
        {
            return _serviceFactories.GetOrAdd(
                Tuple.Create(type, key),
                t => GetServiceFactory(type, key as string))();
        }

        public virtual Func<object> GetServiceFactory(Type type, string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (type == typeof(DbProviderServices))
                {
                    var providerServices = _appConfig.Providers.TryGetDbProviderServices(name);
                    return () => providerServices;
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
    }
}
