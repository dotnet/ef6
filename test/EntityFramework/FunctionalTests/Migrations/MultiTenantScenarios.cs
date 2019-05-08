// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Common;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.History;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class MultiTenantScenarios : DbTestCase
    {
        public MultiTenantScenarios(DatabaseProviderFixture databaseProviderFixture)
            : base(databaseProviderFixture)
        {
        }

        public class TenantA
        {
            public int Id { get; set; }
        }

        public class TenantB
        {
            public int Id { get; set; }
        }

        public class ContextA : DbContext
        {
            public ContextA()
            {
            }

            public ContextA(DbConnection existingConnection, bool contextOwnsConnection)
                : base(existingConnection, contextOwnsConnection)
            {
            }

            public DbSet<TenantA> As { get; set; }
        }

        public class ContextA2 : ContextA
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.HasDefaultSchema("foo");
            }
        }

        public class ContextB : DbContext
        {
            public ContextB()
            {
            }

            public ContextB(DbConnection existingConnection, bool contextOwnsConnection)
                : base(existingConnection, contextOwnsConnection)
            {
            }

            public DbSet<TenantB> Bs { get; set; }
        }

        public class ContextB2 : ContextB
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.HasDefaultSchema("foo");
            }
        }

        [MigrationsTheory]
        public void Can_update_when_explicit_migrations()
        {
            ResetDatabase();

            var migratorA = CreateMigrator<ContextA>();

            var generatedMigrationA = new MigrationScaffolder(migratorA.Configuration).Scaffold("MigrationA");

            migratorA
                = CreateMigrator<ContextA>(
                    contextKey: "KeyA",
                    automaticMigrationsEnabled: false,
                    scaffoldedMigrations: generatedMigrationA);

            migratorA.Update();

            var migratorB = CreateMigrator<ContextB>();

            var generatedMigrationB = new MigrationScaffolder(migratorB.Configuration).Scaffold("MigrationB");

            migratorB
                = CreateMigrator<ContextB>(
                    contextKey: "KeyB",
                    automaticMigrationsEnabled: false,
                    scaffoldedMigrations: generatedMigrationB);

            migratorB.Update();

            Assert.True(TableExists("TenantAs"));
            Assert.True(TableExists("TenantBs"));

            migratorA.Update("0");
            migratorB.Update("0");

            Assert.False(TableExists("TenantAs"));
            Assert.False(TableExists("TenantBs"));
        }

        [MigrationsTheory]
        public void Can_update_when_auto_migrations()
        {
            ResetDatabase();

            var migratorA
                = CreateMigrator<ContextA>(contextKey: "KeyA", automaticDataLossEnabled: true);

            migratorA.Update();

            var migratorB
                = CreateMigrator<ContextB>(contextKey: "KeyB", automaticDataLossEnabled: true);

            migratorB.Update();

            Assert.True(TableExists("TenantAs"));
            Assert.True(TableExists("TenantBs"));

            migratorA.Update("0");
            migratorB.Update("0");

            Assert.False(TableExists("TenantAs"));
            Assert.False(TableExists("TenantBs"));
        }

        [MigrationsTheory]
        public void Can_initialize_with_create_if_not_exists()
        {
            ResetDatabase();

            Database.SetInitializer(new CreateDatabaseIfNotExists<ContextA>());
            Database.SetInitializer(new CreateDatabaseIfNotExists<ContextB>());

            using (var connection = TestDatabase.CreateConnection(TestDatabase.ConnectionString))
            {
                using (var context = new ContextA(connection, contextOwnsConnection: false))
                {
                    context.Database.Initialize(true);
                }

                using (var context = new ContextB(connection, contextOwnsConnection: false))
                {
                    context.Database.Initialize(true);
                }
            }

            Assert.True(TableExists("TenantAs"));
            Assert.True(TableExists("TenantBs"));
        }

        [MigrationsTheory]
        [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
        [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
        public void Can_update_when_default_schema_introduced()
        {
            ResetDatabase();

            var migratorA
                = CreateMigrator<ContextA>(contextKey: "KeyA");

            var generatedMigrationA1
                = new MigrationScaffolder(migratorA.Configuration).Scaffold("MigrationA");

            migratorA
                = CreateMigrator<ContextA>(
                    contextKey: "KeyA",
                    automaticMigrationsEnabled: false,
                    scaffoldedMigrations: generatedMigrationA1);

            migratorA.Update();

            Assert.True(TableExists("dbo.TenantAs"));
            Assert.True(TableExists("dbo." + HistoryContext.DefaultTableName));

            var migratorB
                = CreateMigrator<ContextB>(contextKey: "KeyB");

            var generatedMigrationB1
                = new MigrationScaffolder(migratorB.Configuration).Scaffold("MigrationB");

            migratorB
                = CreateMigrator<ContextB>(
                    contextKey: "KeyB",
                    automaticMigrationsEnabled: false,
                    scaffoldedMigrations: generatedMigrationB1);

            migratorB.Update();

            Assert.True(TableExists("dbo.TenantBs"));
            Assert.True(TableExists("dbo." + HistoryContext.DefaultTableName));

            migratorA
                = CreateMigrator<ContextA2>(
                    contextKey: "KeyA",
                    scaffoldedMigrations: generatedMigrationA1);

            var generatedMigrationA2
                = new MigrationScaffolder(migratorA.Configuration).Scaffold("MigrationA2");

            migratorA
                = CreateMigrator<ContextA2>(
                    contextKey: "KeyA",
                    automaticMigrationsEnabled: false,
                    scaffoldedMigrations: new[] { generatedMigrationA1, generatedMigrationA2 });

            migratorA.Update();

            Assert.True(TableExists("foo.TenantAs"));
            Assert.True(TableExists("foo." + HistoryContext.DefaultTableName));
            Assert.False(TableExists("dbo.TenantAs"));
            Assert.True(TableExists("dbo." + HistoryContext.DefaultTableName));

            migratorB
                = CreateMigrator<ContextB2>(
                    contextKey: "KeyB",
                    scaffoldedMigrations: generatedMigrationB1);

            var generatedMigrationB2
                = new MigrationScaffolder(migratorB.Configuration).Scaffold("MigrationB2");

            migratorB
                = CreateMigrator<ContextB2>(
                    contextKey: "KeyB",
                    automaticMigrationsEnabled: false,
                    scaffoldedMigrations: new[] { generatedMigrationB1, generatedMigrationB2 });

            migratorB.Update();

            Assert.True(TableExists("foo.TenantBs"));
            Assert.True(TableExists("foo." + HistoryContext.DefaultTableName));
            Assert.False(TableExists("dbo.TenantBs"));
            Assert.False(TableExists("dbo." + HistoryContext.DefaultTableName));

            migratorA.Update("0");

            Assert.False(TableExists("foo.TenantAs"));
            Assert.True(TableExists("foo." + HistoryContext.DefaultTableName));

            migratorB.Update("0");

            Assert.False(TableExists("foo.TenantBs"));
            Assert.False(TableExists("foo." + HistoryContext.DefaultTableName));
        }
    }
}
