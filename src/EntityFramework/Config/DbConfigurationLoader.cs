// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.Contracts;

    internal class DbConfigurationLoader
    {
        public virtual InternalConfiguration TryLoadFromConfig(AppConfig config)
        {
            Contract.Requires(config != null);

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
            Contract.Requires(config != null);

            return !string.IsNullOrWhiteSpace(config.ConfigurationTypeName);
        }
    }
}
