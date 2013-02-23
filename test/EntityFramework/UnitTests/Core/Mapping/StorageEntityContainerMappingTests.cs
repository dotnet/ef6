// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public class StorageEntityContainerMappingTests
    {
        [Fact]
        public void Can_get_entity_set_mappings()
        {
            var entityContainerMapping = new StorageEntityContainerMapping(new EntityContainer("C", DataSpace.CSpace));

            Assert.Empty(entityContainerMapping.EntitySetMappings);

            var entitySetMapping
                = new StorageEntitySetMapping(
                    new EntitySet("ES", null, null, null, new EntityType("E", "N", DataSpace.CSpace)), entityContainerMapping);

            entityContainerMapping.AddEntitySetMapping(entitySetMapping);

            Assert.Same(entitySetMapping, entityContainerMapping.EntitySetMappings.Single());
        }

        [Fact]
        public void Can_get_association_set_mappings()
        {
            var entityContainerMapping = new StorageEntityContainerMapping(new EntityContainer("C", DataSpace.CSpace));

            Assert.Empty(entityContainerMapping.AssociationSetMappings);

            var associationSetMapping
                = new StorageAssociationSetMapping(
                    new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)), entityContainerMapping);

            entityContainerMapping.AddAssociationSetMapping(associationSetMapping);

            Assert.Same(associationSetMapping, entityContainerMapping.AssociationSetMappings.Single());
        }
    }
}
