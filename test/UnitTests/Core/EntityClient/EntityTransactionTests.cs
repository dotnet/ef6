// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityClient
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Interception;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class EntityTransactionTests
    {
        public class Connection
        {
            [Fact]
            public void Uses_interception()
            {
                var transactionMock = new Mock<DbTransaction>();
                var transaction = new EntityTransaction(new EntityConnection(), transactionMock.Object);

                var transactionInterceptorMock = new Mock<IDbTransactionInterceptor>();
                DbInterception.Add(transactionInterceptorMock.Object);
                try
                {
                    Assert.Null(transaction.Connection);
                }
                finally
                {
                    DbInterception.Remove(transactionInterceptorMock.Object);
                }

                transactionInterceptorMock.Verify(
                    m => m.ConnectionGetting(It.IsAny<DbTransaction>(), It.IsAny<DbTransactionInterceptionContext<DbConnection>>()),
                        Times.Once());
                transactionInterceptorMock.Verify(
                    m => m.ConnectionGot(It.IsAny<DbTransaction>(), It.IsAny<DbTransactionInterceptionContext<DbConnection>>()),
                        Times.Once());
                transactionMock.Protected().Verify<DbConnection>("DbConnection", Times.Once());
            }
        }

        public class IsolationLevelTests
        {
            [Fact]
            public void Uses_interception()
            {
                var transactionMock = new Mock<DbTransaction>();
                var transaction = new EntityTransaction(new EntityConnection(), transactionMock.Object);
                var transactionInterceptorMock = new Mock<IDbTransactionInterceptor>();
                DbInterception.Add(transactionInterceptorMock.Object);
                try
                {
                    Assert.Equal((IsolationLevel)0, transaction.IsolationLevel);
                }
                finally
                {
                    DbInterception.Remove(transactionInterceptorMock.Object);
                }

                transactionInterceptorMock.Verify(
                    m => m.IsolationLevelGetting(It.IsAny<DbTransaction>(), It.IsAny<DbTransactionInterceptionContext<IsolationLevel>>()),
                        Times.Once());
                transactionInterceptorMock.Verify(
                    m => m.IsolationLevelGot(It.IsAny<DbTransaction>(), It.IsAny<DbTransactionInterceptionContext<IsolationLevel>>()),
                        Times.Once());
                transactionMock.Verify(m => m.IsolationLevel, Times.Once());
            }
        }

        public class Commit
        {
            [Fact]
            public void Commit_wraps_exceptions()
            {
                var transactionMock = new Mock<DbTransaction>();
                transactionMock.Setup(m => m.Commit()).Throws<NotImplementedException>();
                var transaction = new EntityTransaction(new EntityConnection(), transactionMock.Object);

                Assert.Throws<EntityException>(() => transaction.Commit());
            }

            [Fact]
            public void Commit_does_not_wrap_CommitFailedException()
            {
                var transactionMock = new Mock<DbTransaction>();
                transactionMock.Setup(m => m.Commit()).Throws<CommitFailedException>();
                transactionMock.Protected().Setup<DbConnection>("DbConnection").Returns(new Mock<DbConnection>().Object);
                var transaction = new EntityTransaction(new EntityConnection(), transactionMock.Object);

                Assert.Throws<CommitFailedException>(() => transaction.Commit());
            }

            [Fact]
            public void Uses_interception()
            {
                var transactionMock = new Mock<DbTransaction>();
                transactionMock.Protected().Setup<DbConnection>("DbConnection").Returns(new Mock<DbConnection>().Object);
                var transaction = new EntityTransaction(new EntityConnection(), transactionMock.Object);
                var transactionInterceptorMock = new Mock<IDbTransactionInterceptor>();
                DbInterception.Add(transactionInterceptorMock.Object);
                try
                {
                    transaction.Commit();
                }
                finally
                {
                    DbInterception.Remove(transactionInterceptorMock.Object);
                }

                transactionInterceptorMock.Verify(
                    m => m.Committing(It.IsAny<DbTransaction>(), It.IsAny<DbTransactionInterceptionContext>()),
                        Times.Once());
                transactionInterceptorMock.Verify(
                    m => m.Committed(It.IsAny<DbTransaction>(), It.IsAny<DbTransactionInterceptionContext>()),
                        Times.Once());
                transactionMock.Verify(m => m.Commit(), Times.Once());
            }
        }

        public class Rollback
        {
            [Fact]
            public void Uses_interception()
            {
                var transactionMock = new Mock<DbTransaction>();
                transactionMock.Protected().Setup<DbConnection>("DbConnection").Returns(new Mock<DbConnection>().Object);
                var transaction = new EntityTransaction(new EntityConnection(), transactionMock.Object);
                var transactionInterceptorMock = new Mock<IDbTransactionInterceptor>();
                DbInterception.Add(transactionInterceptorMock.Object);
                try
                {
                    transaction.Rollback();
                }
                finally
                {
                    DbInterception.Remove(transactionInterceptorMock.Object);
                }

                transactionInterceptorMock.Verify(
                    m => m.RollingBack(It.IsAny<DbTransaction>(), It.IsAny<DbTransactionInterceptionContext>()),
                        Times.Once());
                transactionInterceptorMock.Verify(
                    m => m.RolledBack(It.IsAny<DbTransaction>(), It.IsAny<DbTransactionInterceptionContext>()),
                        Times.Once());
                transactionMock.Verify(m => m.Rollback(), Times.Once());
            }
        }

        public class Dispose
        {
            [Fact]
            public void Uses_interception()
            {
                var transactionMock = new Mock<DbTransaction>();
                var transaction = new EntityTransaction(new EntityConnection(), transactionMock.Object);
                var transactionInterceptorMock = new Mock<IDbTransactionInterceptor>();
                DbInterception.Add(transactionInterceptorMock.Object);
                try
                {
                    transaction.Dispose();
                }
                finally
                {
                    DbInterception.Remove(transactionInterceptorMock.Object);
                }

                transactionInterceptorMock.Verify(
                    m => m.Disposing(It.IsAny<DbTransaction>(), It.IsAny<DbTransactionInterceptionContext>()),
                        Times.Once());
                transactionInterceptorMock.Verify(
                    m => m.Disposed(It.IsAny<DbTransaction>(), It.IsAny<DbTransactionInterceptionContext>()),
                        Times.Once());
                transactionMock.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
            }
        }
    }
}
