// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using System.Data.Entity.TestHelpers;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Xml.Linq;
    using Moq;
    using Moq.Protected;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    public class DbMigratorTests : DbTestCase
    {
        [MigrationsTheory]
        public void Scaffold_throws_when_pending_migrations()
        {
            var migrator = CreateMigrator<ShopContext_v1>();
            var migration = new MigrationScaffolder(migrator.Configuration).Scaffold("M1");

            Assert.Equal(
                Strings.MigrationsPendingException(migration.MigrationId),
                Assert.Throws<MigrationsPendingException>(
                    () => CreateMigrator<ShopContext_v1>(
                        scaffoldedMigrations: new[] { migration })
                              .Scaffold("M2", "N", false)).Message);
        }

        private class ContextWithNonDefaultCtor : ShopContext_v1
        {
            public ContextWithNonDefaultCtor(string nameOrConnectionString)
                : base(nameOrConnectionString)
            {
            }
        }

        [MigrationsTheory(SkipForLocalDb = true, SkipForSqlAzure = true, Justification = "Test is hard-coded to Sql Express")]
        public void TargetDatabase_should_return_correct_info_for_logging()
        {
            var migrator = CreateMigrator<ShopContext_v1>();

            WhenNotSqlCe(
                () =>
                Assert.Equal(
                    @"'MigrationsTest' (DataSource: .\SQLEXPRESS, Provider: System.Data.SqlClient, Origin: Explicit)",
                    migrator.TargetDatabase));

            WhenSqlCe(
                () =>
                Assert.Equal(
                    "'"
                    + AppDomain.CurrentDomain.BaseDirectory
                    + @"\MigrationsTest.sdf' (DataSource: "
                    + AppDomain.CurrentDomain.BaseDirectory
                    + @"\MigrationsTest.sdf, Provider: System.Data.SqlServerCe.4.0, Origin: Explicit)",
                    migrator.TargetDatabase));
        }

        [MigrationsTheory]
        public void Non_constructible_context_should_throw()
        {
            Assert.Equal(
                Strings.ContextNotConstructible(typeof(ContextWithNonDefaultCtor)),
                Assert.Throws<MigrationsException>(() => CreateMigrator<ContextWithNonDefaultCtor>()).Message);
        }

        [MigrationsTheory]
        public void GetMigrations_should_return_migrations_list()
        {
            var migrator = CreateMigrator<ShopContext_v1>();

            Assert.True(!migrator.GetLocalMigrations().Any());

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            migrator = CreateMigrator<ShopContext_v1>(
                automaticMigrationsEnabled: false,
                scaffoldedMigrations: generatedMigration);

            Assert.Equal(1, migrator.GetLocalMigrations().Count());
        }

        private static void DropMigrationHistoryAndAddEdmMetadata(DbConnection connection, string hash)
        {
            using (var poker = new EdmMetadataContext(connection, contextOwnsConnection: false))
            {
                poker.Database.ExecuteSqlCommand("drop table " + HistoryContext.DefaultTableName);

                poker.Database.ExecuteSqlCommand(
                    ((IObjectContextAdapter)poker).ObjectContext.CreateDatabaseScript());

#pragma warning disable 612,618
                poker.Metadata.Add(
                    new EdmMetadata
                        {
                            ModelHash = hash
                        });
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
                    context.Database.Create();

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

            Assert.True(TableExists(HistoryContext.DefaultTableName));
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
                    context.Database.Create();

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

            Assert.False(TableExists(HistoryContext.DefaultTableName));
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
                    context.Database.Create();

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

            Assert.False(TableExists(HistoryContext.DefaultTableName));
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
                    context.Database.Create();

                    DropMigrationHistoryAndAddEdmMetadata(
                        context.Database.Connection,
#pragma warning disable 612,618
                        EdmMetadata.TryGetModelHash(context));
#pragma warning restore 612,618

                    Assert.True(TableExists("EdmMetadata"));

                    var migrator = CreateMigrator<ShopContext_v1b>();

                    Assert.Equal(
                        Strings.MetadataOutOfDate,
                        Assert.Throws<MigrationsException>(() => migrator.Scaffold("GrowUp", null, ignoreChanges: false)).Message);
                }
            }
            finally
            {
                Database.SetInitializer(new CreateDatabaseIfNotExists<ShopContext_v1>());
            }

            Assert.False(TableExists(HistoryContext.DefaultTableName));
        }

        [MigrationsTheory]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal("configuration", Assert.Throws<ArgumentNullException>(
                () => new DbMigrator((DbMigrationsConfiguration)null)).ParamName);
        }

        [MigrationsTheory]
        public void ScaffoldInitialCreate_should_return_null_when_no_db()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            var scaffoldedMigration = migrator.ScaffoldInitialCreate("Foo");

            Assert.Null(scaffoldedMigration);
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

            var migrator = CreateMigrator<ShopContext_v1>(contextKey: typeof(ShopContext_v1).FullName);

            migrator.Update();

            using (var context = CreateContext<ShopContext_v1>())
            {
                Assert.True(context.Database.CompatibleWithModel(true));
            }
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

            Assert.Equal(
                new AutomaticMigrationsDisabledException(Strings.AutomaticDisabledException).Message,
                Assert.Throws<AutomaticMigrationsDisabledException>(() => migrator.Update()).Message);
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

    public class DbMigratorTests_DatabaseInitialization : DbTestCase
    {
        public class CustomInitizalzer : IDatabaseInitializer<DoNotInitContext>
        {
            public static bool HasRun { get; set; }

            public void InitializeDatabase(DoNotInitContext context)
            {
                HasRun = true;
            }
        }

        public class DoNotInitContext : DbContext
        {
            static DoNotInitContext()
            {
                Database.SetInitializer(new CustomInitizalzer());
            }

            public DoNotInitContext()
                : base(GetConnection(), contextOwnsConnection: true)
            {
            }

            private static DbConnection GetConnection()
            {
                return
                    new SqlConnection(
                        "Data Source=.\\sqlexpress;Initial Catalog=DbMigratorTests_DatabaseInitialization;Integrated Security=True");
            }
        }

        [MigrationsTheory]
        public void DbMigrator_does_not_cause_database_initializer_to_run()
        {
            ResetDatabase();

            var migrator = CreateMigrator<DoNotInitContext>();

            migrator.Update();

            Assert.True(DatabaseExists());
            Assert.False(CustomInitizalzer.HasRun);
        }
    }

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    public class DbMigratorTests_SqlClientOnly : DbTestCase
    {
        [MigrationsTheory]
        [UseDefaultExecutionStrategy]
        public void ExecuteSql_should_honor_CommandTimeout()
        {
            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Configuration.CommandTimeout = 1;

            var migrationStatements
                = new[]
                      {
                          new MigrationStatement
                              {
                                  Sql = "WAITFOR DELAY '00:00:02'"
                              }
                      };

            var ex = Assert.Throws<SqlException>(
                () => migrator.ExecuteStatements(migrationStatements));

            Assert.Equal(-2, ex.Number);
        }

        [MigrationsTheory]
        [UseDefaultExecutionStrategy]
        public void ExecuteSql_when_SuppressTransaction_should_honor_CommandTimeout()
        {
            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Configuration.CommandTimeout = 1;

            var migrationStatements
                = new[]
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

    public class ExecuteStatements : TestBase
    {
        [Fact]
        public void Uses_ExecutionStrategy()
        {
            var configuration = new DbMigrationsConfiguration
            {
                ContextType = typeof(ShopContext_v1),
                MigrationsAssembly = SystemComponentModelDataAnnotationsAssembly,
                MigrationsNamespace = typeof(ShopContext_v1).Namespace
            };

            var migrator = new DbMigrator(configuration);

            var executionStrategyMock = new Mock<IDbExecutionStrategy>();

            MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
            try
            {
                migrator.ExecuteStatements(Enumerable.Empty<MigrationStatement>());
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }

            executionStrategyMock.Verify(m => m.Execute(It.IsAny<Action>()), Times.Once());
        }
    }

    public class ExecuteSql : TestBase
    {
        [Fact]
        public void ExecuteSql_dispatches_commands_to_interceptors()
        {
            var mockCommand = new Mock<DbCommand>();
            mockCommand.Setup(m => m.ExecuteNonQuery()).Returns(2013);

            var mockConnection = new Mock<DbConnection>();
            mockConnection.Protected().Setup<DbCommand>("CreateDbCommand").Returns(mockCommand.Object);

            var mockTransaction = new Mock<DbTransaction>();
            mockTransaction.Protected().Setup<DbConnection>("DbConnection").Returns(mockConnection.Object);

            var migrator = new DbMigrator();
            var statement = new MigrationStatement
                {
                    Sql = "Some Sql"
                };

            var mockInterceptor = new Mock<DbCommandInterceptor> { CallBase = true };
            DbInterception.Add(mockInterceptor.Object);

            try
            {
                migrator.ExecuteSql(mockTransaction.Object, statement, new DbInterceptionContext());
            }
            finally
            {
                DbInterception.Remove(mockInterceptor.Object);
            }

            mockInterceptor.Verify(m => m.NonQueryExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()));
            mockInterceptor.Verify(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()));
        }

        [Fact]
        public void ExecuteSql_with_transactions_suppressed_dispatches_commands_to_interceptors()
        {
            var mockCommand = new Mock<DbCommand>();
            mockCommand.Setup(m => m.ExecuteNonQuery()).Returns(2013);

            var mockConnection = new Mock<DbConnection>();
            mockConnection.Protected().Setup<DbCommand>("CreateDbCommand").Returns(mockCommand.Object);

            var mockTransaction = new Mock<DbTransaction>();
            mockTransaction.Protected().Setup<DbConnection>("DbConnection").Returns(mockConnection.Object);

            var mockFactory = new Mock<DbProviderFactory>();
            mockFactory.Setup(m => m.CreateConnection()).Returns(mockConnection.Object);

            var objectContext = new ObjectContext();
            var mockInternalContext = new Mock<InternalContextForMock>();
            mockInternalContext.Setup(m => m.ObjectContext).Returns(objectContext);
            var context = mockInternalContext.Object.Owner;
            objectContext.InterceptionContext = objectContext.InterceptionContext.WithDbContext(context);

            var migrator = new DbMigrator(context, mockFactory.Object);
            var statement = new MigrationStatement
                {
                    Sql = "Some Sql",
                    SuppressTransaction = true
                };

            var mockInterceptor = new Mock<DbCommandInterceptor> { CallBase = true };
            DbInterception.Add(mockInterceptor.Object);

            try
            {
                migrator.ExecuteSql(mockTransaction.Object, statement, objectContext.InterceptionContext);
            }
            finally
            {
                DbInterception.Remove(mockInterceptor.Object);
            }

            mockInterceptor.Verify(m => m.NonQueryExecuting(
                    mockCommand.Object,
                    It.Is<DbCommandInterceptionContext<int>>(c => c.DbContexts.Contains(context))));

            mockInterceptor.Verify(m => m.NonQueryExecuted(
                    mockCommand.Object,
                    It.Is<DbCommandInterceptionContext<int>>(c => c.DbContexts.Contains(context) && c.Result == 2013)));
        }
    }

    public class Upgrade : TestBase
    {
        [Fact]
        public void Upgrade_does_not_do_model_diff_or_run_Seed_when_using_initializer_path()
        {
            var mockMigrator = new Mock<DbMigrator>(null, null, new Mock<MigrationAssembly>().Object) { CallBase = true };
            mockMigrator.Setup(m => m.IsModelOutOfDate(It.IsAny<XDocument>(), It.IsAny<DbMigration>())).Returns(false);

            mockMigrator.Object.Upgrade(Enumerable.Empty<string>(), "A", "B");

            mockMigrator.Verify(m => m.SeedDatabase(), Times.Never());
            mockMigrator.Verify(m => m.IsModelOutOfDate(It.IsAny<XDocument>(), It.IsAny<DbMigration>()), Times.Never());
        }
    }
}
