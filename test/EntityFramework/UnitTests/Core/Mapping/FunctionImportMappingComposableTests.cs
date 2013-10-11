// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class FunctionImportMappingComposableTests
    {
        [Fact]
        public void Can_not_create_composable_function_import_mapping_with_null_store_function()
        {
            Assert.Equal(
                "functionImport",
                Assert.Throws<ArgumentNullException>(
                    () => new FunctionImportMappingComposable(
                        null,
                        new EdmFunction("f", "store", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = true }),
                        new FunctionImportResultMapping(),
                        new EntityContainerMapping())).ParamName);
        }

        [Fact]
        public void Can_not_create_composable_function_import_mapping_with_null_function_import()
        {
            Assert.Equal(
                "targetFunction",
                Assert.Throws<ArgumentNullException>(
                    () => new FunctionImportMappingComposable(
                        new EdmFunction("f", "entityModel", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = true }),
                        null,
                        new FunctionImportResultMapping(),
                        new EntityContainerMapping())).ParamName);
        }

        [Fact]
        public void Can_not_create_composable_function_import_mapping_with_null_result_mapping()
        {
            Assert.Equal(
                "resultMapping",
                Assert.Throws<ArgumentNullException>(
                    () => new FunctionImportMappingComposable(
                        new EdmFunction("f", "entityModel", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = true }),
                        new EdmFunction("f", "store", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = true }),
                        null,
                        new EntityContainerMapping())).ParamName);
        }

        [Fact]
        public void Can_not_create_composable_function_import_mapping_with_null_container_mapping()
        {
            Assert.Equal(
                "containerMapping",
                Assert.Throws<ArgumentNullException>(
                    () => new FunctionImportMappingComposable(
                        new EdmFunction("f", "entityModel", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = true }),
                        new EdmFunction("f", "store", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = true }),
                        new FunctionImportResultMapping(),
                        null)).ParamName);
        }

        [Fact]
        public void Can_not_create_composable_function_import_mapping_for_non_composable_store_function()
        {
            Assert.Equal(
                Strings.NonComposableFunctionCannotBeMappedAsComposable("targetFunction"),
                Assert.Throws<ArgumentException>(
                    () => new FunctionImportMappingComposable(
                        new EdmFunction("f", "entityModel", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = true }),
                        new EdmFunction("f", "store", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = false }),
                        new FunctionImportResultMapping(),
                        new EntityContainerMapping())).Message);

            Assert.Equal(
                Strings.NonComposableFunctionCannotBeMappedAsComposable("targetFunction"),
                Assert.Throws<ArgumentException>(
                    () => new FunctionImportMappingComposable(
                        new EdmFunction("f", "entityModel", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = true }),
                        new EdmFunction("f", "store", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = false }), 
                        null)).Message);
        }

        [Fact]
        public void Can_not_create_composable_function_import_mapping_non_composable_function_import()
        {
            Assert.Equal(
                Strings.NonComposableFunctionCannotBeMappedAsComposable("functionImport"),
                Assert.Throws<ArgumentException>(
                    () => new FunctionImportMappingComposable(
                        new EdmFunction("f", "entityModel", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = false }),
                        new EdmFunction("f", "store", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = true }),
                        new FunctionImportResultMapping(),
                        new EntityContainerMapping())).Message);

            Assert.Equal(
                Strings.NonComposableFunctionCannotBeMappedAsComposable("functionImport"),
                Assert.Throws<ArgumentException>(
                    () => new FunctionImportMappingComposable(
                        new EdmFunction("f", "entityModel", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = false }),
                        new EdmFunction("f", "store", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = true }),
                        null)).Message);
        }

        [Fact]
        public void Can_not_create_composable_function_import_mapping_with_entity_set_and_null_structural_type_mappings()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var entitySet = new EntitySet("ES", null, null, null, entityType);

            var functionImport =
                new EdmFunction(
                    "f",
                    "entityModel",
                    DataSpace.CSpace,
                    new EdmFunctionPayload
                        {
                            IsComposable = true,
                            EntitySets = new[] { entitySet },
                            ReturnParameters =
                                new[]
                                    {
                                        new FunctionParameter(
                                            "ReturnType", 
                                            TypeUsage.CreateDefaultTypeUsage(entityType), 
                                            ParameterMode.ReturnValue)
                                    }
                        });

            Assert.Equal(
                Strings.ComposableFunctionImportsReturningEntitiesNotSupported,
                Assert.Throws<NotSupportedException>(
                    () => new FunctionImportMappingComposable(
                              functionImport,
                              new EdmFunction(
                              "f", "store", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = true }),
                              new FunctionImportResultMapping(),
                              new EntityContainerMapping())).Message);

            Assert.Equal(
                Strings.ComposableFunctionImportsReturningEntitiesNotSupported,
                Assert.Throws<NotSupportedException>(
                    () => new FunctionImportMappingComposable(
                              functionImport,
                              new EdmFunction(
                              "f", "store", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = true }),
                              null)).Message);
        }

        [Fact]
        public void Can_not_create_mapping_for_function_import_returning_invalid_type()
        {
            var functionImport =
                new EdmFunction(
                    "f",
                    "entityModel",
                    DataSpace.CSpace,
                    new EdmFunctionPayload
                    {
                        IsComposable = true,
                        ReturnParameters =
                            new[]
                                    {
                                        new FunctionParameter(
                                            "ReturnType",                                            
                                            TypeUsage.CreateDefaultTypeUsage(new RowType()), 
                                            ParameterMode.ReturnValue)
                                    }
                    });

            Assert.Equal(
                Strings.InvalidReturnTypeForComposableFunction,
                Assert.Throws<ArgumentException>(
                    () => new FunctionImportMappingComposable(
                              functionImport,
                              new EdmFunction(
                              "f", "store", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = true }),
                              new FunctionImportResultMapping(),
                              new EntityContainerMapping())).Message);

            Assert.Equal(
                Strings.InvalidReturnTypeForComposableFunction,
                Assert.Throws<ArgumentException>(
                    () => new FunctionImportMappingComposable(
                              functionImport,
                              new EdmFunction(
                              "f", "store", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = true }),
                              null)).Message);
        }


        [Fact]
        public void Structural_type_mappings_must_be_not_null_if_function_returns_non_scalar_values()
        {
            var complexType = new ComplexType("CT", "ns", DataSpace.CSpace);

            var functionImport =
                new EdmFunction(
                    "f",
                    "entityModel",
                    DataSpace.CSpace,
                    new EdmFunctionPayload
                    {
                        IsComposable = true,
                        ReturnParameters =
                            new[]
                                    {
                                        new FunctionParameter(
                                            "ReturnType", 
                                            TypeUsage.CreateDefaultTypeUsage(complexType.GetCollectionType()), 
                                            ParameterMode.ReturnValue)
                                    }
                    });

            var structualTypeMappingTestValues =
                new[]
                    {
                        null,
                        new List<Tuple<StructuralType, List<ConditionPropertyMapping>, List<PropertyMapping>>>()
                    };

            foreach (var structuralTypeMappings in structualTypeMappingTestValues)

            Assert.Equal(
                Strings.StructuralTypeMappingsMustNotBeNullForFunctionImportsReturingNonScalarValues,
                Assert.Throws<ArgumentException>(
                    () => new FunctionImportMappingComposable(
                              functionImport,
                              new EdmFunction(
                              "f", "store", DataSpace.CSpace, new EdmFunctionPayload { IsComposable = true }),
                              structuralTypeMappings)).Message);
        }

        [Fact]
        public void Can_get_structural_type_mappings()
        {
            var complexType = new ComplexType("CT", "ns", DataSpace.CSpace);

            var functionImport =
                new EdmFunction(
                    "f",
                    "entityModel",
                    DataSpace.CSpace,
                    new EdmFunctionPayload
                        {
                            IsComposable = true,
                            ReturnParameters =
                                new[]
                                    {
                                        new FunctionParameter(
                                            "ReturnType",
                                            TypeUsage.CreateDefaultTypeUsage(complexType.GetCollectionType()),
                                            ParameterMode.ReturnValue)
                                    }
                        });

            var structuralTypeMapping =
                new List<Tuple<StructuralType, List<ConditionPropertyMapping>, List<PropertyMapping>>>
                    {
                        new Tuple<StructuralType, List<ConditionPropertyMapping>, List<PropertyMapping>>(
                            complexType, null, null)
                    };

            var functionImportMapping =
                new FunctionImportMappingComposable(
                    functionImport,
                    new EdmFunction(
                        "f", "store", DataSpace.CSpace,
                        new EdmFunctionPayload
                            {
                                IsComposable = true
                            }),
                    structuralTypeMapping);

            Assert.Equal(structuralTypeMapping, functionImportMapping.StructuralTypeMappings);
        }

        [Fact]
        public void Can_create_composable_function_import_with_scalar_collection_result()
        {
            DbProviderManifest providerManifest;
            var containerMapping = GetContainerMapping(out providerManifest);

            var cTypeUsageString = TypeUsage.CreateDefaultTypeUsage(
                PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var sTypeUsageString = providerManifest.GetStoreType(cTypeUsageString);

            var rowType = RowType.Create(
                new[]
                {
                    EdmProperty.Create("C", sTypeUsageString),
                },
                null);

            var functionImport = EdmFunction.Create(
                "F", "N", DataSpace.CSpace,
                new EdmFunctionPayload
                {
                    IsComposable = true,
                    ReturnParameters = new[]
                    {
                        FunctionParameter.Create("R", cTypeUsageString.EdmType.GetCollectionType(), ParameterMode.ReturnValue) 
                    }
                },
                null);

            var targetFunction = EdmFunction.Create(
                "SF", "N", DataSpace.SSpace,
                new EdmFunctionPayload
                {
                    IsComposable = true,
                    ReturnParameters = new[]
                    { 
                        FunctionParameter.Create("R", rowType.GetCollectionType(), ParameterMode.ReturnValue)
                    }
                },
                null);

            var resultMapping = new FunctionImportResultMapping();

            var functionImportMapping = new FunctionImportMappingComposable(
                functionImport,
                targetFunction,
                resultMapping,
                containerMapping);

            Assert.Same(resultMapping, functionImportMapping.ResultMapping);
            Assert.Null(functionImportMapping.StructuralTypeMappings);
            Assert.Null(functionImportMapping.TvfKeys);
        }

        [Fact]
        public void Can_create_composable_function_import_with_complex_type_collection_result()
        {
            DbProviderManifest providerManifest;
            var containerMapping = GetContainerMapping(out providerManifest);

            var cTypeUsageString = TypeUsage.CreateDefaultTypeUsage(
                PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var sTypeUsageString = providerManifest.GetStoreType(cTypeUsageString);

            var complexType = ComplexType.Create(
                "RT", "N", DataSpace.CSpace,
                new[]
                {
                    EdmProperty.Create("P1", cTypeUsageString),
                    EdmProperty.Create("P2", cTypeUsageString)
                },
                null);


            var rowType = RowType.Create(
                new[]
                {
                    EdmProperty.Create("C1", sTypeUsageString),
                    EdmProperty.Create("C2", sTypeUsageString)
                },
                null);

            var functionImport = EdmFunction.Create(
                "F", "N", DataSpace.CSpace,
                new EdmFunctionPayload
                {
                    IsComposable = true,
                    ReturnParameters = new[]
                    {
                        FunctionParameter.Create("R", complexType.GetCollectionType(), ParameterMode.ReturnValue) 
                    }
                },
                null);

            var targetFunction = EdmFunction.Create(
                "SF", "N", DataSpace.SSpace,
                new EdmFunctionPayload
                {
                    IsComposable = true,
                    ReturnParameters = new[]
                    { 
                        FunctionParameter.Create("R", rowType.GetCollectionType(), ParameterMode.ReturnValue)
                    }
                },
                null);

            var typeMapping = new FunctionImportComplexTypeMapping(
                complexType,
                new Collection<FunctionImportReturnTypePropertyMapping>()
                {
                    new FunctionImportReturnTypeScalarPropertyMapping("P1", "C1"),
                    new FunctionImportReturnTypeScalarPropertyMapping("P2", "C2"),
                });

            var resultMapping = new FunctionImportResultMapping();
            resultMapping.AddTypeMapping(typeMapping);

            var functionImportMapping = new FunctionImportMappingComposable(
                functionImport,
                targetFunction,
                resultMapping,
                containerMapping);

            Assert.Same(resultMapping, functionImportMapping.ResultMapping);
            Assert.Equal(1, functionImportMapping.StructuralTypeMappings.Count);
            Assert.Null(functionImportMapping.TvfKeys);
        }

        [Fact]
        public void Can_create_composable_function_import_with_entity_type_collection_result()
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

            var entityType = EntityType.Create(
                "RT", "N", DataSpace.CSpace,
                new[] { "PId" },
                new[]
                {
                    EdmProperty.Create("PId", cTypeUsageInt),
                    EdmProperty.Create("P", cTypeUsageInt),
                },
                null);


            var rowType = RowType.Create(
                new[]
                {
                    EdmProperty.Create("CId", sTypeUsageInt),
                    EdmProperty.Create("C", sTypeUsageInt),
                },
                null);

            var functionImport = EdmFunction.Create(
                "F", "N", DataSpace.CSpace,
                new EdmFunctionPayload
                {
                    IsComposable = true,
                    ReturnParameters = new[]
                    {
                        FunctionParameter.Create("R", entityType.GetCollectionType(), ParameterMode.ReturnValue) 
                    }
                },
                null);

            var targetFunction = EdmFunction.Create(
                "SF", "N", DataSpace.SSpace,
                new EdmFunctionPayload
                {
                    IsComposable = true,
                    ReturnParameters = new[]
                    { 
                        FunctionParameter.Create("R", rowType.GetCollectionType(), ParameterMode.ReturnValue)
                    }
                },
                null);

            var typeMapping = new FunctionImportEntityTypeMapping(
                Enumerable.Empty<EntityType>(),
                new[] { entityType },
                new Collection<FunctionImportReturnTypePropertyMapping>()
                {
                    new FunctionImportReturnTypeScalarPropertyMapping("PId", "CId"),
                    new FunctionImportReturnTypeScalarPropertyMapping("P", "C"),
                },
                Enumerable.Empty<FunctionImportEntityTypeMappingCondition>());

            var resultMapping = new FunctionImportResultMapping();
            resultMapping.AddTypeMapping(typeMapping);

            var functionImportMapping = new FunctionImportMappingComposable(
                functionImport,
                targetFunction,
                resultMapping,
                containerMapping);

            Assert.Same(resultMapping, functionImportMapping.ResultMapping);
            Assert.Equal(1, functionImportMapping.StructuralTypeMappings.Count);
            Assert.Equal(1, functionImportMapping.TvfKeys.Length);
        }

        [Fact]
        public void Can_create_composable_function_import_with_entity_type_hierarchy()
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

            var itemCollection = containerMapping.StorageMappingItemCollection.EdmItemCollection.GetItems<EntityType>();
            var baseEntityType = itemCollection.Single(et => et.Name == "E");
            var entityType1 = itemCollection.Single(et => et.Name == "E1");
            var entityType2 = itemCollection.Single(et => et.Name == "E2");

            var rowType = RowType.Create(
                new[]
                {
                    EdmProperty.Create("CId", sTypeUsageInt),
                    EdmProperty.Create("C", sTypeUsageString),
                    EdmProperty.Create("C1", sTypeUsageString),
                    EdmProperty.Create("C2", sTypeUsageString),
                    EdmProperty.Create("CD", sTypeUsageString)
                },
                null);

            var functionImport = EdmFunction.Create(
                "F", "N", DataSpace.CSpace,
                new EdmFunctionPayload
                {
                    IsComposable = true,
                    ReturnParameters = new[]
                    {
                        FunctionParameter.Create("R", baseEntityType.GetCollectionType(), ParameterMode.ReturnValue) 
                    }
                },
                null);

            var targetFunction = EdmFunction.Create(
                "SF", "N", DataSpace.SSpace,
                new EdmFunctionPayload
                {
                    IsComposable = true,
                    ReturnParameters = new[]
                    { 
                        FunctionParameter.Create("R", rowType.GetCollectionType(), ParameterMode.ReturnValue)
                    }
                },
                null);

            var resultMapping = new FunctionImportResultMapping();

            var typeMapping = new FunctionImportEntityTypeMapping(
                new[] { baseEntityType },
                Enumerable.Empty<EntityType>(),
                new Collection<FunctionImportReturnTypePropertyMapping>()
                {
                    new FunctionImportReturnTypeScalarPropertyMapping("Id", "CId"),
                    new FunctionImportReturnTypeScalarPropertyMapping("P", "C"),
                    new FunctionImportReturnTypeScalarPropertyMapping("Discriminator", "CD"),
                },
                Enumerable.Empty<FunctionImportEntityTypeMappingConditionValue>());
            resultMapping.AddTypeMapping(typeMapping);

            typeMapping = new FunctionImportEntityTypeMapping(
                Enumerable.Empty<EntityType>(),
                new[] { entityType1 },
                new Collection<FunctionImportReturnTypePropertyMapping>()
                {
                    new FunctionImportReturnTypeScalarPropertyMapping("P1", "C1"),
                },
                new  []
                {
                    new FunctionImportEntityTypeMappingConditionValue("CD", "E1")
                });
            resultMapping.AddTypeMapping(typeMapping);

            typeMapping = new FunctionImportEntityTypeMapping(
                Enumerable.Empty<EntityType>(),
                new[] { entityType2 },
                new Collection<FunctionImportReturnTypePropertyMapping>()
                {
                    new FunctionImportReturnTypeScalarPropertyMapping("P2", "C2"),
                },
                new []
                {
                    new FunctionImportEntityTypeMappingConditionValue("CD", "E2")
                });
            resultMapping.AddTypeMapping(typeMapping);

            var functionImportMapping = new FunctionImportMappingComposable(
                functionImport,
                targetFunction,
                resultMapping,
                containerMapping);

            Assert.Same(resultMapping, functionImportMapping.ResultMapping);
            Assert.Equal(2, functionImportMapping.StructuralTypeMappings.Count);            
            Assert.Equal(1, functionImportMapping.TvfKeys.Length);

            Assert.Equal(typeof(E1).Name, functionImportMapping.StructuralTypeMappings[0].Item1.Name);
            Assert.Equal(1, functionImportMapping.StructuralTypeMappings[0].Item2.Count());
            Assert.Equal(3, functionImportMapping.StructuralTypeMappings[0].Item3.Count());

            Assert.Equal(typeof(E2).Name, functionImportMapping.StructuralTypeMappings[1].Item1.Name);
            Assert.Equal(1, functionImportMapping.StructuralTypeMappings[0].Item2.Count());
            Assert.Equal(3, functionImportMapping.StructuralTypeMappings[0].Item3.Count());
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

        public abstract class E
        {
            public int Id { get; set; }
            public string P { get; set; }
        }

        public class E1 : E
        {
            public string P1 { get; set; }
        }

        public class E2 : E
        {
            public string P2 { get; set; }
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
