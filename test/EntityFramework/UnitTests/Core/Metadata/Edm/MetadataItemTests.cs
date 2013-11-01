// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class MetadataItemTests
    {
        [Fact]
        public void Can_add_and_get_annotation()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var value = new object();

            Assert.Empty(entityType.Annotations);

            entityType.AddAnnotation("name", value);

            Assert.NotEmpty(entityType.Annotations);
            Assert.Equal(1, entityType.Annotations.Count());

            var annotation = entityType.Annotations.Single();

            Assert.Equal("name", annotation.Name);
            Assert.Same(value, annotation.Value);
        }

        [Fact]
        public void Can_remove_annotation()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var value = new object();

            entityType.AddAnnotation("name", value);
            
            Assert.NotEmpty(entityType.Annotations);

            Assert.True(entityType.RemoveAnnotation("name"));

            Assert.Empty(entityType.Annotations);
        }

        [Fact]
        public void RemoveAnnotation_returns_false_if_annotation_not_found()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var value = new object();

            entityType.AddAnnotation("name", value);

            Assert.False(entityType.RemoveAnnotation("notfound"));
        }

        [Fact]
        public void AddAnnotation_throws_if_metadata_item_is_readonly()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var value = new object();

            entityType.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyCollection,
                Assert.Throws<InvalidOperationException>(() => entityType.AddAnnotation("name", value))
                    .Message);
        }

        [Fact]
        public void RemoveAnnotation_throws_if_metadata_item_is_readonly()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var value = new object();

            entityType.AddAnnotation("name", value);
            entityType.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyCollection,
                Assert.Throws<InvalidOperationException>(() => entityType.RemoveAnnotation("name"))
                    .Message);
        }
    }
}
