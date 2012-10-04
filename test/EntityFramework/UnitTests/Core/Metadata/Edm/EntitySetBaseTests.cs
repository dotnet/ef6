// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class EntitySetBaseTests
    {
        private class TestEntitySetBase : EntitySetBase
        {
        }

        [Fact]
        public void Can_set_and_get_name()
        {
            var entitySetBase
                = new TestEntitySetBase
                {
                    Name = "Foo"
                };

            Assert.Equal("Foo", entitySetBase.Name);
        }
    }
}
