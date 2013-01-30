// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.UnitTests
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Mappers;
    using System.Linq;
    using Moq;
    using Xunit;

    public sealed class PropertyMapperTests
    {
        [Fact]
        public void Map_should_map_complex_type_properties()
        {
            var complexType = new ComplexType("C");
            var mappingContext
                = new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), new EdmModel(DataSpace.CSpace));

            new PropertyMapper(new TypeMapper(mappingContext))
                .Map(new MockPropertyInfo(typeof(int), "Foo"), complexType, () => new ComplexTypeConfiguration(typeof(object)));

            Assert.Equal(1, complexType.Properties.Count);

            var property = complexType.Properties.SingleOrDefault(p => p.Name == "Foo");

            Assert.Equal(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32), property.PrimitiveType);
        }

        [Fact]
        public void Map_should_map_entity_navigation_properties()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = new EntityType();
            model.AddEntitySet("Source", entityType);
            var mappingContext
                = new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model);

            new PropertyMapper(new TypeMapper(mappingContext))
                .Map(new MockPropertyInfo(new MockType(), "Foo"), entityType, () => new EntityTypeConfiguration(typeof(object)));

            Assert.Equal(1, entityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void Map_should_map_complex_properties()
        {
            var mockComplexType = new MockType();
            var mockModelConfiguration = new Mock<ModelConfiguration>();
            mockModelConfiguration
                .Setup(m => m.IsComplexType(mockComplexType)).Returns(true);
            mockModelConfiguration
                .Setup(m => m.GetStructuralTypeConfiguration(mockComplexType))
                .Returns(new Mock<StructuralTypeConfiguration>().Object);
            var entityType = new EntityType();
            var mappingContext
                = new MappingContext(mockModelConfiguration.Object, new ConventionsConfiguration(), new EdmModel(DataSpace.CSpace));

            new PropertyMapper(new TypeMapper(mappingContext))
                .Map(new MockPropertyInfo(mockComplexType, "Foo"), entityType, () => new EntityTypeConfiguration(typeof(object)));

            Assert.Equal(0, entityType.DeclaredNavigationProperties.Count);
            Assert.Equal(1, entityType.DeclaredProperties.Count);
            Assert.NotNull(entityType.DeclaredProperties.Single().ComplexType);
        }

        [Fact]
        public void Map_should_set_correct_name_and_type()
        {
            var entityType = new EntityType();
            var mappingContext
                = new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), new EdmModel(DataSpace.CSpace));

            new PropertyMapper(new TypeMapper(mappingContext))
                .Map(new MockPropertyInfo(typeof(int), "Foo"), entityType, () => new EntityTypeConfiguration(typeof(object)));

            Assert.Equal(1, entityType.DeclaredProperties.Count);

            var property = entityType.GetDeclaredPrimitiveProperties().SingleOrDefault(p => p.Name == "Foo");

            Assert.Equal(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32), property.PrimitiveType);
        }

        [Fact]
        public void Map_should_set_correct_nullability()
        {
            var entityType = new EntityType();
            var mappingContext
                = new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), new EdmModel(DataSpace.CSpace));

            new PropertyMapper(new TypeMapper(mappingContext))
                .Map(
                    new MockPropertyInfo(typeof(int?), "Foo"),
                    entityType, () => new EntityTypeConfiguration(typeof(object)));

            var property = entityType.GetDeclaredPrimitiveProperties().SingleOrDefault(p => p.Name == "Foo");

            Assert.Equal(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32), property.PrimitiveType);
            Assert.Equal(true, property.Nullable);

            new PropertyMapper(new TypeMapper(mappingContext))
                .Map(
                    new MockPropertyInfo(typeof(int), "Bar"),
                    entityType, () => new EntityTypeConfiguration(typeof(object)));

            property = entityType.GetDeclaredPrimitiveProperties().SingleOrDefault(p => p.Name == "Bar");

            Assert.Equal(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32), property.PrimitiveType);
            Assert.Equal(false, property.Nullable);
        }
    }
}
