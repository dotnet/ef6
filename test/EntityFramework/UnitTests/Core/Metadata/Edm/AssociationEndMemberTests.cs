// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Linq;
    using Xunit;

    public class AssociationEndMemberTests
    {
        [Fact]
        public void Create_throws_argument_exception_when_called_with_invalid_arguments()
        {
            var entityType = new EntityType("Source", "Namespace", DataSpace.CSpace);
            var refType = new RefType(entityType);
            var metadataProperty =
                new MetadataProperty(
                    "MetadataProperty",
                    TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                    "value");

            Assert.Throws<ArgumentException>(
                () => AssociationEndMember.Create(
                    null,
                    refType,
                    RelationshipMultiplicity.Many,
                    OperationAction.Cascade,
                    new[] { metadataProperty }));

            Assert.Throws<ArgumentException>(
                () => AssociationEndMember.Create(
                    String.Empty,
                    refType,
                    RelationshipMultiplicity.Many,
                    OperationAction.Cascade,
                    new[] { metadataProperty }));

            Assert.Throws<ArgumentNullException>(
                () => AssociationEndMember.Create(
                    "AssociationEndMember",
                    null,
                    RelationshipMultiplicity.Many,
                    OperationAction.Cascade,
                    new[] { metadataProperty }));
        }

        [Fact]
        public void Create_sets_properties_and_seals_the_instance()
        {
            var entityType = new EntityType("Source", "Namespace", DataSpace.CSpace);
            var refType = new RefType(entityType);
            var metadataProperty =
                new MetadataProperty(
                    "MetadataProperty",
                    TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                    "value");
            var associationEnd =
                AssociationEndMember.Create(
                    "AssociationEndMember",
                    refType,
                    RelationshipMultiplicity.Many,
                    OperationAction.Cascade,
                    new[] { metadataProperty });

            Assert.Equal("AssociationEndMember", associationEnd.Name);
            Assert.Same(entityType, associationEnd.GetEntityType());
            Assert.Equal(RelationshipMultiplicity.Many, associationEnd.RelationshipMultiplicity);
            Assert.Equal(OperationAction.Cascade, associationEnd.DeleteBehavior);
            Assert.Same(metadataProperty, associationEnd.MetadataProperties.SingleOrDefault(p => p.Name == "MetadataProperty"));
            Assert.True(associationEnd.IsReadOnly);
        }
    }
}
