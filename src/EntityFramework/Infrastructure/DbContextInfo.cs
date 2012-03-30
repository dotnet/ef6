namespace System.Data.Entity.Infrastructure
{
    using System.Configuration;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     Provides runtime information about a given <see cref = "DbContext" /> type.
    /// </summary>
    public class DbContextInfo
    {
        private readonly Type _contextType;
        private readonly DbProviderInfo _modelProviderInfo;
        private readonly DbConnectionInfo _connectionInfo;
        private readonly AppConfig _appConfig;
        private readonly Func<DbContext> _activator;
        private readonly string _connectionString;
        private readonly string _connectionProviderName;
        private readonly bool _isConstructible;
        private readonly DbConnectionStringOrigin _connectionStringOrigin;
        private readonly string _connectionStringName;

        private Action<DbModelBuilder> _onModelCreating;

        /// <summary>
        ///     Creates a new instance representing a given <see cref = "DbContext" /> type.
        /// </summary>
        /// <param name = "contextType">The type deriving from <see cref = "DbContext" />.</param>
        public DbContextInfo(Type contextType)
            : this(contextType, null, AppConfig.DefaultInstance, null)
        {
            Contract.Requires(contextType != null);
        }

        /// <summary>
        ///     Creates a new instance representing a given <see cref = "DbContext" /> targeting a specific database.
        /// </summary>
        /// <param name = "contextType">The type deriving from <see cref = "DbContext" />.</param>
        /// <param name="connectionInfo">Connection information for the database to be used.</param>
        public DbContextInfo(Type contextType, DbConnectionInfo connectionInfo)
            : this(contextType, null, AppConfig.DefaultInstance, connectionInfo)
        {
            Contract.Requires(contextType != null);
            Contract.Requires(connectionInfo != null);
        }

        /// <summary>
        ///     Creates a new instance representing a given <see cref = "DbContext" /> type. An external list of 
        ///     connection strings can be supplied and will be used during connection string resolution in place
        ///     of any connection strings specified in external configuration files.
        /// </summary>
        /// <remarks>
        ///     It is preferable to use the constructor that accepts the entire config document instead of using this
        ///     constructor. Providing the entire config document allows DefaultConnectionFactroy entries in the config
        ///     to be found in addition to explicitly specified connection strings.
        /// </remarks>
        /// <param name = "contextType">The type deriving from <see cref = "DbContext" />.</param>
        /// <param name = "connectionStringSettings">A collection of connection strings.</param>
        [Obsolete(
            @"The application configuration can contain multiple settings that affect the connection used by a DbContext. To ensure all configuration is taken into account, use a DbContextInfo constructor that accepts System.Configuration.Configuration"
            )]
        public DbContextInfo(Type contextType, ConnectionStringSettingsCollection connectionStringSettings)
            : this(contextType, null, new AppConfig(connectionStringSettings), null)
        {
            Contract.Requires(contextType != null);
            Contract.Requires(connectionStringSettings != null);
        }

        /// <summary>
        ///     Creates a new instance representing a given <see cref = "DbContext" /> type. An external config 
        ///     object (e.g. app.config or web.config) can be supplied and will be used during connection string
        ///     resolution. This includes looking for connection strings and DefaultConnectionFactory entries.
        /// </summary>
        /// <param name = "contextType">The type deriving from <see cref = "DbContext" />.</param>
        /// <param name = "config">An object representing the config file.</param>
        public DbContextInfo(Type contextType, Configuration config)
            : this(contextType, null, new AppConfig(config), null)
        {
            Contract.Requires(contextType != null);
            Contract.Requires(config != null);
        }

        /// <summary>
        ///     Creates a new instance representing a given <see cref = "DbContext" />, targeting a specific database.
        ///     An external config object (e.g. app.config or web.config) can be supplied and will be used during connection string
        ///     resolution. This includes looking for connection strings and DefaultConnectionFactory entries.
        /// </summary>
        /// <param name = "contextType">The type deriving from <see cref = "DbContext" />.</param>
        /// <param name = "config">An object representing the config file.</param>
        /// <param name="connectionInfo">Connection information for the database to be used.</param>
        public DbContextInfo(Type contextType, Configuration config, DbConnectionInfo connectionInfo)
            : this(contextType, null, new AppConfig(config), connectionInfo)
        {
            Contract.Requires(contextType != null);
            Contract.Requires(config != null);
            Contract.Requires(connectionInfo != null);
        }

        /// <summary>
        ///     Creates a new instance representing a given <see cref = "DbContext" /> type.  A <see cref = "DbProviderInfo" />
        ///     can be supplied in order to override the default determined provider used when constructing
        ///     the underlying EDM model.
        /// </summary>
        /// <param name = "contextType">The type deriving from <see cref = "DbContext" />.</param>
        /// <param name = "modelProviderInfo">A <see cref = "DbProviderInfo" /> specifying the underlying ADO.NET provider to target.</param>
        public DbContextInfo(Type contextType, DbProviderInfo modelProviderInfo)
            : this(contextType, modelProviderInfo, AppConfig.DefaultInstance, null)
        {
            Contract.Requires(contextType != null);
            Contract.Requires(modelProviderInfo != null);
        }

        /// <summary>
        /// Called internally when a context info is needed for an existing context, which may not be constructable.
        /// </summary>
        /// <param name="context">The context instance to get info from.</param>
        internal DbContextInfo(DbContext context)
        {
            Contract.Requires(context != null);

            _contextType = context.GetType();
            _appConfig = AppConfig.DefaultInstance;

            var internalContext = context.InternalContext;
            _connectionProviderName = internalContext.ProviderName;

            _connectionInfo = new DbConnectionInfo(internalContext.OriginalConnectionString, _connectionProviderName);

            _connectionString = internalContext.OriginalConnectionString;
            _connectionStringName = internalContext.ConnectionStringName;
            _connectionStringOrigin = internalContext.ConnectionStringOrigin;
        }

        private DbContextInfo(
            Type contextType,
            DbProviderInfo modelProviderInfo,
            AppConfig config,
            DbConnectionInfo connectionInfo)
        {
            if (!typeof(DbContext).IsAssignableFrom(contextType))
            {
                throw new ArgumentOutOfRangeException("contextType");
            }

            _contextType = contextType;
            _modelProviderInfo = modelProviderInfo;
            _appConfig = config;
            _connectionInfo = connectionInfo;

            _activator = CreateActivator();

            if (_activator != null)
            {
                var context = _activator();

                if (context != null)
                {
                    _isConstructible = true;

                    using (context)
                    {
                        ConfigureContext(context);

                        _connectionString = context.InternalContext.Connection.ConnectionString;
                        _connectionStringName = context.InternalContext.ConnectionStringName;
                        _connectionProviderName = context.InternalContext.ProviderName;
                        _connectionStringOrigin = context.InternalContext.ConnectionStringOrigin;
                    }
                }
            }
        }

        /// <summary>
        ///     The concrete <see cref = "DbContext" /> type.
        /// </summary>
        public virtual Type ContextType
        {
            get { return _contextType; }
        }

        /// <summary>
        ///     Whether or not instances of the underlying <see cref = "DbContext" /> type can be created.
        /// </summary>
        public virtual bool IsConstructible
        {
            get { return _isConstructible; }
        }

        /// <summary>
        ///     The connection string used by the underlying <see cref = "DbContext" /> type.
        /// </summary>
        public virtual string ConnectionString
        {
            get { return _connectionString; }
        }

        /// <summary>
        ///     The connection string name used by the underlying <see cref = "DbContext" /> type.
        /// </summary>
        public virtual string ConnectionStringName
        {
            get { return _connectionStringName; }
        }

        /// <summary>
        ///     The ADO.NET provider name of the connection used by the underlying <see cref = "DbContext" /> type.
        /// </summary>
        public virtual string ConnectionProviderName
        {
            get { return _connectionProviderName; }
        }

        /// <summary>
        ///     The origin of the connection string used by the underlying <see cref = "DbContext" /> type.
        /// </summary>
        public virtual DbConnectionStringOrigin ConnectionStringOrigin
        {
            get { return _connectionStringOrigin; }
        }

        /// <summary>
        ///     An action to be run on the DbModelBuilder after OnModelCreating has been run on the context.
        /// </summary>
        public virtual Action<DbModelBuilder> OnModelCreating
        {
            get { return _onModelCreating; }
            set { _onModelCreating = value; }
        }

        /// <summary>
        ///     If instances of the underlying <see cref = "DbContext" /> type can be created, returns
        ///     a new instance; otherwise returns null.
        /// </summary>
        /// <returns>A <see cref = "DbContext" /> instance.</returns>
        public virtual DbContext CreateInstance()
        {
            if (!IsConstructible)
            {
                return null;
            }

            var context = _activator();

            ConfigureContext(context);

            return context;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void ConfigureContext(DbContext context)
        {
            Contract.Requires(context != null);

            if (_modelProviderInfo != null)
            {
                context.InternalContext.ModelProviderInfo = _modelProviderInfo;
            }

            context.InternalContext.AppConfig = _appConfig;

            if (_connectionInfo != null)
            {
                context.InternalContext.OverrideConnection(new LazyInternalConnection(_connectionInfo));
            }

            if (_onModelCreating != null)
            {
                context.InternalContext.OnModelCreating = _onModelCreating;
            }
        }

        private Func<DbContext> CreateActivator()
        {
            var constructor = _contextType.GetConstructor(Type.EmptyTypes);

            if (constructor != null)
            {
                return () => (DbContext)Activator.CreateInstance(_contextType);
            }

            var factoryType
                = (from t in _contextType.Assembly.GetTypes()
                   where t.IsClass && typeof(IDbContextFactory<>).MakeGenericType(_contextType).IsAssignableFrom(t)
                   select t).FirstOrDefault();

            if (factoryType == null)
            {
                return null;
            }

            if (factoryType.GetConstructor(Type.EmptyTypes) == null)
            {
                throw Error.DbContextServices_MissingDefaultCtor(factoryType);
            }

            var factory = (IDbContextFactory<DbContext>)Activator.CreateInstance(factoryType);

            return factory.Create;
        }
    }
}
