// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Interception
{
    using System.Data.Common;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.TestHelpers;
    using System.Data.SqlClient;
    using System.Linq;
    using Moq;
    using Xunit;

    public class CommitFailureTests : FunctionalTestBase
    {
        [Fact]
        public void No_TransactionHandler_and_no_ExecutionStrategy_throws_CommitFailedException_on_commit_fail()
        {
            Execute_commit_failure_test(
                c => Assert.Throws<DataException>(() => c()).InnerException.ValidateMessage("CommitFailed"),
                c => Assert.Throws<CommitFailedException>(() => c()).ValidateMessage("CommitFailed"),
                expectedBlogs: 1,
                useTransactionHandler: false,
                useExecutionStrategy: false,
                rollbackOnFail: true);
        }

        [Fact]
        public void No_TransactionHandler_and_no_ExecutionStrategy_throws_CommitFailedException_on_false_commit_fail()
        {
            Execute_commit_failure_test(
                c => Assert.Throws<DataException>(() => c()).InnerException.ValidateMessage("CommitFailed"),
                c => Assert.Throws<CommitFailedException>(() => c()).ValidateMessage("CommitFailed"),
                expectedBlogs: 2,
                useTransactionHandler: false,
                useExecutionStrategy: false,
                rollbackOnFail: false);
        }

        [Fact]
        public void TransactionHandler_and_no_ExecutionStrategy_rethrows_original_exception_on_commit_fail()
        {
            Execute_commit_failure_test(
                c => Assert.Throws<TimeoutException>(() => c()),
                c =>
                {
                    var exception = Assert.Throws<EntityException>(() => c());
                    Assert.IsType<TimeoutException>(exception.InnerException);
                },
                expectedBlogs: 1,
                useTransactionHandler: true,
                useExecutionStrategy: false,
                rollbackOnFail: true);
        }

        [Fact]
        public void TransactionHandler_and_no_ExecutionStrategy_does_not_throw_on_false_commit_fail()
        {
            Execute_commit_failure_test(
                c => c(),
                c => c(),
                expectedBlogs: 2,
                useTransactionHandler: true,
                useExecutionStrategy: false,
                rollbackOnFail: false);
        }

        [Fact]
        public void No_TransactionHandler_and_ExecutionStrategy_throws_CommitFailedException_on_commit_fail()
        {
            Execute_commit_failure_test(
                c => Assert.Throws<DataException>(() => c()).InnerException.ValidateMessage("CommitFailed"),
                c => Assert.Throws<CommitFailedException>(() => c()).ValidateMessage("CommitFailed"),
                expectedBlogs: 1,
                useTransactionHandler: false,
                useExecutionStrategy: true,
                rollbackOnFail: true);
        }

        [Fact]
        public void No_TransactionHandler_and_ExecutionStrategy_throws_CommitFailedException_on_false_commit_fail()
        {
            Execute_commit_failure_test(
                c => Assert.Throws<DataException>(() => c()).InnerException.ValidateMessage("CommitFailed"),
                c => Assert.Throws<CommitFailedException>(() => c()).ValidateMessage("CommitFailed"),
                expectedBlogs: 2,
                useTransactionHandler: false,
                useExecutionStrategy: true,
                rollbackOnFail: false);
        }

        [Fact]
        public void TransactionHandler_and_ExecutionStrategy_retries_on_commit_fail()
        {
            Execute_commit_failure_test(
                c => c(),
                c => c(),
                expectedBlogs: 2,
                useTransactionHandler: true,
                useExecutionStrategy: true,
                rollbackOnFail: true);
        }

        private void Execute_commit_failure_test(Action<Action> verifyInitialization, Action<Action> verifySaveChanges, int expectedBlogs, bool useTransactionHandler, bool useExecutionStrategy, bool rollbackOnFail)
        {
            var failingTransactionInterceptor = new FailingTransactionInterceptor();
            DbInterception.Add(failingTransactionInterceptor);

            if (useTransactionHandler)
            {
                MutableResolver.AddResolver<Func<TransactionHandler>>(
                    new TransactionHandlerResolver(() => new CommitFailureHandler(), null, null));
            }

            if (useExecutionStrategy)
            {
                MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(
                    key => (Func<IDbExecutionStrategy>)(() => new SqlAzureExecutionStrategy()));
            }

            try
            {
                using (var context = new BlogContextCommit())
                {
                    context.Database.Delete();
                    FailingTransactionInterceptor.ShouldFailTimes = 1;
                    FailingTransactionInterceptor.ShouldRollBack = rollbackOnFail;
                    verifyInitialization(() => context.Blogs.Count());

                    FailingTransactionInterceptor.ShouldFailTimes = 0;
                    Assert.Equal(1, context.Blogs.Count());

                    FailingTransactionInterceptor.ShouldFailTimes = 1;

                    context.Blogs.Add(new BlogContext.Blog());
                    verifySaveChanges(() => context.SaveChanges());
                }

                using (var context = new BlogContextCommit())
                {
                    Assert.Equal(expectedBlogs, context.Blogs.Count());

                    using (var transactionContext = new TransactionContext(context.Database.Connection))
                    {
                        using (var infoContext = GetInfoContext(transactionContext))
                        {
                            Assert.True(!infoContext.TableExists("__Transactions")
                                || !transactionContext.Transactions.Any());
                        }
                    }
                }

            }
            finally
            {
                DbInterception.Remove(failingTransactionInterceptor);
                MutableResolver.ClearResolvers();
            }

            DbDispatchersHelpers.AssertNoInterceptors();
        }

        [Fact]
        public void TransactionHandler_and_ExecutionStrategy_does_not_retry_on_false_commit_fail()
        {
            MutableResolver.AddResolver<Func<TransactionHandler>>(
                new TransactionHandlerResolver(() => new CommitFailureHandler(), null, null));

            TransactionHandler_and_ExecutionStrategy_does_not_retry_on_false_commit_fail_with_custom_implementation(
                context => context.SaveChanges());
        }

#if !NET40
        [Fact]
        public void TransactionHandler_and_ExecutionStrategy_does_not_retry_on_false_commit_fail_async()
        {
            MutableResolver.AddResolver<Func<TransactionHandler>>(
                new TransactionHandlerResolver(() => new CommitFailureHandler(), null, null));

            TransactionHandler_and_ExecutionStrategy_does_not_retry_on_false_commit_fail_with_custom_implementation(
                context => context.SaveChangesAsync().Wait());
        }
#endif

        [Fact]
        public void TransactionHandler_and_ExecutionStrategy_does_not_retry_on_false_commit_fail_with_custom_TransactionContext()
        {
            MutableResolver.AddResolver<Func<TransactionHandler>>(
                new TransactionHandlerResolver(() => new CommitFailureHandler(c => new MyTransactionContext(c)), null, null));

            TransactionHandler_and_ExecutionStrategy_does_not_retry_on_false_commit_fail_with_custom_implementation(
                context =>
                {
                    context.SaveChanges();

                    using (var infoContext = GetInfoContext(context))
                    {
                        Assert.True(infoContext.TableExists("MyTransactions"));

                        var column = infoContext.Columns.Single(c => c.Name == "Time");
                        Assert.Equal("datetime2", column.Type);
                    }
                });
        }

        public class MyTransactionContext : TransactionContext
        {
            public MyTransactionContext(DbConnection connection)
                : base(connection)
            {
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<TransactionRow>()
                    .ToTable("MyTransactions")
                    .HasKey(e => e.Id)
                    .Property(e => e.CreationTime).HasColumnName("Time").HasColumnType("datetime2");
            }
        }

        private void TransactionHandler_and_ExecutionStrategy_does_not_retry_on_false_commit_fail_with_custom_implementation(
            Action<BlogContextCommit> runAndVerify)
        {
            var failingTransactionInterceptorMock = new Mock<FailingTransactionInterceptor> { CallBase = true };
            DbInterception.Add(failingTransactionInterceptorMock.Object);

            MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(
                key => (Func<IDbExecutionStrategy>)(() => new SqlAzureExecutionStrategy()));

            try
            {
                using (var context = new BlogContextCommit())
                {
                    FailingTransactionInterceptor.ShouldFailTimes = 0;
                    context.Database.Delete();
                    Assert.Equal(1, context.Blogs.Count());

                    FailingTransactionInterceptor.ShouldFailTimes = 20;
                    FailingTransactionInterceptor.ShouldRollBack = false;

                    context.Blogs.Add(new BlogContext.Blog());

                    runAndVerify(context);

                    FailingTransactionInterceptor.ShouldFailTimes = 0;
                    failingTransactionInterceptorMock.Verify(
                        m => m.Committing(It.IsAny<DbTransaction>(), It.IsAny<DbTransactionInterceptionContext>()), Times.Exactly(6));
                }

                using (var context = new BlogContextCommit())
                {
                    Assert.Equal(2, context.Blogs.Count());

                    using (var transactionContext = new TransactionContext(context.Database.Connection))
                    {
                        using (var infoContext = GetInfoContext(transactionContext))
                        {
                            Assert.True(!infoContext.TableExists("__Transactions")
                                || !transactionContext.Transactions.Any());
                        }
                    }
                }
            }
            finally
            {
                DbInterception.Remove(failingTransactionInterceptorMock.Object);
                MutableResolver.ClearResolvers();
            }

            DbDispatchersHelpers.AssertNoInterceptors();
        }

        [Fact]
        public void CommitFailureHandler_supports_nested_transactions()
        {
            MutableResolver.AddResolver<Func<TransactionHandler>>(
                new TransactionHandlerResolver(() => new CommitFailureHandler(), null, null));

            try
            {
                using (var context = new BlogContextCommit())
                {
                    context.Database.Delete();
                    Assert.Equal(1, context.Blogs.Count());

                    context.Blogs.Add(new BlogContext.Blog());
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        using (var innerContext = new BlogContextCommit())
                        {
                            using (var innerTransaction = innerContext.Database.BeginTransaction())
                            {
                                Assert.Equal(1, innerContext.Blogs.Count());
                                innerContext.Blogs.Add(new BlogContext.Blog());
                                innerContext.SaveChanges();
                                innerTransaction.Commit();
                            }
                        }

                        context.SaveChanges();
                        transaction.Commit();
                    }
                }

                using (var context = new BlogContextCommit())
                {
                    Assert.Equal(3, context.Blogs.Count());
                }
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }

            DbDispatchersHelpers.AssertNoInterceptors();
        }

        [Fact]
        public void BuildDatabaseInitializationScript_can_be_used_to_initialize_the_database()
        {
            MutableResolver.AddResolver<Func<TransactionHandler>>(
                new TransactionHandlerResolver(() => new CommitFailureHandler(), null, null));

            MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(
                key => (Func<IDbExecutionStrategy>)(() => new SqlAzureExecutionStrategy()));

            try
            {
                using (var context = new BlogContextCommit())
                {
                    context.Database.Delete();
                    Assert.Equal(1, context.Blogs.Count());
                }

                MutableResolver.AddResolver<Func<TransactionHandler>>(
                    new TransactionHandlerResolver(() => new CommitFailureHandler(c => new TransactionContextNoInit(c)), null, null));

                using (var context = new BlogContextCommit())
                {
                    context.Blogs.Add(new BlogContext.Blog());

                    Assert.Throws<EntityException>(() => context.SaveChanges());

                    context.Database.ExecuteSqlCommand(
                        TransactionalBehavior.DoNotEnsureTransaction,
                        ((IObjectContextAdapter)context).ObjectContext.TransactionHandler.BuildDatabaseInitializationScript());

                    context.SaveChanges();
                }

                using (var context = new BlogContextCommit())
                {
                    Assert.Equal(2, context.Blogs.Count());
                }
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }

            DbDispatchersHelpers.AssertNoInterceptors();
        }

        [Fact]
        public void BuildDatabaseInitializationScript_can_be_used_to_initialize_the_database_if_no_migration_generator()
        {
            var providerInvariantNameMock = new Mock<IProviderInvariantName>();
            providerInvariantNameMock.Setup(m => m.Name).Returns("Foo");

            var providerInvariantNameResolverMock = new Mock<IDbDependencyResolver>();
            providerInvariantNameResolverMock.Setup(m => m.GetService(It.IsAny<Type>(), It.IsAny<object>()))
                .Returns(providerInvariantNameMock.Object);

            MutableResolver.AddResolver<IProviderInvariantName>(providerInvariantNameResolverMock.Object);

            var mockDbProviderServiceResolver = new Mock<IDbDependencyResolver>();
            mockDbProviderServiceResolver
                .Setup(r => r.GetService(It.IsAny<Type>(), It.IsAny<string>()))
                .Returns(SqlProviderServices.Instance);

            MutableResolver.AddResolver<DbProviderServices>(mockDbProviderServiceResolver.Object);

            var mockDbProviderFactoryResolver = new Mock<IDbDependencyResolver>();
            mockDbProviderFactoryResolver
                .Setup(r => r.GetService(It.IsAny<Type>(), It.IsAny<string>()))
                .Returns(SqlClientFactory.Instance);

            MutableResolver.AddResolver<DbProviderFactory>(mockDbProviderFactoryResolver.Object);

            BuildDatabaseInitializationScript_can_be_used_to_initialize_the_database();
        }

        public void FromContext_returns_the_current_handler()
        {
            MutableResolver.AddResolver<Func<TransactionHandler>>(
                new TransactionHandlerResolver(() => new CommitFailureHandler(), null, null));

            try
            {
                using (var context = new BlogContextCommit())
                {
                    context.Database.Delete();
                    Assert.Null(CommitFailureHandler.FromContext(context));
                    Assert.Null(CommitFailureHandler.FromContext(((IObjectContextAdapter)context).ObjectContext));

                    // This will connect to the database and build the CommitFailureHandler
                    Assert.Equal(1, context.Blogs.Count());

                    var commitFailureHandler = CommitFailureHandler.FromContext(((IObjectContextAdapter)context).ObjectContext);
                    Assert.IsType<CommitFailureHandler>(commitFailureHandler);
                    Assert.Same(commitFailureHandler, CommitFailureHandler.FromContext(context));
                }
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }
        }

        public class TransactionContextNoInit : TransactionContext
        {
            static TransactionContextNoInit()
            {
                Database.SetInitializer<TransactionContextNoInit>(null);
            }

            public TransactionContextNoInit(DbConnection connection)
                : base(connection)
            {
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<TransactionRow>()
                    .ToTable("TransactionContextNoInit");
            }
        }

        public class FailingTransactionInterceptor : IDbTransactionInterceptor
        {
            public static int ShouldFailTimes;
            public static bool ShouldRollBack;

            public void ConnectionGetting(DbTransaction transaction, DbTransactionInterceptionContext<DbConnection> interceptionContext)
            {
            }

            public void ConnectionGot(DbTransaction transaction, DbTransactionInterceptionContext<DbConnection> interceptionContext)
            {
            }

            public void IsolationLevelGetting(
                DbTransaction transaction, DbTransactionInterceptionContext<IsolationLevel> interceptionContext)
            {
            }

            public void IsolationLevelGot(DbTransaction transaction, DbTransactionInterceptionContext<IsolationLevel> interceptionContext)
            {
            }

            public virtual void Committing(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
            {
                if (interceptionContext.DbContexts.Any(c => c is TransactionContext))
                {
                    return;
                }

                if (ShouldFailTimes-- > 0)
                {
                    if (ShouldRollBack)
                    {
                        transaction.Rollback();
                    }
                    else
                    {
                        transaction.Commit();
                    }
                    interceptionContext.Exception = new TimeoutException();
                }
            }

            public void Committed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
            {
            }

            public void Disposing(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
            {
            }

            public void Disposed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
            {
            }

            public void RollingBack(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
            {
            }

            public void RolledBack(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
            {
            }
        }

        public class BlogContextCommit : BlogContext
        {
            static BlogContextCommit()
            {
                Database.SetInitializer<BlogContextCommit>(new BlogInitializer());
            }
        }
    }
}
