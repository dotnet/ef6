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
        public void TypeName_returns_edm_type_name()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32);

            var property = EdmProperty.Primitive("P", primitiveType);

            Assert.Equal("Int32", property.TypeName);
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

            property.StoreGeneratedPattern = StoreGeneratedPattern.Identity;

            Assert.Equal(StoreGeneratedPattern.Identity, property.StoreGeneratedPattern);
        }

        [Fact]
        public void Can_set_primitive_type_and_new_type_usage_is_create_with_facets()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String);

            var property = EdmProperty.Primitive("P", primitiveType);
            property.StoreGeneratedPattern = StoreGeneratedPattern.Computed;
            property.ConcurrencyMode = ConcurrencyMode.Fixed;
            property.MaxLength = 42;

            property.PrimitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary);

            Assert.Equal(StoreGeneratedPattern.Computed, property.StoreGeneratedPattern);
            Assert.Equal(ConcurrencyMode.Fixed, property.ConcurrencyMode);
            Assert.Equal(42, property.MaxLength);
        }

        [Fact]
        public void IsKeyMember_should_return_true_when_part_of_key()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32);

            var property = EdmProperty.Primitive("P", primitiveType);

            Assert.False(property.IsKeyMember);

            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            entityType.AddMember(property);

            Assert.False(property.IsKeyMember);

            entityType.AddKeyMember(property);

            Assert.True(property.IsKeyMember);
        }

        [Fact]
        public void IsMaxLengthConstant_returns_true_for_const_MaxLength_facet_and_value_cannot_be_changed()
        {
            var typeUsage =
                TypeUsage.Create(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String),
                    new[] { CreateConstFacet("MaxLength", PrimitiveTypeKind.Int32, 200) });

            var property = new EdmProperty("P", typeUsage);
            Assert.True(property.IsMaxLengthConstant);
            Assert.Equal(200, property.MaxLength);

            property.MaxLength = 300;
            Assert.Equal(200, property.MaxLength);
        }

        [Fact]
        public void IsMaxLengthConstant_returns_false_if_MaxLength_facet_not_present_and_value_is_null()
        {
            var typeUsage =
                TypeUsage.Create(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String), new Facet[0]);

            var property = new EdmProperty("P", typeUsage);
            Assert.False(property.IsMaxLengthConstant);
            Assert.Null(property.MaxLength);
        }

        [Fact]
        public void IsFixedLengthConstant_returns_true_for_const_FixedLength_facet_and_value_cannot_be_changed()
        {
            var typeUsage =
                TypeUsage.Create(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String),
                    new[] { CreateConstFacet("FixedLength", PrimitiveTypeKind.Boolean, true) });

            var property = new EdmProperty("P", typeUsage);
            Assert.True(property.IsFixedLengthConstant);
            Assert.Equal(true, property.IsFixedLength);

            property.IsFixedLength = false;
            Assert.Equal(true, property.IsFixedLength);
        }

        [Fact]
        public void IsFixedLengthConstant_returns_false_if_FixedLength_facet_not_present_and_value_is_null()
        {
            var typeUsage =
                TypeUsage.Create(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String), new Facet[0]);

            var property = new EdmProperty("P", typeUsage);
            Assert.False(property.IsFixedLengthConstant);
            Assert.Null(property.IsFixedLength);
        }

        [Fact]
        public void IsUnicodeConstant_returns_true_for_const_Unicode_facet_and_value_cannot_be_changed()
        {
            var typeUsage =
                TypeUsage.Create(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String),
                    new[] { CreateConstFacet("Unicode", PrimitiveTypeKind.Boolean, true) });

            var property = new EdmProperty("P", typeUsage);
            Assert.True(property.IsUnicodeConstant);
            Assert.Equal(true, property.IsUnicode);

            property.IsFixedLength = false;
            Assert.Equal(true, property.IsUnicode);
        }

        [Fact]
        public void IsUnicodeConstant_returns_false_if_Unicode_facet_not_present_and_value_is_null()
        {
            var typeUsage =
                TypeUsage.Create(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String), new Facet[0]);

            var property = new EdmProperty("P", typeUsage);
            Assert.False(property.IsUnicodeConstant);
            Assert.Null(property.IsUnicode);
        }

        [Fact]
        public void IsPrecisionConstant_returns_true_for_const_Precision_facet_and_value_cannot_be_changed()
        {
            var typeUsage =
                TypeUsage.Create(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String),
                    new[] { CreateConstFacet("Precision", PrimitiveTypeKind.Byte, (byte)10) });

            var property = new EdmProperty("P", typeUsage);
            Assert.True(property.IsPrecisionConstant);
            Assert.Equal(10, (byte)property.Precision);

            property.Precision = 15;
            Assert.Equal(10, (byte)property.Precision);
        }

        [Fact]
        public void IsPrecisionConstant_returns_false_if_Precision_facet_not_present_and_value_is_null()
        {
            var typeUsage =
                TypeUsage.Create(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String), new Facet[0]);

            var property = new EdmProperty("P", typeUsage);
            Assert.False(property.IsPrecisionConstant);
            Assert.Null(property.Precision);
        }

        [Fact]
        public void IsScaleConstant_returns_true_for_const_Scale_facet_and_value_cannot_be_changed()
        {
            var typeUsage =
                TypeUsage.Create(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String),
                    new[] { CreateConstFacet("Scale", PrimitiveTypeKind.Byte, (byte)10) });

            var property = new EdmProperty("P", typeUsage);
            Assert.True(property.IsScaleConstant);
            Assert.Equal(10, (byte)property.Scale);

            property.Scale = 15;
            Assert.Equal(10, (byte)property.Scale);
        }

        [Fact]
        public void IsScaleConstant_returns_false_if_Scale_facet_not_present_and_value_is_null()
        {
            var typeUsage =
                TypeUsage.Create(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String), new Facet[0]);

            var property = new EdmProperty("P", typeUsage);
            Assert.False(property.IsScaleConstant);
            Assert.Null(property.Scale);
        }

        private static Facet CreateConstFacet(string facetName, PrimitiveTypeKind facetTypeKind, object value)
        {
            return
                Facet.Create(
                    new FacetDescription(
                        facetName,
                        PrimitiveType.GetEdmPrimitiveType(facetTypeKind),
                        null,
                        null,
                        value,
                        true,
                        null),
                    value);
        }
    }
}
