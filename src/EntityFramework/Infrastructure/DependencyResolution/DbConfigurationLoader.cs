// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Collections.Generic;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

    internal class DbConfigurationLoader
    {
        public virtual Type TryLoadFromConfig(AppConfig config)
        {
            DebugCheck.NotNull(config);

            var typeName = config.ConfigurationTypeName;
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return null;
            }

            Type type;
            try
            {
                type = Type.GetType(typeName, throwOnError: true);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(Strings.DbConfigurationTypeNotFound(typeName), ex);
            }

            if (!typeof(DbConfiguration).IsAssignableFrom(type))
            {
                throw new InvalidOperationException(
                    Strings.CreateInstance_BadDbConfigurationType(type.ToString(), typeof(DbConfiguration).ToString()));
            }

            return type;
        }

        public virtual bool AppConfigContainsDbConfigurationType(AppConfig config)
        {
            DebugCheck.NotNull(config);

            return !string.IsNullOrWhiteSpace(config.ConfigurationTypeName);
        }
    }
}
