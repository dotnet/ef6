// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Internal;
    using Moq;
    using Xunit;

    public class DbCommandBaseInterceptionContextTests : TestBase
    {
        [Fact]
        public void New_interception_context_has_no_state()
        {
            var interceptionContext = new DbCommandBaseInterceptionContext();

            Assert.Empty(interceptionContext.ObjectContexts);
            Assert.Empty(interceptionContext.DbContexts);
            Assert.False(interceptionContext.IsAsync);
            Assert.Equal(CommandBehavior.Default, interceptionContext.CommandBehavior);
        }

        [Fact]
        public void Cloning_the_interception_context_preserves_contextual_information()
        {
            var objectContext = new ObjectContext();
            var dbContext = CreateDbContext(objectContext);

            var interceptionContext = new DbCommandBaseInterceptionContext()
                .WithDbContext(dbContext)
                .WithObjectContext(objectContext)
                .WithCommandBehavior(CommandBehavior.SchemaOnly)
                .AsAsync();

            Assert.Equal(new[] { objectContext }, interceptionContext.ObjectContexts);
            Assert.Equal(new[] { dbContext }, interceptionContext.DbContexts);
            Assert.True(interceptionContext.IsAsync);
            Assert.Equal(CommandBehavior.SchemaOnly, interceptionContext.CommandBehavior);
        }

        [Fact]
        public void Association_with_a_null_ObjectContext_or_DbContext_throws()
        {
            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => new DbCommandBaseInterceptionContext().WithObjectContext(null)).ParamName);

            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => new DbCommandBaseInterceptionContext().WithDbContext(null)).ParamName);
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
