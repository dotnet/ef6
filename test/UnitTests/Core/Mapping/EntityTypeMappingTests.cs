// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class EntityTypeMappingTests
    {
        [Fact]
        public void Can_get_entity_type()
        {
            var entityTypeMapping 
                = new EntityTypeMapping(
                    new EntitySetMapping(new EntitySet(), new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace))));

            Assert.Null(entityTypeMapping.EntityType);

            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            entityTypeMapping.AddType(entityType);

            Assert.Same(entityType, entityTypeMapping.EntityType);
        }

        [Fact]
        public void Can_create_hierarchy_mappings()
        {
            var entityTypeMapping
                = new EntityTypeMapping(
                    new EntitySetMapping(new EntitySet(), new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace))));

            Assert.False(entityTypeMapping.IsHierarchyMapping);

            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            
            entityTypeMapping.AddIsOfType(entityType);

            Assert.True(entityTypeMapping.IsHierarchyMapping);

            entityTypeMapping.RemoveIsOfType(entityType);
            entityTypeMapping.AddType(entityType);

            Assert.False(entityTypeMapping.IsHierarchyMapping);

            var entityType2 = new EntityType("E2", "N", DataSpace.CSpace);
            entityTypeMapping.AddType(entityType2);

            Assert.True(entityTypeMapping.IsHierarchyMapping);
        }

        [Fact]
        public void AddType_throws_for_null_type()
        {
            var entityTypeMapping
                = new EntityTypeMapping(
                    new EntitySetMapping(
                        new EntitySet(), 
                        new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace))));

            Assert.Equal(
                "type",
                Assert.Throws<ArgumentNullException>(() => entityTypeMapping.AddType(null)).ParamName);
        }

        [Fact]
        public void Added_type_returned_in_type_collection()
        {
            var entityTypeMapping
                = new EntityTypeMapping(
                    new EntitySetMapping(
                        new EntitySet(),
                        new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace))));

            Assert.Empty(entityTypeMapping.Types);
            Assert.Empty(entityTypeMapping.IsOfTypes);

            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            
            entityTypeMapping.AddType(entityType);

            Assert.Same(entityType, entityTypeMapping.Types.Single());
            Assert.Empty(entityTypeMapping.IsOfTypes);
        }

        [Fact]
        public void Added_isOfType_returned_in_isOfType_collection()
        {
            var entityTypeMapping
                = new EntityTypeMapping(
                    new EntitySetMapping(
                        new EntitySet(),
                        new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace))));

            Assert.Empty(entityTypeMapping.Types);
            Assert.Empty(entityTypeMapping.IsOfTypes);

            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            entityTypeMapping.AddIsOfType(entityType);

            Assert.Same(entityType, entityTypeMapping.IsOfTypes.Single());
            Assert.Empty(entityTypeMapping.Types);
        }

        [Fact]
        public void Can_get_entity_set_mapping()
        {
            var entitySetMapping
                = new EntitySetMapping(
                    new EntitySet(),
                    new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)));
            var entityTypeMapping = new EntityTypeMapping(entitySetMapping);

            Assert.Same(entitySetMapping, entityTypeMapping.EntitySetMapping);
        }

        [Fact]
        public void Can_add_remove_type()
        {
            var entityTypeMapping
                = new EntityTypeMapping(
                    new EntitySetMapping(
                        new EntitySet(),
                        new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace))));

            Assert.Empty(entityTypeMapping.Types);

            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            entityTypeMapping.AddType(entityType);

            Assert.Same(entityType, entityTypeMapping.Types.Single());

            entityTypeMapping.RemoveType(entityType);

            Assert.Empty(entityTypeMapping.Types);
        }

        [Fact]
        public void Can_add_remove_isOfType()
        {
            var entityTypeMapping
                = new EntityTypeMapping(
                    new EntitySetMapping(
                        new EntitySet(),
                        new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace))));

            Assert.Empty(entityTypeMapping.IsOfTypes);

            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            entityTypeMapping.AddIsOfType(entityType);

            Assert.Same(entityType, entityTypeMapping.IsOfTypes.Single());

            entityTypeMapping.RemoveIsOfType(entityType);

            Assert.Empty(entityTypeMapping.IsOfTypes);
        }

        [Fact]
        public void Cannot_add_type_when_read_only()
        {
            var entityTypeMapping
                = new EntityTypeMapping(
                    new EntitySetMapping(
                        new EntitySet(),
                        new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace))));

            entityTypeMapping.SetReadOnly();

            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => entityTypeMapping.AddType(entityType)).Message);
        }

        [Fact]
        public void Cannot_remove_type_when_read_only()
        {
            var entityTypeMapping
                = new EntityTypeMapping(
                    new EntitySetMapping(
                        new EntitySet(),
                        new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace))));

            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            entityTypeMapping.AddType(entityType);

            entityTypeMapping.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => entityTypeMapping.RemoveType(entityType)).Message);
        }

        [Fact]
        public void Cannot_add_isOfType_when_read_only()
        {
            var entityTypeMapping
                = new EntityTypeMapping(
                    new EntitySetMapping(
                        new EntitySet(),
                        new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace))));

            entityTypeMapping.SetReadOnly();

            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => entityTypeMapping.AddIsOfType(entityType)).Message);
        }

        [Fact]
        public void Cannot_remove_isOfType_when_read_only()
        {
            var entityTypeMapping
                = new EntityTypeMapping(
                    new EntitySetMapping(
                        new EntitySet(),
                        new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace))));

            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            entityTypeMapping.AddIsOfType(entityType);

            entityTypeMapping.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => entityTypeMapping.RemoveIsOfType(entityType)).Message);
        }

        [Fact]
        public void SetReadOnly_is_called_on_child_mapping_items()
        {
            var entitySet = new EntitySet();
            var entityTypeMapping
                = new EntityTypeMapping(
                    new EntitySetMapping(
                        entitySet,
                        new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace))));
            var fragment = new MappingFragment(entitySet, entityTypeMapping, false);
            entityTypeMapping.AddFragment(fragment);

            Assert.False(fragment.IsReadOnly);
            entityTypeMapping.SetReadOnly();
            Assert.True(fragment.IsReadOnly);
        }
    }
}
