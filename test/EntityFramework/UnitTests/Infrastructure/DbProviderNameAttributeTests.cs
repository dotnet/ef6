// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Resources;
    using System.Data.Entity.SqlServer;
    using System.Linq;
    using Xunit;

    public class ProviderInvariantNameAttributeTests
    {
        private class AnotherSqlStrategy : SqlAzureExecutionStrategy
        {
        }

        [DbProviderName("NonSql")]
        private class NonSqlStrategy : SqlAzureExecutionStrategy
        {
        }

        [DbProviderName("One")]
        [DbProviderName("Two")]
        private class MultiProviderStrategy : SqlAzureExecutionStrategy
        {
        }
        
        [Fact]
        public void GetFromType_doesnt_return_attributes_from_base()
        {
            Assert.Equal(
                Strings.DbProviderNameAttributeNotFound(typeof(AnotherSqlStrategy)),
                Assert.Throws<InvalidOperationException>(
                    () => DbProviderNameAttribute.GetFromType(typeof(AnotherSqlStrategy))).Message);
        }

        [Fact]
        public void GetFromType_returns_overriden_attribute()
        {
            Assert.Equal("NonSql", DbProviderNameAttribute.GetFromType(typeof(NonSqlStrategy)).Single().Name);
        }
        
        [Fact]
        public void GetFromType_returns_multiple_attributes()
        {
            var attributes = DbProviderNameAttribute.GetFromType(typeof(MultiProviderStrategy)).ToList();
            Assert.Equal(2, attributes.Count);
            Assert.Contains("One", attributes.Select(a => a.Name));
            Assert.Contains("Two", attributes.Select(a => a.Name));
        }
        
        [Fact]
        public void GetFromType_throws_if_no_ProviderInvariantNameAttribute()
        {
            Assert.Equal(
                Strings.DbProviderNameAttributeNotFound(typeof(Random)),
                Assert.Throws<InvalidOperationException>(
                    () => DbProviderNameAttribute.GetFromType(typeof(Random))).Message);
        }
        
    }
}
