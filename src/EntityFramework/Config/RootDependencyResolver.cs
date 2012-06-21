namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;

    // TODO: Consider thread safety
    // TODO: Consider caching for perf
    internal class RootDependencyResolver : IDbDependencyResolver
    {
        public virtual object Get(Type type, string name)
        {
            if (type == typeof(DbProviderServices))
            {
                return new ProviderServicesFactory().GetInstanceByConvention(name);
            }

            if (type == typeof(IDbConnectionFactory))
            {
                return new SqlConnectionFactory();
            }

            // TODO: Implement for IDatabaseInitializer

            return null;
        }

        public virtual void Release(object service)
        {
        }
    }
}