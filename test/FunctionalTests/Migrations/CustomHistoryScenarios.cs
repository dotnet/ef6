// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Common;
    using System.Data.Entity.Core;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.SqlClient;
    using System.Linq;
    using Xunit;

#if NETFRAMEWORK
    using System.Data.SqlServerCe;
#endif

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
#if NETFRAMEWORK
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
#endif
    public class CustomHistoryScenarios : DbTestCase
    {
#if NETFRAMEWORK
        private class NonStandardColumnWidthsContext : HistoryContext
        {
            public NonStandardColumnWidthsContext(DbConnection existingConnection, string defaultSchema)
                : base(existingConnection, defaultSchema)
            {
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<HistoryRow>().Property(h => h.MigrationId).HasMaxLength(20);
                modelBuilder.Entity<HistoryRow>().Property(h => h.ContextKey).HasMaxLength(10);
            }
        }

        [MigrationsTheory]
        public void Can_set_non_standard_history_column_lengths()
        {
            ResetDatabase();

            Func<DbConnection, string, HistoryContext> historyContextFactory = (c, s) => new NonStandardColumnWidthsContext(c, s);

            const string contextKey = "This context key is longer than 10 chars";

            var migrator
                = CreateMigrator<ShopContext_v1>(
                    contextKey: contextKey,
                    historyContextFactory: historyContextFactory);

            var generatedMigration
                = new MigrationScaffolder(migrator.Configuration)
                    .Scaffold("MigrationWithASomewhatLongName");

            migrator
                = CreateMigrator<ShopContext_v1>(
                    contextKey: contextKey,
                    automaticMigrationsEnabled: false,
                    scaffoldedMigrations: generatedMigration,
                    historyContextFactory: historyContextFactory);

            migrator.Update();

            var migrationIdColumn = TestDatabase.Info.Columns.Single(c => c.Name == "MigrationId");

            Assert.Equal(20, migrationIdColumn.MaxLength);

            var contextKeyColumn = TestDatabase.Info.Columns.Single(c => c.Name == "ContextKey");

            Assert.Equal(10, contextKeyColumn.MaxLength);

            migrator.Update("0");
        }
#endif

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

        private readonly Func<DbConnection, string, HistoryContext> _testHistoryContextFactoryA =
            (existingConnection, defaultSchema) => new TestHistoryContextA(existingConnection, defaultSchema);

        private readonly Func<DbConnection, string, HistoryContext> _testHistoryContextFactoryB =
            (existingConnection, defaultSchema) => new TestHistoryContextB(existingConnection, defaultSchema);

        public CustomHistoryScenarios(DatabaseProviderFixture databaseProviderFixture)
            : base(databaseProviderFixture)
        {
            databaseProviderFixture.DatabaseName = nameof(CustomHistoryScenarios);
        }

#if NETFRAMEWORK
        [MigrationsTheory]
        public void Can_auto_update_after_explicit_update_when_custom_history_factory()
        {
            ResetDatabase();

            var migrator
                = CreateMigrator<ShopContext_v2>(historyContextFactory: _testHistoryContextFactoryA);

            var generatedMigration
                = new MigrationScaffolder(migrator.Configuration)
                    .Scaffold("Migration_v1");

            migrator
                = CreateMigrator<ShopContext_v2>(
                    automaticMigrationsEnabled: false,
                    historyContextFactory: _testHistoryContextFactoryA,
                    scaffoldedMigrations: generatedMigration);

            migrator.Update();

            Assert.True(TableExists("dbo.OrderLines"));
            Assert.True(TableExists("__Migrations"));

            migrator
                = CreateMigrator<ShopContext_v3>(
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
                = CreateMigrator<ShopContext_v2>(historyContextFactory: _testHistoryContextFactoryA);

            var generatedMigrationA
                = new MigrationScaffolder(migrator.Configuration)
                    .Scaffold("Migration_v1");

            migrator
                = CreateMigrator<ShopContext_v2>(
                    automaticMigrationsEnabled: false,
                    historyContextFactory: _testHistoryContextFactoryA,
                    scaffoldedMigrations: generatedMigrationA);

            migrator.Update();

            Assert.True(TableExists("dbo.OrderLines"));
            Assert.True(TableExists("__Migrations"));

            migrator
                = CreateMigrator<ShopContext_v3>(
                    historyContextFactory: _testHistoryContextFactoryA,
                    scaffoldedMigrations: generatedMigrationA);

            var generatedMigrationB
                = new MigrationScaffolder(migrator.Configuration)
                    .Scaffold("Migration_v2");

            migrator
                = CreateMigrator<ShopContext_v3>(
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
#endif

        [MigrationsTheory]
        public void Auto_update_when_initial_move_should_not_throw()
        {
            ResetDatabase();

            var migrator
                = CreateMigrator<ShopContext_v1>(historyContextFactory: _testHistoryContextFactoryA);

            migrator.Update();

            Assert.True(TableExists("dbo.__Migrations"));
        }

        [MigrationsTheory]
        public void Auto_update_when_factory_changed_default_schema_changed_should_throw()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v5>();

            Assert.Throws<MigrationsException>(() => migrator.Update())
                .ValidateMessage("UnableToMoveHistoryTableWithAuto");
        }

        [MigrationsTheory]
        public void Auto_update_when_factory_changed_should_fail()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator
                = CreateMigrator<ShopContext_v1>(historyContextFactory: _testHistoryContextFactoryB);

            Assert.Throws<EntityCommandExecutionException>(() => migrator.Update());
        }

#if NETFRAMEWORK
        [MigrationsTheory]
        public void Explicit_update_when_factory_changed_move_should_fail()
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

            WhenNotSqlCe(() => Assert.Throws<SqlException>(() => migrator.Update()));
            WhenSqlCe(() => Assert.Throws<SqlCeException>(() => migrator.Update()));
        }

        [MigrationsTheory]
        public void Explicit_update_when_factory_changed_no_move_should_fail()
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

            Assert.Throws<EntityCommandExecutionException>(() => migrator.Update());
        }
#endif

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

            Assert.Empty(migrator.GetDatabaseMigrations());
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

#if NETFRAMEWORK
        [MigrationsTheory]
        public void Can_use_per_provider_factory()
        {
            ResetDatabase();

            try
            {
                MutableResolver.AddResolver<Func<DbConnection, string, HistoryContext>>(_ => _testHistoryContextFactoryA);

                var migrator = CreateMigrator<ShopContext_v1>();

                var generatedMigration
                    = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

                migrator
                    = CreateMigrator<ShopContext_v1>(
                        automaticMigrationsEnabled: false,
                        scaffoldedMigrations: generatedMigration);

                migrator.Update();

                Assert.True(TableExists("MigrationsCustomers"));
                Assert.True(TableExists("__Migrations"));

                migrator.Update("0");

                Assert.False(TableExists("MigrationsCustomers"));
                Assert.False(TableExists("__Migrations"));

                AssertHistoryContextDoesNotExist();
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }
        }

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

            AssertHistoryContextDoesNotExist();
        }
#endif
    }
}
