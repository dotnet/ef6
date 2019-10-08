// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using Xunit;

    public class FunctionImportMappingNonComposableTests
    {
        [Fact]
        public void Can_not_create_non_composable_function_import_mapping_with_null_store_function()
        {
            Assert.Equal(
                "functionImport",
                Assert.Throws<ArgumentNullException>(
                    () => new FunctionImportMappingNonComposable(
                        null,
                        new EdmFunction("f", "store", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = false }),
                        new List<FunctionImportResultMapping>(), 
                        new EntityContainerMapping())).ParamName);
        }

        [Fact]
        public void Can_not_create_non_composable_function_import_mapping_with_null_function_import()
        {
            Assert.Equal(
                "targetFunction",
                Assert.Throws<ArgumentNullException>(
                    () => new FunctionImportMappingNonComposable(
                        new EdmFunction("f", "entityModel", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = false }),
                        null,
                        new List<FunctionImportResultMapping>(),
                        new EntityContainerMapping())).ParamName);
        }

        [Fact]
        public void Can_not_create_non_composable_function_import_mapping_with_null_result_mappings()
        {
            Assert.Equal(
                "resultMappings",
                Assert.Throws<ArgumentNullException>(
                    () => new FunctionImportMappingNonComposable(
                        new EdmFunction("f", "entityModel", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = false }),
                        new EdmFunction("f", "store", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = false }),
                        null,
                        new EntityContainerMapping())).ParamName);
        }

        [Fact]
        public void Can_not_create_non_composable_function_import_mapping_with_null_container_mapping()
        {
            Assert.Equal(
                "containerMapping",
                Assert.Throws<ArgumentNullException>(
                    () => new FunctionImportMappingNonComposable(
                        new EdmFunction("f", "entityModel", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = false }),
                        new EdmFunction("f", "store", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = false }),
                        new List<FunctionImportResultMapping>(), 
                        null)).ParamName);
        }

        [Fact]
        public void Can_create_non_composable_function_with_multiple_results()
        {
            DbProviderManifest providerManifest;
            var containerMapping = GetContainerMapping(out providerManifest);

            var cTypeUsageInt = TypeUsage.Create(
                PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32),
                new[]
                {
                    Facet.Create(MetadataItem.NullableFacetDescription, false) 
                });
            var sTypeUsageInt = TypeUsage.Create(
                providerManifest.GetStoreType(cTypeUsageInt).EdmType,
                new[]
                {
                    Facet.Create(MetadataItem.NullableFacetDescription, false) 
                });
            var cTypeUsageString = TypeUsage.CreateDefaultTypeUsage(
                PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var sTypeUsageString = providerManifest.GetStoreType(cTypeUsageString);

            var complexType = ComplexType.Create(
                "RT1", "N", DataSpace.CSpace,
                new[]
                {
                    EdmProperty.Create("P1", cTypeUsageInt),
                    EdmProperty.Create("P2", cTypeUsageString)
                },
                null);

            var entityType = EntityType.Create(
                "RT2", "N", DataSpace.CSpace,
                new[] { "P3" },
                new[]
                {
                    EdmProperty.Create("P3", cTypeUsageInt),
                    EdmProperty.Create("P4", cTypeUsageString),
                },
                null);

            var rowType1 = RowType.Create(
                new[]
                {
                    EdmProperty.Create("C1", sTypeUsageInt),
                    EdmProperty.Create("C2", sTypeUsageString)
                },
                null);

            var rowType2 = RowType.Create(
                new[]
                {
                    EdmProperty.Create("C3", sTypeUsageInt),
                    EdmProperty.Create("C4", sTypeUsageString)
                },
                null);

            var functionImport = EdmFunction.Create(
                "F", "N", DataSpace.CSpace,
                new EdmFunctionPayload
                {
                    IsComposable = false,
                    ReturnParameters = new[]
                    {
                        FunctionParameter.Create("R1", complexType.GetCollectionType(), ParameterMode.ReturnValue),
                        FunctionParameter.Create("R2", entityType.GetCollectionType(), ParameterMode.ReturnValue) 
                    },
                    EntitySets = new[]
                    {
                        new EntitySet(),
                        new EntitySet() 
                    }
                },
                null);

            var targetFunction = EdmFunction.Create(
                "SF", "N", DataSpace.SSpace,
                new EdmFunctionPayload
                {
                    IsComposable = false,
                    ReturnParameters = new[]
                    { 
                        FunctionParameter.Create("R1", rowType1.GetCollectionType(), ParameterMode.ReturnValue),
                        FunctionParameter.Create("R2", rowType2.GetCollectionType(), ParameterMode.ReturnValue)
                    },
                    EntitySets = new []
                    {
                        new EntitySet(),
                        new EntitySet() 
                    }
                },
                null);

            var resultMappings
                = new List<FunctionImportResultMapping>
                  {
                      new FunctionImportResultMapping(),
                      new FunctionImportResultMapping()
                  };

            resultMappings[0].AddTypeMapping(new FunctionImportComplexTypeMapping(
                complexType,
                new Collection<FunctionImportReturnTypePropertyMapping>()
                {
                    new FunctionImportReturnTypeScalarPropertyMapping("P1", "C1"),
                    new FunctionImportReturnTypeScalarPropertyMapping("P2", "C2"),
                }));

            resultMappings[1].AddTypeMapping(new FunctionImportEntityTypeMapping(
                Enumerable.Empty<EntityType>(),
                new [] { entityType },
                new Collection<FunctionImportReturnTypePropertyMapping>()
                {
                    new FunctionImportReturnTypeScalarPropertyMapping("P3", "C3"),
                    new FunctionImportReturnTypeScalarPropertyMapping("P4", "C4")
                },
                Enumerable.Empty<FunctionImportEntityTypeMappingCondition>()));

            var functionImportMapping = new FunctionImportMappingNonComposable(
                functionImport,
                targetFunction,
                resultMappings,
                containerMapping);

            Assert.Equal(resultMappings.Count, functionImportMapping.ResultMappings.Count);

            functionImportMapping.ResultMappings.Each(m => Assert.False(m.IsReadOnly));
            functionImportMapping.SetReadOnly();
            functionImportMapping.ResultMappings.Each(m => Assert.True(m.IsReadOnly));
        }

        private static EntityContainerMapping GetContainerMapping(out DbProviderManifest providerManifest)
        {
            using (var context = new Context())
            {
                var metadataWorkspace = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;

                var storeItemCollection = (StoreItemCollection)metadataWorkspace.GetItemCollection(DataSpace.SSpace);
                providerManifest = storeItemCollection.ProviderManifest;

                var mappingItemCollection = (StorageMappingItemCollection)
                    metadataWorkspace.GetItemCollection(DataSpace.CSSpace);

                return mappingItemCollection.GetItems<EntityContainerMapping>().Single();
            }
        }

        public class E
        {
            public int Id { get; set; }
            public string P { get; set; }
        }

        public class Context : DbContext
        {
            static Context()
            {
                Database.SetInitializer<Context>(null);
            }

            public DbSet<E> Entities { get; set; }
        }
    }
}
