// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Common;
    using System.Data.SqlClient;
    using Moq;
    using Xunit;

    public class DefaultModelCacheKeyTests : TestBase
    {
        [Fact]
        public void Equals_when_parts_equal_should_return_true()
        {
            var defaultModelCacheKey1 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", typeof(SqlClientFactory), "dbo");
            var defaultModelCacheKey2 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", typeof(SqlClientFactory), "dbo");

            Assert.Equal(defaultModelCacheKey1, defaultModelCacheKey2);
        }

        [Fact]
        public void GetHashCode_when_parts_equal_should_return_same_value()
        {
            var defaultModelCacheKey1 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", typeof(SqlClientFactory), "dbo");
            var defaultModelCacheKey2 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", typeof(SqlClientFactory), "dbo");

            Assert.Equal(defaultModelCacheKey1.GetHashCode(), defaultModelCacheKey2.GetHashCode());
        }

        [Fact]
        public void Equals_when_types_not_equal_should_return_false()
        {
            var defaultModelCacheKey1 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", typeof(SqlClientFactory), "dbo");
            var defaultModelCacheKey2 = new DefaultModelCacheKey(typeof(CacheKeyContext2), "Foo", typeof(SqlClientFactory), "dbo");

            Assert.NotEqual(defaultModelCacheKey1, defaultModelCacheKey2);
        }

        [Fact]
        public void GetHashCode_when_types_not_equal_should_return_different_values()
        {
            var defaultModelCacheKey1 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", typeof(SqlClientFactory), "dbo");
            var defaultModelCacheKey2 = new DefaultModelCacheKey(typeof(CacheKeyContext2), "Foo", typeof(SqlClientFactory), "dbo");

            Assert.NotEqual(defaultModelCacheKey1.GetHashCode(), defaultModelCacheKey2.GetHashCode());
        }

        [Fact]
        public void Equals_when_provider_name_not_equal_should_return_false()
        {
            var defaultModelCacheKey1 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", typeof(SqlClientFactory), "dbo");
            var defaultModelCacheKey2 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Bar", typeof(SqlClientFactory), "dbo");

            Assert.NotEqual(defaultModelCacheKey1, defaultModelCacheKey2);
        }

        [Fact]
        public void GetHashCode_when_provider_name_not_equal_should_return_different_values()
        {
            var defaultModelCacheKey1 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", typeof(SqlClientFactory), "dbo");
            var defaultModelCacheKey2 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Bar", typeof(SqlClientFactory), "dbo");

            Assert.NotEqual(defaultModelCacheKey1.GetHashCode(), defaultModelCacheKey2.GetHashCode());
        }

        [Fact]
        public void Equals_when_provider_type_not_equal_should_return_false()
        {
            var defaultModelCacheKey1 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", typeof(SqlClientFactory), "dbo");
            var defaultModelCacheKey2 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", new Mock<DbProviderFactory>().Object.GetType(), "dbo");

            Assert.NotEqual(defaultModelCacheKey1, defaultModelCacheKey2);
        }

        [Fact]
        public void GetHashCode_when_provider_type_not_equal_should_return_different_values()
        {
            var defaultModelCacheKey1 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", typeof(SqlClientFactory), "dbo");
            var defaultModelCacheKey2 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", new Mock<DbProviderFactory>().Object.GetType(), "dbo");

            Assert.NotEqual(defaultModelCacheKey1.GetHashCode(), defaultModelCacheKey2.GetHashCode());
        }

        [Fact]
        public void Equals_when_schema_not_equal_should_return_false()
        {
            var defaultModelCacheKey1 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", typeof(SqlClientFactory), "dbo");
            var defaultModelCacheKey2 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", typeof(SqlClientFactory), "bar");

            Assert.NotEqual(defaultModelCacheKey1, defaultModelCacheKey2);
        }

        [Fact]
        public void GetHashCode_when_schema_not_equal_should_return_different_values()
        {
            var defaultModelCacheKey1 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", typeof(SqlClientFactory), "dbo");
            var defaultModelCacheKey2 = new DefaultModelCacheKey(typeof(CacheKeyContext1), "Foo", typeof(SqlClientFactory), "bar");

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
