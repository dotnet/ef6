// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public sealed class ComplexTypeExtensionsTests
    {
        [Fact]
        public void AddPrimitiveProperty_should_create_and_add_to_primitive_properties()
        {
            var complexType = new ComplexType("C");
            var property1 = EdmProperty.Primitive("Foo", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            complexType.AddMember(property1);
            var property = property1;

            Assert.NotNull(property);
            Assert.Equal("Foo", property.Name);
            Assert.True(complexType.Properties.Contains(property));
        }

        [Fact]
        public void GetPrimitiveProperty_should_return_correct_property()
        {
            var complexType = new ComplexType("C");
            var property1 = EdmProperty.Primitive("Foo", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            complexType.AddMember(property1);
            var property = property1;

            var foundProperty = complexType.Properties.SingleOrDefault(p => p.Name == "Foo");

            Assert.NotNull(foundProperty);
            Assert.Same(property, foundProperty);
        }

        [Fact]
        public void Should_be_able_to_get_and_set_clr_type()
        {
            var complexType = new ComplexType("C");

            Assert.Null(complexType.GetClrType());

            var type = typeof(object);

            complexType.Annotations.SetClrType(type);

            Assert.Equal(typeof(object), complexType.GetClrType());
        }

        [Fact]
        public void AddComplexProperty_should_create_and_add_complex_property()
        {
            var complexType = new ComplexType("C");
            var complexTypeProperty = new ComplexType("D");
            var property = complexType.AddComplexProperty("Foo", complexTypeProperty);

            Assert.NotNull(property);
            Assert.Equal("Foo", property.Name);
            Assert.Same(complexTypeProperty, property.ComplexType);
            Assert.True(complexType.Properties.Contains(property));
        }
    }
}
