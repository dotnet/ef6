// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Infrastructure;
    using Moq;
    using Xunit;

    public class DefaultModelCacheKeyFactoryTests : TestBase
    {
        [Fact]
        public void Create_should_return_key_based_on_provided_context()
        {
            var cacheKey1 = new DefaultModelCacheKeyFactory().Create(new CacheKeyFactoryContext());
            var cacheKey2 = new DefaultModelCacheKeyFactory().Create(new CacheKeyFactoryContext());

            Assert.Equal(cacheKey1, cacheKey2);
        }

        [Fact]
        public void Create_should_probe_for_custom_key_provider()
        {
            var mockContext
                = new Mock<DbContext>
                      {
                          DefaultValue = DefaultValue.Mock
                      };

            var mockInternalContext = new Mock<InternalContext>();
            mockInternalContext.SetupGet(ic => ic.ProviderName).Returns("foo");

            mockContext.SetupGet(c => c.InternalContext).Returns(mockInternalContext.Object);

            var mockCacheKeyProvider = mockContext.As<IDbModelCacheKeyProvider>();

            new DefaultModelCacheKeyFactory().Create(mockContext.Object);

            mockCacheKeyProvider.VerifyGet(ckp => ckp.CacheKey);
        }

        private class CacheKeyFactoryContext : DbContext
        {
        }
    }
}
