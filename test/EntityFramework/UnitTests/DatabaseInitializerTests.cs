// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using Moq;
    using System;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Data.SqlClient;
    using System.Linq;
    using Xunit;

    /// <summary>
    /// Unit tests for database initialization.
    /// </summary>
    public class DatabaseInitializerTests : TestBase
    {
        #region Helpers

        /// <summary>
        /// Used as a fake context type for tests that don't actually register a strategy.
        /// </summary>
        public class FakeNoRegContext : DbContext
        {
        }

        #endregion

        #region Negative initialization strategy tests

        [Fact]
        public void DropCreateDatabaseAlways_throws_when_given_a_null_context()
        {
            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => new DropCreateDatabaseAlways<DbContext>().InitializeDatabase(null)).ParamName);
        }

        [Fact]
        public void CreateDatabaseIfNotExists_throws_when_given_a_null_context()
        {
            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => new CreateDatabaseIfNotExists<DbContext>().InitializeDatabase(null)).ParamName);
        }

        [Fact]
        public void DropCreateDatabaseIfModelChanges_throws_when_given_a_null_context()
        {
            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => new DropCreateDatabaseIfModelChanges<DbContext>().InitializeDatabase(null)).
                    ParamName);
        }

        [Fact]
        public void MigrateDatabaseToLatestVersion_ctor_throws_when_given_null_connection_name()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("connectionStringName"),
                Assert.Throws<ArgumentException>(() => new MigrateDatabaseToLatestVersion<EmptyContext, TestMigrationsConfiguration>(null)).
                    Message);
        }

        [Fact]
        public void MigrateDatabaseToLatestVersion_ctor_throws_when_given_empty_connection_name()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("connectionStringName"),
                Assert.Throws<ArgumentException>(() => new MigrateDatabaseToLatestVersion<EmptyContext, TestMigrationsConfiguration>("  ")).
                    Message);
        }

        [Fact]
        public void MigrateDatabaseToLatestVersion_throws_when_given_invalid_connection_name()
        {
            var init = new MigrateDatabaseToLatestVersion<EmptyContext, TestMigrationsConfiguration>("YouWontFindMe");

            Assert.Equal(
                Strings.DbContext_ConnectionStringNotFound("YouWontFindMe"),
                Assert.Throws<InvalidOperationException>(() => init.InitializeDatabase(new EmptyContext())).Message);
        }

        [Fact]
        public void MigrateDatabaseToLatestVersion_ctor_throws_when_given_null_migrations_configuration()
        {
            Assert.Equal(
                "configuration",
                Assert.Throws<ArgumentNullException>(() => new MigrateDatabaseToLatestVersion<EmptyContext, TestMigrationsConfiguration>(true, null)).
                    ParamName);
        }

        #endregion

        #region Positive DropCreateDatabaseAlways strategy tests

        [Fact]
        public void DropCreateDatabaseAlways_performs_delete_create_and_seeding()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, DropCreateDatabaseAlways<FakeNoRegContext>>(
                databaseExists: false);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists CreateDatabase Seed", tracker.Result);
        }

        [Fact]
        public void DropCreateDatabaseAlways_performs_delete_create_and_seeding_even_if_database_exists_and_model_matches()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, DropCreateDatabaseAlways<FakeNoRegContext>>(
                databaseExists: true, modelCompatible: true);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists Delete CreateDatabase Seed", tracker.Result);
        }

        [Fact]
        public void DropCreateDatabaseAlways_initializes_if_Migrations_is_configured_and_database_exists()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, DropCreateDatabaseAlways<FakeNoRegContext>>(
                databaseExists: true, modelCompatible: true, migrationsConfigured: true);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists Delete CreateDatabase Seed", tracker.Result);
        }

        [Fact]
        public void DropCreateDatabaseAlways_initializes_if_Migrations_is_configured_and_database_does_not_exist()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, DropCreateDatabaseAlways<FakeNoRegContext>>(
                databaseExists: false, migrationsConfigured: true);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists CreateDatabase Seed", tracker.Result);
        }

        #endregion

        #region Positive CreateDatabaseIfNotExists strategy tests

        [Fact]
        public void CreateDatabaseIfNotExists_creates_and_seeds_database_if_not_exists()
        {
            var tracker =
                new DatabaseInitializerTracker<FakeNoRegContext, CreateDatabaseIfNotExists<FakeNoRegContext>>(databaseExists: false);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists CreateDatabase Seed", tracker.Result);
        }

        [Fact]
        public void CreateDatabaseIfNotExists_does_nothing_if_database_exists_and_model_matches()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, CreateDatabaseIfNotExists<FakeNoRegContext>>(
                databaseExists: true, modelCompatible: true);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists", tracker.Result);
        }

        [Fact]
        public void CreateDatabaseIfNotExists_throws_if_database_exists_and_model_does_not_match()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, CreateDatabaseIfNotExists<FakeNoRegContext>>(
                databaseExists: true, modelCompatible: false);

            Assert.Equal(
                Strings.DatabaseInitializationStrategy_ModelMismatch(tracker.Context.GetType().Name),
                Assert.Throws<InvalidOperationException>(() => tracker.ExecuteStrategy()).Message);
        }

        [Fact]
        public void CreateDatabaseIfNotExists_does_nothing_if_database_exists_but_has_no_metadata()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, CreateDatabaseIfNotExists<FakeNoRegContext>>(
                databaseExists: true, modelCompatible: true, hasMetadata: false);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists", tracker.Result);
        }

        [Fact]
        public void CreateDatabaseIfNotExists_initializes_if_database_does_not_exist_and_Migrations_is_enabled()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, CreateDatabaseIfNotExists<FakeNoRegContext>>(
                databaseExists: false, migrationsConfigured: true);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists CreateDatabase Seed", tracker.Result);
        }

        [Fact] // CodePlex 1192
        public void CreateDatabaseIfNotExists_throws_if_database_exists_and_model_does_not_match_with_Migrations_enabled()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, CreateDatabaseIfNotExists<FakeNoRegContext>>(
                databaseExists: true, modelCompatible: false, migrationsConfigured: true);

            Assert.Equal(
                Strings.DatabaseInitializationStrategy_ModelMismatch(tracker.Context.GetType().Name),
                Assert.Throws<InvalidOperationException>(() => tracker.ExecuteStrategy()).Message);

            Assert.Equal("Exists", tracker.Result);
        }

        [Fact] // CodePlex 1192
        public void CreateDatabaseIfNotExists_does_nothing_if_database_exists_and_model_matches_with_Migrations_enabled()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, CreateDatabaseIfNotExists<FakeNoRegContext>>(
                databaseExists: true, modelCompatible: true, migrationsConfigured: true);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists", tracker.Result);
        }

        [Fact] // CodePlex 1192
        public void CreateDatabaseIfNotExists_does_nothing_if_database_exists_but_has_no_metadata_with_Migrations_enabled()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, CreateDatabaseIfNotExists<FakeNoRegContext>>(
                databaseExists: true, modelCompatible: true, hasMetadata: false, migrationsConfigured: true);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists", tracker.Result);
        }

        #endregion

        #region Positive DropCreateDatabaseIfModelChanges strategy tests

        [Fact]
        public void DropCreateDatabaseIfModelChanges_creates_and_seeds_database_if_not_exists()
        {
            var tracker =
                new DatabaseInitializerTracker<FakeNoRegContext, DropCreateDatabaseIfModelChanges<FakeNoRegContext>>(databaseExists: false);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists CreateDatabase Seed", tracker.Result);
        }

        [Fact]
        public void DropCreateDatabaseIfModelChanges_does_nothing_if_database_exists_and_model_matches()
        {
            var tracker =
                new DatabaseInitializerTracker<FakeNoRegContext, DropCreateDatabaseIfModelChanges<FakeNoRegContext>>(
                    databaseExists: true, modelCompatible: true);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists", tracker.Result);
        }

        [Fact]
        public void DropCreateDatabaseIfModelChanges_recreates_database_if_database_exists_and_model_does_not_match()
        {
            var tracker =
                new DatabaseInitializerTracker<FakeNoRegContext, DropCreateDatabaseIfModelChanges<FakeNoRegContext>>(
                    databaseExists: true, modelCompatible: false);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists Exists Delete CreateDatabase Seed", tracker.Result);
        }

        [Fact]
        public void DropCreateDatabaseIfModelChanges_throws_if_database_exists_but_has_no_metadata()
        {
            var tracker =
                new DatabaseInitializerTracker<FakeNoRegContext, DropCreateDatabaseIfModelChanges<FakeNoRegContext>>(
                    databaseExists: true, modelCompatible: true, hasMetadata: false);

            Assert.Equal(Strings.Database_NoDatabaseMetadata, Assert.Throws<NotSupportedException>(() => tracker.ExecuteStrategy()).Message);
        }

        [Fact]
        public void DropCreateDatabaseIfModelChanges_initializes_if_database_does_not_exist_and_Migrations_is_enabled()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, DropCreateDatabaseIfModelChanges<FakeNoRegContext>>(
                databaseExists: false, migrationsConfigured: true);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists CreateDatabase Seed", tracker.Result);
        }

        [Fact] // CodePlex 1192
        public void DropCreateDatabaseIfModelChanges_does_nothing_if_database_exists_and_model_is_up_to_date_with_Migrations_enabled()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, DropCreateDatabaseIfModelChanges<FakeNoRegContext>>(
                databaseExists: true, modelCompatible: true, migrationsConfigured: true);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists", tracker.Result);
        }

        [Fact]
        public void DropCreateDatabaseIfModelChanges_initializes_if_database_exists_and_model_does_not_match_with_Migrations_enabled()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, DropCreateDatabaseIfModelChanges<FakeNoRegContext>>(
                databaseExists: true, modelCompatible: false, migrationsConfigured: true);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists Exists Delete CreateDatabase Seed", tracker.Result);
        }

        [Fact] // CodePlex 1192
        public void DropCreateDatabaseIfModelChanges_throws_if_database_exists_but_has_no_metadata_with_Migrations_enabled()
        {
            var tracker =
                new DatabaseInitializerTracker<FakeNoRegContext, DropCreateDatabaseIfModelChanges<FakeNoRegContext>>(
                    databaseExists: true, modelCompatible: true, hasMetadata: false, migrationsConfigured: true);

            Assert.Equal(
                Strings.Database_NoDatabaseMetadata, 
                Assert.Throws<NotSupportedException>(() => tracker.ExecuteStrategy()).Message);

            Assert.Equal("Exists", tracker.Result);
        }

        #endregion

        #region Positive MigrateDatabaseToLatestVersion strategy tests

        public class EmptyContext : DbContext
        {
            public EmptyContext() : base() { }

            public EmptyContext(string nameOrConnectionString) : base(nameOrConnectionString) { }
        }

        public class EmptyNonConstructableContext : EmptyContext
        {
            public EmptyNonConstructableContext(string nameOrConnectionString) : base(nameOrConnectionString) { }
        }

        private class TestMigrationsConfiguration<ContextT> : DbMigrationsConfiguration<ContextT>
            where ContextT : EmptyContext
        {
            public TestMigrationsConfiguration()
            {
                MigrationsNamespace = "You.Wont.Find.Any.Migrations.Here";
                AutomaticMigrationsEnabled = true;
            }

            protected override void Seed(ContextT context)
            {
                SeedCalled = true;
                SeedDatabase = context.Database.Connection.Database;
            }

            public static bool SeedCalled { get; set; }
            public static string SeedDatabase { get; set; }
        }

        private class TestMigrationsConfiguration : TestMigrationsConfiguration<EmptyContext>
        {

        }

        private class CustomTestMigrationsConfiguration : TestMigrationsConfiguration
        {
            protected override void Seed(EmptyContext context)
            {
                CustomSeedDatabase = context.Database.Connection.Database;
            }

            public string CustomSeedDatabase { get; set; }
        }

        private class TestNonConstructableMigrationsConfiguration : TestMigrationsConfiguration<EmptyNonConstructableContext>
        {

        }

        [Fact]
        public void MigrateDatabaseToLatestVersion_invokes_migrations_pipeline_to_latest_version()
        {
            var init = new MigrateDatabaseToLatestVersion<EmptyContext, TestMigrationsConfiguration>();

            TestMigrationsConfiguration.SeedCalled = false;

            init.InitializeDatabase(new EmptyContext());

            // If seed gets called we know the migrations pipeline was invoked to update to the latest version
            Assert.True(TestMigrationsConfiguration.SeedCalled);
        }

        [Fact]
        public void MigrateDatabaseToLatestVersion_uses_passed_context()
        {
            var init = new MigrateDatabaseToLatestVersion<EmptyContext, TestMigrationsConfiguration>(true);

            TestMigrationsConfiguration.SeedDatabase = null;

            init.InitializeDatabase(new EmptyContext("MigrateDatabaseToLatestVersionNamedConnectionTest"));

            Assert.Equal("MigrationInitFromConfig", TestMigrationsConfiguration.SeedDatabase);
        }

        [Fact]
        public void MigrateDatabaseToLatestVersion_uses_passed_nonconstructable_context()
        {
            var init = new MigrateDatabaseToLatestVersion<EmptyNonConstructableContext, TestNonConstructableMigrationsConfiguration>(true);

            TestMigrationsConfiguration.SeedDatabase = null;
            TestNonConstructableMigrationsConfiguration.SeedDatabase = null;

            init.InitializeDatabase(new EmptyNonConstructableContext("MigrateDatabaseToLatestVersionNamedConnectionTest"));

            Assert.Null(TestMigrationsConfiguration.SeedDatabase);
            Assert.Equal("MigrationInitFromConfig", TestNonConstructableMigrationsConfiguration.SeedDatabase);
        }

        [Fact]
        public void MigrateDatabaseToLatestVersion_uses_own_context()
        {
            var init = new MigrateDatabaseToLatestVersion<EmptyContext, TestMigrationsConfiguration>(false);

            TestMigrationsConfiguration.SeedDatabase = null;

            init.InitializeDatabase(new EmptyContext("MigrateDatabaseToLatestVersionNamedConnectionTest"));

            Assert.NotEqual("MigrationInitFromConfig", TestMigrationsConfiguration.SeedDatabase);
            Assert.Equal("System.Data.Entity.DatabaseInitializerTests+EmptyContext", TestMigrationsConfiguration.SeedDatabase);
        }

        [Fact]
        public void MigrateDatabaseToLatestVersion_uses_passed_migrations_configuration()
        {
            var configuration = new CustomTestMigrationsConfiguration();
            var init = new MigrateDatabaseToLatestVersion<EmptyContext, TestMigrationsConfiguration>(false, configuration);

            init.InitializeDatabase(new EmptyContext("MigrateDatabaseToLatestVersionNamedConnectionTest"));

            Assert.Equal("System.Data.Entity.DatabaseInitializerTests+EmptyContext", configuration.CustomSeedDatabase);
        }

        private class FakeForMdtlvsgpsc : DbContext
        {
            public DbSet<Blog> Blogs { get; set; }

            public class Blog
            {
                public int BlogId { get; set; }
                public string Name { get; set; }
            }
        }

        private class SeedingMigrationsConfiguration : DbMigrationsConfiguration<FakeForMdtlvsgpsc>
        {
            public static DbContext DbContextUsedForSeeding { get; set; }

            public SeedingMigrationsConfiguration()
            {
                AutomaticMigrationsEnabled = true;
            }

            protected override void Seed(FakeForMdtlvsgpsc context)
            {
                context.Blogs.Add(new FakeForMdtlvsgpsc.Blog());
                context.Blogs.Add(new FakeForMdtlvsgpsc.Blog());
                context.Blogs.Add(new FakeForMdtlvsgpsc.Blog());

                DbContextUsedForSeeding = context;
            }
        }

        [Fact]
        public void MigrateDatabaseToLatestVersion_seed_doesnt_pollute_supplied_context()
        {
            SeedingMigrationsConfiguration.DbContextUsedForSeeding = null;

            using (var context = new FakeForMdtlvsgpsc())
            {
                var init = new MigrateDatabaseToLatestVersion<FakeForMdtlvsgpsc, SeedingMigrationsConfiguration>(useSuppliedContext: true);
                init.InitializeDatabase(context);

                Assert.Equal(0, context.ChangeTracker.Entries().Count());
                Assert.ReferenceEquals(context, SeedingMigrationsConfiguration.DbContextUsedForSeeding);
            }
        }

        [Fact]
        public void MigrateDatabaseToLatestVersion_uses_connection_name_from_config_file()
        {
            var init =
                new MigrateDatabaseToLatestVersion<EmptyContext, TestMigrationsConfiguration>(
                    "MigrateDatabaseToLatestVersionNamedConnectionTest");

            TestMigrationsConfiguration.SeedDatabase = null;

            init.InitializeDatabase(new EmptyContext());

            Assert.Equal("MigrationInitFromConfig", TestMigrationsConfiguration.SeedDatabase);
        }

        #endregion

        #region Positive Database.SetInitializer tests

        public class FakeForSirdi : DbContextUsingMockInternalContext
        {
        }

        [Fact]
        public void SetInitializer_registers_different_initializer()
        {
            var mockInitializer = new Mock<IDatabaseInitializer<FakeForSirdi>>();
            Database.SetInitializer(mockInitializer.Object);

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForSirdi>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);
        }

        public class FakeForSirdifdct1 : DbContextUsingMockInternalContext
        {
        }

        public class FakeForSirdifdct2 : DbContextUsingMockInternalContext
        {
        }

        [Fact]
        public void SetInitializer_registers_different_initializers_for_different_context_types()
        {
            var mockInitializer1 = new Mock<IDatabaseInitializer<FakeForSirdifdct1>>();
            Database.SetInitializer(mockInitializer1.Object);

            var mockInitializer2 = new Mock<IDatabaseInitializer<FakeForSirdifdct2>>();
            Database.SetInitializer(mockInitializer2.Object);

            // Initialize for first type and verify correct initializer called
            var mock = new Mock<InternalContextForMockWithRealContext<FakeForSirdifdct1>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            mockInitializer1.Verify(i => i.InitializeDatabase(It.IsAny<FakeForSirdifdct1>()), Times.Once());
            mockInitializer2.Verify(i => i.InitializeDatabase(It.IsAny<FakeForSirdifdct2>()), Times.Never());

            // Initialize for second type and verify correct initializer called
            var mock2 = new Mock<InternalContextForMockWithRealContext<FakeForSirdifdct2>>();
            mock2.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock2.Object).Initialize(force: true);

            mockInitializer1.Verify(i => i.InitializeDatabase(It.IsAny<FakeForSirdifdct1>()), Times.Once());
            mockInitializer2.Verify(i => i.InitializeDatabase(It.IsAny<FakeForSirdifdct2>()), Times.Once());
        }

        public class FakeForCsiaftsctrfi : DbContextUsingMockInternalContext
        {
        }

        [Fact]
        public void Calling_SetInitializer_again_for_the_same_context_type_replaces_first_initializer()
        {
            var mockInitializer1 = new Mock<IDatabaseInitializer<FakeForCsiaftsctrfi>>();
            Database.SetInitializer(mockInitializer1.Object);

            var mockInitializer2 = new Mock<IDatabaseInitializer<FakeForCsiaftsctrfi>>();
            Database.SetInitializer(mockInitializer2.Object);

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForCsiaftsctrfi>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            mockInitializer1.Verify(i => i.InitializeDatabase(It.IsAny<FakeForCsiaftsctrfi>()), Times.Never());
            mockInitializer2.Verify(i => i.InitializeDatabase(It.IsAny<FakeForCsiaftsctrfi>()), Times.Once());
        }

        public class FakeForDisicdoine : DbContext
        {
        }

        [Fact]
        public void Default_initialization_strategy_is_CreateDatabaseIfNotExists()
        {
            var tracker =
                new DatabaseInitializerTracker<FakeForDisicdoine, DropCreateDatabaseAlways<FakeForDisicdoine>>(databaseExists: false);

            tracker.Context.Database.Initialize(force: true);

            Assert.Equal("UseTempObjectContext Exists CreateDatabase DisposeTempObjectContext", tracker.Result);
        }

        public class FakeForDisirbcsiwn : DbContext
        {
        }

        [Fact]
        public void Default_initialization_strategy_is_removed_by_calling_SetInitializer_with_null()
        {
            var tracker =
                new DatabaseInitializerTracker<FakeForDisirbcsiwn, DropCreateDatabaseAlways<FakeForDisirbcsiwn>>(databaseExists: false);

            var mockContextType = tracker.Context.GetType();
            var initMethod = typeof(Database).GetOnlyDeclaredMethod("SetInitializer").MakeGenericMethod(mockContextType);
            initMethod.Invoke(null, new object[] { null });

            tracker.Context.Database.Initialize(force: true);

            Assert.Equal("UseTempObjectContext DisposeTempObjectContext", tracker.Result);
        }

        public class FakeForIscbrbcsin : DbContextUsingMockInternalContext
        {
        }

        [Fact]
        public void Initialization_strategy_can_be_removed_by_calling_SetInitializer_null()
        {
            var mockInitializer = new Mock<IDatabaseInitializer<FakeForIscbrbcsin>>();
            Database.SetInitializer(mockInitializer.Object);
            Database.SetInitializer<FakeForIscbrbcsin>(null);

            var mock = new Mock<InternalContextForMockWithRealContext<FakeForIscbrbcsin>>();
            mock.Setup(m => m.AppConfig).Returns(new AppConfig(CreateEmptyConfig()));
            new Database(mock.Object).Initialize(force: true);

            mockInitializer.Verify(i => i.InitializeDatabase(It.IsAny<FakeForIscbrbcsin>()), Times.Never());
        }

        #endregion

        #region Database.Initialize (with force false) positive tests

        public class FakeForEieiidhnabi : DbContext
        {
        }

        [Fact]
        public void Initialize_without_force_executes_initializer_if_database_has_not_already_been_initialized()
        {
            var mockInitializer = new Mock<IDatabaseInitializer<FakeForEieiidhnabi>>();
            Database.SetInitializer(mockInitializer.Object);

            new FakeForEieiidhnabi().Database.Initialize(force: false);

            mockInitializer.Verify(i => i.InitializeDatabase(It.IsAny<FakeForEieiidhnabi>()), Times.Once());
        }

        public class FakeForEidneiidhabi : DbContext
        {
        }

        [Fact]
        public void Initialize_without_force_does_not_execute_initializer_if_database_has_already_been_initialized()
        {
            var mockInitializer = new Mock<IDatabaseInitializer<FakeForEidneiidhabi>>();
            Database.SetInitializer(mockInitializer.Object);

            // This will call the initializer
            new FakeForEidneiidhabi().Database.Initialize(force: true);
            mockInitializer.Verify(i => i.InitializeDatabase(It.IsAny<FakeForEidneiidhabi>()), Times.Once());

            // This should not call it again
            new FakeForEidneiidhabi().Database.Initialize(force: false);
            mockInitializer.Verify(i => i.InitializeDatabase(It.IsAny<FakeForEidneiidhabi>()), Times.Once());
        }

        public class FakeForIeiwdhnbi : DbContext
        {
        }

        [Fact]
        public void Initialize_executes_initializer_when_database_has_not_been_initialized()
        {
            var mockInitializer = new Mock<IDatabaseInitializer<FakeForIeiwdhnbi>>();
            Database.SetInitializer(mockInitializer.Object);

            new FakeForIeiwdhnbi().Database.Initialize(force: true);

            mockInitializer.Verify(i => i.InitializeDatabase(It.IsAny<FakeForIeiwdhnbi>()), Times.Once());
        }

        public class FakeForIeiewdhabi : DbContext
        {
        }

        [Fact]
        public void Initialize_executes_initializer_even_when_database_has_already_been_initialized()
        {
            var mockInitializer = new Mock<IDatabaseInitializer<FakeForIeiewdhabi>>();
            Database.SetInitializer(mockInitializer.Object);

            new FakeForIeiewdhabi().Database.Initialize(force: true);
            mockInitializer.Verify(i => i.InitializeDatabase(It.IsAny<FakeForIeiewdhabi>()), Times.Once());

            new FakeForIeiewdhabi().Database.Initialize(force: true);
            mockInitializer.Verify(i => i.InitializeDatabase(It.IsAny<FakeForIeiewdhabi>()), Times.Exactly(2));
        }

        #endregion

        #region ModelMatches positive tests

        [Fact]
        public void CompatibleWithModel_returns_true_if_model_was_not_created_with_Code_First_and_throwing_is_switched_off()
        {
            var mockInternalContext = new Mock<InternalContextForMock>
                                          {
                                              CallBase = true
                                          };
            mockInternalContext.Setup(c => c.CodeFirstModel).Returns((DbCompiledModel)null);

            Assert.True(new Database(mockInternalContext.Object).CompatibleWithModel(throwIfNoMetadata: false));
        }

        #endregion

        #region Database initialization status positive tests

        internal class NonInitializingLazyInternalContext : LazyInternalContext
        {
            public NonInitializingLazyInternalContext(IInternalConnection internalConnection, DbCompiledModel model)
                : base(new Mock<DbContext>().Object, internalConnection, model)
            {
            }

            protected override void InitializeContext()
            {
            }
        }

        #endregion

        #region TempObjectContext and GetObjectContextWithoutDatabaseInitialization positive tests

        public class FakeForGocwdidnid : DbContext
        {
        }

        [Fact]
        public void GetObjectContextWithoutDatabaseInitialization_does_not_initialize_database()
        {
            var mockInitializer = new Mock<IDatabaseInitializer<FakeForGocwdidnid>>();
            Database.SetInitializer(mockInitializer.Object);

            new FakeForGocwdidnid().InternalContext.GetObjectContextWithoutDatabaseInitialization();

            mockInitializer.Verify(i => i.InitializeDatabase(It.IsAny<FakeForGocwdidnid>()), Times.Never());
        }

        public class FakeForUtoccatoc : DbContext
        {
        }

        [Fact]
        public void UseTempObjectContext_creates_a_temporary_object_context()
        {
            var internalContext = new FakeForUtoccatoc().InternalContext;
            var realContext = internalContext.GetObjectContextWithoutDatabaseInitialization();

            internalContext.UseTempObjectContext();

            Assert.NotSame(realContext, internalContext.GetObjectContextWithoutDatabaseInitialization());

            internalContext.DisposeTempObjectContext();

            Assert.Same(realContext, internalContext.GetObjectContextWithoutDatabaseInitialization());
        }

        public class FakeForUtoccmtocotoc : DbContext
        {
        }

        [Fact]
        public void UseTempObjectContext_called_multiple_times_only_creates_one_temporary_object_context()
        {
            var internalContext = new FakeForUtoccmtocotoc().InternalContext;
            var realContext = internalContext.GetObjectContextWithoutDatabaseInitialization();

            internalContext.UseTempObjectContext();
            var fakeContext = internalContext.GetObjectContextWithoutDatabaseInitialization();

            internalContext.UseTempObjectContext();
            Assert.Same(fakeContext, internalContext.GetObjectContextWithoutDatabaseInitialization());

            internalContext.UseTempObjectContext();
            Assert.Same(fakeContext, internalContext.GetObjectContextWithoutDatabaseInitialization());

            internalContext.DisposeTempObjectContext();
            Assert.Same(fakeContext, internalContext.GetObjectContextWithoutDatabaseInitialization());

            internalContext.DisposeTempObjectContext();
            Assert.Same(fakeContext, internalContext.GetObjectContextWithoutDatabaseInitialization());

            internalContext.DisposeTempObjectContext();
            Assert.Same(realContext, internalContext.GetObjectContextWithoutDatabaseInitialization());
        }

        public class FakeForLctdtocdttc : DbContext
        {
        }

        [Fact]
        public void Last_call_to_DisposeTempObjectContext_disposes_the_temp_context()
        {
            var internalContext = new FakeForLctdtocdttc().InternalContext;

            internalContext.UseTempObjectContext();
            internalContext.UseTempObjectContext();

            var fakeContext = internalContext.GetObjectContextWithoutDatabaseInitialization();

            internalContext.DisposeTempObjectContext();
            internalContext.DisposeTempObjectContext();

            Assert.Equal(
                Strings.ObjectContext_ObjectDisposed,
                Assert.Throws<ObjectDisposedException>(() => fakeContext.SaveChanges()).Message);
        }

        #endregion

        #region TryGetModelHash tests

        [Fact]
        public void TryGetModelHash_throws_when_given_null_context()
        {
#pragma warning disable 612,618
            Assert.Equal("context", Assert.Throws<ArgumentNullException>(() => EdmMetadata.TryGetModelHash(null)).ParamName);
#pragma warning restore 612,618
        }

        #endregion

        #region DatabaseCreator tests

        [Fact]
        public void Database_Create_calls_CreateDatabase_if_database_does_not_exist()
        {
            var mockOperations = new Mock<DatabaseOperations>();
            var mockContext = new Mock<InternalContextForMock<FakeContext>>();
            mockContext.Setup(m => m.DatabaseOperations).Returns(mockOperations.Object);
            mockContext.Setup(m => m.CreateObjectContextForDdlOps()).Returns(new Mock<ClonedObjectContext>().Object);
            mockOperations.Setup(m => m.Exists(null, It.IsAny<int?>(), It.IsAny<Lazy<StoreItemCollection>>())).Returns(false);

            mockContext.Object.Owner.Database.Create();

            mockContext.Verify(m => m.CreateDatabase(null, It.IsAny<DatabaseExistenceState>()), Times.Once());
        }

        [Fact]
        public void Database_CreateIfNotExists_calls_CreateDatabase_if_database_does_not_exist()
        {
            var mockOperations = new Mock<DatabaseOperations>();
            var mockContext = new Mock<InternalContextForMock<FakeContext>>();
            mockContext.Setup(m => m.DatabaseOperations).Returns(mockOperations.Object);
            mockContext.Setup(m => m.CreateObjectContextForDdlOps()).Returns(new Mock<ClonedObjectContext>().Object);
            mockOperations.Setup(m => m.Exists(null, It.IsAny<int?>(), It.IsAny<Lazy<StoreItemCollection>>())).Returns(false);

            mockContext.Object.Owner.Database.CreateIfNotExists();

            mockContext.Verify(m => m.CreateDatabase(null, It.IsAny<DatabaseExistenceState>()), Times.Once());
        }

        [Fact]
        public void Database_Create_throws_and_does_not_call_CreateDatabase_if_database_exists()
        {
            var mockOperations = new Mock<DatabaseOperations>();
            var mockContext = new Mock<InternalContextForMock<FakeContext>>() { CallBase = true };
            mockContext.Setup(m => m.DatabaseOperations).Returns(mockOperations.Object);
            mockContext.Setup(m => m.Connection).Returns(new SqlConnection("Database=Foo"));
            mockOperations.Setup(m => m.Exists(It.IsAny<DbConnection>(), It.IsAny<int?>(), It.IsAny<Lazy<StoreItemCollection>>())).Returns(true);

            Assert.Equal(
                Strings.Database_DatabaseAlreadyExists("Foo"),
                Assert.Throws<InvalidOperationException>(() => mockContext.Object.Owner.Database.Create()).Message);

            mockContext.Verify(m => m.CreateDatabase(null, It.IsAny<DatabaseExistenceState>()), Times.Never());
        }

        [Fact]
        public void Database_CreateIfNotExists_does_not_call_CreateDatabase_if_database_exists()
        {
            var mockOperations = new Mock<DatabaseOperations>();
            var mockContext = new Mock<InternalContextForMock<FakeContext>>();
            mockContext.Setup(m => m.DatabaseOperations).Returns(mockOperations.Object);
            mockContext.Setup(m => m.CreateObjectContextForDdlOps()).Returns(new Mock<ClonedObjectContext>().Object);
            mockOperations.Setup(m => m.Exists(null, It.IsAny<int?>(), It.IsAny<Lazy<StoreItemCollection>>())).Returns(true);

            mockContext.Object.Owner.Database.CreateIfNotExists();

            mockContext.Verify(m => m.CreateDatabase(null, It.IsAny<DatabaseExistenceState>()), Times.Never());
        }

        public class FakeContext : DbContext
        {
            static FakeContext()
            {
                Database.SetInitializer<FakeContext>(null);
            }
        }

        #endregion

        [Fact]
        public void Initializer_never_run_automatically_for_any_history_contexts()
        {
            var mockInitializer = new Mock<IDatabaseInitializer<MyHistoryContext>>();
            Database.SetInitializer(mockInitializer.Object);

            new MyHistoryContext().Database.Initialize(force: false);
            mockInitializer.Verify(i => i.InitializeDatabase(It.IsAny<MyHistoryContext>()), Times.Never());
        }

        public class MyHistoryContext : HistoryContext 
        {
        }
    }
}
