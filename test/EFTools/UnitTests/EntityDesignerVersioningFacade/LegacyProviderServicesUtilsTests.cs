// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade
{
    using System;
    using System.Data.Common;
    using Moq;
    using Xunit;

    public class LegacyProviderServicesUtilsTests
    {
        [Fact]
        public void CanGetDbProviderServices_returns_true_if_DbProviderServices_returned_from_service_provider()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(p => p.GetService(typeof(DbProviderServices)))
                .Returns(new Mock<DbProviderServices>());

            Assert.True(LegacyDbProviderServicesUtils.CanGetDbProviderServices(mockServiceProvider.Object));
        }

        [Fact]
        public void CanGetDbProviderServices_returns_false_if_DbProviderServices_not_returned_from_service_provider()
        {
            Assert.False(LegacyDbProviderServicesUtils.CanGetDbProviderServices(new Mock<IServiceProvider>().Object));
        }

        [Fact]
        public void CanGetDbProviderServices_returns_false_if_service_provider_throws()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(p => p.GetService(typeof(DbProviderServices)))
                .Throws<InvalidOperationException>();

            Assert.False(LegacyDbProviderServicesUtils.CanGetDbProviderServices(mockServiceProvider.Object));
        }
    }
}
