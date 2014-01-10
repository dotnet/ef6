// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Resources;
    using Moq;
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

        [Fact]
        public void Can_set_name_and_parent_notified()
        {
            var entitySetBase = new TestEntitySetBase();

            var entityContainerMock = new Mock<EntityContainer>();

            entitySetBase.ChangeEntityContainerWithoutCollectionFixup(entityContainerMock.Object);

            var initialIdentity = entitySetBase.Identity;
            entitySetBase.Name = "Foo";

            entityContainerMock.Verify(e => e.NotifyItemIdentityChanged(entitySetBase, initialIdentity), Times.Once());
        }

        [Fact]
        public void Can_set_and_get_table()
        {
            var entitySetBase
                = new TestEntitySetBase
                      {
                          Table = "Foo"
                      };

            Assert.Equal("Foo", entitySetBase.Table);
        }

        [Fact]
        public void Can_set_and_get_schema()
        {
            var entitySetBase
                = new TestEntitySetBase
                      {
                          Schema = "Foo"
                      };

            Assert.Equal("Foo", entitySetBase.Schema);
        }

        [Fact]
        public void Can_set_and_get_defining_query()
        {
            var entitySetBase
                = new TestEntitySetBase
                      {
                          DefiningQuery = "Foo"
                      };

            Assert.Equal("Foo", entitySetBase.DefiningQuery);
        }

        [Fact]
        public void Cannot_set_defining_query_for_sealed_entity_set_base()
        {
            var entitySetBase = new TestEntitySetBase();
            entitySetBase.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(() => entitySetBase.DefiningQuery = "abc").Message);
        }
    }
}
