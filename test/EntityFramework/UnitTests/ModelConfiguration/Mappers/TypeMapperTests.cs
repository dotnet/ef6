// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Mappers;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;
    using Moq;
    using SimpleModel;
    using Xunit;

    public sealed class TypeMapperTests
    {
        [Fact]
        public void MapEntityType_should_not_map_invalid_structural_type()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));

            Assert.Null(typeMapper.MapEntityType(typeof(string)));
        }

        [Fact]
        public void MapComplexType_should_not_map_invalid_structural_type()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));

            Assert.Null(typeMapper.MapComplexType(typeof(string)));
        }

        [Fact]
        public void MapEntityType_should_not_map_ignored_type()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var mockModelConfiguration = new Mock<ModelConfiguration>();
            var typeMapper = new TypeMapper(new MappingContext(mockModelConfiguration.Object, new ConventionsConfiguration(), model));
            mockModelConfiguration.Setup(m => m.IsIgnoredType(typeof(AType1))).Returns(true);

            var entityType = typeMapper.MapEntityType(typeof(AType1));

            Assert.Null(entityType);
        }

        [Fact]
        public void MapComplexType_should_not_map_ignored_type()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var mockModelConfiguration = new Mock<ModelConfiguration>();
            var typeMapper = new TypeMapper(new MappingContext(mockModelConfiguration.Object, new ConventionsConfiguration(), model));
            mockModelConfiguration.Setup(m => m.IsIgnoredType(typeof(AType1))).Returns(true);

            var complexType = typeMapper.MapComplexType(typeof(AType1));

            Assert.Null(complexType);
        }

        [Fact]
        public void MapEntityType_should_not_bring_in_base_class_by_default()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));

            var entityType = typeMapper.MapEntityType(typeof(AType3));

            Assert.NotNull(entityType);
            Assert.Null(entityType.BaseType);
            Assert.Equal(1, model.EntityTypes.Count());
            Assert.Equal(1, model.Containers.Single().EntitySets.Count);
            Assert.Equal("AType3", model.GetEntitySet(entityType).Name);
        }

        public class AType3 : BType3
        {
        }

        public class BType3
        {
        }

        [Fact]
        public void MapEntityType_should_bring_in_derived_types_from_the_same_assembly()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));

            typeMapper.MapEntityType(typeof(BType3));

            Assert.Equal(2, model.EntityTypes.Count());
            Assert.Equal(1, model.Containers.Single().EntitySets.Count);
        }

        [Fact]
        public void MapEntityType_should_not_try_and_bring_in_derived_types_from_sealed_class()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));

            typeMapper.MapEntityType(typeof(AType2));

            Assert.Equal(1, model.EntityTypes.Count());
            Assert.Equal(1, model.Containers.Single().EntitySets.Count);
        }

        public sealed class AType2
        {
        }

        [Fact]
        public void MapEntityType_should_bring_in_derived_types_from_discovered_assemblies()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));

            Assert.NotSame(typeof(AType9).Assembly(), typeof(Product).Assembly());

            typeMapper.MapEntityType(typeof(CType9));

            Assert.Equal(1, model.EntityTypes.Count());
            Assert.Equal(1, model.Containers.Single().EntitySets.Count);

            typeMapper.MapEntityType(typeof(ExtraEntity));

            Assert.Equal(3, model.EntityTypes.Count());
            Assert.Equal(2, model.Containers.Single().EntitySets.Count);
        }

        public class AType9 : ExtraEntity
        {
        }

        public class CType9
        {
        }

        [Fact]
        public void MapEntityType_should_bring_in_derived_types_from_known_assemblies()
        {
            var mockModelConfiguration = new Mock<ModelConfiguration>();
            mockModelConfiguration
                .Setup(m => m.GetStructuralTypeConfiguration(It.IsAny<Type>()))
                .Returns(new Mock<StructuralTypeConfiguration>().Object);
            var model = new EdmModel(DataSpace.CSpace);

            Assert.NotSame(typeof(AType9).Assembly(), typeof(Product).Assembly());

            mockModelConfiguration.SetupGet(m => m.ConfiguredTypes).Returns(new[] { typeof(AType9) });

            new TypeMapper(new MappingContext(mockModelConfiguration.Object, new ConventionsConfiguration(), model)).MapEntityType(
                typeof(ExtraEntity));

            Assert.Equal(2, model.EntityTypes.Count());
            Assert.Equal(1, model.Containers.Single().EntitySets.Count);
        }

        [Fact]
        public void MapEntityType_should_throw_for_new_type_if_entity_type_with_same_simple_name_already_used()
        {
            var model = new EdmModel(DataSpace.CSpace);

            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));

            Assert.NotNull(typeMapper.MapEntityType(typeof(AType1)));

            Assert.Equal(
                Strings.SimpleNameCollision(typeof(Outer1.AType1).FullName, typeof(AType1).FullName, "AType1"),
                Assert.Throws<NotSupportedException>(() => typeMapper.MapEntityType(typeof(Outer1.AType1))).Message);
        }

        public class Outer1
        {
            public class AType1
            {
            }
        }

        [Fact]
        public void MapEntityType_should_throw_for_new_type_if_enum_type_with_same_simple_name_already_used()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));

            Assert.NotNull(typeMapper.MapEnumType(typeof(AnEnum1)));

            Assert.Equal(
                Strings.SimpleNameCollision(typeof(Outer7.AnEnum1).FullName, typeof(AnEnum1).FullName, "AnEnum1"),
                Assert.Throws<NotSupportedException>(() => typeMapper.MapEntityType(typeof(Outer7.AnEnum1))).Message);
        }

        public class Outer7
        {
            public class AnEnum1
            {
            }
        }

        [Fact]
        public void MapEntityType_should_throw_for_new_type_if_complex_type_with_same_simple_name_already_used()
        {
            var model = new EdmModel(DataSpace.CSpace);

            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.ComplexType(typeof(AType6));

            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            Assert.NotNull(typeMapper.MapComplexType(typeof(AType6)));

            Assert.Equal(
                Strings.SimpleNameCollision(typeof(Outer6.AType6).FullName, typeof(AType6).FullName, "AType6"),
                Assert.Throws<NotSupportedException>(() => typeMapper.MapEntityType(typeof(Outer6.AType6))).Message);
        }

        public class AType6
        {
        }

        public class Outer6
        {
            public class AType6
            {
            }
        }
        
        [Fact]
        public void MapComplexType_should_throw_for_new_type_if_complex_type_with_same_simple_name_already_used()
        {
            var model = new EdmModel(DataSpace.CSpace);

            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.ComplexType(typeof(AType6));
            modelConfiguration.ComplexType(typeof(Outer6.AType6));

            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            Assert.NotNull(typeMapper.MapComplexType(typeof(AType6)));

            Assert.Equal(
                Strings.SimpleNameCollision(typeof(Outer6.AType6).FullName, typeof(AType6).FullName, "AType6"),
                Assert.Throws<NotSupportedException>(() => typeMapper.MapComplexType(typeof(Outer6.AType6))).Message);
        }

        [Fact]
        public void MapComplexType_should_throw_for_new_type_if_entity_type_with_same_simple_name_already_used()
        {
            var model = new EdmModel(DataSpace.CSpace);

            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.ComplexType(typeof(Outer6.AType6));

            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            Assert.NotNull(typeMapper.MapEntityType(typeof(AType6)));

            Assert.Equal(
                Strings.SimpleNameCollision(typeof(Outer6.AType6).FullName, typeof(AType6).FullName, "AType6"),
                Assert.Throws<NotSupportedException>(() => typeMapper.MapComplexType(typeof(Outer6.AType6))).Message);
        }

        [Fact]
        public void MapComplexType_should_throw_for_new_type_if_enum_type_with_same_simple_name_already_used()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.ComplexType(typeof(Outer5.AType5));

            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            Assert.NotNull(typeMapper.MapEnumType(typeof(AType5)));

            Assert.Equal(
                Strings.SimpleNameCollision(typeof(Outer5.AType5).FullName, typeof(AType5).FullName, "AType5"),
                Assert.Throws<NotSupportedException>(() => typeMapper.MapComplexType(typeof(Outer5.AType5))).Message);
        }

        public enum AType5
        {
        }

        public class Outer5
        {
            public class AType5
            {
            }
        }
        
        [Fact]
        public void MapEnumType_should_should_throw_for_new_type_if_enum_type_with_same_simple_name_already_used()
        {
            var model = new EdmModel(DataSpace.CSpace);

            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));

            Assert.NotNull(typeMapper.MapEnumType(typeof(AType4)));

            Assert.Equal(
                Strings.SimpleNameCollision(typeof(Outer4.AType4).FullName, typeof(AType4).FullName, "AType4"),
                Assert.Throws<NotSupportedException>(() => typeMapper.MapEnumType(typeof(Outer4.AType4))).Message);
        }

        public enum AType4
        {
        }

        public class Outer4
        {
            public enum AType4
            {
            }
        }

        [Fact]
        public void MapEnumType_should_should_throw_for_new_type_if_complex_type_with_same_simple_name_already_used()
        {
            var model = new EdmModel(DataSpace.CSpace);

            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.ComplexType(typeof(Outer5.AType5));

            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            Assert.NotNull(typeMapper.MapComplexType(typeof(Outer5.AType5)));

            Assert.Equal(
                Strings.SimpleNameCollision(typeof(AType5).FullName, typeof(Outer5.AType5).FullName, "AType5"),
                Assert.Throws<NotSupportedException>(() => typeMapper.MapEnumType(typeof(AType5))).Message);
        }

        [Fact]
        public void MapEnumType_should_should_throw_for_new_type_if_entity_type_with_same_simple_name_already_used()
        {
            var model = new EdmModel(DataSpace.CSpace);

            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));

            Assert.NotNull(typeMapper.MapEntityType(typeof(Outer5.AType5)));

            Assert.Equal(
                Strings.SimpleNameCollision(typeof(AType5).FullName, typeof(Outer5.AType5).FullName, "AType5"),
                Assert.Throws<NotSupportedException>(() => typeMapper.MapEnumType(typeof(AType5))).Message);
        }

        [Fact]
        public void MapEntityType_should_correctly_map_properties_in_class_hierarchy()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));

            var entityType = typeMapper.MapEntityType(typeof(AType8));
            var baseEntityType = typeMapper.MapEntityType(typeof(BType8));

            Assert.Equal(1, baseEntityType.DeclaredProperties.Count);
            Assert.Same(baseEntityType, entityType.BaseType);
            Assert.Equal(1, entityType.DeclaredProperties.Count);
            Assert.Equal(1, model.Containers.Single().EntitySets.Count);
        }

        public class AType8 : BType8
        {
            public int Id { get; set; }
        }

        public class BType8
        {
            public string Baz { get; set; }
        }

        [Fact]
        public void MapEntityType_should_create_abstract_entity_when_clr_type_is_abstract()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));

            var entityType = typeMapper.MapEntityType(typeof(AType7));

            Assert.NotNull(entityType);
            Assert.True(entityType.Abstract);
        }

        public abstract class AType7
        {
        }

        [Fact]
        public void MapEntityType_should_set_namespace_when_provided_via_model_configuration()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var typeMapper
                = new TypeMapper(
                    new MappingContext(
                        new ModelConfiguration
                            {
                                ModelNamespace = "Bar"
                            },
                        new ConventionsConfiguration(), model));

            var entityType = typeMapper.MapEntityType(typeof(AType7));

            Assert.NotNull(entityType);
            Assert.Equal("Bar", entityType.NamespaceName);
        }

        [Fact]
        public void MapEntityType_should_create_entity_type_with_clr_type_name_and_add_to_model()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));

            var entityType = typeMapper.MapEntityType(typeof(AType1));

            Assert.NotNull(entityType);
            Assert.Same(entityType, model.GetEntityType("AType1"));
        }

        public class AType1
        {
        }

        [Fact]
        public void MapComplexType_should_create_complex_type_with_clr_type_name_and_add_to_model()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.ComplexType(typeof(AType1));
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            var complexType = typeMapper.MapComplexType(typeof(AType1));

            Assert.NotNull(complexType);
            Assert.Same(complexType, model.GetComplexType("AType1"));
        }

        [Fact]
        public void MapComplexType_should_set_namespace_when_provided_via_model_configuration()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var modelConfiguration = new ModelConfiguration
                                         {
                                             ModelNamespace = "Bar"
                                         };
            modelConfiguration.ComplexType(typeof(AType7));
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            var complexType = typeMapper.MapComplexType(typeof(AType7));

            Assert.NotNull(complexType);
            Assert.Equal("Bar", complexType.NamespaceName);
        }

        [Fact]
        public void MapEnumType_should_set_namespace_when_provided_via_model_configuration()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var modelConfiguration = new ModelConfiguration
                                         {
                                             ModelNamespace = "Bar"
                                         };
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            var enumType = typeMapper.MapEnumType(typeof(AnEnum1));

            Assert.NotNull(enumType);
            Assert.Equal("Bar", enumType.NamespaceName);
        }

        [Fact]
        public void MapEnumType_should_create_enum_type_with_clr_type_name_and_add_to_model()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var modelConfiguration = new ModelConfiguration();
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            var enumType = typeMapper.MapEnumType(typeof(AnEnum1));

            Assert.NotNull(enumType);
            Assert.Same(enumType, model.GetEnumType("AnEnum1"));
        }

        public enum AnEnum1
        {
        }

        [Fact]
        public void MapEntityType_should_create_entity_set_and_add_to_model()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));

            var entityType = typeMapper.MapEntityType(typeof(AType1));

            var entitySet = model.GetEntitySet(entityType);

            Assert.NotNull(entitySet);
            Assert.Same(entityType, entitySet.ElementType);
            Assert.Equal("AType1", entitySet.Name);
        }

        [Fact]
        public void MapEntityType_should_not_create_entity_type_if_type_already_exists()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var typeMapper = new TypeMapper(new MappingContext(new ModelConfiguration(), new ConventionsConfiguration(), model));

            typeMapper.MapEntityType(typeof(AType1));
            typeMapper.MapEntityType(typeof(AType1));

            Assert.Equal(1, model.EntityTypes.Count());
        }

        [Fact]
        public void MapEntityType_should_only_map_public_instance_read_write_primitive_properties()
        {
            var modelConfiguration = new ModelConfiguration();
            var model = new EdmModel(DataSpace.CSpace);
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            var entityType = typeMapper.MapEntityType(
                typeof(MapEntityType_should_only_map_public_instance_read_write_primitive_properties_fixture));

            Assert.Equal(2, entityType.DeclaredProperties.Count);
            Assert.Equal(3, entityType.DeclaredNavigationProperties.Count);
        }

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

        [Fact]
        public void MapEntityType_should_recognize_StoreIgnore()
        {
            var type = typeof(TypeMapper_EntityWithStoreIgnore);
            var model = new EdmModel(DataSpace.CSpace);
            var modelConfiguration = new ModelConfiguration();
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            typeMapper.MapEntityType(type);

            Assert.True(modelConfiguration.IsIgnoredType(type));
            Assert.Equal(0, model.EntityTypes.Count());
            Assert.Equal(0, model.ComplexTypes.Count());
        }

        [Fact]
        public void MapComplexType_should_recognize_StoreIgnore()
        {
            var type = typeof(TypeMapper_EntityWithStoreIgnore);
            var model = new EdmModel(DataSpace.CSpace);
            var modelConfiguration = new ModelConfiguration();
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            typeMapper.MapComplexType(type);

            Assert.True(modelConfiguration.IsIgnoredType(type));
            Assert.Equal(0, model.EntityTypes.Count());
            Assert.Equal(0, model.ComplexTypes.Count());
        }

        [NotMapped]
        internal class TypeMapper_EntityWithStoreIgnore
        {
            public int Id { get; set; }
        }

        [Fact]
        public void MapEntityType_should_recognize_StoreInline()
        {
            var type = typeof(TypeMapper_EntityWithStoreInline);
            var model = new EdmModel(DataSpace.CSpace);
            var modelConfiguration = new ModelConfiguration();
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            typeMapper.MapEntityType(type);

            Assert.True(modelConfiguration.IsComplexType(type));
            Assert.Equal(0, model.EntityTypes.Count());
            Assert.Equal(0, model.ComplexTypes.Count());
        }

        [Fact]
        public void MapComplexType_should_recognize_StoreInline()
        {
            var type = typeof(TypeMapper_EntityWithStoreInline);
            var model = new EdmModel(DataSpace.CSpace);
            var modelConfiguration = new ModelConfiguration();
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            typeMapper.MapComplexType(type);

            Assert.True(modelConfiguration.IsComplexType(type));
            Assert.Equal(0, model.EntityTypes.Count());
            Assert.Equal(1, model.ComplexTypes.Count());
        }

        [Fact]
        public void MapEntityType_with_configured_Entity_should_throw_with_StoreInline()
        {
            var type = typeof(TypeMapper_EntityWithStoreInline);
            var model = new EdmModel(DataSpace.CSpace);
            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.Entity(typeof(TypeMapper_EntityWithStoreInline));
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            Assert.Equal(
                Strings.ComplexTypeConfigurationMismatch(type.Name),
                Assert.Throws<InvalidOperationException>(() => typeMapper.MapEntityType(type)).Message);
        }

        [ComplexType]
        internal class TypeMapper_EntityWithStoreInline
        {
            public int Id { get; set; }
        }

        [Fact]
        public void MapEntityType_recognizes_TableName()
        {
            var type = typeof(TypeMapper_EntityWithTableName);
            var model = new EdmModel(DataSpace.CSpace);
            var modelConfiguration = new ModelConfiguration();
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            typeMapper.MapEntityType(type);

            Assert.False(modelConfiguration.IsComplexType(type));
            Assert.Equal(1, model.EntityTypes.Count());
            Assert.Equal(0, model.ComplexTypes.Count());
            Assert.Equal("Foo", modelConfiguration.Entity(typeof(TypeMapper_EntityWithTableName)).GetTableName().Name);
        }

        [Table("Foo")]
        internal class TypeMapper_EntityWithTableName
        {
            public int Id { get; set; }
        }

        [Fact]
        public void MapEntityType_should_recognize_StoreIgnore_over_StoreInline()
        {
            var type = typeof(TypeMapper_EntityWithStoreInlineAndStoreIgnore);
            var model = new EdmModel(DataSpace.CSpace);
            var modelConfiguration = new ModelConfiguration();
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            typeMapper.MapEntityType(type);

            Assert.True(modelConfiguration.IsIgnoredType(type));
            Assert.Equal(0, model.EntityTypes.Count());
            Assert.Equal(0, model.ComplexTypes.Count());
        }

        [Fact]
        public void MapComplexType_should_recognize_StoreIgnore_over_StoreInline()
        {
            var type = typeof(TypeMapper_EntityWithStoreInlineAndStoreIgnore);
            var model = new EdmModel(DataSpace.CSpace);
            var modelConfiguration = new ModelConfiguration();
            var typeMapper = new TypeMapper(new MappingContext(modelConfiguration, new ConventionsConfiguration(), model));

            typeMapper.MapComplexType(type);

            Assert.True(modelConfiguration.IsIgnoredType(type));
            Assert.Equal(0, model.EntityTypes.Count());
            Assert.Equal(0, model.ComplexTypes.Count());
        }

        [NotMapped]
        [ComplexType]
        internal class TypeMapper_EntityWithStoreInlineAndStoreIgnore
        {
            public int Id { get; set; }
        }
    }
}
