namespace System.Data.Entity.Migrations
{

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    // TODO: SDE Merge - No CE Provider
    //[Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class DropIndexScenarios : DbTestCase
    {
        private class DropSimpleIndexMigration : DbMigration
        {
            public override void Up()
            {
                CreateIndex("OrderLines", "OrderId");
                DropIndex("OrderLines", new[] { "OrderId" });
            }
        }

        [MigrationsTheory]
        public void Can_drop_simple_index()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1, DropSimpleIndexMigration>();

            migrator.Update();

            // TODO: Verify
        }

        private class DropCompositeIndexMigration : DbMigration
        {
            public override void Up()
            {
                CreateIndex("OrderLines", new[] { "ProductId", "Sku" });
                DropIndex("OrderLines", new[] { "ProductId", "Sku" });
            }
        }

        [MigrationsTheory]
        public void Can_drop_composite_index()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1, DropCompositeIndexMigration>();

            migrator.Update();

            // TODO: Verify
        }

        private class DropIndexWithName : DbMigration
        {
            public override void Up()
            {
                CreateIndex("OrderLines", new[] { "ProductId", "Sku" }, false, name: "TheIndex");
                DropIndex("OrderLines", "TheIndex");
            }
        }

        [MigrationsTheory]
        public void Can_drop_composite_index_when_name_specified()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1, DropIndexWithName>();

            migrator.Update();

            // TODO: Verify
        }
    }
}