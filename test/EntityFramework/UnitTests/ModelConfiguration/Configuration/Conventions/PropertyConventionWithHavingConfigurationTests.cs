// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Conventions
{
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class PropertyConventionWithHavingConfigurationTests
    {
        [Fact]
        public void Configure_evaluates_preconditions()
        {
            var conventions = new ConventionsConfiguration();
            var entities = new PropertyConventionConfiguration(conventions);

            var ex = Assert.Throws<ArgumentNullException>(
                () => entities.Having<object>(p => null).Configure(null));
            Assert.Equal("propertyConfigurationAction", ex.ParamName);
        }

        [Fact]
        public void Configure_adds_convention()
        {
            Func<PropertyInfo, bool> predicate = p => true;
            Func<PropertyInfo, object> capturingPredicate = p => null;
            Action<ConventionPrimitivePropertyConfiguration, object> configurationAction = (c, o) => { };
            var conventions = new ConventionsConfiguration();
            var properties = new PropertyConventionConfiguration(conventions);

            properties
                .Where(predicate)
                .Having(capturingPredicate)
                .Configure(configurationAction);

            Assert.Equal(16, conventions.ConfigurationConventions.Count());

            var convention = (PropertyConventionWithHaving<object>)conventions.ConfigurationConventions.Last();
            Assert.Equal(1, convention.Predicates.Count());
            Assert.Same(predicate, convention.Predicates.Single());
            Assert.Same(capturingPredicate, convention.CapturingPredicate);
            Assert.Same(configurationAction, convention.PropertyConfigurationAction);
        }
    }
}
