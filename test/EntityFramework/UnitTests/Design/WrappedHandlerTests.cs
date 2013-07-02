// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Design
{
    using Moq;
    using Xunit;

    public class WrappedHandlerTests
    {
        [Fact]
        public void SetResult_invokes_when_implemented()
        {
            var handler = new Mock<HandlerBase>().As<IResultHandler>();
            var wrappedHandler = new WrappedHandler(handler.Object);

            wrappedHandler.SetResult("Value1");

            handler.Verify(h => h.SetResult("Value1"));
        }

        [Fact]
        public void SetResult_is_noop_when_not_implemented()
        {
            var handler = new Mock<HandlerBase>();
            var wrappedHandler = new WrappedHandler(handler.Object);

            wrappedHandler.SetResult("Value1");
        }
    }
}
