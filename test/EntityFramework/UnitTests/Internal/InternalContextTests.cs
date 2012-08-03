// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
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
    }
}
