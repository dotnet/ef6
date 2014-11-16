// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Configuration;
    using System.Data.Common;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
#if !NET40
    using System.Runtime.ExceptionServices;
#endif

    /// <summary>
    /// Provides runtime information about a given <see cref="DbContext" /> type.
    /// </summary>
    public class DbContextInfo
    {
        [ThreadStatic]
        private static DbContextInfo _currentInfo;

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
        private readonly Func<IDbDependencyResolver> _resolver = () => DbConfiguration.DependencyResolver;

        private Action<DbModelBuilder> _onModelCreating;

        /// <summary>
        /// Creates a new instance representing a given <see cref="DbContext" /> type.
        /// </summary>
        /// <param name="contextType">
        /// The type deriving from <see cref="DbContext" />.
        /// </param>
        public DbContextInfo(Type contextType)
            : this(contextType, (Func<IDbDependencyResolver>)null)
        {
        }

        internal DbContextInfo(Type contextType, Func<IDbDependencyResolver> resolver)
            : this(Check.NotNull(contextType, "contextType"), null, AppConfig.DefaultInstance, null, resolver)
        {
        }

        /// <summary>
        /// Creates a new instance representing a given <see cref="DbContext" /> targeting a specific database.
        /// </summary>
        /// <param name="contextType">
        /// The type deriving from <see cref="DbContext" />.
        /// </param>
        /// <param name="connectionInfo"> Connection information for the database to be used. </param>
        public DbContextInfo(Type contextType, DbConnectionInfo connectionInfo)
            : this(
                Check.NotNull(contextType, "contextType"), null, AppConfig.DefaultInstance, Check.NotNull(connectionInfo, "connectionInfo"))
        {
        }

        /// <summary>
        /// Creates a new instance representing a given <see cref="DbContext" /> type. An external list of
        /// connection strings can be supplied and will be used during connection string resolution in place
        /// of any connection strings specified in external configuration files.
        /// </summary>
        /// <remarks>
        /// It is preferable to use the constructor that accepts the entire config document instead of using this
        /// constructor. Providing the entire config document allows DefaultConnectionFactroy entries in the config
        /// to be found in addition to explicitly specified connection strings.
        /// </remarks>
        /// <param name="contextType">
        /// The type deriving from <see cref="DbContext" />.
        /// </param>
        /// <param name="connectionStringSettings"> A collection of connection strings. </param>
        [Obsolete(
            @"The application configuration can contain multiple settings that affect the connection used by a DbContext. To ensure all configuration is taken into account, use a DbContextInfo constructor that accepts System.Configuration.Configuration"
            )]
        public DbContextInfo(Type contextType, ConnectionStringSettingsCollection connectionStringSettings)
            : this(
                Check.NotNull(contextType, "contextType"), null,
                new AppConfig(Check.NotNull(connectionStringSettings, "connectionStringSettings")), null)
        {
        }

        /// <summary>
        /// Creates a new instance representing a given <see cref="DbContext" /> type. An external config
        /// object (e.g. app.config or web.config) can be supplied and will be used during connection string
        /// resolution. This includes looking for connection strings and DefaultConnectionFactory entries.
        /// </summary>
        /// <param name="contextType">
        /// The type deriving from <see cref="DbContext" />.
        /// </param>
        /// <param name="config"> An object representing the config file. </param>
        public DbContextInfo(Type contextType, Configuration config)
            : this(Check.NotNull(contextType, "contextType"), null, new AppConfig(Check.NotNull(config, "config")), null)
        {
        }

        /// <summary>
        /// Creates a new instance representing a given <see cref="DbContext" />, targeting a specific database.
        /// An external config object (e.g. app.config or web.config) can be supplied and will be used during connection string
        /// resolution. This includes looking for connection strings and DefaultConnectionFactory entries.
        /// </summary>
        /// <param name="contextType">
        /// The type deriving from <see cref="DbContext" />.
        /// </param>
        /// <param name="config"> An object representing the config file. </param>
        /// <param name="connectionInfo"> Connection information for the database to be used. </param>
        public DbContextInfo(Type contextType, Configuration config, DbConnectionInfo connectionInfo)
            : this(
                Check.NotNull(contextType, "contextType"), null, new AppConfig(Check.NotNull(config, "config")),
                Check.NotNull(connectionInfo, "connectionInfo"))
        {
        }

        /// <summary>
        /// Creates a new instance representing a given <see cref="DbContext" /> type.  A <see cref="DbProviderInfo" />
        /// can be supplied in order to override the default determined provider used when constructing
        /// the underlying EDM model.
        /// </summary>
        /// <param name="contextType">
        /// The type deriving from <see cref="DbContext" />.
        /// </param>
        /// <param name="modelProviderInfo">
        /// A <see cref="DbProviderInfo" /> specifying the underlying ADO.NET provider to target.
        /// </param>
        public DbContextInfo(Type contextType, DbProviderInfo modelProviderInfo)
            : this(
                Check.NotNull(contextType, "contextType"), Check.NotNull(modelProviderInfo, "modelProviderInfo"), AppConfig.DefaultInstance,
                null)
        {
        }

        /// <summary>
        /// Creates a new instance representing a given <see cref="DbContext" /> type. An external config
        /// object (e.g. app.config or web.config) can be supplied and will be used during connection string
        /// resolution. This includes looking for connection strings and DefaultConnectionFactory entries.
        /// A <see cref="DbProviderInfo" /> can be supplied in order to override the default determined
        /// provider used when constructing the underlying EDM model. This can be useful to prevent EF from
        /// connecting to discover a manifest token.
        /// </summary>
        /// <param name="contextType">
        /// The type deriving from <see cref="DbContext" />.
        /// </param>
        /// <param name="config"> An object representing the config file. </param>
        /// <param name="modelProviderInfo">
        /// A <see cref="DbProviderInfo" /> specifying the underlying ADO.NET provider to target.
        /// </param>
        public DbContextInfo(Type contextType, Configuration config, DbProviderInfo modelProviderInfo)
            : this(
                Check.NotNull(contextType, "contextType"), Check.NotNull(modelProviderInfo, "modelProviderInfo"),
                new AppConfig(Check.NotNull(config, "config")), null)
        {
        }

        // <summary>
        // Called internally when a context info is needed for an existing context, which may not be constructable.
        // </summary>
        // <param name="context"> The context instance to get info from. </param>
        internal DbContextInfo(DbContext context, Func<IDbDependencyResolver> resolver = null)
        {
            Check.NotNull(context, "context");

            _resolver = resolver ?? (() => DbConfiguration.DependencyResolver);

            _contextType = context.GetType();
            _appConfig = AppConfig.DefaultInstance;

            var internalContext = context.InternalContext;
            _connectionProviderName = internalContext.ProviderName;

            _connectionInfo = new DbConnectionInfo(internalContext.OriginalConnectionString, _connectionProviderName);

            _connectionString = internalContext.OriginalConnectionString;
            _connectionStringName = internalContext.ConnectionStringName;
            _connectionStringOrigin = internalContext.ConnectionStringOrigin;
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        private DbContextInfo(
            Type contextType,
            DbProviderInfo modelProviderInfo,
            AppConfig config,
            DbConnectionInfo connectionInfo,
            Func<IDbDependencyResolver> resolver = null)
        {
            if (!typeof(DbContext).IsAssignableFrom(contextType))
            {
                throw new ArgumentOutOfRangeException("contextType");
            }

            _resolver = resolver ?? (() => DbConfiguration.DependencyResolver);

            _contextType = contextType;
            _modelProviderInfo = modelProviderInfo;
            _appConfig = config;
            _connectionInfo = connectionInfo;

            _activator = CreateActivator();

            if (_activator != null)
            {
                var context = CreateInstance();

                if (context != null)
                {
                    _isConstructible = true;

                    using (context)
                    {
                        _connectionString = 
                            DbInterception.Dispatch.Connection.GetConnectionString(
                            context.InternalContext.Connection,
                            new DbInterceptionContext().WithDbContext(context));
                        _connectionStringName = context.InternalContext.ConnectionStringName;
                        _connectionProviderName = context.InternalContext.ProviderName;
                        _connectionStringOrigin = context.InternalContext.ConnectionStringOrigin;
                    }
                }
            }
        }

        /// <summary>
        /// The concrete <see cref="DbContext" /> type.
        /// </summary>
        public virtual Type ContextType
        {
            get { return _contextType; }
        }

        /// <summary>
        /// Whether or not instances of the underlying <see cref="DbContext" /> type can be created.
        /// </summary>
        public virtual bool IsConstructible
        {
            get { return _isConstructible; }
        }

        /// <summary>
        /// The connection string used by the underlying <see cref="DbContext" /> type.
        /// </summary>
        public virtual string ConnectionString
        {
            get { return _connectionString; }
        }

        /// <summary>
        /// The connection string name used by the underlying <see cref="DbContext" /> type.
        /// </summary>
        public virtual string ConnectionStringName
        {
            get { return _connectionStringName; }
        }

        /// <summary>
        /// The ADO.NET provider name of the connection used by the underlying <see cref="DbContext" /> type.
        /// </summary>
        public virtual string ConnectionProviderName
        {
            get { return _connectionProviderName; }
        }

        /// <summary>
        /// The origin of the connection string used by the underlying <see cref="DbContext" /> type.
        /// </summary>
        public virtual DbConnectionStringOrigin ConnectionStringOrigin
        {
            get { return _connectionStringOrigin; }
        }

        /// <summary>
        /// An action to be run on the DbModelBuilder after OnModelCreating has been run on the context.
        /// </summary>
        public virtual Action<DbModelBuilder> OnModelCreating
        {
            get { return _onModelCreating; }
            set { _onModelCreating = value; }
        }

        /// <summary>
        /// If instances of the underlying <see cref="DbContext" /> type can be created, returns
        /// a new instance; otherwise returns null.
        /// </summary>
        /// <returns>
        /// A <see cref="DbContext" /> instance.
        /// </returns>
        public virtual DbContext CreateInstance()
        {
            var configPushed = DbConfigurationManager.Instance.PushConfiguration(_appConfig, _contextType);
            CurrentInfo = this;

            DbContext context = null;
            try
            {
                try
                {
                    context = _activator == null ? null : _activator();
                }
                catch (TargetInvocationException ex)
                {
                    Debug.Assert(ex.InnerException != null);
#if !NET40
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
#endif
                    throw ex.InnerException;
                }

                if (context == null)
                {
                    return null;
                }

                context.InternalContext.OnDisposing += (_, __) => CurrentInfo = null;

                if (configPushed)
                {
                    context.InternalContext.OnDisposing +=
                        (_, __) => DbConfigurationManager.Instance.PopConfiguration(_appConfig);
                }

                context.InternalContext.ApplyContextInfo(this);

                return context;
            }
            catch (Exception)
            {
                if (context != null)
                {
                    context.Dispose();
                }

                throw;
            }
            finally
            {
                if (context == null)
                {
                    CurrentInfo = null;

                    if (configPushed)
                    {
                        DbConfigurationManager.Instance.PopConfiguration(_appConfig);
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal void ConfigureContext(DbContext context)
        {
            DebugCheck.NotNull(context);

            if (_modelProviderInfo != null)
            {
                context.InternalContext.ModelProviderInfo = _modelProviderInfo;
            }

            context.InternalContext.AppConfig = _appConfig;

            if (_connectionInfo != null)
            {
                context.InternalContext.OverrideConnection(new LazyInternalConnection(context, _connectionInfo));
            }
            else if (_modelProviderInfo != null
                     && _appConfig == AppConfig.DefaultInstance)
            {
                context.InternalContext.OverrideConnection(
                    new EagerInternalConnection(
                        context,
                        _resolver().GetService<DbProviderFactory>(
                            _modelProviderInfo.ProviderInvariantName).CreateConnection(), connectionOwned: true));
            }

            if (_onModelCreating != null)
            {
                context.InternalContext.OnModelCreating = _onModelCreating;
            }
        }

        private Func<DbContext> CreateActivator()
        {
            var constructor = _contextType.GetPublicConstructor();

            if (constructor != null)
            {
                return () => (DbContext)Activator.CreateInstance(_contextType);
            }

            var resolvedFactory = _resolver().GetService<Func<DbContext>>(_contextType);

            if (resolvedFactory != null)
            {
                return resolvedFactory;
            }

            var factoryType
                = (from t in _contextType.Assembly().GetAccessibleTypes()
                   where t.IsClass() && typeof(IDbContextFactory<>).MakeGenericType(_contextType).IsAssignableFrom(t)
                   select t).FirstOrDefault();

            if (factoryType == null)
            {
                return null;
            }

            if (factoryType.GetPublicConstructor() == null)
            {
                throw Error.DbContextServices_MissingDefaultCtor(factoryType);
            }

            return ((IDbContextFactory<DbContext>)Activator.CreateInstance(factoryType)).Create;
        }

        internal static DbContextInfo CurrentInfo
        {
            get { return _currentInfo; }
            set { _currentInfo = value; }
        }
    }
}
