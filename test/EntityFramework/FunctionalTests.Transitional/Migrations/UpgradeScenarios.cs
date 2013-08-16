// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Utilities;
    using FunctionalTests.SimpleMigrationsModel;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class UpgradeScenarios : DbTestCase
    {
        public class Ef5MigrationsContext : DbContext
        {
            public DbSet<Blog> Blogs { get; set; }
        }

        public class Ef5MigrationsConfiguration : DbMigrationsConfiguration<Ef5MigrationsContext>
        {
            public Ef5MigrationsConfiguration()
            {
                MigrationsNamespace = "FunctionalTests.SimpleMigrationsModel";
            }
        }

        [MigrationsTheory]
        public void Can_upgrade_from_5_and_existing_code_migrations_still_work()
        {
            ResetDatabase();

            var migrationsConfiguration
                = new Ef5MigrationsConfiguration
                      {
                          TargetDatabase
                              = new DbConnectionInfo(ConnectionString, TestDatabase.ProviderName)
                      };

            var migrator = new DbMigrator(migrationsConfiguration);

            migrator.Update();

            Assert.True(TableExists("dbo.Blogs"));
            Assert.True(TableExists("dbo." + HistoryContext.DefaultTableName));

            migrator.Update("0");

            Assert.False(TableExists("dbo.Blogs"));
            Assert.False(TableExists("dbo." + HistoryContext.DefaultTableName));
        }

        [MigrationsTheory]
        public void Can_upgrade_from_5_and_existing_database_migrations_still_work()
        {
            ResetDatabase();

            var migrationsConfiguration
                = new Ef5MigrationsConfiguration
                      {
                          TargetDatabase
                              = new DbConnectionInfo(ConnectionString, TestDatabase.ProviderName)
                      };

            var migrator = new DbMigrator(migrationsConfiguration);

            migrator.Update();

            var historyRepository
                = new HistoryRepository(
                    ConnectionString, 
                    ProviderFactory, 
                    migrationsConfiguration.ContextKey, 
                    migrationsConfiguration.CommandTimeout,
                    HistoryContext.DefaultFactory);

            ExecuteOperations(
                new MigrationOperation[]
                    {
                        GetDropHistoryTableOperation(),
                        GetCreateHistoryTableOperation()
                    });

            var model = CreateContext<Ef5MigrationsContext>().GetModel();

            // create v5 history rows
            ExecuteOperations(
                new[]
                    {
                        historyRepository.CreateInsertOperation("201112202056275_InitialCreate", model),
                        historyRepository.CreateInsertOperation("201112202056573_AddUrlToBlog", model)
                    });

            migrator.Update("0");

            Assert.False(TableExists("dbo.Blogs"));
            Assert.False(TableExists("dbo." + HistoryContext.DefaultTableName));
        }

        [MigrationsTheory]
        public void Can_upgrade_from_5_and_existing_code_auto_migrations_still_work()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            var historyRepository
                = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null, HistoryContext.DefaultFactory);

            // create v5 history rows
            ExecuteOperations(
                new[]
                    {
                        GetDropHistoryTableOperation(),
                        GetCreateHistoryTableOperation(),
                        historyRepository
                            .CreateInsertOperation(
                                "201112202056275_NoHistoryModelAutomaticMigration",
                                CreateContext<ShopContext_v1>().GetModel())
                    });

            migrator = CreateMigrator<ShopContext_v2>();

            var scaffoldedMigration
                = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration_v2");

            ResetDatabase();

            migrator
                = CreateMigrator<ShopContext_v2>(
                    scaffoldedMigrations: scaffoldedMigration,
                    automaticDataLossEnabled: true);

            migrator.Update();

            Assert.True(TableExists("crm.tbl_customers"));
            Assert.True(TableExists("dbo." + HistoryContext.DefaultTableName));

            migrator.Update("0");

            Assert.False(TableExists("crm.tbl_customers"));
            Assert.False(TableExists("dbo." + HistoryContext.DefaultTableName));
        }
    }
}
