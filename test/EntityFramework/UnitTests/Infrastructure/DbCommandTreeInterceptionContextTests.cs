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
            Assert.Equal(null, interceptionContext.Result);
            Assert.Equal(null, interceptionContext.OriginalResult);
        }

        [Fact]
        public void Cloning_the_interception_context_preserves_contextual_information_but_not_mutable_state()
        {
            var objectContext = new ObjectContext();
            var dbContext = CreateDbContext(objectContext);

            var interceptionContext = new DbCommandTreeInterceptionContext();
            
            interceptionContext.MutableData.SetExecuted(new Mock<DbCommandTree>().Object);

            interceptionContext = interceptionContext
                .WithDbContext(dbContext)
                .WithObjectContext(objectContext)
                .AsAsync();

            Assert.Equal(new[] { objectContext }, interceptionContext.ObjectContexts);
            Assert.Equal(new[] { dbContext }, interceptionContext.DbContexts);
            Assert.True(interceptionContext.IsAsync);

            Assert.Null(interceptionContext.Result);
            Assert.Null(interceptionContext.OriginalResult);
        }

        [Fact]
        public void Result_can_be_mutated()
        {
            var interceptionContext = new DbCommandTreeInterceptionContext();
            Assert.Null(interceptionContext.Result);
            Assert.Null(interceptionContext.OriginalResult);

            var commandTree = new Mock<DbCommandTree>().Object;
            interceptionContext.MutableData.SetExecuted(commandTree);

            Assert.Same(commandTree, interceptionContext.Result);
            Assert.Same(commandTree, interceptionContext.OriginalResult);
          
            var commandTree2 = new Mock<DbCommandTree>().Object;
            interceptionContext.Result = commandTree2;

            Assert.Same(commandTree2, interceptionContext.Result);
            Assert.Same(commandTree, interceptionContext.OriginalResult);
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
