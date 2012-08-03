// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class CreateTableScenarios : DbTestCase
    {
        private class CreateOobTableFkMigration : DbMigration
        {
            public override void Up()
            {
                CreateTable(
                    "Oob_Principal", t => new
                        {
                            Id = t.Int()
                        })
                    .PrimaryKey(t => t.Id);

                CreateTable(
                    "Oob_Dependent", t => new
                        {
                            Id = t.Int(),
                            Fk = t.Int()
                        })
                    .ForeignKey("Oob_Principal", t => t.Fk);
            }
        }

        [MigrationsTheory]
        public void Can_create_oob_table_with_inline_fk()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new CreateOobTableFkMigration());

            migrator.Update();

            var principalTable = Info.Tables.SingleOrDefault(t => t.Name == "Oob_Principal");
            Assert.NotNull(principalTable);
            Assert.Equal(1, principalTable.Columns.Count());
            Assert.True(principalTable.Columns.Any(c => c.Name == "Id" && c.Type == "int"));
            var principalPrimaryKey = principalTable.Constraints.OfType<PrimaryKeyConstraintInfo>().SingleOrDefault();
            Assert.NotNull(principalPrimaryKey);
            Assert.Equal(1, principalPrimaryKey.KeyColumnUsages.Count());
            Assert.True(principalPrimaryKey.KeyColumnUsages.Any(kcu => kcu.ColumnName == "Id"));
            var dependentTable = Info.Tables.SingleOrDefault(t => t.Name == "Oob_Dependent");
            Assert.Equal(2, dependentTable.Columns.Count());
            Assert.True(dependentTable.Columns.Any(c => c.Name == "Id" && c.Type == "int"));
            Assert.True(dependentTable.Columns.Any(c => c.Name == "Fk" && c.Type == "int"));
            var foreignKey = dependentTable.Constraints.OfType<ReferentialConstraintInfo>().SingleOrDefault();
            Assert.NotNull(foreignKey);
            Assert.Equal(1, foreignKey.KeyColumnUsages.Count());
            Assert.True(foreignKey.KeyColumnUsages.Any(kcu => kcu.ColumnName == "Fk"));
            Assert.Equal(1, foreignKey.UniqueConstraint.KeyColumnUsages.Count());
            Assert.True(
                foreignKey.UniqueConstraint.KeyColumnUsages.Any(kcu => kcu.ColumnTableName == "Oob_Principal" && kcu.ColumnName == "Id"));
        }

        private class CreateOobTableInvalidFkMigration : DbMigration
        {
            public override void Up()
            {
                CreateTable(
                    "Oob_Dependent", t => new
                        {
                            Id = t.Int(),
                            Fk = t.Int()
                        })
                    .ForeignKey("Oob_Principal", t => t.Fk);
            }
        }

        [MigrationsTheory]
        public void Throws_on_create_oob_table_with_invalid_fk()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new CreateOobTableInvalidFkMigration());

            Assert.Equal(
                Strings.PartialFkOperation("Oob_Dependent", "Fk"), Assert.Throws<MigrationsException>(() => migrator.Update()).Message);
        }

        private class CreateCustomColumnNameMigration : DbMigration
        {
            public override void Up()
            {
                CreateTable(
                    "Foo", t => new
                        {
                            Id = t.Int(name: "12 Foo Id")
                        });
            }
        }

        [MigrationsTheory]
        public void Can_create_table_with_custom_column_name()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new CreateCustomColumnNameMigration());

            migrator.Update();

            Assert.True(ColumnExists("Foo", "12 Foo Id"));
        }
    }
}
