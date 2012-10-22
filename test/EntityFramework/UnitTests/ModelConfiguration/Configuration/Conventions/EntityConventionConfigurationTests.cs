// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Linq;
    using Xunit;

    public class EntityConventionConfigurationTests
    {
        [Fact]
        public void Where_evaluates_preconditions()
        {
            var entities = new EntityConventionConfiguration();

            var ex = Assert.Throws<ArgumentNullException>(
                () => entities.Where(null));
            Assert.Equal("predicate", ex.ParamName);
        }

        [Fact]
        public void Where_records_predicates()
        {
            Func<Type, bool> predicate1 = t => true;
            Func<Type, bool> predicate2 = t => false;
            var entities = new EntityConventionConfiguration();

            entities.Where(predicate1)
                .Where(predicate2);

            Assert.Equal(2, entities.Predicates.Count);
            Assert.Same(predicate2, entities.Predicates.Last());
        }

        [Fact]
        public void Configure_evaluates_preconditions()
        {
            var entities = new EntityConventionConfiguration();

            var ex = Assert.Throws<ArgumentNullException>(
                () => entities.Configure(null));
            Assert.Equal("entityConfigurationAction", ex.ParamName);
        }

        [Fact]
        public void Configure_sets_action()
        {
            Action<LightweightEntityConfiguration> configurationAction = c => { };
            var entities = new EntityConventionConfiguration();

            entities.Configure(configurationAction);

            Assert.Same(configurationAction, entities.ConfigurationAction);
        }

        [Fact]
        public void Properties_sets_and_returns_property_configuration()
        {
            var entities = new EntityConventionConfiguration();

            var propertyConfiguration = entities.Properties();

            Assert.NotNull(propertyConfiguration);
            Assert.Same(propertyConfiguration, entities.PropertyConfiguration);
        }
    }
}
