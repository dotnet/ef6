// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Functions
{
    using Xunit;

    public class InsertModificationFunctionConfigurationTTests : ModificationFunctionConfigurationTTests
    {
        [Fact]
        public void BindResult_should_return_parent_configuration_for_valid_property_expressions()
        {
            var configuration = new InsertModificationFunctionConfiguration<Entity>();

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
            var configuration = new InsertModificationFunctionConfiguration<Entity>();

            Assert.Throws<InvalidOperationException>(() => configuration.BindResult(e => e.ComplexType.Int, "Foo"));
        }

        protected override ModificationFunctionConfiguration<Entity> CreateConfiguration()
        {
            return new InsertModificationFunctionConfiguration<Entity>();
        }
    }
}
