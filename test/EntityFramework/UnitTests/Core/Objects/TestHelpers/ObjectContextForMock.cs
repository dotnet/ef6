namespace System.Data.Entity.Core.Objects
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Metadata.Internal;
    using Moq;

    public class ObjectContextForMock : ObjectContext
    {
        private EntityConnection _connection;

        internal ObjectContextForMock(EntityConnection connection)
        {
            _connection = connection;
        }

        public override DbConnection Connection
        {
            get { return _connection; }
        }

        public static ObjectContextForMock Create()
        {
            var providerFactoryMock = new Mock<DbProviderFactoryForMock>() { CallBase = true };

            var internalEntityConnectionMock = new Mock<InternalEntityConnection>();
            internalEntityConnectionMock.SetupGet(m => m.StoreProviderFactory).Returns(providerFactoryMock.Object);
            internalEntityConnectionMock.SetupGet(m => m.StoreConnection).Returns(default(DbConnection));
            var entityConnection = new EntityConnection(internalEntityConnectionMock.Object);

            var objectContextMock = new Mock<ObjectContextForMock>(entityConnection) { CallBase = true };

            var internalMetadataWorkspaceMock = new Mock<InternalMetadataWorkspace>(MockBehavior.Strict);
            var metadataWorkspace = new MetadataWorkspace(internalMetadataWorkspaceMock.Object);

            var objectStateManagerMock = new Mock<ObjectStateManager>(metadataWorkspace);

            objectContextMock.Setup(m => m.ObjectStateManager).Returns(objectStateManagerMock.Object);

            return objectContextMock.Object;
        }
    }
}
