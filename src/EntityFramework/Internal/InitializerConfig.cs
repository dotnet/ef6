// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Configuration;
    using System.Data.Entity.Internal.ConfigFile;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal class InitializerConfig
    {
        private const string ConfigKeyKey = "DatabaseInitializerForType";
        private const string DisabledSpecialValue = "Disabled";

        private readonly EntityFrameworkSection _entityFrameworkSettings;
        private readonly KeyValueConfigurationCollection _appSettings;

        public InitializerConfig()
        {
        }

        public InitializerConfig(EntityFrameworkSection entityFrameworkSettings, KeyValueConfigurationCollection appSettings)
        {
            Contract.Requires(entityFrameworkSettings != null);
            Contract.Requires(appSettings != null);

            _entityFrameworkSettings = entityFrameworkSettings;
            _appSettings = appSettings;
        }

        private static object TryGetInitializer(
            Type requiredContextType,
            string contextTypeName,
            string initializerTypeName,
            bool isDisabled,
            Func<object[]> initializerArgs,
            Func<object, object, string> exceptionMessage)
        {
            Contract.Requires(requiredContextType != null);
            Contract.Requires(contextTypeName != null);
            Contract.Requires(initializerTypeName != null);
            Contract.Requires(initializerArgs != null);
            Contract.Requires(exceptionMessage != null);

            try
            {
                if (Type.GetType(contextTypeName, throwOnError: true) == requiredContextType)
                {
                    if (isDisabled)
                    {
                        return Activator.CreateInstance(typeof(NullDatabaseInitializer<>).MakeGenericType(requiredContextType));
                    }

                    return Activator.CreateInstance(Type.GetType(initializerTypeName, throwOnError: true), initializerArgs());
                }
            }
            catch (Exception ex)
            {
                var initializerName = isDisabled ? "Disabled" : initializerTypeName;

                throw new InvalidOperationException(exceptionMessage(initializerName, contextTypeName), ex);
            }
            return null;
        }

        public virtual object TryGetInitializer(Type contextType)
        {
            return TryGetInitializerFromEntityFrameworkSection(contextType) ?? TryGetInitializerFromLegacyConfig(contextType);
        }

        private object TryGetInitializerFromEntityFrameworkSection(Type contextType)
        {
            Contract.Requires(contextType != null);

            return _entityFrameworkSettings.Contexts
                .OfType<ContextElement>()
                .Where(
                    e => e.IsDatabaseInitializationDisabled
                         || !string.IsNullOrWhiteSpace(e.DatabaseInitializer.InitializerTypeName))
                .Select(
                    e => TryGetInitializer(
                        contextType,
                        e.ContextTypeName,
                        e.DatabaseInitializer.InitializerTypeName ?? string.Empty,
                        e.IsDatabaseInitializationDisabled,
                        () => e.DatabaseInitializer.Parameters.GetTypedParameterValues(),
                        Strings.Database_InitializeFromConfigFailed))
                .FirstOrDefault(i => i != null);
        }

        private object TryGetInitializerFromLegacyConfig(Type contextType)
        {
            Contract.Requires(contextType != null);

            foreach (var key in _appSettings.AllKeys.Where(k => k.StartsWith(ConfigKeyKey, StringComparison.OrdinalIgnoreCase)))
            {
                var contextTypeName = key.Remove(0, ConfigKeyKey.Length).Trim();
                var configValue = (_appSettings[key].Value ?? string.Empty).Trim();

                if (String.IsNullOrWhiteSpace(contextTypeName))
                {
                    throw new InvalidOperationException(Strings.Database_BadLegacyInitializerEntry(key, configValue));
                }

                var initializer = TryGetInitializer(
                    contextType,
                    contextTypeName,
                    configValue,
                    configValue.Length == 0 || configValue.Equals(DisabledSpecialValue, StringComparison.OrdinalIgnoreCase),
                    () => new object[0],
                    Strings.Database_InitializeFromLegacyConfigFailed);

                if (initializer != null)
                {
                    return initializer;
                }
            }
            return null;
        }
    }
}
