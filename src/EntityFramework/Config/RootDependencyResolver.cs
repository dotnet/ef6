namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;

    internal class RootDependencyResolver : IDbDependencyResolver
    {
        public virtual object Get(Type type, string name)
        {
            if (type == typeof(DbProviderServices))
            {
                throw new Exception("Could not find EF provider.");
            }

            if (type == typeof(IDbConnectionFactory))
            {
                return Database.DefaultConnectionFactory;
            }

            // TODO Check for IDatabaseInitializer type and return initializer with Database.SetInitializer

            return null;
        }

        public virtual void Release(object service)
        {
        }
    }
}