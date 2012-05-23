namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.IO;
    using System.Text.RegularExpressions;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class BasicMigrationScenarios : DbTestCase
    {
        [MigrationsTheory]
        public void Generate_when_empty_source_database_should_diff_against_empty_model()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            Assert.Equal(4, Regex.Matches(generatedMigration.UserCode, "CreateTable").Count);
        }

        [MigrationsTheory]
        public void Can_generate_and_update_against_empty_source_model()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration_v1");

            migrator = CreateMigrator<ShopContext_v1>(false, scaffoldedMigrations: generatedMigration);

            migrator.Update();

            Assert.True(TableExists("MigrationsProducts"));
        }

        [MigrationsTheory]
        public void Can_generate_against_existing_model()
        {
            Can_generate_and_update_against_empty_source_model();

            var migrator = CreateMigrator<ShopContext_v2>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration_v2");

            Assert.Equal(2, Regex.Matches(generatedMigration.UserCode, "RenameTable").Count);
        }

        [MigrationsTheory]
        public void Can_generate_migration_with_store_side_renames()
        {
            ResetDatabase();

            CreateMigrator<ShopContext_v1>().Update();

            var migrator = CreateMigrator<ShopContext_v2>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            Assert.True(generatedMigration.UserCode.Contains("RenameTable"));
            WhenNotSqlCe(() => Assert.True(generatedMigration.UserCode.Contains("RenameColumn")));
        }

        [MigrationsTheory]
        public void Can_update_generate_update_when_empty_target_database()
        {
            ResetDatabase();

            CreateMigrator<ShopContext_v1>().Update();

            Assert.True(TableExists("MigrationsProducts"));

            var migrator = CreateMigrator<ShopContext_v2>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            migrator = CreateMigrator<ShopContext_v2>(false, scaffoldedMigrations: generatedMigration);

            migrator.Update();

            Assert.True(TableExists("crm.tbl_customers"));
        }

        [MigrationsTheory]
        public void Can_auto_update_v1_when_target_database_does_not_exist()
        {
            var migrator = CreateMigrator<ShopContext_v1>(targetDatabase: Path.GetRandomFileName());

            try
            {
                migrator.Update();

                Assert.True(TableExists("MigrationsProducts"));
            }
            finally
            {
                DropDatabase();
            }
        }

        [MigrationsTheory]
        public void Update_throws_on_automatic_data_loss()
        {
            ResetDatabase();

            CreateMigrator<NonEmptyModel>().Update();

            var migrator = CreateMigrator<EmptyModel>();

            Assert.Equal(new AutomaticDataLossException("Automatic migration was not applied because it would result in data loss.").Message, Assert.Throws<AutomaticDataLossException>(() => migrator.Update()).Message);
        }

        [MigrationsTheory]
        public void Update_can_process_automatic_data_loss()
        {
            ResetDatabase();

            CreateMigrator<NonEmptyModel>().Update();

            var migrator = CreateMigrator<EmptyModel>(automaticDataLossEnabled: true);

            migrator.Update();

            Assert.False(TableExists("MigrationsBlogs"));
        }

        [MigrationsTheory]
        public void Can_update_multiple_migrations_having_a_trailing_automatic_migration()
        {
            ResetDatabase();

            CreateMigrator<ShopContext_v1>().Update();

            var migrator = CreateMigrator<ShopContext_v2>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Version 2");

            ResetDatabase();

            CreateMigrator<ShopContext_v3>(scaffoldedMigrations: generatedMigration).Update();

            Assert.True(TableExists("MigrationsStores"));
        }
    }
}
