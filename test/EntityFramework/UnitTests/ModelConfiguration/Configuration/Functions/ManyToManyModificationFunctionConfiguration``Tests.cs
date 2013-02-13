// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Functions
{
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using Xunit;

    public class ManyToManyModificationFunctionConfigurationTests
    {
        [Fact]
        public void HasName_should_set_name_on_underlying_configuration()
        {
            var configuration = new ManyToManyModificationFunctionConfiguration<Entity, Entity>();

            configuration.HasName("Foo");

            Assert.Equal("Foo", configuration.Configuration.Name);
        }

        [Fact]
        public void LeftKeyParameter_should_return_configuration_for_valid_property_expressions()
        {
            var configuration = new ManyToManyModificationFunctionConfiguration<Entity, Entity>();

            Assert.NotNull(configuration.LeftKeyParameter(e => e.Int));
            Assert.NotNull(configuration.LeftKeyParameter(e => e.Nullable));
            Assert.NotNull(configuration.LeftKeyParameter(e => e.String));
            Assert.NotNull(configuration.LeftKeyParameter(e => e.Bytes));
            Assert.NotNull(configuration.LeftKeyParameter(e => e.Geography));
            Assert.NotNull(configuration.LeftKeyParameter(e => e.Geometry));
        }

        [Fact]
        public void LeftKeyParameter_should_throw_when_complex_property_expression()
        {
            var configuration = new ManyToManyModificationFunctionConfiguration<Entity, Entity>();

            Assert.Equal(
                Strings.InvalidPropertyExpression("e => e.ComplexType.Int"),
                Assert.Throws<InvalidOperationException>(
                    () => configuration.LeftKeyParameter(e => e.ComplexType.Int)).Message);
        }

        [Fact]
        public void RightKeyParameter_should_return_configuration_for_valid_property_expressions()
        {
            var configuration = new ManyToManyModificationFunctionConfiguration<Entity, Entity>();

            Assert.NotNull(configuration.RightKeyParameter(e => e.Int));
            Assert.NotNull(configuration.RightKeyParameter(e => e.Nullable));
            Assert.NotNull(configuration.RightKeyParameter(e => e.String));
            Assert.NotNull(configuration.RightKeyParameter(e => e.Bytes));
            Assert.NotNull(configuration.RightKeyParameter(e => e.Geography));
            Assert.NotNull(configuration.RightKeyParameter(e => e.Geometry));
        }

        [Fact]
        public void RightKeyParameter_should_throw_when_complex_property_expression()
        {
            var configuration = new ManyToManyModificationFunctionConfiguration<Entity, Entity>();

            Assert.Equal(
                Strings.InvalidPropertyExpression("e => e.ComplexType.Int"),
                Assert.Throws<InvalidOperationException>(
                    () => configuration.RightKeyParameter(e => e.ComplexType.Int)).Message);
        }

        protected class Entity
        {
            public int Int { get; set; }
            public short? Nullable { get; set; }
            public string String { get; set; }
            public byte[] Bytes { get; set; }
            public DbGeography Geography { get; set; }
            public DbGeometry Geometry { get; set; }
            public ComplexType ComplexType { get; set; }
        }

        protected class ComplexType
        {
            public int Int { get; set; }
        }
    }
}
