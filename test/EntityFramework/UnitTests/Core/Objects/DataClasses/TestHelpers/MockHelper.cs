// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Moq;

    public static class MockHelper
    {
        public static Mock<EntityCollection<TEntity>> CreateMockEntityCollection<TEntity>(TEntity refreshedValue)
            where TEntity : class
        {
            var entityReferenceMock = new Mock<EntityCollection<TEntity>>() { CallBase = true };

            var objectQueryMock = Objects.MockHelper.CreateMockObjectQuery(refreshedValue);

            bool hasResults = refreshedValue != null;
            entityReferenceMock.Setup(m => m.ValidateLoad<TEntity>(It.IsAny<MergeOption>(), It.IsAny<string>(), out hasResults))
                .Returns(() => objectQueryMock.Object);

            return entityReferenceMock;
        }

        public static Mock<EntityReference<TEntity>> CreateMockEntityReference<TEntity>(TEntity refreshedValue)
            where TEntity : class
        {
            var relationshipNavigation = new RelationshipNavigation(
                relationshipName: "relationship",
                from: "from",
                to: "to",
                fromAccessor: new NavigationPropertyAccessor(string.Empty),
                toAccessor: new NavigationPropertyAccessor(string.Empty));

            var entityReferenceMock = new Mock<EntityReference<TEntity>>(
                Internal.MockHelper.CreateMockEntityWrapper().Object,
                relationshipNavigation,
                new Mock<IRelationshipFixer>(MockBehavior.Strict).Object) { CallBase = true };

            var associationType = new AssociationType(
                                name: "associationName",
                                namespaceName: "associationNamespace",
                                foreignKey: true,
                                dataSpace: DataSpace.CSpace);
            entityReferenceMock.Setup(m => m.RelationMetadata).Returns(associationType);

            var associationSet = new AssociationSet(name: "associationSetName", associationType: associationType);
            entityReferenceMock.Setup(m => m.RelationshipSet).Returns(associationSet);

            entityReferenceMock.Setup(m => m.ObjectContext).Returns(ObjectContextForMock.Create());

            var objectQueryMock = Objects.MockHelper.CreateMockObjectQuery(refreshedValue);

            bool hasResults = refreshedValue != null;
            entityReferenceMock.Setup(m => m.ValidateLoad<TEntity>(It.IsAny<MergeOption>(), It.IsAny<string>(), out hasResults))
                .Returns(() => objectQueryMock.Object);

            var refType = new RefType(new EntityType(name: "entityTypeName", namespaceName: "entityTypeNamespace", dataSpace: DataSpace.CSpace));

            var fromEndMember = new AssociationEndMember(
                name: "fromEndMember",
                endRefType: refType,
                multiplicity: RelationshipMultiplicity.Many);
            entityReferenceMock.Setup(m => m.FromEndMember).Returns(fromEndMember);

            var toEndMember = new AssociationEndMember(
                name: "toEndMember",
                endRefType: refType,
                multiplicity: RelationshipMultiplicity.Many);
            entityReferenceMock.Setup(m => m.ToEndMember).Returns(toEndMember);

            return entityReferenceMock;
        }
    }
}
