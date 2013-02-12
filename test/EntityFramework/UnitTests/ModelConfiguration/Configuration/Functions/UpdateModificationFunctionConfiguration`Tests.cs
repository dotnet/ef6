// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Functions
{
    using Xunit;

    public class UpdateModificationFunctionConfigurationTTests : ModificationFunctionConfigurationTTests
    {
        [Fact]
        public void Parameter_should_return_configuration_for_valid_property_expressions_when_original_values()
        {
            var configuration = new UpdateModificationFunctionConfiguration<Entity>();

            Assert.NotNull(configuration.Parameter(e => e.Int, true));
            Assert.NotNull(configuration.Parameter(e => e.Nullable, true));
            Assert.NotNull(configuration.Parameter(e => e.String, true));
            Assert.NotNull(configuration.Parameter(e => e.Bytes, true));
            Assert.NotNull(configuration.Parameter(e => e.Geography, true));
            Assert.NotNull(configuration.Parameter(e => e.Geometry, true));
            Assert.NotNull(configuration.Parameter(e => e.ComplexType.Int, true));
        }

        [Fact]
        public void BindResult_should_return_parent_configuration_for_valid_property_expressions()
        {
            var configuration = new UpdateModificationFunctionConfiguration<Entity>();

            Assert.Same(configuration, configuration.BindResult(e => e.Int, "Foo"));
            Assert.Same(configuration, configuration.BindResult(e => e.Nullable, "Foo"));
            Assert.Same(configuration, configuration.BindResult(e => e.String, "Foo"));
            Assert.Same(configuration, configuration.BindResult(e => e.Bytes, "Foo"));
            Assert.Same(configuration, configuration.BindResult(e => e.Geography, "Foo"));
            Assert.Same(configuration, configuration.BindResult(e => e.Geometry, "Foo"));
        }
        
        [Fact]
        public void BindResult_should_throw_when_complex_property_expressions()
        {
            var configuration = new UpdateModificationFunctionConfiguration<Entity>();

            Assert.Throws<InvalidOperationException>(() => configuration.BindResult(e => e.ComplexType.Int, "Foo"));
        }

        [Fact]
        public void RowsAffectedParameter_should_set_column_name()
        {
            var configuration = new UpdateModificationFunctionConfiguration<Entity>();

            configuration.RowsAffectedParameter("Foo");

            Assert.Equal("Foo", configuration.Configuration.RowsAffectedParameterName);
        }

        protected override ModificationFunctionConfiguration<Entity> CreateConfiguration()
        {
            return new UpdateModificationFunctionConfiguration<Entity>();
        }
    }
}
