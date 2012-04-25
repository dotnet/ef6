namespace System.Data.Entity.Core.EntityClient
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Metadata.Internal;
    using System.Data.Entity.Resources;
    using System.Linq.Expressions;
    using System.Transactions;
    using Moq;
    using Xunit;

    public class EntityConnectionTests
    {
        public class DelegationToInternalClass
        {
            [Fact]
            public void EntityConnection_properties_delegate_to_internal_class_correctly()
            {
                VerifyGetter(c => c.ConnectionString, m => m.ConnectionString);
                VerifySetter(c => c.ConnectionString = default(string), m => m.ConnectionString = It.IsAny<string>());
                VerifyGetter(c => c.ConnectionTimeout, m => m.ConnectionTimeout);
                VerifyGetter(c => c.State, m => m.State);
                VerifyGetter(c => c.DataSource, m => m.DataSource);
                VerifyGetter(c => c.ServerVersion, m => m.ServerVersion);
                VerifyGetter(c => c.StoreConnection, m => m.StoreConnection);
            }

            [Fact]
            public void EntityConnection_methods_delegate_to_internal_class_correctly()
            {
                VerifyMethod(c => c.GetMetadataWorkspace(), m => m.GetMetadataWorkspace());
                VerifyMethod(c => c.GetMetadataWorkspace(default(bool)), m => m.GetMetadataWorkspace(It.IsAny<bool>()));
                VerifyMethod(c => c.Open(), m => m.Open());
                VerifyMethod(c => c.Close(), m => m.Close());
                VerifyMethod(c => c.EnlistTransaction(default(Transaction)), m => m.EnlistTransaction(It.IsAny<Transaction>()));
                VerifyMethod(c => c.ClearCurrentTransaction(), m => m.ClearCurrentTransaction());
            }

            private void VerifyGetter<TProperty>(
                Func<EntityConnection, TProperty> getterFunc,
                Expression<Func<InternalEntityConnection, TProperty>> mockGetterFunc)
            {
                Assert.NotNull(getterFunc);
                Assert.NotNull(mockGetterFunc);

                var internalConnectionMock = new Mock<InternalEntityConnection>(null, null, true);
                var connection = new EntityConnection(internalConnectionMock.Object);

                getterFunc(connection);
                internalConnectionMock.VerifyGet(mockGetterFunc, Times.Once());
            }

            private void VerifySetter<TProperty>(
                Func<EntityConnection, TProperty> setter,
                Action<InternalEntityConnection> mockSetter)
            {
                Assert.NotNull(setter);
                Assert.NotNull(mockSetter);

                var internalConnectionMock = new Mock<InternalEntityConnection>(null, null, true);
                var connection = new EntityConnection(internalConnectionMock.Object);

                setter(connection);
                internalConnectionMock.VerifySet(m => mockSetter(m) , Times.Once());
            }

            private void VerifyMethod(
                Action<EntityConnection> methodInvoke, 
                Expression<Action<InternalEntityConnection>> mockMethodInvoke)
            {
                Assert.NotNull(methodInvoke);
                Assert.NotNull(mockMethodInvoke);

                var internalConnectionMock = new Mock<InternalEntityConnection>(null, null, true);
                var connection = new EntityConnection(internalConnectionMock.Object);

                methodInvoke(connection);
                internalConnectionMock.Verify(mockMethodInvoke, Times.Once());
            }
        }

        public class Open
        {
            [Fact]
            public void Exception_is_thrown_if_dbConnection_is_null()
            {
                var internalMetadataWorkspaceMock = new Mock<InternalMetadataWorkspace>(MockBehavior.Strict);
                var metadataWorkspace = new MetadataWorkspace(internalMetadataWorkspaceMock.Object);
                var internalEntityConnection = new InternalEntityConnection(metadataWorkspace, null, true);
                var entityConnection = new EntityConnection(internalEntityConnection);

                Assert.Equal(
                    Strings.EntityClient_ConnectionStringNeededBeforeOperation, 
                    Assert.Throws<InvalidOperationException>(() => entityConnection.Open()).Message);
            }

            [Fact]
            public void Opening_EntityConnection_sets_its_State_to_Opened()
            {
                var dbConnectionState = ConnectionState.Closed;
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.Open()).Callback(() => dbConnectionState = ConnectionState.Open);
                dbConnectionMock.SetupGet(m => m.State).Returns(() => dbConnectionState);

                var internalMetadataWorkspaceMock = new Mock<InternalMetadataWorkspace>(MockBehavior.Strict);
                internalMetadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var metadataWorkspace = new MetadataWorkspace(internalMetadataWorkspaceMock.Object);
                var internalEntityConnection = new InternalEntityConnection(metadataWorkspace, dbConnectionMock.Object, true);
                var entityConnection = new EntityConnection(internalEntityConnection);

                entityConnection.Open();

                Assert.Equal(ConnectionState.Open, entityConnection.State);
            }

            [Fact]
            public void Exception_is_thrown_when_trying_to_open_already_opened_connection()
            {
                var dbConnectionState = ConnectionState.Closed;
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.Open()).Callback(() => dbConnectionState = ConnectionState.Open);
                dbConnectionMock.SetupGet(m => m.State).Returns(() => dbConnectionState);

                var internalMetadataWorkspaceMock = new Mock<InternalMetadataWorkspace>(MockBehavior.Strict);
                internalMetadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var metadataWorkspace = new MetadataWorkspace(internalMetadataWorkspaceMock.Object);
                var internalEntityConnection = new InternalEntityConnection(metadataWorkspace, dbConnectionMock.Object, true);
                var entityConnection = new EntityConnection(internalEntityConnection);

                entityConnection.Open();

                Assert.Equal(
                    Strings.EntityClient_CannotReopenConnection,
                    Assert.Throws<InvalidOperationException>(() => entityConnection.Open()).Message);
            }

            [Fact]
            public void Underlying_dbConnection_is_opened_if_it_was_initially_closed()
            {
                var dbConnectionState = ConnectionState.Closed;
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.Open()).Callback(() => dbConnectionState = ConnectionState.Open);
                dbConnectionMock.SetupGet(m => m.State).Returns(() => dbConnectionState);

                var internalMetadataWorkspaceMock = new Mock<InternalMetadataWorkspace>(MockBehavior.Strict);
                internalMetadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var metadataWorkspace = new MetadataWorkspace(internalMetadataWorkspaceMock.Object);
                var internalEntityConnection = new InternalEntityConnection(metadataWorkspace, dbConnectionMock.Object, true);
                var entityConnection = new EntityConnection(internalEntityConnection);
                
                entityConnection.Open();

                dbConnectionMock.Verify(m => m.Open(), Times.Once());
            }

            [Fact]
            public void Underlying_dbConnection_is_not_being_reopened_if_it_was_initally_open()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);

                var internalMetadataWorkspaceMock = new Mock<InternalMetadataWorkspace>(MockBehavior.Strict);
                internalMetadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var metadataWorkspace = new MetadataWorkspace(internalMetadataWorkspaceMock.Object);
                var internalEntityConnection = new InternalEntityConnection(metadataWorkspace, dbConnectionMock.Object, true);
                var entityConnection = new EntityConnection(internalEntityConnection);

                entityConnection.Open();

                dbConnectionMock.Verify(m => m.Open(), Times.Never());
                Assert.Equal(ConnectionState.Open, dbConnectionMock.Object.State);
            }

            [Fact]
            public void Underlying_dbConnection_is_being_closed_if_it_was_initially_closed_and_metadata_initialization_fails()
            {
                var dbConnectionState = ConnectionState.Closed;
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.Open()).Callback(() => dbConnectionState = ConnectionState.Open);
                dbConnectionMock.Setup(m => m.Close()).Verifiable();
                dbConnectionMock.SetupGet(m => m.State).Returns(() => dbConnectionState);

                var internalMetadataWorkspaceMock = new Mock<InternalMetadataWorkspace>(MockBehavior.Strict);
                internalMetadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Throws<InvalidOperationException>();
                var metadataWorkspace = new MetadataWorkspace(internalMetadataWorkspaceMock.Object);
                var internalEntityConnection = new InternalEntityConnection(metadataWorkspace, dbConnectionMock.Object, true);
                var entityConnection = new EntityConnection(internalEntityConnection);

                Assert.Throws<InvalidOperationException>(() => entityConnection.Open());

                dbConnectionMock.Verify(m => m.Close(), Times.Once());
                Assert.Equal(ConnectionState.Closed, entityConnection.State);
            }

            [Fact]
            public void Underlying_dbConnection_is_not_being_closed_if_it_was_initially_opened_and_metadata_initialization_fails()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.Open()).Verifiable();
                dbConnectionMock.Setup(m => m.Close()).Verifiable();
                dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);

                var internalMetadataWorkspaceMock = new Mock<InternalMetadataWorkspace>(MockBehavior.Strict);
                internalMetadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Throws<InvalidOperationException>();
                var metadataWorkspace = new MetadataWorkspace(internalMetadataWorkspaceMock.Object);
                var internalEntityConnection = new InternalEntityConnection(metadataWorkspace, dbConnectionMock.Object, true);
                var entityConnection = new EntityConnection(internalEntityConnection);

                Assert.Throws<InvalidOperationException>(() => entityConnection.Open());

                dbConnectionMock.Verify(m => m.Close(), Times.Never());
                Assert.Equal(ConnectionState.Closed, entityConnection.State);
            }
        }
    }
}
