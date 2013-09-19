// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Infrastructure.Interception;
    using Moq;
    using Xunit;

    public class EntityConnectionDispatcherTests : TestBase
    {
        [Fact]
        public void Opening_dispatches_to_interceptors()
        {
            var interceptionContext = new DbInterceptionContext();
            var connection = new Mock<EntityConnection>().Object;

            var mockInterceptor = new Mock<IEntityConnectionInterceptor>();
            mockInterceptor.Setup(m => m.ConnectionOpening(connection, interceptionContext)).Returns(true);

            var dispatcher = new EntityConnectionDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(mockInterceptor.Object);

            Assert.True(dispatcher.Opening(connection, interceptionContext));

            mockInterceptor.Verify(m => m.ConnectionOpening(connection, interceptionContext));
        }

        [Fact]
        public void Opening_returns_false_if_any_interceptor_returns_false()
        {
            var mockInterceptor1 = new Mock<IEntityConnectionInterceptor>();
            mockInterceptor1.Setup(m => m.ConnectionOpening(It.IsAny<EntityConnection>(), It.IsAny<DbInterceptionContext>())).Returns(true);

            var mockInterceptor2 = new Mock<IEntityConnectionInterceptor>();
            mockInterceptor2.Setup(m => m.ConnectionOpening(It.IsAny<EntityConnection>(), It.IsAny<DbInterceptionContext>())).Returns(false);

            var mockInterceptor3 = new Mock<IEntityConnectionInterceptor>();
            mockInterceptor3.Setup(m => m.ConnectionOpening(It.IsAny<EntityConnection>(), It.IsAny<DbInterceptionContext>())).Returns(true);

            var dispatcher = new EntityConnectionDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(mockInterceptor1.Object);
            internalDispatcher.Add(mockInterceptor2.Object);
            internalDispatcher.Add(mockInterceptor3.Object);

            Assert.False(dispatcher.Opening(new Mock<EntityConnection>().Object, new DbInterceptionContext()));
        }

        [Fact]
        public void Opening_returns_true_if_no_interceptors_are_registered()
        {
            Assert.True(new EntityConnectionDispatcher().Opening(new Mock<EntityConnection>().Object, new DbInterceptionContext()));
        }
    }
}
