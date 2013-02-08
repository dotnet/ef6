// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class MetadataCollectionTests
    {
        [Fact]
        public void Can_remove_item_from_collection()
        {
            var metadataCollection = new MetadataCollection<EntityType>();

            var entityType = new EntityType();

            metadataCollection.Add(entityType);

            Assert.Equal(1, metadataCollection.Count);

            metadataCollection.Remove(entityType);

            Assert.Empty(metadataCollection);
        }

        [Fact]
        public void Can_replace_value_via_ordinal_indexer()
        {
            var metadataCollection = new MetadataCollection<EntityType>();

            var entityType = new EntityType();

            metadataCollection.Add(entityType);

            Assert.Equal(1, metadataCollection.Count);

            var entityType2 = new EntityType();

            metadataCollection[0] = entityType2;

            Assert.Equal(1, metadataCollection.Count);
            Assert.Same(entityType2, metadataCollection[0]);
        }
    }
}
