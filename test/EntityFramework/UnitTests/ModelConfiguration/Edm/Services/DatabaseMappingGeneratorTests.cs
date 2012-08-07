// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Services.UnitTests
{
    using System.Data.Entity;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Linq;
    using Xunit;

    public sealed class DatabaseMappingGeneratorTests
    {
        [Fact]
        public void Generate_should_initialize_mapping_model()
        {
            var model = new EdmModel().Initialize();

            var databaseMapping = new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(model);

            Assert.NotNull(databaseMapping);
            Assert.NotNull(databaseMapping.Database);
            Assert.Same(model.Containers.Single(), databaseMapping.EntityContainerMappings.Single().EntityContainer);
        }

        [Fact]
        public void Generate_can_map_a_simple_entity_type_and_set()
        {
            var model = new EdmModel().Initialize();
            var entityType = model.AddEntityType("E");
            entityType.SetClrType(typeof(object));
            entityType.AddPrimitiveProperty("P1").PropertyType.EdmType = EdmPrimitiveType.Int32;
            entityType.AddPrimitiveProperty("P2").PropertyType.EdmType = EdmPrimitiveType.String;
            var entitySet = model.AddEntitySet("ESet", entityType);

            var databaseMapping = new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(model);

            var entitySetMapping = databaseMapping.GetEntitySetMapping(entitySet);

            Assert.NotNull(entitySetMapping);
            Assert.Same(entitySet, entitySetMapping.EntitySet);

            var entityTypeMapping = entitySetMapping.EntityTypeMappings.Single();

            Assert.Same(entityType, entityTypeMapping.EntityType);
            Assert.NotNull(entityTypeMapping.TypeMappingFragments.Single().Table);
            Assert.Equal("E", entityTypeMapping.TypeMappingFragments.Single().Table.Name);
            Assert.Equal(2, entityTypeMapping.TypeMappingFragments.Single().Table.Columns.Count);
            Assert.Equal(typeof(object), entityTypeMapping.GetClrType());
        }

        [Fact]
        public void Generate_should_correctly_map_string_primitive_property_facets()
        {
            var model = new EdmModel().Initialize();
            var entityType = model.AddEntityType("E");
            entityType.SetClrType(typeof(object));
            model.AddEntitySet("ESet", entityType);
            var property = entityType.AddPrimitiveProperty("P");
            property.PropertyType.EdmType = EdmPrimitiveType.String;

            property.PropertyType.IsNullable = false;
            property.PropertyType.PrimitiveTypeFacets.IsFixedLength = true;
            property.PropertyType.PrimitiveTypeFacets.IsMaxLength = true;
            property.PropertyType.PrimitiveTypeFacets.IsUnicode = true;
            property.PropertyType.PrimitiveTypeFacets.MaxLength = 42;
            property.PropertyType.PrimitiveTypeFacets.Precision = 23;
            property.PropertyType.PrimitiveTypeFacets.Scale = 77;
            property.SetStoreGeneratedPattern(DbStoreGeneratedPattern.Identity);

            var databaseMapping = new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(model);

            var column = databaseMapping.GetEntityTypeMapping(entityType).TypeMappingFragments.Single().PropertyMappings.Single().Column;

            Assert.False(column.IsNullable);
            Assert.Null(column.Facets.IsFixedLength);
            Assert.Equal(true, column.Facets.IsMaxLength);
            Assert.Null(column.Facets.IsUnicode);
            Assert.Equal(42, column.Facets.MaxLength);
            Assert.Null(column.Facets.Precision);
            Assert.Null(column.Facets.Scale);
            Assert.Equal(DbStoreGeneratedPattern.Identity, column.StoreGeneratedPattern);
        }

        [Fact]
        public void Generate_should_correctly_map_time_primitive_property_facets()
        {
            var model = new EdmModel().Initialize();
            var entityType = model.AddEntityType("E");
            entityType.SetClrType(typeof(object));
            model.AddEntitySet("ESet", entityType);
            var property = entityType.AddPrimitiveProperty("P");
            property.PropertyType.EdmType = EdmPrimitiveType.Time;

            property.PropertyType.IsNullable = false;
            property.PropertyType.PrimitiveTypeFacets.IsFixedLength = true;
            property.PropertyType.PrimitiveTypeFacets.IsMaxLength = true;
            property.PropertyType.PrimitiveTypeFacets.IsUnicode = false;
            property.PropertyType.PrimitiveTypeFacets.MaxLength = 42;
            property.PropertyType.PrimitiveTypeFacets.Precision = 23;
            property.PropertyType.PrimitiveTypeFacets.Scale = 77;
            property.SetStoreGeneratedPattern(DbStoreGeneratedPattern.Identity);

            var databaseMapping = new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(model);

            var column = databaseMapping.GetEntityTypeMapping(entityType).TypeMappingFragments.Single().PropertyMappings.Single().Column;

            Assert.False(column.IsNullable);
            Assert.Null(column.Facets.IsFixedLength);
            Assert.Null(column.Facets.IsMaxLength);
            Assert.Null(column.Facets.IsUnicode);
            Assert.Null(column.Facets.MaxLength);
            Assert.Equal<byte?>(23, column.Facets.Precision);
            Assert.Null(column.Facets.Scale);
            Assert.Equal(DbStoreGeneratedPattern.Identity, column.StoreGeneratedPattern);
        }

        [Fact]
        public void Generate_should_correctly_map_decimal_primitive_property_facets()
        {
            var model = new EdmModel().Initialize();
            var entityType = model.AddEntityType("E");
            entityType.SetClrType(typeof(object));
            model.AddEntitySet("ESet", entityType);
            var property = entityType.AddPrimitiveProperty("P");
            property.PropertyType.EdmType = EdmPrimitiveType.Decimal;

            property.PropertyType.IsNullable = false;
            property.PropertyType.PrimitiveTypeFacets.IsFixedLength = true;
            property.PropertyType.PrimitiveTypeFacets.IsMaxLength = true;
            property.PropertyType.PrimitiveTypeFacets.IsUnicode = false;
            property.PropertyType.PrimitiveTypeFacets.MaxLength = 42;
            property.PropertyType.PrimitiveTypeFacets.Precision = 23;
            property.PropertyType.PrimitiveTypeFacets.Scale = 77;
            property.SetStoreGeneratedPattern(DbStoreGeneratedPattern.Identity);

            var databaseMapping = new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(model);

            var column = databaseMapping.GetEntityTypeMapping(entityType).TypeMappingFragments.Single().PropertyMappings.Single().Column;

            Assert.False(column.IsNullable);
            Assert.Null(column.Facets.IsFixedLength);
            Assert.Null(column.Facets.IsMaxLength);
            Assert.Null(column.Facets.IsUnicode);
            Assert.Null(column.Facets.MaxLength);
            Assert.Equal<byte?>(23, column.Facets.Precision);
            Assert.Equal<byte?>(77, column.Facets.Scale);
            Assert.Equal(DbStoreGeneratedPattern.Identity, column.StoreGeneratedPattern);
        }

        [Fact]
        public void Generate_should_map_entity_keys_to_primary_keys()
        {
            var model = new EdmModel().Initialize();
            var entityType = model.AddEntityType("E");
            entityType.SetClrType(typeof(object));
            var idProperty = entityType.AddPrimitiveProperty("Id");
            idProperty.PropertyType.EdmType = EdmPrimitiveType.Int32;
            entityType.DeclaredKeyProperties.Add(idProperty);
            var entitySet = model.AddEntitySet("ESet", entityType);

            var databaseMapping = new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(model);

            var entitySetMapping = databaseMapping.GetEntitySetMapping(entitySet);
            var entityTypeMapping = entitySetMapping.EntityTypeMappings.Single();

            Assert.Equal(1, entityTypeMapping.TypeMappingFragments.Single().Table.KeyColumns.Count());
            Assert.Equal("Id", entityTypeMapping.TypeMappingFragments.Single().Table.KeyColumns.Single().Name);
            Assert.True(entityTypeMapping.TypeMappingFragments.Single().Table.KeyColumns.Single().IsPrimaryKeyColumn);
        }

        [Fact]
        public void Generate_can_map_independent_association_type()
        {
            var model = new EdmModel().Initialize();
            var principalEntityType = model.AddEntityType("P");
            principalEntityType.SetClrType(typeof(object));
            var idProperty1 = principalEntityType.AddPrimitiveProperty("Id1");
            idProperty1.PropertyType.EdmType = EdmPrimitiveType.Int32;
            principalEntityType.DeclaredKeyProperties.Add(idProperty1);
            var idProperty2 = principalEntityType.AddPrimitiveProperty("Id2");
            idProperty2.PropertyType.EdmType = EdmPrimitiveType.String;
            principalEntityType.DeclaredKeyProperties.Add(idProperty2);
            var dependentEntityType = model.AddEntityType("D");
            dependentEntityType.SetClrType(typeof(string));
            model.AddEntitySet("PSet", principalEntityType);
            model.AddEntitySet("DSet", dependentEntityType);
            var associationType
                = model.AddAssociationType(
                    "P_D",
                    principalEntityType, EdmAssociationEndKind.Required,
                    dependentEntityType, EdmAssociationEndKind.Many);
            model.AddAssociationSet("P_DSet", associationType);
            associationType.SourceEnd.DeleteAction = EdmOperationAction.Cascade;

            var databaseMapping = new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(model);

            var foreignKeyConstraint
                =
                databaseMapping.GetEntityTypeMapping(dependentEntityType).TypeMappingFragments.Single().Table.ForeignKeyConstraints.Single();

            Assert.Equal(2, foreignKeyConstraint.DependentColumns.Count);
            Assert.Equal(associationType.Name, foreignKeyConstraint.Name);
            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().AssociationSetMappings.Count());
            Assert.Equal(DbOperationAction.Cascade, foreignKeyConstraint.DeleteAction);

            var foreignKeyColumn = foreignKeyConstraint.DependentColumns.First();

            Assert.False(foreignKeyColumn.IsNullable);
            Assert.Equal("P_Id1", foreignKeyColumn.Name);
        }

        [Fact]
        public void Generate_can_map_foreign_key_association_type()
        {
            var model = new EdmModel().Initialize();
            var principalEntityType = model.AddEntityType("P");
            principalEntityType.SetClrType(typeof(object));
            var dependentEntityType = model.AddEntityType("D");
            dependentEntityType.SetClrType(typeof(string));
            var dependentProperty1 = dependentEntityType.AddPrimitiveProperty("FK1");
            dependentProperty1.PropertyType.EdmType = EdmPrimitiveType.Int32;
            var dependentProperty2 = dependentEntityType.AddPrimitiveProperty("FK2");
            dependentProperty2.PropertyType.EdmType = EdmPrimitiveType.String;
            model.AddEntitySet("PSet", principalEntityType);
            model.AddEntitySet("DSet", dependentEntityType);
            var associationType
                = model.AddAssociationType(
                    "P_D",
                    principalEntityType, EdmAssociationEndKind.Required,
                    dependentEntityType, EdmAssociationEndKind.Many);
            associationType.Constraint
                = new EdmAssociationConstraint
                      {
                          DependentEnd = associationType.TargetEnd,
                          DependentProperties = new[] { dependentProperty1, dependentProperty2 },
                      };
            associationType.SourceEnd.DeleteAction = EdmOperationAction.Cascade;

            var databaseMapping = new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(model);

            var dependentTable = databaseMapping.GetEntityTypeMapping(dependentEntityType).TypeMappingFragments.Single().Table;
            var foreignKeyConstraint = dependentTable.ForeignKeyConstraints.Single();

            Assert.Equal(2, dependentTable.Columns.Count());
            Assert.Equal(2, foreignKeyConstraint.DependentColumns.Count);
            Assert.Equal(DbOperationAction.Cascade, foreignKeyConstraint.DeleteAction);
            Assert.Equal(associationType.Name, foreignKeyConstraint.Name);

            var foreignKeyColumn = foreignKeyConstraint.DependentColumns.First();

            Assert.False(foreignKeyColumn.IsNullable);
            Assert.Equal("FK1", foreignKeyColumn.Name);
        }

        [Fact]
        public void Generate_can_map_type_hierarchies_using_Tph()
        {
            var model = new EdmModel().Initialize();
            var rootEntityType = model.AddEntityType("E");
            rootEntityType.SetClrType(typeof(object));
            rootEntityType.AddPrimitiveProperty("P1").PropertyType.EdmType = EdmPrimitiveType.Int32;
            rootEntityType.AddPrimitiveProperty("P2").PropertyType.EdmType = EdmPrimitiveType.String;
            var entitySet = model.AddEntitySet("ESet", rootEntityType);
            var entityType2 = model.AddEntityType("E2");
            entityType2.AddPrimitiveProperty("P3").PropertyType.EdmType = EdmPrimitiveType.Decimal;
            entityType2.SetClrType(typeof(string));
            entityType2.BaseType = rootEntityType;
            var entityType3 = model.AddEntityType("E3");
            entityType3.SetClrType(typeof(int));
            entityType3.AddPrimitiveProperty("P4").PropertyType.EdmType = EdmPrimitiveType.Int32;
            entityType3.BaseType = entityType2;

            var databaseMapping = new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(model);

            var entitySetMapping = databaseMapping.GetEntitySetMapping(entitySet);

            Assert.NotNull(entitySetMapping);
            var entityTypeMappings = entitySetMapping.EntityTypeMappings;

            Assert.Equal(3, entityTypeMappings.Count);

            var entityType1Mapping = databaseMapping.GetEntityTypeMapping(rootEntityType);
            var entityType2Mapping = databaseMapping.GetEntityTypeMapping(entityType2);
            var entityType3Mapping = databaseMapping.GetEntityTypeMapping(entityType3);

            Assert.Equal(2, entityType1Mapping.TypeMappingFragments.Single().PropertyMappings.Count);
            Assert.Equal(3, entityType2Mapping.TypeMappingFragments.Single().PropertyMappings.Count);
            Assert.Equal(4, entityType3Mapping.TypeMappingFragments.Single().PropertyMappings.Count);

            var table = entityType1Mapping.TypeMappingFragments.Single().Table;
            Assert.Same(table, entityType2Mapping.TypeMappingFragments.Single().Table);
            Assert.Same(table, entityType3Mapping.TypeMappingFragments.Single().Table);
            Assert.Equal(5, table.Columns.Count);
            Assert.Equal("P1", table.Columns[0].Name);
            Assert.Equal("P2", table.Columns[1].Name);
            Assert.Equal("P3", table.Columns[2].Name);
            Assert.Equal("P4", table.Columns[3].Name);
            Assert.Equal("Discriminator", table.Columns[4].Name);
        }

        [Fact]
        public void Generate_maps_abstract_type_hierarchies_correctly()
        {
            var model = new EdmModel().Initialize();
            var rootEntityType = model.AddEntityType("E");
            rootEntityType.SetClrType(typeof(object));
            rootEntityType.AddPrimitiveProperty("P1").PropertyType.EdmType = EdmPrimitiveType.Int32;
            rootEntityType.DeclaredKeyProperties.Add(rootEntityType.Properties.First());
            rootEntityType.AddPrimitiveProperty("P2").PropertyType.EdmType = EdmPrimitiveType.String;
            model.AddEntitySet("ESet", rootEntityType);
            var entityType2 = model.AddEntityType("E2");
            entityType2.AddPrimitiveProperty("P3").PropertyType.EdmType = EdmPrimitiveType.Decimal;
            entityType2.SetClrType(typeof(string));
            entityType2.IsAbstract = true;
            entityType2.BaseType = rootEntityType;
            var entityType3 = model.AddEntityType("E3");
            entityType3.SetClrType(typeof(int));
            entityType3.AddPrimitiveProperty("P4").PropertyType.EdmType = EdmPrimitiveType.Int32;
            entityType3.BaseType = entityType2;

            var databaseMapping = new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(model);

            var entityType1Mapping = databaseMapping.GetEntityTypeMapping(rootEntityType);
            var entityType3Mapping = databaseMapping.GetEntityTypeMapping(entityType3);

            Assert.Equal(2, entityType1Mapping.TypeMappingFragments.Single().PropertyMappings.Count);
            Assert.Equal("P1", entityType1Mapping.TypeMappingFragments.Single().PropertyMappings[0].Column.Name);
            Assert.Equal("P2", entityType1Mapping.TypeMappingFragments.Single().PropertyMappings[1].Column.Name);

            Assert.Equal(4, entityType3Mapping.TypeMappingFragments.Single().PropertyMappings.Count);
            Assert.Equal("P1", entityType3Mapping.TypeMappingFragments.Single().PropertyMappings[0].Column.Name);
            Assert.Equal("P2", entityType3Mapping.TypeMappingFragments.Single().PropertyMappings[1].Column.Name);
            Assert.Equal("P3", entityType3Mapping.TypeMappingFragments.Single().PropertyMappings[2].Column.Name);
            Assert.Equal("P4", entityType3Mapping.TypeMappingFragments.Single().PropertyMappings[3].Column.Name);

            var table = entityType1Mapping.TypeMappingFragments.Single().Table;

            Assert.Equal(5, table.Columns.Count);
        }
    }
}
