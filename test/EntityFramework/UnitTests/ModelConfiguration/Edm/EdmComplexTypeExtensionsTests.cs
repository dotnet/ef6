// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Edm.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Linq;
    using Xunit;

    public sealed class EdmComplexTypeExtensionsTests
    {
        [Fact]
        public void AddPrimitiveProperty_should_create_and_add_to_primitive_properties()
        {
            var complexType = new EdmComplexType();

            var property = complexType.AddPrimitiveProperty("Foo");

            Assert.NotNull(property);
            Assert.Equal("Foo", property.Name);
            Assert.True(complexType.Properties.Contains(property));
        }

        [Fact]
        public void GetPrimitiveProperty_should_return_correct_property()
        {
            var complexType = new EdmComplexType();
            var property = complexType.AddPrimitiveProperty("Foo");

            var foundProperty = complexType.GetPrimitiveProperty("Foo");

            Assert.NotNull(foundProperty);
            Assert.Same(property, foundProperty);
        }

        [Fact]
        public void Should_be_able_to_get_and_set_clr_type()
        {
            var complexType = new EdmComplexType();

            Assert.Null(complexType.GetClrType());

            complexType.SetClrType(typeof(object));

            Assert.Equal(typeof(object), complexType.GetClrType());
        }

        [Fact]
        public void Can_get_and_set_configuration_facet()
        {
            var complexType = new EdmComplexType();
            complexType.SetConfiguration(42);

            Assert.Equal(42, complexType.GetConfiguration());
        }

        [Fact]
        public void AddComplexProperty_should_create_and_add_complex_property()
        {
            var complexType = new EdmComplexType();

            var complexTypeProperty = new EdmComplexType();
            var property = complexType.AddComplexProperty("Foo", complexTypeProperty);

            Assert.NotNull(property);
            Assert.Equal("Foo", property.Name);
            Assert.Same(complexTypeProperty, property.PropertyType.ComplexType);
            Assert.True(complexType.Properties.Contains(property));
        }
    }
}