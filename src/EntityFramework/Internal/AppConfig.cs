namespace System.Data.Entity.Internal
{
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal.ConfigFile;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A simple representation of an app.config or web.config file.
    /// </summary>
    internal class AppConfig
    {
        private const string EFSectionName = "entityFramework";

        private static readonly AppConfig _defaultInstance = new AppConfig();
        private readonly KeyValueConfigurationCollection _appSettings;
        private readonly ConnectionStringSettingsCollection _connectionStrings;
        private readonly EntityFrameworkSection _entityFrameworkSettings;

        private readonly Lazy<IDbConnectionFactory> _defaultConnectionFactory;

        private readonly Lazy<IDbConnectionFactory> _defaultDefaultConnectionFactory =
            new Lazy<IDbConnectionFactory>(() => new SqlConnectionFactory(), isThreadSafe: true);

        private static bool _initializersApplied;
        private static readonly object Lock = new object();

        private static readonly MethodInfo Database_SetInitializerInternal =
            typeof(Database).GetMethod("SetInitializerInternal", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// Initializes a new instance of AppConfig based on supplied configuration
        /// </summary>
        /// <param name="configuration">Configuration to load settings from</param>
        public AppConfig(Configuration configuration)
            : this(
                configuration.ConnectionStrings.ConnectionStrings,
                configuration.AppSettings.Settings,
                (EntityFrameworkSection)configuration.GetSection(EFSectionName))
        {
            Contract.Requires(configuration != null);
        }

        /// <summary>
        /// Initializes a new instance of AppConfig based on supplied connection strings
        /// The default configuration for database initializers and default connection factory will be used
        /// </summary>
        /// <param name="connectionStrings">Connection strings to be used</param>
        public AppConfig(ConnectionStringSettingsCollection connectionStrings)
            : this(connectionStrings, null, null)
        {
            Contract.Requires(connectionStrings != null);
        }

        /// <summary>
        /// Initializes a new instance of AppConfig based on the <see cref="ConfigurationManager"/> for the AppDomain
        /// </summary>
        /// <remarks>
        /// Use AppConfig.DefaultInstance instead of this constructor
        /// </remarks>
        private AppConfig()
            : this(
                ConfigurationManager.ConnectionStrings,
                Convert(ConfigurationManager.AppSettings),
                (EntityFrameworkSection)ConfigurationManager.GetSection(EFSectionName))
        {
        }

        internal AppConfig(
            ConnectionStringSettingsCollection connectionStrings,
            KeyValueConfigurationCollection appSettings,
            EntityFrameworkSection entityFrameworkSettings)
        {
            Contract.Requires(connectionStrings != null);

            _connectionStrings = connectionStrings;
            _appSettings = appSettings;
            _entityFrameworkSettings = entityFrameworkSettings ?? new EntityFrameworkSection();

            if (_entityFrameworkSettings.DefaultConnectionFactory.ElementInformation.IsPresent)
            {
                _defaultConnectionFactory = new Lazy<IDbConnectionFactory>(
                    () =>
                        {
                            var setting = _entityFrameworkSettings.DefaultConnectionFactory;

                            try
                            {
                                var type = setting.GetFactoryType();
                                var args = setting.Parameters.GetTypedParameterValues();
                                return (IDbConnectionFactory)Activator.CreateInstance(type, args);
                            }
                            catch (Exception ex)
                            {
                                throw new InvalidOperationException(
                                    Strings.SetConnectionFactoryFromConfigFailed(setting.FactoryTypeName), ex);
                            }
                        }, isThreadSafe: true);
            }
            else
            {
                _defaultConnectionFactory = _defaultDefaultConnectionFactory;
            }
        }

        /// <summary>
        /// Gets the default connection factory based on the configuration
        /// </summary>
        public IDbConnectionFactory DefaultConnectionFactory
        {
            get { return _defaultConnectionFactory.Value; }
        }

        /// <summary>
        /// Appies any database intializers specified in the configuration
        /// </summary>
        public void ApplyInitializers()
        {
            InternalApplyInitializers(force: false);
        }

        /// <summary>
        /// Appies any database intializers specified in the configuration
        /// </summary>
        /// <param name="force">
        /// Value indicating if initializers should be re-applied if they have already been applied in this AppDomain
        /// </param>
        internal void InternalApplyInitializers(bool force)
        {
            if (!_initializersApplied || force)
            {
                lock (Lock)
                {
                    if (!_initializersApplied || force)
                    {
                        _initializersApplied = true; // Don't repeatedly try if an exception is thrown.

                        if (_appSettings != null)
                        {
                            LegacyDatabaseInitializerConfig.ApplyInitializersFromConfig(_appSettings);
                        }

                        if (_entityFrameworkSettings != null)
                        {
                            foreach (ContextElement init in _entityFrameworkSettings.Contexts)
                            {
                                if (init.IsDatabaseInitializationDisabled
                                    || init.DatabaseInitializer.ElementInformation.IsPresent)
                                {
                                    try
                                    {
                                        var contextType = init.GetContextType();
                                        object initializer = null;

                                        if (!init.IsDatabaseInitializationDisabled)
                                        {
                                            var initializerType = init.DatabaseInitializer.GetInitializerType();
                                            var args = init.DatabaseInitializer.Parameters.GetTypedParameterValues();
                                            initializer = Activator.CreateInstance(initializerType, args);
                                        }

                                        var setInitializerMethod =
                                            Database_SetInitializerInternal.MakeGenericMethod(contextType);
                                        setInitializerMethod.Invoke(
                                            null, BindingFlags.Static | BindingFlags.NonPublic, null,
                                            new[] { initializer, true }, null);
                                    }
                                    catch (Exception ex)
                                    {
                                        var initializerName = init.IsDatabaseInitializationDisabled
                                                                  ? "Disabled"
                                                                  : init.DatabaseInitializer.InitializerTypeName;

                                        throw new InvalidOperationException(
                                            Strings.Database_InitializeFromConfigFailed(
                                                initializerName, init.ContextTypeName),
                                            ex);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the specified connection string from the configuration
        /// </summary>
        /// <param name="name">Name of the connection string to get</param>
        /// <returns>The connection string, or null if there is no connection string with the specified name</returns>
        public ConnectionStringSettings GetConnectionString(string name)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(name));

            return _connectionStrings[name];
        }

        /// <summary>
        /// Gets a singleton instance of configuration based on the <see cref="ConfigurationManager"/> for the AppDomain
        /// </summary>
        public static AppConfig DefaultInstance
        {
            get { return _defaultInstance; }
        }

        private static KeyValueConfigurationCollection Convert(NameValueCollection collection)
        {
            var settings = new KeyValueConfigurationCollection();
            foreach (var key in collection.AllKeys)
            {
                settings.Add(key, ConfigurationManager.AppSettings[key]);
            }
            return settings;
        }

        public DbProviderServices GetDbProviderServices(string providerInvariantName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(providerInvariantName));

            var providerSection =
                _entityFrameworkSettings.Providers.OfType<ProviderElement>().FirstOrDefault(
                    e => providerInvariantName.Equals(e.InvariantName, StringComparison.OrdinalIgnoreCase));

            var providerTypeName = providerSection != null
                                       ? providerSection.ProviderTypeName
                                       : GetProviderTypeByConvention(providerInvariantName);
            var providerType = Type.GetType(providerTypeName, throwOnError: false);

            if (providerType == null)
            {
                throw new InvalidOperationException(Strings.EF6Providers_ProviderTypeMissing(providerTypeName, providerInvariantName));
            }

            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var instanceMember = providerType.GetProperty("Instance", bindingFlags)
                                 ?? (MemberInfo)providerType.GetField("Instance", bindingFlags);
            if (instanceMember == null)
            {
                throw new InvalidOperationException(Strings.EF6Providers_InstanceMissing(providerTypeName));
            }

            var providerInstance = instanceMember.GetValue() as DbProviderServices;
            if (providerInstance == null)
            {
                throw new InvalidOperationException(Strings.EF6Providers_NotDbProviderServices(providerTypeName));
            }

            return providerInstance;
        }

        public virtual string GetProviderTypeByConvention(string providerInvariantName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(providerInvariantName));

            var providerTemplate =
                providerInvariantName.Equals("System.Data.SqlClient", StringComparison.OrdinalIgnoreCase)
                    ? "System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer, Version={0}, Culture=neutral, PublicKeyToken=b77a5c561934e089"
                    : providerInvariantName.Equals("System.Data.SqlServerCe.4.0", StringComparison.OrdinalIgnoreCase)
                          ? "System.Data.Entity.SqlServerCompact.SqlCeProviderServices, EntityFramework.SqlServerCompact, Version={0}, Culture=neutral, PublicKeyToken=b77a5c561934e089"
                          : null;

            if (providerTemplate == null)
            {
                throw new InvalidOperationException(Strings.EF6Providers_NoProviderFound(providerInvariantName));
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                providerTemplate,
                new AssemblyName(typeof(DbContext).Assembly.FullName).Version);
        }
    }
}
