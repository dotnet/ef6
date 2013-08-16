// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Text.RegularExpressions;
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

            using (var context = CreateContext<Ef5MigrationsContext>())
            {
                var model = context.GetModel();

                // create v5 history rows
                ExecuteOperations(
                    new[]
                        {
                            historyRepository.CreateInsertOperation("201112202056275_InitialCreate", model),
                            historyRepository.CreateInsertOperation("201112202056573_AddUrlToBlog", model)
                        });
            }

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
            using (var context = CreateContext<ShopContext_v1>())
            {
                ExecuteOperations(
                    new[]
                        {
                            GetDropHistoryTableOperation(),
                            GetCreateHistoryTableOperation(),
                            historyRepository
                                .CreateInsertOperation(
                                    "201112202056275_NoHistoryModelAutomaticMigration",
                                    context.GetModel())
                        });
            }

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

        private class DropColumnMigration : DbMigration
        {
            public override void Up()
            {
                DropColumn("T", "C");
            }
        }

        [MigrationsTheory]
        public void Scripting_upgrade_from_earlier_version_should_maintain_variable_uniqueness()
        {
            ResetDatabase();

            var createTableOperations = GetLegacyHistoryCreateTableOperations();

            ExecuteOperations(
                createTableOperations.Concat(
                    new[]
                        {
                            new SqlOperation(
                                @"INSERT INTO [__MigrationHistory] ([MigrationId], [CreatedOn], [Model]) 
                                  VALUES ('000000000000000_ExistingMigration', GETDATE(), 0x1F8B0800000000000400ECBD07601C499625262F6DCA7B7F4AF54AD7E074A10880601324D8904010ECC188CDE692EC1D69472329AB2A81CA6556655D661640CCED9DBCF7DE7BEFBDF7DE7BEFBDF7BA3B9D4E27F7DFFF3F5C6664016CF6CE4ADAC99E2180AAC81F3F7E7C1F3F22FEC7BFF71F7CFC7BBC5B94E9655E3745B5FCECA3DDF1CE4769BE9C56B36279F1D947EBF67CFBE0A3DFE3E8374E1E9FCE16EFD29F34EDF6D08EDE5C369F7D346FDBD5A3BB779BE93C5F64CD78514CEBAAA9CEDBF1B45ADCCD66D5DDBD9D9D83BBBB3B777302F111C14AD3C7AFD6CBB658E4FC07FD79522DA7F9AA5D67E517D52C2F1BFD9CBE79CD50D317D9226F56D934FFEC236ADB54657EBC5A95C5346B099DDD8FD2E3B2C80895D77979FE9E78ED3C045E1FD91EA9CF53C2ADBDA67EDAAC58E63577CEFDB6F9BBF6A3F4AEC3EEAEA067867177601C8FBFC8562B22A8372EFD247DAD83DA7EFDFE782F04C6DD69B3097DDB535BD5D945DEF916E39AE5CF8ABA699F666D36C99AFCA3F464B688348B0C5F61DBF177C6F958FBBCCDA4769090261FA52FEBEAB2980181D7D74D9B2FC668307EFD8BCA93B2C897AD6BF045B62CCEF3A67D53BDCDC1A044A3AFCF160FEFEEEC812DEE36CDACBC156FF468B8814BFA54797CD71788C74FF3A6B820E89E782CF32958DD01356DCE96E715D16095D7EDF5EBBCF571354DCCD78AEC17799BCD08CFE3BA2DCEB3694B5F4FF3A6A179FB28FDC9AC5C5393D3C5249F9D2DBF5CB7AB757BDC34F962525E8763DADC3F8B4288F3E32F57F8CB6383AF3F0442B3A021E45F2E9FAC8B7266F17E969521ED874160CA3ECFE9735621AF5BFA995F5C5B482FAAE52D0129F99EE6AB7C3923967C932F5625016BBE5CBECE2EF361DC6EA66148B1C74F8BECA2CE163E05E513C5E475463D7B5D5007FE1BAE3FFAF3F15D28F4A3FF270000FFFF4817137F02060000)")
                        }).ToArray());

            var migrator = CreateMigrator<BlankSlate>(new DropColumnMigration());

            var script = new MigratorScriptingDecorator(migrator).ScriptUpdate(null, null);

            WhenNotSqlCe(() => Assert.Equal(1, new Regex("DECLARE @var0").Matches(script).Count));
            WhenSqlCe(() => Assert.Equal(0, new Regex("DECLARE @var0").Matches(script).Count));
        }

        [MigrationsTheory]
        public void Upgrade_from_earlier_version_should_upgrade_history_table_when_updating_generated()
        {
            ResetDatabase();

            var createTableOperations = GetLegacyHistoryCreateTableOperations();

            ExecuteOperations(
                createTableOperations.Concat(
                    new[]
                        {
                            new SqlOperation(
                                @"INSERT INTO [__MigrationHistory] ([MigrationId], [CreatedOn], [Model]) 
                                  VALUES ('000000000000000_ExistingMigration', GETDATE(), 0x1F8B0800000000000400ECBD07601C499625262F6DCA7B7F4AF54AD7E074A10880601324D8904010ECC188CDE692EC1D69472329AB2A81CA6556655D661640CCED9DBCF7DE7BEFBDF7DE7BEFBDF7BA3B9D4E27F7DFFF3F5C6664016CF6CE4ADAC99E2180AAC81F3F7E7C1F3F22FEC7BFF71F7CFC7BBC5B94E9655E3745B5FCECA3DDF1CE4769BE9C56B36279F1D947EBF67CFBE0A3DFE3E8374E1E9FCE16EFD29F34EDF6D08EDE5C369F7D346FDBD5A3BB779BE93C5F64CD78514CEBAAA9CEDBF1B45ADCCD66D5DDBD9D9D83BBBB3B777302F111C14AD3C7AFD6CBB658E4FC07FD79522DA7F9AA5D67E517D52C2F1BFD9CBE79CD50D317D9226F56D934FFEC236ADB54657EBC5A95C5346B099DDD8FD2E3B2C80895D77979FE9E78ED3C045E1FD91EA9CF53C2ADBDA67EDAAC58E63577CEFDB6F9BBF6A3F4AEC3EEAEA067867177601C8FBFC8562B22A8372EFD247DAD83DA7EFDFE782F04C6DD69B3097DDB535BD5D945DEF916E39AE5CF8ABA699F666D36C99AFCA3F464B688348B0C5F61DBF177C6F958FBBCCDA4769090261FA52FEBEAB2980181D7D74D9B2FC668307EFD8BCA93B2C897AD6BF045B62CCEF3A67D53BDCDC1A044A3AFCF160FEFEEEC812DEE36CDACBC156FF468B8814BFA54797CD71788C74FF3A6B820E89E782CF32958DD01356DCE96E715D16095D7EDF5EBBCF571354DCCD78AEC17799BCD08CFE3BA2DCEB3694B5F4FF3A6A179FB28FDC9AC5C5393D3C5249F9D2DBF5CB7AB757BDC34F962525E8763DADC3F8B4288F3E32F57F8CB6383AF3F0442B3A021E45F2E9FAC8B7266F17E969521ED874160CA3ECFE9735621AF5BFA995F5C5B482FAAE52D0129F99EE6AB7C3923967C932F5625016BBE5CBECE2EF361DC6EA66148B1C74F8BECA2CE163E05E513C5E475463D7B5D5007FE1BAE3FFAF3F15D28F4A3FF270000FFFF4817137F02060000)")
                        }).ToArray());

            var migrator = CreateMigrator<ShopContext_v1>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration1");

            migrator = CreateMigrator<ShopContext_v1>(
                automaticMigrationsEnabled: false,
                scaffoldedMigrations: new[] { generatedMigration });

            Assert.False(ColumnExists(HistoryContext.DefaultTableName, "ProductVersion"));
            Assert.False(ColumnExists(HistoryContext.DefaultTableName, "ContextKey"));
            Assert.True(ColumnExists(HistoryContext.DefaultTableName, "Hash"));
            Assert.True(ColumnExists(HistoryContext.DefaultTableName, "CreatedOn"));

            migrator.Update(generatedMigration.MigrationId);

            Assert.True(ColumnExists(HistoryContext.DefaultTableName, "ProductVersion"));
            Assert.True(ColumnExists(HistoryContext.DefaultTableName, "ContextKey"));
            Assert.False(ColumnExists(HistoryContext.DefaultTableName, "Hash"));
            Assert.False(ColumnExists(HistoryContext.DefaultTableName, "CreatedOn"));

            if (ProviderInfo.ProviderInvariantName == "System.Data.SqlClient")
            {
                Assert.Equal(0, GetColumnIndex(HistoryContext.DefaultTableName, "MigrationId"));
                Assert.Equal(1, GetColumnIndex(HistoryContext.DefaultTableName, "ContextKey"));
                Assert.Equal(2, GetColumnIndex(HistoryContext.DefaultTableName, "Model"));
                Assert.Equal(3, GetColumnIndex(HistoryContext.DefaultTableName, "ProductVersion"));
            }
        }

        [MigrationsTheory]
        public void Upgrade_from_earlier_version_should_upgrade_history_table_when_updating_automatic()
        {
            ResetDatabase();

            var createTableOperations = GetLegacyHistoryCreateTableOperations();

            ExecuteOperations(
                createTableOperations.Concat(
                    new[]
                        {
                            new SqlOperation(
                                @"INSERT INTO [__MigrationHistory] ([MigrationId], [CreatedOn], [Model]) 
                                  VALUES ('000000000000000_ExistingMigration', GETDATE(), 0x1F8B0800000000000400ECBD07601C499625262F6DCA7B7F4AF54AD7E074A10880601324D8904010ECC188CDE692EC1D69472329AB2A81CA6556655D661640CCED9DBCF7DE7BEFBDF7DE7BEFBDF7BA3B9D4E27F7DFFF3F5C6664016CF6CE4ADAC99E2180AAC81F3F7E7C1F3F22FEC7BFF71F7CFC7BBC5B94E9655E3745B5FCECA3DDF1CE4769BE9C56B36279F1D947EBF67CFBE0A3DFE3E8374E1E9FCE16EFD29F34EDF6D08EDE5C369F7D346FDBD5A3BB779BE93C5F64CD78514CEBAAA9CEDBF1B45ADCCD66D5DDBD9D9D83BBBB3B777302F111C14AD3C7AFD6CBB658E4FC07FD79522DA7F9AA5D67E517D52C2F1BFD9CBE79CD50D317D9226F56D934FFEC236ADB54657EBC5A95C5346B099DDD8FD2E3B2C80895D77979FE9E78ED3C045E1FD91EA9CF53C2ADBDA67EDAAC58E63577CEFDB6F9BBF6A3F4AEC3EEAEA067867177601C8FBFC8562B22A8372EFD247DAD83DA7EFDFE782F04C6DD69B3097DDB535BD5D945DEF916E39AE5CF8ABA699F666D36C99AFCA3F464B688348B0C5F61DBF177C6F958FBBCCDA4769090261FA52FEBEAB2980181D7D74D9B2FC668307EFD8BCA93B2C897AD6BF045B62CCEF3A67D53BDCDC1A044A3AFCF160FEFEEEC812DEE36CDACBC156FF468B8814BFA54797CD71788C74FF3A6B820E89E782CF32958DD01356DCE96E715D16095D7EDF5EBBCF571354DCCD78AEC17799BCD08CFE3BA2DCEB3694B5F4FF3A6A179FB28FDC9AC5C5393D3C5249F9D2DBF5CB7AB757BDC34F962525E8763DADC3F8B4288F3E32F57F8CB6383AF3F0442B3A021E45F2E9FAC8B7266F17E969521ED874160CA3ECFE9735621AF5BFA995F5C5B482FAAE52D0129F99EE6AB7C3923967C932F5625016BBE5CBECE2EF361DC6EA66148B1C74F8BECA2CE163E05E513C5E475463D7B5D5007FE1BAE3FFAF3F15D28F4A3FF270000FFFF4817137F02060000)")
                        }).ToArray());

            var migrator = CreateMigrator<ShopContext_v1>();

            Assert.False(ColumnExists(HistoryContext.DefaultTableName, "ProductVersion"));
            Assert.False(ColumnExists(HistoryContext.DefaultTableName, "ContextKey"));
            Assert.True(ColumnExists(HistoryContext.DefaultTableName, "Hash"));
            Assert.True(ColumnExists(HistoryContext.DefaultTableName, "CreatedOn"));

            migrator.Update();

            Assert.True(ColumnExists(HistoryContext.DefaultTableName, "ProductVersion"));
            Assert.True(ColumnExists(HistoryContext.DefaultTableName, "ContextKey"));
            Assert.False(ColumnExists(HistoryContext.DefaultTableName, "Hash"));
            Assert.False(ColumnExists(HistoryContext.DefaultTableName, "CreatedOn"));

            if (ProviderInfo.ProviderInvariantName == "System.Data.SqlClient")
            {
                Assert.Equal(0, GetColumnIndex(HistoryContext.DefaultTableName, "MigrationId"));
                Assert.Equal(1, GetColumnIndex(HistoryContext.DefaultTableName, "ContextKey"));
                Assert.Equal(2, GetColumnIndex(HistoryContext.DefaultTableName, "Model"));
                Assert.Equal(3, GetColumnIndex(HistoryContext.DefaultTableName, "ProductVersion"));
            }
        }

        private static IEnumerable<MigrationOperation> GetLegacyHistoryCreateTableOperations()
        {
            const string tableName = "dbo." + HistoryContext.DefaultTableName;

            var createTableOperation
                = new CreateTableOperation(tableName);

            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.String)
                    {
                        Name = "MigrationId",
                        MaxLength = 255,
                        IsNullable = false
                    });

            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.DateTime)
                    {
                        Name = "CreatedOn"
                    });

            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.Binary)
                    {
                        Name = "Model"
                    });

            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.String)
                    {
                        Name = "Hash"
                    });

            yield return createTableOperation;

            var addPrimaryKeyOperation
                = new AddPrimaryKeyOperation
                      {
                          Table = tableName
                      };

            addPrimaryKeyOperation.Columns.Add("MigrationId");

            yield return addPrimaryKeyOperation;
        }
    }
}
