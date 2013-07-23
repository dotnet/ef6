// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Services
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Linq;
    using Xunit;

    public class ModificationFunctionMappingGeneratorTests
    {
        [Fact]
        public void Can_generate_function_mappings_for_entity_type()
        {
            var functionMappingGenerator
                = new ModificationFunctionMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest);

            var databaseMapping
                = new DbDatabaseMapping()
                    .Initialize(new EdmModel(DataSpace.CSpace), new EdmModel(DataSpace.SSpace));

            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            var intProperty = EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
            entityType.AddKeyMember(intProperty);

            var stringProperty = EdmProperty.CreatePrimitive("Name", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            entityType.AddMember(stringProperty);

            var entitySetMapping
                = databaseMapping.AddEntitySetMapping(
                    databaseMapping.Model.AddEntitySet("ES", entityType));

            var storageEntityTypeMapping 
                = new StorageEntityTypeMapping(
                    new StorageEntitySetMapping(new EntitySet(), databaseMapping.EntityContainerMappings.Single()));
           
            storageEntityTypeMapping.AddType(entityType);
            
            var storageMappingFragment = new StorageMappingFragment(new EntitySet(), storageEntityTypeMapping, false);
            
            storageMappingFragment.AddColumnMapping(
                new ColumnMappingBuilder(new EdmProperty("C0"), new[] { intProperty }));

            storageMappingFragment.AddColumnMapping(
                new ColumnMappingBuilder(new EdmProperty("C1"), new[] { stringProperty }));

            storageEntityTypeMapping.AddFragment(storageMappingFragment);

            entitySetMapping.AddTypeMapping(storageEntityTypeMapping);

            functionMappingGenerator.Generate(entityType, databaseMapping);

            var modificationFunctionMapping
                = entitySetMapping.ModificationFunctionMappings.Single();

            Assert.NotNull(modificationFunctionMapping);

            var functionMapping = modificationFunctionMapping.InsertFunctionMapping;

            Assert.NotNull(functionMapping);
            Assert.Equal(2, functionMapping.ParameterBindings.Count);
            Assert.Null(functionMapping.ResultBindings);

            var function = functionMapping.Function;

            Assert.NotNull(function);
            Assert.Equal("E_Insert", function.Name);
            Assert.Equal(2, function.Parameters.Count);

            functionMapping = modificationFunctionMapping.UpdateFunctionMapping;

            Assert.NotNull(functionMapping);
            Assert.Equal(2, functionMapping.ParameterBindings.Count);
            Assert.Null(functionMapping.ResultBindings);

            function = functionMapping.Function;

            Assert.NotNull(function);
            Assert.Equal("E_Update", function.Name);
            Assert.Equal(2, function.Parameters.Count);

            functionMapping = modificationFunctionMapping.DeleteFunctionMapping;

            Assert.NotNull(functionMapping);
            Assert.Equal(1, functionMapping.ParameterBindings.Count);
            Assert.Null(functionMapping.ResultBindings);

            function = modificationFunctionMapping.DeleteFunctionMapping.Function;

            Assert.NotNull(function);
            Assert.Equal("E_Delete", function.Name);
            Assert.Equal(1, function.Parameters.Count);
        }

        [Fact]
        public void Generate_should_exclude_sgp_properties_from_corresponding_function_mappings()
        {
            var functionMappingGenerator
                = new ModificationFunctionMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest);

            var databaseMapping
                = new DbDatabaseMapping()
                    .Initialize(new EdmModel(DataSpace.CSpace), new EdmModel(DataSpace.SSpace));

            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            var intProperty = EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
            intProperty.SetStoreGeneratedPattern(StoreGeneratedPattern.Identity);
            entityType.AddKeyMember(intProperty);

            var stringProperty = EdmProperty.CreatePrimitive("Name", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            stringProperty.SetStoreGeneratedPattern(StoreGeneratedPattern.Computed);
            entityType.AddMember(stringProperty);

            var entitySetMapping
                = databaseMapping.AddEntitySetMapping(
                    databaseMapping.Model.AddEntitySet("ES", entityType));

            var storageEntityTypeMapping
                = new StorageEntityTypeMapping(
                    new StorageEntitySetMapping(new EntitySet(), databaseMapping.EntityContainerMappings.Single()));

            storageEntityTypeMapping.AddType(entityType);

            var storageMappingFragment = new StorageMappingFragment(new EntitySet(), storageEntityTypeMapping, false);

            storageMappingFragment.AddColumnMapping(
                new ColumnMappingBuilder(new EdmProperty("C0"), new[] { intProperty }));

            storageMappingFragment.AddColumnMapping(
                new ColumnMappingBuilder(new EdmProperty("C1"), new[] { stringProperty }));

            storageEntityTypeMapping.AddFragment(storageMappingFragment);

            entitySetMapping.AddTypeMapping(storageEntityTypeMapping);

            functionMappingGenerator.Generate(entityType, databaseMapping);

            var modificationFunctionMapping
                = entitySetMapping.ModificationFunctionMappings.Single();

            Assert.NotNull(modificationFunctionMapping);

            var functionMapping = modificationFunctionMapping.InsertFunctionMapping;

            Assert.NotNull(functionMapping);
            Assert.Equal(0, functionMapping.ParameterBindings.Count);
            Assert.Equal(2, functionMapping.ResultBindings.Count);

            var function = functionMapping.Function;

            Assert.NotNull(function);
            Assert.Equal("E_Insert", function.Name);
            Assert.Equal(0, function.Parameters.Count);

            functionMapping = modificationFunctionMapping.UpdateFunctionMapping;

            Assert.NotNull(functionMapping);
            Assert.Equal(1, functionMapping.ParameterBindings.Count);
            Assert.Equal(1, functionMapping.ResultBindings.Count);

            function = functionMapping.Function;

            Assert.NotNull(function);
            Assert.Equal("E_Update", function.Name);
            Assert.Equal(1, function.Parameters.Count);

            functionMapping = modificationFunctionMapping.DeleteFunctionMapping;

            Assert.NotNull(functionMapping);
            Assert.Equal(1, functionMapping.ParameterBindings.Count);
            Assert.Null(functionMapping.ResultBindings);

            function = modificationFunctionMapping.DeleteFunctionMapping.Function;

            Assert.NotNull(function);
            Assert.Equal("E_Delete", function.Name);
            Assert.Equal(1, function.Parameters.Count);
        }

        [Fact]
        public void Can_generate_function_mappings_for_many_to_many_association_set_mapping()
        {
            var databaseMapping
                = new DbDatabaseMapping()
                    .Initialize(new EdmModel(DataSpace.CSpace), new EdmModel(DataSpace.SSpace));

            var entityType1 = databaseMapping.Model.AddEntityType("E1");
            entityType1.Annotations.SetClrType(typeof(string));
            databaseMapping.Model.AddEntitySet("E1Set", entityType1);

            var entityType2 = databaseMapping.Model.AddEntityType("E2");
            entityType2.Annotations.SetClrType(typeof(string));
            databaseMapping.Model.AddEntitySet("E2Set", entityType2);

            var entityTypeConfiguration = new EntityTypeConfiguration(typeof(object));
            entityTypeConfiguration.MapToStoredProcedures();
            entityType1.SetConfiguration(entityTypeConfiguration);
            entityType2.SetConfiguration(entityTypeConfiguration);

            var associationSet
                = databaseMapping.Model.AddAssociationSet(
                    "M2MSet",
                    databaseMapping.Model.AddAssociationType(
                        "M2M",
                        entityType1,
                        RelationshipMultiplicity.Many,
                        entityType2,
                        RelationshipMultiplicity.Many));

            var entitySet = new EntitySet("ES", "S", null, null, new EntityType("E", "N", DataSpace.CSpace));

            var associationEndMember1 = new AssociationEndMember("Source", new EntityType("E", "N", DataSpace.CSpace));
            associationSet.AddAssociationSetEnd(new AssociationSetEnd(entitySet, associationSet, associationEndMember1));

            var associationEndMember2 = new AssociationEndMember("Target", new EntityType("E", "N", DataSpace.CSpace));
            associationSet.AddAssociationSetEnd(new AssociationSetEnd(entitySet, associationSet, associationEndMember2));

            var associationSetMapping
                = new StorageAssociationSetMapping(
                    associationSet,
                    entitySet)
                      {
                          SourceEndMapping
                              = new StorageEndPropertyMapping()
                                    {
                                        EndMember = associationEndMember1
                                    },
                          TargetEndMapping
                              = new StorageEndPropertyMapping()
                                    {
                                        EndMember = associationEndMember2
                                    }
                      };

            associationSetMapping.SourceEndMapping
                                 .AddProperty(new StorageScalarPropertyMapping(new EdmProperty("PK"), new EdmProperty("FooId")));

            associationSetMapping.TargetEndMapping
                                 .AddProperty(new StorageScalarPropertyMapping(new EdmProperty("PK"), new EdmProperty("BarId")));

            var functionMappingGenerator
                = new ModificationFunctionMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest);

            functionMappingGenerator.Generate(associationSetMapping, databaseMapping);

            var modificationFunctionMapping
                = associationSetMapping.ModificationFunctionMapping;

            Assert.NotNull(modificationFunctionMapping);

            var functionMapping = modificationFunctionMapping.InsertFunctionMapping;

            Assert.NotNull(functionMapping);
            Assert.Equal(2, functionMapping.ParameterBindings.Count);
            Assert.Null(functionMapping.ResultBindings);

            var function = functionMapping.Function;

            Assert.NotNull(function);
            Assert.Equal("E1E2_Insert", function.Name);
            Assert.Equal(2, function.Parameters.Count);

            functionMapping = modificationFunctionMapping.DeleteFunctionMapping;

            Assert.NotNull(functionMapping);
            Assert.Equal(2, functionMapping.ParameterBindings.Count);
            Assert.Null(functionMapping.ResultBindings);

            function = modificationFunctionMapping.DeleteFunctionMapping.Function;

            Assert.NotNull(function);
            Assert.Equal("E1E2_Delete", function.Name);
            Assert.Equal(2, function.Parameters.Count);
        }
    }
}
