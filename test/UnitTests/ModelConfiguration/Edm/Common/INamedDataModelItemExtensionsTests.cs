// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Common
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public sealed class INamedDataModelItemExtensionsTests
    {
        [Fact]
        public void UniquifyName_should_assign_unique_names()
        {
            var namedItems = new List<EdmProperty>();

            Assert.Equal("Foo", namedItems.UniquifyName("Foo"));

            namedItems.Add(new EdmProperty("Foo"));

            Assert.Equal("Foo1", namedItems.UniquifyName("Foo"));

            namedItems.Add(new EdmProperty("Foo1"));

            Assert.Equal("Foo2", namedItems.UniquifyName("Foo"));
        }
    }
}
