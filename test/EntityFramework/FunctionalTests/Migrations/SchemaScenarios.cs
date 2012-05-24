namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.History;
    using Xunit;

    //[Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    // TODO: SDE Merge - No CE Provider
    //[Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    //[Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class SchemaScenarios : DbTestCase
    {
        private class CustomSchemaContext : ShopContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.HasDefaultSchema("foo");
            }
        }

        //[MigrationsTheory]
        public void Can_update_when_custom_default_schema()
        {
            DropDatabase();

            var migrator = CreateMigrator<CustomSchemaContext>();

            migrator.Update();

            Assert.True(TableExists("foo.OrderLines"));
            Assert.True(TableExists("ordering.Orders"));
            Assert.True(TableExists("foo." + HistoryContext.TableName));
        }

        //[MigrationsTheory]
        public void Can_generate_and_update_when_custom_default_schema()
        {
            DropDatabase();

            var migrator = CreateMigrator<CustomSchemaContext>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration_v1");

            migrator = CreateMigrator<CustomSchemaContext>(false, scaffoldedMigrations: generatedMigration);

            migrator.Update();

            Assert.True(TableExists("foo.OrderLines"));
            Assert.True(TableExists("ordering.Orders"));
            Assert.True(TableExists("foo." + HistoryContext.TableName));
        }
    }
}
