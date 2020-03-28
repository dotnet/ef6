// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.SqlClient;
    using System.Reflection;
    using Xunit;

    public class DatabaseExistsTests : FunctionalTestBase, IDisposable
    {
        private const string Password = "Password1";
        private const string NormalUser = "EFUserWithDbVisibility";
        private const string ImpairedUser = "EFUserWithoutDbVisibility";
        private const string DatabaseName = "ItsAMoose";

        public DatabaseExistsTests()
        {
            EnsureDatabaseExists();
            EnsureUserExists(NormalUser, allowMasterQuery: true);
            EnsureUserExists(ImpairedUser, allowMasterQuery: false);

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
            ExistsTest(NormalUser, persistSecurityInfo: true);
        }

        [Fact]
        public void Exists_check_with_master_no_persist_info()
        {
            ExistsTest(NormalUser, persistSecurityInfo: false);
        }

        [Fact]
        public void Exists_check_without_master_persist_info()
        {
            ExistsTestNoMaster(NormalUser, persistSecurityInfo: true);
        }

        [Fact]
        public void Exists_check_without_master_no_persist_info()
        {
            ExistsTestNoMaster(NormalUser, persistSecurityInfo: false);
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

        [Fact] // CodePlex 2113
        public void Not_exists_check_with_no_master_query_persist_info()
        {
            NotExistsTest(ImpairedUser, persistSecurityInfo: true);
        }

        [Fact] // CodePlex 2113
        public void Not_exists_check_with_no_master_query_no_persist_info()
        {
            NotExistsTest(ImpairedUser, persistSecurityInfo: false);
        }

        [Fact] // CodePlex 2068
        public void Exists_check_with_master_persist_info_open_connection()
        {
            ExistsTestWithConnection(NormalUser, persistSecurityInfo: true, openConnection: true);
        }

        [Fact] // CodePlex 2068
        public void Exists_check_with_master_no_persist_info_open_connection()
        {
            ExistsTestWithConnection(NormalUser, persistSecurityInfo: false, openConnection: true);
        }

        [Fact] // CodePlex 2068
        public void Exists_check_without_master_persist_info_open_connection()
        {
            ExistsTestNoMasterWithConnection(NormalUser, persistSecurityInfo: true, openConnection: true);
        }

        [Fact] // CodePlex 2068
        public void Exists_check_without_master_no_persist_info_open_connection()
        {
            ExistsTestNoMasterWithConnection(NormalUser, persistSecurityInfo: false, openConnection: true);
        }

        [Fact] // CodePlex 2113, 2068
        public void Exists_check_with_no_master_query_persist_info_open_connection()
        {
            ExistsTestWithConnection(ImpairedUser, persistSecurityInfo: true, openConnection: true);
        }

        [Fact] // CodePlex 2113, 2068
        public void Exists_check_with_no_master_query_no_persist_info_open_connection()
        {
            ExistsTestWithConnection(ImpairedUser, persistSecurityInfo: false, openConnection: true);
        }

        [Fact]
        public void Exists_check_with_master_persist_info_closed_connection()
        {
            ExistsTestWithConnection(NormalUser, persistSecurityInfo: true, openConnection: false);
        }

        [Fact]
        public void Exists_check_with_master_no_persist_info_closed_connection()
        {
            ExistsTestWithConnection(NormalUser, persistSecurityInfo: false, openConnection: false);
        }

        [Fact]
        public void Exists_check_without_master_persist_info_closed_connection()
        {
            ExistsTestNoMasterWithConnection(NormalUser, persistSecurityInfo: true, openConnection: false);
        }

        [Fact]
        public void Exists_check_without_master_no_persist_info_closed_connection()
        {
            ExistsTestNoMasterWithConnection(NormalUser, persistSecurityInfo: false, openConnection: false);
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

        private void ExistsTest(string username, bool persistSecurityInfo)
        {
            AssertExists(
                ModelHelpers.SimpleConnectionStringWithCredentials(
                    DatabaseName, username, Password, persistSecurityInfo));
        }

        private void ExistsTestNoMaster(string username, bool persistSecurityInfo)
        {
            var interceptor = new NoMasterInterceptor();
            try
            {
                DbInterception.Add(interceptor);

                AssertExists(
                    ModelHelpers.SimpleConnectionStringWithCredentials(
                        DatabaseName, username, Password, persistSecurityInfo));
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

        private void ExistsTestWithConnection(string username, bool persistSecurityInfo, bool openConnection)
        {
            AssertExistsWithConnection(
                ModelHelpers.SimpleConnectionStringWithCredentials(
                    DatabaseName, username, Password, persistSecurityInfo), openConnection);
        }

        private void ExistsTestNoMasterWithConnection(string username, bool persistSecurityInfo, bool openConnection)
        {
            var interceptor = new NoMasterInterceptor();
            try
            {
                DbInterception.Add(interceptor);

                AssertExistsWithConnection(
                    ModelHelpers.SimpleConnectionStringWithCredentials(
                        DatabaseName, username, Password, persistSecurityInfo), openConnection);
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

        private static void AssertExists(string connectionString)
        {
            using (var context = ExistsContext.Create(connectionString))
            {
                Assert.True(context.Database.Exists());
                Assert.True(context.Database.Exists());
            }
        }

        private static void AssertDoesNotExist(string connectionString)
        {
            using (var context = ExistsContext.Create(connectionString))
            {
                Assert.False(context.Database.Exists());
                Assert.False(context.Database.Exists());
            }
        }

        private static void AssertExistsWithConnection(string connectionString, bool openConnection)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                if (openConnection)
                {
                    connection.Open();
                }

                using (var context = ExistsContext.Create(connection))
                {
                    Assert.True(context.Database.Exists());
                    Assert.True(context.Database.Exists());
                }

                connection.Close();
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
                    Assert.False(context.Database.Exists());
                    Assert.False(context.Database.Exists());
                }

                connection.Close();
            }
        }

        private static void EnsureDatabaseExists()
        {
            using (var connection = new SqlConnection(SimpleConnectionString("master")))
            {
                connection.Open();

                if (!ExecuteScalarReturnsOne(connection, "SELECT Count(*) FROM sys.databases WHERE [name]=N'{0}'", DatabaseName))
                {
                    ExecuteNonQuery(connection, "CREATE DATABASE [{0}]", DatabaseName);
                }
                connection.Close();
            }
        }

        private void EnsureUserExists(string username, bool allowMasterQuery)
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

            using (var connection = new SqlConnection(SimpleConnectionString(DatabaseName)))
            {
                connection.Open();

                var userExists = ExecuteScalarReturnsOne(
                    connection,
                    "SELECT COUNT(*) FROM sys.sysusers WHERE name = N'{0}'", username);

                if (!userExists)
                {
                    ExecuteNonQuery(connection, "CREATE USER [{0}] FROM LOGIN [{0}]", username);
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

        public abstract class ExistsContext : DbContext
        {
            private static int _typeCount;

            static ExistsContext()
            {
                Database.SetInitializer<ExistsContext>(null);
            }

            protected ExistsContext(string connectionString)
                : base(connectionString)
            {
            }

            protected ExistsContext(DbConnection connection)
                : base(connection, contextOwnsConnection: false)
            {
            }

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
        }

        public class ExistsContext<T> : ExistsContext
        {
            static ExistsContext()
            {
                Database.SetInitializer<ExistsContext<T>>(null);
            }

            public ExistsContext(string connectionString)
                : base(connectionString)
            {
            }

            public ExistsContext(DbConnection connection)
                : base(connection)
            {
            }
        }

        public class BasicManifestTokenResolver : IManifestTokenResolver
        {
            public string ResolveManifestToken(DbConnection connection)
            {
                return DbProviderServices.GetProviderServices(connection).GetProviderManifestToken(connection);
            }
        }
    }
}
