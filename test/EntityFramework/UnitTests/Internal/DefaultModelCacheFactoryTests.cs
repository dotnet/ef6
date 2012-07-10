namespace System.Data.Entity.Internal
{
    using Xunit;

    public class DefaultModelCacheFactoryTests : TestBase
    {
        [Fact]
        public void Create_should_return_key_based_on_provided_context()
        {
            var cacheKey1 = new DefaultModelCacheKeyFactory().Create(new CacheKeyFactoryContext());
            var cacheKey2 = new DefaultModelCacheKeyFactory().Create(new CacheKeyFactoryContext());

            Assert.Equal(cacheKey1, cacheKey2);
        }

        private class CacheKeyFactoryContext : DbContext
        {
        }
    }
}
