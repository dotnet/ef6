// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public class DefaultDbMappingViewCacheFactoryTests
    {
        [Fact]
        public void Create_returns_instance_of_type_specified_by_attribute()
        {
            var attribute = new DbMappingViewCacheTypeAttribute(typeof(SampleContext), typeof(SampleMappingViewCache));
            var factory = new DefaultDbMappingViewCacheFactory(attribute);

            var cache = factory.Create("C", "S");

            Assert.NotNull(cache);
            Assert.Same(typeof(SampleMappingViewCache), cache.GetType());
        }

        [Fact]
        public void Different_instances_with_same_attribute_are_equal()
        {
            var attribute = new DbMappingViewCacheTypeAttribute(typeof(SampleContext), typeof(SampleMappingViewCache));
            var factory1 = new DefaultDbMappingViewCacheFactory(attribute);
            var factory2 = new DefaultDbMappingViewCacheFactory(attribute);

            Assert.True(factory1.Equals(factory2));
            Assert.True(factory2.Equals(factory1));
            Assert.True(factory1.GetHashCode() == factory2.GetHashCode());
        }

        private class SampleContext : DbContext
        {
        }

        private class SampleMappingViewCache : DbMappingViewCache
        {
            public override string MappingHashValue
            {
                get { throw new NotImplementedException(); }
            }

            public override DbMappingView GetView(EntitySetBase extent)
            {
                throw new NotImplementedException();
            }
        }
    }
}
