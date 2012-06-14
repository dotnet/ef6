namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Diagnostics.Contracts;

    public class DbConfiguration
    {
        private readonly ResolverChain _resolverChain = new ResolverChain();

        protected internal DbConfiguration()
            : this(
                new RootDependencyResolver(), 
                new ConfigDependencyResolver(AppConfig.DefaultInstance))
        {
        }

        internal DbConfiguration(IDbDependencyResolver rootResolver, IDbDependencyResolver defaultResolver)
        {
            _resolverChain.Add(rootResolver);
            _resolverChain.Add(defaultResolver);
        }

        public static DbConfiguration Instance
        {
            get { return DbConfigurationManager.Instance.GetConfiguration(); }
            set
            {
                Contract.Requires(value != null);

                DbConfigurationManager.Instance.SetConfiguration(value);
            }
        }

        public virtual void AddDependencyResolver(IDbDependencyResolver resolver)
        {
            Contract.Requires(resolver != null);

            _resolverChain.Add(resolver);
        }

        public virtual bool RemoveDependencyResolver(IDbDependencyResolver resolver)
        {
            Contract.Requires(resolver != null);

            return _resolverChain.Remove(resolver);
        }

        [CLSCompliant(false)]
        public virtual void AddEntityFrameworkProvider(string providerInvariantName, DbProviderServices provider)
        {
            AddDependencyResolver(new SingletonDependencyResolver<DbProviderServices>(provider, providerInvariantName));
        }

        [CLSCompliant(false)]
        public virtual DbProviderServices GetEntityFrameworkProvider(string providerInvariantName)
        {
            // TODO: Make sure that places that new the provider get it from here
            return (DbProviderServices)_resolverChain.Get(typeof(DbProviderServices), providerInvariantName);
        }

        public virtual void SetDatabaseInitializer<TContext>(IDatabaseInitializer<TContext> strategy) where TContext : DbContext
        {
            AddDependencyResolver(new SingletonDependencyResolver<IDatabaseInitializer<TContext>>(strategy));
        }

        public virtual IDatabaseInitializer<TContext> GetDatabaseInitializer<TContext>() where TContext : DbContext
        {
            // TODO: Make sure that access to the database initializer now uses this method
            // TODO Check how current contextinfo interacts with initializer (don't think it does)
            return (IDatabaseInitializer<TContext>)_resolverChain.Get(typeof(IDatabaseInitializer<TContext>), null);
        }

        public virtual IDbConnectionFactory DefaultConnectionFactory
        {
            get
            {
                // TODO: Make setting with old method obsolete
                return Database.DefaultConnectionFactoryChanged
                           ? Database.DefaultConnectionFactory
                           : (IDbConnectionFactory)_resolverChain.Get(typeof(IDbConnectionFactory), null);
            }
            set
            {
                AddDependencyResolver(new SingletonDependencyResolver<IDbConnectionFactory>(value));
            }
        }
    }
}