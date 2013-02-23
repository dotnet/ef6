// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class StorageEntityTypeMappingTests
    {
        [Fact]
        public void Can_get_entity_type()
        {
            var entityTypeMapping 
                = new StorageEntityTypeMapping(
                    new StorageEntitySetMapping(new EntitySet(), new StorageEntityContainerMapping(new EntityContainer("C", DataSpace.CSpace))));

            Assert.Null(entityTypeMapping.EntityType);

            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            entityTypeMapping.AddType(entityType);

            Assert.Same(entityType, entityTypeMapping.EntityType);
        }

        [Fact]
        public void Can_create_hierarchy_mappings()
        {
            var entityTypeMapping
                = new StorageEntityTypeMapping(
                    new StorageEntitySetMapping(new EntitySet(), new StorageEntityContainerMapping(new EntityContainer("C", DataSpace.CSpace))));

            Assert.False(entityTypeMapping.IsHierarchyMapping);

            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            entityTypeMapping.AddType(entityType);
            entityTypeMapping.AddIsOfType(entityType);

            Assert.True(entityTypeMapping.IsHierarchyMapping);

            entityTypeMapping.RemoveIsOfType(entityType);

            Assert.False(entityTypeMapping.IsHierarchyMapping);
        }
    }
}
