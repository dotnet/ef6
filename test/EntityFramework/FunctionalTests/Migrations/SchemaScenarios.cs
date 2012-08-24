// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Resources;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class SchemaScenarios : DbTestCase
    {
        private class CustomSchemaContext_v1 : ShopContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.HasDefaultSchema("foo");
            }
        }

        private class CustomSchemaContext_v1b : CustomSchemaContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<MigrationsCustomer>().ToTable("tbl_customers", "crm");
            }
        }

        private class CustomSchemaContext_v2 : ShopContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.HasDefaultSchema("bar");
            }
        }

        [MigrationsTheory]
        public void Can_generate_and_update_when_custom_default_schemas()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            var generatedMigration0 = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration_v0");

            migrator = CreateMigrator<ShopContext_v1>(false, scaffoldedMigrations: generatedMigration0);

            migrator.Update();

            Assert.True(TableExists("dbo.OrderLines"));
            Assert.True(TableExists("ordering.Orders"));
            Assert.True(TableExists("dbo." + HistoryContext.TableName));

            migrator = CreateMigrator<CustomSchemaContext_v1>();

            var generatedMigration1 = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration_v1");

            migrator = CreateMigrator<CustomSchemaContext_v1>(
                false, scaffoldedMigrations: new [] { generatedMigration0, generatedMigration1 });

            migrator.Update();

            WhenNotSqlCe(
                () =>
                {
                    Assert.False(TableExists("dbo.OrderLines"));
                    Assert.False(TableExists("dbo." + HistoryContext.TableName));
                });

            Assert.True(TableExists("foo.OrderLines"));
            Assert.True(TableExists("ordering.Orders"));
            Assert.True(TableExists("foo." + HistoryContext.TableName));

            migrator = CreateMigrator<CustomSchemaContext_v2>(scaffoldedMigrations: generatedMigration1);

            var generatedMigration2 = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration_v2");

            migrator
                = CreateMigrator<CustomSchemaContext_v2>(
                    false, scaffoldedMigrations: new[] { generatedMigration0, generatedMigration1, generatedMigration2 });

            migrator.Update();

            WhenNotSqlCe(
                () =>
                    {
                        Assert.False(TableExists("foo.OrderLines"));
                        Assert.False(TableExists("foo." + HistoryContext.TableName));
                    });

            Assert.True(TableExists("bar.OrderLines"));
            Assert.True(TableExists("ordering.Orders"));
            Assert.True(TableExists("bar." + HistoryContext.TableName));

            migrator.Update("0");

            Assert.False(TableExists("foo.OrderLines"));
            Assert.False(TableExists("ordering.Orders"));
            Assert.False(TableExists("foo." + HistoryContext.TableName));
        }

        [MigrationsTheory]
        public void Can_generate_and_update_clean_database_when_custom_default_schemas()
        {
            ResetDatabase();

            var migrator = CreateMigrator<CustomSchemaContext_v1>();

            var generatedMigration1 = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration_v1");

            migrator = CreateMigrator<CustomSchemaContext_v1>(false, scaffoldedMigrations: generatedMigration1);

            migrator.Update();

            Assert.True(TableExists("foo.OrderLines"));
            Assert.True(TableExists("ordering.Orders"));
            Assert.True(TableExists("foo." + HistoryContext.TableName));

            migrator = CreateMigrator<CustomSchemaContext_v2>(scaffoldedMigrations: generatedMigration1);

            var generatedMigration2 = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration_v2");

            migrator
                = CreateMigrator<CustomSchemaContext_v2>(
                    false, scaffoldedMigrations: new[] { generatedMigration1, generatedMigration2 });

            ResetDatabase();

            migrator.Update();

            WhenNotSqlCe(
                () =>
                    {
                        Assert.False(TableExists("foo.OrderLines"));
                        Assert.False(TableExists("foo." + HistoryContext.TableName));
                    });

            Assert.True(TableExists("bar.OrderLines"));
            Assert.True(TableExists("ordering.Orders"));
            Assert.True(TableExists("bar." + HistoryContext.TableName));

            migrator.Update("0");

            Assert.False(TableExists("foo.OrderLines"));
            Assert.False(TableExists("ordering.Orders"));
            Assert.False(TableExists("foo." + HistoryContext.TableName));
        }

        [MigrationsTheory]
        public void Can_auto_update_after_custom_default_schema_introduced()
        {
            ResetDatabase();

            var migrator = CreateMigrator<CustomSchemaContext_v1>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration_v1");

            migrator = CreateMigrator<CustomSchemaContext_v1>(false, scaffoldedMigrations: generatedMigration);

            migrator.Update();

            Assert.True(TableExists("foo.OrderLines"));
            Assert.True(TableExists("ordering.Orders"));
            Assert.True(TableExists("foo." + HistoryContext.TableName));

            migrator = CreateMigrator<CustomSchemaContext_v1b>(scaffoldedMigrations: generatedMigration);

            migrator.Update();

            Assert.True(TableExists("crm.tbl_customers"));

            migrator.Update("0");

            Assert.False(TableExists("crm.tbl_customers"));
            Assert.False(TableExists("foo.OrderLines"));
            Assert.False(TableExists("ordering.Orders"));
            Assert.False(TableExists("foo." + HistoryContext.TableName));
        }

        [MigrationsTheory]
        public void Can_auto_update_before_custom_default_schema_introduced()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            Assert.True(TableExists("dbo." + HistoryContext.TableName));

            migrator = CreateMigrator<CustomSchemaContext_v1>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration_v1");

            migrator = CreateMigrator<CustomSchemaContext_v1>(scaffoldedMigrations: generatedMigration, automaticDataLossEnabled: true);

            migrator.Update();

            WhenNotSqlCe(
                () =>
                    {
                        Assert.True(TableExists("foo." + HistoryContext.TableName));
                        Assert.False(TableExists("dbo." + HistoryContext.TableName));
                    });

            migrator.Update("0");

            Assert.False(TableExists("foo." + HistoryContext.TableName));
            Assert.False(TableExists("dbo." + HistoryContext.TableName));
        }

        [MigrationsTheory]
        public void Auto_update_when_custom_default_schema_should_throw()
        {
            ResetDatabase();

            var migrator = CreateMigrator<CustomSchemaContext_v1>();

            Assert.Equal(
                Strings.UnableToAutoMigrateDefaultSchema,
                Assert.Throws<MigrationsException>(() => migrator.Update()).Message);
        }

        [MigrationsTheory]
        public void Auto_update_when_custom_default_schema_introduced_should_throw()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<CustomSchemaContext_v1>();

            Assert.Equal(
                Strings.UnableToAutoMigrateDefaultSchema,
                Assert.Throws<MigrationsException>(() => migrator.Update()).Message);
        }

        [MigrationsTheory]
        public void Can_get_database_migrations_when_custom_default_schema_introduced()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            Assert.True(TableExists("dbo." + HistoryContext.TableName));

            migrator = CreateMigrator<CustomSchemaContext_v1>();

            Assert.NotEmpty(migrator.GetDatabaseMigrations());
        }
    }
}
