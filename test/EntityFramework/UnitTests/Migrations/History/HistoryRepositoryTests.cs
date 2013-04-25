// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.History
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Edm;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    public class HistoryRepositoryTests : DbTestCase
    {
        [MigrationsTheory]
        public void GetUpgradeOperations_should_return_add_product_version_column_when_not_present()
        {
            ResetDatabase();

            var historyRepository
                = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);

            var createTableOperation = GetCreateHistoryTableOperation();

            createTableOperation.Columns.Remove(createTableOperation.Columns.Last());

            ExecuteOperations(createTableOperation);

            var addColumnOperation = (AddColumnOperation)historyRepository.GetUpgradeOperations().Last();

            Assert.Equal("ProductVersion", addColumnOperation.Column.Name);
            Assert.Equal("0.7.0.0", addColumnOperation.Column.DefaultValue);
            Assert.Equal(32, addColumnOperation.Column.MaxLength);
            Assert.False(addColumnOperation.Column.IsNullable.Value);
        }

        [MigrationsTheory]
        public void GetUpgradeOperations_should_return_context_key_column_when_not_present()
        {
            ResetDatabase();

            var historyRepository
                = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);

            var createTableOperation = GetCreateHistoryTableOperation();

            createTableOperation.Columns.Remove(createTableOperation.Columns.Single(c => c.Name == "ContextKey"));
            createTableOperation.PrimaryKey.Columns.RemoveAt(1);
            ExecuteOperations(createTableOperation);

            var addColumnOperation = historyRepository.GetUpgradeOperations().OfType<AddColumnOperation>().Single();

            Assert.Equal("ContextKey", addColumnOperation.Column.Name);
            Assert.Equal("MyKey", addColumnOperation.Column.DefaultValue);
            Assert.Equal(512, addColumnOperation.Column.MaxLength);
            Assert.False(addColumnOperation.Column.IsNullable.Value);
        }

        [MigrationsTheory]
        public void GetUpgradeOperations_should_return_nothing_when_table_not_present()
        {
            ResetDatabase();

            var historyRepository
                = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);

            Assert.False(historyRepository.GetUpgradeOperations().Any());
        }

        [MigrationsTheory]
        public void GetMigrationsSince_should_return_all_migrations_when_target_is_empty()
        {
            ResetDatabase();

            var historyRepository
                = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                new[]
                    {
                        GetCreateHistoryTableOperation(),
                        historyRepository.CreateInsertOperation("Migration1", model),
                        historyRepository.CreateInsertOperation("Migration2", model)
                    });

            var migrations = historyRepository.GetMigrationsSince(DbMigrator.InitialDatabase);

            Assert.Equal(2, migrations.Count());
            Assert.Equal("Migration2", migrations.First());
        }

        [MigrationsTheory]
        public void HasMigrations_should_return_true_when_table_has_migrations_for_key()
        {
            ResetDatabase();

            var historyRepository
                = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                new[]
                    {
                        GetCreateHistoryTableOperation(),
                        historyRepository.CreateInsertOperation("Migration1", model)
                    });

            Assert.True(historyRepository.HasMigrations());
        }

        [MigrationsTheory]
        public void HasMigrations_should_return_false_when_context_key_not_matching()
        {
            ResetDatabase();

            var historyRepository1
                = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey1", null);

            var historyRepository2
                = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey2", null);

            ExecuteOperations(GetCreateHistoryTableOperation());

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                new[]
                    {
                        historyRepository2.CreateInsertOperation("Migration2", model)
                    });

            Assert.False(historyRepository1.HasMigrations());
        }

        [MigrationsTheory]
        public void GetMigrationId_should_match_on_name()
        {
            ResetDatabase();

            var historyRepository
                = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                new[]
                    {
                        GetCreateHistoryTableOperation(),
                        historyRepository.CreateInsertOperation("201109192032331_Migration1", model),
                        historyRepository.CreateInsertOperation("201109192032332_Migration2", model)
                    });

            var migrationId = historyRepository.GetMigrationId("Migration1");

            Assert.Equal("201109192032331_Migration1", migrationId);

            migrationId = historyRepository.GetMigrationId("migrATIon2");

            Assert.Equal("201109192032332_Migration2", migrationId);
        }

        [MigrationsTheory]
        public void GetMigrationId_should_match_by_context_key()
        {
            ResetDatabase();

            var historyRepository1
                = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey1", null);

            var historyRepository2
                = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey2", null);

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                new[]
                    {
                        GetCreateHistoryTableOperation(),
                        historyRepository1.CreateInsertOperation("201109192032331_Migration1", model),
                        historyRepository1.CreateInsertOperation("201109192032332_Migration2", model),
                        historyRepository2.CreateInsertOperation("201109192032331_Migration1", model),
                        historyRepository2.CreateInsertOperation("201109192032332_Migration2", model)
                    });

            var migrationId = historyRepository1.GetMigrationId("Migration1");

            Assert.Equal("201109192032331_Migration1", migrationId);

            migrationId = historyRepository1.GetMigrationId("migrATIon2");

            Assert.Equal("201109192032332_Migration2", migrationId);
        }

        [MigrationsTheory]
        public void GetMigrationId_should_throw_when_name_ambiguous()
        {
            ResetDatabase();

            var historyRepository
                = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);

            ExecuteOperations(GetCreateHistoryTableOperation());

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                new[]
                    {
                        historyRepository.CreateInsertOperation("201109192032331_Migration", model),
                        historyRepository.CreateInsertOperation("201109192032332_Migration", model)
                    });

            Assert.Equal(
                Strings.AmbiguousMigrationName("Migration"),
                Assert.Throws<MigrationsException>(() => historyRepository.GetMigrationId("Migration")).Message);
        }

        [MigrationsTheory]
        public void GetMigrationId_should_return_null_when_no_database()
        {
            var historyRepository
                = new HistoryRepository(
                    ConnectionString.Replace(DatabaseProviderFixture.DefaultDatabaseName, "NoSuchDatabase"), ProviderFactory, "MyKey", null);

            Assert.Null(historyRepository.GetMigrationId(DbMigrator.InitialDatabase));
        }

        [MigrationsTheory]
        public void GetMigrationsSince_should_return_subset_when_target_valid()
        {
            ResetDatabase();

            var historyRepository
                = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);

            ExecuteOperations(GetCreateHistoryTableOperation());

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                new[]
                    {
                        historyRepository.CreateInsertOperation("Migration1", model),
                        historyRepository.CreateInsertOperation("Migration2", model)
                    });

            var migrations = historyRepository.GetMigrationsSince("Migration1");

            Assert.Equal(1, migrations.Count());
            Assert.Equal("Migration2", migrations.Single());
        }

        [MigrationsTheory]
        public void GetMigrationsSince_should_return_subset_based_on_context_key()
        {
            ResetDatabase();

            var historyRepository1
                = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey1", null);

            var historyRepository2
                = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey2", null);

            ExecuteOperations(GetCreateHistoryTableOperation());

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                new[]
                    {
                        historyRepository1.CreateInsertOperation("Migration1", model),
                        historyRepository1.CreateInsertOperation("Migration2", model),
                        historyRepository2.CreateInsertOperation("Migration1", model),
                        historyRepository2.CreateInsertOperation("Migration2", model)
                    });

            var migrations = historyRepository1.GetMigrationsSince("Migration1");

            Assert.Equal(1, migrations.Count());
            Assert.Equal("Migration2", migrations.Single());
        }

        [MigrationsTheory]
        public void GetMigrationsSince_should_return_empty_when_target_valid_but_is_latest()
        {
            ResetDatabase();

            var historyRepository
                = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);

            ExecuteOperations(GetCreateHistoryTableOperation());

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                new[]
                    {
                        historyRepository.CreateInsertOperation("Migration1", model),
                        historyRepository.CreateInsertOperation("Migration2", model)
                    });

            var migrations = historyRepository.GetMigrationsSince("Migration2");

            Assert.Equal(0, migrations.Count());
        }

        [MigrationsTheory]
        public void GetPendingMigrations_should_return_empty_set_when_no_database()
        {
            var historyRepository
                = new HistoryRepository(
                    ConnectionString.Replace(DatabaseProviderFixture.DefaultDatabaseName, "NoSuchDatabase"),
                    ProviderFactory, "MyKey", null);

            Assert.False(historyRepository.GetPendingMigrations(Enumerable.Empty<string>()).Any());
        }

        [MigrationsTheory]
        public void GetPendingMigrations_should_return_empty_set_when_no_table()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);

            Assert.False(historyRepository.GetPendingMigrations(Enumerable.Empty<string>()).Any());
        }

        [MigrationsTheory]
        public void GetPendingMigrations_should_return_empty_set_when_no_data()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);

            Assert.False(historyRepository.GetPendingMigrations(Enumerable.Empty<string>()).Any());
        }

        [MigrationsTheory]
        public void GetPendingMigrations_should_return_migrations_not_in_input_set()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                GetCreateHistoryTableOperation(),
                historyRepository.CreateInsertOperation("Migration 1", model),
                historyRepository.CreateInsertOperation("Migration 3", model),
                historyRepository.CreateInsertOperation("Migration 5", model));

            var pendingMigrations =
                historyRepository.GetPendingMigrations(
                    new[] { "Migration 1", "Migration 2", "Migration 3", "Migration 4", "Migration 5" });

            Assert.Equal("Migration 2", pendingMigrations.First());
            Assert.Equal("Migration 4", pendingMigrations.Last());
        }

        [MigrationsTheory]
        public void GetPendingMigrations_should_return_migrations_based_on_context_key()
        {
            ResetDatabase();

            var historyRepository1 = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey1", null);
            var historyRepository2 = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey2", null);

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                GetCreateHistoryTableOperation(),
                historyRepository1.CreateInsertOperation("Migration 1", model),
                historyRepository2.CreateInsertOperation("Migration 1", model),
                historyRepository1.CreateInsertOperation("Migration 3", model),
                historyRepository1.CreateInsertOperation("Migration 5", model));

            var pendingMigrations =
                historyRepository1.GetPendingMigrations(
                    new[] { "Migration 1", "Migration 2", "Migration 3", "Migration 4", "Migration 5" });

            Assert.Equal("Migration 2", pendingMigrations.First());
            Assert.Equal("Migration 4", pendingMigrations.Last());
        }

        [MigrationsTheory]
        public void GetPendingMigrations_should_ignore_InitialCreate_timestamps()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                GetCreateHistoryTableOperation(),
                historyRepository.CreateInsertOperation("000000000000001_InitialCreate", model));

            var pendingMigrations = historyRepository.GetPendingMigrations(
                new[] { "000000000000002_InitialCreate", "Migration 1" });

            Assert.Equal("Migration 1", pendingMigrations.Single());
        }

        [MigrationsTheory]
        public void GetLastModel_should_return_null_when_no_database()
        {
            var historyRepository
                = new HistoryRepository(
                    ConnectionString.Replace(DatabaseProviderFixture.DefaultDatabaseName, "NoSuchDatabase"),
                    ProviderFactory, "MyKey", null);

            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();

            Assert.Null(historyRepository.GetLastModel());
        }

        [MigrationsTheory]
        public void GetLastModel_should_return_null_when_no_table()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();

            Assert.Null(historyRepository.GetLastModel());
        }

        [MigrationsTheory]
        public void GetLastModel_should_return_null_when_no_data()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();

            Assert.Null(historyRepository.GetLastModel());
        }

        [MigrationsTheory]
        public void GetLastModel_should_return_model_when_row()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);

            var model1 = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                GetCreateHistoryTableOperation(),
                historyRepository.CreateInsertOperation("Migration 1", model1));

            ExecuteOperations(
                new[] { historyRepository.CreateInsertOperation("Migration 2", model1) });

            string migrationId;
            var model2 = historyRepository.GetLastModel(out migrationId);

            Assert.NotNull(model2);
            Assert.True(XNode.DeepEquals(model1, model2));
            Assert.Equal("Migration 2", migrationId);
        }

        [MigrationsTheory]
        public void GetLastModel_should_return_model_based_on_context_key()
        {
            ResetDatabase();

            var historyRepository1
                = new HistoryRepository(ConnectionString, ProviderFactory, "Key1", null);

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                GetCreateHistoryTableOperation(),
                historyRepository1.CreateInsertOperation("Migration 1", model));

            var historyRepository2
                = new HistoryRepository(ConnectionString, ProviderFactory, "Key2", null);

            ExecuteOperations(
                new[] { historyRepository2.CreateInsertOperation("Migration 2", model) });

            string migrationId;
            model = historyRepository1.GetLastModel(out migrationId);

            Assert.NotNull(model);
            Assert.Equal("Migration 1", migrationId);

            model = historyRepository2.GetLastModel(out migrationId);

            Assert.NotNull(model);
            Assert.Equal("Migration 2", migrationId);
        }

        [MigrationsTheory]
        public void GetLastModel_should_return_model_based_on_passed_context_key()
        {
            ResetDatabase();

            var historyRepository1
                = new HistoryRepository(ConnectionString, ProviderFactory, "Key1", null);

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                GetCreateHistoryTableOperation(),
                historyRepository1.CreateInsertOperation("Migration 1", model));

            var historyRepository2
                = new HistoryRepository(ConnectionString, ProviderFactory, "Key2", null);

            ExecuteOperations(
                new[] { historyRepository2.CreateInsertOperation("Migration 2", model) });

            string migrationId;
            model = historyRepository1.GetLastModel(out migrationId, "Key2");

            Assert.NotNull(model);
            Assert.Equal("Migration 2", migrationId);
        }

        [MigrationsTheory]
        public void GetLastModel_should_return_model_based_on_passed_context_key_when_custom_default_schema()
        {
            ResetDatabase();

            var historyRepository
                = new HistoryRepository(ConnectionString, ProviderFactory, "LegacyKey", null)
                      {
                          CurrentSchema = "foo"
                      };

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                GetCreateHistoryTableOperation(historyRepository.CurrentSchema),
                historyRepository.CreateInsertOperation("Migration", model));

            historyRepository
                = new HistoryRepository(ConnectionString, ProviderFactory, "NewKey", null, new[] { "foo" });

            string migrationId;
            model = historyRepository.GetLastModel(out migrationId, "LegacyKey");

            Assert.NotNull(model);
            Assert.Equal("Migration", migrationId);
        }

        [MigrationsTheory]
        public void GetModel_should_return_model_based_on_context_key()
        {
            ResetDatabase();

            var historyRepository1
                = new HistoryRepository(ConnectionString, ProviderFactory, "Key1", null);

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                GetCreateHistoryTableOperation(),
                historyRepository1.CreateInsertOperation("Migration 1", model));

            var historyRepository2
                = new HistoryRepository(ConnectionString, ProviderFactory, "Key2", null);

            ExecuteOperations(
                new[] { historyRepository2.CreateInsertOperation("Migration 2", model) });

            model = historyRepository1.GetModel("Migration 1");

            Assert.NotNull(model);

            model = historyRepository2.GetModel("Migration 2");

            Assert.NotNull(model);
        }

        [MigrationsTheory]
        public void Exists_should_return_false_when_no_database()
        {
            var historyRepository
                = new HistoryRepository(
                    ConnectionString.Replace(DatabaseProviderFixture.DefaultDatabaseName, "NoSuchDatabase"),
                    ProviderFactory, "MyKey", null);

            Assert.False(historyRepository.Exists());
        }

        [MigrationsTheory]
        public void Exists_should_return_false_when_no_table()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);

            Assert.False(historyRepository.Exists());
        }

        [MigrationsTheory]
        public void Exists_should_return_true_when_database_and_table()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);

            ExecuteOperations(GetCreateHistoryTableOperation());

            Assert.True(historyRepository.Exists());
        }

        [MigrationsTheory]
        public void IsShared_should_return_true_when_rows_with_other_context_key()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);

            ExecuteOperations(GetCreateHistoryTableOperation());

            Assert.False(historyRepository.IsShared());

            ExecuteOperations(
                new SqlOperation(
                    @"INSERT INTO [__MigrationHistory] ([MigrationId], [ContextKey], [ProductVersion], [Model]) 
                                  VALUES ('000000000000000_ExistingMigration', 
                                          'MyKey',
                                          '1.0',
                                          0x1F8B0800000000000400ECBD07601C499625262F6DCA7B7F4AF54AD7E074A10880601324D8904010ECC188CDE692EC1D69472329AB2A81CA6556655D661640CCED9DBCF7DE7BEFBDF7DE7BEFBDF7BA3B9D4E27F7DFFF3F5C6664016CF6CE4ADAC99E2180AAC81F3F7E7C1F3F22FEC7BFF71F7CFC7BBC5B94E9655E3745B5FCECA3DDF1CE4769BE9C56B36279F1D947EBF67CFBE0A3DFE3E8374E1E9FCE16EFD29F34EDF6D08EDE5C369F7D346FDBD5A3BB779BE93C5F64CD78514CEBAAA9CEDBF1B45ADCCD66D5DDBD9D9D83BBBB3B777302F111C14AD3C7AFD6CBB658E4FC07FD79522DA7F9AA5D67E517D52C2F1BFD9CBE79CD50D317D9226F56D934FFEC236ADB54657EBC5A95C5346B099DDD8FD2E3B2C80895D77979FE9E78ED3C045E1FD91EA9CF53C2ADBDA67EDAAC58E63577CEFDB6F9BBF6A3F4AEC3EEAEA067867177601C8FBFC8562B22A8372EFD247DAD83DA7EFDFE782F04C6DD69B3097DDB535BD5D945DEF916E39AE5CF8ABA699F666D36C99AFCA3F464B688348B0C5F61DBF177C6F958FBBCCDA4769090261FA52FEBEAB2980181D7D74D9B2FC668307EFD8BCA93B2C897AD6BF045B62CCEF3A67D53BDCDC1A044A3AFCF160FEFEEEC812DEE36CDACBC156FF468B8814BFA54797CD71788C74FF3A6B820E89E782CF32958DD01356DCE96E715D16095D7EDF5EBBCF571354DCCD78AEC17799BCD08CFE3BA2DCEB3694B5F4FF3A6A179FB28FDC9AC5C5393D3C5249F9D2DBF5CB7AB757BDC34F962525E8763DADC3F8B4288F3E32F57F8CB6383AF3F0442B3A021E45F2E9FAC8B7266F17E969521ED874160CA3ECFE9735621AF5BFA995F5C5B482FAAE52D0129F99EE6AB7C3923967C932F5625016BBE5CBECE2EF361DC6EA66148B1C74F8BECA2CE163E05E513C5E475463D7B5D5007FE1BAE3FFAF3F15D28F4A3FF270000FFFF4817137F02060000)"));

            Assert.False(historyRepository.IsShared());

            ExecuteOperations(
                new SqlOperation(
                    @"INSERT INTO [__MigrationHistory] ([MigrationId], [ContextKey], [ProductVersion], [Model]) 
                                  VALUES ('000000000000000_ExistingMigration', 
                                          'MyOtherKey',
                                          '1.0',
                                          0x1F8B0800000000000400ECBD07601C499625262F6DCA7B7F4AF54AD7E074A10880601324D8904010ECC188CDE692EC1D69472329AB2A81CA6556655D661640CCED9DBCF7DE7BEFBDF7DE7BEFBDF7BA3B9D4E27F7DFFF3F5C6664016CF6CE4ADAC99E2180AAC81F3F7E7C1F3F22FEC7BFF71F7CFC7BBC5B94E9655E3745B5FCECA3DDF1CE4769BE9C56B36279F1D947EBF67CFBE0A3DFE3E8374E1E9FCE16EFD29F34EDF6D08EDE5C369F7D346FDBD5A3BB779BE93C5F64CD78514CEBAAA9CEDBF1B45ADCCD66D5DDBD9D9D83BBBB3B777302F111C14AD3C7AFD6CBB658E4FC07FD79522DA7F9AA5D67E517D52C2F1BFD9CBE79CD50D317D9226F56D934FFEC236ADB54657EBC5A95C5346B099DDD8FD2E3B2C80895D77979FE9E78ED3C045E1FD91EA9CF53C2ADBDA67EDAAC58E63577CEFDB6F9BBF6A3F4AEC3EEAEA067867177601C8FBFC8562B22A8372EFD247DAD83DA7EFDFE782F04C6DD69B3097DDB535BD5D945DEF916E39AE5CF8ABA699F666D36C99AFCA3F464B688348B0C5F61DBF177C6F958FBBCCDA4769090261FA52FEBEAB2980181D7D74D9B2FC668307EFD8BCA93B2C897AD6BF045B62CCEF3A67D53BDCDC1A044A3AFCF160FEFEEEC812DEE36CDACBC156FF468B8814BFA54797CD71788C74FF3A6B820E89E782CF32958DD01356DCE96E715D16095D7EDF5EBBCF571354DCCD78AEC17799BCD08CFE3BA2DCEB3694B5F4FF3A6A179FB28FDC9AC5C5393D3C5249F9D2DBF5CB7AB757BDC34F962525E8763DADC3F8B4288F3E32F57F8CB6383AF3F0442B3A021E45F2E9FAC8B7266F17E969521ED874160CA3ECFE9735621AF5BFA995F5C5B482FAAE52D0129F99EE6AB7C3923967C932F5625016BBE5CBECE2EF361DC6EA66148B1C74F8BECA2CE163E05E513C5E475463D7B5D5007FE1BAE3FFAF3F15D28F4A3FF270000FFFF4817137F02060000)"));

            Assert.True(historyRepository.IsShared());
        }

        [MigrationsTheory]
        public void Repository_should_work_gracefully_when_no_context_key_column()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);

            var createHistoryTableOperation = GetCreateHistoryTableOperation();

            createHistoryTableOperation.Columns.Remove(
                createHistoryTableOperation.Columns.Single(c => c.Name == "ContextKey"));

            createHistoryTableOperation.PrimaryKey.Columns.Remove("ContextKey");

            ExecuteOperations(
                createHistoryTableOperation,
                new SqlOperation(
                    @"INSERT INTO [__MigrationHistory] ([MigrationId], [ProductVersion], [Model]) 
                                  VALUES ('000000000000000_ExistingMigration', 
                                          '1.0',
                                          0x1F8B0800000000000400ECBD07601C499625262F6DCA7B7F4AF54AD7E074A10880601324D8904010ECC188CDE692EC1D69472329AB2A81CA6556655D661640CCED9DBCF7DE7BEFBDF7DE7BEFBDF7BA3B9D4E27F7DFFF3F5C6664016CF6CE4ADAC99E2180AAC81F3F7E7C1F3F22FEC7BFF71F7CFC7BBC5B94E9655E3745B5FCECA3DDF1CE4769BE9C56B36279F1D947EBF67CFBE0A3DFE3E8374E1E9FCE16EFD29F34EDF6D08EDE5C369F7D346FDBD5A3BB779BE93C5F64CD78514CEBAAA9CEDBF1B45ADCCD66D5DDBD9D9D83BBBB3B777302F111C14AD3C7AFD6CBB658E4FC07FD79522DA7F9AA5D67E517D52C2F1BFD9CBE79CD50D317D9226F56D934FFEC236ADB54657EBC5A95C5346B099DDD8FD2E3B2C80895D77979FE9E78ED3C045E1FD91EA9CF53C2ADBDA67EDAAC58E63577CEFDB6F9BBF6A3F4AEC3EEAEA067867177601C8FBFC8562B22A8372EFD247DAD83DA7EFDFE782F04C6DD69B3097DDB535BD5D945DEF916E39AE5CF8ABA699F666D36C99AFCA3F464B688348B0C5F61DBF177C6F958FBBCCDA4769090261FA52FEBEAB2980181D7D74D9B2FC668307EFD8BCA93B2C897AD6BF045B62CCEF3A67D53BDCDC1A044A3AFCF160FEFEEEC812DEE36CDACBC156FF468B8814BFA54797CD71788C74FF3A6B820E89E782CF32958DD01356DCE96E715D16095D7EDF5EBBCF571354DCCD78AEC17799BCD08CFE3BA2DCEB3694B5F4FF3A6A179FB28FDC9AC5C5393D3C5249F9D2DBF5CB7AB757BDC34F962525E8763DADC3F8B4288F3E32F57F8CB6383AF3F0442B3A021E45F2E9FAC8B7266F17E969521ED874160CA3ECFE9735621AF5BFA995F5C5B482FAAE52D0129F99EE6AB7C3923967C932F5625016BBE5CBECE2EF361DC6EA66148B1C74F8BECA2CE163E05E513C5E475463D7B5D5007FE1BAE3FFAF3F15D28F4A3FF270000FFFF4817137F02060000)"));

            Assert.True(historyRepository.Exists());
            Assert.NotNull(historyRepository.GetLastModel());
            Assert.NotEmpty(historyRepository.GetMigrationsSince("0"));
            Assert.NotNull(historyRepository.GetModel("000000000000000_ExistingMigration"));
            Assert.Equal("000000000000000_ExistingMigration", historyRepository.GetMigrationId("ExistingMigration"));
        }

        [MigrationsTheory]
        public void CreateInsertOperation_should_return_valid_history_operation()
        {
            var modelBuilder = new DbModelBuilder();
            var model = modelBuilder.Build(ProviderInfo);
            var edmxString = new StringBuilder();

            using (var xmlWriter = XmlWriter.Create(
                edmxString, new XmlWriterSettings
                                {
                                    Indent = true
                                }))
            {
                EdmxWriter.WriteEdmx(model, xmlWriter);
            }

            var modelDocument = model.GetModel();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);

            var historyOperation
                = (HistoryOperation)historyRepository.CreateInsertOperation("Migration1", modelDocument);

            Assert.NotEmpty(historyOperation.Commands);
            Assert.Equal(4, historyOperation.Commands.Single().Parameters.Count);
        }

        [MigrationsTheory]
        public void CreateDeleteOperation_should_return_valid_history_operation()
        {
            var modelBuilder = new DbModelBuilder();
            var model = modelBuilder.Build(ProviderInfo);
            var edmxString = new StringBuilder();

            using (var xmlWriter = XmlWriter.Create(
                edmxString, new XmlWriterSettings
                                {
                                    Indent = true
                                }))
            {
                EdmxWriter.WriteEdmx(model, xmlWriter);
            }

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);

            var historyOperation
                = (HistoryOperation)historyRepository.CreateDeleteOperation("Migration1");

            Assert.NotEmpty(historyOperation.Commands);
            Assert.Equal(2, historyOperation.Commands.Single().Parameters.Count);
        }

        [MigrationsTheory]
        public void GetLastModel_gets_latest_based_on_MigrationId()
        {
            var historyRepository = SetupHistoryRepositoryForOrderingTest();

            string migrationId;
            historyRepository.GetLastModel(out migrationId);

            Assert.Equal("227309030010001_Migration1", migrationId);
        }

        [MigrationsTheory]
        public void GetMigrationsSince_gets_migrations_based_on_MigrationId()
        {
            var historyRepository = SetupHistoryRepositoryForOrderingTest();

            var migrations = historyRepository.GetMigrationsSince("227209030010001_Migration2");

            Assert.Equal(1, migrations.Count());
            Assert.Equal("227309030010001_Migration1", migrations.Single());
        }

        private HistoryRepository SetupHistoryRepositoryForOrderingTest()
        {
            ResetDatabase();

            using (var context = CreateContext<ShopContext_v1>())
            {
                var model = context.GetModel();

                var clonedConnection = DbProviderServices.GetProviderFactory(context.Database.Connection).CreateConnection();
                clonedConnection.ConnectionString = context.Database.Connection.ConnectionString;

                using (var historyContext = new HistoryContext(clonedConnection, defaultSchema: null))
                {
                    context.InternalContext.MarkDatabaseInitialized();

                    context.Database.ExecuteSqlCommand(
                        ((IObjectContextAdapter)historyContext).ObjectContext.CreateDatabaseScript());

                    historyContext.History.Add(
                        new HistoryRow
                            {
                                MigrationId = "227309030010001_Migration1",
                                ContextKey = "MyKey",
                                Model = new ModelCompressor().Compress(model),
                                ProductVersion = "",
                            });

                    historyContext.History.Add(
                        new HistoryRow
                            {
                                MigrationId = "227209030010001_Migration2",
                                ContextKey = "MyKey",
                                Model = new ModelCompressor().Compress(model),
                                ProductVersion = "",
                            });

                    historyContext.SaveChanges();
                }
            }

            return new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", null);
        }

        [MigrationsTheory]
        public void HistoryRepository_sets_timeout_onto_HistoryContext()
        {
            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory, "MyKey", 77);

            using (var connection = new SqlConnection())
            {
                using (var context = historyRepository.CreateContext(connection))
                {
                    Assert.Equal(77, context.Database.CommandTimeout);
                }
            }
        }
    }
}
