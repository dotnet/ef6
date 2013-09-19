// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Infrastructure;
    using Xunit;

    public class EagerInternalContextTests
    {
        [Fact]
        public void CommandTimeout_is_obtained_from_and_set_in_ObjectContext_if_ObjectContext_exists()
        {
            using (var context = new DummyContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                objectContext.CommandTimeout = 77;

                var internalContext = new EagerInternalContext(context, objectContext, false);

                Assert.Equal(77, internalContext.CommandTimeout);

                internalContext.CommandTimeout = 88;

                Assert.Equal(88, objectContext.CommandTimeout);
            }
        }

        [Fact]
        public void DbContexts_are_associated_with_interception_context_when_InternalContexts_are_constructed()
        {
            using (var context = new DummyContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                // One context already associated due to LazyInternalContext in construction path above
                Assert.Equal(new[] { context }, objectContext.InterceptionContext.DbContexts);

                using (var context2 = new DummyContext())
                {
                    new EagerInternalContext(context2, objectContext, false);

                    // Now both contexts are associated
                    Assert.Contains(context, objectContext.InterceptionContext.DbContexts);
                    Assert.Contains(context2, objectContext.InterceptionContext.DbContexts);
                }
            }
        }

        public class DummyContext : DbContext
        {
            static DummyContext()
            {
                Database.SetInitializer<DummyContext>(null);
            }
        }
    }
}
