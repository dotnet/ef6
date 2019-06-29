// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using System.Runtime.Remoting.Channels;
    using FunctionalTests.Model;
    using Xunit;
    using System.Data.Entity.Resources;

    public class EntityContainerMappingTests
    {

        [Fact]
        public void Cannot_initialize_with_null_entity_container()
        {
            Assert.Equal(
                "entityContainer",
                Assert.Throws<ArgumentNullException>(
                    () => new EntityContainerMapping(null, null, null, false, false)).ParamName);
        }

        [Fact]
        public void Can_get_store_and_entity_containers()
        {
            var entityContainer = new EntityContainer("C", DataSpace.CSpace);
            var storeContainer = new EntityContainer("S", DataSpace.CSpace);
            var entityContainerMapping = 
                new EntityContainerMapping(entityContainer, storeContainer, null, false, false);

            Assert.Same(entityContainer, entityContainerMapping.EdmEntityContainer);
            Assert.Same(storeContainer, entityContainerMapping.StorageEntityContainer);
        }

        [Fact]
        public void BuiltinTypeKind_is_MetadataItem()
        {
            Assert.Equal(
                BuiltInTypeKind.MetadataItem,
                new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)).BuiltInTypeKind);
        }

        [Fact]
        public void Can_get_entity_set_mappings()
        {
            var entityContainerMapping = new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace));

            Assert.Empty(entityContainerMapping.EntitySetMappings);
            Assert.Empty(entityContainerMapping.EntitySetMaps);

            var entitySetMapping
                = new EntitySetMapping(
                    new EntitySet("ES", null, null, null, new EntityType("E", "N", DataSpace.CSpace)), entityContainerMapping);

            entityContainerMapping.AddSetMapping(entitySetMapping);

            Assert.Same(entitySetMapping, entityContainerMapping.EntitySetMappings.Single());
            Assert.Same(entitySetMapping, entityContainerMapping.EntitySetMaps.Single());
        }

        [Fact]
        public void Can_get_association_set_mappings()
        {
            var entityContainerMapping = new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace));

            Assert.Empty(entityContainerMapping.AssociationSetMappings);
            Assert.Empty(entityContainerMapping.RelationshipSetMaps);

            var associationSetMapping
                = new AssociationSetMapping(
                    new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)), entityContainerMapping);

            entityContainerMapping.AddSetMapping(associationSetMapping);

            Assert.Same(associationSetMapping, entityContainerMapping.AssociationSetMappings.Single());
            Assert.Same(associationSetMapping, entityContainerMapping.RelationshipSetMaps.Single());
        }

        [Fact]
        public void Can_add_and_get_function_import_mapping()
        {
            var typeUsage = 
                TypeUsage.CreateDefaultTypeUsage(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32).GetCollectionType());

            var entityContainerMapping = new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace));

            var composableFunctionMapping =
                new FunctionImportMappingComposable(
                    new EdmFunction(
                        "f", "model", DataSpace.CSpace,
                        new EdmFunctionPayload()
                            {
                                IsComposable = true,
                                ReturnParameters =
                                    new[]
                                        {
                                            new FunctionParameter(
                                                "ReturnType",
                                                typeUsage,
                                                ParameterMode.ReturnValue),
                                        }

                            }),
                    new EdmFunction(
                        "f", "store", DataSpace.SSpace,
                        new EdmFunctionPayload()
                            {
                                IsComposable = true,
                                ReturnParameters =
                                    new[]
                                        {
                                            new FunctionParameter(
                                                "ReturnType",
                                                typeUsage,
                                                ParameterMode.ReturnValue),
                                        }
                            }),
                    null);

            Assert.Empty(entityContainerMapping.FunctionImportMappings);
            entityContainerMapping.AddFunctionImportMapping(composableFunctionMapping);
            Assert.Same(composableFunctionMapping, entityContainerMapping.FunctionImportMappings.Single());
        }

        [Fact]
        public void Cannot_add_null_function_import_mapping()
        {
            Assert.Equal(
                "functionImportMapping",
                Assert.Throws<ArgumentNullException>(
                    () => new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)).AddFunctionImportMapping(null))
                      .ParamName);
        }

        [Fact]
        public void SetReadOnly_is_called_on_child_mapping_items()
        {
            var conceptualContainer = new EntityContainer("C", DataSpace.CSpace);
            var storeContainer = new EntityContainer("S", DataSpace.CSpace);
            var containerMapping = new EntityContainerMapping(conceptualContainer, storeContainer, null, false);

            var entitySet
                = new EntitySet(
                    "ES", "S", "T", "Q",
                    new EntityType("ET", "N", DataSpace.SSpace));
            var entitySetMapping = new EntitySetMapping(entitySet, containerMapping);
            var associationSetMapping
                = new AssociationSetMapping(
                    new AssociationSet(
                        "AS",
                        new AssociationType("AT", "N", false, DataSpace.CSpace)),
                    entitySet);
            var functionImporMapping 
                = new FunctionImportMappingFake(
                    new EdmFunction("FI", "N", DataSpace.CSpace),
                    new EdmFunction("TF", "N", DataSpace.SSpace));

            containerMapping.AddSetMapping(entitySetMapping);
            containerMapping.AddSetMapping(associationSetMapping);
            containerMapping.AddFunctionImportMapping(functionImporMapping);

            Assert.False(containerMapping.IsReadOnly);
            Assert.False(entitySetMapping.IsReadOnly);
            Assert.False(associationSetMapping.IsReadOnly);
            Assert.False(functionImporMapping.IsReadOnly);

            containerMapping.SetReadOnly();

            Assert.True(containerMapping.IsReadOnly);
            Assert.True(entitySetMapping.IsReadOnly);
            Assert.True(associationSetMapping.IsReadOnly);
            Assert.True(functionImporMapping.IsReadOnly);
        }

        private class FunctionImportMappingFake : FunctionImportMapping
        {
            public FunctionImportMappingFake(EdmFunction functionImport, EdmFunction targetFunction)
                : base(functionImport, targetFunction)
            {
            }
        }
    }
}
