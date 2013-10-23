// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public class SimpleMappingContextTests
    {
        [Fact]
        public void Store_model_initialized()
        {
            var storeModel = new EdmModel(DataSpace.SSpace);
            var mappingContext = new SimpleMappingContext(storeModel, true);
            Assert.Same(storeModel, mappingContext.StoreModel);
            Assert.True(mappingContext.IncludeForeignKeyProperties);
        }

        [Fact]
        public void Can_add_get_error()
        {
            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);
            Assert.Empty(mappingContext.Errors);

            var error = new EdmSchemaError("bar", 0xF00, EdmSchemaErrorSeverity.Warning);
            mappingContext.Errors.Add(error);

            Assert.Same(error, mappingContext.Errors.Single());
        }

        [Fact]
        public void Can_add_and_get_property_mapping()
        {
            var p1 = EdmProperty.CreatePrimitive("p1", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
            var p2 = EdmProperty.CreatePrimitive("p2", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);
            mappingContext.AddMapping(p1, p2);
            Assert.Same(p2, mappingContext[p1]);
        }

        [Fact]
        public void Can_add_and_get_entity_type_mapping()
        {
            var e1 = CreateEntityType("e1");
            var e2 = CreateEntityType("e2");

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);
            Assert.Empty(mappingContext.ConceptualEntityTypes());
            mappingContext.AddMapping(e1, e2);
            Assert.Same(e2, mappingContext[e1]);
            Assert.Same(e2, mappingContext.ConceptualEntityTypes().Single());
        }

        [Fact]
        public void Can_add_and_get_entity_set_mapping()
        {
            var dummy = CreateEntityType("e");
            var es1 = EntitySet.Create("es1", null, null, null, dummy, null);
            var es2 = EntitySet.Create("es2", null, null, null, dummy, null);

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);
            mappingContext.AddMapping(es1, es2);
            Assert.Same(es2, mappingContext[es1]);

            EntitySet outEntitySet;

            Assert.True(mappingContext.TryGetValue(es1, out outEntitySet));
            Assert.Same(es2, outEntitySet);

            Assert.False(mappingContext.TryGetValue(es2, out outEntitySet));
            Assert.Null(outEntitySet);
        }

        [Fact]
        public void Can_add_and_get_entity_container_mapping()
        {
            var es1 = EntitySet.Create("es1", null, null, null, CreateEntityType("e"), null);
            var ec1 = EntityContainer.Create("ec1", DataSpace.CSpace, new[] { es1 }, null, null);

            var es2 = EntitySet.Create("es1", null, null, null, CreateEntityType("e"), null);
            var ec2 = EntityContainer.Create("ec2", DataSpace.CSpace, new[] { es2 }, null, null);

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);
            mappingContext.AddMapping(ec1, ec2);
            Assert.Same(ec2, mappingContext[ec1]);
        }

        [Fact]
        public void Can_add_and_get_association_type_mapping()
        {
            var at1 = AssociationType.Create("at1", "ns", false, DataSpace.CSpace, null, null, null, null);
            var at2 = AssociationType.Create("at2", "ns", false, DataSpace.CSpace, null, null, null, null);

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);
            mappingContext.AddMapping(at1, at2);

            Assert.Same(at2, mappingContext[at1]);
        }

        [Fact]
        public void Can_add_and_get_association_set_mapping()
        {
            var at1 = AssociationType.Create("at1", "ns", false, DataSpace.CSpace, null, null, null, null);
            var at2 = AssociationType.Create("at2", "ns", false, DataSpace.CSpace, null, null, null, null);
            var as1 = AssociationSet.Create("as1", at1, null, null, null);
            var as2 = AssociationSet.Create("as2", at2, null, null, null);

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);
            mappingContext.AddMapping(as1, as2);

            Assert.Same(as2, mappingContext[as1]);
        }

        [Fact]
        public void Can_add_and_get_association_end_member_mapping()
        {
            var et1 = CreateEntityType("et1");
            var et2 = CreateEntityType("et2");
            var aem1 = AssociationEndMember.Create("aem1", et1.GetReferenceType(), RelationshipMultiplicity.One, OperationAction.None, null);
            var aem2 = AssociationEndMember.Create("aem2", et2.GetReferenceType(), RelationshipMultiplicity.One, OperationAction.None, null);

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);
            mappingContext.AddMapping(aem1, aem2);

            Assert.Same(aem2, mappingContext[aem1]);
        }

        [Fact]
        public void Can_add_and_get_association_set_end_mapping()
        {
            var et1 = CreateEntityType("et1");
            var et2 = CreateEntityType("et2");
            var es1 = EntitySet.Create("es1", null, null, null, et1, null);
            var es2 = EntitySet.Create("es2", null, null, null, et2, null);
            var aem1 = AssociationEndMember.Create("aem1", et1.GetReferenceType(), RelationshipMultiplicity.One, OperationAction.None, null);
            var aem2 = AssociationEndMember.Create("aem2", et2.GetReferenceType(), RelationshipMultiplicity.One, OperationAction.None, null);
            var at1 = AssociationType.Create("at1", "ns", false, DataSpace.CSpace, aem1, aem2, null, null);
            var as1 = AssociationSet.Create("as1", at1, es1, es2, null);

            Assert.Equal(2, as1.AssociationSetEnds.Count);
            var ase1 = as1.AssociationSetEnds[0];
            var ase2 = as1.AssociationSetEnds[1];

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);
            mappingContext.AddMapping(ase1, ase2);

            Assert.Same(ase2, mappingContext[ase1]);
        }

        [Fact]
        public void Can_add_get_association_type_mapping()
        {
            var storeAssociationType =
                AssociationType.Create("storeAssociationType", "ns.Store", false, DataSpace.SSpace, null, null, null, null);
            var conceptualAssociationType =
                AssociationType.Create("conceptualAssociationType", "ns", false, DataSpace.CSpace, null, null, null, null);

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);
            mappingContext.AddMapping(storeAssociationType, conceptualAssociationType);

            Assert.Same(conceptualAssociationType, mappingContext[storeAssociationType]);

            AssociationType outAssociationType;
            Assert.True(mappingContext.TryGetValue(storeAssociationType, out outAssociationType));
            Assert.Same(conceptualAssociationType, outAssociationType);

            Assert.False(mappingContext.TryGetValue(conceptualAssociationType, out outAssociationType));
            Assert.Null(outAssociationType);
        }

        [Fact]
        public void Can_add_get_association_set_mapping()
        {
            var storeAssociationSet =
                AssociationSet.Create(
                    "storeAssociationSet",
                    AssociationType.Create("storeAssociationType", "ns.Store", false, DataSpace.SSpace, null, null, null, null),
                    null, null, null);

            var conceptualAssociationSet =
                AssociationSet.Create(
                    "conceptualAssociationSet",
                    AssociationType.Create("conceptualAssociationType", "ns", false, DataSpace.CSpace, null, null, null, null),
                    null, null, null);

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);
            mappingContext.AddMapping(storeAssociationSet, conceptualAssociationSet);

            Assert.Same(conceptualAssociationSet, mappingContext[storeAssociationSet]);
        }

        [Fact]
        public void Can_add_and_get_function_mapping()
        {
            var storeFunction = EdmFunction.Create("fs", "ns", DataSpace.SSpace, new EdmFunctionPayload(), null);
            var functionImport = EdmFunction.Create("fs", "ns", DataSpace.SSpace, new EdmFunctionPayload(), null);

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);
            Assert.Empty(mappingContext.MappedStoreFunctions());

            mappingContext.AddMapping(storeFunction, functionImport);
            Assert.Same(functionImport, mappingContext[storeFunction]);
            Assert.Same(storeFunction, mappingContext.MappedStoreFunctions().Single());
        }

        [Fact]
        public void Removing_entity_set_mapping_removes_corresponding_entity_type()
        {
            var storeEntity = CreateEntityType("storeEntity");
            var modelEntity = CreateEntityType("modelEntity");
            var storeEntitySet = EntitySet.Create("storeEntitySet", null, null, null, storeEntity, null);
            var modelEntitySet = EntitySet.Create("modelEntitySet", null, null, null, modelEntity, null);

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);
            mappingContext.AddMapping(storeEntity, modelEntity);
            mappingContext.AddMapping(storeEntitySet, modelEntitySet);

            Assert.Same(modelEntitySet, mappingContext.ConceptualEntitySets().Single());
            Assert.Same(modelEntity, mappingContext.ConceptualEntityTypes().Single());

            mappingContext.RemoveMapping(storeEntitySet);

            Assert.Empty(mappingContext.ConceptualEntitySets());
            Assert.Empty(mappingContext.ConceptualEntityTypes());
        }

        [Fact]
        public void Can_add_and_get_mapping_for_collapsed_entity_sets()
        {
            var storeEntity = CreateEntityType("storeEntity");
            var storeEntitySet = EntitySet.Create("storeEntitySet", null, null, null, storeEntity, null);
            var collapsibleAssociationSet = new CollapsibleEntityAssociationSets(storeEntitySet);

            var associationType = AssociationType.Create("modelAssociationType", "ns", false, DataSpace.CSpace, null, null, null, null);
            var associationSet = AssociationSet.Create("modelAssociationType", associationType, null, null, null);

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);
            mappingContext.AddMapping(collapsibleAssociationSet, associationSet);

            Assert.Same(associationSet, mappingContext[collapsibleAssociationSet]);
        }

        [Fact]
        public void ConceptualAssociationSets_returns_associationsets_for_collapsed_entity_sets()
        {
            var storeEntity = CreateEntityType("storeEntity");
            var storeEntitySet = EntitySet.Create("storeEntitySet", null, null, null, storeEntity, null);
            var collapsibleAssociationSet = new CollapsibleEntityAssociationSets(storeEntitySet);

            var associationType = AssociationType.Create("modelAssociationType", "ns", false, DataSpace.CSpace, null, null, null, null);
            var associationSet = AssociationSet.Create("modelAssociationType", associationType, null, null, null);

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);
            Assert.Empty(mappingContext.ConceptualAssociationSets());

            mappingContext.AddMapping(collapsibleAssociationSet, associationSet);
            Assert.Equal(1, mappingContext.ConceptualAssociationSets().Count());
            Assert.Same(associationSet, mappingContext.ConceptualAssociationSets().Single());
        }

        private static EntityType CreateEntityType(string name)
        {
            return EntityType.Create(name, "ns", DataSpace.CSpace, null, null, null);
        }
    }
}
