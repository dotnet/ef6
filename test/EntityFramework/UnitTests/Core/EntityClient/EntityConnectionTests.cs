// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityClient
{
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using Moq;
    using Moq.Protected;
    using Xunit;
    using IsolationLevel = System.Data.IsolationLevel;
#if !NET40

#endif

    public class EntityConnectionTests
    {
        public class Constructors : TestBase
        {
            [Fact]
            public void EntityConnection_worksapce_constructors_check_arguments()
            {
                Assert.Equal(
                    "workspace",
                    Assert.Throws<ArgumentNullException>(() => new EntityConnection(null, new Mock<DbConnection>().Object)).ParamName);
                Assert.Equal(
                    "connection",
                    Assert.Throws<ArgumentNullException>(() => new EntityConnection(new Mock<MetadataWorkspace>().Object, null)).ParamName);
                Assert.Equal(
                    "workspace",
                    Assert.Throws<ArgumentNullException>(() => new EntityConnection(null, new Mock<DbConnection>().Object, true)).ParamName);
                Assert.Equal(
                    "connection",
                    Assert.Throws<ArgumentNullException>(() => new EntityConnection(new Mock<MetadataWorkspace>().Object, null, true))
                          .ParamName);
            }
        }

        public class GetMetadataWorkspace : TestBase
        {
            [Fact]
            public void GetMetadataWorkspace_returns_the_workspace_passed_to_constructor()
            {
                var mockWorkspace = new Mock<MetadataWorkspace>();
                mockWorkspace.Setup(m => m.IsItemCollectionAlreadyRegistered(It.IsAny<DataSpace>())).Returns(true);
                mockWorkspace.Setup(m => m.GetItemCollection(DataSpace.SSpace)).Returns(new Mock<StoreItemCollection>().Object);

                Assert.Same(mockWorkspace.Object, new EntityConnection(mockWorkspace.Object, new SqlConnection()).GetMetadataWorkspace());
                Assert.Same(
                    mockWorkspace.Object, new EntityConnection(mockWorkspace.Object, new SqlConnection(), true).GetMetadataWorkspace());
            }
        }

        public class Open : TestBase
        {
            [Fact]
            public void Exception_is_thrown_if_dbConnection_is_null()
            {
                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                var metadataWorkspace = metadataWorkspaceMock.Object;
                var entityConnection = new EntityConnection(metadataWorkspace, null, true, true);

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
                dbConnectionMock.SetupGet(m => m.DataSource).Returns(() => "foo");

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var metadataWorkspace = metadataWorkspaceMock.Object;
                var entityConnection = new EntityConnection(metadataWorkspace, dbConnectionMock.Object, true, true);

                entityConnection.Open();
                dbConnectionMock.Raise(
                    conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                Assert.Equal(ConnectionState.Open, entityConnection.State);
            }

            [Fact]
            public void Underlying_dbConnection_is_opened_if_it_was_initially_closed()
            {
                var dbConnectionState = ConnectionState.Closed;
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.Open()).Callback(() => dbConnectionState = ConnectionState.Open);
                dbConnectionMock.SetupGet(m => m.State).Returns(() => dbConnectionState);
                dbConnectionMock.SetupGet(m => m.DataSource).Returns(() => "foo");

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var metadataWorkspace = metadataWorkspaceMock.Object;
                var entityConnection = new EntityConnection(metadataWorkspace, dbConnectionMock.Object, true, true);

                entityConnection.Open();
                dbConnectionMock.Raise(
                    conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                dbConnectionMock.Verify(m => m.Open(), Times.Once());
            }

            [Fact]
            public void Exception_is_thrown_when_trying_to_open_already_opened_connection()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);
                dbConnectionMock.SetupGet(m => m.DataSource).Returns(() => "fake");

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var metadataWorkspace = metadataWorkspaceMock.Object;
                var entityConnection = new EntityConnection(metadataWorkspace, dbConnectionMock.Object, true, true);

                Assert.Equal(
                    Strings.EntityClient_CannotReopenConnection,
                    Assert.Throws<InvalidOperationException>(() => entityConnection.Open()).Message);
            }

            [Fact]
            public void Exception_is_thrown_when_trying_to_open_a_broken_connection()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Broken);
                dbConnectionMock.SetupGet(m => m.DataSource).Returns(() => "fake");

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var metadataWorkspace = metadataWorkspaceMock.Object;
                var entityConnection = new EntityConnection(metadataWorkspace, dbConnectionMock.Object, true, true);

                Assert.Equal(
                    Strings.EntityClient_CannotOpenBrokenConnection,
                    Assert.Throws<InvalidOperationException>(() => entityConnection.Open()).Message);
            }

            [Fact]
            public void Exception_is_thrown_when_store_connection_doesnt_open()
            {
                var storeConnectionState = ConnectionState.Closed;
                var storeConnectionMock = new Mock<DbConnection>();
                storeConnectionMock.Setup(m => m.Close()).Callback(() => storeConnectionState = ConnectionState.Closed);
                storeConnectionMock.SetupGet(m => m.DataSource).Returns("fake");
                storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true, true);

                Assert.Equal(
                    Strings.EntityClient_ConnectionNotOpen,
                    Assert.Throws<InvalidOperationException>(() => entityConnection.Open()).Message);
            }

            [Fact]
            public void Underlying_dbConnection_is_not_being_closed_if_it_was_initially_opened_such_that_it_cannot_be_reopend()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.Open()).Verifiable();
                dbConnectionMock.Setup(m => m.Close()).Verifiable();
                dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);
                dbConnectionMock.SetupGet(m => m.DataSource).Returns(() => "foo");

                var entityConnection = new EntityConnection(
                    new Mock<MetadataWorkspace>(MockBehavior.Strict).Object, dbConnectionMock.Object, true, true);

                Assert.Equal(
                    Strings.EntityClient_CannotReopenConnection,
                    Assert.Throws<InvalidOperationException>(() => entityConnection.Open()).Message);

                dbConnectionMock.Verify(m => m.Close(), Times.Never());

                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State);
            }

            [Fact]
            public void EntityConnection_with_closed_underlying_connection_maintains_closed_if_store_connection_does_not_open_correctly()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.Open()).Verifiable();
                dbConnectionMock.Setup(m => m.Close()).Verifiable();
                dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Closed);
                dbConnectionMock.SetupGet(m => m.DataSource).Returns(() => "foo");

                var entityConnection = new EntityConnection(
                    new Mock<MetadataWorkspace>(MockBehavior.Strict).Object, dbConnectionMock.Object, true, true);

                Assert.Equal(
                    Strings.EntityClient_ConnectionNotOpen,
                    Assert.Throws<InvalidOperationException>(() => entityConnection.Open()).Message);

                Assert.Equal(ConnectionState.Closed, entityConnection.State);
            }

            [Fact]
            public void ExecutionStrategy_is_used()
            {
                var storeConnectionState = ConnectionState.Closed;
                var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                storeConnectionMock.Setup(m => m.Open()).Callback(() => storeConnectionState = ConnectionState.Open);
                storeConnectionMock.SetupGet(m => m.DataSource).Returns("fake");
                storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true, true);

                var executionStrategyMock = new Mock<IExecutionStrategy>();
                executionStrategyMock.Setup(m => m.Execute(It.IsAny<Action>())).Callback<Action>(
                    a =>
                        {
                            storeConnectionMock.Verify(m => m.Open(), Times.Never());
                            a();
                            storeConnectionMock.Verify(m => m.Open(), Times.Once());
                        });

                MutableResolver.AddResolver<IExecutionStrategy>(key => executionStrategyMock.Object);
                try
                {
                    entityConnection.Open();
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                }

                executionStrategyMock.Verify(m => m.Execute(It.IsAny<Action>()), Times.Once());
            }

            [Fact]
            public void StoreConnection_state_mimics_EntityConnection_state_if_only_EntityConnection_is_used_with_ambient_txn()
            {
                using (new TransactionScope())
                {
                    var storeConnectionState = ConnectionState.Closed;
                    var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                    storeConnectionMock.Setup(m => m.Open()).Callback(() => storeConnectionState = ConnectionState.Open);
                    storeConnectionMock.Setup(m => m.Close()).Callback(() => storeConnectionState = ConnectionState.Closed);
                    storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);
                    storeConnectionMock.SetupGet(m => m.DataSource).Returns("fake");

                    var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                    metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                    var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true, true);
                    Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                    Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state

                    // open entityConnection (automatically opens store connection)
                    entityConnection.Open();
                    storeConnectionMock.Raise(
                        conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                    Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state
                    Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State); // underlying storeConnection state

                    // close entityConnection without explicitly closing underlying store connection 
                    entityConnection.Close();
                    storeConnectionMock.Raise(
                        conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

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
                storeConnectionMock.SetupGet(m => m.DataSource).Returns("fake");

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true, true);
                Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state

                // open entityConnection (automatically opens store connection)
                entityConnection.Open();
                storeConnectionMock.Raise(
                    conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State); // underlying storeConnection state

                // close entityConnection without explicitly closing underlying store connection 
                entityConnection.Close();
                storeConnectionMock.Raise(
                    conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

                Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state
            }

            [Fact]
            public void EntityConnection_automatically_opened_if_underlying_StoreConnection_is_opened_with_ambient_txn()
            {
                using (new TransactionScope())
                {
                    var storeConnectionState = ConnectionState.Closed;
                    var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                    storeConnectionMock.Setup(m => m.Open()).Callback(() => storeConnectionState = ConnectionState.Open);
                    storeConnectionMock.Setup(m => m.Close()).Callback(() => storeConnectionState = ConnectionState.Closed);
                    storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);
                    storeConnectionMock.SetupGet(m => m.DataSource).Returns("fake");

                    var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                    var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true, true);
                    Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                    Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state

                    // open underlying store connection without explicitly opening entityConnection
                    entityConnection.StoreConnection.Open();
                    storeConnectionMock.Raise(
                        conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                    Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state is automatically updated
                    Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State);

                    // now close underlying store connection (without explicitly closing entityConnection)
                    entityConnection.StoreConnection.Close();
                    storeConnectionMock.Raise(
                        conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

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
                storeConnectionMock.SetupGet(m => m.DataSource).Returns("fake");

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true, true);
                Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state

                // open underlying store connection without explicitly opening entityConnection
                entityConnection.StoreConnection.Open();
                storeConnectionMock.Raise(
                    conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state is automatically updated
                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State);

                // now close underlying store connection (without explicitly closing entityConnection)
                entityConnection.StoreConnection.Close();
                storeConnectionMock.Raise(
                    conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

                Assert.Equal(ConnectionState.Closed, entityConnection.State);
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State);
            }

            [Fact]
            public void EntityConnection_automatically_closed_if_underlying_StoreConnection_is_closed_with_ambient_txn()
            {
                using (new TransactionScope())
                {
                    var storeConnectionState = ConnectionState.Closed;
                    var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);

                    storeConnectionMock.Setup(m => m.Open()).Callback(() => storeConnectionState = ConnectionState.Open);
                    storeConnectionMock.Setup(m => m.Close()).Callback(() => storeConnectionState = ConnectionState.Closed);
                    storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);
                    storeConnectionMock.SetupGet(m => m.DataSource).Returns("fake");

                    var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                    metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                    // open entityConnection - both entityConnection and store connection should now be open
                    var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true, true);
                    entityConnection.Open();
                    storeConnectionMock.Raise(
                        conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));
                    Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state
                    Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State); // underlying storeConnection state

                    // now close the underlying store connection without explicitly closing entityConnection
                    entityConnection.StoreConnection.Close();
                    storeConnectionMock.Raise(
                        conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

                    Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state automatically updated
                    Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State);

                    // now re-open the store connection and EntityConnection is "resurrected" to an Open state
                    entityConnection.StoreConnection.Open();
                    storeConnectionMock.Raise(
                        conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));
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
                storeConnectionMock.SetupGet(m => m.DataSource).Returns("fake");

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                // open entityConnection - both entityConnection and store connection should now be open
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true, true);
                entityConnection.Open();
                storeConnectionMock.Raise(
                    conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));
                Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State); // underlying storeConnection state

                // now close the underlying store connection without explicitly closing entityConnection
                entityConnection.StoreConnection.Close();
                storeConnectionMock.Raise(
                    conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

                Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state automatically updated
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State);

                // now re-open the store connection and EntityConnection is "resurrected" to an Open state
                entityConnection.StoreConnection.Open();
                storeConnectionMock.Raise(
                    conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));
                Assert.Equal(ConnectionState.Open, entityConnection.State);
                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State);
            }
        }

#if !NET40
        // These tests rely on being able to mock DbConnection, which is very complicated on .NET 4, so disabling these tests on that platform
        public class Dispose
        {
            [Fact]
            public void EntityConnection_disposes_underlying_StoreConnection_if_entityConnectionShouldDisposeStoreConnection_flag_is_set()
            {
                var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                storeConnectionMock.Protected().Setup("Dispose", true).Verifiable();
                storeConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Closed);
                storeConnectionMock.Protected().Setup<DbProviderFactory>("DbProviderFactory").Returns(new Mock<DbProviderFactory>().Object);
                storeConnectionMock.SetupGet(m => m.DataSource).Returns("fake");

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.CSpace)).Returns(true);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.CSSpace)).Returns(true);
                metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.SSpace)).Returns(new Mock<StoreItemCollection>().Object);

                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true);
                entityConnection.Dispose();

                storeConnectionMock.Protected().Verify("Dispose", Times.Once(), true);
            }

            [Fact]
            public void
                EntityConnection_does_not_dispose_underlying_StoreConnection_if_entityConnectionShouldDisposeStoreConnection_flag_is_not_set
                ()
            {
                var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                storeConnectionMock.Protected().Setup("Dispose", true).Verifiable();
                storeConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Closed);
                storeConnectionMock.Protected().Setup<DbProviderFactory>("DbProviderFactory").Returns(new Mock<DbProviderFactory>().Object);
                storeConnectionMock.SetupGet(m => m.DataSource).Returns("fake");

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.CSpace)).Returns(true);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.CSSpace)).Returns(true);
                metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.SSpace)).Returns(new Mock<StoreItemCollection>().Object);

                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, false);
                entityConnection.Dispose();

                storeConnectionMock.Protected().Verify("Dispose", Times.Never(), true);
            }

            [Fact]
            public void
                EntityConnection_does_not_dispose_underlying_StoreConnection_if_the_entityConnectionShouldDisposeStoreConnection_flag_is_not_specified
                ()
            {
                var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                storeConnectionMock.Protected().Setup("Dispose", true).Verifiable();
                storeConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Closed);
                storeConnectionMock.Protected().Setup<DbProviderFactory>("DbProviderFactory").Returns(new Mock<DbProviderFactory>().Object);
                storeConnectionMock.SetupGet(m => m.DataSource).Returns("fake");

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.CSpace)).Returns(true);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.CSSpace)).Returns(true);
                metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.SSpace)).Returns(new Mock<StoreItemCollection>().Object);

                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object);
                entityConnection.Dispose();

                storeConnectionMock.Protected().Verify("Dispose", Times.Never(), true);
            }
        }

        public class OpenAsync : TestBase
        {
            [Fact]
            public void Exception_is_thrown_if_dbConnection_is_null()
            {
                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, null, true, true);

                Assert.Equal(
                    Strings.EntityClient_ConnectionStringNeededBeforeOperation,
                    Assert.Throws<InvalidOperationException>(
                        () => ExceptionHelpers.UnwrapAggregateExceptions(
                            () => entityConnection.OpenAsync().Wait())).Message);
            }

            [Fact]
            public void Opening_EntityConnection_asynchronously_sets_its_State_to_Opened()
            {
                var dbConnectionState = ConnectionState.Closed;
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Callback(
                    () => dbConnectionState = ConnectionState.Open).Returns(Task.FromResult(1));
                dbConnectionMock.SetupGet(m => m.State).Returns(() => dbConnectionState);
                dbConnectionMock.SetupGet(m => m.DataSource).Returns("fake");

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true, true);

                entityConnection.OpenAsync().Wait();
                dbConnectionMock.Raise(
                    conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

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
                dbConnectionMock.SetupGet(m => m.DataSource).Returns("fake");

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true, true);

                entityConnection.OpenAsync().Wait();
                dbConnectionMock.Raise(
                    conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                dbConnectionMock.Verify(m => m.OpenAsync(It.IsAny<CancellationToken>()), Times.Once());
            }

            [Fact]
            public void Exception_is_thrown_when_trying_to_open_already_opened_connection()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);
                dbConnectionMock.SetupGet(m => m.DataSource).Returns(() => "fake");

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var metadataWorkspace = metadataWorkspaceMock.Object;
                var entityConnection = new EntityConnection(metadataWorkspace, dbConnectionMock.Object, true, true);

                Assert.Equal(
                    Strings.EntityClient_CannotReopenConnection,
                    Assert.Throws<InvalidOperationException>(
                        () => ExceptionHelpers.UnwrapAggregateExceptions(
                            () =>
                            entityConnection.OpenAsync().Wait())).Message);
            }

            [Fact]
            public void Exception_is_thrown_when_trying_to_open_a_broken_connection()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Broken);
                dbConnectionMock.SetupGet(m => m.DataSource).Returns(() => "fake");

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var metadataWorkspace = metadataWorkspaceMock.Object;
                var entityConnection = new EntityConnection(metadataWorkspace, dbConnectionMock.Object, true, true);

                Assert.Equal(
                    Strings.EntityClient_CannotOpenBrokenConnection,
                    Assert.Throws<InvalidOperationException>(
                        () => ExceptionHelpers.UnwrapAggregateExceptions(
                            () => entityConnection.OpenAsync().Wait())).Message);
            }

            [Fact]
            public void Exception_is_thrown_when_store_connection_doesnt_open()
            {
                var storeConnectionState = ConnectionState.Closed;
                var storeConnectionMock = new Mock<DbConnection>();
                storeConnectionMock.Setup(m => m.Close()).Callback(() => storeConnectionState = ConnectionState.Closed);
                storeConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));
                storeConnectionMock.SetupGet(m => m.DataSource).Returns("fake");
                storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true, true);

                Assert.Equal(
                    Strings.EntityClient_ConnectionNotOpen,
                    Assert.Throws<InvalidOperationException>(
                        () => ExceptionHelpers.UnwrapAggregateExceptions(
                            () =>
                            entityConnection.OpenAsync().Wait())).Message);
            }

            [Fact]
            public void Underlying_dbConnection_is_not_being_closed_if_it_was_initially_open_such_that_it_cannot_be_reopend()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
                dbConnectionMock.Setup(m => m.Close()).Verifiable();
                dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);
                dbConnectionMock.SetupGet(m => m.DataSource).Returns("fake");

                var entityConnection = new EntityConnection(
                    new Mock<MetadataWorkspace>(MockBehavior.Strict).Object, dbConnectionMock.Object, true, true);

                Assert.Equal(ConnectionState.Open, entityConnection.State);
                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State);

                Assert.Equal(
                    Strings.EntityClient_CannotReopenConnection,
                    Assert.Throws<InvalidOperationException>(
                        () => ExceptionHelpers.UnwrapAggregateExceptions(
                            () => entityConnection.OpenAsync().Wait())).Message);

                dbConnectionMock.Verify(m => m.Close(), Times.Never());

                Assert.Equal(ConnectionState.Open, entityConnection.State);
                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State);
            }

            [Fact]
            public void EntityConnection_with_closed_underlying_connection_maintains_closed_if_store_connection_does_not_open_correctly()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Returns(
                    Task.FromResult(1));
                dbConnectionMock.Setup(m => m.Close()).Verifiable();
                dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Closed);
                dbConnectionMock.SetupGet(m => m.DataSource).Returns("fake");

                var entityConnection = new EntityConnection(
                    new Mock<MetadataWorkspace>(MockBehavior.Strict).Object, dbConnectionMock.Object, true, true);

                Assert.Equal(
                    Strings.EntityClient_ConnectionNotOpen,
                    Assert.Throws<InvalidOperationException>(
                        () => ExceptionHelpers.UnwrapAggregateExceptions(
                            () => entityConnection.OpenAsync().Wait())).Message);

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
                dbConnectionMock.SetupGet(m => m.DataSource).Returns("fake");

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true, true);

                Assert.Equal(
                    Strings.EntityClient_ProviderSpecificError("Open"),
                    Assert.Throws<EntityException>(
                        () => ExceptionHelpers.UnwrapAggregateExceptions(
                            () => entityConnection.OpenAsync().Wait())).Message);

                dbConnectionMock.Verify(m => m.OpenAsync(It.IsAny<CancellationToken>()), Times.Once());
            }

            [Fact]
            public void ExecutionStrategy_is_used()
            {
                var storeConnectionState = ConnectionState.Closed;
                var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                storeConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Returns(
                    () =>
                        {
                            storeConnectionState =
                                ConnectionState.Open;

                            return Task.FromResult(true);
                        });
                storeConnectionMock.SetupGet(m => m.DataSource).Returns("fake");
                storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true, true);

                var executionStrategyMock = new Mock<IExecutionStrategy>();
                executionStrategyMock.Setup(m => m.ExecuteAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
                                     .Returns<Func<Task>, CancellationToken>(
                                         (f, c) =>
                                             {
                                                 storeConnectionMock.Verify(m => m.OpenAsync(It.IsAny<CancellationToken>()), Times.Never());
                                                 f().Wait();
                                                 storeConnectionMock.Verify(m => m.OpenAsync(It.IsAny<CancellationToken>()), Times.Once());
                                                 return Task.FromResult(true);
                                             });

                MutableResolver.AddResolver<IExecutionStrategy>(key => executionStrategyMock.Object);
                try
                {
                    entityConnection.OpenAsync().Wait();
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                }

                executionStrategyMock.Verify(m => m.ExecuteAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()), Times.Once());
            }

            [Fact]
            public void StoreConnection_state_mimics_EntityConnection_state_if_only_EntityConnection_is_used_with_ambient_txn()
            {
                using (new TransactionScope())
                {
                    var storeConnectionState = ConnectionState.Closed;
                    var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                    storeConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Callback(
                        () => storeConnectionState = ConnectionState.Open).Returns(Task.FromResult(1));
                    storeConnectionMock.Setup(m => m.Close()).Callback(() => storeConnectionState = ConnectionState.Closed);
                    storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);
                    storeConnectionMock.SetupGet(m => m.DataSource).Returns("fake");

                    var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                    metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                    var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true, true);
                    Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                    Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state

                    // open entityConnection (automatically opens store connection)
                    entityConnection.OpenAsync().Wait();
                    storeConnectionMock.Raise(
                        conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                    Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state
                    Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State); // underlying storeConnection state

                    // close entityConnection without explicitly closing underlying store connection 
                    entityConnection.Close();
                    storeConnectionMock.Raise(
                        conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

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
                storeConnectionMock.SetupGet(m => m.DataSource).Returns("fake");

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true, true);
                Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state

                // open entityConnection (automatically opens store connection)
                entityConnection.OpenAsync().Wait();
                storeConnectionMock.Raise(
                    conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State); // underlying storeConnection state

                // close entityConnection without explicitly closing underlying store connection 
                entityConnection.Close();
                storeConnectionMock.Raise(
                    conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

                Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state
            }

            [Fact]
            public void EntityConnection_automatically_opened_if_underlying_StoreConnection_is_opened_with_ambient_txn()
            {
                using (new TransactionScope())
                {
                    var storeConnectionState = ConnectionState.Closed;
                    var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                    storeConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Callback(
                        () => storeConnectionState = ConnectionState.Open).Returns(Task.FromResult(1));
                    storeConnectionMock.Setup(m => m.Close()).Callback(() => storeConnectionState = ConnectionState.Closed);
                    storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);
                    storeConnectionMock.SetupGet(m => m.DataSource).Returns("fake");

                    var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                    var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true, true);
                    Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                    Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state

                    // open underlying store connection without explicitly opening entityConnection
                    entityConnection.StoreConnection.OpenAsync().Wait();
                    storeConnectionMock.Raise(
                        conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                    Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state is automatically updated
                    Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State);

                    // now close underlying store connection (without explicitly closing entityConnection)
                    entityConnection.StoreConnection.Close();
                    storeConnectionMock.Raise(
                        conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

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
                storeConnectionMock.SetupGet(m => m.DataSource).Returns("fake");

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true, true);
                Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state

                // open underlying store connection without explicitly opening entityConnection
                entityConnection.StoreConnection.OpenAsync().Wait();
                storeConnectionMock.Raise(
                    conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));

                Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state is automatically updated
                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State);

                // now close underlying store connection (without explicitly closing entityConnection)
                entityConnection.StoreConnection.Close();
                storeConnectionMock.Raise(
                    conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

                Assert.Equal(ConnectionState.Closed, entityConnection.State);
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State);
            }

            [Fact]
            public void EntityConnection_automatically_closed_if_underlying_StoreConnection_is_closed_with_ambient_txn()
            {
                using (new TransactionScope())
                {
                    var storeConnectionState = ConnectionState.Closed;
                    var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                    storeConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Callback(
                        () => storeConnectionState = ConnectionState.Open).Returns(Task.FromResult(1));
                    storeConnectionMock.Setup(m => m.Close()).Callback(() => storeConnectionState = ConnectionState.Closed);
                    storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);
                    storeConnectionMock.SetupGet(m => m.DataSource).Returns("fake");

                    var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                    metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                    // open entityConnection - both entityConnection and store connection should now be open
                    var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true, true);
                    entityConnection.OpenAsync().Wait();
                    storeConnectionMock.Raise(
                        conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));
                    Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state
                    Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State); // underlying storeConnection state

                    // now close the underlying store connection without explicitly closing entityConnection
                    entityConnection.StoreConnection.Close();
                    storeConnectionMock.Raise(
                        conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

                    Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state automatically updated
                    Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State);

                    // now re-open the store connection and EntityConnection is "resurrected" to an Open state
                    entityConnection.StoreConnection.OpenAsync().Wait();
                    storeConnectionMock.Raise(
                        conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));
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
                storeConnectionMock.SetupGet(m => m.DataSource).Returns("fake");

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                // open entityConnection - both entityConnection and store connection should now be open
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true, true);
                entityConnection.OpenAsync().Wait();
                storeConnectionMock.Raise(
                    conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));
                Assert.Equal(ConnectionState.Open, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State); // underlying storeConnection state

                // now close the underlying store connection without explicitly closing entityConnection
                entityConnection.StoreConnection.Close();
                storeConnectionMock.Raise(
                    conn => conn.StateChange -= null, new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));

                Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state automatically updated
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State);

                // now re-open the store connection and EntityConnection is "resurrected" to an Open state
                entityConnection.StoreConnection.OpenAsync().Wait();
                storeConnectionMock.Raise(
                    conn => conn.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));
                Assert.Equal(ConnectionState.Open, entityConnection.State);
                Assert.Equal(ConnectionState.Open, entityConnection.StoreConnection.State);
            }
        }

#endif

        public class Begin : TestBase
        {
            [Fact]
            public void ExecutionStrategy_is_used_to_recover_from_a_transient_error()
            {
                var storeConnectionState = ConnectionState.Open;
                var transientExceptionThrown = false;
                var storeConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                storeConnectionMock.Setup(m => m.Close()).Callback(() => storeConnectionState = ConnectionState.Closed);
                storeConnectionMock.Setup(m => m.Open()).Callback(() => storeConnectionState = ConnectionState.Open);
                storeConnectionMock.SetupGet(m => m.DataSource).Returns("fake");
                storeConnectionMock.SetupGet(m => m.State).Returns(() => storeConnectionState);
                storeConnectionMock.Protected().Setup<DbTransaction>("BeginDbTransaction", IsolationLevel.Unspecified)
                                   .Returns<IsolationLevel>(
                                       il =>
                                           {
                                               if (!transientExceptionThrown)
                                               {
                                                   transientExceptionThrown = true;
                                                   storeConnectionState = ConnectionState.Broken;
                                                   throw new TimeoutException();
                                               }
                                               return new Mock<DbTransaction>().Object;
                                           });

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, storeConnectionMock.Object, true, true);

                var executionStrategyMock = new Mock<IExecutionStrategy>();
                executionStrategyMock.Setup(m => m.Execute(It.IsAny<Func<DbTransaction>>())).Returns<Func<DbTransaction>>(
                    a =>
                        {
                            storeConnectionMock.Protected()
                                               .Verify<DbTransaction>(
                                                   "BeginDbTransaction", Times.Never(), IsolationLevel.Unspecified);

                            Assert.Throws<TimeoutException>(() => a());

                            storeConnectionMock.Protected()
                                               .Verify<DbTransaction>(
                                                   "BeginDbTransaction", Times.Once(), IsolationLevel.Unspecified);

                            var result = a();

                            storeConnectionMock.Protected()
                                               .Verify<DbTransaction>(
                                                   "BeginDbTransaction", Times.Exactly(2), IsolationLevel.Unspecified);

                            return result;
                        });

                MutableResolver.AddResolver<IExecutionStrategy>(key => executionStrategyMock.Object);
                try
                {
                    entityConnection.BeginTransaction();
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                }

                executionStrategyMock.Verify(m => m.Execute(It.IsAny<Func<DbTransaction>>()), Times.Once());
            }
        }

        public class UseTransaction : TestBase
        {
            [Fact]
            public void Passing_null_to_UseStoreTransaction_on_EntityConnection_Clears_Current_Transaction()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                // note: entityConnection will already appear open to mock transaction since underlying connection is open
                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true, true);

                var dbTransactionMock = new Mock<DbTransaction>();
                dbTransactionMock.Protected().SetupGet<DbConnection>("DbConnection").Returns(() => entityConnection.StoreConnection);

                entityConnection.UseStoreTransaction(dbTransactionMock.Object);

                Assert.NotNull(entityConnection.CurrentTransaction);

                entityConnection.UseStoreTransaction(null);

                Assert.Null(entityConnection.CurrentTransaction);
            }

            [Fact]
            public void Passing_a_transaction_to_UseStoreTransaction_when_it_is_already_using_a_transaction_throws_InvalidOperationException()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true, true);

                // set up and use a transaction from the same underlying store connection
                var dbTransactionMock = new Mock<DbTransaction>();
                dbTransactionMock.Protected().SetupGet<DbConnection>("DbConnection").Returns(() => dbConnectionMock.Object);
                entityConnection.UseStoreTransaction(dbTransactionMock.Object);

                // set up a different transaction
                var dbTransactionMock2 = new Mock<DbTransaction>();

                Assert.Equal(
                    Strings.DbContext_TransactionAlreadyStarted,
                    Assert.Throws<InvalidOperationException>(() => entityConnection.UseStoreTransaction(dbTransactionMock2.Object)).Message);
            }

            [Fact]
            public void
                Passing_a_transaction_to_UseStoreTransaction_when_it_is_already_enlisted_in_a_TransactionScope_Transaction_throws_InvalidOperationException()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
                dbConnectionMock.Setup(m => m.EnlistTransaction(It.IsAny<Transaction>())).Verifiable();

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                using (new TransactionScope())
                {
                    var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true, true);
                    entityConnection.EnlistTransaction(Transaction.Current);

                    var dbTransactionMock = new Mock<DbTransaction>();
                    dbTransactionMock.Protected().SetupGet<DbConnection>("DbConnection").Returns(() => dbConnectionMock.Object);
                    Assert.Equal(
                        Strings.DbContext_TransactionAlreadyEnlistedInUserTransaction,
                        Assert.Throws<InvalidOperationException>(() => entityConnection.UseStoreTransaction(dbTransactionMock.Object))
                              .Message);
                }
            }

            [Fact]
            public void Passing_a_transaction_with_no_connection_to_UseStoreTransaction_throws_InvalidOperationException()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true, true);

                // set up a transaction with no underlying store connection
                var otherDbTransactionMock = new Mock<DbTransaction>();
                otherDbTransactionMock.Protected().SetupGet<DbConnection>("DbConnection").Returns(() => null);

                Assert.Equal(
                    Strings.DbContext_InvalidTransactionNoConnection,
                    Assert.Throws<InvalidOperationException>(() => entityConnection.UseStoreTransaction(otherDbTransactionMock.Object))
                          .Message);
            }

            [Fact]
            public void Passing_a_transaction_from_a_different_connection_to_UseStoreTransaction_throws_InvalidOperationException()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true, true);

                // set up a transaction from a different underlying store connection
                var otherDbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                otherDbConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
                var otherDbTransactionMock = new Mock<DbTransaction>();
                otherDbTransactionMock.Protected().SetupGet<DbConnection>("DbConnection").Returns(() => otherDbConnectionMock.Object);

                Assert.Equal(
                    Strings.DbContext_InvalidTransactionForConnection,
                    Assert.Throws<InvalidOperationException>(() => entityConnection.UseStoreTransaction(otherDbTransactionMock.Object))
                          .Message);
            }

            [Fact]
            public void Passing_a_valid_transaction_to_UseStoreTransaction_sets_CurrentTransaction()
            {
                var dbConnectionMock = new Mock<DbConnection>(MockBehavior.Strict);
                dbConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                var entityConnection = new EntityConnection(metadataWorkspaceMock.Object, dbConnectionMock.Object, true, true);

                var dbTransactionMock = new Mock<DbTransaction>();
                dbTransactionMock.Protected().SetupGet<DbConnection>("DbConnection").Returns(() => dbConnectionMock.Object);
                entityConnection.UseStoreTransaction(dbTransactionMock.Object);

                Assert.Equal(dbTransactionMock.Object, entityConnection.CurrentTransaction.StoreTransaction);
            }
        }
    }
}
