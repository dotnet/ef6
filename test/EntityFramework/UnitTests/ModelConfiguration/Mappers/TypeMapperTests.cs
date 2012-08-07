// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.UnitTests
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Mappers;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Reflection;
    using Moq;
    using Xunit;

    public sealed class TypeMapperTests
    {
        [Fact]
        public void MapEntityType_should_not_map_invalid_structural_type()
        {
            var model = new EdmModel().Initialize();
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));

            Assert.Null(typeMapper.MapEntityType(typeof(string)));
        }

        [Fact]
        public void MapComplexType_should_not_map_invalid_structural_type()
        {
            var model = new EdmModel().Initialize();
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));

            Assert.Null(typeMapper.MapComplexType(typeof(string)));
        }

        [Fact]
        public void MapEntityType_should_not_map_ignored_type()
        {
            var model = new EdmModel().Initialize();
            var mockModelConfiguration = new Mock<ModelConfiguration>();
            var typeMapper = new TypeMapper(new MappingContext(mockModelConfiguration.Object, new ConventionsConfiguration(), model));
            var mockType = new MockType("Foo");
            mockModelConfiguration.Setup(m => m.IsIgnoredType(mockType)).Returns(true);

            var entityType = typeMapper.MapEntityType(mockType);

            Assert.Null(entityType);
        }

        [Fact]
        public void MapComplexType_should_not_map_ignored_type()
        {
            var model = new EdmModel().Initialize();
            var mockModelConfiguration = new Mock<ModelConfiguration>();
            var typeMapper = new TypeMapper(new MappingContext(mockModelConfiguration.Object, new ConventionsConfiguration(), model));
            var mockType = new MockType("Foo");
            mockModelConfiguration.Setup(m => m.IsIgnoredType(mockType)).Returns(true);

            var complexType = typeMapper.MapComplexType(mockType);

            Assert.Null(complexType);
        }

        [Fact]
        public void MapEntityType_should_not_bring_in_base_class_by_default()
        {
            var model = new EdmModel().Initialize();
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));
            var mockType = new MockType("Bar").BaseType(new MockType("Foo"));

            var entityType = typeMapper.MapEntityType(mockType);

            Assert.NotNull(entityType);
            Assert.Null(entityType.BaseType);
            Assert.Equal(1, model.Namespaces.Single().EntityTypes.Count);
            Assert.Equal(1, model.Containers.Single().EntitySets.Count);
            Assert.Equal("Bar", model.GetEntitySet(entityType).Name);
        }

        [Fact]
        public void MapEntityType_should_bring_in_derived_types_from_the_same_assembly()
        {
            var model = new EdmModel().Initialize();
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));

            var mockType1 = new MockType("Foo");
            var mockType2 = new MockType("Bar").BaseType(mockType1);

            new MockAssembly(mockType1, mockType2);

            typeMapper.MapEntityType(mockType1);

            Assert.Equal(2, model.Namespaces.Single().EntityTypes.Count);
            Assert.Equal(1, model.Containers.Single().EntitySets.Count);
        }

        [Fact]
        public void MapEntityType_should_not_try_and_bring_in_derived_types_from_sealed_class()
        {
            var model = new EdmModel().Initialize();
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));

            var mockType1 = new MockType("Foo").TypeAttributes(TypeAttributes.Sealed);
            var mockType2 = new MockType("Bar").BaseType(mockType1);

            new MockAssembly(mockType1, mockType2);

            typeMapper.MapEntityType(mockType1);

            Assert.Equal(1, model.Namespaces.Single().EntityTypes.Count);
            Assert.Equal(1, model.Containers.Single().EntitySets.Count);
        }

        [Fact]
        public void MapEntityType_should_bring_in_derived_types_from_discovered_assemblies()
        {
            var model = new EdmModel().Initialize();
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));

            var mockType1 = new MockType("Foo");
            var mockType2 = new MockType("Bar").BaseType(mockType1);
            var mockType3 = new MockType("Baz");

            new MockAssembly(mockType1);
            new MockAssembly(mockType2, mockType3);

            typeMapper.MapEntityType(mockType3);

            Assert.Equal(1, model.Namespaces.Single().EntityTypes.Count);
            Assert.Equal(1, model.Containers.Single().EntitySets.Count);

            typeMapper.MapEntityType(mockType1);

            Assert.Equal(3, model.Namespaces.Single().EntityTypes.Count);
            Assert.Equal(2, model.Containers.Single().EntitySets.Count);
        }

        [Fact]
        public void MapEntityType_should_bring_in_derived_types_from_known_assemblies()
        {
            var mockModelConfiguration = new Mock<ModelConfiguration>();
            mockModelConfiguration
                .Setup(m => m.GetStructuralTypeConfiguration(It.IsAny<Type>()))
                .Returns(new Mock<StructuralTypeConfiguration>().Object);
            var model = new EdmModel().Initialize();
            var mockType1 = new MockType("Foo");
            var mockType2 = new MockType("Bar").BaseType(mockType1);

            new MockAssembly(mockType1);
            new MockAssembly(mockType2);

            mockModelConfiguration.SetupGet(m => m.ConfiguredTypes).Returns(new[] { mockType2.Object });

            new TypeMapper(new MappingContext(mockModelConfiguration.Object, new ConventionsConfiguration(), model)).MapEntityType(
                mockType1);

            Assert.Equal(2, model.Namespaces.Single().EntityTypes.Count);
            Assert.Equal(1, model.Containers.Single().EntitySets.Count);
        }

        [Fact]
        public void MapEntityType_should_ignore_new_type_if_type_name_already_used()
        {
            var model = new EdmModel().Initialize();

            var mockType1 = new MockType("Foo");
            var mockType2 = new MockType("Foo");

            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));

            Assert.NotNull(typeMapper.MapEntityType(mockType1));
            Assert.Null(typeMapper.MapEntityType(mockType2));
        }

        [Fact]
        public void MapComplexType_should_ignore_new_type_if_type_name_already_used()
        {
            var model = new EdmModel().Initialize();

            var mockType1 = new MockType("Foo");
            var mockType2 = new MockType("Foo");

            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.ComplexType(mockType1);
            modelConfiguration.ComplexType(mockType2);

            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            Assert.NotNull(typeMapper.MapComplexType(mockType1));
            Assert.Null(typeMapper.MapComplexType(mockType2));
        }

        [Fact]
        public void MapEnumType_should_ignore_new_type_if_type_name_already_used()
        {
            var model = new EdmModel().Initialize();

            var mockType1 = new MockType("Foo");
            var mockType2 = new MockType("Foo");

            mockType1.SetupGet(t => t.IsEnum).Returns(true);
            mockType1.Setup(t => t.GetEnumUnderlyingType()).Returns(typeof(int));
            mockType1.Setup(t => t.GetEnumNames()).Returns(new string[] { });
            mockType1.Setup(t => t.GetEnumValues()).Returns(new int[] { });
            mockType2.SetupGet(t => t.IsEnum).Returns(true);

            var modelConfiguration = new ModelConfiguration();
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            Assert.NotNull(typeMapper.MapEnumType(mockType1));
            Assert.Null(typeMapper.MapEnumType(mockType2));
        }

        [Fact]
        public void MapEntityType_should_correctly_map_properties_in_class_hierachy()
        {
            var model = new EdmModel().Initialize();
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));
            var mockObjectType = new MockType("Object");
            var mockBaseType = new MockType("Foo").BaseType(mockObjectType).Property<int>("Id");
            var mockType = new MockType("Bar").BaseType(mockBaseType).Property<string>("Baz");

            new MockAssembly(mockObjectType, mockBaseType, mockType);

            var entityType = typeMapper.MapEntityType(mockType);
            var baseEntityType = typeMapper.MapEntityType(mockBaseType);

            Assert.Equal(1, baseEntityType.DeclaredProperties.Count);
            Assert.Same(baseEntityType, entityType.BaseType);
            Assert.Equal(1, entityType.DeclaredProperties.Count);
            Assert.Equal(1, model.Containers.Single().EntitySets.Count);
        }

        [Fact]
        public void MapEntityType_should_create_abstract_entity_when_clr_type_is_abstract()
        {
            var model = new EdmModel().Initialize();
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));
            var mockType = new MockType("Foo").TypeAttributes(TypeAttributes.Abstract);

            var entityType = typeMapper.MapEntityType(mockType);

            Assert.NotNull(entityType);
            Assert.True(entityType.IsAbstract);
        }

        [Fact]
        public void MapEntityType_should_create_entity_type_with_clr_type_name_and_add_to_model()
        {
            var model = new EdmModel().Initialize();
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));
            var mockType = new MockType("Foo");

            var entityType = typeMapper.MapEntityType(mockType);

            Assert.NotNull(entityType);
            Assert.Same(entityType, model.GetEntityType("Foo"));
        }

        [Fact]
        public void MapComplexType_should_create_complex_type_with_clr_type_name_and_add_to_model()
        {
            var model = new EdmModel().Initialize();
            var mockType = new MockType("Foo");
            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.ComplexType(mockType);
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            var complexType = typeMapper.MapComplexType(mockType);

            Assert.NotNull(complexType);
            Assert.Same(complexType, model.GetComplexType("Foo"));
        }

        [Fact]
        public void MapEnumType_should_create_enum_type_with_clr_type_name_and_add_to_model()
        {
            var model = new EdmModel().Initialize();
            var mockType = new MockType("Foo");

            mockType.SetupGet(t => t.IsEnum).Returns(true);
            mockType.Setup(t => t.GetEnumUnderlyingType()).Returns(typeof(int));
            mockType.Setup(t => t.GetEnumNames()).Returns(new string[] { });
            mockType.Setup(t => t.GetEnumValues()).Returns(new int[] { });

            var modelConfiguration = new ModelConfiguration();

            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            var enumType = typeMapper.MapEnumType(mockType);

            Assert.NotNull(enumType);
            Assert.Same(enumType, model.GetEnumType("Foo"));
        }

        [Fact]
        public void MapEntityType_should_create_entity_set_and_add_to_model()
        {
            var model = new EdmModel().Initialize();
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));
            var mockType = new MockType("Foo");

            var entityType = typeMapper.MapEntityType(mockType);

            var entitySet = model.GetEntitySet(entityType);

            Assert.NotNull(entitySet);
            Assert.Same(entityType, entitySet.ElementType);
            Assert.Equal("Foo", entitySet.Name);
        }

        [Fact]
        public void MapEntityType_should_not_create_entity_type_if_type_already_exists()
        {
            var model = new EdmModel().Initialize();
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));
            var mockType = new MockType("Foo");

            typeMapper.MapEntityType(mockType);
            typeMapper.MapEntityType(mockType);

            Assert.Equal(1, model.Namespaces.Single().EntityTypes.Count);
        }

        [Fact]
        public void MapEntityType_should_only_map_public_instance_read_write_primitive_properties()
        {
            var modelConfiguration = new ModelConfiguration();
            var model = new EdmModel().Initialize();
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            var entityType = typeMapper.MapEntityType(
                typeof(MapEntityType_should_only_map_public_instance_read_write_primitive_properties_fixture));

            Assert.Equal(2, entityType.DeclaredProperties.Count);
            Assert.Equal(3, entityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void MapEntityType_should_recognize_StoreIgnore()
        {
            var type = typeof(TypeMapper_EntityWithStoreIgnore);
            var model = new EdmModel().Initialize();
            var modelConfiguration = new ModelConfiguration();
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            typeMapper.MapEntityType(type);

            Assert.True(modelConfiguration.IsIgnoredType(type));
            Assert.Equal(0, model.Namespaces.Single().EntityTypes.Count);
            Assert.Equal(0, model.Namespaces.Single().ComplexTypes.Count);
        }

        [Fact]
        public void MapComplexType_should_recognize_StoreIgnore()
        {
            var type = typeof(TypeMapper_EntityWithStoreIgnore);
            var model = new EdmModel().Initialize();
            var modelConfiguration = new ModelConfiguration();
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            typeMapper.MapComplexType(type);

            Assert.True(modelConfiguration.IsIgnoredType(type));
            Assert.Equal(0, model.Namespaces.Single().EntityTypes.Count);
            Assert.Equal(0, model.Namespaces.Single().ComplexTypes.Count);
        }

        [Fact]
        public void MapEntityType_should_recognize_StoreInline()
        {
            var type = typeof(TypeMapper_EntityWithStoreInline);
            var model = new EdmModel().Initialize();
            var modelConfiguration = new ModelConfiguration();
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            typeMapper.MapEntityType(type);

            Assert.True(modelConfiguration.IsComplexType(type));
            Assert.Equal(0, model.Namespaces.Single().EntityTypes.Count);
            Assert.Equal(0, model.Namespaces.Single().ComplexTypes.Count);
        }

        [Fact]
        public void MapComplexType_should_recognize_StoreInline()
        {
            var type = typeof(TypeMapper_EntityWithStoreInline);
            var model = new EdmModel().Initialize();
            var modelConfiguration = new ModelConfiguration();
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            typeMapper.MapComplexType(type);

            Assert.True(modelConfiguration.IsComplexType(type));
            Assert.Equal(0, model.Namespaces.Single().EntityTypes.Count);
            Assert.Equal(1, model.Namespaces.Single().ComplexTypes.Count);
        }

        [Fact]
        public void MapEntityType_with_configured_Entity_should_throw_with_StoreInline()
        {
            var type = typeof(TypeMapper_EntityWithStoreInline);
            var model = new EdmModel().Initialize();
            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.Entity(typeof(TypeMapper_EntityWithStoreInline));
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            Assert.Equal(
                Strings.ComplexTypeConfigurationMismatch(type),
                Assert.Throws<InvalidOperationException>(() => typeMapper.MapEntityType(type)).Message);
        }

        [Fact]
        public void MapEntityType_recognizes_TableName()
        {
            var type = typeof(TypeMapper_EntityWithTableName);
            var model = new EdmModel().Initialize();
            var modelConfiguration = new ModelConfiguration();
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            typeMapper.MapEntityType(type);

            Assert.False(modelConfiguration.IsComplexType(type));
            Assert.Equal(1, model.Namespaces.Single().EntityTypes.Count);
            Assert.Equal(0, model.Namespaces.Single().ComplexTypes.Count);
            Assert.Equal("Foo", modelConfiguration.Entity(typeof(TypeMapper_EntityWithTableName)).GetTableName().Name);
        }

        [Fact]
        public void MapEntityType_should_recognize_StoreIgnore_over_StoreInline()
        {
            var type = typeof(TypeMapper_EntityWithStoreInlineAndStoreIgnore);
            var model = new EdmModel().Initialize();
            var modelConfiguration = new ModelConfiguration();
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            typeMapper.MapEntityType(type);

            Assert.True(modelConfiguration.IsIgnoredType(type));
            Assert.Equal(0, model.Namespaces.Single().EntityTypes.Count);
            Assert.Equal(0, model.Namespaces.Single().ComplexTypes.Count);
        }

        [Fact]
        public void MapComplexType_should_recognize_StoreIgnore_over_StoreInline()
        {
            var type = typeof(TypeMapper_EntityWithStoreInlineAndStoreIgnore);
            var model = new EdmModel().Initialize();
            var modelConfiguration = new ModelConfiguration();
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            typeMapper.MapComplexType(type);

            Assert.True(modelConfiguration.IsIgnoredType(type));
            Assert.Equal(0, model.Namespaces.Single().EntityTypes.Count);
            Assert.Equal(0, model.Namespaces.Single().ComplexTypes.Count);
        }
    }

    #region Test Fixtures

    internal class MapEntityType_should_only_map_public_instance_read_write_primitive_properties_fixture
    {
        // Positive props
        public int Public { get; set; }
        public string PrivateSetter { get; private set; }

        // Negative props
        private int PrivateReadWrite { get; set; }
        public static long Static { get; set; }

        public TimeSpan ReadOnly
        {
            get { return TimeSpan.Zero; }
        }

        public DateTime WriteOnly
        {
            set { ; }
        }

        // Positive navigation props
        public MapEntityType_related_entity_fixture ReferenceProp { get; set; }
        public ICollection<MapEntityType_related_entity_fixture> ReadWriteCollectionProp { get; set; }

        public ICollection<MapEntityType_related_entity_fixture> ReadOnlyCollectionProp
        {
            get { return new List<MapEntityType_related_entity_fixture>(); }
        }

        // Negative navigation props
        private MapEntityType_related_entity_fixture PrivateReferenceProp { get; set; }
        public static MapEntityType_related_entity_fixture StaticReferenceProp { get; set; }
        private ICollection<MapEntityType_related_entity_fixture> PrivateReadWriteCollectionProp { get; set; }
        public static ICollection<MapEntityType_related_entity_fixture> StaticReadWriteCollectionProp { get; set; }

        private ICollection<MapEntityType_related_entity_fixture> PrivateReadOnlyCollectionProp
        {
            get { return new List<MapEntityType_related_entity_fixture>(); }
        }

        public static ICollection<MapEntityType_related_entity_fixture> StaticReadOnlyCollectionProp
        {
            get { return new List<MapEntityType_related_entity_fixture>(); }
        }
    }

    internal class MapEntityType_related_entity_fixture
    {
    }

    [NotMapped]
    internal class TypeMapper_EntityWithStoreIgnore
    {
        public int Id { get; set; }
    }

    [ComplexType]
    internal class TypeMapper_EntityWithStoreInline
    {
        public int Id { get; set; }
    }

    [Table("Foo")]
    internal class TypeMapper_EntityWithTableName
    {
        public int Id { get; set; }
    }

    [ComplexType]
    [Table("Foo")]
    internal class TypeMapper_EntityWithStoreInlineAndTableName
    {
        public int Id { get; set; }
    }

    [NotMapped]
    [ComplexType]
    internal class TypeMapper_EntityWithStoreInlineAndStoreIgnore
    {
        public int Id { get; set; }
    }

    #endregion
}
