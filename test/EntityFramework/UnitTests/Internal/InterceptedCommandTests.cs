// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Common;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class InterceptedCommandTests : TestBase
    {
        [Fact]
        public void Ctor_should_clone_command_state()
        {
            var mockCommand = new Mock<DbCommand>(MockBehavior.Strict);
            mockCommand.SetupGet(c => c.CommandText).Returns("Foo");
            mockCommand.Protected()
                .SetupGet<DbParameterCollection>("DbParameterCollection")
                .Returns(
                    new Mock<DbParameterCollection>
                        {
                            DefaultValue = DefaultValue.Mock
                        }.Object);

            var interceptedCommand = new InterceptedCommand(mockCommand.Object);

            Assert.Equal("Foo", interceptedCommand.CommandText);
            Assert.Empty(interceptedCommand.Parameters);

            mockCommand.Verify();
        }
    }
}
