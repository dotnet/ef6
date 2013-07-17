// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Linq;
    using Xunit;

    public class DropForeignKeyOperationTests
    {
        [Fact]
        public void Can_get_and_set_properties()
        {
            var dropForeignKeyOperation = new DropForeignKeyOperation
                                              {
                                                  PrincipalTable = "P",
                                                  DependentTable = "D",
                                                  Name = "Foo"
                                              };

            dropForeignKeyOperation.DependentColumns.Add("fk");

            Assert.Equal("P", dropForeignKeyOperation.PrincipalTable);
            Assert.Equal("D", dropForeignKeyOperation.DependentTable);
            Assert.Equal("fk", dropForeignKeyOperation.DependentColumns.Single());
            Assert.Equal("Foo", dropForeignKeyOperation.Name);
            Assert.Equal("FK_D_P_fk", dropForeignKeyOperation.DefaultName);
            Assert.False(dropForeignKeyOperation.HasDefaultName);
        }

        [Fact]
        public void CreateDropIndexOperation_should_return_corresponding_drop_index_operation()
        {
            var addForeignKeyOperation
                = new AddForeignKeyOperation
                      {
                          DependentTable = "D"
                      };
            addForeignKeyOperation.DependentColumns.Add("fk");

            var dropForeignKeyOperation
                = new DropForeignKeyOperation(addForeignKeyOperation)
                      {
                          DependentTable = "D"
                      };

            dropForeignKeyOperation.DependentColumns.Add("fk");

            var dropIndexOperation = dropForeignKeyOperation.CreateDropIndexOperation();

            Assert.Equal("D", dropIndexOperation.Table);
            Assert.NotNull(dropIndexOperation.Inverse);
            Assert.Equal("fk", dropIndexOperation.Columns.Single());
        }

        [Fact]
        public void Inverse_should_return_add_foreign_key_operation()
        {
            var addForeignKeyOperation = new AddForeignKeyOperation();
            var dropForeignKeyOperation = new DropForeignKeyOperation(addForeignKeyOperation);

            Assert.Same(addForeignKeyOperation, dropForeignKeyOperation.Inverse);
        }

        [Fact]
        public void Anonymous_args_should_not_throw_when_string_supplied()
        {
            var dropForeignKeyOperation
                = new DropForeignKeyOperation("Foo");

            Assert.Equal(3, dropForeignKeyOperation.AnonymousArguments["Length"]);
        }
    }
}
