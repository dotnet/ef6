namespace System.Data.Entity.Migrations
{
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class DropColumnScenarios : DbTestCase
    {
        private class DropColumnMigration : DbMigration
        {
            public override void Up()
            {
                DropColumn("MigrationsProducts", "Name");
            }
        }

        [MigrationsTheory]
        public void Can_drop_column()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            Assert.True(ColumnExists("MigrationsProducts", "Name"));

            migrator = CreateMigrator<ShopContext_v1>(new DropColumnMigration());

            migrator.Update();

            Assert.False(ColumnExists("MigrationsProducts", "Name"));
        }
    }
}
