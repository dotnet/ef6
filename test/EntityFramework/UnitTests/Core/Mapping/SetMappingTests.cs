// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public class SetMappingTests
    {
        [Fact]
        public void Can_get_entity_set()
        {
            var entitySet = new EntitySet();
            var storageSetMapping
                = new EntitySetMapping(
                    entitySet,
                    new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)));

            Assert.Same(entitySet, storageSetMapping.EntitySet);
        }

        [Fact]
        public void Can_get_container_mapping()
        {
            var containerMapping = new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace));
            var storageSetMapping
                = new EntitySetMapping(
                    new EntitySet(),
                    containerMapping);

            Assert.Same(containerMapping, storageSetMapping.EntityContainerMapping);
        }

        [Fact]
        public void Can_add_remove_type_mapping()
        {
            var storageSetMapping 
                = new EntitySetMapping(
                    new EntitySet(), 
                    new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)));

            Assert.Empty(storageSetMapping.TypeMappings);

            var entityTypeMapping = new EntityTypeMapping(storageSetMapping);

            storageSetMapping.AddTypeMapping(entityTypeMapping);

            Assert.Same(entityTypeMapping, storageSetMapping.TypeMappings.Single());

            storageSetMapping.RemoveTypeMapping(entityTypeMapping);

            Assert.Empty(storageSetMapping.TypeMappings);
        }

        [Fact]
        public void Can_not_add_null_mapping()
        {
            var storageSetMapping
                = new EntitySetMapping(
                    new EntitySet(),
                    new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)));

            Assert.Equal(
                "typeMapping",
                Assert.Throws<ArgumentNullException>(() => storageSetMapping.AddTypeMapping(null)).ParamName);
        }
    }
}