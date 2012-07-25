// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Model;
    using System.Linq;
    using Xunit;

    public class DropPrimaryKeyOperationTests
    {
        [Fact]
        public void Can_get_and_set_table_and_name_and_columns()
        {
            var dropPrimaryKeyOperation = new DropPrimaryKeyOperation { Table = "T", Name = "Pk" };

            dropPrimaryKeyOperation.Columns.Add("pk2");

            Assert.Equal("T", dropPrimaryKeyOperation.Table);
            Assert.Equal("Pk", dropPrimaryKeyOperation.Name);
            Assert.Equal("pk2", dropPrimaryKeyOperation.Columns.Single());
            Assert.False(dropPrimaryKeyOperation.HasDefaultName);
        }

        [Fact]
        public void Can_get_default_for_name()
        {
            var dropPrimaryKeyOperation = new DropPrimaryKeyOperation { Table = "T" };

            Assert.Equal("PK_T", dropPrimaryKeyOperation.Name);
            Assert.True(dropPrimaryKeyOperation.HasDefaultName);
        }

        [Fact]
        public void Inverse_should_return_drop_operation()
        {
            var dropPrimaryKeyOperation = new DropPrimaryKeyOperation { Table = "T", Name = "Pk" };

            dropPrimaryKeyOperation.Columns.Add("pk2");

            var inverse = (AddPrimaryKeyOperation)dropPrimaryKeyOperation.Inverse;

            Assert.Equal("T", inverse.Table);
            Assert.Equal("Pk", inverse.Name);
            Assert.Equal("pk2", inverse.Columns.Single());
        }
    }
}