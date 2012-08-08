// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal.ConfigFile;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     A simple representation of an app.config or web.config file.
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
            new Lazy<IDbConnectionFactory>(() => null, isThreadSafe: true);

        /// <summary>
        ///     Initializes a new instance of AppConfig based on supplied configuration
        /// </summary>
        /// <param name="configuration"> Configuration to load settings from </param>
        public AppConfig(Configuration configuration)
            : this(
                configuration.ConnectionStrings.ConnectionStrings,
                configuration.AppSettings.Settings,
                (EntityFrameworkSection)configuration.GetSection(EFSectionName))
        {
            Contract.Requires(configuration != null);
        }

        /// <summary>
        ///     Initializes a new instance of AppConfig based on supplied connection strings
        ///     The default configuration for database initializers and default connection factory will be used
        /// </summary>
        /// <param name="connectionStrings"> Connection strings to be used </param>
        public AppConfig(ConnectionStringSettingsCollection connectionStrings)
            : this(connectionStrings, null, null)
        {
            Contract.Requires(connectionStrings != null);
        }

        /// <summary>
        ///     Initializes a new instance of AppConfig based on the <see cref="ConfigurationManager" /> for the AppDomain
        /// </summary>
        /// <remarks>
        ///     Use AppConfig.DefaultInstance instead of this constructor
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
            _appSettings = appSettings ?? new KeyValueConfigurationCollection();
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
        ///     Gets the default connection factory based on the configuration
        /// </summary>
        public virtual IDbConnectionFactory TryGetDefaultConnectionFactory()
        {
            return _defaultConnectionFactory.Value;
        }

        /// <summary>
        ///     Gets the specified connection string from the configuration
        /// </summary>
        /// <param name="name"> Name of the connection string to get </param>
        /// <returns> The connection string, or null if there is no connection string with the specified name </returns>
        public ConnectionStringSettings GetConnectionString(string name)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(name));

            return _connectionStrings[name];
        }

        /// <summary>
        ///     Gets a singleton instance of configuration based on the <see cref="ConfigurationManager" /> for the AppDomain
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

        public virtual ProviderConfig Providers
        {
            get { return new ProviderConfig(_entityFrameworkSettings); }
        }

        public virtual InitializerConfig Initializers
        {
            get { return new InitializerConfig(_entityFrameworkSettings, _appSettings); }
        }

        public virtual string ConfigurationTypeName
        {
            get { return _entityFrameworkSettings.ConfigurationTypeName; }
        }
    }
}
