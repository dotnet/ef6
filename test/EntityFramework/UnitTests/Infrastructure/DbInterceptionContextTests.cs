// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Internal;
    using System.Linq;
    using Moq;
    using Xunit;

    public class DbInterceptionContextTests : TestBase
    {
        [Fact]
        public void New_interception_context_has_no_state()
        {
            var interceptionContext = new DbInterceptionContext();

            Assert.Empty(interceptionContext.ObjectContexts);
            Assert.Empty(interceptionContext.DbContexts);
        }

        [Fact]
        public void Interception_context_can_be_associated_with_one_or_more_ObjectContexts()
        {
            var objectContext1 = new ObjectContext();
            var interceptionContext1 = new DbInterceptionContext().WithObjectContext(objectContext1);

            Assert.Equal(new[] { objectContext1 }, interceptionContext1.ObjectContexts);

            var objectContext2 = new ObjectContext();
            var interceptionContext2 = interceptionContext1.WithObjectContext(objectContext2);

            Assert.Contains(objectContext1, interceptionContext2.ObjectContexts);
            Assert.Contains(objectContext2, interceptionContext2.ObjectContexts);
            Assert.Empty(interceptionContext2.DbContexts);

            Assert.Equal(new[] { objectContext1 }, interceptionContext1.ObjectContexts);
        }

        [Fact]
        public void Interception_context_can_be_associated_with_one_or_more_DbContexts()
        {
            var context1 = CreateDbContext(new ObjectContext());
            var interceptionContext1 = new DbInterceptionContext().WithDbContext(context1);

            Assert.Equal(new[] { context1 }, interceptionContext1.DbContexts);

            var context2 = CreateDbContext(new ObjectContext());
            var interceptionContext2 = interceptionContext1.WithDbContext(context2);

            Assert.Contains(context1, interceptionContext2.DbContexts);
            Assert.Contains(context2, interceptionContext2.DbContexts);
            Assert.Empty(interceptionContext2.ObjectContexts);

            Assert.Equal(new[] { context1 }, interceptionContext1.DbContexts);
        }

        [Fact]
        public void Association_with_a_null_ObjectContext_or_DbContext_throws()
        {
            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => new DbInterceptionContext().WithObjectContext(null)).ParamName);

            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => new DbInterceptionContext().WithDbContext(null)).ParamName);
        }

        [Fact]
        public void Association_with_a_DbContext_does_not_cause_initialization_of_the_context()
        {
            var mockInternalContext = new Mock<InternalContextForMock>();
            var context = mockInternalContext.Object.Owner;

            Assert.Equal(new[] { context }, new DbInterceptionContext().WithDbContext(context).DbContexts);

            mockInternalContext.Verify(m => m.ObjectContext, Times.Never());
        }

        [Fact]
        public void Multiple_contexts_can_be_combined_together()
        {
            var objectContext1 = new ObjectContext();
            var context1 = CreateDbContext(objectContext1);
            var objectContext2 = new ObjectContext();
            var context2 = CreateDbContext(objectContext2);

            var interceptionContext1 = new DbInterceptionContext()
                .WithDbContext(context1)
                .WithDbContext(context2)
                .WithObjectContext(objectContext1);

            var interceptionContext2 = interceptionContext1
                .WithDbContext(context2)
                .WithObjectContext(objectContext1)
                .WithObjectContext(objectContext2);

            var combined = DbInterceptionContext.Combine(new[] { interceptionContext1, interceptionContext2 });

            Assert.Equal(2, combined.DbContexts.Count());
            Assert.Equal(2, combined.ObjectContexts.Count());

            Assert.Contains(context1, combined.DbContexts);
            Assert.Contains(context2, combined.DbContexts);
            Assert.Contains(objectContext1, combined.ObjectContexts);
            Assert.Contains(objectContext2, combined.ObjectContexts);
        }

        [Fact]
        public void Combine_throws_for_null_arg()
        {
            Assert.Equal(
                "interceptionContexts",
                Assert.Throws<ArgumentNullException>(() => DbInterceptionContext.Combine(null)).ParamName);
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
