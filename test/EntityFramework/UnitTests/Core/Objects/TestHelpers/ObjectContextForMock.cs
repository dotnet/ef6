namespace System.Data.Entity.Core.Objects
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
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

            var entityConnectionMock = new Mock<EntityConnection>();
            entityConnectionMock.SetupGet(m => m.StoreProviderFactory).Returns(providerFactoryMock.Object);
            entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(default(DbConnection));
            var entityConnection = entityConnectionMock.Object;

            var objectContextMock = new Mock<ObjectContextForMock>(entityConnection) { CallBase = true };

            var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
            var metadataWorkspace = metadataWorkspaceMock.Object;

            var objectStateManagerMock = new Mock<ObjectStateManager>(metadataWorkspace);

            objectContextMock.Setup(m => m.ObjectStateManager).Returns(objectStateManagerMock.Object);

            return objectContextMock.Object;
        }
    }
}
