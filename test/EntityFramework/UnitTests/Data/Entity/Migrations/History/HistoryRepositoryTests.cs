namespace System.Data.Entity.Migrations
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Edm;
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Resources;
    using System.Data.Metadata.Edm;
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
                = new HistoryRepository(ConnectionString, ProviderFactory);

            var createTableOperation = (CreateTableOperation)
                                       historyRepository.CreateCreateTableOperation(new EdmModelDiffer());

            createTableOperation.Columns.Remove(createTableOperation.Columns.Last());

            ExecuteOperations(createTableOperation);

            var addColumnOperation = (AddColumnOperation)historyRepository.GetUpgradeOperations().Last();

            Assert.Equal("ProductVersion", addColumnOperation.Column.Name);
            Assert.Equal("0.7.0.0", addColumnOperation.Column.DefaultValue);
            Assert.Equal(32, addColumnOperation.Column.MaxLength);
            Assert.False(addColumnOperation.Column.IsNullable.Value);
        }

        [MigrationsTheory]
        public void GetUpgradeOperations_should_return_nothing_when_table_not_present()
        {
            ResetDatabase();

            var historyRepository
                = new HistoryRepository(ConnectionString, ProviderFactory);

            Assert.False(historyRepository.GetUpgradeOperations().Any());
        }

        [MigrationsTheory]
        public void GetMigrationsSince_should_return_all_migrations_when_target_is_empty()
        {
            ResetDatabase();

            var historyRepository
                = new HistoryRepository(ConnectionString, ProviderFactory);

            ExecuteOperations(historyRepository.CreateCreateTableOperation(new EdmModelDiffer()));

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                new[]
                        {
                            historyRepository.CreateInsertOperation("Migration1", model)
                        });

            ExecuteOperations(
                new[]
                        {
                            historyRepository.CreateInsertOperation("Migration2", model)
                        });

            var migrations = historyRepository.GetMigrationsSince(DbMigrator.InitialDatabase);

            Assert.Equal(2, migrations.Count());
            Assert.Equal("Migration2", migrations.First());
        }

        [MigrationsTheory]
        public void GetMigrationId_should_match_on_name()
        {
            ResetDatabase();

            var historyRepository
                = new HistoryRepository(ConnectionString, ProviderFactory);

            ExecuteOperations(historyRepository.CreateCreateTableOperation(new EdmModelDiffer()));

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                new[]
                        {
                            historyRepository.CreateInsertOperation("201109192032331_Migration1", model)
                        });

            ExecuteOperations(
                new[]
                        {
                            historyRepository.CreateInsertOperation("201109192032332_Migration2", model)
                        });

            var migrationId = historyRepository.GetMigrationId("Migration1");

            Assert.Equal("201109192032331_Migration1", migrationId);

            migrationId = historyRepository.GetMigrationId("migrATIon2");

            Assert.Equal("201109192032332_Migration2", migrationId);
        }

        [MigrationsTheory]
        public void GetMigrationId_should_throw_when_name_ambiguous()
        {
            ResetDatabase();

            var historyRepository
                = new HistoryRepository(ConnectionString, ProviderFactory);

            ExecuteOperations(historyRepository.CreateCreateTableOperation(new EdmModelDiffer()));

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                new[]
                        {
                            historyRepository.CreateInsertOperation("201109192032331_Migration", model)
                        });

            ExecuteOperations(
                new[]
                        {
                            historyRepository.CreateInsertOperation("201109192032332_Migration", model)
                        });

            Assert.Equal(Strings.AmbiguousMigrationName("Migration"), Assert.Throws<MigrationsException>(() => historyRepository.GetMigrationId("Migration")).Message);
        }

        [MigrationsTheory]
        public void GetMigrationId_should_return_null_when_no_database()
        {
            var historyRepository
            = new HistoryRepository(ConnectionString.Replace(DatabaseProviderFixture.DefaultDatabaseName, "NoSuchDatabase"), ProviderFactory);

            Assert.Null(historyRepository.GetMigrationId(DbMigrator.InitialDatabase));
        }

        [MigrationsTheory]
        public void GetMigrationsSince_should_return_subset_when_target_valid()
        {
            ResetDatabase();

            var historyRepository
                = new HistoryRepository(ConnectionString, ProviderFactory);

            ExecuteOperations(historyRepository.CreateCreateTableOperation(new EdmModelDiffer()));

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                new[]
                        {
                            historyRepository.CreateInsertOperation("Migration1", model)
                        });

            ExecuteOperations(
                new[]
                        {
                            historyRepository.CreateInsertOperation("Migration2", model)
                        });

            var migrations = historyRepository.GetMigrationsSince("Migration1");

            Assert.Equal(1, migrations.Count());
            Assert.Equal("Migration2", migrations.Single());
        }

        [MigrationsTheory]
        public void GetMigrationsSince_should_return_empty_when_target_valid_but_is_latest()
        {
            ResetDatabase();

            var historyRepository
                = new HistoryRepository(ConnectionString, ProviderFactory);

            ExecuteOperations(historyRepository.CreateCreateTableOperation(new EdmModelDiffer()));

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                new[]
                        {
                            historyRepository.CreateInsertOperation("Migration1", model)
                        });

            ExecuteOperations(
                new[]
                        {
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
                    ProviderFactory);

            Assert.False(historyRepository.GetPendingMigrations(Enumerable.Empty<string>()).Any());
        }

        [MigrationsTheory]
        public void GetPendingMigrations_should_return_empty_set_when_no_table()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory);

            Assert.False(historyRepository.GetPendingMigrations(Enumerable.Empty<string>()).Any());
        }

        [MigrationsTheory]
        public void GetPendingMigrations_should_return_empty_set_when_no_data()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory);

            Assert.False(historyRepository.GetPendingMigrations(Enumerable.Empty<string>()).Any());
        }

        [MigrationsTheory]
        public void GetPendingMigrations_should_return_migrations_not_in_input_set()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory);

            var model = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                historyRepository.CreateCreateTableOperation(new EdmModelDiffer()),
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
        public void GetLastModel_should_return_null_when_no_database()
        {
            var historyRepository
                = new HistoryRepository(
                ConnectionString.Replace(DatabaseProviderFixture.DefaultDatabaseName, "NoSuchDatabase"),
                    ProviderFactory);

            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();

            Assert.Null(historyRepository.GetLastModel());
        }

        [MigrationsTheory]
        public void GetLastModel_should_return_null_when_no_table()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory);
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();

            Assert.Null(historyRepository.GetLastModel());
        }

        [MigrationsTheory]
        public void GetLastModel_should_return_null_when_no_data()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory);
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();

            Assert.Null(historyRepository.GetLastModel());
        }

        [MigrationsTheory]
        public void GetLastModel_should_return_model_when_row()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory);

            var model1 = CreateContext<ShopContext_v1>().GetModel();

            ExecuteOperations(
                historyRepository.CreateCreateTableOperation(new EdmModelDiffer()),
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
        public void Exists_should_return_false_when_no_database()
        {
            var historyRepository
                = new HistoryRepository(
                ConnectionString.Replace(DatabaseProviderFixture.DefaultDatabaseName, "NoSuchDatabase"),
                    ProviderFactory);

            Assert.False(historyRepository.Exists());
        }

        [MigrationsTheory]
        public void Exists_should_return_false_when_no_table()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory);

            Assert.False(historyRepository.Exists());
        }

        [MigrationsTheory]
        public void Exists_should_return_true_when_database_and_table()
        {
            ResetDatabase();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory);

            ExecuteOperations(historyRepository.CreateCreateTableOperation(new EdmModelDiffer()));

            Assert.True(historyRepository.Exists());
        }

        [MigrationsTheory]
        public void GetCreateOperation_should_return_valid_create_table_operation()
        {
            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory);

            var createTableOperation = (CreateTableOperation)
                                       historyRepository.CreateCreateTableOperation(new EdmModelDiffer());

            Assert.Equal(HistoryContext.TableName, createTableOperation.Name);
            Assert.True((bool)createTableOperation.AnonymousArguments["IsMSShipped"]);
            Assert.Equal(3, createTableOperation.Columns.Count());

            var migrationColumn = createTableOperation.Columns.Single(c => c.Name == "MigrationId");
            Assert.Equal(PrimitiveTypeKind.String, migrationColumn.Type);
            Assert.Equal(255, migrationColumn.MaxLength);
            Assert.False(migrationColumn.IsNullable.Value);

            var modelColumn = createTableOperation.Columns.Single(c => c.Name == "Model");
            Assert.Equal(PrimitiveTypeKind.Binary, modelColumn.Type);
            Assert.False(modelColumn.IsNullable.Value);
            Assert.Null(modelColumn.MaxLength);

            var previousVersionColumn = createTableOperation.Columns.Single(c => c.Name == "ProductVersion");
            Assert.Equal(PrimitiveTypeKind.String, previousVersionColumn.Type);
            Assert.Equal(32, previousVersionColumn.MaxLength);
            Assert.False(previousVersionColumn.IsNullable.Value);
        }

        [MigrationsTheory]
        public void CreateInsertOperation_should_return_valid_add_operation()
        {
            var modelBuilder = new DbModelBuilder();
            var model = modelBuilder.Build(ProviderInfo);

            var edmxString = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(edmxString, new XmlWriterSettings { Indent = true }))
            {
                EdmxWriter.WriteEdmx(model, xmlWriter);
            }

            var modelDocument = model.ToXDocument();

            var historyRepository = new HistoryRepository(ConnectionString, ProviderFactory);
            var insertHistoryOperation
                = (InsertHistoryOperation)historyRepository.CreateInsertOperation("Migration1", modelDocument);

            Assert.Equal("Migration1", insertHistoryOperation.MigrationId);
            Assert.Equal((object)new ModelCompressor().Compress(modelDocument), (object)insertHistoryOperation.Model);
        }

        [MigrationsTheory]
        public void GetLastModel_gets_latest_based_on_MigrationId_not_on_CreatedOn()
        {
            var historyRepository = SetupHistoryRepositoryForOrderingTest();

            string migrationId;
            historyRepository.GetLastModel(out migrationId);

            Assert.Equal("227309030010001_Migration1", migrationId);
        }

        [MigrationsTheory]
        public void GetMigrationsSince_gets_migrations_based_on_MigrationId_not_on_CreatedOn()
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

                using (var historyContext = new LegacyHistoryContext(clonedConnection))
                {
                    context.Database.ExecuteSqlCommand(
                        ((IObjectContextAdapter)historyContext).ObjectContext.CreateDatabaseScript());

                    historyContext.History.Add(
                        new HistoryRow
                        {
                            MigrationId = "227309030010001_Migration1",
#pragma warning disable 612,618
                            CreatedOn = new DateTime(2273, 9, 3, 0, 10, 0, 0, DateTimeKind.Utc),
#pragma warning restore 612,618
                            Model = new ModelCompressor().Compress(model),
                            ProductVersion = "",
                        });

                    historyContext.History.Add(
                        new HistoryRow
                        {
                            MigrationId = "227209030010001_Migration2", // Id is before
#pragma warning disable 612,618
                            CreatedOn = new DateTime(2274, 9, 3, 0, 10, 0, 0, DateTimeKind.Utc), // CreatedOn is after
#pragma warning restore 612,618
                            Model = new ModelCompressor().Compress(model),
                            ProductVersion = "",
                        });

                    historyContext.SaveChanges();
                }
            }

            return new HistoryRepository(ConnectionString, ProviderFactory);
        }
    }
}