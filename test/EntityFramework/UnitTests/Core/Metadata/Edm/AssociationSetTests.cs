// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Linq;
    using Xunit;

    public class AssociationSetTests
    {
        [Fact]
        public void Can_get_and_set_ends_via_wrapper_properties()
        {
            var associationType
                = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)
                      {
                          SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace)),
                          TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace))
                      };

            var associationSet = new AssociationSet("A", associationType);

            Assert.Null(associationSet.SourceSet);
            Assert.Null(associationSet.TargetSet);

            var sourceEntitySet = new EntitySet();

            associationSet.SourceSet = sourceEntitySet;

            var targetEntitySet = new EntitySet();

            associationSet.TargetSet = targetEntitySet;

            Assert.Same(sourceEntitySet, associationSet.SourceSet);
            Assert.Same(targetEntitySet, associationSet.TargetSet);
        }

        [Fact]
        public void Can_get_association_ends_from_association_set()
        {
            var sourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            var targetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));

            var associationType
                = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);

            associationType.AddKeyMember(targetEnd);
            associationType.AddKeyMember(sourceEnd);

            var associationSet = new AssociationSet("A", associationType);

            Assert.Null(associationSet.SourceEnd);
            Assert.Null(associationSet.TargetEnd);

            associationSet.AddAssociationSetEnd(new AssociationSetEnd(new EntitySet(), associationSet, sourceEnd));
            associationSet.AddAssociationSetEnd(new AssociationSetEnd(new EntitySet(), associationSet, targetEnd));

            Assert.Same(sourceEnd, associationSet.SourceEnd);
            Assert.Same(targetEnd, associationSet.TargetEnd);
        }

        [Fact]
        public void Create_throws_argument_exception_when_called_with_invalid_arguments()
        {
            var source = new EntityType("Source", "Namespace", DataSpace.CSpace);
            var target = new EntityType("Target", "Namespace", DataSpace.CSpace);
            var other = new EntityType("Other", "Namespace", DataSpace.CSpace);
            var sourceEnd = new AssociationEndMember("SourceEnd", source);
            var targetEnd = new AssociationEndMember("TargetEnd", target);
            var constraint =
                new ReferentialConstraint(
                    sourceEnd,
                    targetEnd,
                    new[] { new EdmProperty("SourceProperty") },
                    new[] { new EdmProperty("TargetProperty") });
            var associationType =
                AssociationType.Create(
                    "AssociationType",
                    "Namespace",
                    true,
                    DataSpace.CSpace,
                    sourceEnd,
                    targetEnd,
                    constraint,
                    Enumerable.Empty<MetadataProperty>());
            var sourceSet = new EntitySet("SourceSet", "Schema", "Table", "Query", source);
            var targetSet = new EntitySet("TargetSet", "Schema", "Table", "Query", target);
            var otherSet = new EntitySet("OtherSet", "Schema", "Table", "Query", other);
            var metadataProperty =
                new MetadataProperty(
                    "MetadataProperty",
                    TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                    "value");

            Assert.Throws<ArgumentException>(
                () => AssociationSet.Create(
                    null,
                    associationType,
                    sourceSet,
                    targetSet,
                    new [] { metadataProperty }));

            Assert.Throws<ArgumentException>(
                () => AssociationSet.Create(
                    String.Empty,
                    associationType,
                    sourceSet,
                    targetSet,
                    new[] { metadataProperty }));

            Assert.Throws<ArgumentNullException>(
                () => AssociationSet.Create(
                    "AssociationSet",
                    null,
                    sourceSet,
                    targetSet,
                    new[] { metadataProperty }));

            Assert.Throws<ArgumentException>(
                () => AssociationSet.Create(
                    "AssociationSet",
                    associationType,
                    otherSet,
                    targetSet,
                    new[] { metadataProperty }));

            Assert.Throws<ArgumentException>(
                () => AssociationSet.Create(
                    "AssociationSet",
                    associationType,
                    sourceSet,
                    otherSet,
                    new[] { metadataProperty }));
        }

        [Fact]
        public void Create_sets_properties_and_seals_the_instance()
        {
            var source = new EntityType("Source", "Namespace", DataSpace.CSpace);
            var target = new EntityType("Target", "Namespace", DataSpace.CSpace);
            var sourceEnd = new AssociationEndMember("SourceEnd", source);
            var targetEnd = new AssociationEndMember("TargetEnd", target);
            var constraint =
                new ReferentialConstraint(
                    sourceEnd,
                    targetEnd,
                    new[] { new EdmProperty("SourceProperty") },
                    new[] { new EdmProperty("TargetProperty") });
            var associationType =
                AssociationType.Create(
                    "AssociationType",
                    "Namespace",
                    true,
                    DataSpace.CSpace,
                    sourceEnd,
                    targetEnd,
                    constraint,
                    Enumerable.Empty<MetadataProperty>());
            var sourceSet = new EntitySet("SourceSet", "Schema", "Table", "Query", source);
            var targetSet = new EntitySet("TargetSet", "Schema", "Table", "Query", target);
            var metadataProperty =
                new MetadataProperty(
                    "MetadataProperty",
                    TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                    "value");
            var associationSet = 
                AssociationSet.Create(
                    "AssociationSet",
                    associationType,
                    sourceSet,
                    targetSet,
                    new[] { metadataProperty });

            Assert.Equal("AssociationSet", associationSet.Name);
            Assert.Same(associationType, associationSet.ElementType);
            Assert.Same(sourceSet, associationSet.SourceSet);
            Assert.Same(targetSet, associationSet.TargetSet);
            Assert.Same(source, associationSet.SourceEnd.GetEntityType());
            Assert.Same(target, associationSet.TargetEnd.GetEntityType());
            Assert.Same(metadataProperty, associationSet.MetadataProperties.SingleOrDefault(p => p.Name == "MetadataProperty"));
            Assert.True(associationSet.IsReadOnly);
        }
    }
}
