// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Functions
{
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class ManyToManyModificationFunctionConfigurationTests : ModificationFunctionConfigurationTTests
    {
        [Fact]
        public void HasName_should_set_name_on_underlying_configuration()
        {
            var configuration = new ManyToManyModificationFunctionConfiguration<Entity, Entity>();

            configuration.HasName("Foo");

            Assert.Equal("Foo", configuration.Configuration.Name);
        }

        [Fact]
        public void HasName_when_schema_should_set_name_and_schema_on_underlying_configuration()
        {
            var configuration = new ManyToManyModificationFunctionConfiguration<Entity, Entity>();

            configuration.HasName("Foo", "Bar");

            Assert.Equal("Foo", configuration.Configuration.Name);
            Assert.Equal("Bar", configuration.Configuration.Schema);
        }

        [Fact]
        public void LeftKeyParameter_should_return_configuration_for_valid_property_expressions()
        {
            var configuration = new ManyToManyModificationFunctionConfiguration<Entity, Entity>();

            Assert.Same(configuration, configuration.LeftKeyParameter(e => e.Int, "Foo"));
            Assert.Same(configuration, configuration.LeftKeyParameter(e => e.Nullable, "Foo"));
            Assert.Same(configuration, configuration.LeftKeyParameter(e => e.String, "Foo"));
            Assert.Same(configuration, configuration.LeftKeyParameter(e => e.Bytes, "Foo"));
        }

        [Fact]
        public void LeftKeyParameter_should_set_parameter_name_for_valid_property_expressions()
        {
            Assert.Equal(
                "Foo",
                new ManyToManyModificationFunctionConfiguration<Entity, Entity>()
                    .LeftKeyParameter(e => e.Int, "Foo").Configuration.ParameterNames.Single().Value.Item1);

            Assert.Equal(
                "Foo",
                new ManyToManyModificationFunctionConfiguration<Entity, Entity>()
                    .LeftKeyParameter(e => e.Nullable, "Foo").Configuration.ParameterNames.Single().Value.Item1);

            Assert.Equal(
                "Foo",
                new ManyToManyModificationFunctionConfiguration<Entity, Entity>()
                    .LeftKeyParameter(e => e.String, "Foo").Configuration.ParameterNames.Single().Value.Item1);

            Assert.Equal(
                "Foo",
                new ManyToManyModificationFunctionConfiguration<Entity, Entity>()
                    .LeftKeyParameter(e => e.Bytes, "Foo").Configuration.ParameterNames.Single().Value.Item1);
        }

        [Fact]
        public void LeftKeyParameter_should_throw_when_complex_property_expression()
        {
            var configuration = new ManyToManyModificationFunctionConfiguration<Entity, Entity>();

            Assert.Equal(
                Strings.InvalidPropertyExpression("e => e.ComplexType.Int"),
                Assert.Throws<InvalidOperationException>(
                    () => configuration.LeftKeyParameter(e => e.ComplexType.Int, "Foo")).Message);
        }

        [Fact]
        public void RightKeyParameter_should_return_configuration_for_valid_property_expressions()
        {
            var configuration = new ManyToManyModificationFunctionConfiguration<Entity, Entity>();

            Assert.Same(configuration, configuration.RightKeyParameter(e => e.Int, "Foo"));
            Assert.Same(configuration, configuration.RightKeyParameter(e => e.Nullable, "Foo"));
            Assert.Same(configuration, configuration.RightKeyParameter(e => e.String, "Foo"));
            Assert.Same(configuration, configuration.RightKeyParameter(e => e.Bytes, "Foo"));
        }

        [Fact]
        public void RightKeyParameter_should_set_parameter_name_for_valid_property_expressions()
        {
            Assert.Equal(
                "Foo",
                new ManyToManyModificationFunctionConfiguration<Entity, Entity>()
                    .RightKeyParameter(e => e.Int, "Foo").Configuration.ParameterNames.Single().Value.Item1);

            Assert.Equal(
                "Foo",
                new ManyToManyModificationFunctionConfiguration<Entity, Entity>()
                    .RightKeyParameter(e => e.Nullable, "Foo").Configuration.ParameterNames.Single().Value.Item1);

            Assert.Equal(
                "Foo",
                new ManyToManyModificationFunctionConfiguration<Entity, Entity>()
                    .RightKeyParameter(e => e.String, "Foo").Configuration.ParameterNames.Single().Value.Item1);

            Assert.Equal(
                "Foo",
                new ManyToManyModificationFunctionConfiguration<Entity, Entity>()
                    .RightKeyParameter(e => e.Bytes, "Foo").Configuration.ParameterNames.Single().Value.Item1);
        }

        [Fact]
        public void RightKeyParameter_should_throw_when_complex_property_expression()
        {
            var configuration = new ManyToManyModificationFunctionConfiguration<Entity, Entity>();

            Assert.Equal(
                Strings.InvalidPropertyExpression("e => e.ComplexType.Int"),
                Assert.Throws<InvalidOperationException>(
                    () => configuration.RightKeyParameter(e => e.ComplexType.Int, "Foo")).Message);
        }
    }
}
