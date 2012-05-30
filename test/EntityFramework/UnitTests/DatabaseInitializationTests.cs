namespace ProductivityApiUnitTests
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Resources;
    using System.Data.SqlClient;
    using System.Xml.Linq;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit tests for database initialization.
    /// </summary>
    public class DatabaseInitializationTests : TestBase
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
            Assert.Equal("context", Assert.Throws<ArgumentNullException>(() => new DropCreateDatabaseAlways<DbContext>().InitializeDatabase(null)).ParamName);
        }

        [Fact]
        public void CreateDatabaseIfNotExists_throws_when_given_a_null_context()
        {
            Assert.Equal("context", Assert.Throws<ArgumentNullException>(() => new CreateDatabaseIfNotExists<DbContext>().InitializeDatabase(null)).ParamName);
        }

        [Fact]
        public void DropCreateDatabaseIfModelChanges_throws_when_given_a_null_context()
        {
            Assert.Equal("context", Assert.Throws<ArgumentNullException>(() => new DropCreateDatabaseIfModelChanges<DbContext>().InitializeDatabase(null)).ParamName);
        }

        [Fact]
        public void MigrateDatabaseToLatestVersion_ctor_throws_when_given_null_connection_name()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("connectionStringName"), Assert.Throws<ArgumentException>(() => new MigrateDatabaseToLatestVersion<EmptyContext, TestMigrationsConfiguration>(null)).Message);
        }

        [Fact]
        public void MigrateDatabaseToLatestVersion_ctor_throws_when_given_empty_connection_name()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("connectionStringName"), Assert.Throws<ArgumentException>(() => new MigrateDatabaseToLatestVersion<EmptyContext, TestMigrationsConfiguration>("  ")).Message);
        }

        [Fact]
        public void MigrateDatabaseToLatestVersion_throws_when_given_invalid_connection_name()
        {
            var init = new MigrateDatabaseToLatestVersion<EmptyContext, TestMigrationsConfiguration>("YouWontFindMe");

            Assert.Equal(Strings.DbContext_ConnectionStringNotFound("YouWontFindMe"), Assert.Throws<InvalidOperationException>(() => init.InitializeDatabase(new EmptyContext())).Message);
        }

        #endregion

        #region Positive DropCreateDatabaseAlways strategy tests

        [Fact]
        public void DropCreateDatabaseAlways_performs_delete_create_and_seeding()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, DropCreateDatabaseAlways<FakeNoRegContext>>(databaseExists: false);

            tracker.ExecuteStrategy();

            Assert.Equal("DeleteIfExists Exists CreateDatabase Seed", tracker.Result);
        }

        [Fact]
        public void DropCreateDatabaseAlways_performs_delete_create_and_seeding_even_if_database_exists_and_model_matches()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, DropCreateDatabaseAlways<FakeNoRegContext>>(databaseExists: true, modelCompatible: true);

            tracker.ExecuteStrategy();

            Assert.Equal("DeleteIfExists Exists CreateDatabase Seed", tracker.Result);
        }

        #endregion

        #region Positive CreateDatabaseIfNotExists strategy tests

        [Fact]
        public void CreateDatabaseIfNotExists_creates_and_seeds_database_if_not_exists()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, CreateDatabaseIfNotExists<FakeNoRegContext>>(databaseExists: false);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists CreateDatabase Seed", tracker.Result);
        }

        [Fact]
        public void CreateDatabaseIfNotExists_does_nothing_if_database_exists_and_model_matches()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, CreateDatabaseIfNotExists<FakeNoRegContext>>(databaseExists: true, modelCompatible: true);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists", tracker.Result);
        }

        [Fact]
        public void CreateDatabaseIfNotExists_throws_if_database_exists_and_model_does_not_match()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, CreateDatabaseIfNotExists<FakeNoRegContext>>(databaseExists: true, modelCompatible: false);

            Assert.Equal(Strings.DatabaseInitializationStrategy_ModelMismatch(tracker.Context.GetType().Name), Assert.Throws<InvalidOperationException>(() => tracker.ExecuteStrategy()).Message);
        }

        [Fact]
        public void CreateDatabaseIfNotExists_does_nothing_if_database_exists_but_has_no_metadata()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, CreateDatabaseIfNotExists<FakeNoRegContext>>(databaseExists: true, modelCompatible: true, hasMetadata: false);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists", tracker.Result);
        }

        #endregion

        #region Positive DropCreateDatabaseIfModelChanges strategy tests

        [Fact]
        public void DropCreateDatabaseIfModelChanges_creates_and_seeds_database_if_not_exists()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, DropCreateDatabaseIfModelChanges<FakeNoRegContext>>(databaseExists: false);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists Exists CreateDatabase Seed", tracker.Result);
        }

        [Fact]
        public void DropCreateDatabaseIfModelChanges_does_nothing_if_database_exists_and_model_matches()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, DropCreateDatabaseIfModelChanges<FakeNoRegContext>>(databaseExists: true, modelCompatible: true);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists", tracker.Result);
        }

        [Fact]
        public void DropCreateDatabaseIfModelChanges_recreates_database_if_database_exists_and_model_does_not_match()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, DropCreateDatabaseIfModelChanges<FakeNoRegContext>>(databaseExists: true, modelCompatible: false);

            tracker.ExecuteStrategy();

            Assert.Equal("Exists DeleteIfExists Exists CreateDatabase Seed", tracker.Result);
        }

        [Fact]
        public void DropCreateDatabaseIfModelChanges_throws_if_database_exists_but_has_no_metadata()
        {
            var tracker = new DatabaseInitializerTracker<FakeNoRegContext, DropCreateDatabaseIfModelChanges<FakeNoRegContext>>(databaseExists: true, modelCompatible: true, hasMetadata: false);

            Assert.Equal(Strings.Database_NoDatabaseMetadata, Assert.Throws<NotSupportedException>(() => tracker.ExecuteStrategy()).Message);
        }

        #endregion

        #region Positive MigrateDatabaseToLatestVersion strategy tests

        public class EmptyContext : DbContext
        {
        }

        private class TestMigrationsConfiguration : DbMigrationsConfiguration<EmptyContext>
        {
            public TestMigrationsConfiguration()
            {
                MigrationsNamespace = "You.Wont.Find.Any.Migrations.Here";
                AutomaticMigrationsEnabled = true;
            }

            protected override void Seed(EmptyContext context)
            {
                SeedCalled = true;
                SeedDatabase = context.Database.Connection.Database;
            }

            public static bool SeedCalled { get; set; }
            public static string SeedDatabase { get; set; }
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
        public void MigrateDatabaseToLatestVersion_use_connection_name_from_config_file()
        {
            var init = new MigrateDatabaseToLatestVersion<EmptyContext, TestMigrationsConfiguration>("MigrateDatabaseToLatestVersionNamedConnectionTest");

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
            var tracker = new DatabaseInitializerTracker<FakeForDisicdoine, DropCreateDatabaseAlways<FakeForDisicdoine>>(databaseExists: false);

            tracker.Context.Database.Initialize(force: true);

            Assert.Equal("UseTempObjectContext Exists CreateDatabase DisposeTempObjectContext", tracker.Result);
        }

        public class FakeForDisirbcsiwn : DbContext
        {
        }

        [Fact]
        public void Default_initialization_strategy_is_removed_by_calling_SetInitializer_with_null()
        {
            var tracker = new DatabaseInitializerTracker<FakeForDisirbcsiwn, DropCreateDatabaseAlways<FakeForDisirbcsiwn>>(databaseExists: false);

            var mockContextType = tracker.Context.GetType();
            var initMethod = typeof(Database).GetMethod("SetInitializer").MakeGenericMethod(mockContextType);
            initMethod.Invoke(null, new object[] { null });

            tracker.Context.Database.Initialize(force: true);

            Assert.Equal("", tracker.Result);
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
            var mockInternalContext = new Mock<InternalContextForMock> { CallBase = true };
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

            Assert.Throws<ObjectDisposedException>(() => fakeContext.SaveChanges()).ValidateMessage(
                "ObjectContext_ObjectDisposed");
        }

        #endregion

        #region ModelMatches tests

        private static Mock<InternalContextForMock<FakeNoRegContext>> CreateContextForCompatibleTest(bool modelMatches, bool codeFirst = true)
        {
            var mockInternalContext = new Mock<InternalContextForMock<FakeNoRegContext>>();
            mockInternalContext.Setup(c => c.CodeFirstModel).Returns(codeFirst ? new DbCompiledModel() : null);
            mockInternalContext.Setup(c => c.QueryForModel()).Returns(new XDocument());
            mockInternalContext.Setup(c => c.ModelMatches(It.IsAny<XDocument>())).Returns(modelMatches);
            return mockInternalContext;
        }

        private static Mock<InternalContextForMock<FakeNoRegContext>> CreateContextForCompatibleTest(string databaseHash, bool codeFirst = true)
        {
            var mockInternalContext = new Mock<InternalContextForMock<FakeNoRegContext>>();
            mockInternalContext.Setup(c => c.CodeFirstModel).Returns(codeFirst ? new DbCompiledModel() : null);
            mockInternalContext.Setup(c => c.QueryForModel()).Returns((XDocument)null);
            mockInternalContext.Setup(c => c.QueryForModelHash()).Returns(databaseHash);
            return mockInternalContext;
        }

        [Fact]
        public void CompatibleWithModel_returns_true_if_model_matches_database_and_method_is_asked_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest(modelMatches: true);
            var mockHashFactory = new Mock<ModelHashCalculator>();

            Assert.True(new ModelCompatibilityChecker()
                .CompatibleWithModel(mockInternalContext.Object, mockHashFactory.Object, throwIfNoMetadata: true));
        }

        [Fact]
        public void CompatibleWithModel_returns_true_if_model_matches_database_and_method_is_asked_not_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest(modelMatches: true);
            var mockHashFactory = new Mock<ModelHashCalculator>();

            Assert.True(new ModelCompatibilityChecker()
                .CompatibleWithModel(mockInternalContext.Object, mockHashFactory.Object, throwIfNoMetadata: false));
        }

        [Fact]
        public void CompatibleWithModel_returns_false_if_model_does_not_match_database_and_method_is_asked_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest(modelMatches: false);
            var mockHashFactory = new Mock<ModelHashCalculator>();

            Assert.False(new ModelCompatibilityChecker()
                .CompatibleWithModel(mockInternalContext.Object, mockHashFactory.Object, throwIfNoMetadata: true));
        }

        [Fact]
        public void CompatibleWithModel_returns_false_if_model_does_not_match_database_and_method_is_asked_not_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest(modelMatches: false);
            var mockHashFactory = new Mock<ModelHashCalculator>();

            Assert.False(new ModelCompatibilityChecker()
                .CompatibleWithModel(mockInternalContext.Object, mockHashFactory.Object, throwIfNoMetadata: false));
        }

        [Fact]
        public void CompatibleWithModel_returns_true_if_model_matches_database_using_model_hash_check_and_method_is_asked_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest("<Hash>");
            var mockHashFactory = new Mock<ModelHashCalculator>();
            mockHashFactory.Setup(f => f.Calculate(It.IsAny<DbCompiledModel>())).Returns("<Hash>");

            Assert.True(new ModelCompatibilityChecker()
                .CompatibleWithModel(mockInternalContext.Object, mockHashFactory.Object, throwIfNoMetadata: true));
        }

        [Fact]
        public void CompatibleWithModel_returns_true_if_model_matches_database_using_model_hash_check_and_method_is_asked_not_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest("<Hash>");
            var mockHashFactory = new Mock<ModelHashCalculator>();
            mockHashFactory.Setup(f => f.Calculate(It.IsAny<DbCompiledModel>())).Returns("<Hash>");

            Assert.True(new ModelCompatibilityChecker()
                .CompatibleWithModel(mockInternalContext.Object, mockHashFactory.Object, throwIfNoMetadata: false));
        }

        [Fact]
        public void CompatibleWithModel_returns_false_if_model_does_not_match_database_using_model_hash_check_and_method_is_asked_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest("<Hash1>");
            var mockHashFactory = new Mock<ModelHashCalculator>();
            mockHashFactory.Setup(f => f.Calculate(It.IsAny<DbCompiledModel>())).Returns("<Hash2>");

            Assert.False(new ModelCompatibilityChecker()
                .CompatibleWithModel(mockInternalContext.Object, mockHashFactory.Object, throwIfNoMetadata: true));
        }

        [Fact]
        public void CompatibleWithModel_returns_false_if_model_does_not_match_database_using_model_hash_check_and_method_is_asked_not_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest("<Hash1>");
            var mockHashFactory = new Mock<ModelHashCalculator>();
            mockHashFactory.Setup(f => f.Calculate(It.IsAny<DbCompiledModel>())).Returns("<Hash2>");

            Assert.False(new ModelCompatibilityChecker()
                .CompatibleWithModel(mockInternalContext.Object, mockHashFactory.Object, throwIfNoMetadata: false));
        }

        [Fact]
        public void CompatibleWithModel_returns_true_if_model_is_not_from_code_first_and_method_is_asked_not_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest("<Hash>", codeFirst: false);
            var mockHashFactory = new Mock<ModelHashCalculator>();

            Assert.True(new ModelCompatibilityChecker()
                .CompatibleWithModel(mockInternalContext.Object, mockHashFactory.Object, throwIfNoMetadata: false));
        }

        [Fact]
        public void CompatibleWithModel_returns_true_if_database_has_no_hash_and_method_is_asked_not_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest(null);
            var mockHashFactory = new Mock<ModelHashCalculator>();
            mockHashFactory.Setup(f => f.Calculate(It.IsAny<DbCompiledModel>())).Returns("<Hash>");

            Assert.True(new ModelCompatibilityChecker()
                .CompatibleWithModel(mockInternalContext.Object, mockHashFactory.Object, throwIfNoMetadata: false));
        }

        [Fact]
        public void CompatibleWithModel_throws_if_model_is_not_from_code_first_and_method_is_asked_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest("<Hash>", codeFirst: false);
            var mockHashFactory = new Mock<ModelHashCalculator>();
            mockHashFactory.Setup(f => f.Calculate(It.IsAny<DbCompiledModel>())).Returns((string)null);

            Assert.Equal(Strings.Database_NonCodeFirstCompatibilityCheck, Assert.Throws<NotSupportedException>(() => new ModelCompatibilityChecker().CompatibleWithModel(
                mockInternalContext.Object,
                mockHashFactory.Object,
                throwIfNoMetadata: true)).Message);
        }

        [Fact]
        public void CompatibleWithModel_throws_if_database_has_no_hash_and_method_is_asked_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest(null);
            var mockHashFactory = new Mock<ModelHashCalculator>();
            mockHashFactory.Setup(f => f.Calculate(It.IsAny<DbCompiledModel>())).Returns("<Hash>");

            Assert.Equal(Strings.Database_NoDatabaseMetadata, Assert.Throws<NotSupportedException>(() => new ModelCompatibilityChecker().CompatibleWithModel(
                mockInternalContext.Object,
                mockHashFactory.Object,
                throwIfNoMetadata: true)).Message);
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
            mockOperations.Setup(m => m.Exists(null)).Returns(false);

            mockContext.Object.Owner.Database.Create();

            mockContext.Verify(m => m.CreateDatabase(null), Times.Once());

        }

        [Fact]
        public void Database_CreateIfNotExists_calls_CreateDatabase_if_database_does_not_exist()
        {
            var mockOperations = new Mock<DatabaseOperations>();
            var mockContext = new Mock<InternalContextForMock<FakeContext>>();
            mockContext.Setup(m => m.DatabaseOperations).Returns(mockOperations.Object);
            mockContext.Setup(m => m.CreateObjectContextForDdlOps()).Returns(new Mock<ClonedObjectContext>().Object);
            mockOperations.Setup(m => m.Exists(null)).Returns(false);

            mockContext.Object.Owner.Database.CreateIfNotExists();

            mockContext.Verify(m => m.CreateDatabase(null), Times.Once());

        }

        [Fact]
        public void Database_Create_throws_and_does_not_call_CreateDatabase_if_database_exists()
        {
            var mockOperations = new Mock<DatabaseOperations>();
            var mockContext = new Mock<InternalContextForMock<FakeContext>>();
            mockContext.Setup(m => m.DatabaseOperations).Returns(mockOperations.Object);
            mockContext.Setup(m => m.CreateObjectContextForDdlOps()).Returns(new Mock<ClonedObjectContext>().Object);
            mockContext.Setup(m => m.Connection).Returns(new SqlConnection("Database=Foo"));
            mockOperations.Setup(m => m.Exists(null)).Returns(true);

            Assert.Equal(Strings.Database_DatabaseAlreadyExists("Foo"), Assert.Throws<InvalidOperationException>(() => mockContext.Object.Owner.Database.Create()).Message);

            mockContext.Verify(m => m.CreateDatabase(null), Times.Never());

        }

        [Fact]
        public void Database_CreateIfNotExists_does_not_call_CreateDatabase_if_database_exists()
        {
            var mockOperations = new Mock<DatabaseOperations>();
            var mockContext = new Mock<InternalContextForMock<FakeContext>>();
            mockContext.Setup(m => m.DatabaseOperations).Returns(mockOperations.Object);
            mockContext.Setup(m => m.CreateObjectContextForDdlOps()).Returns(new Mock<ClonedObjectContext>().Object);
            mockOperations.Setup(m => m.Exists(null)).Returns(true);

            mockContext.Object.Owner.Database.CreateIfNotExists();

            mockContext.Verify(m => m.CreateDatabase(null), Times.Never());

        }

        [Fact]
        public void CreateDatabase_uses_core_provider_when_not_in_code_first_mode()
        {
            var mockOperations = new Mock<DatabaseOperations>();
            var mockContext = CreateMockContextForMigrator(mockOperations, codeFirst: false);

            new DatabaseCreator().CreateDatabase(
                mockContext.Object,
                (config, context) => { Assert.True(false); return null; },
                null);

            mockOperations.Verify(m => m.Create(null), Times.Once());
            mockContext.Verify(m => m.SaveMetadataToDatabase(), Times.Once());
            mockContext.Verify(m => m.MarkDatabaseInitialized(), Times.Once());
        }

        [Fact]
        public void CreateDatabase_uses_core_provider_when_provider_is_not_SQL_Server_or_SQL_CE()
        {
            var mockOperations = new Mock<DatabaseOperations>();
            var mockContext = CreateMockContextForMigrator(mockOperations);
            mockContext.Setup(m => m.ProviderName).Returns("Some.Other.Provider");

            new DatabaseCreator().CreateDatabase(
                mockContext.Object,
                (config, context) => { Assert.True(false); return null; },
                null);

            mockOperations.Verify(m => m.Create(null), Times.Once());
            mockContext.Verify(m => m.SaveMetadataToDatabase(), Times.Once());
            mockContext.Verify(m => m.MarkDatabaseInitialized(), Times.Once());
        }

        [Fact]
        public void CreateDatabase_uses_Migrations_when_provider_is_SQL_Server()
        {
            CreateDatabase_uses_Migrations_when_provider_is_known("System.Data.SqlClient");
        }

        [Fact]
        public void CreateDatabase_uses_Migrations_when_provider_is_SQL_CE()
        {
            CreateDatabase_uses_Migrations_when_provider_is_known("System.Data.SqlServerCe.4.0");
        }

        private void CreateDatabase_uses_Migrations_when_provider_is_known(string provider)
        {
            var mockOperations = new Mock<DatabaseOperations>();
            var mockContext = CreateMockContextForMigrator(mockOperations);
            mockContext.Setup(m => m.ProviderName).Returns(provider);

            Mock<DbMigrator> mockMigrator = null;

            new DatabaseCreator().CreateDatabase(
                mockContext.Object,
                (config, context) => (mockMigrator = new Mock<DbMigrator>(config, context)).Object,
                null);

            mockMigrator.Verify(m => m.Update(null), Times.Once());
            mockOperations.Verify(m => m.Create(null), Times.Never());
            mockContext.Verify(m => m.SaveMetadataToDatabase(), Times.Never());
            mockContext.Verify(m => m.MarkDatabaseInitialized(), Times.Once());
        }

        [Fact]
        public void CreateDatabase_using_Migrations_configures_a_migrator_appropriately()
        {
            DbMigrationsConfiguration configuration = null;

            new DatabaseCreator().CreateDatabase(
                CreateMockContextForMigrator().Object,
                (config, context) =>
                {
                    configuration = config;
                    return new Mock<DbMigrator>(config, context).Object;
                },
                null);

            Assert.True(typeof(FakeContext).IsAssignableFrom(configuration.ContextType));
            Assert.True(configuration.AutomaticMigrationsEnabled);
            Assert.Equal("Database=Foo",
                            configuration.TargetDatabase.GetConnectionString(AppConfig.DefaultInstance).ConnectionString);
        }

        [Fact]
        public void CreateDatabase_using_Migrations_calls_Update_on_a_migrator_and_marks_database_as_initialized()
        {
            var mockContext = CreateMockContextForMigrator();
            Mock<DbMigrator> mockMigrator = null;

            new DatabaseCreator().CreateDatabase(
                mockContext.Object,
                (config, context) => (mockMigrator = new Mock<DbMigrator>(config, context)).Object,
                null);

            mockMigrator.Verify(m => m.Update(null), Times.Once());
            mockContext.Verify(m => m.MarkDatabaseInitialized(), Times.Once());
        }

        [Fact]
        public void CreateDatabase_using_Migrations_does_not_disable_initializers()
        {
            Mock<DbMigrator> mockMigrator = null;

            new DatabaseCreator().CreateDatabase(
                CreateMockContextForMigrator().Object,
                (config, context) => (mockMigrator = new Mock<DbMigrator>(config, context)).Object,
                null);

            mockMigrator.Verify(m => m.DisableInitializer(It.IsAny<Type>()), Times.Never());
        }

        [Fact]
        public void CreateDatabase_using_Migrations_does_not_dispose_users_context()
        {
            var mockContext = CreateMockContextForMigrator();

            new DatabaseCreator().CreateDatabase(
                mockContext.Object,
                (config, context) => new Mock<DbMigrator>(config, context).Object,
                null);

            Assert.False(mockContext.Object.IsDisposed);
        }

        public class FakeContext : DbContext
        {
            static FakeContext()
            {
                Database.SetInitializer<FakeContext>(null);
            }
        }

        private Mock<InternalContextForMock<FakeContext>> CreateMockContextForMigrator(Mock<DatabaseOperations> mockOperations = null, bool codeFirst = true)
        {
            var mockCompiledModel = new Mock<DbCompiledModel>();
            mockCompiledModel.Setup(m => m.CachedModelBuilder).Returns(new DbModelBuilder());

            var mockContext = new Mock<InternalContextForMock<FakeContext>>();
            mockContext.Setup(m => m.CodeFirstModel).Returns(codeFirst ? mockCompiledModel.Object : null);
            mockContext.Setup(m => m.OriginalConnectionString).Returns("Database=Foo");
            mockContext.Setup(m => m.ProviderName).Returns("System.Data.SqlClient");
            mockContext.Setup(m => m.ModelProviderInfo).Returns(ProviderRegistry.Sql2008_ProviderInfo);
            mockContext.Setup(m => m.Connection).Returns(new SqlConnection());

            if (mockOperations != null)
            {
                mockContext.Setup(m => m.DatabaseOperations).Returns(mockOperations.Object);
            }

            return mockContext;
        }

        #endregion
    }
}
