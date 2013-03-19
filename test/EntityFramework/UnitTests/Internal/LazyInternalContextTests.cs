// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Core.Objects;
    using Moq;
    using Xunit;

    public class LazyInternalContextTests
    {
        [Fact]
        public void CommandTimeout_is_obtained_from_and_set_in_ObjectContext_if_ObjectContext_exists()
        {
            var mockContext = new Mock<LazyInternalContext>(
                new Mock<DbContext>().Object, new Mock<IInternalConnection>().Object, null, null, null, null)
                {
                    CallBase = true
                };
            var objectContext = new ObjectContext
                {
                    CommandTimeout = 77
                };
            mockContext.Setup(m => m.ObjectContextInUse).Returns(objectContext);

            Assert.Equal(77, mockContext.Object.CommandTimeout);

            mockContext.Object.CommandTimeout = 88;

            Assert.Equal(88, objectContext.CommandTimeout);
        }

        [Fact]
        public void CommandTimeout_is_obtained_from_and_set_in_field_if_ObjectContext_does_not_exist()
        {
            var mockContext = new Mock<LazyInternalContext>(
                new Mock<DbContext>().Object, new Mock<IInternalConnection>().Object, null, null, null, null)
                {
                    CallBase = true
                };
            mockContext.Setup(m => m.ObjectContextInUse).Returns((ObjectContext)null);

            Assert.Null(mockContext.Object.CommandTimeout);

            mockContext.Object.CommandTimeout = 88;

            Assert.Equal(88, mockContext.Object.CommandTimeout);
        }
    }
}
