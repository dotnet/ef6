// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

    internal class DbConfigurationLoader
    {
        public virtual InternalConfiguration TryLoadFromConfig(AppConfig config)
        {
            DebugCheck.NotNull(config);

            var typeName = config.ConfigurationTypeName;
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return null;
            }

            Type configType;
            try
            {
                configType = Type.GetType(typeName, throwOnError: true);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(Strings.DbConfigurationTypeNotFound(typeName), ex);
            }

            return configType.CreateInstance<DbConfiguration>(Strings.CreateInstance_BadDbConfigurationType).InternalConfiguration;
        }

        public virtual bool AppConfigContainsDbConfigurationType(AppConfig config)
        {
            DebugCheck.NotNull(config);

            return !string.IsNullOrWhiteSpace(config.ConfigurationTypeName);
        }
    }
}
