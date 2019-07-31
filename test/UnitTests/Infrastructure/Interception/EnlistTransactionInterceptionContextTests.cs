// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Entity.Core.Objects;
    using System.Threading.Tasks;
    using System.Transactions;
    using Xunit;

    public class EnlistTransactionInterceptionContextTests : TestBase
    {
        [Fact]
        public void Constructor_throws_on_null()
        {
            Assert.Equal(
                "copyFrom",
                Assert.Throws<ArgumentNullException>(() => new EnlistTransactionInterceptionContext(null)).ParamName);
        }

        [Fact]
        public void Initially_has_no_state()
        {
            var interceptionContext = new EnlistTransactionInterceptionContext();

            Assert.Empty(interceptionContext.DbContexts);
            Assert.Null(interceptionContext.Exception);
            Assert.False(interceptionContext.IsAsync);
            Assert.False(interceptionContext.IsExecutionSuppressed);
            Assert.Null(interceptionContext.Transaction);
            Assert.Empty(interceptionContext.ObjectContexts);
            Assert.Null(interceptionContext.OriginalException);
            Assert.Equal((TaskStatus)0, interceptionContext.TaskStatus);
        }

        [Fact]
        public void Cloning_the_interception_context_preserves_contextual_information_but_not_mutable_state()
        {
            var objectContext = new ObjectContext();
            var dbContext = DbContextMockHelper.CreateDbContext(objectContext);

            var interceptionContext = new EnlistTransactionInterceptionContext();
            interceptionContext.SuppressExecution();
            interceptionContext.Exception = new Exception("Cheez Whiz");

            var transaction = new CommittableTransaction();
            interceptionContext = interceptionContext
                .WithDbContext(dbContext)
                .WithObjectContext(objectContext)
                .WithTransaction(transaction)
                .AsAsync();

            Assert.Equal(new[] { objectContext }, interceptionContext.ObjectContexts);
            Assert.Equal(new[] { dbContext }, interceptionContext.DbContexts);
            Assert.True(interceptionContext.IsAsync);
            Assert.Same(transaction, interceptionContext.Transaction);

            Assert.Null(interceptionContext.Exception);
            Assert.Null(interceptionContext.OriginalException);
            Assert.False(interceptionContext.IsExecutionSuppressed);
        }

        [Fact]
        public void Association_the_base_with_a_null_ObjectContext_or_DbContext_throws()
        {
            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => new EnlistTransactionInterceptionContext().WithObjectContext(null)).ParamName);

            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => new EnlistTransactionInterceptionContext().WithDbContext(null)).ParamName);
        }
    }
}
