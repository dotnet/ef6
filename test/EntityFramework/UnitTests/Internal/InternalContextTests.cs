// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Migrations.History;
    using DaFunc;
    using Moq;
    using Xunit;

    public class InternalContextTests
    {
        [Fact]
        public void OnDisposing_event_is_raised_when_once_when_context_is_disposed_and_never_again()
        {
            var eventCount = 0;
            var context = new EagerInternalContext(new Mock<DbContext>().Object);

            context.OnDisposing += (_, __) => eventCount++;

            context.Dispose();
            Assert.Equal(1, eventCount);

            context.Dispose();
            Assert.Equal(1, eventCount);
        }

        [Fact]
        public void ContextKey_returns_to_string_of_context_type()
        {
            var genericFuncy = new GT<NT, NT>.GenericFuncy<GT<GT<NT, NT>, NT>, NT>();

            var internalContext = new EagerInternalContext(genericFuncy);

            Assert.Equal(genericFuncy.GetType().ToString(), internalContext.ContextKey);
        }

        private class LongTypeNameInternalContext : EagerInternalContext
        {
            public LongTypeNameInternalContext(DbContext owner)
                : base(owner)
            {
            }

            internal override string OwnerShortTypeName
            {
                get { return new string('a', 600); }
            }
        }

        [Fact]
        public void ContextKey_restricts_value_to_max_length()
        {
            var internalContext = new LongTypeNameInternalContext(new Mock<DbContext>().Object);

            Assert.Equal(new string('a', HistoryContext.ContextKeyMaxLength), internalContext.ContextKey);
        }
    }
}
