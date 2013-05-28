// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.Infrastructure;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class ScriptingScenarios : DbTestCase
    {
        private string CreateMetadataStatement
        {
            get
            {
                if (DatabaseProvider == DatabaseProvider.SqlServerCe)
                {
                    return "CREATE TABLE [__MigrationHistory]";
                }

                return "CREATE TABLE [dbo].[__MigrationHistory]";
            }
        }

        private string DropMetadataStatement
        {
            get
            {
                if (DatabaseProvider == DatabaseProvider.SqlServerCe)
                {
                    return "DROP TABLE [__MigrationHistory]";
                }

                return "DROP TABLE [dbo].[__MigrationHistory]";
            }
        }

        [MigrationsTheory]
        public void Can_script_pending_migrations()
        {
            ResetDatabase();

            var migrator1 = CreateMigrator<ShopContext_v1>();
            var version1 = new MigrationScaffolder(migrator1.Configuration).Scaffold("Version1");

            CreateMigrator<ShopContext_v1>(scaffoldedMigrations: version1).Update();

            var migrator2 = CreateMigrator<ShopContext_v2>(scaffoldedMigrations: version1);

            var version2 = new MigrationScaffolder(migrator2.Configuration).Scaffold("Version2");

            var migrator3 = CreateMigrator<ShopContext_v2>(scaffoldedMigrations: new[] { version1, version2 });
            var scriptingDecorator = new MigratorScriptingDecorator(migrator3);

            var script = scriptingDecorator.ScriptUpdate(null, null);

            Assert.False(script.Contains("Version1"));
            Assert.True(script.Contains("Version2"));
            Assert.False(script.Contains("AutomaticMigration"));
        }

        [MigrationsTheory(SlowGroup = TestGroup.MigrationsTests)]
        public void Can_script_windows_whenDatabaseExists_true()
        {
            Can_script_windows(true);
        }

        [MigrationsTheory(SlowGroup = TestGroup.MigrationsTests)]
        public void Can_script_windows_whenDatabaseExists_false()
        {
            Can_script_windows(false);
        }

        private void Can_script_windows(bool whenDatabaseExists)
        {
            ResetDatabase();

            var migrator1 = CreateMigrator<ShopContext_v1>();
            var version1 = new MigrationScaffolder(migrator1.Configuration).Scaffold("Version1");

            CreateMigrator<ShopContext_v1>(scaffoldedMigrations: version1).Update();

            var migrator2 = CreateMigrator<ShopContext_v2>(scaffoldedMigrations: version1);

            var version2 = new MigrationScaffolder(migrator2.Configuration).Scaffold("Version2");

            CreateMigrator<ShopContext_v2>(scaffoldedMigrations: new[] { version1, version2 }).Update();

            var migrator3 = CreateMigrator<ShopContext_v3>(scaffoldedMigrations: new[] { version1, version2 });
            var version3 = new MigrationScaffolder(migrator3.Configuration).Scaffold("Version3");

            var migrator4 = CreateMigrator<ShopContext_v3>(scaffoldedMigrations: new[] { version1, version2, version3 });
            var scriptingDecorator = new MigratorScriptingDecorator(migrator4);

            if (!whenDatabaseExists)
            {
                ResetDatabase();
            }

            // All
            var script = scriptingDecorator.ScriptUpdate(DbMigrator.InitialDatabase, null);

            Assert.True(script.Contains(CreateMetadataStatement));
            Assert.True(script.Contains("Version1"));
            Assert.True(script.Contains("Version2"));
            Assert.True(script.Contains("Version3"));
            Assert.False(script.Contains("AutomaticMigration"));

            if (!whenDatabaseExists
                || DatabaseProvider == DatabaseProvider.SqlClient)
            {
                using (var connection = ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = ConnectionString;

                    using (var command = connection.CreateCommand())
                    {
                        connection.Open();
                        
                        foreach (var batch in script.Split(new[] { "GO\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            command.CommandText = batch;
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            
            // 1
            script = scriptingDecorator.ScriptUpdate(DbMigrator.InitialDatabase, version1.MigrationId);

            Assert.True(script.Contains(CreateMetadataStatement));
            Assert.True(script.Contains("Version1"));
            Assert.False(script.Contains("Version2"));
            Assert.False(script.Contains("Version3"));
            Assert.False(script.Contains("AutomaticMigration"));

            // 1 & 2
            script = scriptingDecorator.ScriptUpdate(DbMigrator.InitialDatabase, version2.MigrationId);

            Assert.True(script.Contains(CreateMetadataStatement));
            Assert.True(script.Contains("Version1"));
            Assert.True(script.Contains("Version2"));
            Assert.False(script.Contains("Version3"));
            Assert.False(script.Contains("AutomaticMigration"));
        }

        [MigrationsTheory]
        public void Can_script_first_migration_with_leading_automatic_migration_whenDatabaseExists_true()
        {
            Can_script_first_migration_with_leading_automatic_migration(true);
        }

        [MigrationsTheory]
        public void Can_script_first_migration_with_leading_automatic_migration_whenDatabaseExists_false()
        {
            Can_script_first_migration_with_leading_automatic_migration(false);
        }

        private void Can_script_first_migration_with_leading_automatic_migration(bool whenDatabaseExists)
        {
            ResetDatabase();

            CreateMigrator<ShopContext_v2>().Update();

            var migrator1 = CreateMigrator<ShopContext_v3>();
            var version2 = new MigrationScaffolder(migrator1.Configuration).Scaffold("Version2");

            var migrator2 = CreateMigrator<ShopContext_v3>(scaffoldedMigrations: version2);

            migrator2.Update();

            if (!whenDatabaseExists)
            {
                ResetDatabase();
            }

            var scriptingDecorator
                = new MigratorScriptingDecorator(CreateMigrator<ShopContext_v3>(scaffoldedMigrations: version2));

            // Act
            var script = scriptingDecorator.ScriptUpdate(DbMigrator.InitialDatabase, version2.MigrationId);

            // Assert
            Assert.True(script.Contains(CreateMetadataStatement));
            Assert.True(script.Contains("AutomaticMigration"));
            Assert.True(script.Contains("Version2"));
        }

        [MigrationsTheory]
        public void Can_script_middle_migration_with_leading_automatic_migration_whenDatabaseExists_true()
        {
            Can_script_middle_migration_with_leading_automatic_migration(true);
        }

        [MigrationsTheory]
        public void Can_script_middle_migration_with_leading_automatic_migration_whenDatabaseExists_false()
        {
            Can_script_middle_migration_with_leading_automatic_migration(false);
        }

        private void Can_script_middle_migration_with_leading_automatic_migration(bool whenDatabaseExists)
        {
            ResetDatabase();

            var migrator1 = CreateMigrator<ShopContext_v1>();
            var version1 = new MigrationScaffolder(migrator1.Configuration).Scaffold("Version1");

            CreateMigrator<ShopContext_v1>(scaffoldedMigrations: version1).Update();

            CreateMigrator<ShopContext_v2>(automaticDataLossEnabled: true, scaffoldedMigrations: version1).Update();

            var migrator2 = CreateMigrator<ShopContext_v3>(scaffoldedMigrations: version1);
            var version3 = new MigrationScaffolder(migrator2.Configuration).Scaffold("Version3");

            var migrator3 = CreateMigrator<ShopContext_v3>(
                automaticDataLossEnabled: true, scaffoldedMigrations: new[] { version1, version3 });

            migrator3.Update();

            var scriptingDecorator = new MigratorScriptingDecorator(migrator3);

            if (!whenDatabaseExists)
            {
                ResetDatabase();
            }

            // Act
            var script = scriptingDecorator.ScriptUpdate(version1.MigrationId, version3.MigrationId);

            // Assert
            Assert.False(script.Contains(CreateMetadataStatement));
            Assert.False(script.Contains("Version1"));
            Assert.True(script.Contains("AutomaticMigration"));
            Assert.True(script.Contains("Version3"));
        }

        [MigrationsTheory]
        public void Can_script_last_migration_with_trailing_automatic_migration_whenDatabaseExists_true()
        {
            Can_script_last_migration_with_trailing_automatic_migration(true);
        }

        [MigrationsTheory]
        public void Can_script_last_migration_with_trailing_automatic_migration_whenDatabaseExists_false()
        {
            Can_script_last_migration_with_trailing_automatic_migration(false);
        }

        private void Can_script_last_migration_with_trailing_automatic_migration(bool whenDatabaseExists)
        {
            ResetDatabase();

            var migrator1 = CreateMigrator<ShopContext_v1>();
            var version1 = new MigrationScaffolder(migrator1.Configuration).Scaffold("Version1");

            var migrator2 = CreateMigrator<ShopContext_v2>(automaticDataLossEnabled: true, scaffoldedMigrations: version1);
            
            migrator2.Update();
            
            var scriptingDecorator = new MigratorScriptingDecorator(migrator2);

            if (!whenDatabaseExists)
            {
                ResetDatabase();
            }

            var script = scriptingDecorator.ScriptUpdate(DbMigrator.InitialDatabase, version1.MigrationId);

            Assert.NotNull(script);
            Assert.True(script.Contains(CreateMetadataStatement));
            Assert.True(script.Contains("Version1"));
            Assert.False(script.Contains("AutomaticMigration"));
        }

        [MigrationsTheory]
        public void Can_script_trailing_automatic_migration_whenDatabaseExists_true()
        {
            Can_script_trailing_automatic_migration(true);
        }

        [MigrationsTheory]
        public void Can_script_trailing_automatic_migration_whenDatabaseExists_false()
        {
            Can_script_trailing_automatic_migration(false);
        }

        private void Can_script_trailing_automatic_migration(bool whenDatabaseExists)
        {
            ResetDatabase();

            var migrator1 = CreateMigrator<ShopContext_v1>();
            var version1 = new MigrationScaffolder(migrator1.Configuration).Scaffold("Version1");

            var migrator2 = CreateMigrator<ShopContext_v2>(automaticDataLossEnabled: true, scaffoldedMigrations: version1);

            migrator2.Update();

            var scriptingDecorator = new MigratorScriptingDecorator(migrator2);

            if (!whenDatabaseExists)
            {
                ResetDatabase();
            }

            // Act
            var script = scriptingDecorator.ScriptUpdate(version1.MigrationId, null);

            // Assert
            Assert.False(script.Contains(CreateMetadataStatement));
            Assert.False(script.Contains("Version1"));
            Assert.True(script.Contains("AutomaticMigration"));
        }

        [MigrationsTheory]
        public void Can_script_downs()
        {
            ResetDatabase();

            var version1 = new MigrationScaffolder(CreateMigrator<ShopContext_v1>().Configuration).Scaffold("Version1");
            var migrator = CreateMigrator<ShopContext_v1>(scaffoldedMigrations: version1);

            migrator.Update();

            var scriptingDecorator = new MigratorScriptingDecorator(migrator);

            var script = scriptingDecorator.ScriptUpdate(null, DbMigrator.InitialDatabase);

            Assert.True(script.Contains(DropMetadataStatement));
            Assert.False(script.Contains("Version1"));
        }

        [MigrationsTheory]
        public void Can_script_using_migration_names_whenDatabaseExists_true()
        {
            Can_script_using_migration_names(true);
        }

        [MigrationsTheory]
        public void Can_script_using_migration_names_whenDatabaseExists_false()
        {
            Can_script_using_migration_names(false);
        }

        private void Can_script_using_migration_names(bool whenDatabaseExists)
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();
            var version1 = new MigrationScaffolder(migrator.Configuration).Scaffold("Banana");

            CreateMigrator<ShopContext_v1>(scaffoldedMigrations: version1).Update();

            migrator = CreateMigrator<ShopContext_v2>(scaffoldedMigrations: version1);
            var version2 = new MigrationScaffolder(migrator.Configuration).Scaffold("Apple");

            migrator = CreateMigrator<ShopContext_v2>(scaffoldedMigrations: new[] { version1, version2 });
            migrator.Update();

            if (!whenDatabaseExists)
            {
                ResetDatabase();
            }

            var scriptingDecorator
                = new MigratorScriptingDecorator(
                    CreateMigrator<ShopContext_v2>(scaffoldedMigrations: new[] { version1, version2 }));

            var script = scriptingDecorator.ScriptUpdate("Banana", "Apple");

            Assert.False(script.Contains(CreateMetadataStatement));
            Assert.False(script.Contains("Banana"));
            Assert.True(script.Contains("Apple"));
            Assert.False(script.Contains("AutomaticMigration"));
        }
    }
}
