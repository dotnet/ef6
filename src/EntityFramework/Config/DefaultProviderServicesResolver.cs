namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Resources;

    internal class DefaultProviderServicesResolver : IDbDependencyResolver
    {
        public virtual object GetService(Type type, string name)
        {
            if (type == typeof(DbProviderServices))
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException(Strings.ProviderInvariantNotPassedToResolver);
                }

                return new ProviderServicesFactory().GetInstanceByConvention(name);
            }

            return null;
        }

        public virtual void Release(object service)
        {
        }
    }
}
