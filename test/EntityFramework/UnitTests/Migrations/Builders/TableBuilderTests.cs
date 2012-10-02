// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Builders
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Migrations.Model;
    using System.Linq;
    using Xunit;

    public class TableBuilderTests
    {
        private class TestMigration : DbMigration
        {
            public override void Up()
            {
            }
        }

        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(
                "createTableOperation",
                Assert.Throws<ArgumentNullException>(() => new TableBuilder<object>(null, new TestMigration())).ParamName);
        }

        public class Columns
        {
            public object Foo { get; set; }
            public object Bar { get; set; }
        }

        [Fact]
        public void PrimaryKey_should_set_key_columns_and_name()
        {
            var createTableOperation = new CreateTableOperation("T");
            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.Guid)
                    {
                        Name = "Foo"
                    });
            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.Guid)
                    {
                        Name = "Bar"
                    });
            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.Guid)
                    {
                        Name = "Baz"
                    });

            var tableBuilder = new TableBuilder<Columns>(createTableOperation, new TestMigration());

            tableBuilder.PrimaryKey(
                c => new
                         {
                             c.Bar,
                             c.Foo
                         }, name: "PK_Custom");

            Assert.Equal(2, createTableOperation.PrimaryKey.Columns.Count());
            Assert.Equal("Bar", createTableOperation.PrimaryKey.Columns.First());
            Assert.Equal("Foo", createTableOperation.PrimaryKey.Columns.Last());
            Assert.Equal("PK_Custom", createTableOperation.PrimaryKey.Name);
        }

        [Fact]
        public void ForeignKey_should_create_and_add_fk_model()
        {
            var createTableOperation = new CreateTableOperation("T");
            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.Guid)
                    {
                        Name = "Foo"
                    });
            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.Guid)
                    {
                        Name = "Bar"
                    });

            var migration = new TestMigration();
            var tableBuilder = new TableBuilder<Columns>(createTableOperation, migration);

            tableBuilder.ForeignKey(
                "P", c => new
                              {
                                  c.Foo,
                                  c.Bar
                              }, true, "my_fk");

            Assert.Equal(1, migration.Operations.Count());

            var addForeignKeyOperation = migration.Operations.Cast<AddForeignKeyOperation>().Single();

            Assert.Equal("P", addForeignKeyOperation.PrincipalTable);
            Assert.Equal("Foo", addForeignKeyOperation.DependentColumns.First());
            Assert.Equal("Bar", addForeignKeyOperation.DependentColumns.Last());
            Assert.True(addForeignKeyOperation.CascadeDelete);
            Assert.Equal("my_fk", addForeignKeyOperation.Name);
        }

        [Fact]
        public void Index_should_add_create_index_operation_to_model()
        {
            var createTableOperation = new CreateTableOperation("T");
            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.Guid)
                    {
                        Name = "Foo"
                    });
            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.Guid)
                    {
                        Name = "Bar"
                    });

            var migration = new TestMigration();
            var tableBuilder = new TableBuilder<Columns>(createTableOperation, migration);

            tableBuilder.Index(
                c => new
                         {
                             c.Foo,
                             c.Bar
                         }, unique: true);

            Assert.Equal(1, migration.Operations.Count());

            var createIndexOperation
                = migration.Operations.Cast<CreateIndexOperation>().Single();

            Assert.Equal("T", createIndexOperation.Table);
            Assert.True(createIndexOperation.IsUnique);
            Assert.Equal("Foo", createIndexOperation.Columns.First());
            Assert.Equal("Bar", createIndexOperation.Columns.Last());
        }
    }
}
