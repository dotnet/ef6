// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Common;
    using System.Data.Entity.Core.Objects;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;

    public class BeginTransactionInterceptionContextTests
    {
        [Fact]
        public void Constructor_throws_on_null()
        {
            Assert.Equal(
                "copyFrom",
                Assert.Throws<ArgumentNullException>(() => new BeginTransactionInterceptionContext(null)).ParamName);
        }

        [Fact]
        public void Initially_has_no_state()
        {
            var interceptionContext = new BeginTransactionInterceptionContext();

            Assert.Empty(interceptionContext.DbContexts);
            Assert.Null(interceptionContext.Exception);
            Assert.False(interceptionContext.IsAsync);
            Assert.False(interceptionContext.IsExecutionSuppressed);
            Assert.Equal(IsolationLevel.Unspecified, interceptionContext.IsolationLevel);
            Assert.Empty(interceptionContext.ObjectContexts);
            Assert.Null(interceptionContext.OriginalException);
            Assert.Null(interceptionContext.OriginalResult);
            Assert.Null(interceptionContext.Result);
            Assert.Equal((TaskStatus)0, interceptionContext.TaskStatus);
        }

        [Fact]
        public void Cloning_the_interception_context_preserves_contextual_information_but_not_mutable_state()
        {
            var objectContext = new ObjectContext();
            var dbContext = DbContextMockHelper.CreateDbContext(objectContext);

            var interceptionContext = new BeginTransactionInterceptionContext();

            var mutableData = ((IDbMutableInterceptionContext<DbTransaction>)interceptionContext).MutableData;
            var transaction = new Mock<DbTransaction>().Object;
            mutableData.SetExecuted(transaction);
            mutableData.SetExceptionThrown(new Exception("Cheez Whiz"));

            interceptionContext = interceptionContext
                .WithDbContext(dbContext)
                .WithObjectContext(objectContext)
                .WithIsolationLevel(IsolationLevel.RepeatableRead)
                .AsAsync();

            interceptionContext = new BeginTransactionInterceptionContext(interceptionContext);

            Assert.Equal(new[] { objectContext }, interceptionContext.ObjectContexts);
            Assert.Equal(new[] { dbContext }, interceptionContext.DbContexts);
            Assert.True(interceptionContext.IsAsync);
            Assert.Equal(IsolationLevel.RepeatableRead, interceptionContext.IsolationLevel);

            Assert.Null(interceptionContext.Result);
            Assert.Null(interceptionContext.OriginalResult);
            Assert.Null(interceptionContext.Exception);
            Assert.Null(interceptionContext.OriginalException);
            Assert.False(interceptionContext.IsExecutionSuppressed);
        }

        [Fact]
        public void Association_the_base_with_a_null_ObjectContext_or_DbContext_throws()
        {
            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => new BeginTransactionInterceptionContext().WithObjectContext(null)).ParamName);

            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => new BeginTransactionInterceptionContext().WithDbContext(null)).ParamName);
        }
    }
}
