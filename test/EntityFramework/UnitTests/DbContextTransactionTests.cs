// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class DbContextTransactionTests : TestBase
    {
        #region Constructors

        [Fact]
        public void Creating_DbContextTransaction_does_not_open_connection_if_already_open()
        {
            var mockEntityTransaction = new Mock<EntityTransaction>();
            var mockEntityConnection = new Mock<EntityConnection>();
            mockEntityConnection.SetupGet(m => m.State).Returns(ConnectionState.Open);
            mockEntityConnection.Setup(m => m.BeginTransaction()).Returns(mockEntityTransaction.Object);

            var dbContextTransaction = new DbContextTransaction(mockEntityConnection.Object);

            mockEntityConnection.Verify(m => m.Open(), Times.Never());
        }

        [Fact]
        public void Creating_DbContextTransaction_specifying_IsolationLevel_does_not_open_connection_if_already_open()
        {
            var mockEntityTransaction = new Mock<EntityTransaction>();
            var mockEntityConnection = new Mock<EntityConnection>();
            mockEntityConnection.SetupGet(m => m.State).Returns(ConnectionState.Open);
            mockEntityConnection.Setup(m => m.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(mockEntityTransaction.Object);

            var dbContextTransaction = new DbContextTransaction(mockEntityConnection.Object, IsolationLevel.Snapshot);

            mockEntityConnection.Verify(m => m.Open(), Times.Never());
        }

        [Fact]
        public void Creating_DbContextTransaction_opens_connection_if_closed()
        {
            var connectionState = ConnectionState.Closed;
            var mockEntityTransaction = new Mock<EntityTransaction>();
            var mockEntityConnection = new Mock<EntityConnection>();
            mockEntityConnection.SetupGet(m => m.State).Returns(() => connectionState);
            mockEntityConnection.Setup(m => m.Open()).Callback(() => connectionState = ConnectionState.Open);
            mockEntityConnection.Setup(m => m.BeginTransaction()).Returns(mockEntityTransaction.Object);

            var dbContextTransaction = new DbContextTransaction(mockEntityConnection.Object);

            mockEntityConnection.Verify(m => m.Open(), Times.Once());
            Assert.Equal(ConnectionState.Open, connectionState);
        }

        [Fact]
        public void Creating_DbContextTransaction_specifying_IsolationLevel_opens_connection_if_closed()
        {
            var connectionState = ConnectionState.Closed;
            var mockEntityTransaction = new Mock<EntityTransaction>();
            var mockEntityConnection = new Mock<EntityConnection>();
            mockEntityConnection.SetupGet(m => m.State).Returns(() => connectionState);
            mockEntityConnection.Setup(m => m.Open()).Callback(() => connectionState = ConnectionState.Open);
            mockEntityConnection.Setup(m => m.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(mockEntityTransaction.Object);

            var dbContextTransaction = new DbContextTransaction(mockEntityConnection.Object, IsolationLevel.RepeatableRead);

            mockEntityConnection.Verify(m => m.Open(), Times.Once());
            Assert.Equal(ConnectionState.Open, connectionState);
        }

        #endregion

        #region Getters

        [Fact]
        public void Calling_DbContextTransaction_StoreTransaction_returns_underlying_StoreTransaction()
        {
            var mockDbTransaction = new Mock<DbTransaction>();
            var mockEntityTransaction = new Mock<EntityTransaction>();
            mockEntityTransaction.Setup(m => m.StoreTransaction).Returns(mockDbTransaction.Object);
            var mockEntityConnection = new Mock<EntityConnection>();
            mockEntityConnection.SetupGet(m => m.State).Returns(ConnectionState.Open);
            mockEntityConnection.Setup(m => m.BeginTransaction()).Returns(mockEntityTransaction.Object);

            var dbContextTransaction = new DbContextTransaction(mockEntityConnection.Object);

            Assert.Equal(mockDbTransaction.Object, dbContextTransaction.StoreTransaction);
        }

        #endregion

        #region Commit & Rollback

        [Fact]
        public void Calling_Commit_on_DbContextTransaction_calls_Commit_on_underlying_EntityTransaction()
        {
            var mockEntityTransaction = new Mock<EntityTransaction>();
            var mockEntityConnection = new Mock<EntityConnection>();
            mockEntityConnection.SetupGet(m => m.State).Returns(ConnectionState.Open);
            mockEntityConnection.Setup(m => m.BeginTransaction()).Returns(mockEntityTransaction.Object);

            var dbContextTransaction = new DbContextTransaction(mockEntityConnection.Object);
            dbContextTransaction.Commit();

            mockEntityTransaction.Verify(m => m.Commit(), Times.Once());
        }

        [Fact]
        public void Calling_Rollback_on_DbContextTransaction_calls_Rollback_on_underlying_EntityTransaction()
        {
            var mockEntityTransaction = new Mock<EntityTransaction>();
            var mockEntityConnection = new Mock<EntityConnection>();
            mockEntityConnection.SetupGet(m => m.State).Returns(ConnectionState.Open);
            mockEntityConnection.Setup(m => m.BeginTransaction()).Returns(mockEntityTransaction.Object);

            var dbContextTransaction = new DbContextTransaction(mockEntityConnection.Object);
            dbContextTransaction.Rollback();

            mockEntityTransaction.Verify(m => m.Rollback(), Times.Once());
        }

        #endregion

        #region Dispose

        [Fact]
        public void Calling_Dispose_on_DbContextTransaction_which_did_not_open_the_connection_leaves_connection_open()
        {
            var connectionState = ConnectionState.Open;
            var mockEntityTransaction = new Mock<EntityTransaction>();
            mockEntityTransaction.Protected().Setup("Dispose", true).Verifiable();
            var mockEntityConnection = new Mock<EntityConnection>();
            mockEntityConnection.SetupGet(m => m.State).Returns(() => connectionState);
            mockEntityConnection.Setup(m => m.Open()).Callback(() => connectionState = ConnectionState.Open);
            mockEntityConnection.Setup(m => m.Close()).Callback(() => connectionState = ConnectionState.Closed);
            mockEntityConnection.Setup(m => m.BeginTransaction()).Returns(mockEntityTransaction.Object);

            var dbContextTransaction = new DbContextTransaction(mockEntityConnection.Object);
            Assert.Equal(ConnectionState.Open, connectionState);

            dbContextTransaction.Dispose();

            mockEntityTransaction.Protected().Verify("Dispose", Times.Once(), true);
            Assert.Equal(ConnectionState.Open, connectionState);
        }

        [Fact]
        public void Calling_Dispose_on_DbContextTransaction_which_opened_the_connection_closes_the_connection()
        {
            var connectionState = ConnectionState.Closed;
            var mockEntityTransaction = new Mock<EntityTransaction>();
            mockEntityTransaction.Protected().Setup("Dispose", true).Verifiable();
            var mockEntityConnection = new Mock<EntityConnection>();
            mockEntityConnection.SetupGet(m => m.State).Returns(() => connectionState);
            mockEntityConnection.Setup(m => m.Open()).Callback(() => connectionState = ConnectionState.Open);
            mockEntityConnection.Setup(m => m.Close()).Callback(() => connectionState = ConnectionState.Closed);
            mockEntityConnection.Setup(m => m.BeginTransaction()).Returns(mockEntityTransaction.Object);

            var dbContextTransaction = new DbContextTransaction(mockEntityConnection.Object);
            Assert.Equal(ConnectionState.Open, connectionState);

            dbContextTransaction.Dispose();

            mockEntityTransaction.Protected().Verify("Dispose", Times.Once(), true);
            Assert.Equal(ConnectionState.Closed, connectionState);
        }

        #endregion
    }
}
