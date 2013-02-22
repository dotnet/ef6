// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public sealed class EntitySetExtensionsTests
    {
        [Fact]
        public void UniquifyName_should_assign_unique_names()
        {
            var namedItems = new List<EntitySet>();

            Assert.Equal("Foo", namedItems.UniquifyIdentifier("Foo"));

            namedItems.Add(new EntitySet("ES1", null, "Foo", null, new EntityType()));

            Assert.Equal("Foo1", namedItems.UniquifyIdentifier("Foo"));

            namedItems.Add(new EntitySet("ES2", null, "Foo1", null, new EntityType()));

            Assert.Equal("Foo2", namedItems.UniquifyIdentifier("Foo"));
        }
    }
}
