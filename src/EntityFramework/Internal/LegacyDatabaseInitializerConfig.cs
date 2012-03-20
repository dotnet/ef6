namespace System.Data.Entity.Internal
{
    using System.Configuration;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Encapsulates information read from the application config file that specifies a database initializer
    ///     and allows that initializer to be dynamically applied.
    /// </summary>
    internal class LegacyDatabaseInitializerConfig
    {
        #region Fields and constructors

        private const string ConfigKeyKey = "DatabaseInitializerForType";
        private const string DisabledSpecialValue = "Disabled";

        private static readonly MethodInfo Database_SetInitializerInternal = typeof(Database).GetMethod(
            "SetInitializerInternal", BindingFlags.Static | BindingFlags.NonPublic);

        private readonly string _contextTypeName;
        private readonly string _initializerTypeName;
        private readonly bool _disabled;

        /// <summary>
        ///     Initializes a new instance of the <see cref = "LegacyDatabaseInitializerConfig" /> class.
        /// </summary>
        /// <param name = "configKey">The key from the entry in the config file.</param>
        /// <param name = "configValue">The value from the enrty in the config file.</param>
        public LegacyDatabaseInitializerConfig(string configKey, string configValue)
        {
            Contract.Requires(configKey != null);

            System.Diagnostics.Contracts.Contract.Assert(
                configKey.StartsWith(ConfigKeyKey, StringComparison.OrdinalIgnoreCase), "configKey must start with " + ConfigKeyKey);

            _contextTypeName = configKey.Remove(0, ConfigKeyKey.Length).Trim();

            if (String.IsNullOrWhiteSpace(configValue) ||
                configValue.Trim().Equals(DisabledSpecialValue, StringComparison.OrdinalIgnoreCase))
            {
                _disabled = true;
            }
            else
            {
                _initializerTypeName = configValue.Trim();
            }

            if (String.IsNullOrWhiteSpace(_contextTypeName) || (!_disabled && (String.IsNullOrWhiteSpace(_initializerTypeName))))
            {
                throw Error.Database_BadLegacyInitializerEntry(configKey, configValue);
            }
        }

        #endregion

        #region Applying initializers

        /// <summary>
        ///     Uses the context type and initializer type specified in the config to create an initializer instance
        ///     and set it with the DbDbatabase.SetInitializer method.
        /// </summary>
        public void ApplyInitializer()
        {
            try
            {
                var contextType = Type.GetType(_contextTypeName);
                if (contextType == null)
                {
                    throw Error.Database_FailedToResolveType(_contextTypeName);
                }

                object initializer = null;
                if (!_disabled)
                {
                    var initializerType = Type.GetType(_initializerTypeName);
                    if (initializerType == null)
                    {
                        throw Error.Database_FailedToResolveType(_initializerTypeName);
                    }

                    initializer = Activator.CreateInstance(initializerType);
                }

                var setInitializerMethod = Database_SetInitializerInternal.MakeGenericMethod(contextType);
                setInitializerMethod.Invoke(null, BindingFlags.Static | BindingFlags.NonPublic, null, new[] { initializer, true }, null);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    Strings.Database_InitializeFromLegacyConfigFailed(_disabled ? "Disabled" : _initializerTypeName, _contextTypeName), ex);
            }
        }

        /// <summary>
        ///     Reads all initializers from the application config file and sets them using the Database class.
        /// </summary>
        public static void ApplyInitializersFromConfig(KeyValueConfigurationCollection appSettings)
        {
            var keys = appSettings.AllKeys;
            if (keys != null)
            {
                foreach (var config in keys.Where(k => k.StartsWith(ConfigKeyKey, StringComparison.OrdinalIgnoreCase)).
                    Select(k => new LegacyDatabaseInitializerConfig(k, appSettings[k].Value)))
                {
                    config.ApplyInitializer();
                }
            }
        }

        #endregion
    }
}