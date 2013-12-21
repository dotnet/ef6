// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class MetadataItemTests
    {
        [Fact]
        public void AddAnnotation_checks_arguments()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(() => entityType.AddAnnotation(null, null)).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(() => entityType.AddAnnotation("", null)).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(() => entityType.AddAnnotation(" ", null)).Message);
        }

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
        public void Can_remove_annotation_by_setting_null()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var value = new object();

            entityType.AddAnnotation("name", value);

            Assert.NotEmpty(entityType.Annotations);

            entityType.AddAnnotation("name", null);

            Assert.Empty(entityType.Annotations);
        }

        [Fact]
        public void Passing_a_null_value_causes_annotation_to_not_be_set()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            entityType.AddAnnotation("name", null);

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

        [Fact]
        public void SerializableAnnotations_returns_only_annotations_with_annotation_prefix_and_strips_prefix()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            Assert.Empty(entityType.SerializableAnnotations);

            entityType.AddAnnotation("name", new object());

            Assert.Empty(entityType.SerializableAnnotations);

            entityType.AddAnnotation(XmlConstants.CustomAnnotationPrefix + "First", "Amy");
            entityType.AddAnnotation(XmlConstants.CustomAnnotationPrefix + "Last", "Winehouse");

            Assert.Equal(2, entityType.SerializableAnnotations.Count);
            Assert.Same("Amy", entityType.SerializableAnnotations["First"]);
            Assert.Same("Winehouse", entityType.SerializableAnnotations["Last"]);
        }
    }
}
