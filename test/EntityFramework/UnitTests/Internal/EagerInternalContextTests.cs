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

        public class DummyContext : DbContext
        {
            static DummyContext()
            {
                Database.SetInitializer<DummyContext>(null);
            }
        }
    }
}
