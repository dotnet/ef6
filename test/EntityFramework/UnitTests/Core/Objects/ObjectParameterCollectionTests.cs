namespace System.Data.Entity.Core.Objects
{
    using System;
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class ObjectParameterCollectionTests
    {
        [Fact]
        public void Add_throws_for_null_argument()
        {
            var objectParameterCollection = new ObjectParameterCollection(new ClrPerspective(new MetadataWorkspace()));
            Assert.Equal("item",
                Assert.Throws<ArgumentNullException>(
                    () => objectParameterCollection.Add(null)).ParamName);
        }

        [Fact]
        public void Contains_throws_for_null_argument()
        {
            var objectParameterCollection = new ObjectParameterCollection(new ClrPerspective(new MetadataWorkspace()));
            Assert.Equal("item",
                Assert.Throws<ArgumentNullException>(
                    () => objectParameterCollection.Contains((ObjectParameter)null)).ParamName);
        }

        [Fact]
        public void Remove_throws_for_null_argument()
        {
            var objectParameterCollection = new ObjectParameterCollection(new ClrPerspective(new MetadataWorkspace()));
            Assert.Equal("item",
                Assert.Throws<ArgumentNullException>(
                    () => objectParameterCollection.Remove(null)).ParamName);
        }
    }
}
