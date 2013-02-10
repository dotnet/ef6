// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Migrations.Model;
    using System.Linq;
    using Xunit;

    public class DbMigrationTests
    {
        [Fact]
        public void AddPrimaryKey_single_column_creates_add_primary_key_operation()
        {
            var migration = new TestMigration();

            migration.AddPrimaryKey("t", "c", "pk");

            var addPrimaryKeyOperation = migration.Operations.Cast<AddPrimaryKeyOperation>().Single();

            Assert.Equal("t", addPrimaryKeyOperation.Table);
            Assert.Equal("c", addPrimaryKeyOperation.Columns.Single());
            Assert.Equal("pk", addPrimaryKeyOperation.Name);
            Assert.True(addPrimaryKeyOperation.IsClustered);
        }

        [Fact]
        public void AddPrimaryKey_multiple_columns_creates_add_primary_key_operation()
        {
            var migration = new TestMigration();

            migration.AddPrimaryKey("t", new[] { "c1", "c2" }, "pk");

            var addPrimaryKeyOperation = migration.Operations.Cast<AddPrimaryKeyOperation>().Single();

            Assert.Equal("t", addPrimaryKeyOperation.Table);
            Assert.Equal("c1", addPrimaryKeyOperation.Columns.First());
            Assert.Equal("c2", addPrimaryKeyOperation.Columns.Last());
            Assert.Equal("pk", addPrimaryKeyOperation.Name);
            Assert.True(addPrimaryKeyOperation.IsClustered);
        }

        [Fact]
        public void AddPrimaryKey_can_set_clustered_parameter()
        {
            var migration = new TestMigration();

            migration.AddPrimaryKey("t", "c", "pk", clustered: false);

            var addPrimaryKeyOperation = migration.Operations.Cast<AddPrimaryKeyOperation>().Single();

            Assert.False(addPrimaryKeyOperation.IsClustered);
        }

        [Fact]
        public void DropPrimaryKey_by_name_creates_drop_primary_key_operation()
        {
            var migration = new TestMigration();

            migration.DropPrimaryKey("t", "pk");

            var dropForeignKeyOperation = migration.Operations.Cast<DropPrimaryKeyOperation>().Single();

            Assert.Equal("t", dropForeignKeyOperation.Table);
            Assert.Equal("pk", dropForeignKeyOperation.Name);
        }

        [Fact]
        public void DropPrimaryKey_by_table_name_creates_drop_primary_key_operation()
        {
            var migration = new TestMigration();

            migration.DropPrimaryKey("t");

            var dropForeignKeyOperation = migration.Operations.Cast<DropPrimaryKeyOperation>().Single();

            Assert.Equal("t", dropForeignKeyOperation.Table);
        }

        [Fact]
        public void AddForeignKey_creates_add_foreign_key_operation()
        {
            var migration = new TestMigration();

            migration.AddForeignKey("d", "dc", "p", "pc", true, "fk");

            var addForeignKeyOperation = migration.Operations.Cast<AddForeignKeyOperation>().Single();

            Assert.Equal("d", addForeignKeyOperation.DependentTable);
            Assert.Equal("dc", addForeignKeyOperation.DependentColumns.Single());
            Assert.Equal("p", addForeignKeyOperation.PrincipalTable);
            Assert.Equal("pc", addForeignKeyOperation.PrincipalColumns.Single());
            Assert.Equal("fk", addForeignKeyOperation.Name);
            Assert.True(addForeignKeyOperation.CascadeDelete);
        }

        [Fact]
        public void AddForeignKey_creates_add_foreign_key_operation_when_composite_key()
        {
            var migration = new TestMigration();

            migration.AddForeignKey("d", new[] { "dc1", "dc2" }, "p", new[] { "pc1", "pc2" }, true, "fk");

            var addForeignKeyOperation = migration.Operations.Cast<AddForeignKeyOperation>().Single();

            Assert.Equal("d", addForeignKeyOperation.DependentTable);
            Assert.Equal("dc1", addForeignKeyOperation.DependentColumns.First());
            Assert.Equal("dc2", addForeignKeyOperation.DependentColumns.Last());
            Assert.Equal("p", addForeignKeyOperation.PrincipalTable);
            Assert.Equal("pc1", addForeignKeyOperation.PrincipalColumns.First());
            Assert.Equal("pc2", addForeignKeyOperation.PrincipalColumns.Last());
            Assert.Equal("fk", addForeignKeyOperation.Name);
            Assert.True(addForeignKeyOperation.CascadeDelete);
        }

        [Fact]
        public void DropForeignKey_creates_drop_foreign_key_operation()
        {
            var migration = new TestMigration();

            migration.DropForeignKey("d", "dc", "p");

            var dropForeignKeyOperation = migration.Operations.Cast<DropForeignKeyOperation>().Single();

            Assert.Equal("d", dropForeignKeyOperation.DependentTable);
            Assert.Equal("dc", dropForeignKeyOperation.DependentColumns.Single());
            Assert.Equal("p", dropForeignKeyOperation.PrincipalTable);
            Assert.Equal(dropForeignKeyOperation.DefaultName, dropForeignKeyOperation.Name);
        }

        [Fact]
        public void DropForeignKey_creates_drop_foreign_key_operation_when_composite_key()
        {
            var migration = new TestMigration();

            migration.DropForeignKey("d", new[] { "dc1", "dc2" }, "p", new[] { "pc1", "pc2" });

            var dropForeignKeyOperation = migration.Operations.Cast<DropForeignKeyOperation>().Single();

            Assert.Equal("d", dropForeignKeyOperation.DependentTable);
            Assert.Equal("dc1", dropForeignKeyOperation.DependentColumns.First());
            Assert.Equal("dc2", dropForeignKeyOperation.DependentColumns.Last());
            Assert.Equal("p", dropForeignKeyOperation.PrincipalTable);
            Assert.Equal(dropForeignKeyOperation.DefaultName, dropForeignKeyOperation.Name);
        }

        [Fact]
        public void DropForeignKey_creates_drop_foreign_key_operation_with_name()
        {
            var migration = new TestMigration();

            migration.DropForeignKey("Foo", "fk");

            var dropForeignKeyOperation = migration.Operations.Cast<DropForeignKeyOperation>().Single();

            Assert.Equal("Foo", dropForeignKeyOperation.DependentTable);
            Assert.Equal("fk", dropForeignKeyOperation.Name);
        }

        [Fact]
        public void DropColumn_creates_drop_column_operation()
        {
            var migration = new TestMigration();

            migration.DropColumn("Customers", "OldColumn");

            var dropColumnOperation = migration.Operations.Cast<DropColumnOperation>().Single();

            Assert.Equal("Customers", dropColumnOperation.Table);
            Assert.Equal("OldColumn", dropColumnOperation.Name);
        }

        [Fact]
        public void Can_call_create_table_with_anonymous_arguments()
        {
            var migration = new TestMigration();

            migration.CreateTable(
                "Foo", cs => new
                {
                    Id = cs.Int()
                }, new
                {
                    Foo = 123
                });

            var createTableOperation = migration.Operations.Cast<CreateTableOperation>().Single();

            Assert.Equal(123, createTableOperation.AnonymousArguments["Foo"]);
        }

        [Fact]
        public void AddColumn_creates_add_column_operation_with_column_model()
        {
            var migration = new TestMigration();

            migration.AddColumn("Customers", "NewColumn", c => c.Byte(nullable: false));

            var addColumnOperation = migration.Operations.Cast<AddColumnOperation>().Single();

            Assert.Equal("Customers", addColumnOperation.Table);
            Assert.Equal("NewColumn", addColumnOperation.Column.Name);
            Assert.Equal(PrimitiveTypeKind.Byte, addColumnOperation.Column.Type);
            Assert.False(addColumnOperation.Column.IsNullable.Value);
        }

        [Fact]
        public void CreateTable_can_build_table_with_columns()
        {
            var migration = new TestMigration();

            migration.CreateTable(
                "Customers",
                cs => new
                {
                    Id = cs.Int(),
                    Name = cs.String()
                });

            var createTableOperation = migration.Operations.Cast<CreateTableOperation>().Single();

            Assert.Equal("Customers", createTableOperation.Name);
            Assert.Equal(2, createTableOperation.Columns.Count());

            var column = createTableOperation.Columns.First();

            Assert.Equal("Id", column.Name);
            Assert.Equal(PrimitiveTypeKind.Int32, column.Type);

            column = createTableOperation.Columns.Last();

            Assert.Equal("Name", column.Name);
            Assert.Equal(PrimitiveTypeKind.String, column.Type);
        }

        [Fact]
        public void CreateTable_can_build_table_with_custom_column_name()
        {
            var migration = new TestMigration();

            migration.CreateTable(
                "Customers",
                cs => new
                {
                    Id = cs.Int(name: "Customer Id")
                });

            var createTableOperation = migration.Operations.Cast<CreateTableOperation>().Single();

            var column = createTableOperation.Columns.Single();

            Assert.Equal("Customer Id", column.Name);
            Assert.Equal(PrimitiveTypeKind.Int32, column.Type);
        }

        [Fact]
        public void CreateTable_can_build_table_with_index()
        {
            var migration = new TestMigration();

            migration.CreateTable(
                "Customers",
                cs => new
                {
                    Id = cs.Int(),
                    Name = cs.String()
                })
                     .Index(
                         t => new
                         {
                             t.Id,
                             t.Name
                         }, unique: true, clustered: true);

            var createIndexOperation = migration.Operations.OfType<CreateIndexOperation>().Single();

            Assert.NotNull(createIndexOperation.Table);
            Assert.Equal(2, createIndexOperation.Columns.Count());
            Assert.True(createIndexOperation.IsUnique);
            Assert.True(createIndexOperation.IsClustered);
        }

        [Fact]
        public void DropTable_should_add_drop_table_operation()
        {
            var migration = new TestMigration();

            migration.DropTable("Customers");

            var dropTableOperation = migration.Operations.Cast<DropTableOperation>().Single();

            Assert.NotNull(dropTableOperation);
            Assert.Equal("Customers", dropTableOperation.Name);
        }

        [Fact]
        public void RenameTable_should_add_rename_table_operation()
        {
            var migration = new TestMigration();

            migration.RenameTable("old", "new");

            var renameTableOperation = migration.Operations.Cast<RenameTableOperation>().Single();

            Assert.NotNull(renameTableOperation);
            Assert.Equal("old", renameTableOperation.Name);
            Assert.Equal("new", renameTableOperation.NewName);
        }

        [Fact]
        public void RenameColumn_should_add_rename_column_operation()
        {
            var migration = new TestMigration();

            migration.RenameColumn("table", "old", "new");

            var renameColumnOperation = migration.Operations.Cast<RenameColumnOperation>().Single();

            Assert.NotNull(renameColumnOperation);
            Assert.Equal("table", renameColumnOperation.Table);
            Assert.Equal("old", renameColumnOperation.Name);
            Assert.Equal("new", renameColumnOperation.NewName);
        }

        [Fact]
        public void CreateIndex_should_add_create_index_operation()
        {
            var migration = new TestMigration();

            migration.CreateIndex("table", new[] { "Foo", "Bar" }, true);

            var createIndexOperation = migration.Operations.Cast<CreateIndexOperation>().Single();

            Assert.Equal("table", createIndexOperation.Table);
            Assert.Equal("Foo", createIndexOperation.Columns.First());
            Assert.Equal("Bar", createIndexOperation.Columns.Last());
            Assert.True(createIndexOperation.IsUnique);
            Assert.False(createIndexOperation.IsClustered);
        }

        [Fact]
        public void CreateIndex_can_set_clustered_parameter()
        {
            var migration = new TestMigration();

            migration.CreateIndex("table", new[] { "Foo", "Bar" }, clustered: true);

            var createIndexOperation = migration.Operations.Cast<CreateIndexOperation>().Single();

            Assert.True(createIndexOperation.IsClustered);
        }

        [Fact]
        public void Sql_should_add_sql_operation()
        {
            var migration = new TestMigration();

            migration.Sql("foo");

            var sqlOperation = migration.Operations.Cast<SqlOperation>().Single();

            Assert.Equal("foo", sqlOperation.Sql);
        }

        [Fact]
        public void Explictly_calling_IAddMigrationOperation_should_add_operation()
        {
            IAddMigrationOperation migration = new TestMigration();

            migration.AddOperation(new SqlOperation("foo"));

            var sqlOperation = ((TestMigration)migration).Operations.Cast<SqlOperation>().Single();

            Assert.Equal("foo", sqlOperation.Sql);
        }
    }
}
