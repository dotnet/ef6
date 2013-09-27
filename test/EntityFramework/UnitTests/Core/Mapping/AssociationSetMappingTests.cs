// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class AssociationSetMappingTests
    {
        [Fact]
        public void Can_initialize_with_entity_set()
        {
            var entitySet = new EntitySet();
            var associationSet = new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace));

            var associationSetMapping
                = new AssociationSetMapping(associationSet, entitySet);

            var associationTypeMapping = associationSetMapping.TypeMappings.Single();

            Assert.NotNull(associationTypeMapping);
            Assert.Same(associationSet.ElementType, associationTypeMapping.Types.Single());
            Assert.Same(associationSetMapping, associationTypeMapping.SetMapping);

            var mappingFragment = associationTypeMapping.MappingFragments.Single();

            Assert.Same(entitySet, mappingFragment.TableSet);
        }

        [Fact]
        public void Can_get_association_set()
        {
            var entitySet = new EntitySet();
            var associationSet = new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace));

            var associationSetMapping
                = new AssociationSetMapping(associationSet, entitySet);

            Assert.Same(associationSet, associationSetMapping.AssociationSet);
        }

        [Fact]
        public void Can_get_and_set_store_entity_set()
        {
            var entitySet1 = new EntitySet();
            var associationSet = new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace));

            var associationSetMapping
                = new AssociationSetMapping(associationSet, entitySet1);

            Assert.Same(entitySet1, associationSetMapping.StoreEntitySet);

            var entitySet2 = new EntitySet();

            associationSetMapping.StoreEntitySet = entitySet2;

            Assert.Same(entitySet2, associationSetMapping.StoreEntitySet);
        }

        [Fact]
        public void Can_get_table()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var entitySet = new EntitySet("ES", null, null, null, entityType);
            var associationSet = new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace));

            var associationSetMapping
                = new AssociationSetMapping(associationSet, entitySet);

            Assert.Same(entityType, associationSetMapping.Table);
        }

        [Fact]
        public void Can_get_and_set_source_and_target_end_mappings()
        {
            var entitySet1 = new EntitySet();
            var associationSet = new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace));

            var associationSetMapping
                = new AssociationSetMapping(associationSet, entitySet1);

            Assert.Null(associationSetMapping.SourceEndMapping);
            Assert.Null(associationSetMapping.TargetEndMapping);

            var sourceEndMapping = new EndPropertyMapping();

            associationSetMapping.SourceEndMapping = sourceEndMapping;

            Assert.Same(sourceEndMapping, associationSetMapping.SourceEndMapping);

            var targetEndMapping = new EndPropertyMapping();

            associationSetMapping.TargetEndMapping = targetEndMapping;

            Assert.Same(targetEndMapping, associationSetMapping.TargetEndMapping);
        }

        [Fact]
        public void Can_add_get_remove_column_conditions()
        {
            var entitySet1 = new EntitySet();
            var associationSet = new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace));

            var associationSetMapping
                = new AssociationSetMapping(associationSet, entitySet1);

            Assert.Empty(associationSetMapping.Conditions);

            var conditionPropertyMapping
                = new ConditionPropertyMapping(null, new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })), 42, null);

            associationSetMapping.AddCondition(conditionPropertyMapping);

            Assert.Same(conditionPropertyMapping, associationSetMapping.Conditions.Single());

            associationSetMapping.RemoveCondition(conditionPropertyMapping);

            Assert.Empty(associationSetMapping.Conditions);
        }

        [Fact]
        public void Cannot_set_source_end_mapping_when_read_only()
        {
            var entitySet = new EntitySet();
            var associationSet = new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace));
            var associationSetMapping
                = new AssociationSetMapping(associationSet, entitySet);
            var sourceEndMapping = new EndPropertyMapping();

            associationSetMapping.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => (associationSetMapping.SourceEndMapping = sourceEndMapping)).Message);
        }

        [Fact]
        public void Cannot_set_target_end_mapping_when_read_only()
        {
            var entitySet = new EntitySet();
            var associationSet = new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace));
            var associationSetMapping
                = new AssociationSetMapping(associationSet, entitySet);
            var targetEndMapping = new EndPropertyMapping();

            associationSetMapping.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => (associationSetMapping.TargetEndMapping = targetEndMapping)).Message);
        }

        [Fact]
        public void Cannot_set__modification_function_mapping_when_read_only()
        {
            var entitySet = new EntitySet();
            var associationSet = new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace));
            var associationSetMapping
                = new AssociationSetMapping(associationSet, entitySet);
            var modificationFunctionMapping = new AssociationSetModificationFunctionMapping(associationSet, null, null);

            associationSetMapping.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => (associationSetMapping.ModificationFunctionMapping = modificationFunctionMapping)).Message);
        }

        [Fact]
        public void Cannot_add_condition_when_read_only()
        {
            var entitySet = new EntitySet();
            var associationSet = new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace));
            var associationSetMapping
                = new AssociationSetMapping(associationSet, entitySet);
            var conditionPropertyMapping
                = new ConditionPropertyMapping(null, new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })), 42, null);

            associationSetMapping.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => associationSetMapping.AddCondition(conditionPropertyMapping)).Message);
        }

        [Fact]
        public void Cannot_remove_condition_when_read_only()
        {
            var entitySet = new EntitySet();
            var associationSet = new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace));
            var associationSetMapping
                = new AssociationSetMapping(associationSet, entitySet);
            var conditionPropertyMapping
                = new ConditionPropertyMapping(null, new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })), 42, null);

            associationSetMapping.AddCondition(conditionPropertyMapping);
            associationSetMapping.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => associationSetMapping.RemoveCondition(conditionPropertyMapping)).Message);
        }
    }
}
