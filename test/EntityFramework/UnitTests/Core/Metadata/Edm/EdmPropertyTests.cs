// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class EdmPropertyTests
    {
        [Fact]
        public void Primitive_should_create_primitive_property()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32);

            var property = EdmProperty.Primitive("P", primitiveType);

            Assert.NotNull(property);
            Assert.NotNull(property.TypeUsage);
            Assert.Same(primitiveType, property.TypeUsage.EdmType);
        }

        [Fact]
        public void Enum_should_create_enum_property()
        {
            var enumType = new EnumType();

            var property = EdmProperty.Enum("P", enumType);

            Assert.NotNull(property);
            Assert.NotNull(property.TypeUsage);
            Assert.Same(enumType, property.TypeUsage.EdmType);
        }

        [Fact]
        public void Complex_should_create_complex_property()
        {
            var complexType = new ComplexType();

            var property = EdmProperty.Complex("P", complexType);

            Assert.NotNull(property);
            Assert.NotNull(property.TypeUsage);
            Assert.Same(complexType, property.TypeUsage.EdmType);
        }

        [Fact]
        public void Can_set_nullable_facet()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32);

            var property = EdmProperty.Primitive("P", primitiveType);

            Assert.True(property.Nullable);

            property.Nullable = false;

            Assert.False(property.Nullable);
        }

        [Fact]
        public void IsComplexType_returns_true_when_complex_property()
        {
            var complexType = new ComplexType();

            var property = EdmProperty.Complex("P", complexType);

            Assert.False(property.IsPrimitiveType);
            Assert.False(property.IsEnumType);
            Assert.True(property.IsComplexType);
        }

        [Fact]
        public void IsPrimitiveType_returns_true_when_primitive_property()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32);

            var property = EdmProperty.Primitive("P", primitiveType);

            Assert.False(property.IsComplexType);
            Assert.False(property.IsEnumType);
            Assert.True(property.IsPrimitiveType);
        }

        [Fact]
        public void IsEnumType_returns_true_when_enum_property()
        {
            var enumType = new EnumType();

            var property = EdmProperty.Enum("P", enumType);

            Assert.False(property.IsComplexType);
            Assert.False(property.IsPrimitiveType);
            Assert.True(property.IsEnumType);
        }

        [Fact]
        public void IsCollectionType_returns_true_when_collection_property()
        {
            var collectionType = new CollectionType();

            var property = new EdmProperty("P", TypeUsage.Create(collectionType));

            Assert.False(property.IsComplexType);
            Assert.False(property.IsPrimitiveType);
            Assert.False(property.IsEnumType);
            Assert.True(property.IsCollectionType);
        }

        [Fact]
        public void IsUnderlyingPrimitiveType_returns_true_when_underlying_primitive_property()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32);

            var property = EdmProperty.Primitive("P", primitiveType);

            Assert.True(property.IsUnderlyingPrimitiveType);

            var enumType = new EnumType();

            property = EdmProperty.Enum("P", enumType);

            Assert.True(property.IsUnderlyingPrimitiveType);
        }

        [Fact]
        public void ComplexType_returns_type_when_complex_property()
        {
            var complexType = new ComplexType();

            var property = EdmProperty.Complex("P", complexType);

            Assert.Same(complexType, property.ComplexType);
        }

        [Fact]
        public void PrimitiveType_returns_type_when_primitive_property()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32);

            var property = EdmProperty.Primitive("P", primitiveType);

            Assert.Same(primitiveType, property.PrimitiveType);
        }

        [Fact]
        public void EnumType_returns_type_when_enum_property()
        {
            var enumType = new EnumType();

            var property = EdmProperty.Enum("P", enumType);

            Assert.Same(enumType, property.EnumType);
        }

        [Fact]
        public void UnderlyingPrimitiveType_returns_type_when_underlying_primitive_property()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32);

            var property = EdmProperty.Primitive("P", primitiveType);

            Assert.Same(primitiveType, property.UnderlyingPrimitiveType);

            var enumType = new EnumType();

            property = EdmProperty.Enum("P", enumType);

            Assert.Same(primitiveType, property.UnderlyingPrimitiveType);

            var complexType = new ComplexType();

            property = EdmProperty.Complex("P", complexType);

            Assert.Null(property.UnderlyingPrimitiveType);
        }

        [Fact]
        public void Can_get_and_set_facet_wrappers()
        {
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            property.ConcurrencyMode = ConcurrencyMode.Fixed;
            property.CollectionKind = CollectionKind.List;

            Assert.Equal(ConcurrencyMode.Fixed, property.ConcurrencyMode);
            Assert.Equal(CollectionKind.List, property.CollectionKind);

            property.MaxLength = 42;

            Assert.Equal(42, property.MaxLength.Value);

            property.IsMaxLength = true;

            Assert.Null(property.MaxLength);
            Assert.True(property.IsMaxLength);

            property.IsFixedLength = true;
            property.IsUnicode = true;

            Assert.True(property.IsFixedLength.Value);
            Assert.True(property.IsUnicode.Value);

            property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Decimal));

            property.Precision = 42;
            property.Scale = 42;

            Assert.Equal(42, property.Precision.Value);
            Assert.Equal(42, property.Scale.Value);
        }
    }
}
