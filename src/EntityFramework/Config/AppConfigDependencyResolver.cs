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
    using System.Diagnostics.Contracts;

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
            Contract.Requires(appConfig != null);

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
                var connectionFactory = _appConfig.TryGetDefaultConnectionFactory();
                return () => connectionFactory;
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
