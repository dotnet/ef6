// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using Xunit;

    public class MetadataCollectionTests
    {
        [Fact]
        public void Can_remove_item_from_collection()
        {
            var metadataCollection = new MetadataCollection<EntityType>();

            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            metadataCollection.Add(entityType);

            Assert.Equal(1, metadataCollection.Count);

            metadataCollection.Remove(entityType);

            Assert.Empty(metadataCollection);
        }

        [Fact]
        public void Can_remove_item_from_collection_and_identity_dictionary_updated()
        {
            var metadataCollection = new MetadataCollection<EntityType>();

            for (var i = 0; i < 30; i++)
            {
                metadataCollection.Add(new EntityType("E" + i, "N", DataSpace.CSpace));
            }

            for (var i = 0; i < metadataCollection.Count; i++)
            {
                // This will throw if the identity cache gets out of sync.

                var entityType = metadataCollection[i];

                metadataCollection.Remove(entityType);
                metadataCollection.Add(entityType);
            }

            Assert.Equal(30, metadataCollection.Count);
        }

        [Fact]
        public void Can_replace_item_via_ordinal_indexer()
        {
            var metadataCollection = new MetadataCollection<EntityType>();

            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            metadataCollection.Add(entityType);

            Assert.Equal(1, metadataCollection.Count);

            var entityType2 = new EntityType("E", "N", DataSpace.CSpace);

            metadataCollection[0] = entityType2;

            Assert.Equal(1, metadataCollection.Count);
            Assert.Same(entityType2, metadataCollection[0]);
        }

        [Fact]
        public void AddRange_throws_ArgumentNullException_for_null_collection_item()
        {
            var metadataCollection = new MetadataCollection<MetadataItem>();

            Assert.Equal(
                Strings.ADP_CollectionParameterElementIsNull("items"),
                Assert.Throws<ArgumentException>(
                    () => metadataCollection.AtomicAddRange(
                        new List<MetadataItem>
                            {
                                null
                            })).Message);
        }
    }
}
