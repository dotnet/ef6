namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;

    // TODO: Consider thread safety
    // TODO: Consider caching for perf
    internal class AppConfigDependencyResolver : IDbDependencyResolver
    {
        private readonly AppConfig _appConfig;

        public AppConfigDependencyResolver(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        public virtual object Get(Type type, string name)
        {
            if (type == typeof(DbProviderServices) && !string.IsNullOrWhiteSpace(name))
            {
                var providerTypeName = _appConfig.Providers.TryGetDbProviderServicesTypeName(name);
                if (providerTypeName != null)
                {
                    return new ProviderServicesFactory().GetInstance(providerTypeName, name);
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