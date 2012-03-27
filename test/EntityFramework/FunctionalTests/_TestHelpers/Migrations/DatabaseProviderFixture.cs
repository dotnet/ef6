namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Design;

    public class DatabaseProviderFixture
    {
        public const string DefaultDatabaseName = "MigrationsTest";

        private Dictionary<DatabaseProvider, TestDatabase> _testDatabases = new Dictionary<DatabaseProvider, TestDatabase>();
        private Dictionary<ProgrammingLanguage, MigrationCodeGenerator> _codeGenerators = new Dictionary<ProgrammingLanguage, MigrationCodeGenerator>();
        private Dictionary<ProgrammingLanguage, MigrationCompiler> _migrationCompilers = new Dictionary<ProgrammingLanguage, MigrationCompiler>();

        public DatabaseProviderFixture()
        {
            foreach (DatabaseProvider provider in Enum.GetValues(typeof(DatabaseProvider)))
            {
                _testDatabases[provider] = InitializeTestDatabase(provider, DefaultDatabaseName);
            }

            _codeGenerators[Migrations.ProgrammingLanguage.CSharp] = new CSharpMigrationCodeGenerator();
            _migrationCompilers[Migrations.ProgrammingLanguage.CSharp] = new MigrationCompiler("cs"); ;
            _codeGenerators[Migrations.ProgrammingLanguage.VB] = new VisualBasicMigrationCodeGenerator();
            _migrationCompilers[Migrations.ProgrammingLanguage.VB] = new MigrationCompiler("vb");
        }

        public Dictionary<DatabaseProvider, TestDatabase> TestDatabases
        {
            get { return _testDatabases; }
        }

        public Dictionary<ProgrammingLanguage, MigrationCodeGenerator> CodeGenerators
        {
            get { return _codeGenerators; }
        }

        public Dictionary<ProgrammingLanguage, MigrationCompiler> MigrationCompilers
        {
            get { return _migrationCompilers; }
        }

        public static TestDatabase InitializeTestDatabase(DatabaseProvider provider, string databaseName)
        {
            TestDatabase testDatabase;
            switch (provider)
            {
                case DatabaseProvider.SqlClient:
                    testDatabase = new SqlTestDatabase(databaseName);
                    break;
                case DatabaseProvider.SqlServerCe:
                    testDatabase = new SqlCeTestDatabase(databaseName);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported provider");
            }

            testDatabase.EnsureDatabase();
            return testDatabase;
        }
    }
}
