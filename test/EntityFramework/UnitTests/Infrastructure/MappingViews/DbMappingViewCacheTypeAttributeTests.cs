// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.MappingViews
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using Xunit;

    public class DbMappingViewCacheTypeAttributeTests
    {
        [Fact]
        public void Can_create_instance_with_valid_context_and_cache_types()
        {
            var attribute = new DbMappingViewCacheTypeAttribute(typeof(SampleContext), typeof(SampleMappingViewCache));

            Assert.Same(typeof(SampleContext), attribute.ContextType);
            Assert.Same(typeof(SampleMappingViewCache), attribute.CacheType);
        }

        [Fact]
        public void Can_create_instance_with_valid_context_type_and_cache_type_name()
        {
            var cacheTypeName = typeof(SampleMappingViewCache).AssemblyQualifiedName;
            var attribute = new DbMappingViewCacheTypeAttribute(typeof(SampleContext), cacheTypeName);

            Assert.Same(typeof(SampleContext), attribute.ContextType);
            Assert.Same(typeof(SampleMappingViewCache), attribute.CacheType);
        }

        [Fact]
        public void Constructors_validate_preconditions()
        {
            var cacheTypeName = typeof(SampleMappingViewCache).AssemblyQualifiedName;

            Assert.Equal("contextType", 
                Assert.Throws<ArgumentNullException>(() => 
                    new DbMappingViewCacheTypeAttribute(null, typeof(SampleMappingViewCache))).ParamName);
            Assert.Equal("contextType",
                Assert.Throws<ArgumentNullException>(() =>
                    new DbMappingViewCacheTypeAttribute(null, cacheTypeName)).ParamName);

            Assert.Equal("cacheType", 
                Assert.Throws<ArgumentNullException>(() => 
                    new DbMappingViewCacheTypeAttribute(typeof(SampleContext), (Type)null)).ParamName);
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("cacheTypeName"),
                Assert.Throws<ArgumentException>(() =>
                    new DbMappingViewCacheTypeAttribute(typeof(SampleContext), (string)null)).Message);
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("cacheTypeName"),
                Assert.Throws<ArgumentException>(() =>
                    new DbMappingViewCacheTypeAttribute(typeof(SampleContext), String.Empty)).Message);
        }

        [Fact]
        public void Constructors_throw_if_invalid_context_type()
        {
            var cacheTypeName = typeof(SampleMappingViewCache).AssemblyQualifiedName;
            var exception = new ArgumentException(
                Strings.DbMappingViewCacheTypeAttribute_InvalidContextType(typeof(object)),
                "contextType");

            Assert.Equal(exception.Message,
                Assert.Throws<ArgumentException>(() =>
                    new DbMappingViewCacheTypeAttribute(typeof(object), typeof(SampleMappingViewCache))).Message);

            Assert.Equal(exception.Message,
                Assert.Throws<ArgumentException>(() =>
                    new DbMappingViewCacheTypeAttribute(typeof(object), cacheTypeName)).Message);
        }

        [Fact]
        public void Constructor_throws_if_invalid_cache_type()
        {
            var exception = new ArgumentException(
                Strings.Generated_View_Type_Super_Class(typeof(object)),
                "cacheType");

            Assert.Equal(exception.Message,
                Assert.Throws<ArgumentException>(() =>
                    new DbMappingViewCacheTypeAttribute(typeof(SampleContext), typeof(object))).Message);
        }

        [Fact]
        public void Constructor_throws_if_invalid_cache_type_name()
        {
            const string cacheTypeName = "InvalidCacheTypeName";
            var exception = new ArgumentException(
                Strings.DbMappingViewCacheTypeAttribute_CacheTypeNotFound(cacheTypeName),
                "cacheTypeName");

            Assert.Equal(exception.Message,
                Assert.Throws<ArgumentException>(() =>
                    new DbMappingViewCacheTypeAttribute(typeof(SampleContext), cacheTypeName)).Message);
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
