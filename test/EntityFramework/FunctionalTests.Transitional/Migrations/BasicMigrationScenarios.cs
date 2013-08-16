// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Utilities;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class BasicMigrationScenarios : DbTestCase
    {
        [MigrationsTheory]
        public void ScaffoldInitialCreate_should_return_null_when_db_not_initialized()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            var scaffoldedMigration = migrator.ScaffoldInitialCreate("Foo");

            Assert.Null(scaffoldedMigration);
        }

        [MigrationsTheory]
        public void ScaffoldInitialCreate_should_return_scaffolded_migration_when_db_initialized()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1b>();

            var initialCreate = new MigrationScaffolder(migrator.Configuration).Scaffold("InitialCreate");

            migrator = CreateMigrator<ShopContext_v1b>(scaffoldedMigrations: initialCreate, contextKey: typeof(ShopContext_v1b).FullName);

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1b>(contextKey: "NewOne");

            var scaffoldedMigration = migrator.ScaffoldInitialCreate("Foo");

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

            var scaffoldedMigration = migrator.ScaffoldInitialCreate("Foo");

            Assert.NotNull(scaffoldedMigration);
            Assert.NotSame(initialCreate, scaffoldedMigration);
            Assert.Equal(initialCreate.MigrationId, scaffoldedMigration.MigrationId);
        }

        [MigrationsTheory]
        public void Update_blocks_automatic_migration_when_explicit_source_model()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v2>(automaticDataLossEnabled: true);

            migrator.Update();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration1");

            ResetDatabase();

            migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            // Fix-up migrationId to come after previous automatic migration
            var oldMigrationId = generatedMigration.MigrationId;
            var newMigrationId = MigrationAssembly.CreateMigrationId(oldMigrationId.MigrationName());
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

        [MigrationsTheory]
        public void Update_down_when_automatic_should_migrate_to_target_version()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>(automaticDataLossEnabled: true);

            migrator.Update();

            Assert.True(TableExists("MigrationsCustomers"));

            migrator.Update(DbMigrator.InitialDatabase);

            Assert.False(TableExists("MigrationsCustomers"));

            Assert.Null(
                new HistoryRepository(
                    ConnectionString, ProviderFactory,
                    "System.Data.Entity.Migrations.DbMigrationsConfiguration",
                    null,
                    HistoryContext.DefaultFactory).GetLastModel());

            migrator.Update();

            Assert.True(TableExists("MigrationsCustomers"));
            Assert.NotNull(
                new HistoryRepository(
                    ConnectionString, ProviderFactory,
                    "System.Data.Entity.Migrations.DbMigrationsConfiguration",
                    null,
                    HistoryContext.DefaultFactory).GetLastModel());
        }

        [MigrationsTheory]
        public void Update_down_when_automatic_and_multiple_steps_should_migrate_to_target_version()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null, HistoryContext.DefaultFactory);

            var migrator = CreateMigrator<ShopContext_v2>(automaticDataLossEnabled: true);

            migrator.Update();

            Assert.True(TableExists("crm.tbl_customers"));

            migrator = CreateMigrator<ShopContext_v3>(automaticDataLossEnabled: true);

            migrator.Update();

            Assert.True(TableExists("MigrationsStores"));

            migrator.Update(DbMigrator.InitialDatabase);

            Assert.False(TableExists("crm.tbl_customers"));
            Assert.False(TableExists("MigrationsStores"));
            Assert.Null(historyRepository.GetLastModel());
        }

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

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null, HistoryContext.DefaultFactory);

            Assert.Null(historyRepository.GetLastModel());
        }

        [MigrationsTheory]
        public void Update_down_when_explicit_and_automatic_should_migrate_to_target_version()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null, HistoryContext.DefaultFactory);

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
            Assert.Null(historyRepository.GetLastModel());
        }

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
