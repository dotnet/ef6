// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Functions
{
    using System.Linq;
    using Xunit;

    public class LightweightDeleteModificationFunctionConfigurationTests : ModificationFunctionConfigurationTTests
    {
        [Fact]
        public void HasName_should_set_name_on_underlying_configuration()
        {
            var configuration = new LightweightDeleteModificationFunctionConfiguration(typeof(Entity));

            configuration.HasName("Foo");

            Assert.Equal("Foo", configuration.Configuration.Name);
        }

        [Fact]
        public void HasName_when_schema_should_set_name_and_schema_on_underlying_configuration()
        {
            var configuration = new LightweightDeleteModificationFunctionConfiguration(typeof(Entity));

            configuration.HasName("Foo", "Bar");

            Assert.Equal("Foo", configuration.Configuration.Name);
            Assert.Equal("Bar", configuration.Configuration.Schema);
        }

        [Fact]
        public void Parameter_should_return_configuration_for_valid_property_expressions()
        {
            var configuration = new LightweightDeleteModificationFunctionConfiguration(typeof(Entity));

            Assert.Same(configuration, configuration.Parameter("Int", "Foo"));
            Assert.Same(configuration, configuration.Parameter(typeof(Entity).GetProperty("String"), "Foo"));
        }

        [Fact]
        public void Parameter_should_set_parameter_name_for_valid_property_expressions()
        {
            Assert.Equal(
                "Foo",
                new LightweightDeleteModificationFunctionConfiguration(typeof(Entity))
                    .Parameter("Int", "Foo").Configuration.ParameterNames.Single().Value.Item1);

            Assert.Equal(
                "Foo",
                new LightweightDeleteModificationFunctionConfiguration(typeof(Entity))
                    .Parameter(typeof(Entity).GetProperty("String"), "Foo").Configuration.ParameterNames.Single().Value.Item1);
        }

        [Fact]
        public void Parameter_is_no_op_when_not_found()
        {
            var configuration = new LightweightDeleteModificationFunctionConfiguration(typeof(Entity));

            Assert.Same(configuration, configuration.Parameter("Int1", "Foo"));
            Assert.Same(configuration, configuration.Parameter(typeof(Entity).GetProperty("String1"), "Foo"));
        }

        [Fact]
        public void RowsAffectedParameter_should_set_column_name()
        {
            var configuration = new LightweightDeleteModificationFunctionConfiguration(typeof(Entity));

            configuration.RowsAffectedParameter("Foo");

            Assert.Equal("Foo", configuration.Configuration.RowsAffectedParameterName);
        }
    }
}
