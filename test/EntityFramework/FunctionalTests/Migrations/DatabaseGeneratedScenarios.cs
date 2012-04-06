namespace System.Data.Entity.Migrations
{
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    // TODO: SDE Merge - No CE Provider
    //[Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class DatabaseGeneratedScenarios : DbTestCase
    {
        [MigrationsTheory]
        public void Can_auto_migrate_when_string_column_with_identity_database_generated_option()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v4>();

            migrator.Update();
        }
    }
}