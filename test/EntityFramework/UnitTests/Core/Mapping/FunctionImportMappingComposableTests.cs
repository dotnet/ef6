// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using Xunit;

    public class FunctionImportMappingComposableTests
    {
        [Fact]
        public void Can_not_create_composable_function_import_mapping_for_non_composable_store_function()
        {
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

    }
}
