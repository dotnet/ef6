// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Functions
{
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class AssociationModificationFunctionConfigurationTests : ModificationFunctionConfigurationTTests
    {
        [Fact]
        public void Parameter_should_return_configuration_for_valid_property_expressions()
        {
            var configuration
                = new AssociationModificationFunctionConfiguration<Entity>(
                    new MockPropertyInfo(),
                    new ModificationFunctionConfiguration());

            Assert.Same(configuration, configuration.Parameter(e => e.Int, "Foo"));
            Assert.Same(configuration, configuration.Parameter(e => e.Nullable, "Foo"));
            Assert.Same(configuration, configuration.Parameter(e => e.String, "Foo"));
            Assert.Same(configuration, configuration.Parameter(e => e.Bytes, "Foo"));
        }

        [Fact]
        public void Parameter_should_throw_when_complex_property_expression()
        {
            var configuration
                = new AssociationModificationFunctionConfiguration<Entity>(
                    new MockPropertyInfo(),
                    new ModificationFunctionConfiguration());

            Assert.Equal(
                Strings.InvalidPropertyExpression("e => e.ComplexType.Int"),
                Assert.Throws<InvalidOperationException>(
                    () => configuration.Parameter(e => e.ComplexType.Int, "Foo")).Message);
        }

        [Fact]
        public void Parameter_should_set_parameter_name_for_valid_property_expressions()
        {
            var modificationFunctionConfiguration = new ModificationFunctionConfiguration();

            var configuration
                = new AssociationModificationFunctionConfiguration<Entity>(
                    new MockPropertyInfo(),
                    modificationFunctionConfiguration);

            configuration.Parameter(e => e.Int, "Foo");

            Assert.Equal("Foo", modificationFunctionConfiguration.ParameterNames.Single().Value.Item1);

            modificationFunctionConfiguration.ParameterNames.Clear();

            configuration.Parameter(e => e.Nullable, "Foo");

            Assert.Equal("Foo", modificationFunctionConfiguration.ParameterNames.Single().Value.Item1);

            modificationFunctionConfiguration.ParameterNames.Clear();

            configuration.Parameter(e => e.String, "Foo");

            Assert.Equal("Foo", modificationFunctionConfiguration.ParameterNames.Single().Value.Item1);

            modificationFunctionConfiguration.ParameterNames.Clear();

            configuration.Parameter(e => e.Bytes, "Foo");

            Assert.Equal("Foo", modificationFunctionConfiguration.ParameterNames.Single().Value.Item1);

            modificationFunctionConfiguration.ParameterNames.Clear();
        }
    }
}
