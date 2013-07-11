// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using Xunit;

    public class CrossDatabaseScenarios : DbTestCase
    {
        public class CrossProviderContext_v1 : DbContext
        {
            public DbSet<MigrationsCustomer> Customers { get; set; }
            public DbSet<MigrationsProduct> Products { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Conventions.Remove<SqlCePropertyMaxLengthConvention>();

                base.OnModelCreating(modelBuilder);
            }
        }

        public class CrossProviderContext_v2 : CrossProviderContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<MigrationsCustomer>().Ignore(c => c.CustomerNumber);
            }
        }

        [MigrationsTheory]
        public void Can_scaffold_on_sql_server_and_run_on_ce()
        {
            DatabaseProvider = DatabaseProvider.SqlClient;

            ResetDatabase();

            var migrator = CreateMigrator<CrossProviderContext_v1>();

            var scaffoldedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            DatabaseProvider = DatabaseProvider.SqlServerCe;

            ResetDatabase();

            migrator = CreateMigrator<CrossProviderContext_v1>(scaffoldedMigrations: scaffoldedMigration);

            migrator.Update();

            Assert.True(TableExists("MigrationsProducts"));
        }

        [MigrationsTheory]
        public void Can_scaffold_on_ce_and_run_on_sql()
        {
            DatabaseProvider = DatabaseProvider.SqlServerCe;

            ResetDatabase();

            var migrator = CreateMigrator<CrossProviderContext_v1>();

            var scaffoldedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            DatabaseProvider = DatabaseProvider.SqlClient;

            ResetDatabase();

            migrator = CreateMigrator<CrossProviderContext_v1>(scaffoldedMigrations: scaffoldedMigration);

            migrator.Update();

            Assert.True(TableExists("MigrationsProducts"));
        }

        [MigrationsTheory]
        public void Can_scaffold_on_sql_and_run_on_ce_after_initial_auto()
        {
            DatabaseProvider = DatabaseProvider.SqlClient;

            ResetDatabase();

            var migrator = CreateMigrator<CrossProviderContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<CrossProviderContext_v2>();

            var scaffoldedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            DatabaseProvider = DatabaseProvider.SqlServerCe;

            ResetDatabase();

            migrator = CreateMigrator<CrossProviderContext_v2>(scaffoldedMigrations: scaffoldedMigration);

            migrator.Update();

            Assert.True(TableExists("MigrationsProducts"));
            Assert.False(ColumnExists("MigrationsProducts", "CustomerNumber"));
        }
    }
}
