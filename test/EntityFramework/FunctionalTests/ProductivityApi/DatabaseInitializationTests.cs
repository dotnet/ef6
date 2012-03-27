namespace ProductivityApiTests
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Metadata.Edm;
    using System.Linq;
    using System.Transactions;
    using BadMappingModel;
    using ConcurrencyModel;
    using FunctionalTests.SimpleMigrationsModel;
    using SimpleModel;
    using Xunit;

    /// <summary>
    /// Functional tests for database initialization and seeding.  Unit tests also exist in the unit tests project.
    /// </summary>
    public class DatabaseInitializationTests : FunctionalTestBase
    {
        #region Infrastructure/setup

        private const string MigrationHistoryTableName = "__MigrationHistory";

        static DatabaseInitializationTests()
        {
            CreateMetadataFilesForSimpleModel();
        }

        #endregion

        #region Database initializers and associated contexts for testing

        private class SimpleContextForDropCreateDatabaseAlways : SimpleModelContext
        {
        }

        private class SimpleDropCreateDatabaseAlways :
            DropCreateDatabaseAlways<SimpleContextForDropCreateDatabaseAlways>
        {
            protected override void Seed(SimpleContextForDropCreateDatabaseAlways context)
            {
                context.Categories.Add(new Category("Watchers"));
            }
        }

        private class SimpleContextForCreateDatabaseIfNotExists : SimpleModelContext
        {
            public SimpleContextForCreateDatabaseIfNotExists()
            {
            }

            public SimpleContextForCreateDatabaseIfNotExists(DbCompiledModel model)
                : base(model)
            {
            }
        }

        private class SimpleCreateDatabaseIfNotExists :
            CreateDatabaseIfNotExists<SimpleContextForCreateDatabaseIfNotExists>
        {
            protected override void Seed(SimpleContextForCreateDatabaseIfNotExists context)
            {
                context.Categories.Add(new Category("Watchers"));
            }
        }

        private class SimpleContextForDropCreateDatabaseIfModelChanges : SimpleModelContext
        {
        }

        private class SimpleDropCreateDatabaseIfModelChanges :
            DropCreateDatabaseIfModelChanges<SimpleContextForDropCreateDatabaseIfModelChanges>
        {
            protected override void Seed(SimpleContextForDropCreateDatabaseIfModelChanges context)
            {
                context.Categories.Add(new Category("Watchers"));
            }
        }

        public class InitializerFromAppConfigContext : DbContext
        {
        }

        public class InitializerFromAppConfig : IDatabaseInitializer<InitializerFromAppConfigContext>
        {
            public void InitializeDatabase(InitializerFromAppConfigContext context)
            {
                InitializerCalled = true;
            }

            public static bool InitializerCalled { get; set; }
        }

        public class InitializerWithCtorArgsContext : DbContext
        {
        }

        public class InitializerWithCtorArgs : IDatabaseInitializer<InitializerWithCtorArgsContext>
        {
            public InitializerWithCtorArgs(string arg1, int arg2)
            {
                CtorArg1 = arg1;
                CtorArg2 = arg2;
            }

            public void InitializeDatabase(InitializerWithCtorArgsContext context)
            {
                InitializerCalled = true;
            }

            public static bool InitializerCalled { get; set; }

            public static string CtorArg1 { get; set; }

            public static int? CtorArg2 { get; set; }
        }

        #endregion

        #region Positive DropCreateDatabaseAlways strategy tests

        [Fact]
        public void DropCreateDatabaseAlways_performs_delete_create_and_seeding()
        {
            Database.Delete(SimpleConnection<SimpleContextForDropCreateDatabaseAlways>());
            Database.SetInitializer(new SimpleDropCreateDatabaseAlways());

            using (var context = new SimpleContextForDropCreateDatabaseAlways())
            {
                context.Database.Initialize(force: true);

                // Check that the database is created and seeded
                Assert.Equal("Watchers", context.Categories.Single().Id);

                // Now add some more data
                context.Categories.Add(new Category("Slayers"));
                context.SaveChanges();

                // Now force initializer to run again.  Database will be deleted and recreated so above data will be gone.
                context.Database.Initialize(force: true);

                Assert.Equal(1, context.Categories.Count());
                Assert.Equal("Watchers", context.Categories.Single().Id);
            }
        }

        [Fact]
        public void Initializer_that_inserts_data_should_not_have_side_effects()
        {
            Database.SetInitializer(new SimpleDropCreateDatabaseAlways());

            using (var context = new SimpleContextForDropCreateDatabaseAlways())
            {
                // Force the initializer to re-run
                context.Database.Initialize(force: true);

                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public void Transaction_can_be_started_after_database_initialization_has_happened()
        {
            Database.Delete(SimpleConnection<SimpleContextForDropCreateDatabaseAlways>());
            Database.SetInitializer(new SimpleDropCreateDatabaseAlways());

            using (var context = new SimpleContextForDropCreateDatabaseAlways())
            {
                // SQL Server doesn't allow Create/Drop Database actions within a transaction.
                context.Database.Initialize(force: true);

                // Database Initializer shouldn't start a transaction on it's own
                Assert.Equal(0, GetTransactionCount(context.Database.Connection));

                using (new TransactionScope())
                {
                    // Now add some more data
                    context.Categories.Add(new Category("Watchers2"));
                    context.SaveChanges();

                    Assert.Equal(1, GetTransactionCount(context.Database.Connection));
                    Assert.True(context.Categories.Where(c => c.Id == "Watchers2").AsNoTracking().Any());
                }
            }

            using (var context = new SimpleContextForDropCreateDatabaseAlways())
            {
                Assert.False(context.Categories.Where(c => c.Id == "Watchers2").AsNoTracking().Any());
            }
        }

        #endregion

        #region Positive CreateDatabaseIfNotExists strategy tests

        [Fact]
        public void CreateDatabaseIfNotExists_creates_and_seeds_database_if_not_exists()
        {
            Database.Delete(SimpleConnection<SimpleContextForCreateDatabaseIfNotExists>());
            Database.SetInitializer(new SimpleCreateDatabaseIfNotExists());

            using (var context = new SimpleContextForCreateDatabaseIfNotExists())
            {
                context.Database.Initialize(force: true);

                // Check that the database is created and seeded
                Assert.Equal("Watchers", context.Categories.Single().Id);
            }
        }

        [Fact]
        public void CreateDatabaseIfNotExists_does_nothing_if_database_exists_and_model_matches()
        {
            Database.Delete(SimpleConnection<SimpleContextForCreateDatabaseIfNotExists>());
            Database.SetInitializer(new SimpleCreateDatabaseIfNotExists());

            using (var context = new SimpleContextForCreateDatabaseIfNotExists())
            {
                Initializer_does_nothing_if_database_exists_and_model_matches(context);
            }
        }

        [Fact]
        public void CreateDatabaseIfNotExists_in_transaction_does_nothing_if_database_exists_and_model_matches()
        {
            Database.Delete(SimpleConnection<SimpleContextForCreateDatabaseIfNotExists>());
            Database.SetInitializer(new SimpleCreateDatabaseIfNotExists());

            using (var context = new SimpleContextForCreateDatabaseIfNotExists())
            {
                Initializer_does_nothing_if_database_exists_and_model_matches(context, useTransaction: true,
                                                                              useLocal: false);
            }
        }

        [Fact]
        public void CreateDatabaseIfNotExists_in_local_transaction_does_nothing_if_database_exists_and_model_matches()
        {
            Database.Delete(SimpleConnection<SimpleContextForCreateDatabaseIfNotExists>());
            Database.SetInitializer(new SimpleCreateDatabaseIfNotExists());

            using (var context = new SimpleContextForCreateDatabaseIfNotExists())
            {
                Initializer_does_nothing_if_database_exists_and_model_matches(context, useTransaction: true,
                                                                              useLocal: true);
            }
        }

        [Fact]
        public void DropCreateDatabaseIfModelChanges_in_transaction_does_nothing_if_database_exists_and_model_matches()
        {
            Database.Delete(SimpleConnection<SimpleContextForDropCreateDatabaseIfModelChanges>());
            Database.SetInitializer(new SimpleDropCreateDatabaseIfModelChanges());

            using (var context = new SimpleContextForDropCreateDatabaseIfModelChanges())
            {
                Initializer_does_nothing_if_database_exists_and_model_matches(context, useTransaction: true,
                                                                              useLocal: false);
            }
        }

        [Fact]
        public void
            DropCreateDatabaseIfModelChanges_in_local_transaction_does_nothing_if_database_exists_and_model_matches()
        {
            Database.Delete(SimpleConnection<SimpleContextForDropCreateDatabaseIfModelChanges>());
            Database.SetInitializer(new SimpleDropCreateDatabaseIfModelChanges());

            using (var context = new SimpleContextForDropCreateDatabaseIfModelChanges())
            {
                Initializer_does_nothing_if_database_exists_and_model_matches(context, useTransaction: true,
                                                                              useLocal: true);
            }
        }

        private void Initializer_does_nothing_if_database_exists_and_model_matches(SimpleModelContext context,
                                                                                   bool useTransaction = false,
                                                                                   bool useLocal = false)
        {
            context.Database.Initialize(force: true);

            // Check that the database is created and seeded
            Assert.Equal("Watchers", context.Categories.Single().Id);

            // Now add some more data
            context.Categories.Add(new Category("Slayers"));
            context.SaveChanges();

            DbTransaction localTransaction = null;

            // Now force initializer to run again.  The database should not be deleted or re-seeded.
            if (useTransaction)
            {
                if (useLocal)
                {
                    // Begin a local transaction
                    localTransaction = BeginLocalTransaction(context);

                    // This call should succeed even under transaction since database initialization does nothing here.
                    context.Database.Initialize(force: true);

                    // Even if transaction is committed nothing should have changed.
                    localTransaction.Commit();
                }

                using (var transaction = new TransactionScope())
                {
                    // This call should succeed even under transaction since database initialization does nothing here.
                    context.Database.Initialize(force: true);

                    // Even if transaction is committed nothing should have changed.
                    transaction.Complete();
                }
            }
            else
            {
                context.Database.Initialize(force: true);
            }

            var categoriesInDatabase = context.Categories.AsNoTracking();
            Assert.Equal(2, categoriesInDatabase.Count());
            Assert.True(categoriesInDatabase.Any(c => c.Id == "Watchers"));
            Assert.True(categoriesInDatabase.Any(c => c.Id == "Slayers"));


            if (localTransaction != null)
            {
                CloseEntityConnection(context);
            }
        }

        public class EdmMetadataPokerContext : DbContext
        {
            static EdmMetadataPokerContext()
            {
                Database.SetInitializer<EdmMetadataPokerContext>(null);
            }

            public EdmMetadataPokerContext(DbConnection existingConnection)
                : base(existingConnection, contextOwnsConnection: false)
            {
            }

#pragma warning disable 612,618
            public virtual DbSet<EdmMetadata> Metadata { get; set; }
#pragma warning restore 612,618

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
#pragma warning disable 612,618
                modelBuilder.Entity<EdmMetadata>().ToTable("EdmMetadata");
#pragma warning restore 612,618
            }
        }

        private static void DropMigrationHistoryAndAddEdmMetadata(DbConnection connection, string hash)
        {
            using (var poker = new EdmMetadataPokerContext(connection))
            {
                poker.Database.ExecuteSqlCommand("drop table " + MigrationHistoryTableName);

                poker.Database.ExecuteSqlCommand(
                    ((IObjectContextAdapter)poker).ObjectContext.CreateDatabaseScript());

#pragma warning disable 612,618
                poker.Metadata.Add(new EdmMetadata { ModelHash = hash });
#pragma warning restore 612,618

                poker.SaveChanges();
            }
        }

        public class HistoryPokerContext : DbContext
        {
            static HistoryPokerContext()
            {
                Database.SetInitializer<HistoryPokerContext>(null);
            }

            public HistoryPokerContext(DbConnection existingConnection)
                : base(existingConnection, contextOwnsConnection: false)
            {
            }

            public virtual DbSet<System.Data.Entity.Migrations.History.HistoryRow> History { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<System.Data.Entity.Migrations.History.HistoryRow>().ToTable(MigrationHistoryTableName);
                modelBuilder.Entity<System.Data.Entity.Migrations.History.HistoryRow>().HasKey(h => h.MigrationId);
#pragma warning disable 612,618
                modelBuilder.Entity<System.Data.Entity.Migrations.History.HistoryRow>().Ignore(h => h.CreatedOn);
#pragma warning restore 612,618
            }
        }

        private byte[] _compressedEmptyModel;

        private byte[] CompressedEmptyModel
        {
            get
            {
                if (_compressedEmptyModel == null)
                {
                    using (var context = new EmptyContext())
                    {
                        context.Database.Initialize(force: true);

                        using (var peeker = new HistoryPokerContext(context.Database.Connection))
                        {
                            _compressedEmptyModel = peeker.History.Single().Model;
                        }
                    }
                }
                return _compressedEmptyModel;
            }
        }

        private void MutateMigrationsHistory(DbConnection connection)
        {
            using (var poker = new HistoryPokerContext(connection))
            {
                poker.History.Single().Model = CompressedEmptyModel;
                poker.SaveChanges();
            }
        }

        [Fact]
        public void CreateDatabaseIfNotExists_throws_if_database_exists_and_model_does_not_match()
        {
            Database.Delete(SimpleConnection<SimpleContextForCreateDatabaseIfNotExists>());
            Database.SetInitializer(new SimpleCreateDatabaseIfNotExists());

            using (var context = new SimpleContextForCreateDatabaseIfNotExists())
            {
                context.Database.Initialize(force: true);

                // Check that the database is created and seeded
                Assert.Equal("Watchers", context.Categories.Single().Id);

                // Now tweak the model hash so it looks like the database doesn't match.
                MutateMigrationsHistory(context.Database.Connection);

                // Now force initializer to run again; it should throw.
                Assert.Throws<InvalidOperationException>(() => context.Database.Initialize(force: true)).ValidateMessage
                    ("DatabaseInitializationStrategy_ModelMismatch", context.GetType().Name);
            }
        }

        [Fact]
        public void Local_transaction_can_be_started_after_database_initialization_has_happened()
        {
            Database.Delete(SimpleConnection<SimpleContextForCreateDatabaseIfNotExists>());
            Database.SetInitializer(new SimpleCreateDatabaseIfNotExists());

            using (var context = new SimpleContextForCreateDatabaseIfNotExists())
            {
                // SQL Server doesn't allow Create/Drop Database actions within a transaction.
                context.Database.Initialize(force: true);

                // Check that the database is created and seeded
                Assert.Equal("Watchers", context.Categories.Single().Id);

                // Database Initializer shouldn't start a transaction on it's own
                Assert.Equal(0, GetTransactionCount(context.Database.Connection));

                // Begin a local transaction
                var transaction = BeginLocalTransaction(context);

                context.Categories.Find("Watchers").DetailedDescription = "those Watching";
                context.SaveChanges();

                transaction.Commit();
                CloseEntityConnection(context);

                Assert.Equal("those Watching", context.Categories.AsNoTracking().Single().DetailedDescription);
            }

            using (var context = new SimpleContextForCreateDatabaseIfNotExists())
            {
                Assert.Equal("those Watching", context.Categories.Single().DetailedDescription);
            }
        }

        [Fact]
        public void MigrationHistory_table_is_created_and_populated_even_for_a_database_created_with_an_empty_model()
        {
            using (var context = new EmptyContext())
            {
                context.Database.Delete();
                context.Database.Create();

                using (var poker = new HistoryPokerContext(context.Database.Connection))
                {
                    var historyRow = poker.History.Single();

                    Assert.NotNull(historyRow.Model);
                    Assert.True(historyRow.MigrationId.EndsWith("InitialCreate"));
                }
            }
        }

        #endregion

        #region Positive DropCreateDatabaseIfModelChanges strategy tests

        [Fact]
        public void DropCreateDatabaseIfModelChanges_creates_and_seeds_database_if_not_exists()
        {
            Database.Delete(SimpleConnection<SimpleContextForDropCreateDatabaseIfModelChanges>());
            Database.SetInitializer(new SimpleDropCreateDatabaseIfModelChanges());

            using (var context = new SimpleContextForDropCreateDatabaseIfModelChanges())
            {
                context.Database.Initialize(force: true);

                // Check that the database is created and seeded
                Assert.Equal("Watchers", context.Categories.Single().Id);
            }
        }

        [Fact]
        public void DropCreateDatabaseIfModelChanges_SetInitializer_in_systran_with_no_model_changes()
        {
            Database.Delete(SimpleConnection<SimpleContextForDropCreateDatabaseIfModelChanges>());
            Database.SetInitializer(new SimpleDropCreateDatabaseIfModelChanges());

            using (var context = new SimpleContextForDropCreateDatabaseIfModelChanges())
            {
                context.Database.Initialize(force: true);
                using (TransactionScope tx = new TransactionScope())
                {
                    Assert.Equal(1, GetTransactionCount(context.Database.Connection));

                    // This should work in the transaction since Drop, Create statements aren't made since model didn't change.
                    context.Database.Initialize(force: true);

                    // Database Initializer shouldn't start a transaction on it's own, so the transaction count should still be the same
                    Assert.Equal(1, GetTransactionCount(context.Database.Connection));
                }
            }
        }

        [Fact]
        public void DropCreateDatabaseIfModelChanges_does_nothing_if_database_exists_and_model_matches()
        {
            Database.Delete(SimpleConnection<SimpleContextForDropCreateDatabaseIfModelChanges>());
            Database.SetInitializer(new SimpleDropCreateDatabaseIfModelChanges());

            using (var context = new SimpleContextForDropCreateDatabaseIfModelChanges())
            {
                Initializer_does_nothing_if_database_exists_and_model_matches(context);
            }
        }

        [Fact]
        public void DropCreateDatabaseIfModelChanges_recreates_database_if_database_exists_and_model_does_not_match()
        {
            Database.Delete(SimpleConnection<SimpleContextForDropCreateDatabaseIfModelChanges>());
            Database.SetInitializer(new SimpleDropCreateDatabaseIfModelChanges());

            using (var context = new SimpleContextForDropCreateDatabaseIfModelChanges())
            {
                context.Database.Initialize(force: true);

                // Check that the database is created and seeded
                Assert.Equal("Watchers", context.Categories.Single().Id);

                // Now add some more data
                context.Categories.Add(new Category("Slayers"));
                context.SaveChanges();

                // Now tweak the model hash so it looks like the database doesn't match.
                MutateMigrationsHistory(context.Database.Connection);

                // Now force initializer to run again.  Database will be deleted and recreated so above data will be gone.
                context.Database.Initialize(force: true);

                Assert.Equal(1, context.Categories.Count());
                Assert.Equal("Watchers", context.Categories.Single().Id);
            }
        }

        #endregion

        #region Positive MigrateDatabaseToLatestVersion strategy tests

        [Fact]
        public void MigrateDatabaseToLatestVersion_migrates_to_latest_version()
        {
            using (var ctx = new MigrateInitializerContext())
            {
                ctx.Database.Delete();
            }

            using (var ctx = new MigrateInitializerContext())
            {
                Assert.False(ctx.Database.Exists());

                ctx.Blogs.FirstOrDefault();

                // Ensure seed data was applied
                var blogs = ctx.Blogs.ToArray();
                Assert.Equal(2, blogs.Count());
                Assert.True(blogs.Any(b => b.Name == "romiller.com"));
                Assert.True(blogs.Any(b => b.Name == "blogs.msdn.com\adonet"));
            }

            // Ensure all migrations were applied
            var appliedMigrations = new System.Data.Entity.Migrations.DbMigrator(new MigrateInitializerConfiguration()).GetDatabaseMigrations();
            Assert.Equal(2, appliedMigrations.Count());
            Assert.True(appliedMigrations.Contains("201112202056275_InitialCreate"));
            Assert.True(appliedMigrations.Contains("201112202056573_AddUrlToBlog"));
        }

        #endregion

        #region CompatibleWithModel positive tests

        [Fact]
        public void CompatibleWithModel_returns_true_when_current_model_matches_model_in_database()
        {
            Database.Delete(SimpleConnection<SimpleContextForCreateDatabaseIfNotExists>());
            Database.SetInitializer(new SimpleCreateDatabaseIfNotExists());

            using (var context = new SimpleContextForCreateDatabaseIfNotExists())
            {
                context.Database.Initialize(force: true);

                Assert.True(context.Database.CompatibleWithModel(throwIfNoMetadata: true));
            }
        }

        [Fact]
        public void CompatibleWithModel_returns_true_when_MigrationHistory_table_is_missing_but_EdmMetadata_hash_matches()
        {
            Database.Delete(SimpleConnection<SimpleContextForCreateDatabaseIfNotExists>());
            Database.SetInitializer(new SimpleCreateDatabaseIfNotExists());

            using (var context = new SimpleContextForCreateDatabaseIfNotExists())
            {
                context.Database.Initialize(force: true);

#pragma warning disable 612,618
                DropMigrationHistoryAndAddEdmMetadata(context.Database.Connection, EdmMetadata.TryGetModelHash(context));
#pragma warning restore 612,618

                Assert.True(context.Database.CompatibleWithModel(throwIfNoMetadata: true));
            }
        }

        [Fact]
        public void CompatibleWithModel_returns_false_when_current_model_does_not_match_model_in_database()
        {
            Database.SetInitializer(new SimpleCreateDatabaseIfNotExists());

            using (var context = new SimpleContextForCreateDatabaseIfNotExists())
            {
                context.Database.Delete();
                context.Database.Create();

                VerifyMigrationsHistoryTable(context, historyShouldExist: true);

                // Now tweak the model hash so it looks like the database doesn't match.
                MutateMigrationsHistory(context.Database.Connection);

                Assert.False(context.Database.CompatibleWithModel(throwIfNoMetadata: true));
            }
        }

        [Fact]
        public void CompatibleWithModel_returns_false_when_MigrationHistory_table_is_missing_and_EdmMetadata_hash_does_not_match()
        {
            Database.SetInitializer(new SimpleCreateDatabaseIfNotExists());

            using (var context = new SimpleContextForCreateDatabaseIfNotExists())
            {
                context.Database.Delete();
                context.Database.Create();

                DropMigrationHistoryAndAddEdmMetadata(context.Database.Connection, "Hash Mismatch");

                Assert.False(context.Database.CompatibleWithModel(throwIfNoMetadata: true));
            }
        }

        private class SimpleContextWithNoMetadata : SimpleModelContext
        {
            public SimpleContextWithNoMetadata()
            {
            }

            public SimpleContextWithNoMetadata(DbCompiledModel model)
                : base(model)
            {
            }
        }

        [Fact]
        public void CompatibleWithModel_throws_or_returns_true_if_metadata_table_is_not_in_database()
        {
            Database.SetInitializer<SimpleContextWithNoMetadata>(null);

            // Create a database that doesn't include the model metadata.
            var modelBuilder = SimpleModelContext.CreateBuilder();

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo).Compile();

            using (var context = new SimpleContextWithNoMetadata(model))
            {
                context.Database.Delete();
                context.Database.Create();

                context.Database.ExecuteSqlCommand("drop table " + MigrationHistoryTableName);

                VerifyMigrationsHistoryTable(context, historyShouldExist: false);
            }

            // Now create a new context around a model that does contain model metadata.
            using (var context = new SimpleContextWithNoMetadata())
            {
                Assert.True(context.Database.Exists());
                Assert.True(context.Database.CompatibleWithModel(throwIfNoMetadata: false));
                Assert.Throws<NotSupportedException>(() => context.Database.CompatibleWithModel(throwIfNoMetadata: true))
                    .ValidateMessage("Database_NoDatabaseMetadata");
            }
        }

        [Fact]
        public void CompatibleWithModel_throws_or_returns_true_if_metadata_table_contains_no_row()
        {
            Database.Delete(SimpleConnection<SimpleContextForCreateDatabaseIfNotExists>());
            Database.SetInitializer(new SimpleCreateDatabaseIfNotExists());

            using (var context = new SimpleContextForCreateDatabaseIfNotExists())
            {
                context.Database.Initialize(force: true);

                // Now corrupt the metadata info.
                using (var poker = new HistoryPokerContext(context.Database.Connection))
                {
                    poker.History.Remove(poker.History.Single());
                    poker.SaveChanges();
                }

                Assert.True(context.Database.CompatibleWithModel(throwIfNoMetadata: false));
                Assert.Throws<NotSupportedException>(() => context.Database.CompatibleWithModel(throwIfNoMetadata: true))
                    .ValidateMessage("Database_NoDatabaseMetadata");
            }
        }

        [Fact]
        public void CompatibleWithModel_throws_or_returns_true_with_DbContext_created_from_ObjectContext()
        {
            using (var context = new F1Context(GetObjectContext(new F1Context()), dbContextOwnsObjectContext: true))
            {
                Assert.True(context.Database.CompatibleWithModel(throwIfNoMetadata: false));
                Assert.Throws<NotSupportedException>(() => context.Database.CompatibleWithModel(throwIfNoMetadata: true))
                    .ValidateMessage("Database_NonCodeFirstCompatibilityCheck");
            }
        }

        [Fact]
        public void CompatibleWithModel_throws_or_returns_true_with_DbContext_created_from_existing_EDMX()
        {
            using (var context = new SimpleModelContext(SimpleModelEntityConnectionString))
            {
                Assert.True(context.Database.CompatibleWithModel(throwIfNoMetadata: false));
                Assert.Throws<NotSupportedException>(() => context.Database.CompatibleWithModel(throwIfNoMetadata: true))
                    .ValidateMessage("Database_NonCodeFirstCompatibilityCheck");
            }
        }

        #endregion

        #region Database creation positive tests

        public class SchemaTable
        {
            public string name { get; set; }
        }

        private void VerifyMigrationsHistoryTable(SimpleModelContext context, bool historyShouldExist)
        {
            var tables =
                GetObjectContext(context).ExecuteStoreQuery<SchemaTable>("SELECT name FROM sys.Tables").ToList();

            Assert.Equal(historyShouldExist, tables.Any(t => t.name == MigrationHistoryTableName));

            // Sanity check that the other tables are still there and that we're querying for the correct database.
            Assert.True(tables.Any(t => t.name == "Products"));
            Assert.True(tables.Any(t => t.name == "Categories"));
        }

        #endregion

        #region Database initializer set in app.config tests

        [Fact]
        public void Database_initializer_can_be_set_in_app_config()
        {
            using (var context = new InitializerFromAppConfigContext())
            {
                Assert.False(InitializerFromAppConfig.InitializerCalled);

                context.Database.Initialize(force: true);

                Assert.True(InitializerFromAppConfig.InitializerCalled);
            }
        }

        [Fact]
        public void Database_initializer_with_ctor_args_can_be_set_in_app_config()
        {
            using (var context = new InitializerWithCtorArgsContext())
            {
                Assert.False(InitializerWithCtorArgs.InitializerCalled);

                context.Database.Initialize(force: true);

                Assert.True(InitializerWithCtorArgs.InitializerCalled);
                Assert.Equal("TestArgumentOne", InitializerWithCtorArgs.CtorArg1);
                Assert.Equal(2, InitializerWithCtorArgs.CtorArg2);
            }
        }

        [Fact]
        public void Database_initialization_can_be_disabled_in_app_config()
        {
            using (var context = new DisabledByConfigInitializerContext())
            {
                // Delete it just in case some previous failed run created the database.
                context.Database.Delete();

                context.Database.Initialize(force: true);

                Assert.False(context.Database.Exists());
            }
        }

        [Fact]
        public void Database_initialization_can_be_disabled_in_app_config_if_initializer_is_also_set_in_config()
        {
            using (var context = new DisabledByConfigWithInitializerAlsoSetInConfigContext())
            {
                // Delete it just in case some previous failed run created the database.
                context.Database.Delete();

                context.Database.Initialize(force: true);

                Assert.False(context.Database.Exists());
            }
        }

        #endregion

        #region Legacy database initializer set in app.config tests

        [Fact]
        public void Database_initialization_can_be_disabled_by_Disabled_legacy_entry_in_app_config()
        {
            using (var context = new DisabledByLegacyConfigInitializerContext())
            {
                // Delete it just in case some previous failed run created the database.
                context.Database.Delete();

                context.Database.Initialize(force: true);

                Assert.False(context.Database.Exists());
            }
        }

        [Fact]
        public void Database_initialization_can_be_disabled_by_legacy_emptyentry_in_app_config()
        {
            using (var context = new DisabledByLegacyConfigWithEmptyInitializerContext())
            {
                // Delete it just in case some previous failed run created the database.
                context.Database.Delete();

                context.Database.Initialize(force: true);

                Assert.False(context.Database.Exists());
            }
        }

        #endregion

        #region Initialization with bad mapping

        public class BizContextWithNoEdmMetadata : DbContext
        {
            public DbSet<Employee> Employees { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Employee>().Map(mc => mc.Requires("Disc").HasValue("A"));
                modelBuilder.Entity<OnSiteEmployee>().Map(mc => mc.Requires("Disc").HasValue("A"));
                modelBuilder.Entity<OffSiteEmployee>().Map(mc => mc.Requires("Disc").HasValue("B"));
                modelBuilder.Entity<OnSiteEmployee>().HasRequired(e => e.Office);
            }
        }

        public class InitializerForBadMappingCase : DropCreateDatabaseAlways<BizContextWithNoEdmMetadata>
        {
            protected override void Seed(BizContextWithNoEdmMetadata context)
            {
                context.Employees.Add(new OnSiteEmployee());
            }
        }

        // See Dev11 bug 136276
        [Fact]
        public void Using_model_with_bad_mapping_but_no_EdmMetadata_table_should_result_in_DataException_containing_a_MappingException()
        {
            Database.SetInitializer(new InitializerForBadMappingCase());
            using (var context = new BizContextWithNoEdmMetadata())
            {
                try
                {
                    context.Employees.FirstOrDefault();
                    Assert.True(false);
                }
                catch (DataException ex)
                {
                    // Just validate that we got the expected exception types.
                    // The actual contents of the exception message is hard to validate and is not
                    // directly relevant to this test case--if comes from core EF.
                    var dbUpdateException = ex.InnerException;
                    Assert.IsType<DbUpdateException>(dbUpdateException);

                    var updateException = dbUpdateException.InnerException;
                    Assert.IsType<UpdateException>(updateException);

                    var mappingException = updateException.InnerException;
                    Assert.IsType<MappingException>(mappingException);
                }
            }
        }

        public class BizContextWithNoEdmMetadata2 : DbContext
        {
            public DbSet<Employee> Employees { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Employee>().Map(mc => mc.Requires("Disc").HasValue("A"));
                modelBuilder.Entity<OnSiteEmployee>().Map(mc => mc.Requires("Disc").HasValue("A"));
                modelBuilder.Entity<OffSiteEmployee>().Map(mc => mc.Requires("Disc").HasValue("B"));
                modelBuilder.Entity<OnSiteEmployee>().HasRequired(e => e.Office);
            }
        }

        public class InitializerForBadMappingCaseWithQuery : DropCreateDatabaseAlways<BizContextWithNoEdmMetadata2>
        {
            protected override void Seed(BizContextWithNoEdmMetadata2 context)
            {
                context.Employees.FirstOrDefault();
            }
        }

        // See Dev11 bug 136276
        [Fact]
        public void Using_model_with_bad_mapping_but_no_EdmMetadata_table_and_initializer_that_throws_before_SaveChanges_should_result_in_DataException_containing_a_MappingException()
        {
            Database.SetInitializer(new InitializerForBadMappingCaseWithQuery());
            using (var context = new BizContextWithNoEdmMetadata2())
            {
                try
                {
                    context.Employees.FirstOrDefault();
                    Assert.True(false);
                }
                catch (DataException ex)
                {
                    // Just validate that we got the expected exception types.
                    // The actual contents of the exception message is hard to validate and is not
                    // directly relevant to this test case--if comes from core EF.
                    var entityCommandCompilationException = ex.InnerException;
                    Assert.IsType<EntityCommandCompilationException>(entityCommandCompilationException);

                    var mappingException = entityCommandCompilationException.InnerException;
                    Assert.IsType<MappingException>(mappingException);
                }
            }
        }

        public class SimpleModelWithBadInitializer : SimpleModelContext
        {
        }

        public class BadInitializerForSimpleModel : DropCreateDatabaseAlways<SimpleModelWithBadInitializer>
        {
            protected override void Seed(SimpleModelWithBadInitializer context)
            {
                context.Entry(new Product { Id = 999, CategoryId = "FOO" }).State = EntityState.Modified;
            }
        }

        [Fact]
        public void Initializer_that_throws_should_result_in_initialization_exception()
        {
            Database.SetInitializer(new BadInitializerForSimpleModel());
            using (var context = new SimpleModelWithBadInitializer())
            {
                Assert.Throws<DataException>(() => context.Products.Load()).ValidateMessage(
                    "Database_InitializationException");
            }
        }

        #endregion

        #region O-space loading for context used in initializer

        public class TestOSpaceLoadingInitializer : IDatabaseInitializer<ContextForInitializerOSpaceLoading>
        {
            public void InitializeDatabase(ContextForInitializerOSpaceLoading context)
            {
                var metadata = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;

                // This will throw if the O-Space type for Product is not loaded.
                var oSpaceType = (EntityType)metadata.GetType("Product", "SimpleModel", DataSpace.OSpace);

                var clrType = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace)).GetClrType(oSpaceType);
                Assert.Same(typeof(Product), clrType);
            }
        }

        public class ContextForInitializerOSpaceLoading : DbContext
        {
            public ContextForInitializerOSpaceLoading()
            {
                Database.SetInitializer(new TestOSpaceLoadingInitializer());
            }

            public DbSet<Product> Products { get; set; }
        }

        // See Dev11 bug 138963
        [Fact]
        public void O_space_types_are_loaded_when_a_Code_First_context_is_used_in_a_database_initializer()
        {
            using (var context = new ContextForInitializerOSpaceLoading())
            {
                context.Database.Initialize(force: false);
            }
        }

        #endregion

        #region Constructing a new context in the initializer

        public class UseMeUseMeContext : DbContext
        {
            public DbSet<Product> Products { get; set; }
        }

        public class UseMeUseMeInitializer : IDatabaseInitializer<UseMeUseMeContext>
        {
            public static int InitializationCount { get; set; }

            public void InitializeDatabase(UseMeUseMeContext context)
            {
                InitializationCount++;

                using (var context2 = new UseMeUseMeContext())
                {
                    var _ = ((IObjectContextAdapter)context2).ObjectContext;
                }
            }
        }

        [Fact]
        public void Constructing_a_new_context_instance_in_an_initializer_should_work_and_should_not_cause_initialization_to_run_twice()
        {
            Database.SetInitializer(new UseMeUseMeInitializer());

            using (var context = new UseMeUseMeContext())
            {
                var _ = ((IObjectContextAdapter)context).ObjectContext;
            }

            Assert.Equal(1, UseMeUseMeInitializer.InitializationCount);
        }

        public class UseMeUseYouContext : DbContext
        {
            public DbSet<Product> Products { get; set; }
        }

        public class UseMeUseYouInitializer : IDatabaseInitializer<UseMeUseYouContext>
        {
            public static int InitializationCount { get; set; }

            public void InitializeDatabase(UseMeUseYouContext context)
            {
                InitializationCount++;

                using (var context2 = new YouUsedMeContext())
                {
                    var _ = ((IObjectContextAdapter)context2).ObjectContext;
                }
            }
        }

        public class YouUsedMeContext : DbContext
        {
            public DbSet<Product> Products { get; set; }
        }

        public class YouUsedMeContextInitializer : IDatabaseInitializer<YouUsedMeContext>
        {
            public static int InitializationCount { get; set; }

            public void InitializeDatabase(YouUsedMeContext context)
            {
                InitializationCount++;
            }
        }

        [Fact]
        public void
            Constructing_a_different_context_instance_in_an_initializer_should_work_and_both_should_be_initialized()
        {
            Database.SetInitializer(new UseMeUseYouInitializer());
            Database.SetInitializer(new YouUsedMeContextInitializer());

            using (var context = new UseMeUseYouContext())
            {
                var _ = ((IObjectContextAdapter)context).ObjectContext;
            }

            Assert.Equal(1, UseMeUseYouInitializer.InitializationCount);
            Assert.Equal(1, YouUsedMeContextInitializer.InitializationCount);
        }

        #endregion

        #region Delete marking database as not initialized

        public abstract class ContextForDeleteAndReuse : DbContext
        {
            public DbSet<Product> Products { get; set; }
        }

        public class ContextForDeleteAndReuse1 : ContextForDeleteAndReuse
        {
        }

        public class InitializerForDeleteAndReuse<TContext> : DropCreateDatabaseAlways<TContext>
            where TContext : ContextForDeleteAndReuse
        {
            protected override void Seed(TContext context)
            {
                HasRun = true;
                context.Products.Add(new Product { Name = "Dev11" });
            }

            public bool HasRun { get; set; }
        }

        [Fact]
        public void
            Calling_Database_Delete_and_then_creating_a_new_context_and_using_it_causes_the_initializer_to_run_again()
        {
            var initializer = new InitializerForDeleteAndReuse<ContextForDeleteAndReuse1>();
            Database.SetInitializer(initializer);

            using (var context = new ContextForDeleteAndReuse1())
            {
                context.Products.Single(e => e.Name == "Dev11");
                Assert.True(initializer.HasRun);

                initializer.HasRun = false;
                context.Database.Delete();
            }

            using (var context = new ContextForDeleteAndReuse1())
            {
                context.Products.Single(e => e.Name == "Dev11");
                Assert.True(initializer.HasRun);
            }
        }

        public class ContextForDeleteAndReuse2 : ContextForDeleteAndReuse
        {
        }

        [Fact]
        public void Calling_Database_Delete_and_then_continuing_to_use_the_context_causes_the_initializer_to_run_again()
        {
            var initializer = new InitializerForDeleteAndReuse<ContextForDeleteAndReuse2>();
            Database.SetInitializer(initializer);

            using (var context = new ContextForDeleteAndReuse2())
            {
                context.Products.Single(e => e.Name == "Dev11");
                Assert.True(initializer.HasRun);

                initializer.HasRun = false;
                context.Database.Delete();

                context.Products.Single(e => e.Name == "Dev11");
                Assert.True(initializer.HasRun);
            }
        }

        #endregion
    }
}