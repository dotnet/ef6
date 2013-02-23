// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public class StorageEntitySetMappingTests
    {
        [Fact]
        public void Can_get_entity_type_mappings()
        {
            var entityContainerMapping = new StorageEntityContainerMapping(new EntityContainer("C", DataSpace.CSpace));
            var entitySetMapping = new StorageEntitySetMapping(new EntitySet(), entityContainerMapping);

            Assert.Empty(entitySetMapping.EntityTypeMappings);

            var entityTypeMapping
                = new StorageEntityTypeMapping(
                    new StorageEntitySetMapping(new EntitySet(), entityContainerMapping));

            entitySetMapping.AddTypeMapping(entityTypeMapping);

            Assert.Same(entityTypeMapping, entitySetMapping.EntityTypeMappings.Single());
        }

        [Fact]
        public void Can_get_entity_set()
        {
            var entityContainerMapping = new StorageEntityContainerMapping(new EntityContainer("C", DataSpace.CSpace));
            var entitySet = new EntitySet();
            var entitySetMapping = new StorageEntitySetMapping(entitySet, entityContainerMapping);

            Assert.Same(entitySet, entitySetMapping.EntitySet);
        }
    }
}
