// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     Searches types (usually obtained from an assembly) for different kinds of <see cref="DbConfiguration" />.
    /// </summary>
    internal class DbConfigurationFinder
    {
        public virtual Type TryFindConfigurationType(IEnumerable<Type> typesToSearch)
        {
            Contract.Requires(typesToSearch != null);

            var configurations = typesToSearch
                .Where(
                    t => typeof(DbConfiguration).IsAssignableFrom(t)
                         && t != typeof(DbConfiguration)
                         && t != typeof(DbConfigurationProxy)
                         && !t.IsAbstract
                         && !t.IsGenericType)
                .ToList();

            // If there any null configurations then return one of them.
            var nullConfig = configurations.FirstOrDefault(c => typeof(DbNullConfiguration).IsAssignableFrom(c));
            if (nullConfig != null)
            {
                return nullConfig;
            }

            // Else if there is exactly one proxy config then use it.
            var proxyConfigs = configurations.Where(c => typeof(DbConfigurationProxy).IsAssignableFrom(c));
            if (proxyConfigs.Count() > 1)
            {
                throw new InvalidOperationException(
                    Strings.MultipleConfigsInAssembly(proxyConfigs.First().Assembly, typeof(DbConfigurationProxy).Name));
            }
            if (proxyConfigs.Count() == 1)
            {
                return proxyConfigs.First().CreateInstance<DbConfigurationProxy>().ConfigurationToUse();
            }

            // Else if there is exactly one normal config then use it, otherwise return null.
            if (configurations.Count > 1)
            {
                throw new InvalidOperationException(
                    Strings.MultipleConfigsInAssembly(configurations.First().Assembly, typeof(DbConfiguration).Name));
            }

            return configurations.FirstOrDefault();
        }

        public virtual DbConfiguration TryCreateConfiguration(IEnumerable<Type> typesToSearch)
        {
            Contract.Requires(typesToSearch != null);

            var configType = TryFindConfigurationType(typesToSearch);

            return configType == null || typeof(DbNullConfiguration).IsAssignableFrom(configType)
                       ? null
                       : configType.CreateInstance<DbConfiguration>();
        }
    }
}
