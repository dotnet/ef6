namespace System.Data.Entity.Config
{
    using System.Collections.Concurrent;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.Sql;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Resolves dependencies from a config file.
    /// </summary>
    internal class AppConfigDependencyResolver : IDbDependencyResolver
    {
        private readonly AppConfig _appConfig;
        private readonly ConcurrentDictionary<Tuple<Type, string>, Func<object>> _serviceFactories
            = new ConcurrentDictionary<Tuple<Type, string>, Func<object>>();

        public AppConfigDependencyResolver(AppConfig appConfig)
        {
            Contract.Requires(appConfig != null);

            _appConfig = appConfig;
        }

        public virtual object GetService(Type type, string name)
        {
            return _serviceFactories.GetOrAdd(
                Tuple.Create(type, name), 
                t => GetServiceFactory(type, name))();
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

            // TODO: Implement for IDatabaseInitializer

            return () => null;
        }

        public virtual void Release(object service)
        {
        }
    }
}
