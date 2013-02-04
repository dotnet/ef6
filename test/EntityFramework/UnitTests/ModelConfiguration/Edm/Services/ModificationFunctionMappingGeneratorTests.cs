// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Services.UnitTests
{
    using System.Data.Entity.Core.Metadata.Edm;
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

            var entityType = new EntityType();

            var intProperty = EdmProperty.Primitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
            entityType.AddKeyMember(intProperty);

            var stringProperty = EdmProperty.Primitive("Name", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            entityType.AddMember(stringProperty);

            var entitySetMapping
                = databaseMapping.AddEntitySetMapping(
                    databaseMapping.Model.AddEntitySet("ES", entityType));

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

            var entityType = new EntityType();

            var intProperty = EdmProperty.Primitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
            intProperty.SetStoreGeneratedPattern(StoreGeneratedPattern.Identity);
            entityType.AddKeyMember(intProperty);

            var stringProperty = EdmProperty.Primitive("Name", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            stringProperty.SetStoreGeneratedPattern(StoreGeneratedPattern.Computed);
            entityType.AddMember(stringProperty);

            var entitySetMapping
                = databaseMapping.AddEntitySetMapping(
                    databaseMapping.Model.AddEntitySet("ES", entityType));

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
    }
}
