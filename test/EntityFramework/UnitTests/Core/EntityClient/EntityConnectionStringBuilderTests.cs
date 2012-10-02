// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityClient
{
    using Xunit;

    public class EntityConnectionStringBuilderTests
    {
        [Fact]
        public void ContainsKey_throws_for_null_argument()
        {
            Assert.Equal(
                "keyword",
                Assert.Throws<ArgumentNullException>(
                    () => new EntityConnectionStringBuilder().ContainsKey(null)).ParamName);
        }

        [Fact]
        public void Indexer_Get_throws_for_null_argument()
        {
            Assert.Equal(
                "keyword",
                Assert.Throws<ArgumentNullException>(
                    () => new EntityConnectionStringBuilder()[null]).ParamName);
        }

        [Fact]
        public void Indexer_Set_throws_for_null_argument()
        {
            Assert.Equal(
                "keyword",
                Assert.Throws<ArgumentNullException>(
                    () => new EntityConnectionStringBuilder()[null] = new object()).ParamName);
        }

        [Fact]
        public void Remove_throws_for_null_argument()
        {
            Assert.Equal(
                "keyword",
                Assert.Throws<ArgumentNullException>(
                    () => new EntityConnectionStringBuilder().Remove(null)).ParamName);
        }

        [Fact]
        public void TryGetValue_throws_for_null_argument()
        {
            object value;
            Assert.Equal(
                "keyword",
                Assert.Throws<ArgumentNullException>(
                    () => new EntityConnectionStringBuilder().TryGetValue(null, out value)).ParamName);
        }
    }
}
