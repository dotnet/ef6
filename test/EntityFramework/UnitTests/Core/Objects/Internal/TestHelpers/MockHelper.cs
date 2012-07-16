namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Data.Entity.Core.Objects.DataClasses;
    using Moq;

    public static class MockHelper
    {
        internal static Mock<IEntityWrapper> CreateMockEntityWrapper()
        {
            var mockEntityWrapper = new Mock<IEntityWrapper>(MockBehavior.Strict);

            var entityWithRelationshipsMock = new Mock<IEntityWithRelationships>(MockBehavior.Strict);
            mockEntityWrapper.Setup(m => m.Entity).Returns(entityWithRelationshipsMock.Object);

            var entityKey = new EntityKey(qualifiedEntitySetName: "entityContainerName.entitySetName");
            mockEntityWrapper.Setup(m => m.EntityKey).Returns(entityKey);

            return mockEntityWrapper;
        }
    }
}
