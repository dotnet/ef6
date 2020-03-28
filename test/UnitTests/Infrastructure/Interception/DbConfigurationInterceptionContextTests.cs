// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Internal;
    using Moq;
    using Xunit;

    public class DbConfigurationInterceptionContextTests : TestBase
    {
        [Fact]
        public void New_interception_context_has_no_state()
        {
            var interceptionContext = new DbConfigurationInterceptionContext();

            Assert.Empty(interceptionContext.ObjectContexts);
            Assert.Empty(interceptionContext.DbContexts);
        }

        [Fact]
        public void Cloning_the_interception_context_preserves_contextual_information_but_not_mutable_state()
        {
            var objectContext = new ObjectContext();
            var dbContext = CreateDbContext(objectContext);

            var interceptionContext = new DbConfigurationInterceptionContext();

            interceptionContext = interceptionContext
                .WithDbContext(dbContext)
                .WithObjectContext(objectContext)
                .AsAsync();

            Assert.Equal(new[] { objectContext }, interceptionContext.ObjectContexts);
            Assert.Equal(new[] { dbContext }, interceptionContext.DbContexts);
            Assert.True(interceptionContext.IsAsync);
        }

        [Fact]
        public void Interception_context_constructor_throws_on_null()
        {
            Assert.Equal(
                "copyFrom",
                Assert.Throws<ArgumentNullException>(() => new DbConfigurationInterceptionContext(null)).ParamName);
        }

        [Fact]
        public void Association_with_a_null_ObjectContext_or_DbContext_throws()
        {
            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => new DbConfigurationInterceptionContext().WithObjectContext(null)).ParamName);

            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => new DbConfigurationInterceptionContext().WithDbContext(null)).ParamName);
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
