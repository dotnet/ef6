// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using Xunit;

    public class MappingItemTests
    {
        private class AMappingItem : MappingItem
        {
            public void Modify()
            {
                ThrowIfReadOnly();
            }
        }

        [Fact]
        public void Can_add_retrieve_remove_annotations()
        {
            var mappingItem = new AMappingItem();
            var annotation = MetadataProperty.CreateAnnotation("N", "V");

            Assert.Equal(0, mappingItem.Annotations.Count);

            mappingItem.Annotations.Add(annotation);

            Assert.Equal(1, mappingItem.Annotations.Count);
            Assert.Same(annotation, mappingItem.Annotations[0]);

            mappingItem.Annotations.Remove(annotation);

            Assert.Equal(0, mappingItem.Annotations.Count);
        }

        [Fact]
        public void Can_retrieve_set_validate_ReadOnly_flag()
        {
            var mappingItem = new AMappingItem();

            Assert.False(mappingItem.IsReadOnly);
            Assert.DoesNotThrow(mappingItem.Modify);

            mappingItem.SetReadOnly();

            Assert.True(mappingItem.IsReadOnly);
            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(() => mappingItem.Modify())
                    .Message);
        }
    }
}
