// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Internal;
    using Moq;
    using Xunit;

    public class DbCommandTreeInterceptionContextTests : TestBase
    {
        [Fact]
        public void New_interception_context_has_no_state()
        {
            var interceptionContext = new DbCommandTreeInterceptionContext();

            Assert.Empty(interceptionContext.ObjectContexts);
            Assert.Empty(interceptionContext.DbContexts);
            Assert.Null(interceptionContext.Exception);
            Assert.Equal(null, interceptionContext.Result);
            Assert.False(interceptionContext.IsResultSet);
        }

        [Fact]
        public void Interception_context_can_be_associated_command_specific_state_and_all_state_is_preserved()
        {
            var commandTree = new Mock<DbCommandTree>().Object;
            var objectContext = new ObjectContext();
            var dbContext = CreateDbContext(objectContext);
            var exception = new Exception();

            var interceptionContext = new DbCommandTreeInterceptionContext { Result = commandTree }
                .WithDbContext(dbContext)
                .WithObjectContext(objectContext)
                .WithException(exception);

            Assert.Equal(new[] { objectContext }, interceptionContext.ObjectContexts);
            Assert.Equal(new[] { dbContext }, interceptionContext.DbContexts);
            Assert.Same(exception, interceptionContext.Exception);
            Assert.Same(commandTree, interceptionContext.Result);
            Assert.True(interceptionContext.IsResultSet);
        }

        [Fact]
        public void Result_can_be_mutated()
        {
            var interceptionContext = new DbCommandTreeInterceptionContext();
            Assert.Null(interceptionContext.Result);
            Assert.False(interceptionContext.IsResultSet);

            var commandTree = new Mock<DbCommandTree>().Object;
            interceptionContext.Result = commandTree;
            Assert.True(interceptionContext.IsResultSet);
            Assert.Same(commandTree, interceptionContext.Result);
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
