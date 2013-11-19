// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.MappingViews
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class DefaultDbMappingViewCacheFactoryTests
    {
        [Fact]
        public void Create_returns_instance_of_type_specified_in_constructor()
        {
            var factory = new DefaultDbMappingViewCacheFactory(typeof(SampleMappingViewCache));

            var cache = factory.Create("C", "S");

            Assert.NotNull(cache);
            Assert.Same(typeof(SampleMappingViewCache), cache.GetType());
        }

        [Fact]
        public void Different_instances_containing_the_same_mapping_view_cache_type_are_equal()
        {
            var factory1 = new DefaultDbMappingViewCacheFactory(typeof(SampleMappingViewCache));
            var factory2 = new DefaultDbMappingViewCacheFactory(typeof(SampleMappingViewCache));

            Assert.True(factory1.Equals(factory2));
            Assert.True(factory2.Equals(factory1));
            Assert.True(factory1.GetHashCode() == factory2.GetHashCode());
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
