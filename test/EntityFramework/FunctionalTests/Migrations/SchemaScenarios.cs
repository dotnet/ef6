namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.History;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
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

        [MigrationsTheory]
        public void Can_update_when_custom_default_schema()
        {
            DropDatabase();

            var migrator = CreateMigrator<CustomSchemaContext>();

            migrator.Update();

            Assert.True(TableExists("foo.OrderLines"));
            Assert.True(TableExists("ordering.Orders"));
            Assert.True(TableExists("foo." + HistoryContext.TableName));
        }

        private class CustomSchemaContext2 : ShopContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.HasDefaultSchema("bar");
            }
        }

        [MigrationsTheory]
        public void Can_generate_and_update_when_custom_default_schema()
        {
            DropDatabase();

            var migrator = CreateMigrator<CustomSchemaContext2>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration_v1");

            migrator = CreateMigrator<CustomSchemaContext2>(false, scaffoldedMigrations: generatedMigration);

            migrator.Update();

            Assert.True(TableExists("bar.OrderLines"));
            Assert.True(TableExists("ordering.Orders"));
            Assert.True(TableExists("bar." + HistoryContext.TableName));
        }

        // TODO: [MigrationsTheory(Skip = "In progress")]
        public void Can_get_database_migrations_when_custom_default_schema_introduced()
        {
            DropDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            Assert.True(TableExists("dbo.OrderLines"));
            Assert.True(TableExists("ordering.Orders"));
            Assert.True(TableExists("dbo." + HistoryContext.TableName));

            migrator = CreateMigrator<CustomSchemaContext>();

            Assert.NotEmpty(migrator.GetDatabaseMigrations());
        }
    }
}
