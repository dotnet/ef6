// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public class StorageAssociationSetMappingTests
    {
        [Fact]
        public void Can_initialize_with_entity_set()
        {
            var entitySet = new EntitySet();
            var associationSet = new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace));

            var associationSetMapping
                = new StorageAssociationSetMapping(associationSet, entitySet);

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
                = new StorageAssociationSetMapping(associationSet, entitySet);

            Assert.Same(associationSet, associationSetMapping.AssociationSet);
        }

        [Fact]
        public void Can_get_and_set_store_entity_set()
        {
            var entitySet1 = new EntitySet();
            var associationSet = new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace));

            var associationSetMapping
                = new StorageAssociationSetMapping(associationSet, entitySet1);

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
                = new StorageAssociationSetMapping(associationSet, entitySet);

            Assert.Same(entityType, associationSetMapping.Table);
        }

        [Fact]
        public void Can_get_and_set_source_and_target_end_mappings()
        {
            var entitySet1 = new EntitySet();
            var associationSet = new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace));

            var associationSetMapping
                = new StorageAssociationSetMapping(associationSet, entitySet1);

            Assert.Null(associationSetMapping.SourceEndMapping);
            Assert.Null(associationSetMapping.TargetEndMapping);

            var sourceEndMapping = new StorageEndPropertyMapping();

            associationSetMapping.SourceEndMapping = sourceEndMapping;

            Assert.Same(sourceEndMapping, associationSetMapping.SourceEndMapping);

            var targetEndMapping = new StorageEndPropertyMapping();

            associationSetMapping.TargetEndMapping = targetEndMapping;

            Assert.Same(targetEndMapping, associationSetMapping.TargetEndMapping);
        }

        [Fact]
        public void Can_add_column_conditions()
        {
            var entitySet1 = new EntitySet();
            var associationSet = new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace));

            var associationSetMapping
                = new StorageAssociationSetMapping(associationSet, entitySet1);

            Assert.Empty(associationSetMapping.ColumnConditions);

            var conditionPropertyMapping
                = new StorageConditionPropertyMapping(null, new EdmProperty("C"), 42, null);

            associationSetMapping.AddColumnCondition(conditionPropertyMapping);

            Assert.Same(conditionPropertyMapping, associationSetMapping.ColumnConditions.Single());
        }
    }
}
