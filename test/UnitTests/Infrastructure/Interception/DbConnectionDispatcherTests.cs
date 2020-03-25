// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using Moq;
    using Moq.Protected;
    using Xunit;
    using IsolationLevel = System.Data.IsolationLevel;

    public class DbConnectionDispatcherTests : TestBase
    {
        [Fact]
        public void BeginTransaction_executes_operation_and_dispatches_to_interceptors()
        {
            var mockConnection = new Mock<DbConnection>();
            var mockInterceptor = new Mock<IDbConnectionInterceptor>();
            var dispatcher = new DbConnectionDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(mockInterceptor.Object);

            var interceptionContext = new BeginTransactionInterceptionContext().WithIsolationLevel(IsolationLevel.Serializable);
            dispatcher.BeginTransaction(mockConnection.Object, interceptionContext);

            mockConnection.Protected().Verify<DbTransaction>("BeginDbTransaction", Times.Once(), IsolationLevel.Serializable);
            mockInterceptor.Verify(m => m.BeginningTransaction(mockConnection.Object, It.IsAny<BeginTransactionInterceptionContext>()), Times.Once());
            mockInterceptor.Verify(m => m.BeganTransaction(mockConnection.Object, It.IsAny<BeginTransactionInterceptionContext>()), Times.Once());
        }

        [Fact]
        public void Close_executes_operation_and_dispatches_to_interceptors()
        {
            var mockConnection = new Mock<DbConnection>();
            var mockInterceptor = new Mock<IDbConnectionInterceptor>();
            var dispatcher = new DbConnectionDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(mockInterceptor.Object);

            var interceptionContext = new DbConnectionInterceptionContext();
            dispatcher.Close(mockConnection.Object, interceptionContext);

            mockConnection.Verify(m => m.Close(), Times.Once());
            mockInterceptor.Verify(m => m.Closing(mockConnection.Object, It.IsAny<DbConnectionInterceptionContext>()), Times.Once());
            mockInterceptor.Verify(m => m.Closed(mockConnection.Object, It.IsAny<DbConnectionInterceptionContext>()), Times.Once());
        }

        [Fact]
        public void Dispose_executes_operation_and_dispatches_to_interceptors()
        {
            var mockConnection = new Mock<DbConnection>();
            var mockInterceptor = new Mock<IDbConnectionInterceptor>();
            var dispatcher = new DbConnectionDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(mockInterceptor.Object);

            var interceptionContext = new DbConnectionInterceptionContext();
            dispatcher.Dispose(mockConnection.Object, interceptionContext);

            mockConnection.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
            mockInterceptor.Verify(m => m.Disposing(mockConnection.Object, It.IsAny<DbConnectionInterceptionContext>()), Times.Once());
            mockInterceptor.Verify(m => m.Disposed(mockConnection.Object, It.IsAny<DbConnectionInterceptionContext>()), Times.Once());
        }

        [Fact]
        public void EnlistTransaction_executes_operation_and_dispatches_to_interceptors()
        {
            var mockConnection = new Mock<DbConnection>();
            var mockInterceptor = new Mock<IDbConnectionInterceptor>();
            var dispatcher = new DbConnectionDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(mockInterceptor.Object);

            var transaction = new CommittableTransaction();
            var interceptionContext = new EnlistTransactionInterceptionContext().WithTransaction(transaction);
            dispatcher.EnlistTransaction(mockConnection.Object, interceptionContext);

            mockConnection.Verify(m => m.EnlistTransaction(transaction), Times.Once());
            mockInterceptor.Verify(m => m.EnlistingTransaction(mockConnection.Object, It.IsAny<EnlistTransactionInterceptionContext>()), Times.Once());
            mockInterceptor.Verify(m => m.EnlistedTransaction(mockConnection.Object, It.IsAny<EnlistTransactionInterceptionContext>()), Times.Once());
        }

        [Fact]
        public void Open_executes_operation_and_dispatches_to_interceptors()
        {
            var mockConnection = new Mock<DbConnection>();
            var mockInterceptor = new Mock<IDbConnectionInterceptor>();
            var dispatcher = new DbConnectionDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(mockInterceptor.Object);

            var interceptionContext = new DbConnectionInterceptionContext();
            dispatcher.Open(mockConnection.Object, interceptionContext);

            mockConnection.Verify(m => m.Open(), Times.Once());
            mockInterceptor.Verify(m => m.Opening(mockConnection.Object, It.IsAny<DbConnectionInterceptionContext>()), Times.Once());
            mockInterceptor.Verify(m => m.Opened(mockConnection.Object, It.IsAny<DbConnectionInterceptionContext>()), Times.Once());
        }

#if !NET40
        [Fact]
        public void OpenAsync_executes_operation_and_dispatches_to_interceptors()
        {
            var mockConnection = new Mock<DbConnection>();
            mockConnection.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Returns(() => Task.FromResult(true));
            var mockInterceptor = new Mock<IDbConnectionInterceptor>();
            var dispatcher = new DbConnectionDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(mockInterceptor.Object);

            var interceptionContext = new DbConnectionInterceptionContext();
            dispatcher.OpenAsync(mockConnection.Object, interceptionContext, CancellationToken.None).Wait();

            mockConnection.Verify(m => m.OpenAsync(CancellationToken.None), Times.Once());
            mockInterceptor.Verify(m => m.Opening(mockConnection.Object, It.IsAny<DbConnectionInterceptionContext>()), Times.Once());
            mockInterceptor.Verify(m => m.Opened(mockConnection.Object, It.IsAny<DbConnectionInterceptionContext>()), Times.Once());
        }
#endif

        [Fact]
        public void GetConnectionString_executes_operation_and_dispatches_to_interceptors()
        {
            var mockConnection = new Mock<DbConnection>();
            var mockInterceptor = new Mock<IDbConnectionInterceptor>();
            var dispatcher = new DbConnectionDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(mockInterceptor.Object);

            var interceptionContext = new DbInterceptionContext();
            dispatcher.GetConnectionString(mockConnection.Object, interceptionContext);

            mockConnection.Verify(m => m.ConnectionString, Times.Once());
            mockInterceptor.Verify(m => m.ConnectionStringGetting(mockConnection.Object, It.IsAny<DbConnectionInterceptionContext<string>>()), Times.Once());
            mockInterceptor.Verify(m => m.ConnectionStringGot(mockConnection.Object, It.IsAny<DbConnectionInterceptionContext<string>>()), Times.Once());
        }

        [Fact]
        public void SetConnectionString_executes_operation_and_dispatches_to_interceptors()
        {
            var mockConnection = new Mock<DbConnection>();
            var mockInterceptor = new Mock<IDbConnectionInterceptor>();
            var dispatcher = new DbConnectionDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(mockInterceptor.Object);

            var interceptionContext = new DbConnectionPropertyInterceptionContext<string>().WithValue("foo");
            dispatcher.SetConnectionString(mockConnection.Object, interceptionContext);

            mockConnection.VerifySet(m => m.ConnectionString = "foo", Times.Once());
            mockInterceptor.Verify(m => m.ConnectionStringSetting(mockConnection.Object, It.IsAny<DbConnectionPropertyInterceptionContext<string>>()), Times.Once());
            mockInterceptor.Verify(m => m.ConnectionStringSet(mockConnection.Object, It.IsAny<DbConnectionPropertyInterceptionContext<string>>()), Times.Once());
        }

        [Fact]
        public void GetConnectionTimeout_executes_operation_and_dispatches_to_interceptors()
        {
            var mockConnection = new Mock<DbConnection>();
            var mockInterceptor = new Mock<IDbConnectionInterceptor>();
            var dispatcher = new DbConnectionDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(mockInterceptor.Object);

            var interceptionContext = new DbInterceptionContext();
            dispatcher.GetConnectionTimeout(mockConnection.Object, interceptionContext);

            mockConnection.Verify(m => m.ConnectionTimeout, Times.Once());
            mockInterceptor.Verify(m => m.ConnectionTimeoutGetting(mockConnection.Object, It.IsAny<DbConnectionInterceptionContext<int>>()), Times.Once());
            mockInterceptor.Verify(m => m.ConnectionTimeoutGot(mockConnection.Object, It.IsAny<DbConnectionInterceptionContext<int>>()), Times.Once());
        }

        [Fact]
        public void GetDatabase_executes_operation_and_dispatches_to_interceptors()
        {
            var mockConnection = new Mock<DbConnection>();
            var mockInterceptor = new Mock<IDbConnectionInterceptor>();
            var dispatcher = new DbConnectionDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(mockInterceptor.Object);

            var interceptionContext = new DbInterceptionContext();
            dispatcher.GetDatabase(mockConnection.Object, interceptionContext);

            mockConnection.Verify(m => m.Database, Times.Once());
            mockInterceptor.Verify(m => m.DatabaseGetting(mockConnection.Object, It.IsAny<DbConnectionInterceptionContext<string>>()), Times.Once());
            mockInterceptor.Verify(m => m.DatabaseGot(mockConnection.Object, It.IsAny<DbConnectionInterceptionContext<string>>()), Times.Once());
        }

        [Fact]
        public void GetDataSource_executes_operation_and_dispatches_to_interceptors()
        {
            var mockConnection = new Mock<DbConnection>();
            var mockInterceptor = new Mock<IDbConnectionInterceptor>();
            var dispatcher = new DbConnectionDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(mockInterceptor.Object);

            var interceptionContext = new DbInterceptionContext();
            dispatcher.GetDataSource(mockConnection.Object, interceptionContext);

            mockConnection.Verify(m => m.DataSource, Times.Once());
            mockInterceptor.Verify(m => m.DataSourceGetting(mockConnection.Object, It.IsAny<DbConnectionInterceptionContext<string>>()), Times.Once());
            mockInterceptor.Verify(m => m.DataSourceGot(mockConnection.Object, It.IsAny<DbConnectionInterceptionContext<string>>()), Times.Once());
        }

        [Fact]
        public void GetServerVersion_executes_operation_and_dispatches_to_interceptors()
        {
            var mockConnection = new Mock<DbConnection>();
            var mockInterceptor = new Mock<IDbConnectionInterceptor>();
            var dispatcher = new DbConnectionDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(mockInterceptor.Object);

            var interceptionContext = new DbInterceptionContext();
            dispatcher.GetServerVersion(mockConnection.Object, interceptionContext);

            mockConnection.Verify(m => m.ServerVersion, Times.Once());
            mockInterceptor.Verify(m => m.ServerVersionGetting(mockConnection.Object, It.IsAny<DbConnectionInterceptionContext<string>>()), Times.Once());
            mockInterceptor.Verify(m => m.ServerVersionGot(mockConnection.Object, It.IsAny<DbConnectionInterceptionContext<string>>()), Times.Once());
        }

        [Fact]
        public void GetState_executes_operation_and_dispatches_to_interceptors()
        {
            var mockConnection = new Mock<DbConnection>();
            var mockInterceptor = new Mock<IDbConnectionInterceptor>();
            var dispatcher = new DbConnectionDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(mockInterceptor.Object);

            var interceptionContext = new DbInterceptionContext();
            dispatcher.GetState(mockConnection.Object, interceptionContext);

            mockConnection.Verify(m => m.State, Times.Once());
            mockInterceptor.Verify(m => m.StateGetting(mockConnection.Object, It.IsAny<DbConnectionInterceptionContext<ConnectionState>>()), Times.Once());
            mockInterceptor.Verify(m => m.StateGot(mockConnection.Object, It.IsAny<DbConnectionInterceptionContext<ConnectionState>>()), Times.Once());
        }
    }
}
