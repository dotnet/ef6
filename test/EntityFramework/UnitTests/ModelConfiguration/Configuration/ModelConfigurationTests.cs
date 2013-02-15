// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.UnitTests
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Services;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using Moq;
    using Xunit;

    public sealed class ModelConfigurationTests
    {
        [Fact]
        public void Configure_when_base_entity_mapped_to_function_should_map_sub_types_to_functions()
        {
            var modelConfiguration = new ModelConfiguration();

            var rootType = new MockType();
            var middleType = new MockType().BaseType(rootType);
            var leafType = new MockType().BaseType(middleType);

            modelConfiguration.Entity(rootType).MapToStoredProcedures();
            modelConfiguration.Entity(middleType);
            modelConfiguration.Entity(leafType);

            var model = new EdmModel(DataSpace.CSpace);

            var rootEntity = model.AddEntityType("Root");
            rootEntity.Annotations.SetClrType(rootType);

            var middleEntity = model.AddEntityType("Middle");
            middleEntity.Annotations.SetClrType(middleType);
            middleEntity.BaseType = rootEntity;

            var leafEntity = model.AddEntityType("Leaf");
            leafEntity.Annotations.SetClrType(leafType);
            leafEntity.BaseType = middleEntity;

            modelConfiguration.Configure(model);

            Assert.True(modelConfiguration.Entity(rootType).IsMappedToFunctions);
            Assert.True(modelConfiguration.Entity(middleType).IsMappedToFunctions);
            Assert.True(modelConfiguration.Entity(leafType).IsMappedToFunctions);
        }

        [Fact]
        public void Configure_should_throw_when_only_derived_type_mapped_to_functions()
        {
            var modelConfiguration = new ModelConfiguration();

            var baseType = new MockType("B");
            var derivedType = new MockType("D").BaseType(baseType);

            modelConfiguration.Entity(baseType);
            modelConfiguration.Entity(derivedType).MapToStoredProcedures();

            var model = new EdmModel(DataSpace.CSpace);

            var baseEntity = model.AddEntityType("Base");
            baseEntity.Annotations.SetClrType(baseType);

            var derivedEntity = model.AddEntityType("Derived");
            derivedEntity.Annotations.SetClrType(derivedType);
            derivedEntity.BaseType = baseEntity;

            Assert.Equal(
                Strings.BaseTypeNotMappedToFunctions("B", "D"),
                Assert.Throws<InvalidOperationException>(
                    () => modelConfiguration.Configure(model)).Message);
        }

        [Fact]
        public void Configure_should_not_throw_when_only_derived_type_mapped_to_function_but_base_is_abstract()
        {
            var modelConfiguration = new ModelConfiguration();

            var baseType = new MockType("B");
            var derivedType = new MockType("D").BaseType(baseType);

            modelConfiguration.Entity(baseType);
            modelConfiguration.Entity(derivedType).MapToStoredProcedures();

            var model = new EdmModel(DataSpace.CSpace);

            var baseEntity = model.AddEntityType("Base");
            baseEntity.Annotations.SetClrType(baseType);
            baseEntity.Abstract = true;
            
            var derivedEntity = model.AddEntityType("Derived");
            derivedEntity.Annotations.SetClrType(derivedType);
            derivedEntity.BaseType = baseEntity;

            modelConfiguration.Configure(model);

            Assert.True(modelConfiguration.Entity(baseType).IsMappedToFunctions);
            Assert.True(modelConfiguration.Entity(derivedType).IsMappedToFunctions);
        }

        [Fact]
        public void Configure_should_configure_default_schema_on_functions()
        {
            var modelConfiguration
                = new ModelConfiguration
                      {
                          DefaultSchema = "foo"
                      };

            var databaseMetadata = new EdmModel(DataSpace.SSpace);
            databaseMetadata.AddFunction("F", new EdmFunctionPayload());

            var databaseMapping
                = new DbDatabaseMapping().Initialize(
                    new EdmModel(DataSpace.CSpace),
                    databaseMetadata);

            modelConfiguration.Configure(databaseMapping, ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Equal("foo", databaseMapping.Database.Functions.Single().Schema);
        }

        [Fact]
        public void Can_get_and_set_model_namespace()
        {
            var modelConfiguration = new ModelConfiguration();

            Assert.Null(modelConfiguration.ModelNamespace);

            modelConfiguration.ModelNamespace = "Foo";

            Assert.Equal("Foo", modelConfiguration.ModelNamespace);
        }

        [Fact]
        public void Configure_should_configure_default_default_schema()
        {
            var modelConfiguration = new ModelConfiguration();

            var databaseMetadata = new EdmModel(DataSpace.CSpace);
            databaseMetadata.AddEntitySet("ES", new EntityType());

            var databaseMapping
                = new DbDatabaseMapping().Initialize(
                    new EdmModel(DataSpace.CSpace),
                    databaseMetadata);

            modelConfiguration.Configure(databaseMapping, ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Equal("dbo", databaseMapping.Database.GetEntitySets().Single().Schema);
        }

        [Fact]
        public void Configure_should_configure_default_schema()
        {
            var modelConfiguration
                = new ModelConfiguration
                      {
                          DefaultSchema = "foo"
                      };

            var databaseMetadata = new EdmModel(DataSpace.CSpace);
            databaseMetadata.AddEntitySet("ES", new EntityType());

            var databaseMapping
                = new DbDatabaseMapping().Initialize(
                    new EdmModel(DataSpace.CSpace),
                    databaseMetadata);

            modelConfiguration.Configure(databaseMapping, ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Equal("foo", databaseMapping.Database.GetEntitySets().Single().Schema);
        }

        [Fact]
        public void GetConfiguredProperties_should_return_all_configured_properties()
        {
            var modelConfiguration = new ModelConfiguration();
            var mockType = new MockType();
            var mockPropertyInfo = new MockPropertyInfo(typeof(string), "S");

            Assert.False(modelConfiguration.GetConfiguredProperties(mockType).Any());

            modelConfiguration.Entity(mockType).Property(new PropertyPath(mockPropertyInfo));

            Assert.Same(mockPropertyInfo.Object, modelConfiguration.GetConfiguredProperties(mockType).Single());
        }

        [Fact]
        public void IsIgnoredProperty_should_return_true_if_property_is_ignored()
        {
            var modelConfiguration = new ModelConfiguration();
            var mockType = new MockType();
            var mockPropertyInfo = new MockPropertyInfo(typeof(string), "S");

            Assert.False(modelConfiguration.IsIgnoredProperty(mockType, mockPropertyInfo));

            modelConfiguration.Entity(mockType).Ignore(mockPropertyInfo);

            Assert.True(modelConfiguration.IsIgnoredProperty(mockType, mockPropertyInfo));
        }

        [Fact]
        public void Configure_should_configure_active_entities_and_complex_types()
        {
            var mockEntityType = new MockType();
            var mockComplexType = new MockType();

            var model = new EdmModel(DataSpace.CSpace);
            var entityType = model.AddEntityType("E");

            entityType.Annotations.SetClrType(mockEntityType);
            var complexType = model.AddComplexType("C");

            complexType.Annotations.SetClrType(mockComplexType);

            var modelConfiguration = new ModelConfiguration();
            var mockComplexTypeConfiguration = new Mock<ComplexTypeConfiguration>(mockComplexType.Object);
            var mockEntityTypeConfiguration = new Mock<EntityTypeConfiguration>(mockEntityType.Object);

            modelConfiguration.Add(mockComplexTypeConfiguration.Object);
            modelConfiguration.Add(mockEntityTypeConfiguration.Object);

            modelConfiguration.Configure(model);

            mockComplexTypeConfiguration.Verify(c => c.Configure(complexType));
            mockEntityTypeConfiguration.Verify(c => c.Configure(entityType, model));
        }

        [Fact]
        public void ConfiguredTypes_returns_all_known_types()
        {
            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.Entity(new MockType());
            modelConfiguration.ComplexType(new MockType());
            modelConfiguration.Ignore(new MockType());

            Assert.Equal(3, modelConfiguration.ConfiguredTypes.Count());
        }

        [Fact]
        public void Entities_returns_only_configured_non_ignored_entity_types()
        {
            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.ComplexType(new MockType());
            var mockEntityType = new MockType();
            modelConfiguration.Entity(mockEntityType);
            var mockIgnoredEntityType = new MockType();
            modelConfiguration.Entity(mockIgnoredEntityType);
            modelConfiguration.Ignore(mockIgnoredEntityType);

            Assert.Same(mockEntityType.Object, modelConfiguration.Entities.Single());
        }

        [Fact]
        public void ComplexTypes_returns_only_configured_non_ignored_complex_types()
        {
            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.Entity(new MockType());
            var mockComplexType = new MockType();
            modelConfiguration.ComplexType(mockComplexType);
            var mockIgnoredComplexType = new MockType();
            modelConfiguration.ComplexType(mockIgnoredComplexType);
            modelConfiguration.Ignore(mockIgnoredComplexType);

            Assert.Same(mockComplexType.Object, modelConfiguration.ComplexTypes.Single());
        }

        [Fact]
        public void Adding_multiple_entity_configurations_should_throw()
        {
            var modelConfiguration = new ModelConfiguration();
            var entityTypeConfiguration = new Mock<EntityTypeConfiguration>(typeof(object)).Object;
            modelConfiguration.Add(entityTypeConfiguration);

            Assert.Equal(
                Strings.DuplicateStructuralTypeConfiguration(typeof(object)),
                Assert.Throws<InvalidOperationException>(() => modelConfiguration.Add(entityTypeConfiguration)).Message);
        }

        [Fact]
        public void Adding_multiple_complex_type_configurations_should_throw()
        {
            var modelConfiguration = new ModelConfiguration();
            var complexTypeConfiguration = new Mock<ComplexTypeConfiguration>(typeof(object)).Object;
            modelConfiguration.Add(complexTypeConfiguration);

            Assert.Equal(
                Strings.DuplicateStructuralTypeConfiguration(typeof(object)),
                Assert.Throws<InvalidOperationException>(() => modelConfiguration.Add(complexTypeConfiguration)).Message);
        }

        [Fact]
        public void Adding_complex_type_and_entity_configurations_should_throw()
        {
            var modelConfiguration = new ModelConfiguration();
            var complexTypeConfiguration = new Mock<ComplexTypeConfiguration>(typeof(object)).Object;
            var entityTypeConfiguration = new Mock<EntityTypeConfiguration>(typeof(object)).Object;
            modelConfiguration.Add(complexTypeConfiguration);

            Assert.Equal(
                Strings.DuplicateStructuralTypeConfiguration(typeof(object)),
                Assert.Throws<InvalidOperationException>(() => modelConfiguration.Add(entityTypeConfiguration)).Message);
        }

        [Fact]
        public void Can_add_and_get_entity_configuration()
        {
            var modelConfiguration = new ModelConfiguration();
            var entityTypeConfiguration = new Mock<EntityTypeConfiguration>(typeof(object)).Object;
            modelConfiguration.Add(entityTypeConfiguration);

            Assert.Same(entityTypeConfiguration, modelConfiguration.Entity(typeof(object)));
        }

        [Fact]
        public void Can_add_and_get_complex_type_configuration()
        {
            var modelConfiguration = new ModelConfiguration();
            var complexTypeConfiguration = new Mock<ComplexTypeConfiguration>(typeof(object)).Object;
            modelConfiguration.Add(complexTypeConfiguration);

            Assert.Same(complexTypeConfiguration, modelConfiguration.ComplexType(typeof(object)));
        }

        [Fact]
        public void Entity_should_return_new_configuration_if_no_configuration_found()
        {
            var modelConfiguration = new ModelConfiguration();

            Assert.NotNull(modelConfiguration.Entity(typeof(object)));
        }

        [Fact]
        public void Entity_should_throw_when_configuration_is_not_for_entity()
        {
            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.Add(new Mock<ComplexTypeConfiguration>(typeof(object)).Object);

            Assert.Equal(
                Strings.EntityTypeConfigurationMismatch(typeof(object)),
                Assert.Throws<InvalidOperationException>(() => modelConfiguration.Entity(typeof(object))).Message);
        }

        [Fact]
        public void ComplexType_should_return_new_configuration_if_no_configuration_found()
        {
            var modelConfiguration = new ModelConfiguration();

            Assert.NotNull(modelConfiguration.ComplexType(typeof(object)));
        }

        [Fact]
        public void ComplexType_should_throw_when_configuration_is_not_for_complex_type()
        {
            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.Add(new Mock<EntityTypeConfiguration>(typeof(object)).Object);

            Assert.Equal(
                Strings.ComplexTypeConfigurationMismatch(typeof(object)),
                Assert.Throws<InvalidOperationException>(() => modelConfiguration.ComplexType(typeof(object))).Message);
        }

        [Fact]
        public void GetStructuralTypeConfiguration_should_return_registered_entity_configuration()
        {
            var modelConfiguration = new ModelConfiguration();
            var entityTypeConfiguration = new Mock<EntityTypeConfiguration>(typeof(object)).Object;
            modelConfiguration.Add(entityTypeConfiguration);

            Assert.Same(entityTypeConfiguration, modelConfiguration.GetStructuralTypeConfiguration(typeof(object)));
        }

        [Fact]
        public void GetStructuralTypeConfiguration_should_return_registered_complex_type_configuration()
        {
            var modelConfiguration = new ModelConfiguration();
            var complexTypeConfiguration = new Mock<ComplexTypeConfiguration>(typeof(object)).Object;
            modelConfiguration.Add(complexTypeConfiguration);

            Assert.Same(complexTypeConfiguration, modelConfiguration.GetStructuralTypeConfiguration(typeof(object)));
        }

        [Fact]
        public void GetStructuralTypeConfiguration_should_return_null_configuration_when_not_found()
        {
            var modelConfiguration = new ModelConfiguration();

            Assert.Null(modelConfiguration.GetStructuralTypeConfiguration(typeof(object)));
        }

        [Fact]
        public void IsComplexType_should_return_true_when_type_is_complex()
        {
            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.Add(new Mock<ComplexTypeConfiguration>(typeof(object)).Object);

            Assert.True(modelConfiguration.IsComplexType(typeof(object)));
        }

        [Fact]
        public void Ignore_should_add_property_to_list_of_ignored_properties()
        {
            var modelConfiguration = new ModelConfiguration();

            modelConfiguration.Ignore(typeof(string));

            Assert.True(modelConfiguration.IsIgnoredType(typeof(string)));
        }

        //             E1
        //           / 
        //  (ToTable)E2    
        //       /
        //      E3 (ToTable)
        [Fact]
        public void Configure_mapping_can_configure_two_levels_of_TPT()
        {
            //Setup
            var model = TestModelBuilderHelpers.CreateTwoLevelInheritanceWithThreeEntities();

            var entityType1 = model.GetEntityType("E1");
            var entityType2 = model.GetEntityType("E2");
            var entityType3 = model.GetEntityType("E3");
            (entityType1.GetDeclaredPrimitiveProperties().SingleOrDefault(p => p.Name == "P1")).SetStoreGeneratedPattern(
                StoreGeneratedPattern.Identity);

            var databaseMapping = new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(model);

            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.Entity(entityType2.GetClrType()).ToTable("E2");
            modelConfiguration.Entity(entityType3.GetClrType()).ToTable("E3");

            modelConfiguration.Configure(databaseMapping, ProviderRegistry.Sql2008_ProviderManifest);

            var entityTypeMapping1 = databaseMapping.GetEntityTypeMapping(entityType1);
            var entityTypeMapping2 = databaseMapping.GetEntityTypeMapping(entityType2);
            var entityTypeMapping3 = databaseMapping.GetEntityTypeMapping(entityType3);

            var table1 = entityTypeMapping1.MappingFragments.Single().Table;
            var table2 = entityTypeMapping2.MappingFragments.Single().Table;
            var table3 = entityTypeMapping3.MappingFragments.Single().Table;

            Assert.NotSame(table1, table2);
            Assert.NotSame(table1, table3);
            Assert.NotSame(table3, table2);

            Assert.True(entityTypeMapping1.IsHierarchyMapping);
            Assert.Equal(2, table1.Properties.Count);
            Assert.Equal("P1", table1.Properties[0].Name);
            Assert.Equal("P2", table1.Properties[1].Name);
            Assert.Equal(StoreGeneratedPattern.Identity, table1.Properties[0].StoreGeneratedPattern);
            Assert.Equal(2, entityTypeMapping1.MappingFragments.Single().ColumnMappings.Count());
            Assert.Equal("P1", entityTypeMapping1.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty.Name);
            Assert.Equal("P2", entityTypeMapping1.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty.Name);

            Assert.True(entityTypeMapping2.IsHierarchyMapping);
            Assert.Equal(StoreGeneratedPattern.None, table2.Properties[0].StoreGeneratedPattern);
            Assert.Equal(2, entityTypeMapping2.MappingFragments.Single().ColumnMappings.Count());
            Assert.Equal("P1", entityTypeMapping2.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty.Name);
            Assert.Equal("P3", entityTypeMapping2.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty.Name);
            Assert.Equal(2, table2.Properties.Count);
            Assert.Equal("P1", table2.Properties[0].Name);
            Assert.Equal("P3", table2.Properties[1].Name);
            Assert.NotSame(table1.Properties[0], table2.Properties[0]);

            Assert.False(entityTypeMapping3.IsHierarchyMapping);
            Assert.Equal(StoreGeneratedPattern.None, table3.Properties[0].StoreGeneratedPattern);
            Assert.Equal(2, entityTypeMapping3.MappingFragments.Single().ColumnMappings.Count());
            Assert.Equal("P1", entityTypeMapping3.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty.Name);
            Assert.Equal("P4", entityTypeMapping3.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty.Name);
            Assert.Equal(2, table3.Properties.Count);
            Assert.Equal("P1", table3.Properties[0].Name);
            Assert.Equal("P4", table3.Properties[1].Name);
            Assert.NotSame(table1.Properties[0], table3.Properties[0]);
            Assert.NotSame(table2.Properties[0], table3.Properties[0]);
        }

        //             E1
        //           /   \
        //  (ToTable)E2     E3(ToTable)
        //
        [Fact]
        public void Configure_mapping_can_configure_one_level_TPT_on_both_sides_of_tree()
        {
            //Setup
            var model = TestModelBuilderHelpers.CreateSingleLevelInheritanceWithThreeEntities();

            var entityType1 = model.GetEntityType("E1");
            var entityType2 = model.GetEntityType("E2");
            var entityType3 = model.GetEntityType("E3");
            (entityType1.GetDeclaredPrimitiveProperties().SingleOrDefault(p => p.Name == "P1")).SetStoreGeneratedPattern(
                StoreGeneratedPattern.Identity);

            var databaseMapping = new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(model);
            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.Entity(entityType2.GetClrType()).ToTable("E2");
            modelConfiguration.Entity(entityType3.GetClrType()).ToTable("E3");

            modelConfiguration.Configure(databaseMapping, ProviderRegistry.Sql2008_ProviderManifest);

            var entityTypeMapping1 = databaseMapping.GetEntityTypeMapping(entityType1);
            var entityTypeMapping2 = databaseMapping.GetEntityTypeMapping(entityType2);
            var entityTypeMapping3 = databaseMapping.GetEntityTypeMapping(entityType3);

            var table1 = entityTypeMapping1.MappingFragments.Single().Table;
            var table2 = entityTypeMapping2.MappingFragments.Single().Table;
            var table3 = entityTypeMapping3.MappingFragments.Single().Table;

            Assert.NotSame(table1, table2);
            Assert.NotSame(table1, table3);
            Assert.NotSame(table3, table2);

            Assert.True(entityTypeMapping1.IsHierarchyMapping);
            Assert.Equal(2, table1.Properties.Count);
            Assert.Equal("P1", table1.Properties[0].Name);
            Assert.Equal(StoreGeneratedPattern.Identity, table1.Properties[0].StoreGeneratedPattern);
            Assert.Equal("P2", table1.Properties[1].Name);
            Assert.Equal(2, entityTypeMapping1.MappingFragments.Single().ColumnMappings.Count());
            Assert.Equal("P1", entityTypeMapping1.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty.Name);
            Assert.Equal("P2", entityTypeMapping1.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty.Name);

            Assert.False(entityTypeMapping2.IsHierarchyMapping);
            Assert.Equal(2, entityTypeMapping2.MappingFragments.Single().ColumnMappings.Count());
            Assert.Equal("P1", entityTypeMapping2.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty.Name);
            Assert.Equal(StoreGeneratedPattern.None, table2.Properties[0].StoreGeneratedPattern);
            Assert.Equal("P3", entityTypeMapping2.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty.Name);
            Assert.Equal(2, table2.Properties.Count);
            Assert.Equal("P1", table2.Properties[0].Name);
            Assert.Equal("P3", table2.Properties[1].Name);
            Assert.NotSame(table1.Properties[0], table2.Properties[0]);

            Assert.False(entityTypeMapping3.IsHierarchyMapping);
            Assert.Equal(2, entityTypeMapping3.MappingFragments.Single().ColumnMappings.Count());
            Assert.Equal(StoreGeneratedPattern.None, table3.Properties[0].StoreGeneratedPattern);
            Assert.Equal("P1", entityTypeMapping3.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty.Name);
            Assert.Equal("P4", entityTypeMapping3.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty.Name);
            Assert.Equal(2, table3.Properties.Count);
            Assert.Equal("P1", table3.Properties[0].Name);
            Assert.Equal("P4", table3.Properties[1].Name);
            Assert.NotSame(table1.Properties[0], table3.Properties[0]);
            Assert.NotSame(table2.Properties[0], table3.Properties[0]);
        }

        //              E1
        //            / 
        //  (TPC)E2    
        [Fact]
        public void Configure_mapping_can_process_simple_TPC_mapping()
        {
            //Setup
            var model = TestModelBuilderHelpers.CreateSimpleInheritanceTwoEntities();
            var databaseMapping = new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(model);

            var entityType1 = model.GetEntityType("E1");
            var entityType2 = model.GetEntityType("E2");

            // Action
            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.Entity(entityType2.GetClrType())
                              .AddMappingConfiguration(
                                  new EntityMappingConfiguration
                                      {
                                          MapInheritedProperties = true,
                                          TableName = new DatabaseName("E2")
                                      });
            modelConfiguration.Configure(databaseMapping, ProviderRegistry.Sql2008_ProviderManifest);

            //Validate
            var entitySetMapping = databaseMapping.GetEntitySetMapping(model.GetEntitySet(entityType1));
            Assert.NotNull(entitySetMapping);
            Assert.Equal(2, entitySetMapping.EntityTypeMappings.Count());
            var entityType1Mapping = databaseMapping.GetEntityTypeMapping(entityType1);
            var entityType2Mapping = databaseMapping.GetEntityTypeMapping(entityType2);

            var table1 = entityType1Mapping.MappingFragments.Single().Table;
            var table2 = entityType2Mapping.MappingFragments.Single().Table;
            Assert.NotSame(table1, table2);

            Assert.False(entityType1Mapping.IsHierarchyMapping);
            Assert.Equal(2, table1.Properties.Count);
            Assert.Equal("P1", table1.Properties[0].Name);
            Assert.Equal("P2", table1.Properties[1].Name);
            Assert.Equal(2, entityType1Mapping.MappingFragments.Single().ColumnMappings.Count());
            Assert.Equal("P1", entityType1Mapping.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty.Name);
            Assert.Equal("P2", entityType1Mapping.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty.Name);

            Assert.False(entityType2Mapping.IsHierarchyMapping);
            Assert.Equal(3, entityType2Mapping.MappingFragments.Single().ColumnMappings.Count());
            Assert.Equal("P1", entityType2Mapping.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty.Name);
            Assert.Equal("P2", entityType2Mapping.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty.Name);
            Assert.Equal("P3", entityType2Mapping.MappingFragments.Single().ColumnMappings.ElementAt(2).ColumnProperty.Name);
            Assert.Equal(3, table2.Properties.Count);
            Assert.Equal("P1", table2.Properties[0].Name);
            Assert.Equal("P2", table2.Properties[1].Name);
            Assert.Equal("P3", table2.Properties[2].Name);
            Assert.NotSame(table1.Properties[0], table2.Properties[0]);
            Assert.NotSame(table1.Properties[1], table2.Properties[1]);
            Assert.NotSame(
                entityType1Mapping.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty,
                entityType2Mapping.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty);
            Assert.NotSame(
                entityType1Mapping.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty,
                entityType2Mapping.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty);
        }

        //             E
        //           / 
        //    (TPC)E2    
        //       /
        //      E3(TPC)
        [Fact]
        public void Configure_mapping_can_process_two_levels_of_TPC()
        {
            //Setup
            var model = TestModelBuilderHelpers.CreateTwoLevelInheritanceWithThreeEntities();
            var databaseMapping = new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(model);

            var entityType1 = model.GetEntityType("E1");
            var entityType2 = model.GetEntityType("E2");
            var entityType3 = model.GetEntityType("E3");

            // Action
            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.Entity(entityType2.GetClrType())
                              .AddMappingConfiguration(
                                  new EntityMappingConfiguration
                                      {
                                          MapInheritedProperties = true,
                                          TableName = new DatabaseName("E2")
                                      });
            modelConfiguration.Entity(entityType3.GetClrType())
                              .AddMappingConfiguration(
                                  new EntityMappingConfiguration
                                      {
                                          MapInheritedProperties = true,
                                          TableName = new DatabaseName("E3")
                                      });
            modelConfiguration.Configure(databaseMapping, ProviderRegistry.Sql2008_ProviderManifest);

            //Validate
            var entitySetMapping = databaseMapping.GetEntitySetMapping(model.GetEntitySet(entityType1));
            Assert.NotNull(entitySetMapping);
            Assert.Equal(3, entitySetMapping.EntityTypeMappings.Count());

            var entityType1Mapping = databaseMapping.GetEntityTypeMapping(entityType1);
            var entityType2Mapping = databaseMapping.GetEntityTypeMapping(entityType2);
            var entityType3Mapping = databaseMapping.GetEntityTypeMapping(entityType3);

            var table1 = entityType1Mapping.MappingFragments.Single().Table;
            var table2 = entityType2Mapping.MappingFragments.Single().Table;
            var table3 = entityType3Mapping.MappingFragments.Single().Table;

            Assert.False(entityType1Mapping.IsHierarchyMapping);
            Assert.Equal(2, table1.Properties.Count);
            Assert.Equal("P1", table1.Properties[0].Name);
            Assert.Equal("P2", table1.Properties[1].Name);
            Assert.Equal(2, entityType1Mapping.MappingFragments.Single().ColumnMappings.Count());
            Assert.Same(entityType1Mapping.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty, table1.Properties[0]);
            Assert.Same(entityType1Mapping.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty, table1.Properties[1]);

            Assert.False(entityType2Mapping.IsHierarchyMapping);
            Assert.Equal(3, table2.Properties.Count);
            Assert.Equal("P1", table2.Properties[0].Name);
            Assert.Equal("P2", table2.Properties[1].Name);
            Assert.Equal("P3", table2.Properties[2].Name);
            Assert.NotSame(table1, table2);
            Assert.NotSame(table1.Properties[0], table2.Properties[0]);
            Assert.NotSame(table1.Properties[1], table2.Properties[1]);
            Assert.Equal(3, entityType2Mapping.MappingFragments.Single().ColumnMappings.Count());
            Assert.Same(entityType2Mapping.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty, table2.Properties[0]);
            Assert.Same(entityType2Mapping.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty, table2.Properties[1]);
            Assert.Same(entityType2Mapping.MappingFragments.Single().ColumnMappings.ElementAt(2).ColumnProperty, table2.Properties[2]);

            Assert.False(entityType3Mapping.IsHierarchyMapping);
            Assert.Equal(4, table3.Properties.Count);
            Assert.Equal("P1", table3.Properties[0].Name);
            Assert.Equal("P2", table3.Properties[1].Name);
            Assert.Equal("P3", table3.Properties[2].Name);
            Assert.Equal("P4", table3.Properties[3].Name);
            Assert.NotSame(table1, table3);
            Assert.NotSame(table3, table2);
            Assert.NotSame(table1.Properties[0], table3.Properties[0]);
            Assert.NotSame(table1.Properties[1], table3.Properties[1]);
            Assert.NotSame(table3.Properties[0], table2.Properties[0]);
            Assert.NotSame(table3.Properties[1], table2.Properties[1]);
            Assert.NotSame(table2.Properties[2], table3.Properties[2]);
            Assert.Equal(4, entityType3Mapping.MappingFragments.Single().ColumnMappings.Count());
            Assert.Same(entityType3Mapping.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty, table3.Properties[0]);
            Assert.Same(entityType3Mapping.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty, table3.Properties[1]);
            Assert.Same(entityType3Mapping.MappingFragments.Single().ColumnMappings.ElementAt(2).ColumnProperty, table3.Properties[2]);
            Assert.Same(entityType3Mapping.MappingFragments.Single().ColumnMappings.ElementAt(3).ColumnProperty, table3.Properties[3]);
        }

        //             E
        //           /   \
        //    (TPC)E2     E3(TPC)
        //
        [Fact]
        public void Configure_mapping_can_process_one_level_TPC_on_both_sides_of_tree()
        {
            //Setup
            var model = TestModelBuilderHelpers.CreateSingleLevelInheritanceWithThreeEntities();
            var databaseMapping = new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(model);

            var entityType1 = model.GetEntityType("E1");
            var entityType2 = model.GetEntityType("E2");
            var entityType3 = model.GetEntityType("E3");

            // Action
            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.Entity(entityType2.GetClrType())
                              .AddMappingConfiguration(
                                  new EntityMappingConfiguration
                                      {
                                          MapInheritedProperties = true,
                                          TableName = new DatabaseName("E2")
                                      });
            modelConfiguration.Entity(entityType3.GetClrType())
                              .AddMappingConfiguration(
                                  new EntityMappingConfiguration
                                      {
                                          MapInheritedProperties = true,
                                          TableName = new DatabaseName("E3")
                                      });
            modelConfiguration.Configure(databaseMapping, ProviderRegistry.Sql2008_ProviderManifest);

            //Validate
            var entitySetMapping = databaseMapping.GetEntitySetMapping(model.GetEntitySet(entityType1));
            Assert.NotNull(entitySetMapping);
            Assert.Equal(3, entitySetMapping.EntityTypeMappings.Count());

            var entityType1Mapping = databaseMapping.GetEntityTypeMapping(entityType1);
            var entityType2Mapping = databaseMapping.GetEntityTypeMapping(entityType2);
            var entityType3Mapping = databaseMapping.GetEntityTypeMapping(entityType3);

            var table1 = entityType1Mapping.MappingFragments.Single().Table;
            var table2 = entityType2Mapping.MappingFragments.Single().Table;
            var table3 = entityType3Mapping.MappingFragments.Single().Table;

            Assert.False(entityType1Mapping.IsHierarchyMapping);
            Assert.Equal(2, table1.Properties.Count);
            Assert.Equal("P1", table1.Properties[0].Name);
            Assert.Equal("P2", table1.Properties[1].Name);
            Assert.Equal(2, entityType1Mapping.MappingFragments.Single().ColumnMappings.Count());
            Assert.Same(entityType1Mapping.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty, table1.Properties[0]);
            Assert.Same(entityType1Mapping.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty, table1.Properties[1]);

            Assert.False(entityType2Mapping.IsHierarchyMapping);
            Assert.Equal("E2", table2.Name);
            Assert.Equal(3, table2.Properties.Count);
            Assert.Equal("P1", table2.Properties[0].Name);
            Assert.Equal("P2", table2.Properties[1].Name);
            Assert.Equal("P3", table2.Properties[2].Name);
            Assert.NotSame(table1, table2);
            Assert.NotSame(table1.Properties[0], table2.Properties[0]);
            Assert.NotSame(table1.Properties[1], table2.Properties[1]);
            Assert.Equal(3, entityType2Mapping.MappingFragments.Single().ColumnMappings.Count());
            Assert.Same(entityType2Mapping.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty, table2.Properties[0]);
            Assert.Same(entityType2Mapping.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty, table2.Properties[1]);
            Assert.Same(entityType2Mapping.MappingFragments.Single().ColumnMappings.ElementAt(2).ColumnProperty, table2.Properties[2]);

            Assert.False(entityType3Mapping.IsHierarchyMapping);
            Assert.Equal("E3", table3.Name);
            Assert.Equal(3, table3.Properties.Count);
            Assert.Equal("P1", table3.Properties[0].Name);
            Assert.Equal("P2", table3.Properties[1].Name);
            Assert.Equal("P4", table3.Properties[2].Name);
            Assert.NotSame(table1, table3);
            Assert.NotSame(table3, table2);
            Assert.NotSame(table1.Properties[0], table3.Properties[0]);
            Assert.NotSame(table1.Properties[1], table3.Properties[1]);
            Assert.Equal(3, entityType3Mapping.MappingFragments.Single().ColumnMappings.Count());
            Assert.Same(entityType3Mapping.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty, table3.Properties[0]);
            Assert.Same(entityType3Mapping.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty, table3.Properties[1]);
            Assert.Same(entityType3Mapping.MappingFragments.Single().ColumnMappings.ElementAt(2).ColumnProperty, table3.Properties[2]);
        }

        //              E1
        //            / 
        //         E2  
        [Fact]
        public void Configure_mapping_can_process_simple_TPH_mapping()
        {
            //Setup
            var model = TestModelBuilderHelpers.CreateSimpleInheritanceTwoEntities();
            var databaseMapping = new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(model);

            var entityType1 = model.GetEntityType("E1");
            var entityType2 = model.GetEntityType("E2");

            // Action
            var modelConfiguration = new ModelConfiguration();
            var entity1Configuration = modelConfiguration.Entity(entityType1.GetClrType());
            var entity1MappingConfiguration =
                new EntityMappingConfiguration
                    {
                        TableName = new DatabaseName("E1")
                    };
            entity1MappingConfiguration
                .AddValueCondition(
                    new ValueConditionConfiguration(entity1MappingConfiguration, "disc")
                        {
                            Value = "foo"
                        });
            entity1Configuration.AddMappingConfiguration(entity1MappingConfiguration);
            var entity1SubTypeMappingConfiguration =
                new EntityMappingConfiguration
                    {
                        TableName = new DatabaseName("E1")
                    };
            entity1SubTypeMappingConfiguration
                .AddValueCondition(
                    new ValueConditionConfiguration(entity1SubTypeMappingConfiguration, "disc")
                        {
                            Value = "bar"
                        });
            entity1Configuration.AddSubTypeMappingConfiguration(entityType2.GetClrType(), entity1SubTypeMappingConfiguration);
            modelConfiguration.NormalizeConfigurations();
            modelConfiguration.Configure(databaseMapping, ProviderRegistry.Sql2008_ProviderManifest);

            //Validate
            var entitySetMapping = databaseMapping.GetEntitySetMapping(model.GetEntitySet(entityType1));
            Assert.NotNull(entitySetMapping);
            Assert.Equal(3, entitySetMapping.EntityTypeMappings.Count());
            var entityType1Mapping = databaseMapping.GetEntityTypeMapping(entityType1);
            var entityType1MappingConditions = databaseMapping.GetEntityTypeMappings(entityType1).Single(tm => !tm.IsHierarchyMapping);
            var entityType2Mapping = databaseMapping.GetEntityTypeMapping(entityType2);

            var table1 = entityType1Mapping.MappingFragments.Single().Table;
            var table2 = entityType2Mapping.MappingFragments.Single().Table;

            Assert.True(entityType1Mapping.IsHierarchyMapping);
            Assert.Equal(4, table1.Properties.Count);
            Assert.Equal("P1", table1.Properties[0].Name);
            Assert.Equal("P2", table1.Properties[1].Name);
            Assert.Equal("P3", table1.Properties[2].Name);
            Assert.Equal("disc", table1.Properties[3].Name);
            Assert.Equal(2, entityType1Mapping.MappingFragments.Single().ColumnMappings.Count());
            Assert.Equal("P1", entityType1Mapping.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty.Name);
            Assert.Equal("P2", entityType1Mapping.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty.Name);
            Assert.Same(
                table1.Properties[3], entityType1MappingConditions.MappingFragments.Single().ColumnConditions.Single().ColumnProperty);
            Assert.Equal("foo", entityType1MappingConditions.MappingFragments.Single().ColumnConditions.Single().Value);
            Assert.Equal(
                "nvarchar", entityType1MappingConditions.MappingFragments.Single().ColumnConditions.Single().ColumnProperty.TypeName);
            Assert.Equal(
                DatabaseMappingGenerator.DiscriminatorMaxLength,
                entityType1MappingConditions.MappingFragments.Single().ColumnConditions.Single().ColumnProperty.MaxLength);

            Assert.False(entityType2Mapping.IsHierarchyMapping);
            Assert.Same(table1, table2);
            Assert.Equal(2, entityType2Mapping.MappingFragments.Single().ColumnMappings.Count());
            Assert.Same(table1.Properties[0], table2.Properties[0]);
            Assert.Same(table1.Properties[1], table2.Properties[1]);
            Assert.Equal("P1", entityType2Mapping.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty.Name);
            Assert.Equal("P3", entityType2Mapping.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty.Name);
            Assert.Same(table2.Properties[3], entityType2Mapping.MappingFragments.Single().ColumnConditions.Single().ColumnProperty);
            Assert.Equal("bar", entityType2Mapping.MappingFragments.Single().ColumnConditions.Single().Value);
        }

        //             E1
        //           / 
        //         E2    
        //       /
        //      E3
        [Fact]
        public void Configure_mapping_can_process_two_levels_of_TPH()
        {
            //Setup
            var model = TestModelBuilderHelpers.CreateTwoLevelInheritanceWithThreeEntities();
            var databaseMapping = new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(model);

            var entityType1 = model.GetEntityType("E1");
            var entityType2 = model.GetEntityType("E2");
            var entityType3 = model.GetEntityType("E3");

            // Action
            var modelConfiguration = new ModelConfiguration();
            var entity1Configuration = modelConfiguration.Entity(entityType1.GetClrType());
            var entity1MappingConfiguration =
                new EntityMappingConfiguration
                    {
                        TableName = new DatabaseName("E1")
                    };
            entity1MappingConfiguration
                .AddValueCondition(
                    new ValueConditionConfiguration(entity1MappingConfiguration, "disc")
                        {
                            Value = 1
                        });
            entity1Configuration.AddMappingConfiguration(entity1MappingConfiguration);
            var entity1SubTypeMappingConfiguration =
                new EntityMappingConfiguration
                    {
                        TableName = new DatabaseName("E1")
                    };
            entity1SubTypeMappingConfiguration
                .AddValueCondition(
                    new ValueConditionConfiguration(entity1SubTypeMappingConfiguration, "disc")
                        {
                            Value = 3
                        });
            entity1SubTypeMappingConfiguration
                .AddNullabilityCondition(
                    new NotNullConditionConfiguration(
                        entity1SubTypeMappingConfiguration,
                        new PropertyPath(
                            entityType3.GetDeclaredPrimitiveProperties().SingleOrDefault(p => p.Name == "P4").GetClrPropertyInfo())));
            entity1Configuration.AddSubTypeMappingConfiguration(entityType3.GetClrType(), entity1SubTypeMappingConfiguration);
            var entity2Configuration = modelConfiguration.Entity(entityType2.GetClrType());
            var entity2MappingConfiguration =
                new EntityMappingConfiguration
                    {
                        TableName = new DatabaseName("E1")
                    };
            entity2MappingConfiguration
                .AddValueCondition(
                    new ValueConditionConfiguration(entity2MappingConfiguration, "disc")
                        {
                            Value = 2
                        });
            entity2Configuration.AddMappingConfiguration(entity2MappingConfiguration);
            modelConfiguration.NormalizeConfigurations();
            modelConfiguration.Configure(databaseMapping, ProviderRegistry.Sql2008_ProviderManifest);

            //Validate
            var entitySetMapping = databaseMapping.GetEntitySetMapping(model.GetEntitySet(entityType1));
            Assert.NotNull(entitySetMapping);
            Assert.Equal(4, entitySetMapping.EntityTypeMappings.Count());

            var entityType1Mapping = databaseMapping.GetEntityTypeMapping(entityType1);
            var entityType1MappingConditions = databaseMapping.GetEntityTypeMappings(entityType1).Single(x => !x.IsHierarchyMapping);
            var entityType2Mapping = databaseMapping.GetEntityTypeMapping(entityType2);
            var entityType3Mapping = databaseMapping.GetEntityTypeMapping(entityType3);

            var table1 = entityType1Mapping.MappingFragments.Single().Table;
            var table2 = entityType2Mapping.MappingFragments.Single().Table;
            var table3 = entityType3Mapping.MappingFragments.Single().Table;

            Assert.True(entityType1Mapping.IsHierarchyMapping);
            Assert.Equal(5, table1.Properties.Count);
            Assert.Equal("P1", table1.Properties[0].Name);
            Assert.Equal("P2", table1.Properties[1].Name);
            Assert.Equal("P3", table1.Properties[2].Name);
            Assert.Equal("P4", table1.Properties[3].Name);
            Assert.Equal("disc", table1.Properties[4].Name);
            Assert.Equal(2, entityType1Mapping.MappingFragments.Single().ColumnMappings.Count());
            Assert.Equal("P1", entityType1Mapping.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty.Name);
            Assert.Equal("P2", entityType1Mapping.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty.Name);
            Assert.Same(
                table1.Properties[4], entityType1MappingConditions.MappingFragments.Single().ColumnConditions.Single().ColumnProperty);
            Assert.Equal(1, entityType1MappingConditions.MappingFragments.Single().ColumnConditions.Single().Value);

            Assert.False(entityType2Mapping.IsHierarchyMapping);
            Assert.Same(table1, table2);
            Assert.Same(table1.Properties[0], table2.Properties[0]);
            Assert.Same(table1.Properties[1], table2.Properties[1]);
            Assert.Equal(2, entityType2Mapping.MappingFragments.Single().ColumnMappings.Count());
            Assert.Equal("P1", entityType2Mapping.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty.Name);
            Assert.Equal("P3", entityType2Mapping.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty.Name);
            Assert.Same(table2.Properties[4], entityType2Mapping.MappingFragments.Single().ColumnConditions.Single().ColumnProperty);
            Assert.Equal(2, entityType2Mapping.MappingFragments.Single().ColumnConditions.Single().Value);
            Assert.Null(entityType2Mapping.MappingFragments.Single().ColumnConditions.Single().IsNull);

            Assert.False(entityType3Mapping.IsHierarchyMapping);
            Assert.Same(table1, table3);
            Assert.Same(table1.Properties[0], table3.Properties[0]);
            Assert.Same(table1.Properties[1], table3.Properties[1]);
            Assert.Equal(3, entityType3Mapping.MappingFragments.Single().ColumnMappings.Count());
            Assert.Equal("P1", entityType3Mapping.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty.Name);
            Assert.Equal("P3", entityType3Mapping.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty.Name);
            Assert.Equal("P4", entityType3Mapping.MappingFragments.Single().ColumnMappings.ElementAt(2).ColumnProperty.Name);
            Assert.Same(table3.Properties[4], entityType3Mapping.MappingFragments.Single().ColumnConditions.ElementAt(0).ColumnProperty);
            Assert.Equal(3, entityType3Mapping.MappingFragments.Single().ColumnConditions.ElementAt(0).Value);
            Assert.Equal("int", entityType3Mapping.MappingFragments.Single().ColumnConditions.ElementAt(0).ColumnProperty.TypeName);
            Assert.Same(table3.Properties[3], entityType3Mapping.MappingFragments.Single().ColumnConditions.ElementAt(1).ColumnProperty);
            Assert.Equal(false, entityType3Mapping.MappingFragments.Single().ColumnConditions.ElementAt(1).IsNull);
        }

        //             E1
        //           /   \
        //         E2     E3
        [Fact]
        public void Configure_mapping_can_process_one_level_TPH_on_both_sides_of_tree()
        {
            //Setup
            var model = TestModelBuilderHelpers.CreateSingleLevelInheritanceWithThreeEntities();
            var databaseMapping = new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(model);

            var entityType1 = model.GetEntityType("E1");
            var entityType2 = model.GetEntityType("E2");
            var entityType3 = model.GetEntityType("E3");

            // Action
            var modelConfiguration = new ModelConfiguration();
            var entity1Configuration = modelConfiguration.Entity(entityType1.GetClrType());
            var entity1MappingConfiguration =
                new EntityMappingConfiguration
                    {
                        TableName = new DatabaseName("E1")
                    };
            entity1MappingConfiguration
                .AddValueCondition(
                    new ValueConditionConfiguration(entity1MappingConfiguration, "P3")
                        {
                            Value = null
                        });
            entity1MappingConfiguration
                .AddValueCondition(
                    new ValueConditionConfiguration(entity1MappingConfiguration, "P4")
                        {
                            Value = null
                        });
            entity1Configuration.AddMappingConfiguration(entity1MappingConfiguration);
            var entity1SubTypeMappingConfiguration =
                new EntityMappingConfiguration
                    {
                        TableName = new DatabaseName("E1")
                    };
            entity1SubTypeMappingConfiguration
                .AddValueCondition(
                    new ValueConditionConfiguration(entity1SubTypeMappingConfiguration, "P3")
                        {
                            Value = null
                        });
            entity1SubTypeMappingConfiguration
                .AddNullabilityCondition(
                    new NotNullConditionConfiguration(
                        entity1SubTypeMappingConfiguration,
                        new PropertyPath(
                            entityType3.GetDeclaredPrimitiveProperties().SingleOrDefault(p => p.Name == "P4").GetClrPropertyInfo())));
            entity1Configuration.AddSubTypeMappingConfiguration(entityType3.GetClrType(), entity1SubTypeMappingConfiguration);
            var entity2Configuration = modelConfiguration.Entity(entityType2.GetClrType());
            var entity2MappingConfiguration =
                new EntityMappingConfiguration
                    {
                        TableName = new DatabaseName("E1")
                    };
            entity2MappingConfiguration
                .AddNullabilityCondition(
                    new NotNullConditionConfiguration(
                        entity2MappingConfiguration,
                        new PropertyPath(
                            entityType2.GetDeclaredPrimitiveProperties().SingleOrDefault(p => p.Name == "P3").GetClrPropertyInfo())));
            entity2MappingConfiguration
                .AddValueCondition(
                    new ValueConditionConfiguration(entity2MappingConfiguration, "P4")
                        {
                            Value = null
                        });
            entity2Configuration.AddMappingConfiguration(entity2MappingConfiguration);
            modelConfiguration.NormalizeConfigurations();
            modelConfiguration.Configure(databaseMapping, ProviderRegistry.Sql2008_ProviderManifest);

            //Validate
            var entitySetMapping = databaseMapping.GetEntitySetMapping(model.GetEntitySet(entityType1));
            Assert.NotNull(entitySetMapping);
            Assert.Equal(4, entitySetMapping.EntityTypeMappings.Count());

            var entityType1Mapping = databaseMapping.GetEntityTypeMapping(entityType1);
            var entityType1MappingConditions = databaseMapping.GetEntityTypeMappings(entityType1).Single(x => !x.IsHierarchyMapping);
            var entityType2Mapping = databaseMapping.GetEntityTypeMapping(entityType2);
            var entityType3Mapping = databaseMapping.GetEntityTypeMapping(entityType3);

            var table1 = entityType1Mapping.MappingFragments.Single().Table;
            var table2 = entityType2Mapping.MappingFragments.Single().Table;
            var table3 = entityType3Mapping.MappingFragments.Single().Table;

            Assert.True(entityType1Mapping.IsHierarchyMapping);
            Assert.Equal(4, table1.Properties.Count);
            Assert.Equal("P1", table1.Properties[0].Name);
            Assert.Equal("P2", table1.Properties[1].Name);
            Assert.Equal("P3", table1.Properties[2].Name);
            Assert.Equal("P4", table1.Properties[3].Name);
            Assert.Equal(2, entityType1Mapping.MappingFragments.Single().ColumnMappings.Count());
            Assert.Equal("P1", entityType1Mapping.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty.Name);
            Assert.Equal("P2", entityType1Mapping.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty.Name);
            Assert.Same(
                table1.Properties[2], entityType1MappingConditions.MappingFragments.Single().ColumnConditions.ElementAt(0).ColumnProperty);
            Assert.True((bool)entityType1MappingConditions.MappingFragments.Single().ColumnConditions.ElementAt(0).IsNull);
            Assert.Same(
                table1.Properties[3], entityType1MappingConditions.MappingFragments.Single().ColumnConditions.ElementAt(1).ColumnProperty);
            Assert.True((bool)entityType1MappingConditions.MappingFragments.Single().ColumnConditions.ElementAt(1).IsNull);

            Assert.False(entityType2Mapping.IsHierarchyMapping);
            Assert.Same(table1, table2);
            Assert.Same(table1.Properties[0], table2.Properties[0]);
            Assert.Same(table1.Properties[1], table2.Properties[1]);
            Assert.Equal(2, entityType2Mapping.MappingFragments.Single().ColumnMappings.Count());
            Assert.Equal("P1", entityType2Mapping.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty.Name);
            Assert.Equal("P3", entityType2Mapping.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty.Name);
            Assert.Same(table1.Properties[3], entityType2Mapping.MappingFragments.Single().ColumnConditions.ElementAt(0).ColumnProperty);
            Assert.True((bool)entityType2Mapping.MappingFragments.Single().ColumnConditions.ElementAt(0).IsNull);
            Assert.Same(table1.Properties[2], entityType2Mapping.MappingFragments.Single().ColumnConditions.ElementAt(1).ColumnProperty);
            Assert.False((bool)entityType2Mapping.MappingFragments.Single().ColumnConditions.ElementAt(1).IsNull);

            Assert.False(entityType3Mapping.IsHierarchyMapping);
            Assert.Same(table1, table3);
            Assert.Same(table1.Properties[0], table3.Properties[0]);
            Assert.Same(table1.Properties[1], table3.Properties[1]);
            Assert.Equal(2, entityType3Mapping.MappingFragments.Single().ColumnMappings.Count());
            Assert.Equal("P1", entityType3Mapping.MappingFragments.Single().ColumnMappings.ElementAt(0).ColumnProperty.Name);
            Assert.Equal("P4", entityType3Mapping.MappingFragments.Single().ColumnMappings.ElementAt(1).ColumnProperty.Name);
            Assert.Same(table1.Properties[2], entityType3Mapping.MappingFragments.Single().ColumnConditions.ElementAt(0).ColumnProperty);
            Assert.True((bool)entityType3Mapping.MappingFragments.Single().ColumnConditions.ElementAt(0).IsNull);
            Assert.Same(table1.Properties[3], entityType3Mapping.MappingFragments.Single().ColumnConditions.ElementAt(1).ColumnProperty);
            Assert.False((bool)entityType3Mapping.MappingFragments.Single().ColumnConditions.ElementAt(1).IsNull);
        }

        [Fact]
        public void Configure_mapping_can_process_entity_splitting()
        {
            EdmModel model =
                new TestModelBuilder()
                    .Entity("E")
                    .Key("P1")
                    .Property("P2")
                    .Property("P3")
                    .Property("P4")
                    .Property("P5");
            var databaseMapping = new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(model);

            var entityType = model.GetEntityType("E");
            var modelConfiguration = new ModelConfiguration();
            var entityMappingConfiguration1 =
                new EntityMappingConfiguration
                    {
                        Properties =
                            new List<PropertyPath>
                                {
                                    new PropertyPath(
                                        entityType.GetDeclaredPrimitiveProperties().SingleOrDefault(p => p.Name == "P1").GetClrPropertyInfo(
                                            
                                        )),
                                    new PropertyPath(
                                        entityType.GetDeclaredPrimitiveProperties().SingleOrDefault(p => p.Name == "P2").GetClrPropertyInfo(
                                            
                                        )),
                                    new PropertyPath(
                                        entityType.GetDeclaredPrimitiveProperties().SingleOrDefault(p => p.Name == "P3").GetClrPropertyInfo(
                                            
                                        ))
                                },
                        TableName = new DatabaseName("E1")
                    };
            var entityMappingConfiguration2 =
                new EntityMappingConfiguration
                    {
                        Properties =
                            new List<PropertyPath>
                                {
                                    new PropertyPath(
                                        entityType.GetDeclaredPrimitiveProperties().SingleOrDefault(p => p.Name == "P1").GetClrPropertyInfo(
                                            
                                        )),
                                    new PropertyPath(
                                        entityType.GetDeclaredPrimitiveProperties().SingleOrDefault(p => p.Name == "P4").GetClrPropertyInfo(
                                            
                                        )),
                                    new PropertyPath(
                                        entityType.GetDeclaredPrimitiveProperties().SingleOrDefault(p => p.Name == "P5").GetClrPropertyInfo(
                                            
                                        ))
                                },
                        TableName = new DatabaseName("E2")
                    };
            var entityConfiguration = modelConfiguration.Entity(entityType.GetClrType());
            entityConfiguration.AddMappingConfiguration(entityMappingConfiguration1);
            entityConfiguration.AddMappingConfiguration(entityMappingConfiguration2);
            modelConfiguration.Configure(databaseMapping, ProviderRegistry.Sql2008_ProviderManifest);

            var entityTypeMapping = databaseMapping.GetEntityTypeMapping(entityType);
            var table1 = entityTypeMapping.MappingFragments[0].Table;
            var table2 = entityTypeMapping.MappingFragments[1].Table;
            Assert.NotSame(table1, table2);
            Assert.Equal("E1", table1.GetTableName().Name);
            Assert.Equal("E2", table2.GetTableName().Name);
            Assert.Equal(3, table1.Properties.Count);
            Assert.Equal(3, table2.Properties.Count);
            Assert.Equal(2, entityTypeMapping.MappingFragments.Count);
            var entityTypeMappingFragment1 = entityTypeMapping.MappingFragments[0];
            var entityTypeMappingFragment2 = entityTypeMapping.MappingFragments[1];
            Assert.Equal(3, entityTypeMappingFragment1.ColumnMappings.Count());
            Assert.Equal("P1", entityTypeMappingFragment1.ColumnMappings.ElementAt(0).ColumnProperty.Name);
            Assert.Equal("P2", entityTypeMappingFragment1.ColumnMappings.ElementAt(1).ColumnProperty.Name);
            Assert.Equal("P3", entityTypeMappingFragment1.ColumnMappings.ElementAt(2).ColumnProperty.Name);
            Assert.Equal(3, entityTypeMappingFragment2.ColumnMappings.Count());
            Assert.Equal("P1", entityTypeMappingFragment2.ColumnMappings.ElementAt(0).ColumnProperty.Name);
            Assert.Equal("P4", entityTypeMappingFragment2.ColumnMappings.ElementAt(1).ColumnProperty.Name);
            Assert.Equal("P5", entityTypeMappingFragment2.ColumnMappings.ElementAt(2).ColumnProperty.Name);
        }

        [Fact]
        public void Configure_entity_splitting_should_throw_if_ignored_property_is_mapped()
        {
            EdmModel model =
                new TestModelBuilder()
                    .Entity("E")
                    .Key("P1");
            var databaseMapping = new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(model);

            var entityType = model.GetEntityType("E");
            var modelConfiguration = new ModelConfiguration();
            var entityConfiguration = modelConfiguration.Entity(entityType.GetClrType());
            var p1PropertyInfo = entityType.GetDeclaredPrimitiveProperties().SingleOrDefault(p => p.Name == "P1").GetClrPropertyInfo();
            var entityMappingConfiguration1 =
                new EntityMappingConfiguration
                    {
                        Properties = new List<PropertyPath>
                                         {
                                             new PropertyPath(p1PropertyInfo),
                                             new PropertyPath(new MockPropertyInfo(typeof(int), "P2"))
                                         },
                        TableName = new DatabaseName("E")
                    };
            entityConfiguration.AddMappingConfiguration(entityMappingConfiguration1);

            Assert.Equal(
                Strings.EntityMappingConfiguration_CannotMapIgnoredProperty("E", "P2"),
                Assert.Throws<InvalidOperationException>(
                    () => modelConfiguration.Configure(databaseMapping, ProviderRegistry.Sql2008_ProviderManifest)).Message);
        }
    }

    # region Test model builder helper methods

    internal static class TestModelBuilderHelpers
    {
        //              E1
        //            / 
        //          E2    
        public static EdmModel CreateSimpleInheritanceTwoEntities()
        {
            return new TestModelBuilder()
                .Entity("E1")
                .Key("P1")
                .Property("P2")
                .Subclass("E2")
                .Property("P3");
        }

        //             E1
        //           / 
        //        E2    
        //       /
        //      E3 
        public static EdmModel CreateTwoLevelInheritanceWithThreeEntities()
        {
            return new TestModelBuilder()
                .Entity("E1")
                .Key("P1")
                .Property("P2")
                .Subclass("E2")
                .Property("P3")
                .Subclass("E3")
                .Property("P4");
        }

        //             E1
        //           /   \
        //          E2     E3
        //
        public static EdmModel CreateSingleLevelInheritanceWithThreeEntities()
        {
            return new TestModelBuilder()
                .Entity("E1")
                .Key("P1")
                .Property("P2")
                .Subclass("E2")
                .Property("P3")
                .Subclass("E3", "E1")
                .Property("P4");
        }

        //             E1
        //           /   \
        //         E2     E4
        //       /
        //      E3
        public static EdmModel CreateTwoLevelInheritanceWithFourEntities()
        {
            return new TestModelBuilder()
                .Entity("E1")
                .Key("P1")
                .Property("P2")
                .Subclass("E2")
                .Property("P3")
                .Subclass("E3")
                .Property("P4")
                .Subclass("E4", "E1")
                .Property("P5");
        }
    }

    #endregion
}
