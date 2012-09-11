// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.Edm;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Migrations.Utilities;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using Xunit;

    public enum DatabaseProvider
    {
        SqlClient,
        SqlServerCe
    }

    public enum ProgrammingLanguage
    {
        CSharp,
        VB
    }

    public abstract class DbTestCase : TestBase, IUseFixture<DatabaseProviderFixture>
    {
        private DatabaseProviderFixture _databaseProviderFixture;

        private DatabaseProvider _databaseProvider = DatabaseProvider.SqlClient;

        private ProgrammingLanguage _programmingLanguage = ProgrammingLanguage.CSharp;

        public DatabaseProvider DatabaseProvider
        {
            get { return _databaseProvider; }
            set
            {
                _databaseProvider = value;
                TestDatabase = _databaseProviderFixture.TestDatabases[_databaseProvider];
            }
        }

        public ProgrammingLanguage ProgrammingLanguage
        {
            get { return _programmingLanguage; }
            set
            {
                _programmingLanguage = value;
                CodeGenerator = _databaseProviderFixture.CodeGenerators[_programmingLanguage];
                MigrationCompiler = _databaseProviderFixture.MigrationCompilers[_programmingLanguage];
            }
        }

        public TestDatabase TestDatabase { get; private set; }

        public MigrationCodeGenerator CodeGenerator { get; private set; }

        public MigrationCompiler MigrationCompiler { get; private set; }

        public virtual void Init(DatabaseProvider provider, ProgrammingLanguage language)
        {
            try
            {
                _databaseProvider = provider;
                _programmingLanguage = language;

                TestDatabase = _databaseProviderFixture.TestDatabases[_databaseProvider];
                CodeGenerator = _databaseProviderFixture.CodeGenerators[_programmingLanguage];
                MigrationCompiler = _databaseProviderFixture.MigrationCompilers[_programmingLanguage];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                throw;
            }
        }

        public void WhenSqlCe(Action action)
        {
            if (_databaseProvider == DatabaseProvider.SqlServerCe)
            {
                action();
            }
        }

        public void WhenNotSqlCe(Action action)
        {
            if (_databaseProvider != DatabaseProvider.SqlServerCe)
            {
                action();
            }
        }

        public DbMigrator CreateMigrator<TContext, TMigration>()
            where TContext : DbContext
            where TMigration : DbMigration, new()
        {
            var migrationsConfiguration = CreateMigrationsConfiguration<TContext>();

            migrationsConfiguration.MigrationsAssembly = typeof(TMigration).Assembly;

            return new DbMigrator(migrationsConfiguration);
        }

        public DbMigrator CreateMigrator<TContext>(DbMigration migration)
            where TContext : DbContext
        {
            var modelCompressor = new ModelCompressor();

            var generatedMigration
                = CodeGenerator
                    .Generate(
                        UtcNowGenerator.UtcNowAsMigrationIdTimestamp() + "_" + migration.GetType().Name,
                        migration.GetOperations(),
                        Convert.ToBase64String(modelCompressor.Compress(CreateContext<TContext>().GetModel())),
                        Convert.ToBase64String(modelCompressor.Compress(CreateContext<TContext>().GetModel())),
                        "System.Data.Entity.Migrations",
                        migration.GetType().Name);

            return new DbMigrator(CreateMigrationsConfiguration<TContext>(scaffoldedMigrations: generatedMigration));
        }

        public DbMigrator CreateMigrator<TContext>(
            bool automaticMigrationsEnabled = true,
            bool automaticDataLossEnabled = false,
            string targetDatabase = null,
            string contextKey = null,
            params ScaffoldedMigration[] scaffoldedMigrations)
            where TContext : DbContext
        {
            return new DbMigrator(
                CreateMigrationsConfiguration<TContext>(
                    automaticMigrationsEnabled,
                    automaticDataLossEnabled,
                    targetDatabase,
                    contextKey,
                    scaffoldedMigrations));
        }

        public DbMigrationsConfiguration CreateMigrationsConfiguration<TContext>(
            bool automaticMigrationsEnabled = true,
            bool automaticDataLossEnabled = false,
            string targetDatabase = null,
            string contextKey = null,
            params ScaffoldedMigration[] scaffoldedMigrations)
            where TContext : DbContext
        {
            var migrationsConfiguration
                = new DbMigrationsConfiguration
                      {
                          AutomaticMigrationsEnabled = automaticMigrationsEnabled,
                          AutomaticMigrationDataLossAllowed = automaticDataLossEnabled,
                          ContextType = typeof(TContext),
                          MigrationsAssembly = SystemComponentModelDataAnnotationsAssembly,
                          MigrationsNamespace = typeof(TContext).Namespace
                      };

            if (!string.IsNullOrWhiteSpace(contextKey))
            {
                migrationsConfiguration.ContextKey = contextKey;
            }

            if (!string.IsNullOrWhiteSpace(targetDatabase))
            {
                TestDatabase = DatabaseProviderFixture.InitializeTestDatabase(DatabaseProvider, targetDatabase);
            }

            if ((scaffoldedMigrations != null)
                && scaffoldedMigrations.Any())
            {
                migrationsConfiguration.MigrationsAssembly = MigrationCompiler.Compile(
                    migrationsConfiguration.MigrationsNamespace,
                    scaffoldedMigrations);
            }

            migrationsConfiguration.TargetDatabase = new DbConnectionInfo(TestDatabase.ConnectionString, TestDatabase.ProviderName);

            migrationsConfiguration.CodeGenerator = CodeGenerator;

            return migrationsConfiguration;
        }

        public void ConfigureMigrationsConfiguration(DbMigrationsConfiguration migrationsConfiguration)
        {
            migrationsConfiguration.TargetDatabase = new DbConnectionInfo(TestDatabase.ConnectionString, TestDatabase.ProviderName);
            migrationsConfiguration.CodeGenerator = CodeGenerator;

            migrationsConfiguration.MigrationsAssembly = SystemComponentModelDataAnnotationsAssembly;
        }

        public TContext CreateContext<TContext>()
            where TContext : DbContext
        {
            var contextInfo = new DbContextInfo(
                typeof(TContext), new DbConnectionInfo(TestDatabase.ConnectionString, TestDatabase.ProviderName));

            return (TContext)contextInfo.CreateInstance();
        }

        public void ResetDatabase()
        {
            if (DatabaseExists())
            {
                TestDatabase.ResetDatabase();
            }
            else
            {
                TestDatabase.EnsureDatabase();
            }
        }

        public void DropDatabase()
        {
            if (DatabaseExists())
            {
                TestDatabase.DropDatabase();
            }
        }

        public bool DatabaseExists()
        {
            return TestDatabase.Exists();
        }

        public bool TableExists(string name)
        {
            return Info.TableExists(name);
        }

        public bool ColumnExists(string table, string name)
        {
            return Info.ColumnExists(table, name);
        }

        public string ConnectionString
        {
            get { return TestDatabase.ConnectionString; }
        }

        public DbProviderFactory ProviderFactory
        {
            get { return DbProviderFactories.GetFactory(TestDatabase.ProviderName); }
        }

        public string ProviderManifestToken
        {
            get { return TestDatabase.ProviderManifestToken; }
        }

        public DbProviderInfo ProviderInfo
        {
            get { return new DbProviderInfo(TestDatabase.ProviderName, ProviderManifestToken); }
        }

        public MigrationSqlGenerator SqlGenerator
        {
            get { return TestDatabase.SqlGenerator; }
        }

        public InfoContext Info
        {
            get { return TestDatabase.Info; }
        }

        public void SetFixture(DatabaseProviderFixture databaseProviderFixture)
        {
            _databaseProviderFixture = databaseProviderFixture;
        }

        public void ExecuteOperations(params MigrationOperation[] operations)
        {
            using (var connection = ProviderFactory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;

                foreach (var migrationStatement in SqlGenerator.Generate(operations, ProviderManifestToken))
                {
                    using (var command = connection.CreateCommand())
                    {
                        if (connection.State
                            != ConnectionState.Open)
                        {
                            connection.Open();
                        }

                        command.CommandText = migrationStatement.Sql;
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public CreateTableOperation GetCreateHistoryTableOperation()
        {
            using (var connection = ProviderFactory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;

                return (CreateTableOperation)
                       new EdmModelDiffer().Diff(
                           new DbModelBuilder().Build(ProviderInfo).GetModel(),
                           new HistoryContext(connection, contextOwnsConnection: true, defaultSchema: null).GetModel(),
                           includeSystemOperations: true)
                           .Single();
            }
        }

        public DropTableOperation GetDropHistoryTableOperation()
        {
            using (var connection = ProviderFactory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;

                return (DropTableOperation)
                       new EdmModelDiffer().Diff(
                           new HistoryContext(connection, contextOwnsConnection: true, defaultSchema: null).GetModel(),
                           new DbModelBuilder().Build(ProviderInfo).GetModel(),
                           includeSystemOperations: true)
                           .Single();
            }
        }
    }
}
