// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Design
{
    using Xunit;

    public class HandlerBaseTests
    {
        private interface ICustomHandler
        {
        }

        private class MyHandler : HandlerBase, IResultHandler
        {
            public void SetResult(object value)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void ImplementsContract_returns_true_when_implemented()
        {
            var handler = new MyHandler();

            Assert.True(handler.ImplementsContract(typeof(IResultHandler).FullName));
        }

        [Fact]
        public void ImplementsContract_returns_false_when_not_implemented()
        {
            var handler = new MyHandler();

            Assert.False(handler.ImplementsContract(typeof(ICustomHandler).FullName));
        }

        [Fact]
        public void ImplementsContract_returns_false_when_unknown()
        {
            var handler = new MyHandler();

            Assert.False(handler.ImplementsContract("My.Fake.Contract"));
        }
    }
}
