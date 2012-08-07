// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using Xunit;

    public sealed class EdmPropertyExtensionsTests
    {
        [Fact]
        public void GetStoreGeneratedPattern_should_return_null_when_not_set()
        {
            var property = new EdmProperty().AsPrimitive();

            var storeGeneratedPattern = property.GetStoreGeneratedPattern();

            Assert.Null(storeGeneratedPattern);
        }

        [Fact]
        public void SetStoreGeneratedPattern_should_create_annotation_and_add_to_property_facets()
        {
            var property = new EdmProperty().AsPrimitive();

            property.SetStoreGeneratedPattern(DbStoreGeneratedPattern.Computed);

            var storeGeneratedPattern = property.GetStoreGeneratedPattern();

            Assert.NotNull(storeGeneratedPattern);
            Assert.Equal(DbStoreGeneratedPattern.Computed, storeGeneratedPattern);
        }

        [Fact]
        public void SetStoreGeneratedPattern_should_update_existing_annotation()
        {
            var property = new EdmProperty().AsPrimitive();

            property.SetStoreGeneratedPattern(DbStoreGeneratedPattern.Computed);
            property.SetStoreGeneratedPattern(DbStoreGeneratedPattern.None);

            var storeGeneratedPattern = property.GetStoreGeneratedPattern();

            Assert.NotNull(storeGeneratedPattern);
            Assert.Equal(DbStoreGeneratedPattern.None, storeGeneratedPattern);
        }

        [Fact]
        public void Can_get_and_set_configuration_annotation()
        {
            var property = new EdmProperty();

            property.SetConfiguration(42);

            Assert.Equal(42, property.GetConfiguration());
        }

        [Fact]
        public void AsPrimitive_should_create_property_type_and_facets()
        {
            var property = new EdmProperty().AsPrimitive();

            Assert.NotNull(property.PropertyType);
            Assert.NotNull(property.PropertyType.PrimitiveTypeFacets);
        }

        [Fact]
        public void AsComplex_should_create_property_type()
        {
            var property = new EdmProperty().AsComplex(new EdmComplexType());

            Assert.NotNull(property.PropertyType);
            Assert.True(property.PropertyType.IsComplexType);
            Assert.Equal(false, property.PropertyType.IsNullable);
        }

        [Fact]
        public void AsEnum_should_create_property_type()
        {
            var property = new EdmProperty().AsEnum(new EdmEnumType());

            Assert.NotNull(property.PropertyType);
            Assert.True(property.PropertyType.IsEnumType);
        }
    }
}
