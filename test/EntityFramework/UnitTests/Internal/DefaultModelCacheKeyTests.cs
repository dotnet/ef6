namespace System.Data.Entity.Internal
{
    using Xunit;

    public class DefaultModelCacheKeyTests : TestBase
    {
        [Fact]
        public void Equals_when_parts_equal_should_return_true()
        {
            var defaultModelCacheKey1 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", "dbo");
            var defaultModelCacheKey2 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", "dbo");

            Assert.Equal(defaultModelCacheKey1, defaultModelCacheKey2);
        }

        [Fact]
        public void GetHashCode_when_parts_equal_should_return_same_value()
        {
            var defaultModelCacheKey1 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", "dbo");
            var defaultModelCacheKey2 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", "dbo");

            Assert.Equal(defaultModelCacheKey1.GetHashCode(), defaultModelCacheKey2.GetHashCode());
        }

        [Fact]
        public void Equals_when_types_not_equal_should_return_false()
        {
            var defaultModelCacheKey1 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", "dbo");
            var defaultModelCacheKey2 = new DefaultModelCacheKey(typeof(CacheKeyContext2), "Foo", "dbo");

            Assert.NotEqual(defaultModelCacheKey1, defaultModelCacheKey2);
        }

        [Fact]
        public void GetHashCode_when_types_not_equal_should_return_false()
        {
            var defaultModelCacheKey1 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", "dbo");
            var defaultModelCacheKey2 = new DefaultModelCacheKey(typeof(CacheKeyContext2), "Foo", "dbo");

            Assert.NotEqual(defaultModelCacheKey1.GetHashCode(), defaultModelCacheKey2.GetHashCode());
        }

        [Fact]
        public void Equals_when_providers_not_equal_should_return_false()
        {
            var defaultModelCacheKey1 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", "dbo");
            var defaultModelCacheKey2 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Bar", "dbo");

            Assert.NotEqual(defaultModelCacheKey1, defaultModelCacheKey2);
        }

        [Fact]
        public void GetHashCode_when_providers_not_equal_should_return_false()
        {
            var defaultModelCacheKey1 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", "dbo");
            var defaultModelCacheKey2 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Bar", "dbo");

            Assert.NotEqual(defaultModelCacheKey1.GetHashCode(), defaultModelCacheKey2.GetHashCode());
        }

        [Fact]
        public void Equals_when_schema_not_equal_should_return_false()
        {
            var defaultModelCacheKey1 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", "dbo");
            var defaultModelCacheKey2 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", "bar");

            Assert.NotEqual(defaultModelCacheKey1, defaultModelCacheKey2);
        }

        [Fact]
        public void GetHashCode_when_schema_not_equal_should_return_false()
        {
            var defaultModelCacheKey1 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", "dbo");
            var defaultModelCacheKey2 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", "bar");

            Assert.NotEqual(defaultModelCacheKey1.GetHashCode(), defaultModelCacheKey2.GetHashCode());
        }

        private class CacheKeyContext1 : DbContext
        {
        }

        private class CacheKeyContext2 : DbContext
        {
        }
    }
}
