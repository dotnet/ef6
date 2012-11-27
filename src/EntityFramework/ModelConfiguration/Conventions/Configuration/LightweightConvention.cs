// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;

    internal class LightweightConvention : IConfigurationConvention<Type, EntityTypeConfiguration>,
                                           IConfigurationConvention<PropertyInfo, PrimitivePropertyConfiguration>
    {
        private readonly EntityConventionConfiguration _configuration;

        public LightweightConvention(EntityConventionConfiguration conventionConfiguration)
        {
            DebugCheck.NotNull(conventionConfiguration);

            _configuration = conventionConfiguration;
        }

        public void Apply(Type memberInfo, Func<EntityTypeConfiguration> configuration)
        {
            Check.NotNull(memberInfo, "memberInfo");
            Check.NotNull(configuration, "configuration");

            if (_configuration.ConfigurationAction != null
                && _configuration.Predicates.All(p => p(memberInfo)))
            {
                _configuration.ConfigurationAction(new LightweightEntityConfiguration(memberInfo, configuration));
            }
        }

        public void Apply(PropertyInfo memberInfo, Func<PrimitivePropertyConfiguration> configuration)
        {
            Check.NotNull(memberInfo, "memberInfo");
            Check.NotNull(configuration, "configuration");

            if (_configuration.PropertyConfiguration != null
                && _configuration.PropertyConfiguration.ConfigurationAction != null
                && _configuration.Predicates.All(p => p(memberInfo.ReflectedType))
                && _configuration.PropertyConfiguration.Predicates.All(p => p(memberInfo)))
            {
                _configuration.PropertyConfiguration.ConfigurationAction(new LightweightPropertyConfiguration(memberInfo, configuration));
            }
        }
    }
}
