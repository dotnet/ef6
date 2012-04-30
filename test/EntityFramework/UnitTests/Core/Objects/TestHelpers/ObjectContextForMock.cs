namespace System.Data.Entity.Core.Objects
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Metadata.Internal;
    using System.Data.Entity.Core.Objects.Internal;
    using Moq;

    public class ObjectContextForMock : ObjectContext
    {
        internal ObjectContextForMock(InternalObjectContext internalObjectContext)
            : base(internalObjectContext)
        {
        }

        public static ObjectContextForMock Create()
        {
            var internalObjectContextMock = new Mock<InternalObjectContext>(MockBehavior.Strict);

            var internalMetadataWorkspaceMock = new Mock<InternalMetadataWorkspace>(MockBehavior.Strict);
            var metadataWorkspace = new MetadataWorkspace(internalMetadataWorkspaceMock.Object);

            var objectStateManagerMock = new Mock<ObjectStateManager>(metadataWorkspace);

            internalObjectContextMock.Setup(m => m.ObjectStateManager).Returns(objectStateManagerMock.Object);

            return new ObjectContextForMock(internalObjectContextMock.Object);
        }
    }
}
