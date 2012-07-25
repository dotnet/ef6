// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Edm.Db.UnitTests
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Db;
    using Xunit;

    public sealed class DbAliasedMetadataItemExtensionsTests
    {
        [Fact]
        public void UniquifyName_should_assign_unique_names()
        {
            var namedItems = new List<DbAliasedMetadataItem>();

            Assert.Equal("Foo", namedItems.UniquifyIdentifier("Foo"));

            namedItems.Add(new DbTableMetadata { DatabaseIdentifier = "Foo" });

            Assert.Equal("Foo1", namedItems.UniquifyIdentifier("Foo"));

            namedItems.Add(new DbTableMetadata { DatabaseIdentifier = "Foo1" });

            Assert.Equal("Foo2", namedItems.UniquifyIdentifier("Foo"));
        }
    }
}