// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Entity.Core.Objects;
    using System.Threading.Tasks;
    using Xunit;

    public class DbConnectionPropertyInterceptionContextTests : TestBase
    {
        [Fact]
        public void Constructor_throws_on_null()
        {
            Assert.Equal(
                "copyFrom",
                Assert.Throws<ArgumentNullException>(() => new DbConnectionPropertyInterceptionContext<int>(null)).ParamName);
        }

        [Fact]
        public void Initially_has_no_state()
        {
            var interceptionContext = new DbConnectionPropertyInterceptionContext<int>();

            Assert.Empty(interceptionContext.DbContexts);
            Assert.Null(interceptionContext.Exception);
            Assert.False(interceptionContext.IsAsync);
            Assert.False(interceptionContext.IsExecutionSuppressed);
            Assert.Equal(0, interceptionContext.Value);
            Assert.Empty(interceptionContext.ObjectContexts);
            Assert.Null(interceptionContext.OriginalException);
            Assert.Equal((TaskStatus)0, interceptionContext.TaskStatus);
            Assert.Null(interceptionContext.UserState);
        }

        [Fact]
        public void Cloning_the_interception_context_preserves_contextual_information_but_not_mutable_state()
        {
            var objectContext = new ObjectContext();
            var dbContext = DbContextMockHelper.CreateDbContext(objectContext);

            var interceptionContext = new DbConnectionPropertyInterceptionContext<int>();
            interceptionContext.SuppressExecution();
            interceptionContext.Exception = new Exception("Cheez Whiz");
            interceptionContext.UserState = "Tilsit";

            interceptionContext = interceptionContext
                .WithDbContext(dbContext)
                .WithObjectContext(objectContext)
                .WithValue(23)
                .AsAsync();

            Assert.Equal(new[] { objectContext }, interceptionContext.ObjectContexts);
            Assert.Equal(new[] { dbContext }, interceptionContext.DbContexts);
            Assert.True(interceptionContext.IsAsync);
            Assert.Equal(23, interceptionContext.Value);

            Assert.Null(interceptionContext.Exception);
            Assert.Null(interceptionContext.OriginalException);
            Assert.False(interceptionContext.IsExecutionSuppressed);
            Assert.Null(interceptionContext.UserState);
        }

        [Fact]
        public void Association_the_base_with_a_null_ObjectContext_or_DbContext_throws()
        {
            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => new DbConnectionPropertyInterceptionContext<int>().WithObjectContext(null))
                    .ParamName);

            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => new DbConnectionPropertyInterceptionContext<int>().WithDbContext(null)).ParamName);
        }
    }
}
