// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Migrations.History;
    using System.Data.SqlClient;
    using System.Reflection;
    using Xunit;

    public class DatabaseExistsInInitializerTests : FunctionalTestBase, IDisposable
    {
        private const string Password = "PLACEHOLDER";
        private const string NormalUser = "EFGooseWithDbVisibility";
        private const string ImpairedUser = "EFGooseWithoutDbVisibility";
        private const string DatabaseWithMigrationHistory = "MigratoryGoose";
        private const string DatabaseWithoutMigrationHistory = "NonMigratoryGoose";
        private const string DatabaseOutOfDate = "MigratedGoose";

        public DatabaseExistsInInitializerTests()
        {
            EnsureDatabaseExists(DatabaseWithMigrationHistory, drophistoryTable: false, outOfDate: false);
            EnsureUserExists(DatabaseWithMigrationHistory, NormalUser, allowMasterQuery: true);
            EnsureUserExists(DatabaseWithMigrationHistory, ImpairedUser, allowMasterQuery: false);

            EnsureDatabaseExists(DatabaseWithoutMigrationHistory, drophistoryTable: true, outOfDate: false);
            EnsureUserExists(DatabaseWithoutMigrationHistory, NormalUser, allowMasterQuery: true);
            EnsureUserExists(DatabaseWithoutMigrationHistory, ImpairedUser, allowMasterQuery: false);

            EnsureDatabaseExists(DatabaseOutOfDate, drophistoryTable: false, outOfDate: true);
            EnsureUserExists(DatabaseOutOfDate, NormalUser, allowMasterQuery: true);
            EnsureUserExists(DatabaseOutOfDate, ImpairedUser, allowMasterQuery: false);

            MutableResolver.AddResolver<IManifestTokenResolver>(
                new SingletonDependencyResolver<IManifestTokenResolver>(new BasicManifestTokenResolver()));
        }

        public void Dispose()
        {
            MutableResolver.ClearResolvers();
        }

        [Fact]
        public void Exists_check_with_master_persist_info()
        {
            ExistsTest(DatabaseWithMigrationHistory, NormalUser, persistSecurityInfo: true);
        }

        [Fact]
        public void Exists_check_with_master_no_persist_info()
        {
            ExistsTest(DatabaseWithMigrationHistory, NormalUser, persistSecurityInfo: false);
        }

        [Fact]
        public void Exists_check_without_master_persist_info()
        {
            ExistsTestNoMaster(DatabaseWithMigrationHistory, NormalUser, persistSecurityInfo: true);
        }

        [Fact]
        public void Exists_check_without_master_no_persist_info()
        {
            ExistsTestNoMaster(DatabaseWithMigrationHistory, NormalUser, persistSecurityInfo: false);
        }

        [Fact]
        public void Exists_check_with_master_persist_info_no_MigrationHistory()
        {
            ExistsTest(DatabaseWithoutMigrationHistory, NormalUser, persistSecurityInfo: true);
        }

        [Fact]
        public void Exists_check_with_master_no_persist_info_no_MigrationHistory()
        {
            ExistsTest(DatabaseWithoutMigrationHistory, NormalUser, persistSecurityInfo: false);
        }

        [Fact]
        public void Exists_check_without_master_persist_info_no_MigrationHistory()
        {
            ExistsTestNoMaster(DatabaseWithoutMigrationHistory, NormalUser, persistSecurityInfo: true);
        }

        [Fact]
        public void Exists_check_without_master_no_persist_info_no_MigrationHistory()
        {
            ExistsTestNoMaster(DatabaseWithoutMigrationHistory, NormalUser, persistSecurityInfo: false);
        }

        [Fact]
        public void Exists_check_with_master_persist_info_out_of_date()
        {
            ExistsTest(DatabaseOutOfDate, NormalUser, persistSecurityInfo: true);
        }

        [Fact]
        public void Exists_check_with_master_no_persist_info_out_of_date()
        {
            ExistsTest(DatabaseOutOfDate, NormalUser, persistSecurityInfo: false);
        }

        [Fact]
        public void Exists_check_without_master_persist_info_out_of_date()
        {
            ExistsTestNoMaster(DatabaseOutOfDate, NormalUser, persistSecurityInfo: true);
        }

        [Fact]
        public void Exists_check_without_master_no_persist_info_out_of_date()
        {
            ExistsTestNoMaster(DatabaseOutOfDate, NormalUser, persistSecurityInfo: false);
        }

        [Fact]
        public void Not_exists_check_with_master_persist_info()
        {
            NotExistsTest(NormalUser, persistSecurityInfo: true);
        }

        [Fact]
        public void Not_exists_check_with_master_no_persist_info()
        {
            NotExistsTest(NormalUser, persistSecurityInfo: false);
        }

        [Fact]
        public void Not_exists_check_without_master_persist_info()
        {
            NotExistsTestNoMaster(NormalUser, persistSecurityInfo: true);
        }

        [Fact]
        public void Not_exists_check_without_master_no_persist_info()
        {
            NotExistsTestNoMaster(NormalUser, persistSecurityInfo: false);
        }

        [Fact] // CodePlex 2068
        public void Exists_check_with_master_persist_info_open_connection()
        {
            ExistsTestWithConnection(DatabaseWithMigrationHistory, NormalUser, persistSecurityInfo: true, openConnection: true);
        }

        [Fact] // CodePlex 2068
        public void Exists_check_with_master_no_persist_info_open_connection()
        {
            ExistsTestWithConnection(DatabaseWithMigrationHistory, NormalUser, persistSecurityInfo: false, openConnection: true);
        }

        [Fact] // CodePlex 2068
        public void Exists_check_without_master_persist_info_open_connection()
        {
            ExistsTestNoMasterWithConnection(DatabaseWithMigrationHistory, NormalUser, persistSecurityInfo: true, openConnection: true);
        }

        [Fact] // CodePlex 2068
        public void Exists_check_without_master_no_persist_info_open_connection()
        {
            ExistsTestNoMasterWithConnection(DatabaseWithMigrationHistory, NormalUser, persistSecurityInfo: false, openConnection: true);
        }

        [Fact] // CodePlex 2113, 2068
        public void Exists_check_with_no_master_query_persist_info_open_connection()
        {
            ExistsTestWithConnection(DatabaseWithMigrationHistory, ImpairedUser, persistSecurityInfo: true, openConnection: true);
        }

        [Fact] // CodePlex 2113, 2068
        public void Exists_check_with_no_master_query_no_persist_info_open_connection()
        {
            ExistsTestWithConnection(DatabaseWithMigrationHistory, ImpairedUser, persistSecurityInfo: false, openConnection: true);
        }

        [Fact]
        public void Exists_check_with_master_persist_info_closed_connection()
        {
            ExistsTestWithConnection(DatabaseWithMigrationHistory, NormalUser, persistSecurityInfo: true, openConnection: false);
        }

        [Fact]
        public void Exists_check_with_master_no_persist_info_closed_connection()
        {
            ExistsTestWithConnection(DatabaseWithMigrationHistory, NormalUser, persistSecurityInfo: false, openConnection: false);
        }

        [Fact]
        public void Exists_check_without_master_persist_info_closed_connection()
        {
            ExistsTestNoMasterWithConnection(DatabaseWithMigrationHistory, NormalUser, persistSecurityInfo: true, openConnection: false);
        }

        [Fact]
        public void Exists_check_without_master_no_persist_info_closed_connection()
        {
            ExistsTestNoMasterWithConnection(DatabaseWithMigrationHistory, NormalUser, persistSecurityInfo: false, openConnection: false);
        }

        [Fact] // CodePlex 2068
        public void Exists_check_with_master_persist_info_open_connection_no_MigrationHistory()
        {
            ExistsTestWithConnection(DatabaseWithoutMigrationHistory, NormalUser, persistSecurityInfo: true, openConnection: true);
        }

        [Fact] // CodePlex 2068
        public void Exists_check_with_master_no_persist_info_open_connection_no_MigrationHistory()
        {
            ExistsTestWithConnection(DatabaseWithoutMigrationHistory, NormalUser, persistSecurityInfo: false, openConnection: true);
        }

        [Fact] // CodePlex 2068
        public void Exists_check_without_master_persist_info_open_connection_no_MigrationHistory()
        {
            ExistsTestNoMasterWithConnection(DatabaseWithoutMigrationHistory, NormalUser, persistSecurityInfo: true, openConnection: true);
        }

        [Fact] // CodePlex 2068
        public void Exists_check_without_master_no_persist_info_open_connection_no_MigrationHistory()
        {
            ExistsTestNoMasterWithConnection(DatabaseWithoutMigrationHistory, NormalUser, persistSecurityInfo: false, openConnection: true);
        }

        [Fact] // CodePlex 2113, 2068
        public void Exists_check_with_no_master_query_persist_info_open_connection_no_MigrationHistory()
        {
            ExistsTestWithConnection(DatabaseWithoutMigrationHistory, ImpairedUser, persistSecurityInfo: true, openConnection: true);
        }

        [Fact] // CodePlex 2113, 2068
        public void Exists_check_with_no_master_query_no_persist_info_open_connection_no_MigrationHistory()
        {
            ExistsTestWithConnection(DatabaseWithoutMigrationHistory, ImpairedUser, persistSecurityInfo: false, openConnection: true);
        }

        [Fact]
        public void Exists_check_with_master_persist_info_closed_connection_no_MigrationHistory()
        {
            ExistsTestWithConnection(DatabaseWithoutMigrationHistory, NormalUser, persistSecurityInfo: true, openConnection: false);
        }

        [Fact]
        public void Exists_check_with_master_no_persist_info_closed_connection_no_MigrationHistory()
        {
            ExistsTestWithConnection(DatabaseWithoutMigrationHistory, NormalUser, persistSecurityInfo: false, openConnection: false);
        }

        [Fact]
        public void Exists_check_without_master_persist_info_closed_connection_no_MigrationHistory()
        {
            ExistsTestNoMasterWithConnection(DatabaseWithoutMigrationHistory, NormalUser, persistSecurityInfo: true, openConnection: false);
        }

        [Fact]
        public void Exists_check_without_master_no_persist_info_closed_connection_no_MigrationHistory()
        {
            ExistsTestNoMasterWithConnection(DatabaseWithoutMigrationHistory, NormalUser, persistSecurityInfo: false, openConnection: false);
        }

        [Fact] // CodePlex 2068
        public void Exists_check_with_master_persist_info_open_connection_out_of_date()
        {
            ExistsTestWithConnection(DatabaseOutOfDate, NormalUser, persistSecurityInfo: true, openConnection: true);
        }

        [Fact] // CodePlex 2068
        public void Exists_check_with_master_no_persist_info_open_connection_out_of_date()
        {
            ExistsTestWithConnection(DatabaseOutOfDate, NormalUser, persistSecurityInfo: false, openConnection: true);
        }

        [Fact] // CodePlex 2068
        public void Exists_check_without_master_persist_info_open_connection_out_of_date()
        {
            ExistsTestNoMasterWithConnection(DatabaseOutOfDate, NormalUser, persistSecurityInfo: true, openConnection: true);
        }

        [Fact] // CodePlex 2068
        public void Exists_check_without_master_no_persist_info_open_connection_out_of_date()
        {
            ExistsTestNoMasterWithConnection(DatabaseOutOfDate, NormalUser, persistSecurityInfo: false, openConnection: true);
        }

        [Fact] // CodePlex 2113, 2068
        public void Exists_check_with_no_master_query_persist_info_open_connection_out_of_date()
        {
            ExistsTestWithConnection(DatabaseOutOfDate, ImpairedUser, persistSecurityInfo: true, openConnection: true);
        }

        [Fact] // CodePlex 2113, 2068
        public void Exists_check_with_no_master_query_no_persist_info_open_connection_out_of_date()
        {
            ExistsTestWithConnection(DatabaseOutOfDate, ImpairedUser, persistSecurityInfo: false, openConnection: true);
        }

        [Fact]
        public void Exists_check_with_master_persist_info_closed_connection_out_of_date()
        {
            ExistsTestWithConnection(DatabaseOutOfDate, NormalUser, persistSecurityInfo: true, openConnection: false);
        }

        [Fact]
        public void Exists_check_with_master_no_persist_info_closed_connection_out_of_date()
        {
            ExistsTestWithConnection(DatabaseOutOfDate, NormalUser, persistSecurityInfo: false, openConnection: false);
        }

        [Fact]
        public void Exists_check_without_master_persist_info_closed_connection_out_of_date()
        {
            ExistsTestNoMasterWithConnection(DatabaseOutOfDate, NormalUser, persistSecurityInfo: true, openConnection: false);
        }

        [Fact]
        public void Exists_check_without_master_no_persist_info_closed_connection_out_of_date()
        {
            ExistsTestNoMasterWithConnection(DatabaseOutOfDate, NormalUser, persistSecurityInfo: false, openConnection: false);
        }

        [Fact]
        public void Not_exists_check_with_master_persist_info_closed_connection()
        {
            NotExistsTestWithConnection(NormalUser, persistSecurityInfo: true, openConnection: false);
        }

        [Fact]
        public void Not_exists_check_with_master_no_persist_info_closed_connection()
        {
            NotExistsTestWithConnection(NormalUser, persistSecurityInfo: false, openConnection: false);
        }

        [Fact]
        public void Not_exists_check_without_master_persist_info_closed_connection()
        {
            NotExistsTestNoMasterWithConnection(NormalUser, persistSecurityInfo: true, openConnection: false);
        }

        [Fact]
        public void Not_exists_check_without_master_no_persist_info_closed_connection()
        {
            NotExistsTestNoMasterWithConnection(NormalUser, persistSecurityInfo: false, openConnection: false);
        }

        [Fact] // CodePlex 2113
        public void Not_exists_check_with_no_master_query_persist_info_closed_connection()
        {
            NotExistsTestWithConnection(ImpairedUser, persistSecurityInfo: true, openConnection: false);
        }

        [Fact] // CodePlex 2113
        public void Not_exists_check_with_no_master_query_no_persist_info_closed_connection()
        {
            NotExistsTestWithConnection(ImpairedUser, persistSecurityInfo: false, openConnection: false);
        }

        private void ExistsTest(string databaseName, string username, bool persistSecurityInfo)
        {
            AssertExists(
                databaseName,
                ModelHelpers.SimpleConnectionStringWithCredentials(
                    databaseName, username, Password, persistSecurityInfo));
        }

        private void ExistsTestNoMaster(string databaseName, string username, bool persistSecurityInfo)
        {
            var interceptor = new NoMasterInterceptor();
            try
            {
                DbInterception.Add(interceptor);

                AssertExists(
                    databaseName,
                    ModelHelpers.SimpleConnectionStringWithCredentials(
                        databaseName, username, Password, persistSecurityInfo));
            }
            finally
            {
                DbInterception.Remove(interceptor);
            }
        }

        private void NotExistsTest(string username, bool persistSecurityInfo)
        {
            AssertDoesNotExist(
                ModelHelpers.SimpleConnectionStringWithCredentials(
                    "IDoNotExist", username, Password, persistSecurityInfo));
        }

        private void NotExistsTestNoMaster(string username, bool persistSecurityInfo)
        {
            var interceptor = new NoMasterInterceptor();
            try
            {
                DbInterception.Add(interceptor);

                AssertDoesNotExist(
                    ModelHelpers.SimpleConnectionStringWithCredentials(
                        "IDoNotExist", username, Password, persistSecurityInfo));
            }
            finally
            {
                DbInterception.Remove(interceptor);
            }
        }

        private void ExistsTestWithConnection(string databaseName, string username, bool persistSecurityInfo, bool openConnection)
        {
            AssertExistsWithConnection(
                databaseName,
                ModelHelpers.SimpleConnectionStringWithCredentials(
                    databaseName, username, Password, persistSecurityInfo), openConnection);
        }

        private void ExistsTestNoMasterWithConnection(string databaseName, string username, bool persistSecurityInfo, bool openConnection)
        {
            var interceptor = new NoMasterInterceptor();
            try
            {
                DbInterception.Add(interceptor);

                AssertExistsWithConnection(
                    databaseName,
                    ModelHelpers.SimpleConnectionStringWithCredentials(
                        databaseName, username, Password, persistSecurityInfo), openConnection);
            }
            finally
            {
                DbInterception.Remove(interceptor);
            }
        }

        private void NotExistsTestWithConnection(string username, bool persistSecurityInfo, bool openConnection)
        {
            AssertDoesNotExistWithConnection(
                ModelHelpers.SimpleConnectionStringWithCredentials(
                    "IDoNotExist", username, Password, persistSecurityInfo), openConnection);
        }

        private void NotExistsTestNoMasterWithConnection(string username, bool persistSecurityInfo, bool openConnection)
        {
            var interceptor = new NoMasterInterceptor();
            try
            {
                DbInterception.Add(interceptor);

                AssertDoesNotExistWithConnection(
                    ModelHelpers.SimpleConnectionStringWithCredentials(
                        "IDoNotExist", username, Password, persistSecurityInfo), openConnection);
            }
            finally
            {
                DbInterception.Remove(interceptor);
            }
        }

        private static void AssertExists(string databaseName, string connectionString)
        {
            using (var context = ExistsContext.Create(connectionString))
            {
                AssertExists(databaseName, context);
            }
        }

        private static void AssertDoesNotExist(string connectionString)
        {
            using (var context = ExistsContext.Create(connectionString))
            {
                context.Database.Initialize(force: false);

                Assert.True(context.InitializerCalled, "Expected initializer to be called");
                Assert.False(context.Exists, "Expected context to not exist"); ;
            }
        }

        private static void AssertExistsWithConnection(string databaseName, string connectionString, bool openConnection)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                if (openConnection)
                {
                    connection.Open();
                }

                using (var context = ExistsContext.Create(connection))
                {
                    AssertExists(databaseName, context);
                }

                connection.Close();
            }
        }

        private static void AssertExists(string databaseName, ExistsContext context)
        {
            context.Database.Initialize(force: false);

            Assert.True(context.InitializerCalled, "Expected initializer to be called");
            Assert.True(context.Exists, "Expected context to exist");

            if (databaseName == DatabaseWithMigrationHistory)
            {
                context.SetDropCreateIfNotExists();
                context.Database.Initialize(force: true);
                context.Database.Initialize(force: true);

                context.SetDropCreateIfModelChanges();
                context.Database.Initialize(force: true);
                context.Database.Initialize(force: true);
            }
            else if (databaseName == DatabaseWithoutMigrationHistory)
            {
                context.SetDropCreateIfNotExists();
                context.Database.Initialize(force: true);
                context.Database.Initialize(force: true);

                context.SetDropCreateIfModelChanges();
                Assert.Throws<NotSupportedException>(() => context.Database.Initialize(force: true))
                    .ValidateMessage("Database_NoDatabaseMetadata");
            }
            else if (databaseName == DatabaseOutOfDate)
            {
                context.SetDropCreateIfNotExists();
                Assert.Throws<InvalidOperationException>(() => context.Database.Initialize(force: true))
                    .ValidateMessage("DatabaseInitializationStrategy_ModelMismatch", context.GetType().Name);
            }
        }

        private static void AssertDoesNotExistWithConnection(string connectionString, bool openConnection)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                if (openConnection)
                {
                    connection.Open();
                }

                using (var context = ExistsContext.Create(connection))
                {
                    context.Database.Initialize(force: false);

                    Assert.True(context.InitializerCalled, "Expected initializer to be called");
                    Assert.False(context.Exists, "Expected context to not exist");
                }

                connection.Close();
            }
        }

        private static void EnsureDatabaseExists(string databaseName, bool drophistoryTable, bool outOfDate)
        {
            using (var context = outOfDate
                ? new ExistsContextModelChanged(SimpleConnectionString(databaseName))
                : new ExistsContext(SimpleConnectionString(databaseName)))
            {
                if (!context.Database.Exists())
                {
                    context.Database.Create();

                    if (drophistoryTable)
                    {
                        context.Database.ExecuteSqlCommand("DROP TABLE " + HistoryContext.DefaultTableName);
                    }
                    else
                    {
                        context.Database.ExecuteSqlCommand(@"UPDATE __MigrationHistory SET ContextKey = 'TestContextKey'");
                    }
                }
            }
        }

        private void EnsureUserExists(string databaseName, string username, bool allowMasterQuery)
        {
            using (var connection = new SqlConnection(SimpleConnectionString("master")))
            {
                connection.Open();

                var loginExists = ExecuteScalarReturnsOne(
                    connection,
                    "SELECT COUNT(*) FROM sys.sql_logins WHERE name = N'{0}'", username);

                if (!loginExists)
                {
                    ExecuteNonQuery(connection, "CREATE LOGIN [{0}] WITH PASSWORD=N'{1}'", username, Password);
                }

                var userExists = ExecuteScalarReturnsOne(
                    connection,
                    "SELECT COUNT(*) FROM sys.sysusers WHERE name = N'{0}'", username);

                if (!userExists)
                {
                    ExecuteNonQuery(connection, "CREATE USER [{0}] FROM LOGIN [{0}]", username);
                    if (!allowMasterQuery)
                    {
                        ExecuteNonQuery(connection, "DENY VIEW ANY DATABASE TO [{0}]", username);
                    }
                }

                connection.Close();
            }

            using (var connection = new SqlConnection(SimpleConnectionString(databaseName)))
            {
                connection.Open();

                var userExists = ExecuteScalarReturnsOne(
                    connection,
                    "SELECT COUNT(*) FROM sys.sysusers WHERE name = N'{0}'", username);

                if (!userExists)
                {
                    ExecuteNonQuery(connection, "CREATE USER [{0}] FROM LOGIN [{0}]", username);
                    ExecuteNonQuery(connection, "GRANT VIEW DEFINITION TO [{0}]", username);
                    ExecuteNonQuery(connection, "GRANT SELECT TO [{0}]", username);
                }

                connection.Close();
            }
        }

        private static void ExecuteNonQuery(SqlConnection connection, string commandText, params object[] args)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = string.Format(commandText, args);
                command.ExecuteNonQuery();
            }
        }

        private static bool ExecuteScalarReturnsOne(SqlConnection connection, string commandText, params object[] args)
        {
            using (var command = connection.CreateCommand())
            {
                try
                {
                    command.CommandText = string.Format(commandText, args);
                    return (int)command.ExecuteScalar() == 1;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public class NoMasterInterceptor : IDbConnectionInterceptor
        {
            public void BeginningTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
            {
            }

            public void BeganTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
            {
            }

            public void Closing(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
            {
            }

            public void Closed(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
            {
            }

            public void ConnectionStringGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void ConnectionStringGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void ConnectionStringSetting(
                DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext)
            {
            }

            public void ConnectionStringSet(DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext)
            {
            }

            public void ConnectionTimeoutGetting(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext)
            {
            }

            public void ConnectionTimeoutGot(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext)
            {
            }

            public void DatabaseGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void DatabaseGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void DataSourceGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void DataSourceGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void Disposing(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
            {
            }

            public void Disposed(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
            {
            }

            public void EnlistingTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext)
            {
            }

            public void EnlistedTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext)
            {
            }

            public void Opening(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
            {
                if (connection.Database == "master")
                {
                    interceptionContext.Exception =
                        (SqlException)Activator.CreateInstance(
                            typeof(SqlException), BindingFlags.Instance | BindingFlags.NonPublic, null,
                            new object[] { "No master for you!", null, null, Guid.NewGuid() }, null);
                }
            }

            public void Opened(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
            {
            }

            public void ServerVersionGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void ServerVersionGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void StateGetting(DbConnection connection, DbConnectionInterceptionContext<ConnectionState> interceptionContext)
            {
            }

            public void StateGot(DbConnection connection, DbConnectionInterceptionContext<ConnectionState> interceptionContext)
            {
            }
        }

        public class ExistsContext : DbContext
        {
            public bool InitializerCalled { get; set; }
            public bool Exists { get; set; }

            private static int _typeCount;

            static ExistsContext()
            {
                Database.SetInitializer<ExistsContext>(null);
            }

            public ExistsContext(string connectionString)
                : base(connectionString)
            {
                SetContextKey();
            }

            public ExistsContext(DbConnection connection)
                : base(connection, contextOwnsConnection: false)
            {
                SetContextKey();
            }

            private void SetContextKey()
            {
                var internalContext = typeof(DbContext)
                    .GetField("_internalContext", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(this);

                internalContext.GetType().BaseType
                    .GetField("_defaultContextKey", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(internalContext, "TestContextKey");
            }

            public DbSet<ExistsEntity> Entities { get; set; }

            public static ExistsContext Create(string connectionString)
            {
                return (ExistsContext)Activator.CreateInstance(GetNewContxtType(), connectionString);
            }

            public static ExistsContext Create(DbConnection connection)
            {
                return (ExistsContext)Activator.CreateInstance(GetNewContxtType(), connection);
            }

            private static Type GetNewContxtType()
            {
                var typeNumber = _typeCount++;

                var typeBits = new Type[8];
                for (var bit = 0; bit < 8; bit++)
                {
                    typeBits[bit] = ((typeNumber & 1) == 1) ? typeof(int) : typeof(string);
                    typeNumber >>= 1;
                }

                return typeof(ExistsContext<>).MakeGenericType(typeof(Tuple<,,,,,,,>).MakeGenericType(typeBits));
            }

            public virtual void SetDropCreateIfNotExists()
            {
                throw new NotImplementedException();
            }

            public virtual void SetDropCreateIfModelChanges()
            {
                throw new NotImplementedException();
            }
        }

        public class ExistsContextModelChanged : ExistsContext
        {
            static ExistsContextModelChanged()
            {
                Database.SetInitializer<ExistsContextModelChanged>(null);
            }

            public ExistsContextModelChanged(string connectionString)
                : base(connectionString)
            {
            }

            public ExistsContextModelChanged(DbConnection connection)
                : base(connection)
            {
            }

            public DbSet<ModelChangedEntity> ModelChangedEntities { get; set; }
        }

        public class ExistsContext<T> : ExistsContext
        {
            private static readonly ExistsInitializer<T> _initializer = new ExistsInitializer<T>();

            private static readonly CreateDatabaseIfNotExists<ExistsContext<T>> _dropCreateIfNotExists
                = new CreateDatabaseIfNotExists<ExistsContext<T>>();

            private static readonly DropCreateDatabaseIfModelChanges<ExistsContext<T>> _dropCreateIfModelChanges
                = new DropCreateDatabaseIfModelChanges<ExistsContext<T>>();

            private static readonly DropCreateDatabaseAlways<ExistsContext<T>> _dropCreateAlways
                = new DropCreateDatabaseAlways<ExistsContext<T>>();

            static ExistsContext()
            {
                Database.SetInitializer(_initializer);
            }

            public ExistsContext(string connectionString)
                : base(connectionString)
            {
            }

            public ExistsContext(DbConnection connection)
                : base(connection)
            {
            }

            public override void SetDropCreateIfNotExists()
            {
                Database.SetInitializer(_dropCreateIfNotExists);
            }

            public override void SetDropCreateIfModelChanges()
            {
                Database.SetInitializer(_dropCreateIfModelChanges);
            }
        }

        public class BasicManifestTokenResolver : IManifestTokenResolver
        {
            public string ResolveManifestToken(DbConnection connection)
            {
                return DbProviderServices.GetProviderServices(connection).GetProviderManifestToken(connection);
            }
        }

        public class ExistsInitializer<T> : IDatabaseInitializer<ExistsContext<T>>
        {
            public void InitializeDatabase(ExistsContext<T> context)
            {
                context.InitializerCalled = true;
                context.Exists = context.Database.Exists();
            }
        }

        public class ExistsEntity
        {
            public int Id { get; set; }
        }

        public class ModelChangedEntity
        {
            public int Id { get; set; }
        }
    }
}
