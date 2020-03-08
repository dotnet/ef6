// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Data.Entity.Infrastructure.Interception;
    using Moq;
    using Xunit;

    public class InterceptorsCollectionTests : TestBase
    {
        [Fact]
        public void All_registered_interceptors_are_returned()
        {
            var interceptor1 = new Mock<IDbConfigurationInterceptor>().Object;
            var interceptor2 = new Mock<IDbConfigurationInterceptor>().Object;

            var mockElement1 = new Mock<InterceptorElement>(1);
            mockElement1.Setup(m => m.CreateInterceptor()).Returns(interceptor1);

            var mockElement2 = new Mock<InterceptorElement>(2);
            mockElement2.Setup(m => m.CreateInterceptor()).Returns(interceptor2);

            var collection = new InterceptorsCollection();
            collection.AddElement(mockElement1.Object);
            collection.AddElement(mockElement2.Object);

            Assert.Equal(new[] { interceptor1, interceptor2 }, collection.Interceptors);
        }
    }
}
