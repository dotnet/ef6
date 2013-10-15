// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Common;
    using Moq;
    using Moq.Protected;
    using Xunit;

    internal class DbConnectionDispatcherTests
    {
        [Fact]
        public void BeginTransaction_executes_operation_and_dispatches_to_interceptors()
        {
            var mockConnection = new Mock<DbConnection>();
            var mockInterceptor = new Mock<IDbConnectionInterceptor>();
            var dispatcher = new DbConnectionDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(mockInterceptor.Object);

            var interceptionContext = new BeginTransactionInterceptionContext();
            dispatcher.BeginTransaction(mockConnection.Object, interceptionContext);

            mockConnection.Protected().Verify<DbTransaction>("BeginDbTransaction", Times.Once(), ItExpr.IsAny<IsolationLevel>());
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
    }
}
