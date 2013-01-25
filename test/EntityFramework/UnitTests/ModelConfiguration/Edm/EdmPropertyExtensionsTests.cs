// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.UnitTests
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public sealed class EdmPropertyExtensionsTests
    {
        [Fact]
        public void GetStoreGeneratedPattern_should_return_null_when_not_set()
        {
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            var storeGeneratedPattern = property.GetStoreGeneratedPattern();

            Assert.Null(storeGeneratedPattern);
        }

        [Fact]
        public void SetStoreGeneratedPattern_should_create_annotation_and_add_to_property_facets()
        {
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            property.SetStoreGeneratedPattern(StoreGeneratedPattern.Computed);

            var storeGeneratedPattern = property.GetStoreGeneratedPattern();

            Assert.NotNull(storeGeneratedPattern);
            Assert.Equal(StoreGeneratedPattern.Computed, storeGeneratedPattern);
        }

        [Fact]
        public void SetStoreGeneratedPattern_should_update_existing_annotation()
        {
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            property.SetStoreGeneratedPattern(StoreGeneratedPattern.Computed);
            property.SetStoreGeneratedPattern(StoreGeneratedPattern.None);

            var storeGeneratedPattern = property.GetStoreGeneratedPattern();

            Assert.NotNull(storeGeneratedPattern);
            Assert.Equal(StoreGeneratedPattern.None, storeGeneratedPattern);
        }

        [Fact]
        public void Can_get_and_set_configuration_annotation()
        {
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            property.SetConfiguration(42);

            Assert.Equal(42, property.GetConfiguration());
        }

        [Fact]
        public void AsPrimitive_should_create_property_type_and_facets()
        {
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            Assert.NotNull(property.TypeUsage);
        }

        [Fact]
        public void AsComplex_should_create_property_type()
        {
            var property = EdmProperty.Complex("P", new ComplexType("C"));

            Assert.NotNull(property.TypeUsage);
            Assert.True(property.IsComplexType);
            Assert.Equal(false, property.Nullable);
        }

        [Fact]
        public void Enum_should_create_property_type()
        {
            var property = EdmProperty.Enum("P", new EnumType());

            Assert.NotNull(property.TypeUsage);
            Assert.True(property.IsEnumType);
        }

        [Fact]
        public void HasStoreGeneratedPattern_should_return_true_when_not_null_or_none()
        {
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            Assert.False(property.HasStoreGeneratedPattern());

            property.SetStoreGeneratedPattern(StoreGeneratedPattern.None);

            Assert.False(property.HasStoreGeneratedPattern());

            property.SetStoreGeneratedPattern(StoreGeneratedPattern.Computed);

            Assert.True(property.HasStoreGeneratedPattern());
        }
    }
}
