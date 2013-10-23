// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Linq;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery;
    using Moq;
    using Xunit;

    public class ModelGeneratorTests
    {
        [Fact]
        public void CreateStoreModel_creates_store_model()
        {
            var mockBuilderSettings = new Mock<ModelBuilderSettings>();
            mockBuilderSettings.Setup(s => s.RuntimeProviderInvariantName).Returns("System.Data.SqlClient");
            mockBuilderSettings.Object.ProviderManifestToken = "2008";
            mockBuilderSettings.Object.TargetSchemaVersion = EntityFrameworkVersion.Version3;
            mockBuilderSettings.Object.IncludeForeignKeysInModel = true;

            var mockModelGenerator =
                new Mock<ModelGenerator>(mockBuilderSettings.Object, "storeNamespace")
                    {
                        CallBase = true
                    };

            mockModelGenerator
                .Setup(g => g.GetStoreSchemaDetails(It.IsAny<StoreSchemaConnectionFactory>()))
                .Returns(
                    new StoreSchemaDetails(
                        Enumerable.Empty<TableDetailsRow>(),
                        Enumerable.Empty<TableDetailsRow>(),
                        Enumerable.Empty<RelationshipDetailsRow>(),
                        Enumerable.Empty<FunctionDetailsRowView>(),
                        Enumerable.Empty<TableDetailsRow>()));

            var model = mockModelGenerator.Object.CreateStoreModel();

            Assert.NotNull(model);
            Assert.Equal("System.Data.SqlClient", model.ProviderInfo.ProviderInvariantName);
            Assert.Equal("2008", model.ProviderInfo.ProviderManifestToken);
            mockModelGenerator.Verify(g => g.GetStoreSchemaDetails(It.IsAny<StoreSchemaConnectionFactory>()), Times.Once());
        }

        [Fact]
        public void GenerateModel_genertes_model_and_sets_all_the_properties()
        {
            var mockModelGenerator = new Mock<ModelGenerator>(new ModelBuilderSettings(), "storeNamespace");

            var storeModel = new EdmModel(DataSpace.SSpace);
            var mappingContext = new SimpleMappingContext(storeModel, true);
            mappingContext.AddMapping(
                storeModel.Containers.Single(),
                EntityContainer.Create("C", DataSpace.CSpace, null, null, null));

            mockModelGenerator
                .Setup(g => g.CreateStoreModel())
                .Returns(() => storeModel);
            mockModelGenerator
                .Setup(g => g.CreateMappingContext(It.Is<EdmModel>(model => model == storeModel)))
                .Returns(() => mappingContext);

            var errors = new List<EdmSchemaError>();
            var databaseMapping = mockModelGenerator.Object.GenerateModel(errors).DatabaseMapping;
            Assert.Same(storeModel, databaseMapping.Database);
            Assert.NotNull(databaseMapping.Model);
            Assert.Equal(1, databaseMapping.EntityContainerMappings.Count);
            mockModelGenerator.Verify(
                g => g.CreateMappingContext(It.IsAny<EdmModel>()), Times.Once());
            Assert.Empty(errors);
        }

        [Fact]
        public void GenerateModel_combines_store_model_and_mapping_errors()
        {
            var storeModelError = new EdmSchemaError("storeError", 42, EdmSchemaErrorSeverity.Error);
            var errorMetadataProperty =
                MetadataProperty.Create(
                    MetadataItemHelper.SchemaErrorsMetadataPropertyName,
                    TypeUsage.CreateDefaultTypeUsage(
                        PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String).GetCollectionType()),
                    new List<EdmSchemaError> { storeModelError });

            var entityType =
                EntityType.Create(
                    "foo", "bar", DataSpace.SSpace, new string[0], new EdmMember[0],
                    new[] { errorMetadataProperty });

            var storeModel = new EdmModel(DataSpace.SSpace);
            storeModel.AddItem(entityType);

            var mappingContext = new SimpleMappingContext(storeModel, true);
            mappingContext.AddMapping(
                storeModel.Containers.Single(),
                EntityContainer.Create("C", DataSpace.CSpace, null, null, null));
            mappingContext.Errors.Add(new EdmSchemaError("mappingError", 911, EdmSchemaErrorSeverity.Warning));

            var mockModelGenerator = new Mock<ModelGenerator>(new ModelBuilderSettings(), "storeNamespace");
            mockModelGenerator
                .Setup(g => g.CreateStoreModel())
                .Returns(() => storeModel);

            mockModelGenerator
                .Setup(g => g.CreateMappingContext(It.IsAny<EdmModel>()))
                .Returns(() => mappingContext);

            var errors = new List<EdmSchemaError>();
            mockModelGenerator.Object.GenerateModel(errors);
            Assert.Equal(new[] { storeModelError, mappingContext.Errors.Single() }, errors);
        }

        [Fact]
        public void CreateMappingContext_creates_mapping_context()
        {
            var storeModel = new EdmModel(DataSpace.SSpace);

            var mappingContext =
                new ModelGenerator(new ModelBuilderSettings(), "storeNamespace")
                    .CreateMappingContext(storeModel);

            Assert.NotNull(mappingContext);
            Assert.Same(storeModel, mappingContext.StoreModel);
        }

        [Fact]
        public void Mapping_context_created_with_CreateMappingContext_will_pluralize_if_pluralization_enabled()
        {
            var entityType = EntityType.Create("entities", "ns", DataSpace.SSpace, new string[0], new EdmMember[0], null);
            var entitySet = EntitySet.Create("entitySet", "dbo", "t", null, entityType, null);
            var container = EntityContainer.Create("container", DataSpace.SSpace, new[] { entitySet }, null, null);

            var storeModel = EdmModel.CreateStoreModel(container, null, null);

            var mappingContext =
                new ModelGenerator(new ModelBuilderSettings { UsePluralizationService = true }, "storeNamespace")
                    .CreateMappingContext(storeModel);

            Assert.Equal("entity", mappingContext[entityType].Name);
            Assert.Equal("entitySets", mappingContext[entitySet].Name);
        }

        [Fact]
        public void Mapping_context_created_with_CreateMappingContext_will_not_pluralize_if_pluralization_disabled()
        {
            var entityType = EntityType.Create("entities", "ns", DataSpace.SSpace, new string[0], new EdmMember[0], null);
            var entitySet = EntitySet.Create("entitySet", "dbo", "t", null, entityType, null);
            var container = EntityContainer.Create("container", DataSpace.SSpace, new[] { entitySet }, null, null);

            var storeModel = EdmModel.CreateStoreModel(container, null, null);

            var mappingContext =
                new ModelGenerator(new ModelBuilderSettings { UsePluralizationService = false }, "storeNamespace")
                    .CreateMappingContext(storeModel);

            Assert.Equal("entities", mappingContext[entityType].Name);
            Assert.Equal("entitySet", mappingContext[entitySet].Name);
        }

        [Fact]
        public void CollectStoreModelErrors_returns_empty_error_list_for_model_without_errors()
        {
            var entityType =
                EntityType.Create("foo", "bar", DataSpace.SSpace, new string[0], new EdmMember[0], null);

            var model = new EdmModel(DataSpace.SSpace);
            model.AddItem(entityType);

            var schemaErrors = ModelGenerator.CollectStoreModelErrors(model);

            Assert.NotNull(schemaErrors);
            Assert.Empty(schemaErrors);
        }

        [Fact]
        public void CollectStoreModelErrors_returns_errors_on_model_items()
        {
            var edmSchemaError = new EdmSchemaError("msg", 42, EdmSchemaErrorSeverity.Error);
            var errorMetadataProperty =
                MetadataProperty.Create(
                    MetadataItemHelper.SchemaErrorsMetadataPropertyName,
                    TypeUsage.CreateDefaultTypeUsage(
                        PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String).GetCollectionType()),
                    new List<EdmSchemaError> { edmSchemaError });

            var entityType =
                EntityType.Create(
                    "foo", "bar", DataSpace.SSpace, new string[0], new EdmMember[0],
                    new[] { errorMetadataProperty });

            var model = new EdmModel(DataSpace.SSpace);
            model.AddItem(entityType);

            var schemaErrors = ModelGenerator.CollectStoreModelErrors(model);

            Assert.NotNull(schemaErrors);
            Assert.Equal(1, schemaErrors.Count);
            Assert.Same(edmSchemaError, schemaErrors.Single());
        }

        [Fact]
        public void CollectStoreModelErrors_returns_errors_from_function_return_rowtypes()
        {
            var edmSchemaError = new EdmSchemaError("msg", 42, EdmSchemaErrorSeverity.Error);
            var errorMetadataProperty =
                MetadataProperty.Create(
                    MetadataItemHelper.SchemaErrorsMetadataPropertyName,
                    TypeUsage.CreateDefaultTypeUsage(
                        PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String).GetCollectionType()),
                    new List<EdmSchemaError> { edmSchemaError });

            var rowType = RowType.Create(new EdmProperty[0], new[] { errorMetadataProperty });

            var function =
                EdmFunction.Create(
                    "foo",
                    "bar",
                    DataSpace.SSpace,
                    new EdmFunctionPayload
                        {
                            ReturnParameters =
                                new[]
                                    {
                                        FunctionParameter.Create(
                                            "ReturnType",
                                            rowType,
                                            ParameterMode.ReturnValue)
                                    }
                        },
                    null);

            var model = new EdmModel(DataSpace.SSpace);
            model.AddItem(function);

            var schemaErrors = ModelGenerator.CollectStoreModelErrors(model);

            Assert.NotNull(schemaErrors);
            Assert.Equal(1, schemaErrors.Count);
            Assert.Same(edmSchemaError, schemaErrors.Single());
        }

        [Fact]
        public void GetStoreSchemaDetails_uses_correct_version_of_store_schema_version()
        {
            var mockEntityConnectionObject = new Mock<EntityConnection>().Object;
            var mockBuilderSettings = new Mock<ModelBuilderSettings>();
            mockBuilderSettings.Setup(s => s.RuntimeProviderInvariantName).Returns("fakeInvariantName");
            mockBuilderSettings.Object.TargetSchemaVersion = EntityFrameworkVersion.Version3;

            var mockConnectionFactory = new Mock<StoreSchemaConnectionFactory>();

            var version = EntityFrameworkVersion.Version1;
            mockConnectionFactory.Setup(
                f => f.Create(
                    It.IsAny<IDbDependencyResolver>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Version>(), out version))
                .Returns(mockEntityConnectionObject);

            var mockModelGenerator =
                new Mock<ModelGenerator>(mockBuilderSettings.Object, "modelNs")
                    {
                        CallBase = true
                    };

            mockModelGenerator
                .Setup(g => g.CreateDbSchemaLoader(It.IsAny<EntityConnection>(), It.IsAny<Version>()))
                .Returns(new Mock<EntityStoreSchemaGeneratorDatabaseSchemaLoader>(mockEntityConnectionObject, version).Object);

            mockModelGenerator.Object.GetStoreSchemaDetails(mockConnectionFactory.Object);

            // the idea is that we should use version returned as from Create method as the out parameter and not 
            // the one on model builder settings. 
            mockModelGenerator
                .Verify(
                    g => g.CreateDbSchemaLoader(
                        It.IsAny<EntityConnection>(),
                        It.Is<Version>(v => v == EntityFrameworkVersion.Version1)),
                    Times.Once());
        }
    }
}
