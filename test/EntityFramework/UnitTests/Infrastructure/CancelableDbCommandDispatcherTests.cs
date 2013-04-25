// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using Moq;
    using Xunit;

    public class CancelableDbCommandDispatcherTests : TestBase
    {
        [Fact]
        public void Executing_dispatches_to_interceptors()
        {
            var interceptionContext = new DbInterceptionContext();
            var command = new Mock<DbCommand>().Object;

            var mockInterceptor = new Mock<ICancelableDbCommandInterceptor>();
            mockInterceptor.Setup(m => m.CommandExecuting(command, interceptionContext)).Returns(true);

            var dispatcher = new CancelableDbCommandDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(mockInterceptor.Object);

            Assert.True(dispatcher.Executing(command, interceptionContext));

            mockInterceptor.Verify(m => m.CommandExecuting(command, interceptionContext));
        }

        [Fact]
        public void Executing_returns_false_if_any_interceptor_returns_false()
        {
            var mockInterceptor1 = new Mock<ICancelableDbCommandInterceptor>();
            mockInterceptor1.Setup(m => m.CommandExecuting(It.IsAny<DbCommand>(), It.IsAny<DbInterceptionContext>())).Returns(true);

            var mockInterceptor2 = new Mock<ICancelableDbCommandInterceptor>();
            mockInterceptor2.Setup(m => m.CommandExecuting(It.IsAny<DbCommand>(), It.IsAny<DbInterceptionContext>())).Returns(false);

            var mockInterceptor3 = new Mock<ICancelableDbCommandInterceptor>();
            mockInterceptor3.Setup(m => m.CommandExecuting(It.IsAny<DbCommand>(), It.IsAny<DbInterceptionContext>())).Returns(true);

            var dispatcher = new CancelableDbCommandDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(mockInterceptor1.Object);
            internalDispatcher.Add(mockInterceptor2.Object);
            internalDispatcher.Add(mockInterceptor3.Object);

            Assert.False(dispatcher.Executing(new Mock<DbCommand>().Object, new DbInterceptionContext()));
        }

        [Fact]
        public void Executing_returns_true_if_no_interceptors_are_registered()
        {
            Assert.True(new CancelableDbCommandDispatcher().Executing(new Mock<DbCommand>().Object, new DbInterceptionContext()));
        }
    }
}
