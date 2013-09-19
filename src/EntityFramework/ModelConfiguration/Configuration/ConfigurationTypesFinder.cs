// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Utilities;

    internal class ConfigurationTypesFinder
    {
        private readonly ConfigurationTypeActivator _activator;
        private readonly ConfigurationTypeFilter _filter;

        public ConfigurationTypesFinder()
            : this(new ConfigurationTypeActivator(), new ConfigurationTypeFilter())
        {
        }

        public ConfigurationTypesFinder(ConfigurationTypeActivator activator, ConfigurationTypeFilter filter)
        {
            DebugCheck.NotNull(activator);
            DebugCheck.NotNull(filter);

            _activator = activator;
            _filter = filter;
        }

        public virtual void AddConfigurationTypesToModel(IEnumerable<Type> types, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(types);
            DebugCheck.NotNull(modelConfiguration);

            foreach (var type in types)
            {
                if (_filter.IsEntityTypeConfiguration(type))
                {
                    modelConfiguration.Add(_activator.Activate<EntityTypeConfiguration>(type));
                }
                else if (_filter.IsComplexTypeConfiguration(type))
                {
                    modelConfiguration.Add(_activator.Activate<ComplexTypeConfiguration>(type));
                }
            }
        }
    }
}
