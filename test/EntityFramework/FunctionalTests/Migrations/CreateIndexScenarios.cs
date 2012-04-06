namespace System.Data.Entity.Migrations
{

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    // TODO: SDE Merge - No CE Provider
    //[Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class CreateIndexScenarios : DbTestCase
    {
        private class CreateSimpleIndexMigration : DbMigration
        {
            public override void Up()
            {
                CreateIndex("OrderLines", "OrderId", unique: true, name: "IX_Custom_Name");
            }
        }

        [MigrationsTheory]
        public void Can_create_simple_index()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new CreateSimpleIndexMigration());

            migrator.Update();

            // TODO: Verify
        }

        private class CreateCompositeIndexMigration : DbMigration
        {
            public override void Up()
            {
                CreateIndex("OrderLines", new[] { "ProductId", "Sku" }, true);
            }
        }

        [MigrationsTheory]
        public void Can_create_composite_index()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1, CreateCompositeIndexMigration>();

            migrator.Update();

            // TODO: Verify
        }

        [MigrationsTheory]
        public void Bug_49966_should_not_generate_duplicate_foreign_keys()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ProcessedTransactionContext>();

            migrator.Update();
        }
    }
}