// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Entity.Core.Objects;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Extensions;

    public class DbConnectionPropertyInterceptionContextTests : TestBase
    {
        [Fact]
        public void Constructor_throws_on_null()
        {
            Assert.Equal(
                "copyFrom",
                Assert.Throws<ArgumentNullException>(() => new DbConnectionPropertyInterceptionContext<int>(null)).ParamName);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Initially_has_no_state(bool useObsoleteState)
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
            if (useObsoleteState)
            {
#pragma warning disable 618
                Assert.Null(interceptionContext.UserState);
#pragma warning restore 618
            }
            else
            {
                Assert.Null(interceptionContext.FindUserState("A"));
                Assert.Null(interceptionContext.FindUserState("B"));
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Cloning_the_interception_context_preserves_contextual_information_but_not_mutable_state(bool useObsoleteState)
        {
            var objectContext = new ObjectContext();
            var dbContext = DbContextMockHelper.CreateDbContext(objectContext);

            var interceptionContext = new DbConnectionPropertyInterceptionContext<int>();
            interceptionContext.SuppressExecution();
            interceptionContext.Exception = new Exception("Cheez Whiz");
            if (useObsoleteState)
            {
#pragma warning disable 618
                interceptionContext.UserState = "Cheddar";
#pragma warning restore 618
            }
            else
            {
                interceptionContext.SetUserState("A", "AState");
                interceptionContext.SetUserState("B", "BState");
            }

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
            if (useObsoleteState)
            {
#pragma warning disable 618
                Assert.Null(interceptionContext.UserState);
#pragma warning restore 618
            }
            else
            {
                Assert.Null(interceptionContext.FindUserState("A"));
                Assert.Null(interceptionContext.FindUserState("B"));
            }
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
