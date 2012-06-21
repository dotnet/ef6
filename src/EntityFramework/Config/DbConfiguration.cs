namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Diagnostics.Contracts;

    // TODO: Thread safety
    public class DbConfiguration
    {
        private readonly CompositeResolver<ResolverChain, ResolverChain> _resolvers
            = new CompositeResolver<ResolverChain, ResolverChain>(new ResolverChain(), new ResolverChain());
        
        private bool _isLocked;

        protected internal DbConfiguration()
            : this(new AppConfigDependencyResolver(AppConfig.DefaultInstance), new RootDependencyResolver())
        {
        }

        internal DbConfiguration(IDbDependencyResolver appConfigResolver, IDbDependencyResolver rootResolver)
        {
            _resolvers.First.Add(appConfigResolver);
            _resolvers.Second.Add(rootResolver);
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

        internal void Lock()
        {
            _isLocked = true;
        }

        internal void AddAppConfigResolver(IDbDependencyResolver resolver)
        {
            Contract.Requires(resolver != null);
            CheckNotLocked();

            _resolvers.First.Add(resolver);
        }

        protected void AddDependencyResolver(IDbDependencyResolver resolver)
        {
            Contract.Requires(resolver != null);
            CheckNotLocked();

            // New resolvers always run after the config resolvers so that config always wins over code
            _resolvers.Second.Add(resolver);
        }

        [CLSCompliant(false)]
        protected void AddEntityFrameworkProvider(string providerInvariantName, DbProviderServices provider)
        {
            CheckNotLocked();

            AddDependencyResolver(new SingletonDependencyResolver<DbProviderServices>(provider, providerInvariantName));
        }

        [CLSCompliant(false)]
        public DbProviderServices GetEntityFrameworkProvider(string providerInvariantName)
        {
            // TODO: use generic version of Get
            return (DbProviderServices)_resolvers.Get(typeof(DbProviderServices), providerInvariantName);
        }

        protected void SetDatabaseInitializer<TContext>(IDatabaseInitializer<TContext> strategy) where TContext : DbContext
        {
            CheckNotLocked();

            AddDependencyResolver(new SingletonDependencyResolver<IDatabaseInitializer<TContext>>(strategy));
        }

        public IDatabaseInitializer<TContext> GetDatabaseInitializer<TContext>() where TContext : DbContext
        {
            // TODO: Make sure that access to the database initializer now uses this method
            // TODO Check how current contextinfo interacts with initializer (don't think it does)
            return (IDatabaseInitializer<TContext>)_resolvers.Get(typeof(IDatabaseInitializer<TContext>), null);
        }

        public void SetDefaultConnectionFactory(IDbConnectionFactory value)
        {
            CheckNotLocked();

            AddDependencyResolver(new SingletonDependencyResolver<IDbConnectionFactory>(value));
        }

        public IDbConnectionFactory GetDefaultConnectionFactory()
        {
            return Database.DefaultConnectionFactoryChanged
#pragma warning disable 612,618
                       ? Database.DefaultConnectionFactory
#pragma warning restore 612,618
                       : (IDbConnectionFactory)_resolvers.Get(typeof(IDbConnectionFactory), null);
        }

        private void CheckNotLocked()
        {
            if (_isLocked)
            {
                throw new InvalidOperationException("Configuration can only be changed before the configuration is used. Try setting configuration in the constructor of your DbConfiguration class.");
            }
        }
    }
}