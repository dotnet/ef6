namespace System.Data.Entity.Migrations
{
    using System.Linq;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    // TODO: SDE Merge - No CE Provider
    //[Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class DropForeignKeyScenarios : DbTestCase
    {
        private class DropSimpleForeignKeyMigration : DbMigration
        {
            public override void Up()
            {
                DropForeignKey("OrderLines", "OrderId", "ordering.Orders");
            }
        }

        [MigrationsTheory]
        public void Can_drop_simple_foreign_key_constraint()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            Assert.True(Info.TableConstraints.Any(tc => tc.Name == "FK_OrderLines_ordering.Orders_OrderId"));

            migrator = CreateMigrator<ShopContext_v1>(new DropSimpleForeignKeyMigration());

            migrator.Update();

            Assert.False(Info.TableConstraints.Any(tc => tc.Name == "FK_OrderLines_ordering.Orders_OrderId"));
        }

        private class DropCompositeForeignKeyMigration : DbMigration
        {
            public override void Up()
            {
                AddForeignKey("OrderLines", new[] { "ProductId", "Sku" }, "MigrationsProducts", new[] { "ProductId", "Sku" });
                DropForeignKey("OrderLines", new[] { "ProductId", "Sku" }, "MigrationsProducts", new[] { "ProductId", "Sku" });
            }
        }

        [MigrationsTheory]
        public void Can_drop_composite_foreign_key_constraint()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new DropCompositeForeignKeyMigration());

            migrator.Update();

            Assert.False(Info.TableConstraints.Any(tc => tc.Name == "FK_OrderLines_Products_ProductId_Sku"));
        }

        private class DropForeignKeyNoPrincipalMigration : DbMigration
        {
            public override void Up()
            {
                AddForeignKey("OrderLines", new[] { "ProductId", "Sku" }, "MigrationsProducts");
                DropForeignKey("OrderLines", new[] { "ProductId", "Sku" }, "MigrationsProducts");
            }
        }

        [MigrationsTheory]
        public void Can_drop_composite_foreign_key_constraint_when_principal_columns_not_specified()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new DropForeignKeyNoPrincipalMigration());

            migrator.Update();

            Assert.False(Info.TableConstraints.Any(tc => tc.Name == "FK_OrderLines_Products_ProductId_Sku"));
        }

        private class DropForeignKeyWithName : DbMigration
        {
            public override void Up()
            {
                AddForeignKey("OrderLines", new[] { "ProductId", "Sku" }, "MigrationsProducts", name: "TheFK");
                DropForeignKey("OrderLines", "TheFK");
            }
        }

        [MigrationsTheory]
        public void Can_drop_composite_foreign_key_constraint_when_name_specified()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new DropForeignKeyWithName());

            migrator.Update();

            Assert.False(Info.TableConstraints.Any(tc => tc.Name == "TheFK"));
        }
    }
}
