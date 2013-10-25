// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using Moq;
    using Xunit;

    public class ConfigLoadedCollectionTests : TestBase
    {
        [Fact]
        public void All_registered_handlers_are_returned()
        {
            EventHandler<DbConfigurationLoadedEventArgs> handler1 = (_, __) => { };
            EventHandler<DbConfigurationLoadedEventArgs> handler2 = (_, __) => { };
            EventHandler<DbConfigurationLoadedEventArgs> handler3 = (_, __) => { };

            var mockElement1 = new Mock<ConfigLoadedHandlerElement>(1);
            mockElement1.Setup(m => m.CreateHandlerDelegate()).Returns(handler1);

            var mockElement2 = new Mock<ConfigLoadedHandlerElement>(2);
            mockElement2.Setup(m => m.CreateHandlerDelegate()).Returns(handler2);

            var mockElement3 = new Mock<ConfigLoadedHandlerElement>(3);
            mockElement3.Setup(m => m.CreateHandlerDelegate()).Returns(handler3);

            var collection = new ConfigLoadedCollection();
            collection.AddElement(mockElement1.Object);
            collection.AddElement(mockElement2.Object);
            collection.AddElement(mockElement3.Object);

            Assert.Equal(new[] { handler1, handler2, handler3 }, collection.RegisteredHandlers);
        }
    }
}
