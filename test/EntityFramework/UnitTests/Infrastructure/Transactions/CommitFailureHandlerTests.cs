// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.TestDoubles;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;
    using Xunit;
    using MockHelper = System.Data.Entity.Core.Objects.MockHelper;

    public class CommitFailureHandlerTests
    {
        public class Initialize : TestBase
        {
            [Fact]
            public void Initializes_with_ObjectContext()
            {
                var context = MockHelper.CreateMockObjectContext<object>();

                using (var handler = CreateCommitFailureHandlerMock().Object)
                {
                    handler.Initialize(context);

                    Assert.Same(context, handler.ObjectContext);
                    Assert.Same(context.InterceptionContext.DbContexts.FirstOrDefault(), handler.DbContext);
                    Assert.Same(((EntityConnection)context.Connection).StoreConnection, handler.Connection);
                    Assert.Same(((EntityConnection)context.Connection).StoreConnection, handler.Connection);
                    Assert.NotNull(handler.TransactionContext);
                }
            }

            [Fact]
            public void Initializes_with_DbContext()
            {
                var context = new DbContext("c");

                using (var handler = CreateCommitFailureHandlerMock().Object)
                {
                    handler.Initialize(context, context.Database.Connection);

                    Assert.Null(handler.ObjectContext);
                    Assert.Same(context, handler.DbContext);
                    Assert.Same(context.Database.Connection, handler.Connection);
                    Assert.NotNull(handler.TransactionContext);
                }
            }

            [Fact]
            public void Throws_for_null_parameters()
            {
                using (var handler = new CommitFailureHandler())
                {
                    Assert.Equal(
                        "connection",
                        Assert.Throws<ArgumentNullException>(() => handler.Initialize(new DbContext("c"), null)).ParamName);
                    Assert.Equal(
                        "context",
                        Assert.Throws<ArgumentNullException>(() => handler.Initialize(null, new Mock<DbConnection>().Object)).ParamName);
                    Assert.Equal(
                        "context",
                        Assert.Throws<ArgumentNullException>(() => handler.Initialize(null)).ParamName);
                }
            }

            [Fact]
            public void Throws_if_already_initialized_with_ObjectContext()
            {
                var context = MockHelper.CreateMockObjectContext<object>();

                using (var handler = CreateCommitFailureHandlerMock().Object)
                {
                    handler.Initialize(context);

                    Assert.Equal(
                        Strings.TransactionHandler_AlreadyInitialized,
                        Assert.Throws<InvalidOperationException>(() => handler.Initialize(context)).Message);

                    var dbContext = new DbContext("c");
                    Assert.Equal(
                        Strings.TransactionHandler_AlreadyInitialized,
                        Assert.Throws<InvalidOperationException>(() => handler.Initialize(dbContext, dbContext.Database.Connection)).Message);
                }
            }

            [Fact]
            public void Throws_if_already_initialized_with_DbContext()
            {
                var dbContext = new DbContext("c");

                using (var handler = new CommitFailureHandler())
                {
                    handler.Initialize(dbContext, dbContext.Database.Connection);

                    var context = MockHelper.CreateMockObjectContext<object>();
                    Assert.Equal(
                        Strings.TransactionHandler_AlreadyInitialized,
                        Assert.Throws<InvalidOperationException>(() => handler.Initialize(context)).Message);

                    Assert.Equal(
                        Strings.TransactionHandler_AlreadyInitialized,
                        Assert.Throws<InvalidOperationException>(() => handler.Initialize(dbContext, dbContext.Database.Connection)).Message);
                }
            }
        }

        public class Dispose : TestBase
        {
            [Fact]
            public void Removes_interceptor()
            {
                var mockConnection = new Mock<DbConnection>().Object;
                var handlerMock = new Mock<CommitFailureHandler> { CallBase = true };
                using (handlerMock.Object)
                {
                    DbInterception.Dispatch.Connection.Close(mockConnection, new DbInterceptionContext());
                    handlerMock.Verify(m => m.Closed(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()), Times.Once());

                    handlerMock.Object.Dispose();

                    DbInterception.Dispatch.Connection.Close(mockConnection, new DbInterceptionContext());
                    handlerMock.Verify(m => m.Closed(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext>()), Times.Once());
                }
            }

            [Fact]
            public void Delegates_to_protected_method()
            {
                var context = MockHelper.CreateMockObjectContext<int>();
                var commitFailureHandlerMock = CreateCommitFailureHandlerMock();
                commitFailureHandlerMock.Object.Initialize(context);
                using (var handler = commitFailureHandlerMock.Object)
                {
                    handler.Dispose();
                    commitFailureHandlerMock.Protected().Verify("Dispose", Times.Once(), true);
                }
            }

            [Fact]
            public void Can_be_invoked_twice_without_throwing()
            {
                var handler = new CommitFailureHandler();

                handler.Dispose();
                handler.Dispose();
            }
        }

        public class MatchesParentContext : TestBase
        {
            [Fact]
            public void Throws_for_null_parameters()
            {
                using (var handler = new CommitFailureHandler())
                {
                    Assert.Equal(
                        "connection",
                        Assert.Throws<ArgumentNullException>(() => handler.MatchesParentContext(null, new DbInterceptionContext()))
                            .ParamName);
                    Assert.Equal(
                        "interceptionContext",
                        Assert.Throws<ArgumentNullException>(() => handler.MatchesParentContext(new Mock<DbConnection>().Object, null))
                            .ParamName);
                }
            }

            [Fact]
            public void Returns_false_with_DbContext_if_nothing_matches()
            {
                using (var handler = CreateCommitFailureHandlerMock().Object)
                {
                    handler.Initialize(new DbContext("c"), CreateMockConnection());

                    Assert.False(
                        handler.MatchesParentContext(
                            new Mock<DbConnection>().Object,
                            new DbInterceptionContext().WithObjectContext(MockHelper.CreateMockObjectContext<object>())
                                .WithDbContext(new DbContext("c"))));
                }
            }

            [Fact]
            public void Returns_false_with_DbContext_if_different_context_same_connection()
            {
                var connection = CreateMockConnection();
                using (var handler = CreateCommitFailureHandlerMock().Object)
                {
                    handler.Initialize(new DbContext("c"), connection);

                    Assert.False(
                        handler.MatchesParentContext(
                            connection,
                            new DbInterceptionContext().WithObjectContext(MockHelper.CreateMockObjectContext<object>())
                                .WithDbContext(new DbContext("c"))));
                }
            }

            [Fact]
            public void Returns_true_with_DbContext_if_same_context()
            {
                var context = new DbContext("c");
                using (var handler = CreateCommitFailureHandlerMock().Object)
                {
                    handler.Initialize(context, CreateMockConnection());

                    Assert.True(
                        handler.MatchesParentContext(
                            new Mock<DbConnection>().Object,
                            new DbInterceptionContext().WithObjectContext(MockHelper.CreateMockObjectContext<object>())
                                .WithDbContext(context)));
                }
            }

            [Fact]
            public void Returns_true_with_DbContext_if_no_context_same_connection()
            {
                var connection = CreateMockConnection();
                using (var handler = CreateCommitFailureHandlerMock().Object)
                {
                    handler.Initialize(new DbContext("c"), connection);

                    Assert.True(
                        handler.MatchesParentContext(
                            connection,
                            new DbInterceptionContext()));
                }
            }

            [Fact]
            public void Returns_false_with_ObjectContext_if_nothing_matches()
            {
                using (var handler = CreateCommitFailureHandlerMock().Object)
                {
                    handler.Initialize(MockHelper.CreateMockObjectContext<object>());

                    Assert.False(
                        handler.MatchesParentContext(
                            new Mock<DbConnection>().Object,
                            new DbInterceptionContext().WithObjectContext(MockHelper.CreateMockObjectContext<object>())
                                .WithDbContext(new DbContext("c"))));
                }
            }

            [Fact]
            public void Returns_false_with_ObjectContext_if_different_context_same_connection()
            {
                var context = MockHelper.CreateMockObjectContext<object>();
                using (var handler = CreateCommitFailureHandlerMock().Object)
                {
                    handler.Initialize(context);

                    Assert.False(
                        handler.MatchesParentContext(
                            ((EntityConnection)context.Connection).StoreConnection,
                            new DbInterceptionContext().WithObjectContext(MockHelper.CreateMockObjectContext<object>())
                                .WithDbContext(new DbContext("c"))));
                }
            }

            [Fact]
            public void Returns_true_with_ObjectContext_if_same_ObjectContext()
            {
                var context = MockHelper.CreateMockObjectContext<object>();
                using (var handler = CreateCommitFailureHandlerMock().Object)
                {
                    handler.Initialize(context);

                    Assert.True(
                        handler.MatchesParentContext(
                            new Mock<DbConnection>().Object,
                            new DbInterceptionContext().WithObjectContext(context)
                                .WithDbContext(new DbContext("c"))));
                }
            }

            [Fact]
            public void Returns_true_with_ObjectContext_if_no_context_same_connection()
            {
                var context = MockHelper.CreateMockObjectContext<object>();
                using (var handler = CreateCommitFailureHandlerMock().Object)
                {
                    handler.Initialize(context);

                    Assert.True(
                        handler.MatchesParentContext(
                            ((EntityConnection)context.Connection).StoreConnection,
                            new DbInterceptionContext()));
                }
            }
        }

        public class PruneTransactionHistory : TestBase
        {
            [Fact]
            public void Delegates_to_protected_method()
            {
                var handlerMock = new Mock<CommitFailureHandler> { CallBase = true };
                handlerMock.Protected().Setup("PruneTransactionHistory", ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>()).Callback(() => { });
                using (var handler = handlerMock.Object)
                {
                    handler.PruneTransactionHistory();
                    handlerMock.Protected().Verify("PruneTransactionHistory", Times.Once(), true, true);
                }
            }
        }

#if !NET40
        public class PruneTransactionHistoryAsync : TestBase
        {
            [Fact]
            public void Delegates_to_protected_method()
            {
                var handlerMock = new Mock<CommitFailureHandler> { CallBase = true };
                handlerMock.Protected().Setup<Task>("PruneTransactionHistoryAsync", ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<CancellationToken>())
                    .Returns(() => Task.FromResult(true));
                using (var handler = handlerMock.Object)
                {
                    handler.PruneTransactionHistoryAsync().Wait();
                    handlerMock.Protected().Verify<Task>("PruneTransactionHistoryAsync", Times.Once(), true, true, CancellationToken.None);
                }
            }

            [Fact]
            public void Delegates_to_protected_method_with_CancelationToken()
            {
                var handlerMock = new Mock<CommitFailureHandler> { CallBase = true };
                handlerMock.Protected().Setup<Task>("PruneTransactionHistoryAsync", ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<CancellationToken>())
                    .Returns(() => Task.FromResult(true));
                using (var handler = handlerMock.Object)
                {
                    var token = new CancellationToken();
                    handler.PruneTransactionHistoryAsync(token).Wait();
                    handlerMock.Protected().Verify<Task>("PruneTransactionHistoryAsync", Times.Once(), true, true, token);
                }
            }
        }
#endif

        public class ClearTransactionHistory : TestBase
        {
            [Fact]
            public void Delegates_to_protected_method()
            {
                var context = MockHelper.CreateMockObjectContext<int>();
                var commitFailureHandlerMock = CreateCommitFailureHandlerMock();
                commitFailureHandlerMock.Object.Initialize(context);
                using (var handler = commitFailureHandlerMock.Object)
                {
                    handler.ClearTransactionHistory();
                    commitFailureHandlerMock.Protected().Verify("PruneTransactionHistory", Times.Once(), true, true);
                }
            }
        }

#if !NET40
        public class ClearTransactionHistoryAsync : TestBase
        {
            [Fact]
            public void Delegates_to_protected_method()
            {
                var context = MockHelper.CreateMockObjectContext<int>();
                var commitFailureHandlerMock = CreateCommitFailureHandlerMock();
                commitFailureHandlerMock.Object.Initialize(context);
                using (var handler = commitFailureHandlerMock.Object)
                {
                    handler.ClearTransactionHistoryAsync().Wait();
                    commitFailureHandlerMock.Protected()
                        .Verify<Task>("PruneTransactionHistoryAsync", Times.Once(), true, true, CancellationToken.None);
                }
            }

            [Fact]
            public void Delegates_to_protected_method_with_CancelationToken()
            {
                var context = MockHelper.CreateMockObjectContext<int>();
                var commitFailureHandlerMock = CreateCommitFailureHandlerMock();
                commitFailureHandlerMock.Object.Initialize(context);
                using (var handler = commitFailureHandlerMock.Object)
                {
                    var token = new CancellationToken();
                    handler.ClearTransactionHistoryAsync(token).Wait();
                    commitFailureHandlerMock.Protected().Verify<Task>("PruneTransactionHistoryAsync", Times.Once(), true, true, token);
                }
            }
        }
#endif

        public class BeganTransaction
        {
            [Fact]
            public void BeganTransaction_does_not_fail_if_exception_thrown_such_that_there_is_no_transaction()
            {
                var context = MockHelper.CreateMockObjectContext<object>();
                var handler = CreateCommitFailureHandlerMock().Object;
                handler.Initialize(context);

                var interceptionContext = new BeginTransactionInterceptionContext().WithObjectContext(context);

                Assert.DoesNotThrow(() => handler.BeganTransaction(new Mock<DbConnection>().Object, interceptionContext));
            }
        }

        private static DbConnection CreateMockConnection()
        {
            var connectionMock = new Mock<DbConnection>();
            connectionMock.Protected()
                .Setup<DbProviderFactory>("DbProviderFactory")
                .Returns(GenericProviderFactory<DbProviderFactory>.Instance);

            return connectionMock.Object;
        }

        private static Mock<CommitFailureHandler> CreateCommitFailureHandlerMock()
        {
            Func<DbConnection, TransactionContext> transactionContextFactory =
                c =>
                {
                    var transactionContextMock = new Mock<TransactionContext>(c) { CallBase = true };
                    var transactionRowSet = new InMemoryDbSet<TransactionRow>();
                    transactionContextMock.Setup(m => m.Transactions).Returns(transactionRowSet);
                    transactionContextMock.Setup(m => m.InternalContext).Returns(Mock.Of<InternalContext>());
                    return transactionContextMock.Object;
                };
            var handlerMock = new Mock<CommitFailureHandler>(transactionContextFactory) { CallBase = true };
            handlerMock.Protected().Setup("PruneTransactionHistory", ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>()).Callback(() => { });
#if !NET40
            handlerMock.Protected()
                .Setup<Task>("PruneTransactionHistoryAsync", ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<CancellationToken>())
                .Returns(() => Task.FromResult(true));
#endif
            return handlerMock;
        }
    }
}