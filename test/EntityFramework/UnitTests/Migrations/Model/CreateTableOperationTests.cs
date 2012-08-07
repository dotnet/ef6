// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public class CreateTableOperationTests
    {
        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("name")).Message,
                Assert.Throws<ArgumentException>(() => new CreateTableOperation(null)).Message);
        }

        [Fact]
        public void Name_should_return_name_from_ctor()
        {
            Assert.Equal("Foo", new CreateTableOperation("Foo").Name);
        }

        [Fact]
        public void Can_add_and_enumerate_columns()
        {
            var createTableOperation = new CreateTableOperation("Foo");
            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.Int64)
                    {
                        Name = "Bar",
                        IsNullable = true
                    });

            Assert.NotNull(createTableOperation.Columns.Single());
        }

        [Fact]
        public void Inverse_should_produce_drop_table_operation()
        {
            var createTableOperation = new CreateTableOperation("Foo");

            var dropTableOperation = (DropTableOperation)createTableOperation.Inverse;

            Assert.Equal("Foo", dropTableOperation.Name);
        }

        [Fact]
        public void Can_set_and_get_primary_key()
        {
            var addPrimaryKeyOperation = new AddPrimaryKeyOperation();
            var table = new CreateTableOperation("T")
                            {
                                PrimaryKey = addPrimaryKeyOperation
                            };

            Assert.Same(addPrimaryKeyOperation, table.PrimaryKey);
            Assert.Equal("T", addPrimaryKeyOperation.Table);
        }
    }
}
