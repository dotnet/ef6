// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Common;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.History;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    public class CustomHistoryScenarios : DbTestCase
    {
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

                var historyRepository = new HistoryRepository(
                    ConnectionString, 
                    ProviderFactory, 
                    "MyKey",
                    null,
                    _testHistoryContextFactoryA);

                Assert.Null(historyRepository.GetLastModel());
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }
        }

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

        private readonly Func<DbConnection, string, HistoryContext> _testHistoryContextFactoryA =
            (existingConnection, defaultSchema) => new TestHistoryContextA(existingConnection, defaultSchema);

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

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null, HistoryContext.DefaultFactory);

            Assert.Null(historyRepository.GetLastModel());
        }
    }
}
