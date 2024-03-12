// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
#if NET452
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
#endif
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class BasicMigrationScenarios : DbTestCase
    {
        public BasicMigrationScenarios(DatabaseProviderFixture databaseProviderFixture)
            : base(databaseProviderFixture)
        {
        }

        private class ErrorContext : DbContext
        {
            public DbSet<ErrorEntity> Entities { get; set; }
        }

        private class ErrorEntity
        {
            public int Id { get; set; }
        }

        private class ErrorMigration : DbMigration
        {
            public override void Up()
            {
                Sql("Bluth's Frozen Bananas");
            }
        }

#if NET452
        [MigrationsTheory]
        public void Database_not_deleted_when_at_least_one_good_migration()
        {
            DropDatabase();

            var migrator = CreateMigrator<ErrorContext>(new ErrorMigration());

            try
            {
                migrator.Update();
            }
            catch
            {
                Assert.True(DatabaseExists());
            }
        }
#endif

        [MigrationsTheory]
        public void GetHistory_should_return_migrations_list()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            Assert.True(!migrator.GetDatabaseMigrations().Any());

            migrator.Update();

            Assert.Equal(1, migrator.GetDatabaseMigrations().Count());
        }

        [MigrationsTheory]
        public void Generate_should_create_custom_migration_step()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            Assert.NotNull(generatedMigration);
            Assert.True(generatedMigration.MigrationId.Contains("Migration"));
        }

#if NET452
        [MigrationsTheory]
        public void Generate_should_emit_null_source_when_last_migration_was_explicit()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration1");

            migrator = CreateMigrator<ShopContext_v1>(scaffoldedMigrations: generatedMigration);

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v2>();

            generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration2");

            Assert.True(
                generatedMigration.DesignerCode.Contains("return null")
                || generatedMigration.DesignerCode.Contains("Return Nothing"));
        }
#endif

        [MigrationsTheory]
        public void Generate_should_emit_source_when_last_migration_was_automatic()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v2>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration2");

            Assert.True(
                generatedMigration.DesignerCode
                                  .Contains("Resources.GetString(\"Source\")"));
        }

#if NET452
        [MigrationsTheory]
        public void Update_should_execute_pending_custom_scripts()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            migrator = CreateMigrator<ShopContext_v1>(
                automaticMigrationsEnabled: false,
                scaffoldedMigrations: generatedMigration);

            migrator.Update();

            Assert.True(TableExists("MigrationsCustomers"));
        }
#endif

        [MigrationsTheory]
        public void Generate_when_model_up_to_date_should_create_stub_migration()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            Assert.True(generatedMigration.UserCode.Length > 300);
        }

#if NET452
        [MigrationsTheory]
        public void Update_down_when_target_migration_id_valid_should_migrate_to_target_version_without_timestamp_part()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v2>();

            var generatedMigration1 = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration1");

            migrator = CreateMigrator<ShopContext_v2>(
                automaticMigrationsEnabled: false,
                scaffoldedMigrations: generatedMigration1);

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v3>();

            var generatedMigration2 = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration2");

            migrator = CreateMigrator<ShopContext_v3>(
                automaticMigrationsEnabled: false,
                scaffoldedMigrations: new[] { generatedMigration1, generatedMigration2 });

            migrator.Update();

            Assert.True(TableExists("MigrationsStores"));

            migrator.Update("Migration1");

            Assert.True(TableExists("crm.tbl_customers"));
        }

        [MigrationsTheory]
        public void Can_specify_target_up_migration_without_timestamp_part()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            migrator = CreateMigrator<ShopContext_v1>(
                automaticMigrationsEnabled: false,
                scaffoldedMigrations: generatedMigration);

            migrator.Update("Migration");

            Assert.True(TableExists("MigrationsCustomers"));
        }

        [MigrationsTheory]
        public void Update_when_target_migration_id_invalid_should_throw()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            migrator = CreateMigrator<ShopContext_v1>(
                automaticMigrationsEnabled: false,
                scaffoldedMigrations: generatedMigration);

            migrator.Update();

            Assert.Throws<MigrationsException>(() => migrator.Update("balony"))
                  .ValidateMessage("MigrationNotFound", "balony");
        }

        [MigrationsTheory]
        public void Update_when_target_migration_id_valid_should_migrate_to_target_version()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            var generatedMigration1 = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration1");

            migrator = CreateMigrator<ShopContext_v2>();

            var generatedMigration2 = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration2");

            migrator = CreateMigrator<ShopContext_v2>(
                automaticMigrationsEnabled: false,
                scaffoldedMigrations: new[] { generatedMigration1, generatedMigration2 });

            migrator.Update(generatedMigration1.MigrationId);

            Assert.True(TableExists("MigrationsCustomers"));
        }

        [MigrationsTheory]
        public void Update_down_when_target_migration_id_valid_should_migrate_to_target_version()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v2>();

            var generatedMigration1 = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration1");

            migrator = CreateMigrator<ShopContext_v2>(
                automaticMigrationsEnabled: false,
                scaffoldedMigrations: generatedMigration1);

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v3>();

            var generatedMigration2 = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration2");

            migrator = CreateMigrator<ShopContext_v3>(
                automaticMigrationsEnabled: false,
                scaffoldedMigrations: generatedMigration2);

            migrator.Update();

            var generatedMigration3 = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration3");

            migrator = CreateMigrator<ShopContext_v3>(
                automaticMigrationsEnabled: false,
                scaffoldedMigrations: new[] { generatedMigration1, generatedMigration2, generatedMigration3 });

            migrator.Update();

            Assert.True(TableExists("MigrationsStores"));

            migrator.Update(generatedMigration1.MigrationId);

            Assert.True(TableExists("crm.tbl_customers"));
        }

        [MigrationsTheory]
        public void Update_down_when_initial_version_and_no_database_should_be_noop()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration1");

            migrator = CreateMigrator<ShopContext_v1>(
                automaticMigrationsEnabled: false,
                scaffoldedMigrations: generatedMigration);

            migrator.Update();

            Assert.True(TableExists("MigrationsCustomers"));

            DropDatabase();

            migrator.Update(DbMigrator.InitialDatabase);

            Assert.False(migrator.GetDatabaseMigrations().Any());
        }
#endif

        [MigrationsTheory]
        public void Generate_when_empty_source_database_should_diff_against_empty_model()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            Assert.Equal(4, Regex.Matches(generatedMigration.UserCode, "CreateTable").Count);
        }

#if NET452
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
#endif

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

#if NET452
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
#endif

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

            Assert.Throws<AutomaticDataLossException>(() => migrator.Update()).ValidateMessage("AutomaticDataLoss");
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

#if NET452
        [MigrationsTheory]
        public void Can_update_multiple_migrations_having_a_trailing_automatic_migration()
        {
            ResetDatabase();

            CreateMigrator<ShopContext_v2>().Update();

            var migrator = CreateMigrator<ShopContext_v3>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Version 2");

            ResetDatabase();

            CreateMigrator<ShopContext_v6>(
                automaticDataLossEnabled: true,
                scaffoldedMigrations: generatedMigration).Update();

            Assert.True(TableExists("MigrationsStores"));
        }

        [MigrationsTheory]
        public void Can_downgrade_with_leading_automatic_when_database_empty()
        {
            ResetDatabase();

            CreateMigrator<ShopContext_v2>().Update();

            var migrator = CreateMigrator<ShopContext_v3>();

            var scaffoldedMigration
                = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            migrator
                = CreateMigrator<ShopContext_v3>(
                    scaffoldedMigrations: scaffoldedMigration,
                    automaticDataLossEnabled: true);

            ResetDatabase();

            migrator.Update();

            Assert.True(TableExists("OrderLines"));

            migrator.Update("0");

            Assert.False(TableExists("OrderLines"));
        }

        [MigrationsTheory]
        public void Update_when_new_earlier_migration_should_throw_auto_disabled_exception()
        {
            ResetDatabase();

            var migratorA = CreateMigrator<MultiUserContextA>();
            var m1 = new MigrationScaffolder(migratorA.Configuration).Scaffold("M1");

            var migratorB = CreateMigrator<MultiUserContextB>();
            var m2 = new MigrationScaffolder(migratorB.Configuration).Scaffold("M2");

            CreateMigrator<MultiUserContextB>(scaffoldedMigrations: m2).Update();

            Assert.Throws<AutomaticMigrationsDisabledException>(
                () => CreateMigrator<MultiUserContextAB>(
                    scaffoldedMigrations: new[] { m1, m2 },
                    automaticMigrationsEnabled: false)
                          .Update());
        }
#endif

        [MigrationsTheory]
        public void ScaffoldInitialCreate_should_return_null_when_db_not_initialized()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            var migrationsScaffolder = new MigrationScaffolder(migrator.Configuration);
            migrationsScaffolder.Namespace = "Foo";
            var scaffoldedMigration = migrationsScaffolder.ScaffoldInitialCreate();

            Assert.Null(scaffoldedMigration);
        }

#if NET452
        [MigrationsTheory]
        public void ScaffoldInitialCreate_should_return_scaffolded_migration_when_db_initialized()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1b>();

            var initialCreate = new MigrationScaffolder(migrator.Configuration).Scaffold("InitialCreate");

            migrator = CreateMigrator<ShopContext_v1b>(scaffoldedMigrations: initialCreate, contextKey: typeof(ShopContext_v1b).FullName);

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1b>(contextKey: "NewOne");

            var migrationsScaffolder = new MigrationScaffolder(migrator.Configuration);
            migrationsScaffolder.Namespace = "Foo";
            var scaffoldedMigration = migrationsScaffolder.ScaffoldInitialCreate();

            Assert.NotNull(scaffoldedMigration);
            Assert.NotSame(initialCreate, scaffoldedMigration);
            Assert.Equal(initialCreate.MigrationId, scaffoldedMigration.MigrationId);

            WhenNotSqlCe(
                () => Assert.Contains("INSERT [dbo].[MigrationsCustomers]([CustomerNumber],", initialCreate.UserCode));
        }

        [MigrationsTheory]
        public void ScaffoldInitialCreate_should_return_scaffolded_migration_when_db_initialized_and_schema_specified()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v5>();

            var initialCreate = new MigrationScaffolder(migrator.Configuration).Scaffold("InitialCreate");

            migrator = CreateMigrator<ShopContext_v5>(scaffoldedMigrations: initialCreate, contextKey: typeof(ShopContext_v5).FullName);

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v5>(contextKey: "NewOne");

            var migrationsScaffolder = new MigrationScaffolder(migrator.Configuration);
            migrationsScaffolder.Namespace = "Foo";
            var scaffoldedMigration = migrationsScaffolder.ScaffoldInitialCreate();

            Assert.NotNull(scaffoldedMigration);
            Assert.NotSame(initialCreate, scaffoldedMigration);
            Assert.Equal(initialCreate.MigrationId, scaffoldedMigration.MigrationId);
        }

        [MigrationsTheory(SkipForLocalDb = true, Justification = "Test is too flaky.")]
        public void Update_blocks_automatic_migration_when_explicit_source_model()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v2>(automaticDataLossEnabled: true);

            migrator.Update();

            string migrationName = "Migration1";
            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold(migrationName);

            ResetDatabase();

            migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            // Fix-up migrationId to come after previous automatic migration
            var oldMigrationId = generatedMigration.MigrationId;
            var newMigrationId = GenerateUniqueMigrationName(migrationName);

            generatedMigration.MigrationId = newMigrationId;
            generatedMigration.DesignerCode = generatedMigration.DesignerCode.Replace(oldMigrationId, newMigrationId);

            migrator
                = CreateMigrator<ShopContext_v2>(
                    automaticMigrationsEnabled: false,
                    automaticDataLossEnabled: false,
                    scaffoldedMigrations: generatedMigration);

            Assert.Throws<AutomaticDataLossException>(() => migrator.Update())
                  .ValidateMessage("AutomaticDataLoss");
        }
#endif

        [MigrationsTheory]
        public void Update_down_when_automatic_should_migrate_to_target_version()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>(automaticDataLossEnabled: true);

            migrator.Update();

            Assert.True(TableExists("MigrationsCustomers"));

            migrator.Update(DbMigrator.InitialDatabase);

            Assert.False(TableExists("MigrationsCustomers"));

            AssertHistoryContextDoesNotExist();

            migrator.Update();

            Assert.True(TableExists("MigrationsCustomers"));
            AssertHistoryContextEntryExists("System.Data.Entity.Migrations.DbMigrationsConfiguration");
        }

        [MigrationsTheory]
        public void Update_down_when_automatic_and_multiple_steps_should_migrate_to_target_version()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v2>(automaticDataLossEnabled: true);

            migrator.Update();

            Assert.True(TableExists("crm.tbl_customers"));

            migrator = CreateMigrator<ShopContext_v3>(automaticDataLossEnabled: true);

            migrator.Update();

            Assert.True(TableExists("MigrationsStores"));

            migrator.Update(DbMigrator.InitialDatabase);

            Assert.False(TableExists("crm.tbl_customers"));
            Assert.False(TableExists("MigrationsStores"));

            AssertHistoryContextDoesNotExist();
        }

#if NET452
        [MigrationsTheory]
        public void Update_down_when_explicit_should_migrate_to_target_version()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration1");

            migrator = CreateMigrator<ShopContext_v1>(
                automaticMigrationsEnabled: false,
                scaffoldedMigrations: generatedMigration);

            migrator.Update();

            Assert.True(TableExists("MigrationsCustomers"));

            migrator.Update(DbMigrator.InitialDatabase);

            Assert.False(TableExists("MigrationsCustomers"));

            AssertHistoryContextDoesNotExist();
        }

        [MigrationsTheory]
        public void Update_down_when_explicit_and_automatic_should_migrate_to_target_version()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v2>();

            migrator.Update();

            Assert.True(TableExists("crm.tbl_customers"));

            migrator = CreateMigrator<ShopContext_v3>(automaticDataLossEnabled: true);

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            migrator = CreateMigrator<ShopContext_v3>(
                automaticDataLossEnabled: true,
                scaffoldedMigrations: generatedMigration);

            migrator.Update();

            Assert.True(TableExists("crm.tbl_customers"));

            migrator.Update(DbMigrator.InitialDatabase);

            Assert.False(TableExists("MigrationsStores"));
            Assert.False(TableExists("tbl_customers"));
            AssertHistoryContextDoesNotExist();
        }
#endif

        public class MultiUserContextA : DbContext
        {
            public DbSet<MultiUserA> As { get; set; }
        }

        public class MultiUserContextB : DbContext
        {
            public DbSet<MultiUserB> Bs { get; set; }
        }

        public class MultiUserContextAB : DbContext
        {
            public DbSet<MultiUserA> As { get; set; }
            public DbSet<MultiUserB> Bs { get; set; }
        }

        public class MultiUserA
        {
            public int Id { get; set; }
        }

        public class MultiUserB
        {
            public int Id { get; set; }
        }
    }
}
