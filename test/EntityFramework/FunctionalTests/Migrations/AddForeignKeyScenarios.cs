// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Migrations
{
    using System.Linq;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class AddForeignKeyScenarios : DbTestCase
    {
        private class AddSimpleForeignKeyMigration : DbMigration
        {
            public override void Up()
            {
                AddForeignKey("OrderLines", "OrderId", "ordering.Orders", "OrderId", name: "FK_Custom_Name");
            }
        }

        [MigrationsTheory]
        public void Can_add_simple_foreign_key_constraint()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AddSimpleForeignKeyMigration());

            migrator.Update();

            var foreignKey = Info.TableConstraints.OfType<ReferentialConstraintInfo>().SingleOrDefault(rc => rc.Name == "FK_Custom_Name");
            Assert.NotNull(foreignKey);
            Assert.Equal(1, foreignKey.KeyColumnUsages.Count());
            Assert.True(foreignKey.KeyColumnUsages.Any(kcu => kcu.Position == 1 && kcu.ColumnTableName == "OrderLines" && kcu.ColumnName == "OrderId"));
            Assert.Equal(1, foreignKey.UniqueConstraint.KeyColumnUsages.Count());
            var keyColumnUsage = foreignKey.UniqueConstraint.KeyColumnUsages.SingleOrDefault(kcu => kcu.Position == 1 && kcu.ColumnTableName == "Orders" && kcu.ColumnName == "OrderId");
            Assert.NotNull(keyColumnUsage);
            WhenNotSqlCe(() => Assert.Equal("ordering", keyColumnUsage.ColumnTableSchema));
        }

        private class AddCompositeForeignKeyMigration : DbMigration
        {
            public override void Up()
            {
                AddForeignKey("OrderLines", new[] { "ProductId", "Sku" }, "MigrationsProducts", new[] { "ProductId", "Sku" });
            }
        }

        [MigrationsTheory]
        public void Can_add_composite_foreign_key_constraint()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AddCompositeForeignKeyMigration());

            migrator.Update();

            var foreignKey = Info.TableConstraints.OfType<ReferentialConstraintInfo>().SingleOrDefault(rc => rc.Name == "FK_OrderLines_MigrationsProducts_ProductId_Sku");
            Assert.NotNull(foreignKey);
            Assert.Equal(2, foreignKey.KeyColumnUsages.Count());
            Assert.True(foreignKey.KeyColumnUsages.Any(kcu => kcu.Position == 1 && kcu.ColumnTableName == "OrderLines" && kcu.ColumnName == "ProductId"));
            Assert.True(foreignKey.KeyColumnUsages.Any(kcu => kcu.Position == 2 && kcu.ColumnTableName == "OrderLines" && kcu.ColumnName == "Sku"));
            Assert.Equal(2, foreignKey.UniqueConstraint.KeyColumnUsages.Count());
            Assert.True(foreignKey.UniqueConstraint.KeyColumnUsages.Any(kcu => kcu.Position == 1 && kcu.ColumnTableName == "MigrationsProducts" && kcu.ColumnName == "ProductId"));
            Assert.True(foreignKey.UniqueConstraint.KeyColumnUsages.Any(kcu => kcu.Position == 2 && kcu.ColumnTableName == "MigrationsProducts" && kcu.ColumnName == "Sku"));
        }

        private class AddForeignKeyNoPrincipalMigration : DbMigration
        {
            public override void Up()
            {
                AddForeignKey("OrderLines", new[] { "ProductId", "Sku" }, "MigrationsProducts");
            }
        }

        [MigrationsTheory]
        public void Can_add_composite_foreign_key_constraint_when_principal_columns_not_specified()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AddForeignKeyNoPrincipalMigration());

            migrator.Update();

            var foreignKey = Info.TableConstraints.OfType<ReferentialConstraintInfo>().SingleOrDefault(rc => rc.Name == "FK_OrderLines_MigrationsProducts_ProductId_Sku");
            Assert.NotNull(foreignKey);
            Assert.Equal(2, foreignKey.KeyColumnUsages.Count());
            Assert.True(foreignKey.KeyColumnUsages.Any(kcu => kcu.Position == 1 && kcu.ColumnTableName == "OrderLines" && kcu.ColumnName == "ProductId"));
            Assert.True(foreignKey.KeyColumnUsages.Any(kcu => kcu.Position == 2 && kcu.ColumnTableName == "OrderLines" && kcu.ColumnName == "Sku"));
            Assert.Equal(2, foreignKey.UniqueConstraint.KeyColumnUsages.Count());
            Assert.True(foreignKey.UniqueConstraint.KeyColumnUsages.Any(kcu => kcu.Position == 1 && kcu.ColumnTableName == "MigrationsProducts" && kcu.ColumnName == "ProductId"));
            Assert.True(foreignKey.UniqueConstraint.KeyColumnUsages.Any(kcu => kcu.Position == 2 && kcu.ColumnTableName == "MigrationsProducts" && kcu.ColumnName == "Sku"));
        }
    }
}
