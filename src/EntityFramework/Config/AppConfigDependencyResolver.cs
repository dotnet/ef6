namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.Contracts;

    // TODO: Consider thread safety
    // TODO: Consider caching for perf
    /// <summary>
    /// Resolves dependencies from a config file.
    /// </summary>
    internal class AppConfigDependencyResolver : IDbDependencyResolver
    {
        private readonly AppConfig _appConfig;

        public AppConfigDependencyResolver(AppConfig appConfig)
        {
            Contract.Requires(appConfig != null);

            _appConfig = appConfig;
        }

        public virtual object GetService(Type type, string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (type == typeof(DbProviderServices))
                {
                    return _appConfig.Providers.TryGetDbProviderServices(name);
                }

                if (type == typeof(MigrationSqlGenerator))
                {
                    return _appConfig.Providers.TryGetMigrationSqlGenerator(name);
                }
            }

            if (type == typeof(IDbConnectionFactory))
            {
                return _appConfig.TryGetDefaultConnectionFactory();
            }

            // TODO: Implement for IDatabaseInitializer

            return null;
        }

        public virtual void Release(object service)
        {
        }
    }
}
