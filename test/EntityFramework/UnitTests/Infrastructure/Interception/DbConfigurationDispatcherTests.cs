// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using Moq;
    using Xunit;

    public class DbConfigurationDispatcherTests
    {
        [Fact]
        public void Loaded_dispatches_to_interceptors()
        {
            var interceptionContext = new DbInterceptionContext();
            var eventArgs = new DbConfigurationLoadedEventArgs(new Mock<InternalConfiguration>(null, null, null, null, null).Object);
            var mockInterceptor1 = new Mock<IDbConfigurationInterceptor>();
            var mockInterceptor2 = new Mock<IDbConfigurationInterceptor>();

            var dispatcher = new DbConfigurationDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(mockInterceptor1.Object);
            internalDispatcher.Add(mockInterceptor2.Object);

            dispatcher.Loaded(eventArgs, interceptionContext);

            mockInterceptor1.Verify(m => m.Loaded(eventArgs, It.IsAny<DbConfigurationInterceptionContext>()), Times.Once());
            mockInterceptor2.Verify(m => m.Loaded(eventArgs, It.IsAny<DbConfigurationInterceptionContext>()), Times.Once());
        }
    }
}
