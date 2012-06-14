namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;

    internal class ConfigDependencyResolver : IDbDependencyResolver
    {
        private readonly AppConfig _appConfig;

        public ConfigDependencyResolver(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        public virtual object Get(Type type, string name)
        {
            // TODO: Implement the rest of the resolution (including DatabaseInitializer)

            if (type == typeof(DbProviderServices))
            {
                return _appConfig.Providers.GetDbProviderServices(name);
            }

            if (type == typeof(IDbConnectionFactory))
            {
                return _appConfig.DefaultConnectionFactory;
            }

            return null;
        }

        public virtual void Release(object service)
        {
        }
    }
}