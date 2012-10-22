// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class PropertyConventionConfigurationTests
    {
        [Fact]
        public void Where_evaluates_preconditions()
        {
            var properties = new PropertyConventionConfiguration();

            var ex = Assert.Throws<ArgumentNullException>(
                () => properties.Where(null));
            Assert.Equal("predicate", ex.ParamName);
        }

        [Fact]
        public void Where_records_predicates()
        {
            Func<PropertyInfo, bool> predicate1 = p => true;
            Func<PropertyInfo, bool> predicate2 = p => false;
            var properties = new PropertyConventionConfiguration();

            properties.Where(predicate1)
                .Where(predicate2);

            Assert.Equal(2, properties.Predicates.Count);
            Assert.Same(predicate2, properties.Predicates.Last());
        }

        [Fact]
        public void Configure_evaluates_preconditions()
        {
            var properties = new PropertyConventionConfiguration();

            var ex = Assert.Throws<ArgumentNullException>(
                () => properties.Configure(null));
            Assert.Equal("propertyConfigurationAction", ex.ParamName);
        }

        [Fact]
        public void Configure_sets_action()
        {
            Action<LightweightPropertyConfiguration> configurationAction = c => { };
            var properties = new PropertyConventionConfiguration();

            properties.Configure(configurationAction);

            Assert.Same(configurationAction, properties.ConfigurationAction);
        }
    }
}
