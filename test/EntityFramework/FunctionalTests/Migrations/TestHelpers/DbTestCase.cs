// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Functionals.Utilities;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Migrations.Utilities;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Linq;
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

    public class BlankSlate : DbContext
    {
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
            using (var context = CreateContext<TContext>())
            {
                var generatedMigration
                    = CodeGenerator
                        .Generate(
                            GenerateUniqueMigrationName(migration.GetType().Name),
                            migration.GetOperations(),
                            Convert.ToBase64String(CompressModel(GetModel(context))),
                            Convert.ToBase64String(CompressModel(GetModel(context))),
                            "System.Data.Entity.Migrations",
                            migration.GetType().Name);

                return new DbMigrator(CreateMigrationsConfiguration<TContext>(scaffoldedMigrations: generatedMigration));
            }
        }

        public DbMigrator CreateMigrator<TContext>(
            bool automaticMigrationsEnabled = true,
            bool automaticDataLossEnabled = false,
            string targetDatabase = null,
            string contextKey = null,
            Func<DbConnection, string, HistoryContext> historyContextFactory = null,
            params ScaffoldedMigration[] scaffoldedMigrations)
            where TContext : DbContext
        {
            return new DbMigrator(
                CreateMigrationsConfiguration<TContext>(
                    automaticMigrationsEnabled,
                    automaticDataLossEnabled,
                    targetDatabase,
                    contextKey,
                    historyContextFactory,
                    scaffoldedMigrations));
        }

        public DbMigrationsConfiguration CreateMigrationsConfiguration<TContext>(
            bool automaticMigrationsEnabled = true,
            bool automaticDataLossEnabled = false,
            string targetDatabase = null,
            string contextKey = null,
            Func<DbConnection, string, HistoryContext> historyContextFactory = null,
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

            if (historyContextFactory != null)
            {
                migrationsConfiguration.SetHistoryContextFactory(TestDatabase.ProviderName, historyContextFactory);
            }

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

        public int GetColumnIndex(string table, string name)
        {
            return Info.GetColumnIndex(table, name);
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

        protected CreateTableOperation GetCreateHistoryTableOperation(string schema = "dbo")
        {
            var createTableOperation = new CreateTableOperation(schema + ".__MigrationHistory");

            var migrationId = new ColumnModel(PrimitiveTypeKind.String)
            {
                Name = "MigrationId",
                MaxLength = 150,
                //StoreType = "nvarchar",
            };

            var contextKey = new ColumnModel(PrimitiveTypeKind.String)
            {
                Name = "ContextKey",
                MaxLength = 300,
                //StoreType = "nvarchar"
            };

            var model = new ColumnModel(PrimitiveTypeKind.Binary)
            {
                Name = "Model",
                //StoreType = "image",
            };

            var productVersion = new ColumnModel(PrimitiveTypeKind.String)
            {
                Name = "ProductVersion",
                MaxLength = 32,
                //StoreType = "nvarchar",
            };

            createTableOperation.Columns.Add(migrationId);
            createTableOperation.Columns.Add(contextKey);
            createTableOperation.Columns.Add(model);
            createTableOperation.Columns.Add(productVersion);

            createTableOperation.PrimaryKey = new AddPrimaryKeyOperation();
            createTableOperation.PrimaryKey.Columns.Add("MigrationId");
            createTableOperation.PrimaryKey.Columns.Add("ContextKey");

            return createTableOperation;
        }

        protected DropTableOperation GetDropHistoryTableOperation(string schema = "dbo")
        {
            var createHistoryTableOperation = GetCreateHistoryTableOperation(schema);

            return new DropTableOperation(schema + ".__MigrationHistory", createHistoryTableOperation);
        }

        protected void AssertHistoryContextEntryExists(string contextKey)
        {
            using (var connection = ProviderFactory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                using (var historyContext = new HistoryContext(connection, "dbo"))
                {
                    Assert.True(historyContext.History.Any(h => h.ContextKey == contextKey));
                }
            }
        }

        protected void AssertHistoryContextDoesNotExist()
        {
            using (var connection = ProviderFactory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                using (var historyContext = new HistoryContext(connection, "dbo"))
                {
                    Assert.Throws<EntityCommandExecutionException>(() => historyContext.History.Count());
                }
            }
        }

        protected HistoryOperation CreateInsertOperation(string contextKey, string migrationId, XDocument model)
        {
            var productVersion = typeof(DbContext).Assembly
                .GetCustomAttributes<AssemblyInformationalVersionAttribute>()
                .Single()
                .InformationalVersion;

            using (var connection = ProviderFactory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                using (var historyContext = new HistoryContext(connection, "dbo"))
                {
                    historyContext.History.Add(
                        new HistoryRow
                        {
                            MigrationId = migrationId,
                            ContextKey = contextKey,
                            Model = CompressModel(model),
                            ProductVersion = productVersion,
                        });

                    var cancellingLogger = new CommandTreeCancellingLogger(historyContext);
                    DbInterception.Add(cancellingLogger);

                    historyContext.SaveChanges();

                    return new HistoryOperation(
                        cancellingLogger.Log.OfType<DbModificationCommandTree>().ToList());
                }
            }
        }

        protected XDocument GetModel(DbModel model)
        {
            return GetModel(w => EdmxWriter.WriteEdmx(model, w));
        }

        protected XDocument GetModel(DbContext context)
        {
            return GetModel(w => EdmxWriter.WriteEdmx(context, w));
        }

        protected string GenerateUniqueMigrationName(string migrationName)
        {
            return DateTime.UtcNow.ToString("yyyyMMddHHmmssf", CultureInfo.InvariantCulture) + "_" + migrationName;
        }

        private XDocument GetModel(Action<XmlWriter> writeXml)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(memoryStream))
                {
                    writeXml(xmlWriter);
                }

                memoryStream.Position = 0;

                return XDocument.Load(memoryStream);
            }
        }

        private byte[] CompressModel(XDocument model)
        {
            using (var outStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(outStream, CompressionMode.Compress))
                {
                    model.Save(gzipStream);
                }

                return outStream.ToArray();
            }
        }

        private class CommandTreeCancellingLogger : DbCommandInterceptor, IDbCommandTreeInterceptor
        {
            private readonly DbContext _context;

            public CommandTreeCancellingLogger(DbContext context)
            {
                _context = context;
                Log = new List<DbCommandTree>();
            }

            public IList<DbCommandTree> Log { get; private set; }

            public override void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
            {
                if (interceptionContext.DbContexts.Contains(_context))
                {
                    interceptionContext.Result = 1;
                }
            }

            public void TreeCreated(DbCommandTreeInterceptionContext interceptionContext)
            {
                if (interceptionContext.DbContexts.Contains(_context))
                {
                    Log.Add(interceptionContext.Result);
                }
            }
        }
    }
}
