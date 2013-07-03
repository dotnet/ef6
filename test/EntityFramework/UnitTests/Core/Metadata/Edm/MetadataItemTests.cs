// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Linq;
    using Xunit;

    public class MetadataItemTests
    {
        [Fact]
        public void Can_get_and_set_annotations()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var value = new object();

            Assert.Empty(entityType.Annotations);

            entityType.AddAnnotation("name", value);

            var annotation = entityType.Annotations.Single();

            Assert.Equal("name", annotation.Name);
            Assert.Same(value, annotation.Value);
        }

        public class AnnotationCollectioTests
        {
            [Fact]
            public void Count_is_implemented()
            {
                var typeUsage = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
                var metadataItem = new EntityType("E", "N", DataSpace.CSpace);

                metadataItem.MetadataProperties.Source.Add(MetadataProperty.Create("PN", typeUsage, "PV"));

                var count = metadataItem.MetadataProperties.Count;

                Assert.Equal(0, metadataItem.Annotations.Count);

                metadataItem.Annotations.Add(MetadataProperty.CreateAnnotation("AN", "AV"));

                Assert.Equal(count + 1, metadataItem.MetadataProperties.Count);
                Assert.Equal(1, metadataItem.Annotations.Count);
            }

            [Fact]
            public void IsReadOnly_is_implemented()
            {
                var metadataItem = new EntityType("E", "N", DataSpace.CSpace);

                Assert.False(metadataItem.Annotations.IsReadOnly);

                metadataItem.SetReadOnly();

                Assert.True(metadataItem.Annotations.IsReadOnly);
            }

            [Fact]
            public void Add_is_implemented()
            {
                var typeUsage = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
                var metadataItem = new EntityType("E", "N", DataSpace.CSpace);

                metadataItem.MetadataProperties.Source.Add(MetadataProperty.Create("PN", typeUsage, "PV"));
                var count = metadataItem.MetadataProperties.Count;

                var annotation = MetadataProperty.CreateAnnotation("AN", "AV");
                metadataItem.Annotations.Add(annotation);

                Assert.Same(annotation, metadataItem.MetadataProperties.ElementAt(count));
                Assert.Same(annotation, metadataItem.Annotations.Single());
            }

            [Fact]
            public void Clear_is_implemented()
            {
                var typeUsage = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
                var metadataItem = new EntityType("E", "N", DataSpace.CSpace);

                metadataItem.MetadataProperties.Source.Add(MetadataProperty.Create("PN", typeUsage, "PV"));
                var count = metadataItem.MetadataProperties.Count;

                metadataItem.Annotations.Add(MetadataProperty.CreateAnnotation("AN1", "AV1"));
                metadataItem.Annotations.Add(MetadataProperty.CreateAnnotation("AN2", "AV2"));
                
                Assert.Equal(count + 2, metadataItem.MetadataProperties.Count);
                Assert.Equal(2, metadataItem.Annotations.Count);

                metadataItem.Annotations.Clear();

                Assert.Equal(count, metadataItem.MetadataProperties.Count);
                Assert.Equal(0, metadataItem.Annotations.Count);
                foreach (var p in metadataItem.MetadataProperties)
                {
                    Assert.False(p.IsAnnotation);
                }
            }

            [Fact]
            public void Contains_is_implemented()
            {
                var typeUsage = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
                var metadataItem = new EntityType("E", "N", DataSpace.CSpace);
                var annotation = MetadataProperty.CreateAnnotation("AN", "AV");

                metadataItem.MetadataProperties.Source.Add(MetadataProperty.Create("PN", typeUsage, "PV"));

                Assert.False(metadataItem.Annotations.Contains(annotation));

                metadataItem.Annotations.Add(annotation);

                Assert.True(metadataItem.Annotations.Contains(annotation));
            }

            [Fact]
            public void CopyTo_is_implemented()
            {
                var typeUsage = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
                var metadataItem = new EntityType("E", "N", DataSpace.CSpace);
                var annotation1 = MetadataProperty.CreateAnnotation("AN1", "AV1");
                var annotation2 = MetadataProperty.CreateAnnotation("AN2", "AV2");

                metadataItem.MetadataProperties.Source.Add(MetadataProperty.Create("PN", typeUsage, "PV"));
                metadataItem.Annotations.Add(annotation1);
                metadataItem.Annotations.Add(annotation2);

                var annotations = new MetadataProperty[2];
                metadataItem.Annotations.CopyTo(annotations, 0);

                Assert.Same(annotation1, annotations[0]);
                Assert.Same(annotation2, annotations[1]);
            }

            [Fact]
            public void Remove_is_implemented()
            {
                var typeUsage = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
                var metadataItem = new EntityType("E", "N", DataSpace.CSpace);
                var annotation = MetadataProperty.CreateAnnotation("AN", "AV");

                metadataItem.MetadataProperties.Source.Add(MetadataProperty.Create("PN", typeUsage, "PV"));
                metadataItem.Annotations.Add(annotation);
                var count = metadataItem.MetadataProperties.Count;

                Assert.Equal(1, metadataItem.Annotations.Count);

                metadataItem.Annotations.Remove(annotation);

                Assert.Equal(count - 1, metadataItem.MetadataProperties.Count);
                Assert.Equal(0, metadataItem.Annotations.Count);
                foreach (var p in metadataItem.MetadataProperties)
                {
                    Assert.False(p.IsAnnotation);
                }
            }

            [Fact]
            public void GetEnumerator_is_implemented()
            {
                var typeUsage = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
                var metadataItem = new EntityType("E", "N", DataSpace.CSpace);
                var annotation1 = MetadataProperty.CreateAnnotation("AN1", "AV1");
                var annotation2 = MetadataProperty.CreateAnnotation("AN2", "AV2");

                metadataItem.MetadataProperties.Source.Add(MetadataProperty.Create("PN", typeUsage, "PV"));
                metadataItem.Annotations.Add(annotation1);
                metadataItem.Annotations.Add(annotation2);

                var enumerator = metadataItem.Annotations.GetEnumerator();

                Assert.True(enumerator.MoveNext());
                Assert.Same(annotation1, enumerator.Current);
                Assert.True(enumerator.MoveNext());
                Assert.Same(annotation2, enumerator.Current);
                Assert.False(enumerator.MoveNext());
            }
        }
    }
}
