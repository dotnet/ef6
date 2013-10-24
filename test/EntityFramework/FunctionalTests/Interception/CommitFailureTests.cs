// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Interception
{
    using System.Data.Common;
    using System.Data.Entity.Core;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.TestHelpers;
    using System.Linq;
    using Moq;
    using Xunit;

    public class CommitFailureTests : FunctionalTestBase
    {
        [Fact]
        public void No_TransactionHandler_and_no_ExecutionStrategy_throws_CommitFailedException_on_commit_fail()
        {
            Execute_commit_failure_test(
                c => Assert.Throws<CommitFailedException>(() => c.SaveChanges()),
                expectedBlogs: 1,
                useTransactionHandler: false,
                useExecutionStrategy: false,
                rollbackOnFail: true);
        }

        [Fact]
        public void No_TransactionHandler_and_no_ExecutionStrategy_throws_CommitFailedException_on_false_commit_fail()
        {
            Execute_commit_failure_test(
                c => Assert.Throws<CommitFailedException>(() => c.SaveChanges()),
                expectedBlogs: 2,
                useTransactionHandler: false,
                useExecutionStrategy: false,
                rollbackOnFail: false);
        }

        [Fact]
        public void TransactionHandler_and_no_ExecutionStrategy_rethrows_original_exception_on_commit_fail()
        {
            Execute_commit_failure_test(
                c =>
                {
                    var exception = Assert.Throws<EntityException>(() => c.SaveChanges());
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
                c => c.SaveChanges(),
                expectedBlogs: 2,
                useTransactionHandler: true,
                useExecutionStrategy: false,
                rollbackOnFail: false);
        }

        [Fact]
        public void No_TransactionHandler_and_ExecutionStrategy_throws_CommitFailedException_on_commit_fail()
        {
            Execute_commit_failure_test(
                c => Assert.Throws<CommitFailedException>(() => c.SaveChanges()),
                expectedBlogs: 1,
                useTransactionHandler: false,
                useExecutionStrategy: true,
                rollbackOnFail: true);
        }

        [Fact]
        public void No_TransactionHandler_and_ExecutionStrategy_throws_CommitFailedException_on_false_commit_fail()
        {
            Execute_commit_failure_test(
                c => Assert.Throws<CommitFailedException>(() => c.SaveChanges()),
                expectedBlogs: 2,
                useTransactionHandler: false,
                useExecutionStrategy: true,
                rollbackOnFail: false);
        }

        [Fact]
        public void TransactionHandler_and_ExecutionStrategy_retries_on_commit_fail()
        {
            Execute_commit_failure_test(
                c => c.SaveChanges(),
                expectedBlogs: 2,
                useTransactionHandler: true,
                useExecutionStrategy: true,
                rollbackOnFail: true);
        }

        private static void Execute_commit_failure_test(
            Action<BlogContextCommit> runAndVerify, int expectedBlogs, bool useTransactionHandler, bool useExecutionStrategy,
            bool rollbackOnFail)
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
                    FailingTransactionInterceptor.ShouldFailTimes = 0;
                    context.Database.Delete();
                    Assert.Equal(1, context.Blogs.Count());

                    FailingTransactionInterceptor.ShouldFailTimes = 1;
                    FailingTransactionInterceptor.ShouldRollBack = rollbackOnFail;

                    context.Blogs.Add(new BlogContext.Blog());
                    runAndVerify(context);
                }

                using (var context = new BlogContextCommit())
                {
                    Assert.Equal(expectedBlogs, context.Blogs.Count());
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
            TransactionHandler_and_ExecutionStrategy_does_not_retry_on_false_commit_fail_with_custom_implementation(
                context => context.SaveChanges());
        }

#if !NET40
        [Fact]
        public void TransactionHandler_and_ExecutionStrategy_does_not_retry_on_false_commit_fail_async()
        {
            TransactionHandler_and_ExecutionStrategy_does_not_retry_on_false_commit_fail_with_custom_implementation(
                context => context.SaveChangesAsync().Wait());
        }
#endif

        [Fact]
        public void TransactionHandler_and_ExecutionStrategy_does_not_retry_on_false_commit_fail_with_custom_TransactionContext()
        {
            MutableResolver.AddResolver<Func<DbConnection, TransactionContext>>(
                new TransactionContextResolver(c => new MyTransactionContext(c), null, null));

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

            MutableResolver.AddResolver<Func<TransactionHandler>>(
                new TransactionHandlerResolver(() => new CommitFailureHandler(), null, null));

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
                        m => m.Committing(It.IsAny<DbTransaction>(), It.IsAny<DbTransactionInterceptionContext>()), Times.Exactly(4));
                }

                using (var context = new BlogContextCommit())
                {
                    Assert.Equal(2, context.Blogs.Count());
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

                MutableResolver.AddResolver<Func<DbConnection, TransactionContext>>(
                    new TransactionContextResolver(c => new TransactionContextNoInit(c), null, null));

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
