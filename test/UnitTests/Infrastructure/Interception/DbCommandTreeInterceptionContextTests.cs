// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Internal;
    using Moq;
    using Xunit;
    using Xunit.Extensions;

    public class DbCommandTreeInterceptionContextTests : TestBase
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void New_interception_context_has_no_state(bool useObsoleteState)
        {
            var interceptionContext = new DbCommandTreeInterceptionContext();

            Assert.Empty(interceptionContext.ObjectContexts);
            Assert.Empty(interceptionContext.DbContexts);
            Assert.Equal(null, interceptionContext.Result);
            Assert.Equal(null, interceptionContext.OriginalResult);
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
            var dbContext = CreateDbContext(objectContext);

            var interceptionContext = new DbCommandTreeInterceptionContext();
            
            interceptionContext.MutableData.SetExecuted(new Mock<DbCommandTree>().Object);
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
                .AsAsync();

            Assert.Equal(new[] { objectContext }, interceptionContext.ObjectContexts);
            Assert.Equal(new[] { dbContext }, interceptionContext.DbContexts);
            Assert.True(interceptionContext.IsAsync);

            Assert.Null(interceptionContext.Result);
            Assert.Null(interceptionContext.OriginalResult);
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
        public void Interception_context_constructor_throws_on_null()
        {
            Assert.Equal(
                "copyFrom",
                Assert.Throws<ArgumentNullException>(() => new DbCommandTreeInterceptionContext(null)).ParamName);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Result_can_be_mutated(bool useObsoleteState)
        {
            var interceptionContext = new DbCommandTreeInterceptionContext();
            Assert.Null(interceptionContext.Result);
            Assert.Null(interceptionContext.OriginalResult);
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

            var commandTree = new Mock<DbCommandTree>().Object;
            interceptionContext.MutableData.SetExecuted(commandTree);
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

            Assert.Same(commandTree, interceptionContext.Result);
            Assert.Same(commandTree, interceptionContext.OriginalResult);
            if (useObsoleteState)
            {
#pragma warning disable 618
                Assert.Equal("Cheddar", interceptionContext.UserState);
#pragma warning restore 618
            }
            else
            {
                Assert.Equal("AState", interceptionContext.FindUserState("A"));
                Assert.Equal("BState", interceptionContext.FindUserState("B"));
                Assert.Null(interceptionContext.FindUserState("C"));
            }

            var commandTree2 = new Mock<DbCommandTree>().Object;
            interceptionContext.Result = commandTree2;

            Assert.Same(commandTree2, interceptionContext.Result);
            Assert.Same(commandTree, interceptionContext.OriginalResult);
            if (useObsoleteState)
            {
#pragma warning disable 618
                Assert.Equal("Cheddar", interceptionContext.UserState);
#pragma warning restore 618
            }
            else
            {
                Assert.Equal("AState", interceptionContext.FindUserState("A"));
                Assert.Equal("BState", interceptionContext.FindUserState("B"));
                Assert.Null(interceptionContext.FindUserState("C"));
            }
        }

        [Fact]
        public void Association_with_a_null_ObjectContext_or_DbContext_throws()
        {
            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => new DbCommandTreeInterceptionContext().WithObjectContext(null)).ParamName);

            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => new DbCommandTreeInterceptionContext().WithDbContext(null)).ParamName);
        }

        private static DbContext CreateDbContext(ObjectContext objectContext)
        {
            var mockInternalContext = new Mock<InternalContextForMock>();
            mockInternalContext.Setup(m => m.ObjectContext).Returns(objectContext);
            var context = mockInternalContext.Object.Owner;
            objectContext.InterceptionContext = objectContext.InterceptionContext.WithDbContext(context);
            return context;
        }
    }
}
