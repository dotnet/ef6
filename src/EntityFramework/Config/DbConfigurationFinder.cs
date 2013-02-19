// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Searches types (usually obtained from an assembly) for different kinds of <see cref="DbConfiguration" />.
    /// </summary>
    internal class DbConfigurationFinder
    {
        public virtual Type TryFindConfigurationType(Type contextType, IEnumerable<Type> typesToSearch = null)
        {
            DebugCheck.NotNull(contextType);

            var typeFromAttribute = contextType.GetCustomAttributes(inherit: true)
                                               .OfType<DbConfigurationTypeAttribute>()
                                               .Select(a => a.ConfigurationType)
                                               .FirstOrDefault();

            if (typeFromAttribute != null)
            {
                if (!typeof(DbConfiguration).IsAssignableFrom(typeFromAttribute))
                {
                    throw new InvalidOperationException(
                        Strings.CreateInstance_BadDbConfigurationType(typeFromAttribute.ToString(), typeof(DbConfiguration).ToString()));
                }
                return typeFromAttribute;
            }

            var configurations = (typesToSearch ?? contextType.Assembly.GetAccessibleTypes())
                .Where(
                    t => typeof(DbConfiguration).IsAssignableFrom(t)
                         && t != typeof(DbConfiguration)
                         && !t.IsAbstract
                         && !t.IsGenericType)
                .ToList();

            if (configurations.Count > 1)
            {
                throw new InvalidOperationException(
                    Strings.MultipleConfigsInAssembly(configurations.First().Assembly, typeof(DbConfiguration).Name));
            }

            return configurations.FirstOrDefault();
        }
    }
}
