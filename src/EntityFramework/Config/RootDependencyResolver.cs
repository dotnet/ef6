namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;

    // TODO: Consider thread safety
    // TODO: Consider caching for perf
    /// <summary>
    /// This resolver is always the last resolver in the internal resolver chain and is
    /// responsible for providing the default service for each dependency or throwing an
    /// exception if there is no reasonable default service.
    /// </summary>
    internal class RootDependencyResolver : IDbDependencyResolver
    {
        /// <inheritdoc/>
        public virtual object Get(Type type, string name)
        {
            // TODO: Handle Database initializer

            if (type == typeof(DbProviderServices))
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException(Strings.ProviderInvariantNotPassedToResolver);
                }

                return new ProviderServicesFactory().GetInstanceByConvention(name);
            }

            if (type == typeof(IDbConnectionFactory))
            {
                return new SqlConnectionFactory();
            }

            if (type == typeof(IDbModelCacheKeyFactory))
            {
                return new DefaultModelCacheKeyFactory();
            }

            Contract.Assert(false, "End of resolver chain reached without resolving dependency.");

            return null;
        }

        /// <inheritdoc/>
        public virtual void Release(object service)
        {
        }
    }
}
