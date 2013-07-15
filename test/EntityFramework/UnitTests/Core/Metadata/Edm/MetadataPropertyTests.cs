// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class MetadataPropertyTests
    {
        [Fact]
        public void Can_get_and_set_value_property()
        {
            var metadataProperty
                = new MetadataProperty
                      {
                          Value = "Foo"
                      };

            Assert.Equal("Foo", metadataProperty.Value);
        }

        [Fact]
        public void Create_sets_properties_and_seals_MetadataProperty()
        {
            var typeUsage = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var metadataProperty = MetadataProperty.Create("property", typeUsage, "value");

            Assert.Equal("property", metadataProperty.Name);
            Assert.Same(typeUsage, metadataProperty.TypeUsage);
            Assert.Equal("value", metadataProperty.Value);
            Assert.True(metadataProperty.IsReadOnly);
            Assert.False(metadataProperty.IsAnnotation);
        }

        [Fact]
        public void CreateAnnotation_creates_correct_metdata_property()
        {
            var value = new object();
            var annotation = MetadataProperty.CreateAnnotation("name", value);

            Assert.Equal("name", annotation.Name);
            Assert.Equal(value, annotation.Value);
            Assert.Null(annotation.TypeUsage);
            Assert.Equal(PropertyKind.Extended, annotation.PropertyKind);
            Assert.True(annotation.IsAnnotation);
        }

        [Fact]
        public void Can_set_and_get_annotation_value()
        {
            var value1 = new object();
            var value2 = new object();

            var annotation = MetadataProperty.CreateAnnotation("name", value1);

            Assert.Equal("name", annotation.Name);
            Assert.Same(value1, annotation.Value);

            annotation.Value = value2;

            Assert.Equal("name", annotation.Name);
            Assert.Same(value2, annotation.Value);
        }

        [Fact]
        public void Setting_annotation_value_throws_if_metadata_item_is_readonly()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var value1 = new object();
            var value2 = new object();

            entityType.AddAnnotation("name", value1);
            entityType.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(() => entityType.Annotations.Single().Value = value2)
                    .Message);
        }

    }
}
