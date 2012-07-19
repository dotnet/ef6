namespace System.Data.Entity.Core.EntityClient
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using Moq;
    using Xunit;

    public class EntityConnectionTests
    {
        public class Open
        {
            [Fact]
            public void Exception_is_thrown_if_dbConnection_is_null()
            {
                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                var metadataWorkspace = metadataWorkspaceMock.Object;
                var entityConnection = new EntityConnection(metadataWorkspace, null, true);

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

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var metadataWorkspace = metadataWorkspaceMock.Object;
                var entityConnection = new EntityConnection(metadataWorkspace, dbConnectionMock.Object, true);

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

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var metadataWorkspace = metadataWorkspaceMock.Object;
                var entityConnection = new EntityConnection(metadataWorkspace, dbConnectionMock.Object, true);

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

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var metadataWorkspace = metadataWorkspaceMock.Object;
                var entityConnection = new EntityConnection(metadataWorkspace, dbConnectionMock.Object, true);

                entityConnection.Open();

                dbConnectionMock.Verify(m => m.Open(), Times.Once());
            }

            [Fact]
            public void Underlying_dbConnection_is_not_being_reopened_if_it_was_initally_open()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var metadataWorkspace = metadataWorkspaceMock.Object;
                var entityConnection = new EntityConnection(metadataWorkspace, dbConnectionMock.Object, true);

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

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Throws<InvalidOperationException>();
                var metadataWorkspace = metadataWorkspaceMock.Object;
                var entityConnection = new EntityConnection(metadataWorkspace, dbConnectionMock.Object, true);

                Assert.Throws<InvalidOperationException>(() => entityConnection.Open());

                dbConnectionMock.Verify(m => m.Close(), Times.Once());
            }

            [Fact]
            public void Underlying_dbConnection_is_not_being_closed_if_it_was_initially_opened_and_metadata_initialization_fails()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.Open()).Verifiable();
                dbConnectionMock.Setup(m => m.Close()).Verifiable();
                dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Throws<InvalidOperationException>();
                var metadataWorkspace = (metadataWorkspaceMock.Object);
                var entityConnection = new EntityConnection(metadataWorkspace, dbConnectionMock.Object, true);

                Assert.Throws<InvalidOperationException>(() => entityConnection.Open());

                dbConnectionMock.Verify(m => m.Close(), Times.Never());
            }

            [Fact]
            public void EntityConnection_maintains_closed_if_metadata_initialization_fails()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.Open()).Verifiable();
                dbConnectionMock.Setup(m => m.Close()).Verifiable();
                dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Throws<InvalidOperationException>();
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true);

                Assert.Throws<InvalidOperationException>(() => entityConnection.Open());

                Assert.Equal(ConnectionState.Closed, entityConnection.State);
            }
        }

        public class OpenAsync
        {
            [Fact]
            public void Exception_is_thrown_if_dbConnection_is_null()
            {
                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, null, true);

                AssertThrowsInAsyncMethod<InvalidOperationException>(
                    Strings.EntityClient_ConnectionStringNeededBeforeOperation,
                    () => entityConnection.OpenAsync().Wait());
            }

            [Fact]
            public async void Opening_EntityConnection_asynchronously_sets_its_State_to_Opened()
            {
                var dbConnectionState = ConnectionState.Closed;
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Callback(() => dbConnectionState = ConnectionState.Open).Returns(Task.FromResult(1));
                dbConnectionMock.SetupGet(m => m.State).Returns(() => dbConnectionState);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true);

                await entityConnection.OpenAsync();

                Assert.Equal(ConnectionState.Open, entityConnection.State);
            }

            [Fact]
            public async void Exception_is_thrown_when_trying_to_asynchronously_open_already_opened_connection()
            {
                var dbConnectionState = ConnectionState.Closed;
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Callback(() => dbConnectionState = ConnectionState.Open).Returns(Task.FromResult(1));
                dbConnectionMock.SetupGet(m => m.State).Returns(() => dbConnectionState);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true);

                await entityConnection.OpenAsync();

                AssertThrowsInAsyncMethod<InvalidOperationException>(
                    Strings.EntityClient_CannotReopenConnection,
                    () => entityConnection.OpenAsync().Wait());
            }

            [Fact]
            public async void Underlying_dbConnection_is_opened_if_it_was_initially_closed()
            {
                var dbConnectionState = ConnectionState.Closed;
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Callback(() => dbConnectionState = ConnectionState.Open).Returns(Task.FromResult(1));
                dbConnectionMock.SetupGet(m => m.State).Returns(() => dbConnectionState);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true);

                await entityConnection.OpenAsync();

                dbConnectionMock.Verify(m => m.OpenAsync(It.IsAny<CancellationToken>()), Times.Once());
            }

            [Fact]
            public async void Underlying_dbConnection_is_not_being_reopened_if_it_was_initally_open()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true);

                await entityConnection.OpenAsync();

                dbConnectionMock.Verify(m => m.OpenAsync(It.IsAny<CancellationToken>()), Times.Never());
                Assert.Equal(ConnectionState.Open, dbConnectionMock.Object.State);
            }

            [Fact]
            public void Underlying_dbConnection_is_being_closed_if_it_was_initially_closed_and_metadata_initialization_fails()
            {
                var dbConnectionState = ConnectionState.Closed;
                var dbConnectionMock = new Mock<DbConnection>();
                dbConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Callback(() => dbConnectionState = ConnectionState.Open).Returns(Task.FromResult(1));
                dbConnectionMock.Setup(m => m.Close()).Verifiable();
                dbConnectionMock.SetupGet(m => m.State).Returns(() => dbConnectionState);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Throws<InvalidOperationException>();
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true);

                AssertThrowsInAsyncMethod<InvalidOperationException>(null, () => entityConnection.OpenAsync().Wait());

                dbConnectionMock.Verify(m => m.Close(), Times.Once());
            }

            [Fact]
            public void Underlying_dbConnection_is_not_being_closed_if_it_was_initially_opened_and_metadata_initialization_fails()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.Open()).Verifiable();
                dbConnectionMock.Setup(m => m.Close()).Verifiable();
                dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Throws<InvalidOperationException>();
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true);

                AssertThrowsInAsyncMethod<InvalidOperationException>(null, () => entityConnection.OpenAsync().Wait());

                dbConnectionMock.Verify(m => m.Close(), Times.Never());
            }

            [Fact]
            public void EntityConnection_maintains_closed_if_metadata_initialization_fails()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.OpenAsync(CancellationToken.None)).Verifiable();
                dbConnectionMock.Setup(m => m.Close()).Verifiable();
                dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Throws<InvalidOperationException>();
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true);

                AssertThrowsInAsyncMethod<InvalidOperationException>(null, () => entityConnection.OpenAsync().Wait());

                Assert.Equal(ConnectionState.Closed, entityConnection.State);
            }
        }

        private static void AssertThrowsInAsyncMethod<TException>(string expectedMessage, Xunit.Assert.ThrowsDelegate testCode)
            where TException : Exception
        {
            var exception = Assert.Throws<AggregateException>(testCode);
            var innerException = exception.InnerExceptions.Single();
            Assert.IsType<TException>(innerException);
            if (expectedMessage != null)
            {
                Assert.Equal(expectedMessage, innerException.Message);
            }
        }
    }
}
