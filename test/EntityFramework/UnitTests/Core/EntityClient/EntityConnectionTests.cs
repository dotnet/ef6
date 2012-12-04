// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityClient
{
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Linq;
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
                dbConnectionMock.Raise(conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                Assert.Equal(ConnectionState.Open, entityConnection.State);
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
                dbConnectionMock.Raise(conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                dbConnectionMock.Verify(m => m.Open(), Times.Once());
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
                dbConnectionMock.Raise(conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                Assert.Equal(
                    Strings.EntityClient_CannotReopenConnection,
                    Assert.Throws<InvalidOperationException>(() => entityConnection.Open()).Message);
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

                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State);
            }

            [Fact]
            public void EntityConnection_with_closed_underlying_connection_maintains_closed_if_metadata_initialization_fails()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.Open()).Verifiable();
                dbConnectionMock.Setup(m => m.Close()).Verifiable();
                dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Closed);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Throws<InvalidOperationException>();
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true);

                Assert.Throws<InvalidOperationException>(() => entityConnection.Open());

                Assert.Equal(ConnectionState.Closed, entityConnection.State);
            }

            [Fact]
            public void StoreConnection_state_mimics_EntityConnection_state_if_only_EntityConnection_is_used_with_ambient_txn()
            {
                using (var transaction = new TransactionScope())
                {
                    var storeConnectionState = ConnectionState.Closed;
                    var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                    storeConnectionMock.Setup(m => m.Open()).Callback(() => storeConnectionState = ConnectionState.Open);
                    storeConnectionMock.Setup(m => m.Close()).Callback(() => storeConnectionState = ConnectionState.Closed);
                    storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);

                    var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                    metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                    var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true);
                    Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                    Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state

                    // open entityConnection (automatically opens store connection)
                    entityConnection.Open();
                    storeConnectionMock.Raise(conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                    Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state
                    Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State); // underlying storeConnection state

                    // close entityConnection without explicitly closing underlying store connection 
                    entityConnection.Close();
                    storeConnectionMock.Raise(conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

                    Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                    Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state
                }
            }

            [Fact]
            public void StoreConnection_state_mimics_EntityConnection_state_if_only_EntityConnection_is_used_without_txn()
            {
                var storeConnectionState = ConnectionState.Closed;
                var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                storeConnectionMock.Setup(m => m.Open()).Callback(() => storeConnectionState = ConnectionState.Open);
                storeConnectionMock.Setup(m => m.Close()).Callback(() => storeConnectionState = ConnectionState.Closed);
                storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true);
                Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state

                // open entityConnection (automatically opens store connection)
                entityConnection.Open();
                storeConnectionMock.Raise(conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State); // underlying storeConnection state

                // close entityConnection without explicitly closing underlying store connection 
                entityConnection.Close();
                storeConnectionMock.Raise(conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

                Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state
            }

            [Fact]
            public void EntityConnection_automatically_opened_if_underlying_StoreConnection_is_opened_with_ambient_txn()
            {
                using (var transaction = new TransactionScope())
                {
                    var storeConnectionState = ConnectionState.Closed;
                    var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                    storeConnectionMock.Setup(m => m.Open()).Callback(() => storeConnectionState = ConnectionState.Open);
                    storeConnectionMock.Setup(m => m.Close()).Callback(() => storeConnectionState = ConnectionState.Closed);
                    storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);

                    var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                    var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true);
                    Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                    Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state

                    // open underlying store connection without explicitly opening entityConnection
                    entityConnection.StoreConnection.Open();
                    storeConnectionMock.Raise(conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                    Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state is automatically updated
                    Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State);

                    // now close underlying store connection (without explicitly closing entityConnection)
                    entityConnection.StoreConnection.Close();
                    storeConnectionMock.Raise(conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

                    Assert.Equal(ConnectionState.Closed, entityConnection.State);
                    Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State);
                }
            }

            [Fact]
            public void EntityConnection_automatically_opened_if_underlying_StoreConnection_is_opened_without_txn()
            {
                var storeConnectionState = ConnectionState.Closed;
                var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                storeConnectionMock.Setup(m => m.Open()).Callback(() => storeConnectionState = ConnectionState.Open);
                storeConnectionMock.Setup(m => m.Close()).Callback(() => storeConnectionState = ConnectionState.Closed);
                storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true);
                Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state

                // open underlying store connection without explicitly opening entityConnection
                entityConnection.StoreConnection.Open();
                storeConnectionMock.Raise(conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state is automatically updated
                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State);

                // now close underlying store connection (without explicitly closing entityConnection)
                entityConnection.StoreConnection.Close();
                storeConnectionMock.Raise(conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

                Assert.Equal(ConnectionState.Closed, entityConnection.State);
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State);
            }

            [Fact]
            public void EntityConnection_automatically_closed_if_underlying_StoreConnection_is_closed_with_ambient_txn()
            {
                using (var transaction = new TransactionScope())
                {
                    var storeConnectionState = ConnectionState.Closed;
                    var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);

                    storeConnectionMock.Setup(m => m.Open()).Callback(() => storeConnectionState = ConnectionState.Open);
                    storeConnectionMock.Setup(m => m.Close()).Callback(() => storeConnectionState = ConnectionState.Closed);
                    storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);

                    var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                    metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                    // open entityConnection - both entityConnection and store connection should now be open
                    var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true);
                    entityConnection.Open();
                    storeConnectionMock.Raise(conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));
                    Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state
                    Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State); // underlying storeConnection state

                    // now close the underlying store connection without explicitly closing entityConnection
                    entityConnection.StoreConnection.Close();
                    storeConnectionMock.Raise(conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

                    Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state automatically updated
                    Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State);

                    // now re-open the store connection and EntityConnection is "resurrected" to an Open state
                    entityConnection.StoreConnection.Open();
                    storeConnectionMock.Raise(conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));
                    Assert.Equal(ConnectionState.Open, entityConnection.State);
                    Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State);
                }
            }

            [Fact]
            public void EntityConnection_automatically_closed_if_underlying_StoreConnection_is_closed_without_txn()
            {
                var storeConnectionState = ConnectionState.Closed;
                var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);

                storeConnectionMock.Setup(m => m.Open()).Callback(() => storeConnectionState = ConnectionState.Open);
                storeConnectionMock.Setup(m => m.Close()).Callback(() => storeConnectionState = ConnectionState.Closed);
                storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                // open entityConnection - both entityConnection and store connection should now be open
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true);
                entityConnection.Open();
                storeConnectionMock.Raise(conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));
                Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State); // underlying storeConnection state

                // now close the underlying store connection without explicitly closing entityConnection
                entityConnection.StoreConnection.Close();
                storeConnectionMock.Raise(conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

                Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state automatically updated
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State);

                // now re-open the store connection and EntityConnection is "resurrected" to an Open state
                entityConnection.StoreConnection.Open();
                storeConnectionMock.Raise(conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));
                Assert.Equal(ConnectionState.Open, entityConnection.State);
                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State);
            }

        }

#if !NET40

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
            public void Opening_EntityConnection_asynchronously_sets_its_State_to_Opened()
            {
                var dbConnectionState = ConnectionState.Closed;
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Callback(
                    () => dbConnectionState = ConnectionState.Open).Returns(Task.FromResult(1));
                dbConnectionMock.SetupGet(m => m.State).Returns(() => dbConnectionState);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true);

                entityConnection.OpenAsync().Wait();
                dbConnectionMock.Raise(conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                Assert.Equal(ConnectionState.Open, entityConnection.State);
            }

            [Fact]
            public void Underlying_dbConnection_is_opened_if_it_was_initially_closed()
            {
                var dbConnectionState = ConnectionState.Closed;
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Callback(
                    () => dbConnectionState = ConnectionState.Open).Returns(Task.FromResult(1));
                dbConnectionMock.SetupGet(m => m.State).Returns(() => dbConnectionState);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true);

                entityConnection.OpenAsync().Wait();
                dbConnectionMock.Raise(conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                dbConnectionMock.Verify(m => m.OpenAsync(It.IsAny<CancellationToken>()), Times.Once());
            }

            [Fact]
            public void Attempt_to_reopen_EntityConnection_throws()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true);

                AssertThrowsInAsyncMethod<InvalidOperationException>(
                    Strings.EntityClient_CannotReopenConnection, () => entityConnection.OpenAsync().Wait());
            }

            [Fact]
            public void Underlying_dbConnection_is_being_closed_if_it_was_initially_closed_and_metadata_initialization_fails()
            {
                var dbConnectionState = ConnectionState.Closed;
                var dbConnectionMock = new Mock<DbConnection>();
                dbConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Callback(
                    () => dbConnectionState = ConnectionState.Open).Returns(Task.FromResult(1));
                dbConnectionMock.Setup(m => m.Close()).Verifiable();
                dbConnectionMock.SetupGet(m => m.State).Returns(() => dbConnectionState);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Throws<InvalidOperationException>();
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true);

                AssertThrowsInAsyncMethod<InvalidOperationException>(null, () => entityConnection.OpenAsync().Wait());

                dbConnectionMock.Verify(m => m.Close(), Times.Once());
            }

            [Fact]
            public void Underlying_dbConnection_is_not_being_closed_if_it_was_initially_open_and_metadata_initialization_fails()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
                dbConnectionMock.Setup(m => m.Close()).Verifiable();
                dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Throws<InvalidOperationException>();
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true);

                Assert.Equal(ConnectionState.Open, entityConnection.State);
                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State);

                AssertThrowsInAsyncMethod<InvalidOperationException>(null, () => entityConnection.OpenAsync().Wait());

                dbConnectionMock.Verify(m => m.Close(), Times.Never());

                Assert.Equal(ConnectionState.Open, entityConnection.State);
                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State);
            }

            [Fact]
            public void EntityConnection_with_closed_underlying_connection_maintains_closed_if_metadata_initialization_fails()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Returns(
                    Task.FromResult(1));
                dbConnectionMock.Setup(m => m.Close()).Verifiable();
                dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Closed);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Throws<InvalidOperationException>();
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true);

                AssertThrowsInAsyncMethod<InvalidOperationException>(null, () => entityConnection.OpenAsync().Wait());

                Assert.Equal(ConnectionState.Closed, entityConnection.State);
            }

            [Fact]
            public void Exceptions_from_Underlying_dbConnection_are_wrapped()
            {
                var dbConnectionState = ConnectionState.Closed;
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Throws(
                    new AggregateException(new InvalidOperationException()));

                dbConnectionMock.SetupGet(m => m.State).Returns(() => dbConnectionState);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true);

                AssertThrowsInAsyncMethod<EntityException>(
                    Strings.EntityClient_ProviderSpecificError("Open"),
                    () => entityConnection.OpenAsync().Wait());

                dbConnectionMock.Verify(m => m.OpenAsync(It.IsAny<CancellationToken>()), Times.Once());
            }

            [Fact]
            public void StoreConnection_state_mimics_EntityConnection_state_if_only_EntityConnection_is_used_with_ambient_txn()
            {
                using (var transaction = new TransactionScope())
                {
                    var storeConnectionState = ConnectionState.Closed;
                    var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                    storeConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Callback(
                        () => storeConnectionState = ConnectionState.Open).Returns(Task.FromResult(1));
                    storeConnectionMock.Setup(m => m.Close()).Callback(() => storeConnectionState = ConnectionState.Closed);
                    storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);

                    var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                    metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                    var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true);
                    Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                    Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state

                    // open entityConnection (automatically opens store connection)
                    entityConnection.OpenAsync().Wait();
                    storeConnectionMock.Raise(conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                    Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state
                    Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State); // underlying storeConnection state

                    // close entityConnection without explicitly closing underlying store connection 
                    entityConnection.Close();
                    storeConnectionMock.Raise(conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

                    Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                    Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state
                }
            }

            [Fact]
            public void StoreConnection_state_mimics_EntityConnection_state_if_only_EntityConnection_is_used_without_txn()
            {
                var storeConnectionState = ConnectionState.Closed;
                var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                storeConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Callback(
                    () => storeConnectionState = ConnectionState.Open).Returns(Task.FromResult(1));
                storeConnectionMock.Setup(m => m.Close()).Callback(() => storeConnectionState = ConnectionState.Closed);
                storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true);
                Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state

                // open entityConnection (automatically opens store connection)
                entityConnection.OpenAsync().Wait();
                storeConnectionMock.Raise(conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State); // underlying storeConnection state

                // close entityConnection without explicitly closing underlying store connection 
                entityConnection.Close();
                storeConnectionMock.Raise(conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

                Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state
            }

            [Fact]
            public void EntityConnection_automatically_opened_if_underlying_StoreConnection_is_opened_with_ambient_txn()
            {
                using (var transaction = new TransactionScope())
                {
                    var storeConnectionState = ConnectionState.Closed;
                    var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                    storeConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Callback(
                        () => storeConnectionState = ConnectionState.Open).Returns(Task.FromResult(1));
                    storeConnectionMock.Setup(m => m.Close()).Callback(() => storeConnectionState = ConnectionState.Closed);
                    storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);

                    var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                    var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true);
                    Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                    Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state

                    // open underlying store connection without explicitly opening entityConnection
                    entityConnection.StoreConnection.OpenAsync().Wait();
                    storeConnectionMock.Raise(conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                    Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state is automatically updated
                    Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State);

                    // now close underlying store connection (without explicitly closing entityConnection)
                    entityConnection.StoreConnection.Close();
                    storeConnectionMock.Raise(conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

                    Assert.Equal(ConnectionState.Closed, entityConnection.State);
                    Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State);
                }
            }

            [Fact]
            public void EntityConnection_automatically_opened_if_underlying_StoreConnection_is_opened_without_txn()
            {
                var storeConnectionState = ConnectionState.Closed;
                var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                storeConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Callback(
                    () => storeConnectionState = ConnectionState.Open).Returns(Task.FromResult(1));
                storeConnectionMock.Setup(m => m.Close()).Callback(() => storeConnectionState = ConnectionState.Closed);
                storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true);
                Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state

                // open underlying store connection without explicitly opening entityConnection
                entityConnection.StoreConnection.OpenAsync().Wait();
                storeConnectionMock.Raise(conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state is automatically updated
                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State);

                // now close underlying store connection (without explicitly closing entityConnection)
                entityConnection.StoreConnection.Close();
                storeConnectionMock.Raise(conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

                Assert.Equal(ConnectionState.Closed, entityConnection.State);
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State);
            }

            [Fact]
            public void EntityConnection_automatically_closed_if_underlying_StoreConnection_is_closed_with_ambient_txn()
            {
                using (var transaction = new TransactionScope())
                {
                    var storeConnectionState = ConnectionState.Closed;
                    var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                    storeConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Callback(
                        () => storeConnectionState = ConnectionState.Open).Returns(Task.FromResult(1));
                    storeConnectionMock.Setup(m => m.Close()).Callback(() => storeConnectionState = ConnectionState.Closed);
                    storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);

                    var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                    metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                    // open entityConnection - both entityConnection and store connection should now be open
                    var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true);
                    entityConnection.OpenAsync().Wait();
                    storeConnectionMock.Raise(conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));
                    Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state
                    Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State); // underlying storeConnection state

                    // now close the underlying store connection without explicitly closing entityConnection
                    entityConnection.StoreConnection.Close();
                    storeConnectionMock.Raise(conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

                    Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state automatically updated
                    Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State);

                    // now re-open the store connection and EntityConnection is "resurrected" to an Open state
                    entityConnection.StoreConnection.OpenAsync().Wait();
                    storeConnectionMock.Raise(conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));
                    Assert.Equal(ConnectionState.Open, entityConnection.State);
                    Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State);
                }
            }

            [Fact]
            public void EntityConnection_automatically_closed_if_underlying_StoreConnection_is_closed_without_txn()
            {
                var storeConnectionState = ConnectionState.Closed;
                var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                storeConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Callback(
                    () => storeConnectionState = ConnectionState.Open).Returns(Task.FromResult(1));
                storeConnectionMock.Setup(m => m.Close()).Callback(() => storeConnectionState = ConnectionState.Closed);
                storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                // open entityConnection - both entityConnection and store connection should now be open
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true);
                entityConnection.OpenAsync().Wait();
                storeConnectionMock.Raise(conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));
                Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State); // underlying storeConnection state

                // now close the underlying store connection without explicitly closing entityConnection
                entityConnection.StoreConnection.Close();
                storeConnectionMock.Raise(conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

                Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state automatically updated
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State);

                // now re-open the store connection and EntityConnection is "resurrected" to an Open state
                entityConnection.StoreConnection.OpenAsync().Wait();
                storeConnectionMock.Raise(conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));
                Assert.Equal(ConnectionState.Open, entityConnection.State);
                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State);
            }
        }

        private static void AssertThrowsInAsyncMethod<TException>(string expectedMessage, Assert.ThrowsDelegate testCode)
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

#endif
    }
}
