// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Data.SqlClient;
    using System.Linq;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    public class DbMigratorTests : DbTestCase
    {
        private class ContextWithNonDefaultCtor : ShopContext_v1
        {
            public ContextWithNonDefaultCtor(string nameOrConnectionString)
                : base(nameOrConnectionString)
            {
            }
        }

        [MigrationsTheory]
        public void TargetDatabase_should_return_correct_info_for_logging()
        {
            var migrator = CreateMigrator<ShopContext_v1>();

            WhenNotSqlCe(
                () =>
                Assert.Equal(
                    @"'MigrationsTest' (DataSource: .\sqlexpress, Provider: System.Data.SqlClient, Origin: Explicit)",
                    migrator.TargetDatabase));

            WhenSqlCe(
                    () =>
                    Assert.Equal(
                        @"'MigrationsTest.sdf' (DataSource: MigrationsTest.sdf, Provider: System.Data.SqlServerCe.4.0, Origin: Explicit)",
                        migrator.TargetDatabase));
        }

        [MigrationsTheory]
        public void Upgrade_from_earlier_version_should_upgrade_history_table_when_updating_generated()
        {
            ResetDatabase();

            var historyRepository
                = new HistoryRepository(ConnectionString, ProviderFactory);

            var createTableOperation = (CreateTableOperation)
                                   historyRepository.CreateCreateTableOperation(c => new LegacyHistoryContext(c, false), new EdmModelDiffer());

            createTableOperation.Columns.Remove(createTableOperation.Columns.Last());
            createTableOperation.Columns.Add(new ColumnModel(PrimitiveTypeKind.String) { Name = "Hash" });

            ExecuteOperations(
                createTableOperation,
                new SqlOperation(
                    "INSERT INTO [__MigrationHistory] ([MigrationId], [CreatedOn], [Model]) VALUES ('000000000000000_ExistingMigration', GETDATE(), 0x1F8B0800000000000400ECBD07601C499625262F6DCA7B7F4AF54AD7E074A10880601324D8904010ECC188CDE692EC1D69472329AB2A81CA6556655D661640CCED9DBCF7DE7BEFBDF7DE7BEFBDF7BA3B9D4E27F7DFFF3F5C6664016CF6CE4ADAC99E2180AAC81F3F7E7C1F3F22FEC7BFF71F7CFC7BBC5B94E9655E3745B5FCECA3DDF1CE4769BE9C56B36279F1D947EBF67CFBE0A3DFE3E8374E1E9FCE16EFD29F34EDF6D08EDE5C369F7D346FDBD5A3BB779BE93C5F64CD78514CEBAAA9CEDBF1B45ADCCD66D5DDBD9D9D83BBBB3B777302F111C14AD3C7AFD6CBB658E4FC07FD79522DA7F9AA5D67E517D52C2F1BFD9CBE79CD50D317D9226F56D934FFEC236ADB54657EBC5A95C5346B099DDD8FD2E3B2C80895D77979FE9E78ED3C045E1FD91EA9CF53C2ADBDA67EDAAC58E63577CEFDB6F9BBF6A3F4AEC3EEAEA067867177601C8FBFC8562B22A8372EFD247DAD83DA7EFDFE782F04C6DD69B3097DDB535BD5D945DEF916E39AE5CF8ABA699F666D36C99AFCA3F464B688348B0C5F61DBF177C6F958FBBCCDA4769090261FA52FEBEAB2980181D7D74D9B2FC668307EFD8BCA93B2C897AD6BF045B62CCEF3A67D53BDCDC1A044A3AFCF160FEFEEEC812DEE36CDACBC156FF468B8814BFA54797CD71788C74FF3A6B820E89E782CF32958DD01356DCE96E715D16095D7EDF5EBBCF571354DCCD78AEC17799BCD08CFE3BA2DCEB3694B5F4FF3A6A179FB28FDC9AC5C5393D3C5249F9D2DBF5CB7AB757BDC34F962525E8763DADC3F8B4288F3E32F57F8CB6383AF3F0442B3A021E45F2E9FAC8B7266F17E969521ED874160CA3ECFE9735621AF5BFA995F5C5B482FAAE52D0129F99EE6AB7C3923967C932F5625016BBE5CBECE2EF361DC6EA66148B1C74F8BECA2CE163E05E513C5E475463D7B5D5007FE1BAE3FFAF3F15D28F4A3FF270000FFFF4817137F02060000)"));

            var migrator = CreateMigrator<ShopContext_v1>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration1");

            migrator = CreateMigrator<ShopContext_v1>(
                automaticMigrationsEnabled: false,
                scaffoldedMigrations: new[] { generatedMigration });

            Assert.False(ColumnExists(HistoryContext.TableName, "ProductVersion"));
            Assert.True(ColumnExists(HistoryContext.TableName, "Hash"));
            Assert.True(ColumnExists(HistoryContext.TableName, "CreatedOn"));

            migrator.Update(generatedMigration.MigrationId);

            Assert.True(ColumnExists(HistoryContext.TableName, "ProductVersion"));
            Assert.False(ColumnExists(HistoryContext.TableName, "Hash"));
            Assert.False(ColumnExists(HistoryContext.TableName, "CreatedOn"));
        }

        [MigrationsTheory]
        public void Upgrade_from_earlier_version_should_upgrade_history_table_when_updating_automatic()
        {
            ResetDatabase();

            var historyRepository
                = new HistoryRepository(ConnectionString, ProviderFactory);

            var createTableOperation = (CreateTableOperation)
                                   historyRepository.CreateCreateTableOperation(c => new LegacyHistoryContext(c, false), new EdmModelDiffer());

            createTableOperation.Columns.Remove(createTableOperation.Columns.Last());
            createTableOperation.Columns.Add(new ColumnModel(PrimitiveTypeKind.String) { Name = "Hash" });

            ExecuteOperations(
                createTableOperation,
                new SqlOperation(
                    "INSERT INTO [__MigrationHistory] ([MigrationId], [CreatedOn], [Model]) VALUES ('000000000000000_ExistingMigration', GETDATE(), 0x1F8B0800000000000400ECBD07601C499625262F6DCA7B7F4AF54AD7E074A10880601324D8904010ECC188CDE692EC1D69472329AB2A81CA6556655D661640CCED9DBCF7DE7BEFBDF7DE7BEFBDF7BA3B9D4E27F7DFFF3F5C6664016CF6CE4ADAC99E2180AAC81F3F7E7C1F3F22FEC7BFF71F7CFC7BBC5B94E9655E3745B5FCECA3DDF1CE4769BE9C56B36279F1D947EBF67CFBE0A3DFE3E8374E1E9FCE16EFD29F34EDF6D08EDE5C369F7D346FDBD5A3BB779BE93C5F64CD78514CEBAAA9CEDBF1B45ADCCD66D5DDBD9D9D83BBBB3B777302F111C14AD3C7AFD6CBB658E4FC07FD79522DA7F9AA5D67E517D52C2F1BFD9CBE79CD50D317D9226F56D934FFEC236ADB54657EBC5A95C5346B099DDD8FD2E3B2C80895D77979FE9E78ED3C045E1FD91EA9CF53C2ADBDA67EDAAC58E63577CEFDB6F9BBF6A3F4AEC3EEAEA067867177601C8FBFC8562B22A8372EFD247DAD83DA7EFDFE782F04C6DD69B3097DDB535BD5D945DEF916E39AE5CF8ABA699F666D36C99AFCA3F464B688348B0C5F61DBF177C6F958FBBCCDA4769090261FA52FEBEAB2980181D7D74D9B2FC668307EFD8BCA93B2C897AD6BF045B62CCEF3A67D53BDCDC1A044A3AFCF160FEFEEEC812DEE36CDACBC156FF468B8814BFA54797CD71788C74FF3A6B820E89E782CF32958DD01356DCE96E715D16095D7EDF5EBBCF571354DCCD78AEC17799BCD08CFE3BA2DCEB3694B5F4FF3A6A179FB28FDC9AC5C5393D3C5249F9D2DBF5CB7AB757BDC34F962525E8763DADC3F8B4288F3E32F57F8CB6383AF3F0442B3A021E45F2E9FAC8B7266F17E969521ED874160CA3ECFE9735621AF5BFA995F5C5B482FAAE52D0129F99EE6AB7C3923967C932F5625016BBE5CBECE2EF361DC6EA66148B1C74F8BECA2CE163E05E513C5E475463D7B5D5007FE1BAE3FFAF3F15D28F4A3FF270000FFFF4817137F02060000)"));

            var migrator = CreateMigrator<ShopContext_v1>();

            Assert.False(ColumnExists(HistoryContext.TableName, "ProductVersion"));
            Assert.True(ColumnExists(HistoryContext.TableName, "Hash"));
            Assert.True(ColumnExists(HistoryContext.TableName, "CreatedOn"));

            migrator.Update();

            Assert.True(ColumnExists(HistoryContext.TableName, "ProductVersion"));
            Assert.False(ColumnExists(HistoryContext.TableName, "Hash"));
            Assert.False(ColumnExists(HistoryContext.TableName, "CreatedOn"));
        }

        [MigrationsTheory]
        public void Non_constructible_context_should_throw()
        {
            ResetDatabase();

            Assert.Equal(Strings.ContextNotConstructible(typeof(ContextWithNonDefaultCtor)), Assert.Throws<MigrationsException>(() => CreateMigrator<ContextWithNonDefaultCtor>()).Message);
        }

        [MigrationsTheory]
        public void GetMigrations_should_return_migrations_list()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            Assert.True(!migrator.GetLocalMigrations().Any());

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            migrator = CreateMigrator<ShopContext_v1>(
                automaticMigrationsEnabled: false,
                scaffoldedMigrations: generatedMigration);

            Assert.Equal(1, migrator.GetLocalMigrations().Count());
        }

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
        public void Update_down_when_target_migration_id_valid_should_migrate_to_target_version_without_timestamp_part()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            var generatedMigration1 = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration1");

            migrator = CreateMigrator<ShopContext_v1>(
                automaticMigrationsEnabled: false,
                scaffoldedMigrations: generatedMigration1);

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v2>();

            var generatedMigration2 = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration2");

            migrator = CreateMigrator<ShopContext_v2>(
                automaticMigrationsEnabled: false,
                scaffoldedMigrations: new[] { generatedMigration1, generatedMigration2 });

            migrator.Update();

            Assert.True(TableExists("crm.tbl_customers"));

            migrator.Update("Migration1");

            Assert.True(TableExists("MigrationsCustomers"));
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

            Assert.Equal(Strings.MigrationNotFound("balony"), Assert.Throws<MigrationsException>(() => migrator.Update("balony")).Message);
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

            var migrator = CreateMigrator<ShopContext_v1>();

            var generatedMigration1 = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration1");

            migrator = CreateMigrator<ShopContext_v1>(
                automaticMigrationsEnabled: false,
                scaffoldedMigrations: generatedMigration1);

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v2>();

            var generatedMigration2 = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration2");

            migrator = CreateMigrator<ShopContext_v2>(
                automaticMigrationsEnabled: false,
                scaffoldedMigrations: generatedMigration2);

            migrator.Update();

            var generatedMigration3 = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration3");

            migrator = CreateMigrator<ShopContext_v2>(
                automaticMigrationsEnabled: false,
                scaffoldedMigrations: new[] { generatedMigration1, generatedMigration2, generatedMigration3 });

            migrator.Update();

            Assert.True(TableExists("crm.tbl_customers"));

            migrator.Update(generatedMigration1.MigrationId);

            Assert.True(TableExists("MigrationsCustomers"));
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

            Assert.Null(new HistoryRepository(ConnectionString, ProviderFactory).GetLastModel());

            migrator.Update();

            Assert.True(TableExists("MigrationsCustomers"));
            Assert.NotNull(new HistoryRepository(ConnectionString, ProviderFactory).GetLastModel());
        }

        [MigrationsTheory]
        public void Update_down_when_automatic_and_multiple_steps_should_migrate_to_target_version()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory);

            var migrator = CreateMigrator<ShopContext_v1>(automaticDataLossEnabled: true);

            migrator.Update();

            Assert.True(TableExists("MigrationsCustomers"));

            migrator = CreateMigrator<ShopContext_v2>(automaticDataLossEnabled: true);

            migrator.Update();

            Assert.True(TableExists("crm.tbl_customers"));

            migrator.Update(DbMigrator.InitialDatabase);

            Assert.False(TableExists("MigrationsCustomers"));
            Assert.False(TableExists("tbl_customers"));
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

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory);

            Assert.Null(historyRepository.GetLastModel());
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

        [MigrationsTheory]
        public void Update_down_when_explicit_and_automatic_should_migrate_to_target_version()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory);

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            Assert.True(TableExists("MigrationsCustomers"));

            migrator = CreateMigrator<ShopContext_v2>(automaticDataLossEnabled: true);

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            migrator = CreateMigrator<ShopContext_v2>(
                automaticDataLossEnabled: true,
                scaffoldedMigrations: generatedMigration);

            migrator.Update();

            Assert.True(TableExists("crm.tbl_customers"));

            migrator.Update(DbMigrator.InitialDatabase);

            Assert.False(TableExists("MigrationsCustomers"));
            Assert.False(TableExists("tbl_customers"));
            Assert.Null(historyRepository.GetLastModel());
        }

        private static void DropMigrationHistoryAndAddEdmMetadata(DbConnection connection, string hash)
        {
            using (var poker = new EdmMetadataContext(connection, contextOwnsConnection: false))
            {
                poker.Database.ExecuteSqlCommand("drop table " + HistoryContext.TableName);

                poker.Database.ExecuteSqlCommand(
                    ((IObjectContextAdapter)poker).ObjectContext.CreateDatabaseScript());

#pragma warning disable 612,618
                poker.Metadata.Add(new EdmMetadata { ModelHash = hash });
#pragma warning restore 612,618

                poker.SaveChanges();
            }
        }

        [MigrationsTheory]
        public void Upgrade_when_database_up_to_date_creates_bootstrap_history_record()
        {
            DropDatabase();

            try
            {
                Database.SetInitializer(new DropCreateDatabaseAlways<ShopContext_v1>());

                using (var context = CreateContext<ShopContext_v1>())
                {
                    context.Database.Initialize(true);

                    DropMigrationHistoryAndAddEdmMetadata(
                        context.Database.Connection,
#pragma warning disable 612,618
 EdmMetadata.TryGetModelHash(context));
#pragma warning restore 612,618

                    Assert.True(TableExists("EdmMetadata"));

                    var migrator = CreateMigrator<ShopContext_v1>();

                    migrator.Update();
                }
            }
            finally
            {
                Database.SetInitializer(new CreateDatabaseIfNotExists<ShopContext_v1>());
            }

            Assert.True(TableExists(HistoryContext.TableName));
        }

        [MigrationsTheory]
        public void Generate_when_database_up_to_date_creates_empty_migration()
        {
            DropDatabase();

            try
            {
                Database.SetInitializer(new DropCreateDatabaseAlways<ShopContext_v1>());

                using (var context = CreateContext<ShopContext_v1>())
                {
                    context.Database.Initialize(true);

                    DropMigrationHistoryAndAddEdmMetadata(
                        context.Database.Connection,
#pragma warning disable 612,618
 EdmMetadata.TryGetModelHash(context));
#pragma warning restore 612,618

                    Assert.True(TableExists("EdmMetadata"));

                    var migrator = CreateMigrator<ShopContext_v1>();

                    var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Empty");

                    Assert.Equal(300, generatedMigration.UserCode.Length);
                }
            }
            finally
            {
                Database.SetInitializer(new CreateDatabaseIfNotExists<ShopContext_v1>());
            }

            Assert.False(TableExists(HistoryContext.TableName));
        }

        private class ShopContext_v1b : ShopContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<MigrationsCustomer>().Property(c => c.Name).HasColumnName("new_name");
            }
        }

        [MigrationsTheory]
        public void Upgrade_when_database_out_of_date_throws()
        {
            DropDatabase();

            try
            {
                Database.SetInitializer(new DropCreateDatabaseAlways<ShopContext_v1>());

                using (var context = CreateContext<ShopContext_v1>())
                {
                    context.Database.Initialize(true);

                    DropMigrationHistoryAndAddEdmMetadata(
                        context.Database.Connection,
#pragma warning disable 612,618
 EdmMetadata.TryGetModelHash(context));
#pragma warning restore 612,618

                    Assert.True(TableExists("EdmMetadata"));

                    var migrator = CreateMigrator<ShopContext_v1b>();

                    Assert.Equal(Strings.MetadataOutOfDate, Assert.Throws<MigrationsException>(() => migrator.Update()).Message);
                }
            }
            finally
            {
                Database.SetInitializer(new CreateDatabaseIfNotExists<ShopContext_v1>());
            }

            Assert.False(TableExists(HistoryContext.TableName));
        }

        [MigrationsTheory]
        public void Generate_when_database_out_of_date_throws()
        {
            DropDatabase();

            try
            {
                Database.SetInitializer(new DropCreateDatabaseAlways<ShopContext_v1>());

                using (var context = CreateContext<ShopContext_v1>())
                {
                    context.Database.Initialize(true);

                    DropMigrationHistoryAndAddEdmMetadata(
                        context.Database.Connection,
#pragma warning disable 612,618
 EdmMetadata.TryGetModelHash(context));
#pragma warning restore 612,618

                    Assert.True(TableExists("EdmMetadata"));

                    var migrator = CreateMigrator<ShopContext_v1b>();

                    Assert.Equal(Strings.MetadataOutOfDate, Assert.Throws<MigrationsException>(() => migrator.Scaffold("GrowUp", null, ignoreChanges: false)).Message);
                }
            }
            finally
            {
                Database.SetInitializer(new CreateDatabaseIfNotExists<ShopContext_v1>());
            }

            Assert.False(TableExists(HistoryContext.TableName));
        }

        [MigrationsTheory]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal("configuration", Assert.Throws<ArgumentNullException>(() => new DbMigrator(null)).ParamName);
        }

        [MigrationsTheory]
        public void ScaffoldInitialCreate_should_return_null_when_no_db()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            var scaffoldedMigration = migrator.ScaffoldInitialCreate("Foo");

            Assert.Null(scaffoldedMigration);
        }

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

            var migrator = CreateMigrator<ShopContext_v1>();

            var initialCreate = new MigrationScaffolder(migrator.Configuration).Scaffold("InitialCreate");

            migrator = CreateMigrator<ShopContext_v1>(scaffoldedMigrations: initialCreate);

            migrator.Update();

            var scaffoldedMigration = migrator.ScaffoldInitialCreate("Foo");

            Assert.NotNull(scaffoldedMigration);
            Assert.NotSame(initialCreate, scaffoldedMigration);
            Assert.Equal(initialCreate.MigrationId, scaffoldedMigration.MigrationId);
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
                generatedMigration.DesignerCode
                    .Contains("IMigrationMetadata.Source\r\n        {\r\n            get { return null; }"));
        }

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
                    .Contains("IMigrationMetadata.Source\r\n        {\r\n            get { return R"));
        }

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

        [MigrationsTheory]
        public void Generate_when_model_up_to_date_should_create_stub_migration()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            Assert.Equal(304, generatedMigration.UserCode.Length);
        }

        private class TestLogger : MigrationsLogger
        {
            public readonly List<string> Messages = new List<string>();

            public override void Info(string message)
            {
                Messages.Add(message);
            }

            public override void Warning(string message)
            {
                Messages.Add(message);
            }

            public override void Verbose(string sql)
            {
                Messages.Add(sql);
            }
        }

        [MigrationsTheory]
        public void Can_setup_decorator_pattern()
        {
            ResetDatabase();

            var testLogger = new TestLogger();

            var migrator = CreateMigrator<ShopContext_v1>();

            new MigratorLoggingDecorator(migrator, testLogger).Update();

            Assert.True(testLogger.Messages.Count > 0);
        }

        [MigrationsTheory]
        public void Update_should_update_legacy_hash()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            Assert.True(CreateContext<ShopContext_v1>().Database.CompatibleWithModel(true));
        }

        [MigrationsTheory]
        public void First_migration_should_have_null_source_model()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            Assert.True(
                generatedMigration.DesignerCode
                    .Contains("IMigrationMetadata.Source\r\n        {\r\n            get { return null; }"));
        }

        [MigrationsTheory]
        public void Update_blocks_automatic_migration()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>(automaticMigrationsEnabled: false);

            Assert.Equal(new AutomaticMigrationsDisabledException(Strings.AutomaticDisabledException).Message, Assert.Throws<AutomaticMigrationsDisabledException>(() => migrator.Update()).Message);
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

            Assert.Equal(new AutomaticDataLossException(Strings.AutomaticDataLoss).Message, Assert.Throws<AutomaticDataLossException>(() => migrator.Update()).Message);
        }

        [MigrationsTheory]
        public void Generate_should_not_create_database()
        {
            var migrator = CreateMigrator<ShopContext_v1>(targetDatabase: "NoSuchDatabase");

            DropDatabase();

            new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            Assert.False(DatabaseExists());
        }
    }

    public class DbMigratorTests_ContextConstruction : DbTestCase
    {
        private class NuGetContext : DbContext
        {
            public NuGetContext()
                : base(GetConnection(), contextOwnsConnection: true)
            {
            }

            private static DbConnection GetConnection()
            {
                return
                    new SqlConnection(
                        "Data Source=.\\sqlexpress;Initial Catalog=DbMigratorTests_ContextConstruction;Integrated Security=True");
            }
        }

        [MigrationsTheory]
        public void Can_use_external_context_connection()
        {
            ResetDatabase();

            var migrator = CreateMigrator<NuGetContext>();

            migrator.Update();

            Assert.True(DatabaseExists());
        }
    }

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    public class DbMigratorTests_SqlClientOnly : DbTestCase
    {
        [MigrationsTheory]
        public void ExecuteSql_should_honor_CommandTimeout()
        {
            var migrator = CreateMigrator<ShopContext_v1>();
            migrator.Configuration.CommandTimeout = 1;
            var migrationStatements = new[]
                {
                    new MigrationStatement { Sql = "WAITFOR DELAY '00:00:02'" }
                };

            var ex = Assert.Throws<SqlException>(
                () => migrator.ExecuteStatements(migrationStatements));

            Assert.Equal(-2, ex.Number);
        }

        [MigrationsTheory]
        public void ExecuteSql_when_SuppressTransaction_should_honor_CommandTimeout()
        {
            var migrator = CreateMigrator<ShopContext_v1>();
            migrator.Configuration.CommandTimeout = 1;
            var migrationStatements = new[]
                {
                    new MigrationStatement
                    {
                        Sql = "WAITFOR DELAY '00:00:02'",
                        SuppressTransaction = true
                    }
                };

            var ex = Assert.Throws<SqlException>(
                () => migrator.ExecuteStatements(migrationStatements));

            Assert.Equal(-2, ex.Number);
        }
    }
}
