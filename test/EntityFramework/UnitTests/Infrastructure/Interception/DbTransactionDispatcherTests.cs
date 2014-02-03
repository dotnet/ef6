// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Common;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class DbTransactionDispatcherTests : TestBase
    {
        [Fact]
        public void Commit_executes_operation_and_dispatches_to_interceptors()
        {
            var transactionMock = new Mock<DbTransaction>();
            transactionMock.Protected().Setup<DbConnection>("DbConnection").Returns(new Mock<DbConnection>().Object);
            var interceptorMock = new Mock<IDbTransactionInterceptor>();
            var dispatcher = new DbTransactionDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(interceptorMock.Object);

            var interceptionContext = new DbInterceptionContext();
            dispatcher.Commit(transactionMock.Object, interceptionContext);

            transactionMock.Verify(m => m.Commit(), Times.Once());
            interceptorMock.Verify(m => m.Committing(transactionMock.Object, It.IsAny<DbTransactionInterceptionContext>()), Times.Once());
            interceptorMock.Verify(m => m.Committed(transactionMock.Object, It.IsAny<DbTransactionInterceptionContext>()), Times.Once());
        }

        [Fact]
        public void Dispose_executes_operation_and_dispatches_to_interceptors()
        {
            var transactionMock = new Mock<DbTransaction>();
            var interceptorMock = new Mock<IDbTransactionInterceptor>();
            var dispatcher = new DbTransactionDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(interceptorMock.Object);

            var interceptionContext = new DbInterceptionContext();
            dispatcher.Dispose(transactionMock.Object, interceptionContext);

            transactionMock.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
            interceptorMock.Verify(m => m.Disposing(transactionMock.Object, It.IsAny<DbTransactionInterceptionContext>()), Times.Once());
            interceptorMock.Verify(m => m.Disposed(transactionMock.Object, It.IsAny<DbTransactionInterceptionContext>()), Times.Once());
        }

        [Fact]
        public void GetConnection_executes_operation_and_dispatches_to_interceptors()
        {
            var transactionMock = new Mock<DbTransaction>();
            var interceptorMock = new Mock<IDbTransactionInterceptor>();
            var dispatcher = new DbTransactionDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(interceptorMock.Object);

            var interceptionContext = new DbInterceptionContext();
            dispatcher.GetConnection(transactionMock.Object, interceptionContext);

            transactionMock.Protected().Verify<DbConnection>("DbConnection", Times.Once());
            interceptorMock.Verify(
                m => m.ConnectionGetting(transactionMock.Object, It.IsAny<DbTransactionInterceptionContext<DbConnection>>()), Times.Once());
            interceptorMock.Verify(
                m => m.ConnectionGot(transactionMock.Object, It.IsAny<DbTransactionInterceptionContext<DbConnection>>()), Times.Once());
        }

        [Fact]
        public void GetIsolationLevel_executes_operation_and_dispatches_to_interceptors()
        {
            var transactionMock = new Mock<DbTransaction>();
            var interceptorMock = new Mock<IDbTransactionInterceptor>();
            var dispatcher = new DbTransactionDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(interceptorMock.Object);

            var interceptionContext = new DbInterceptionContext();
            dispatcher.GetIsolationLevel(transactionMock.Object, interceptionContext);

            transactionMock.Verify(m => m.IsolationLevel, Times.Once());
            interceptorMock.Verify(
                m => m.IsolationLevelGetting(transactionMock.Object, It.IsAny<DbTransactionInterceptionContext<IsolationLevel>>()),
                Times.Once());
            interceptorMock.Verify(
                m => m.IsolationLevelGot(transactionMock.Object, It.IsAny<DbTransactionInterceptionContext<IsolationLevel>>()), Times.Once());
        }

        [Fact]
        public void Rollback_executes_operation_and_dispatches_to_interceptors()
        {
            var transactionMock = new Mock<DbTransaction>();
            transactionMock.Protected().Setup<DbConnection>("DbConnection").Returns(new Mock<DbConnection>().Object);
            var interceptorMock = new Mock<IDbTransactionInterceptor>();
            var dispatcher = new DbTransactionDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(interceptorMock.Object);

            var interceptionContext = new DbInterceptionContext();
            dispatcher.Rollback(transactionMock.Object, interceptionContext);

            transactionMock.Verify(m => m.Rollback(), Times.Once());
            interceptorMock.Verify(m => m.RollingBack(transactionMock.Object, It.IsAny<DbTransactionInterceptionContext>()), Times.Once());
            interceptorMock.Verify(m => m.RolledBack(transactionMock.Object, It.IsAny<DbTransactionInterceptionContext>()), Times.Once());
        }
    }
}
