// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Common;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Infrastructure;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    public class CustomHistoryScenarios : DbTestCase
    {
        private class TestHistoryContextA : HistoryContext
        {
            public TestHistoryContextA(DbConnection existingConnection, string defaultSchema)
                : base(existingConnection, defaultSchema)
            {
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<HistoryRow>().ToTable("__Migrations");
                modelBuilder.Entity<HistoryRow>().Property(h => h.MigrationId).HasColumnName("_id");
                modelBuilder.Entity<HistoryRow>().Property(h => h.ContextKey).HasColumnName("_context_key");
                modelBuilder.Entity<HistoryRow>().Property(h => h.Model).HasColumnName("_model");
            }
        }

        private class TestHistoryContextB : HistoryContext
        {
            public TestHistoryContextB(DbConnection existingConnection, string defaultSchema)
                : base(existingConnection, defaultSchema)
            {
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<HistoryRow>().Property(h => h.Model).HasColumnName("metadata");
            }
        }

        private readonly HistoryContextFactory _testHistoryContextFactoryA =
            (existingConnection, defaultSchema) => new TestHistoryContextA(existingConnection, defaultSchema);

        private readonly HistoryContextFactory _testHistoryContextFactoryB =
            (existingConnection, defaultSchema) => new TestHistoryContextB(existingConnection, defaultSchema);

        [MigrationsTheory]
        public void Can_explicit_update_when_custom_history_factory()
        {
            ResetDatabase();

            var migrator
                = CreateMigrator<ShopContext_v1>(historyContextFactory: _testHistoryContextFactoryA);

            var generatedMigration
                = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            migrator
                = CreateMigrator<ShopContext_v1>(
                    automaticMigrationsEnabled: false,
                    scaffoldedMigrations: generatedMigration,
                    historyContextFactory: _testHistoryContextFactoryA);

            migrator.Update();

            Assert.True(TableExists("MigrationsCustomers"));
            Assert.True(TableExists("__Migrations"));

            migrator.Update("0");

            Assert.False(TableExists("MigrationsCustomers"));
            Assert.False(TableExists("__Migrations"));

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);

            Assert.Null(historyRepository.GetLastModel());
        }

        //[MigrationsTheory]
        // TODO: Re-enable when Migrations SPROC fluent APIS implemented.
        public void Can_auto_update_after_explicit_update_when_custom_history_factory()
        {
            ResetDatabase();

            var migrator
                = CreateMigrator<ShopContext_v1>(historyContextFactory: _testHistoryContextFactoryA);

            var generatedMigration
                = new MigrationScaffolder(migrator.Configuration)
                    .Scaffold("Migration_v1");

            migrator
                = CreateMigrator<ShopContext_v1>(
                    automaticMigrationsEnabled: false,
                    historyContextFactory: _testHistoryContextFactoryA,
                    scaffoldedMigrations: generatedMigration);

            migrator.Update();

            Assert.True(TableExists("dbo.OrderLines"));
            Assert.True(TableExists("__Migrations"));

            migrator
                = CreateMigrator<ShopContext_v2>(
                    automaticDataLossEnabled: true,
                    historyContextFactory: _testHistoryContextFactoryA,
                    scaffoldedMigrations: generatedMigration);

            migrator.Update();

            Assert.True(TableExists("crm.tbl_customers"));

            migrator.Update("0");

            Assert.False(TableExists("crm.tbl_customers"));
            Assert.False(TableExists("dbo.OrderLines"));
            Assert.False(TableExists("__Migrations"));
        }

        [MigrationsTheory]
        public void Can_explicit_update_after_explicit_update_when_custom_history_factory()
        {
            ResetDatabase();

            var migrator
                = CreateMigrator<ShopContext_v1>(historyContextFactory: _testHistoryContextFactoryA);

            var generatedMigrationA
                = new MigrationScaffolder(migrator.Configuration)
                    .Scaffold("Migration_v1");

            migrator
                = CreateMigrator<ShopContext_v1>(
                    automaticMigrationsEnabled: false,
                    historyContextFactory: _testHistoryContextFactoryA,
                    scaffoldedMigrations: generatedMigrationA);

            migrator.Update();

            Assert.True(TableExists("dbo.OrderLines"));
            Assert.True(TableExists("__Migrations"));

            migrator
                = CreateMigrator<ShopContext_v2>(
                    historyContextFactory: _testHistoryContextFactoryA,
                    scaffoldedMigrations: generatedMigrationA);

            var generatedMigrationB
                = new MigrationScaffolder(migrator.Configuration)
                    .Scaffold("Migration_v2");

            migrator
                = CreateMigrator<ShopContext_v2>(
                    automaticMigrationsEnabled: false,
                    automaticDataLossEnabled: true,
                    historyContextFactory: _testHistoryContextFactoryA,
                    scaffoldedMigrations: new[] { generatedMigrationA, generatedMigrationB });

            migrator.Update();

            Assert.True(TableExists("crm.tbl_customers"));

            migrator.Update("0");

            Assert.False(TableExists("crm.tbl_customers"));
            Assert.False(TableExists("dbo.OrderLines"));
            Assert.False(TableExists("__Migrations"));
        }

        [MigrationsTheory]
        public void Auto_update_when_initial_move_should_throw()
        {
            ResetDatabase();

            var migrator
                = CreateMigrator<ShopContext_v1>(historyContextFactory: _testHistoryContextFactoryA);

            Assert.Throws<MigrationsException>(() => migrator.Update())
                  .ValidateMessage("HistoryMigrationNotSupported");
        }

        [MigrationsTheory]
        public void Auto_update_when_initial_no_move_should_throw()
        {
            ResetDatabase();

            var migrator
                = CreateMigrator<ShopContext_v1>(historyContextFactory: _testHistoryContextFactoryB);

            Assert.Throws<MigrationsException>(() => migrator.Update())
                  .ValidateMessage("HistoryMigrationNotSupported");
        }

        [MigrationsTheory]
        public void Auto_update_when_factory_changed_move_should_throw()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator
                = CreateMigrator<ShopContext_v1>(historyContextFactory: _testHistoryContextFactoryA);

            Assert.Throws<MigrationsException>(() => migrator.Update())
                  .ValidateMessage("HistoryMigrationNotSupported");
        }

        [MigrationsTheory]
        public void Auto_update_when_factory_changed_no_move_should_throw()
        {
            ResetDatabase();

            var migrator
                = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator
                = CreateMigrator<ShopContext_v1>(historyContextFactory: _testHistoryContextFactoryB);

            Assert.Throws<MigrationsException>(() => migrator.Update())
                  .ValidateMessage("HistoryMigrationNotSupported");
        }

        [MigrationsTheory]
        public void Explicit_update_when_factory_changed_move_should_throw()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            var generatedMigrationA
                = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration A");

            migrator
                = CreateMigrator<ShopContext_v1>(
                    automaticMigrationsEnabled: false,
                    scaffoldedMigrations: generatedMigrationA);

            migrator.Update();

            var generatedMigrationB
                = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration B");

            migrator
                = CreateMigrator<ShopContext_v1>(
                    automaticMigrationsEnabled: false,
                    scaffoldedMigrations: new[] { generatedMigrationA, generatedMigrationB },
                    historyContextFactory: _testHistoryContextFactoryA);

            Assert.Throws<MigrationsException>(() => migrator.Update())
                  .ValidateMessage("HistoryMigrationNotSupported");
        }

        [MigrationsTheory]
        public void Explicit_update_when_factory_changed_no_move_should_throw()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            var generatedMigrationA
                = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration A");

            migrator
                = CreateMigrator<ShopContext_v1>(
                    automaticMigrationsEnabled: false,
                    scaffoldedMigrations: generatedMigrationA);

            migrator.Update();

            var generatedMigrationB
                = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration B");

            migrator
                = CreateMigrator<ShopContext_v1>(
                    automaticMigrationsEnabled: false,
                    scaffoldedMigrations: new[] { generatedMigrationA, generatedMigrationB },
                    historyContextFactory: _testHistoryContextFactoryB);

            Assert.Throws<MigrationsException>(() => migrator.Update())
                  .ValidateMessage("HistoryMigrationNotSupported");
        }

        [MigrationsTheory]
        public void Get_database_migrations_when_no_migrations_should_not_throw()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>(historyContextFactory: _testHistoryContextFactoryA);

            Assert.Empty(migrator.GetDatabaseMigrations());
        }

        [MigrationsTheory]
        public void Get_database_migrations_when_factory_introduced_after_auto_should_not_throw()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            Assert.True(TableExists("dbo." + HistoryContext.DefaultTableName));

            migrator = CreateMigrator<ShopContext_v1>(historyContextFactory: _testHistoryContextFactoryA);

            Assert.NotEmpty(migrator.GetDatabaseMigrations());
        }

        [MigrationsTheory]
        public void Get_database_migrations_when_factory_introduced_after_explicit_should_throw()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            var generatedMigration
                = new MigrationScaffolder(migrator.Configuration)
                    .Scaffold("Migration_v1");

            migrator
                = CreateMigrator<ShopContext_v1>(
                    automaticMigrationsEnabled: false,
                    scaffoldedMigrations: generatedMigration);

            migrator.Update();

            Assert.True(TableExists("dbo." + HistoryContext.DefaultTableName));

            migrator = CreateMigrator<ShopContext_v1>(
                historyContextFactory: _testHistoryContextFactoryA,
                scaffoldedMigrations: generatedMigration);

            Assert.Throws<MigrationsException>(() => migrator.GetDatabaseMigrations())
                  .ValidateMessage("HistoryMigrationNotSupported");
        }

        [MigrationsTheory]
        public void Get_pending_migrations_when_no_migrations_should_not_throw()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>(historyContextFactory: _testHistoryContextFactoryA);

            Assert.Empty(migrator.GetPendingMigrations());
        }

        [MigrationsTheory]
        public void Get_pending_migrations_when_factory_introduced_after_auto_should_not_throw()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            Assert.True(TableExists("dbo." + HistoryContext.DefaultTableName));

            migrator = CreateMigrator<ShopContext_v1>(historyContextFactory: _testHistoryContextFactoryA);

            Assert.Empty(migrator.GetPendingMigrations());
        }

        [MigrationsTheory]
        public void Get_pending_migrations_when_factory_introduced_after_explicit_should_throw()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            var generatedMigration
                = new MigrationScaffolder(migrator.Configuration)
                    .Scaffold("Migration_v1");

            migrator
                = CreateMigrator<ShopContext_v1>(
                    automaticMigrationsEnabled: false,
                    scaffoldedMigrations: generatedMigration);

            migrator.Update();

            Assert.True(TableExists("dbo." + HistoryContext.DefaultTableName));

            migrator = CreateMigrator<ShopContext_v1>(
                historyContextFactory: _testHistoryContextFactoryA,
                scaffoldedMigrations: generatedMigration);

            Assert.Throws<MigrationsException>(() => migrator.GetPendingMigrations())
                  .ValidateMessage("HistoryMigrationNotSupported");
        }
    }
}
