// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
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

            entityTypeMapping.AddType(entityType);
            entityTypeMapping.AddIsOfType(entityType);

            Assert.True(entityTypeMapping.IsHierarchyMapping);

            entityTypeMapping.RemoveIsOfType(entityType);

            Assert.False(entityTypeMapping.IsHierarchyMapping);
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
    }
}
