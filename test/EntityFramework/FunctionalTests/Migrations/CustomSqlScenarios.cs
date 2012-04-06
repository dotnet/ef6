namespace System.Data.Entity.Migrations
{
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    // TODO: SDE Merge - No CE Provider
    //[Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class CustomSqlScenarios : DbTestCase
    {
        private class CustomSqlMigration : DbMigration
        {
            public override void Up()
            {
                Sql("CREATE TABLE [Foo](Id [int])");
            }
        }

        [MigrationsTheory]
        public void Can_update_when_migration_contains_custom_sql()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new CustomSqlMigration());

            migrator.Update();

            Assert.True(TableExists("Foo"));
        }
    }
}