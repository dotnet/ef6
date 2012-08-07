// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.UnitTests
{
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public sealed class ConfigurationRegistrarTests
    {
        [Fact]
        public void Add_entity_configuration_should_validate_preconditions()
        {
            Assert.Equal(
                Error.ArgumentNull("entityTypeConfiguration").Message,
                Assert.Throws<ArgumentNullException>(
                    () => new ConfigurationRegistrar(new ModelConfiguration()).Add(null as EntityTypeConfiguration<object>)).Message);
        }

        [Fact]
        public void Add_complex_type_should_validate_preconditions()
        {
            Assert.Equal(
                Error.ArgumentNull("complexTypeConfiguration").Message,
                Assert.Throws<ArgumentNullException>(
                    () => new ConfigurationRegistrar(new ModelConfiguration()).Add(null as ComplexTypeConfiguration<object>)).Message);
        }

        [Fact]
        public void Add_entity_configuration_should_add_to_model_configuration()
        {
            var modelConfiguration = new ModelConfiguration();
            var entityConfiguration = new EntityTypeConfiguration<object>();

            new ConfigurationRegistrar(modelConfiguration).Add(entityConfiguration);

            Assert.Same(entityConfiguration.Configuration, modelConfiguration.Entity(typeof(object)));
        }

        [Fact]
        public void Add_complex_type_configuration_should_add_to_model_configuration()
        {
            var modelConfiguration = new ModelConfiguration();
            var complexTypeConfiguration = new ComplexTypeConfiguration<object>();

            new ConfigurationRegistrar(modelConfiguration).Add(complexTypeConfiguration);

            Assert.Same(complexTypeConfiguration.Configuration, modelConfiguration.ComplexType(typeof(object)));
        }

        [Fact]
        public void Get_configured_types_should_return_types()
        {
            var modelConfiguration = new ModelConfiguration();
            var configurationRegistrar
                = new ConfigurationRegistrar(modelConfiguration)
                    .Add(new ComplexTypeConfiguration<object>())
                    .Add(new EntityTypeConfiguration<string>());

            Assert.Equal(2, configurationRegistrar.GetConfiguredTypes().Count());
        }
    }
}
