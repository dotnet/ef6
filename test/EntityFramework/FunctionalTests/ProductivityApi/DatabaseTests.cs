// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.TestHelpers;
    using System.Data.SqlClient;
    using System.IO;
    using SimpleModel;
    using Xunit;
    using System.Globalization;

    /// <summary>
    /// Functional tests for Database.  Unit tests also exist in the unit tests project.
    /// </summary>
    public class DatabaseTests : FunctionalTestBase
    {
        #region Infrastructure/setup

        public DatabaseTests()
        {
            using (var context = new SimpleModelContext())
            {
                context.Database.Initialize(force: false);
            }

            using (var context = new SimpleModelContextWithNoData())
            {
                context.Database.Initialize(force: false);
            }

            using (var connection = new SqlConnection(SimpleConnectionString("master")))
            {
                connection.Open();

                if (DatabaseTestHelpers.IsSqlAzure(connection.ConnectionString))
                {
                    CreateLoginForSqlAzure(connection);
                }
                else
                {
                    CreateLoginForSqlServer(connection);
                }
            }
        }

        private void CreateLoginForSqlServer(SqlConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText
                    = string.Format(
@"IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'EFTestSimpleModelUser')
BEGIN
  CREATE LOGIN [EFTestSimpleModelUser] WITH PASSWORD=N'Password1', DEFAULT_DATABASE=[{0}],  CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
  DENY VIEW ANY DATABASE TO [EFTestSimpleModelUser]
  CREATE USER [EFTestSimpleModelUser] FOR LOGIN [EFTestSimpleModelUser]
  DENY SELECT TO [EFTestSimpleModelUser]
  GRANT CREATE DATABASE TO [EFTestSimpleModelUser]
END", DefaultDbName<SimpleModelContext>());
                command.ExecuteNonQuery();
            }
        }

        private void CreateLoginForSqlAzure(SqlConnection connection)
        {
            bool loginExists = false;
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM sys.sql_logins WHERE name = N'EFTestSimpleModelUser'";
                loginExists = (int)command.ExecuteScalar() == 1;
            }

            if (!loginExists)
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "CREATE LOGIN [EFTestSimpleModelUser] WITH PASSWORD=N'Password1'";
                    command.ExecuteNonQuery();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
@"BEGIN
  CREATE USER [EFTestSimpleModelUser] FOR LOGIN [EFTestSimpleModelUser]
  DENY SELECT TO [EFTestSimpleModelUser]
  GRANT CREATE DATABASE TO [EFTestSimpleModelUser]
END";
                    command.ExecuteNonQuery();
                }
            }            
        }

        #endregion

        #region Attachable database infrastructure

        private class AttachedContext : SimpleModelContext
        {
            public AttachedContext(string connectionString)
                : base(connectionString)
            {
            }
        }

        private void AttachableDatabaseTest(Action<AttachedContext> testMethod, bool useInitialCatalog = true)
        {
            using (var context = new AttachedContext(SimpleAttachConnectionString<AttachedContext>(useInitialCatalog)))
            {
                // SQL Azure and LocalDB do not support attaching databases
                var connectionString = context.Database.Connection.ConnectionString;
                if (DatabaseTestHelpers.IsSqlAzure(connectionString) || DatabaseTestHelpers.IsLocalDb(connectionString))
                {
                    return;
                }

                try
                {
                    // Execute actual test
                    testMethod(context);
                }
                finally
                {
                    // Ensure database is deleted/detached
                    context.Database.Delete();
                }
            }
        }

        #endregion

        #region Positive Exists tests

        [Fact]
        public void DatabaseExists_returns_true_for_existing_database_when_using_Database_obtained_from_context()
        {
            using (var context = new SimpleModelContext())
            {
                // Note: Database gets created as part of TestInit
                Assert.True(context.Database.Exists());
            }
        }

        [Fact]
        public void DatabaseExists_returns_true_for_existing_attached_database_with_InitialCatalog_when_using_Database_obtained_from_context()
        {
            DatabaseExists_returns_true_for_existing_attached_database_when_using_Database_obtained_from_context(true);
        }

        [Fact]
        public void DatabaseExists_returns_true_for_existing_attached_database_without_InitialCatalog_when_using_Database_obtained_from_context()
        {
            DatabaseExists_returns_true_for_existing_attached_database_when_using_Database_obtained_from_context(false);
        }

        private void DatabaseExists_returns_true_for_existing_attached_database_when_using_Database_obtained_from_context(bool useInitialCatalog)
        {
            AttachableDatabaseTest(
                (context) =>
                    {
                        // See CodePlex 1554 - Handle User Instance flakiness
                        MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(new ExecutionStrategyResolver<IDbExecutionStrategy>(
                            SqlProviderServices.ProviderInvariantName, null, () => new SqlAzureExecutionStrategy()));
                        try
                        {
                            // Ensure database is initialized
                            context.Database.Initialize(force: true);

                            Assert.True(context.Database.Exists());
                        }
                        finally
                        {
                            MutableResolver.ClearResolvers();
                        }
                    }, useInitialCatalog);
        }

        [Fact]
        public void DatabaseExists_returns_true_for_existing_database_when_using_static_method_taking_database_name()
        {
            Assert.True(Database.Exists(DefaultDbName<SimpleModelContext>()));
        }

        [Fact]
        public void DatabaseExists_returns_true_for_existing_database_when_using_static_method_taking_connection_string()
        {
            Assert.True(Database.Exists(SimpleConnectionString<SimpleModelContext>()));
        }

        [Fact]
        public void
            DatabaseExists_returns_true_for_existing_database_when_using_static_method_taking_named_connection_string_that_exists_Config_file
            ()
        {
            DatabaseExists_returns_true_for_existing_database_when_using_static_method_taking_named_connection_string(
                "SimpleModelInAppConfig");
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void
            DatabaseExists_returns_true_for_existing_database_when_using_static_method_taking_named_connection_string_where_the_last_token_exists_in_Config_file
            ()
        {
            DatabaseExists_returns_true_for_existing_database_when_using_static_method_taking_named_connection_string(
                "SomeRandom.String.X.Y.Z.SimpleModelInAppConfig");
        }

        private void
            DatabaseExists_returns_true_for_existing_database_when_using_static_method_taking_named_connection_string(
            string namedConnectionString)
        {
            // Arrange
            Database.Delete(namedConnectionString);
            using (var context = new SimpleModelContext(namedConnectionString))
            {
                context.Database.Create();
            }

            // Act- Assert
            Assert.True(Database.Exists(namedConnectionString));

            // Clean up Act
            Database.Delete(namedConnectionString);
        }

        [Fact]
        public void DatabaseExists_returns_true_for_existing_database_when_using_static_method_taking_existing_connection_which_is_closed()
        {
            using (var connection = SimpleConnection<SimpleModelContext>())
            {
                Assert.True(connection.State == ConnectionState.Closed);
                Assert.True(Database.Exists(connection));
            }
        }

        [Fact]
        public void DatabaseExists_returns_true_for_existing_database_when_using_static_method_taking_existing_connection_which_is_opened()
        {
            using (var connection = SimpleConnection<SimpleModelContext>())
            {
                connection.Open();
                Assert.True(connection.State == ConnectionState.Open);
                var eventsTracker = new ConnectionEventsTracker(connection);
                Assert.True(Database.Exists(connection));
                eventsTracker.VerifyNoConnectionEventsWereFired();
            }
        }

        [Fact]
        public void DatabaseExists_returns_false_for_non_existing_database_when_using_Database_obtained_from_context()
        {
            using (var context = new EmptyContext())
            {
                DatabaseExists_returns_false_for_non_existing_database(() => context.Database.Exists());
            }
        }

        [Fact]
        public void DatabaseExists_returns_false_for_non_existing_database_when_using_static_method_taking_database_name()
        {
            DatabaseExists_returns_false_for_non_existing_database(() => Database.Exists(DefaultDbName<EmptyContext>()));
        }

        [Fact]
        public void DatabaseExists_returns_false_for_non_existing_database_when_using_static_method_taking_named_connection_string()
        {
            // Arrange
            Database.Delete("SimpleModelInAppConfig");
            Assert.False(Database.Exists("SimpleModelInAppConfig"));
            Assert.False(Database.Exists("NonExistant.NamespaceName.SimpleModelInAppConfig"));
        }

        [Fact]
        public void DatabaseExists_returns_false_for_non_existing_database_when_using_static_method_taking_connection_string()
        {
            DatabaseExists_returns_false_for_non_existing_database(
                () => Database.Exists(SimpleConnectionString<EmptyContext>()));
        }

        [Fact]
        public void
            DatabaseExists_returns_false_for_non_existing_database_when_using_static_method_taking_existing_connection_which_is_closed()
        {
            using (var connection = SimpleConnection<EmptyContext>())
            {
                Assert.True(connection.State == ConnectionState.Closed);
                var eventsTracker = new ConnectionEventsTracker(connection);
                DatabaseExists_returns_false_for_non_existing_database(() => Database.Exists(connection));
                eventsTracker.VerifyNoConnectionEventsWereFired();
            }
        }

        private void DatabaseExists_returns_false_for_non_existing_database(Func<bool> databaseExists)
        {
            Database.Delete(DefaultDbName<EmptyContext>());

            Assert.False(databaseExists());
        }

        public class NoMasterPermissionContext : SimpleModelContext
        {
            public NoMasterPermissionContext(string nameOrConnectionString)
                : base(nameOrConnectionString)
            {
            }
        }

        [Fact]
        public void DatabaseExists_returns_true_for_existing_database_when_no_master_permissions()
        {
            using (var context = new NoMasterPermissionContext(SimpleConnectionString<NoMasterPermissionContext>()))
            {
                context.Database.Delete();
                context.Database.Initialize(force: false);
            }

            using (var connection = new SqlConnection(SimpleConnectionString<NoMasterPermissionContext>()))
            {
                connection.Open();
                if (DatabaseTestHelpers.IsSqlAzure(connection.ConnectionString))
                {
                    // Scenario not supported on SqlAzure, need to be connected to master
                    // in order to view existing users
                    return;
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = string.Format(
@"IF NOT EXISTS (SELECT * FROM sys.sysusers WHERE name= N'EFTestSimpleModelUser')
BEGIN
  CREATE USER [EFTestSimpleModelUser] FOR LOGIN [EFTestSimpleModelUser]
END");
                    command.ExecuteNonQuery();
                }
            }

            var connectionString
                = SimpleConnectionStringWithCredentials<NoMasterPermissionContext>(
                    "EFTestSimpleModelUser",
                    "Password1");

            using (var context = new NoMasterPermissionContext(connectionString))
            {
                // Note: Database gets created as part of TestInit
                Assert.True(context.Database.Exists());
            }
        }

        [Fact]
        public void DatabaseExists_returns_true_for_existing_database_when_no_master_nor_database_permissions()
        {
            using (var context = new NoMasterPermissionContext(SimpleConnectionString<NoMasterPermissionContext>()))
            {
                context.Database.Delete();
                context.Database.Initialize(force: false);
            }

            using (var connection = new SqlConnection(SimpleConnectionString<NoMasterPermissionContext>()))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    // Double-check there's no user for this login
                    command.CommandText
                        = string.Format(
                            @"IF EXISTS (SELECT * FROM sys.sysusers WHERE name= N'EFTestSimpleModelUser')
                              BEGIN
                                DROP USER [EFTestSimpleModelUser]
                              END");
                    command.ExecuteNonQuery();
                }
            }

            var connectionString
                = SimpleConnectionStringWithCredentials<NoMasterPermissionContext>(
                    "EFTestSimpleModelUser",
                    "Password1");

            using (var context = new NoMasterPermissionContext(connectionString))
            {
                if (DatabaseTestHelpers.IsSqlAzure(connectionString))
                {
                    Assert.False(context.Database.Exists());
                }
                else
                {
                    Assert.True(context.Database.Exists());
                }
            }
        }

        [Fact]
        public void DatabaseExists_returns_false_for_non_existing_database_when_no_master_permissions()
        {
            var connectionString
                = SimpleConnectionStringWithCredentials<EmptyContext>(
                    "EFTestSimpleModelUser",
                    "Password1");

            DatabaseExists_returns_false_for_non_existing_database(
                () => Database.Exists(connectionString));
        }

        //[Fact]
        // See issue 1091
        public void DatabaseExists_returns_true_for_existing_attached_database_when_no_master_permissions()
        {
            var connectionString = SimpleAttachConnectionString<AttachedContext>();
            using (var context = new AttachedContext(connectionString))
            {
                context.Database.Delete();
                // Ensure database is initialized
                context.Database.Initialize(force: true);
            }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        @"IF NOT EXISTS (SELECT * FROM sys.sysusers WHERE name= N'EFTestSimpleModelUser')
                          BEGIN
                            CREATE USER [EFTestSimpleModelUser] FOR LOGIN [EFTestSimpleModelUser]
                          END";
                    command.ExecuteNonQuery();
                }
            }

            try
            {
                using (var context = new AttachedContext(
                    SimpleAttachConnectionStringWithCredentials<AttachedContext>(
                        "EFTestSimpleModelUser",
                        "Password1")))
                {
                    Assert.True(context.Database.Exists());
                }
            }
            finally
            {
                using (var context = new AttachedContext(SimpleAttachConnectionString<AttachedContext>()))
                {
                    context.Database.Delete();
                }
            }
        }

        [Fact]
        public void DatabaseExists_returns_true_for_existing_attached_database_with_InitialCatalog_when_no_master_nor_database_permission()
        {
            DatabaseExists_returns_true_for_existing_attached_database_when_no_master_nor_database_permission(true);
        }

        [Fact]
        public void DatabaseExists_returns_true_for_existing_attached_database_without_InitialCatalog_when_no_master_nor_database_permission()
        {
            DatabaseExists_returns_true_for_existing_attached_database_when_no_master_nor_database_permission(false);
        }

        private void DatabaseExists_returns_true_for_existing_attached_database_when_no_master_nor_database_permission(bool useInitialcatalog)
        {
            using (var context = new AttachedContext(SimpleAttachConnectionString<AttachedContext>()))
            {
                if (DatabaseTestHelpers.IsSqlAzure(context.Database.Connection.ConnectionString))
                {
                    // SQL Azure does not suppot attaching databases
                    return;
                }

                // Ensure database is initialized
                context.Database.Initialize(force: true);
            }

            // See CodePlex 1554 - Handle User Instance flakiness
            MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(new ExecutionStrategyResolver<IDbExecutionStrategy>(
                SqlProviderServices.ProviderInvariantName, null, () => new SqlAzureExecutionStrategy()));
            try
            {
                using (var context = new AttachedContext(
                    SimpleAttachConnectionStringWithCredentials<AttachedContext>(
                        "EFTestSimpleModelUser",
                        "Password1",
                        useInitialcatalog)))
                {
                    Assert.True(context.Database.Exists());
                }
            }
            finally
            {
                MutableResolver.ClearResolvers();

                using (var context = new AttachedContext(SimpleAttachConnectionString<AttachedContext>()))
                {
                    context.Database.Delete();
                }
            }
        }

        [Fact]
        public void DatabaseExists_returns_false_for_non_existing_attached_database_with_InitialCatalog_when_no_master_permission()
        {
            DatabaseExists_returns_false_for_non_existing_attached_database_when_no_master_permission(true);
        }

        [Fact]
        public void DatabaseExists_returns_false_for_non_existing_attached_database_without_InitialCatalog_when_no_master_permission()
        {
            DatabaseExists_returns_false_for_non_existing_attached_database_when_no_master_permission(false);
        }

        private void DatabaseExists_returns_false_for_non_existing_attached_database_when_no_master_permission(bool useInitialCatalog)
        {
            using (var context = new AttachedContext(
                SimpleAttachConnectionStringWithCredentials<AttachedContext>(
                    "EFTestSimpleModelUser",
                    "Password1", useInitialCatalog)))
            {
                Assert.False(context.Database.Exists());
            }
        }

        #endregion

        #region Positive Delete tests

        [Fact]
        public void Can_delete_database_using_Database_obtained_from_context()
        {
            using (var context = new EmptyContext())
            {
                var database = context.Database;
                Can_delete_database(() => database.Delete(), database.Exists);
            }
        }

        [Fact]
        public void Can_delete_database_using_static_method_taking_database_name()
        {
            Can_delete_database(
                () => Database.Delete(DefaultDbName<EmptyContext>()),
                () => Database.Exists(DefaultDbName<EmptyContext>()));
        }

        [Fact]
        public void Can_delete_database_using_static_method_taking_named_connection_string_that_exists_in_config_file()
        {
            Can_delete_database_using_static_method_taking_named_connection_string("SimpleModelInAppConfig");
        }

        [Fact]
        public void Can_delete_database_using_static_method_taking_named_connection_string_where_last_token_exists_in_config_file()
        {
            Can_delete_database_using_static_method_taking_named_connection_string(
                "NonExistant.Namespace.SimpleModelInAppConfig");
        }

        private void Can_delete_database_using_static_method_taking_named_connection_string(string namedConnectionString)
        {
            // Arrange
            Database.Delete(namedConnectionString);
            using (var context = new SimpleModelContext(namedConnectionString))
            {
                context.Database.Create();
            }

            // Act- Assert
            Database.Delete(namedConnectionString);
            Assert.False(Database.Exists(namedConnectionString));
        }

        [Fact]
        public void Can_delete_database_using_static_method_taking_connection_string()
        {
            Can_delete_database(
                () => Database.Delete(SimpleConnectionString<EmptyContext>()),
                () => Database.Exists(SimpleConnectionString<EmptyContext>()));
        }

        [Fact]
        public void Can_delete_database_using_static_method_taking_existing_connection_which_is_closed()
        {
            using (var connection = SimpleConnection<EmptyContext>())
            {
                // Arrange
                Assert.True(connection.State == ConnectionState.Closed);

                // Act/Assert
                Can_delete_database(
                    () => Database.Delete(connection),
                    () => Database.Exists(connection));
            }
        }

        private void Can_delete_database(Action deleteDatabase, Func<bool> databaseExists)
        {
            using (var context = new EmptyContext())
            {
                context.Database.CreateIfNotExists();
            }

            deleteDatabase();

            Assert.False(databaseExists());
        }

        [Fact]
        public void Can_delete_database_if_exists_using_Database_obtained_from_context()
        {
            using (var context = new EmptyContext())
            {
                var database = context.Database;
                Can_delete_database_if_exists(database.Delete, database.Exists);
            }
        }

        [Fact]
        public void Can_delete_attached_database_with_InitialCatalog_if_exists_using_Database_obtained_from_context()
        {
            Can_delete_attached_database_if_exists_using_Database_obtained_from_context(true);
        }

        [Fact]
        public void Can_delete_attached_database_without_InitialCatalog_if_exists_using_Database_obtained_from_context()
        {
            Can_delete_attached_database_if_exists_using_Database_obtained_from_context(false);
        }

        private void Can_delete_attached_database_if_exists_using_Database_obtained_from_context(bool useInitialCatalog)
        {
            AttachableDatabaseTest(
                (context) =>
                    {
                        // See CodePlex 1554 - Handle User Instance flakiness
                        MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(new ExecutionStrategyResolver<IDbExecutionStrategy>(
                            SqlProviderServices.ProviderInvariantName, null, () => new SqlAzureExecutionStrategy()));
                        try
                        {
                            // Ensure database is initialized
                            context.Database.Initialize(force: true);

                            Assert.True(context.Database.Delete());
                            Assert.False(context.Database.Exists());
                            Assert.False(File.Exists(GetAttachDbFilename(context)));
                        }
                        finally
                        {
                            MutableResolver.ClearResolvers();
                        }
                    }, useInitialCatalog);
        }

        [Fact]
        public void Can_delete_database_if_exists_using_static_method_taking_database_name()
        {
            Can_delete_database_if_exists(
                () => Database.Delete(DefaultDbName<EmptyContext>()),
                () => Database.Exists(DefaultDbName<EmptyContext>()));
        }

        [Fact]
        public void
            Can_delete_database_if_exists_using_static_method_taking_named_connection_string_that_exists_in_config_file()
        {
            Can_delete_database_if_exists_using_static_method_taking_named_connection_string("SimpleModelInAppConfig");
        }

        [Fact]
        public void Can_delete_database_if_exists_using_static_method_taking_named_connection_string_where_last_token_exists_in_config_file(
            
            )
        {
            Can_delete_database_if_exists_using_static_method_taking_named_connection_string(
                "NonExistant.NamespaceName.SimpleModelInAppConfig");
        }

        private void Can_delete_database_if_exists_using_static_method_taking_named_connection_string(
            string namedConnectionString)
        {
            // Arrange
            Database.Delete(namedConnectionString);
            using (var context = new SimpleModelContext(namedConnectionString))
            {
                context.Database.Create();
                Assert.True(Database.Exists(namedConnectionString));
            }

            // Act- Assert
            Database.Delete(namedConnectionString);
            Assert.False(Database.Exists(namedConnectionString));
        }

        [Fact]
        public void Can_delete_database_if_exists_using_static_method_taking_connection_string()
        {
            Can_delete_database_if_exists(
                () => Database.Delete(SimpleConnectionString<EmptyContext>()),
                () => Database.Exists(SimpleConnectionString<EmptyContext>()));
        }

        [Fact]
        public void Can_delete_database_if_exists_using_static_method_taking_existing_connection_which_is_closed()
        {
            using (var connection = SimpleConnection<EmptyContext>())
            {
                // Arrange
                Assert.True(connection.State == ConnectionState.Closed);

                // Act/Assert
                Can_delete_database_if_exists(
                    () => Database.Delete(connection),
                    () => Database.Exists(connection));
            }
        }

        private void Can_delete_database_if_exists(Func<bool> deleteDatabaseIfExists, Func<bool> databaseExists)
        {
            using (var context = new EmptyContext())
            {
                context.Database.CreateIfNotExists();
            }

            Assert.True(deleteDatabaseIfExists());

            Assert.False(databaseExists());
        }

        [Fact]
        public void DeleteDatabaseIfExists_does_nothing_if_database_does_not_exist_using_Database_obtained_from_context()
        {
            using (var context = new EmptyContext())
            {
                DeleteDatabaseIfExists_does_nothing_if_database_does_not_exist(context.Database.Delete);
            }
        }

        [Fact]
        public void DeleteDatabaseIfExists_does_nothing_if_attached_database_does_not_exist_using_Database_obtained_from_context()
        {
            AttachableDatabaseTest(
                (context) =>
                    {
                        // NOTE: Database has not been initialized/created
                        Assert.False(context.Database.Delete());
                    });
        }

        [Fact]
        public void DeleteDatabaseIfExists_does_nothing_if_attached_database_without_InitialCatalog_does_not_exist_using_Database_obtained_from_context()
        {
            AttachableDatabaseTest(
                (context) =>
                {
                    // See CodePlex 1554 - Handle User Instance flakiness
                    MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(new ExecutionStrategyResolver<IDbExecutionStrategy>(
                        SqlProviderServices.ProviderInvariantName, null, () => new SqlAzureExecutionStrategy()));
                    try
                    {
                        // NOTE: Database has not been initialized/created
                        Assert.False(context.Database.Delete());
                    }
                    finally
                    {
                        MutableResolver.ClearResolvers();
                    }
                }, useInitialCatalog: false);
        }

        [Fact]
        public void
            DeleteDatabaseIfExists_does_nothing_if_database_does_not_exist_using_static_method_taking_database_name()
        {
            DeleteDatabaseIfExists_does_nothing_if_database_does_not_exist(
                () => Database.Delete(DefaultDbName<EmptyContext>()));
        }

        public void
            DeleteDatabaseIfExists_does_nothing_if_database_does_not_exist_using_static_method_taking_named_connection_string_that_exists_in_config_file
            ()
        {
            DeleteDatabaseIfExists_does_nothing_if_database_does_not_exist_using_static_method_taking_named_connection_string
                ("SimpleModelInAppConfig");
        }

        public void
            DeleteDatabaseIfExists_does_nothing_if_database_does_not_exist_using_static_method_taking_named_connection_string_where_last_token_exists_in_config_file
            ()
        {
            DeleteDatabaseIfExists_does_nothing_if_database_does_not_exist_using_static_method_taking_named_connection_string
                ("NonExistant.NamespaceName.SimpleModelInAppConfig");
        }

        private void
            DeleteDatabaseIfExists_does_nothing_if_database_does_not_exist_using_static_method_taking_named_connection_string
            (string namedConnectionString)
        {
            // Arrange, for test prep
            Database.Delete(namedConnectionString);
            Assert.False(Database.Exists(namedConnectionString));

            // Act- Assert
            Database.Delete(namedConnectionString);
            Assert.False(Database.Exists(namedConnectionString));
        }

        [Fact]
        public void
            DeleteDatabaseIfExists_does_nothing_if_database_does_not_exist_using_static_method_taking_connection_string()
        {
            DeleteDatabaseIfExists_does_nothing_if_database_does_not_exist(
                () => Database.Delete(SimpleConnectionString<EmptyContext>()));
        }

        [Fact]
        public void
            DeleteDatabaseIfExists_does_nothing_if_database_does_not_exist_using_static_method_taking_existing_connection_which_is_closed()
        {
            using (var connection = SimpleConnection<EmptyContext>())
            {
                // Arrange
                Assert.True(connection.State == ConnectionState.Closed);
                var eventsTracker = new ConnectionEventsTracker(connection);

                // Act
                DeleteDatabaseIfExists_does_nothing_if_database_does_not_exist(() => Database.Delete(connection));

                // Assert
                eventsTracker.VerifyNoConnectionEventsWereFired();
            }
        }

        private void DeleteDatabaseIfExists_does_nothing_if_database_does_not_exist(Func<bool> deleteDatabaseIfExists)
        {
            Database.Delete(DefaultDbName<EmptyContext>());

            Assert.False(deleteDatabaseIfExists());
        }

        #endregion

        #region Positive Create tests

        [Fact]
        public void
            Can_create_database_using_Database_obtained_from_context_even_when_context_not_initialized_Dev10_904982()
        {
            using (var context = new EmptyContext())
            {
                Can_create_database(context.Database);
            }
        }

        [Fact]
        public void Can_create_database_using_Database_obtained_from_context()
        {
            using (var context = new EmptyContext())
            {
                GetObjectContext(context); // Ensures context is initialized
                Can_create_database(context.Database);
            }
        }

        [Fact]
        public void Can_create_attached_database_with_InitialCatalog_using_Database_obtained_from_context()
        {
            Can_create_attached_database_using_Database_obtained_from_context(true);
        }

        [Fact]
        public void Can_create_attached_database_without_InitialCatalog_using_Database_obtained_from_context()
        {
            Can_create_attached_database_using_Database_obtained_from_context(false);
        }

        private void Can_create_attached_database_using_Database_obtained_from_context(bool useInitialCatalog)
        {
            AttachableDatabaseTest(
                (context) =>
                    {
                        // See CodePlex 1554 - Handle User Instance flakiness
                        MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(new ExecutionStrategyResolver<IDbExecutionStrategy>(
                            SqlProviderServices.ProviderInvariantName, null, () => new SqlAzureExecutionStrategy()));
                        try
                        {
                            // Ensure database is initialized
                            context.Database.Initialize(force: true);

                            Can_create_database(context.Database);
                        }
                        finally
                        {
                            MutableResolver.ClearResolvers();
                        }
                    }, useInitialCatalog);
        }

        private void Can_create_database(Database database)
        {
            database.Delete();

            database.Create();

            Assert.True(database.Exists());
        }

        [Fact]
        public void Can_create_database_if_it_does_not_exist_using_Database_obtained_from_context_Dev10_884353()
        {
            using (var context = new EmptyContext())
            {
                GetObjectContext(context); // Ensures context is initialized
                Database.Delete(DefaultDbName<EmptyContext>());

                Assert.True(context.Database.CreateIfNotExists());

                Assert.True(context.Database.Exists());
            }
        }

        [Fact]
        public void CreateDatabaseIfNotExists_does_nothing_if_database_exists_using_Database_obtained_from_context()
        {
            using (var context = new EmptyContext())
            {
                context.Database.CreateIfNotExists();

                Assert.False(context.Database.CreateIfNotExists());

                Assert.True(context.Database.Exists());
            }
        }

        #endregion

        #region Negative Create tests

        [Fact]
        public void CreateDatabase_throws_if_database_already_exists_using_Database_obtained_from_context()
        {
            using (var context = new EmptyContext())
            {
                context.Database.CreateIfNotExists();
                Assert.Throws<InvalidOperationException>(() => context.Database.Create()).ValidateMessage(
                    "Database_DatabaseAlreadyExists", "SimpleModel.EmptyContext");
            }
        }

        #endregion

        #region Positive multiple operations tests

        [Fact]
        public void Database_can_be_reused_for_multiple_operations_when_backed_by_a_context()
        {
            using (var context = new EmptyContext())
            {
                var database = context.Database;
                database.Delete(); // First use
                Assert.False(database.Exists()); // Second use
            }
        }

        [Fact]
        public void An_existing_connection_can_be_reused_for_multiple_operations_when_using_static_methods()
        {
            using (var connection = SimpleConnection<EmptyContext>())
            {
                Database.Delete(connection); // First use
                Assert.False(Database.Exists(connection)); // Second use
            }
        }

        [Fact]
        public void A_database_name_can_be_reused_for_multiple_operations_when_using_static_methods()
        {
            Database.Delete(DefaultDbName<EmptyContext>()); // First use
            Assert.False(Database.Exists(DefaultDbName<EmptyContext>())); // Second use
        }

        [Fact]
        public void
            A_named_conection_string_can_be_reused_for_multiple_operations_when_using_static_methods_Delete_and_Create_scenario_using_named_connection_string_exists_in_config_file
            ()
        {
            A_named_conection_string_can_be_reused_for_multiple_operations_when_using_static_methods_Delete_and_Create(
                "SimpleModelInAppConfig");
        }

        [Fact]
        public void
            A_named_conection_string_can_be_reused_for_multiple_operations_when_using_static_methods_Delete_and_Create_scenario_where_named_connection_string_last_token_exists_in_config_file
            ()
        {
            A_named_conection_string_can_be_reused_for_multiple_operations_when_using_static_methods_Delete_and_Create(
                "NonExistant.NameSpace.SimpleModelInAppConfig");
        }

        private void
            A_named_conection_string_can_be_reused_for_multiple_operations_when_using_static_methods_Delete_and_Create(
            string namedConnectionString)
        {
            // Recreation of DB, a common scenario

            // Arrange
            Database.Delete(namedConnectionString);
            Assert.False(Database.Exists(namedConnectionString));

            using (var context = new SimpleModelContext(namedConnectionString))
            {
                // Act - Assert
                context.Database.Create();
                Assert.True(context.Database.Exists());
            }
        }

        [Fact]
        public void
            A_named_conection_string_can_be_reused_for_multiple_operations_when_using_static_methods_Create_and_Delete_scenario_using_named_connection_string_exists_in_config_file
            ()
        {
            A_named_conection_string_can_be_reused_for_multiple_operations_when_using_static_methods_Create_and_Delete(
                "SimpleModelInAppConfig");
        }

        [Fact]
        public void
            A_named_conection_string_can_be_reused_for_multiple_operations_when_using_static_methods_Create_and_Delete_scenario_using_named_connection_string_last_token_in_config_file
            ()
        {
            A_named_conection_string_can_be_reused_for_multiple_operations_when_using_static_methods_Create_and_Delete(
                "NonExistant.NameSpace.SimpleModelInAppConfig");
        }

        private void
            A_named_conection_string_can_be_reused_for_multiple_operations_when_using_static_methods_Create_and_Delete(
            string namedConnectionString)
        {
            // Create and Delete, a common scenario
            // Arrange
            Database.Delete(namedConnectionString);
            Assert.False(Database.Exists(namedConnectionString));

            using (var context = new SimpleModelContext(namedConnectionString))
            {
                // Act - Assert
                context.Database.Create();
                Assert.True(context.Database.Exists());
            }

            // Act -Assert
            Database.Delete(namedConnectionString);
            Assert.False(Database.Exists(namedConnectionString));
        }

        [Fact]
        public void Checking_if_database_exists_should_not_create_database_Dev10_883499()
        {
            using (var context = new EmptyContext())
            {
                GetObjectContext(context); // Ensures context is initialized
                Database.Delete(DefaultDbName<EmptyContext>());
                Assert.False(context.Database.Exists());

                context.Database.Create();
                Assert.True(context.Database.Exists());
            }
        }

        #endregion

        #region Mutating the database connection string (Dev11 357496)

        public abstract class MutatingConnectionContext<TContext> : SimpleModelContext
        {
            public static string StartingConnectionString = SimpleConnectionString(typeof(TContext).FullName);
            public static string ChangedConnectionString = SimpleConnectionString(typeof(TContext).FullName + "_Mutated");

            protected MutatingConnectionContext()
                : base(StartingConnectionString)
            {
            }
        }

        public class MutatingConnectionContextInitializer<TContext> :
            DropCreateDatabaseAlways<MutatingConnectionContext<TContext>>
        {
            public string ConnectionStringUsed { get; set; }

            protected override void Seed(MutatingConnectionContext<TContext> context)
            {
                ConnectionStringUsed = context.Database.Connection.ConnectionString;
            }
        }

        public class MutatingConnectionContext1 : MutatingConnectionContext<MutatingConnectionContext1>
        {
            public MutatingConnectionContext1()
            {
                Database.Connection.ConnectionString = ChangedConnectionString;
            }
        }

        [Fact]
        public void Connection_string_can_be_set_inside_DbContext_constructor_as_long_as_database_initialization_has_not_happened()
        {
            Database.Delete(MutatingConnectionContext1.StartingConnectionString);
            Database.Delete(MutatingConnectionContext1.ChangedConnectionString);

            var initializer = new MutatingConnectionContextInitializer<MutatingConnectionContext1>();
            Database.SetInitializer<MutatingConnectionContext1>(initializer);

            using (var context = new MutatingConnectionContext1())
            {
                context.Products.Load();

                Assert.Equal(MutatingConnectionContext1.ChangedConnectionString, initializer.ConnectionStringUsed);
                Assert.Equal(
                    RemovePasswordAndSemicolonsFromConnectionString(MutatingConnectionContext1.ChangedConnectionString),
                    RemovePasswordAndSemicolonsFromConnectionString(context.Database.Connection.ConnectionString));

                Assert.False(Database.Exists(MutatingConnectionContext1.StartingConnectionString));
                Assert.True(Database.Exists(MutatingConnectionContext1.ChangedConnectionString));
            }
        }

        private string RemovePasswordAndSemicolonsFromConnectionString(string connectionString)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            builder.Remove("password");

            return builder.ToString().Replace(";", ""); 
        }

        public class MutatingConnectionContext2 : MutatingConnectionContext<MutatingConnectionContext2>
        {
        }

        [Fact]
        public void Connection_string_can_be_set_after_DbContext_construction_as_long_as_database_initialization_has_not_happened()
        {
            Database.Delete(MutatingConnectionContext2.StartingConnectionString);
            Database.Delete(MutatingConnectionContext2.ChangedConnectionString);

            var initializer = new MutatingConnectionContextInitializer<MutatingConnectionContext2>();
            Database.SetInitializer<MutatingConnectionContext2>(initializer);

            using (var context = new MutatingConnectionContext2())
            {
                context.Database.Connection.ConnectionString = MutatingConnectionContext2.ChangedConnectionString;

                context.Products.Load();

                Assert.Equal(MutatingConnectionContext2.ChangedConnectionString, initializer.ConnectionStringUsed);
                Assert.Equal(
                    RemovePasswordAndSemicolonsFromConnectionString(MutatingConnectionContext2.ChangedConnectionString),
                    RemovePasswordAndSemicolonsFromConnectionString(context.Database.Connection.ConnectionString));

                Assert.False(Database.Exists(MutatingConnectionContext2.StartingConnectionString));
                Assert.True(Database.Exists(MutatingConnectionContext2.ChangedConnectionString));
            }
        }

        public abstract class MutatingConnectionContext4<TContext> : SimpleModelContext
            where TContext : DbContext
        {
            static MutatingConnectionContext4()
            {
                Database.SetInitializer(new DropCreateDatabaseAlways<TContext>());
            }

            protected MutatingConnectionContext4(string connectionString)
                : base(connectionString)
            {
            }
        }

        public class MutatingConnectionContext4a : MutatingConnectionContext4<MutatingConnectionContext4a>
        {
            public MutatingConnectionContext4a(string connectionString)
                : base(connectionString)
            {
            }
        }

        [Fact]
        public void If_connection_is_changed_to_point_to_different_database_then_operations_that_use_OriginalConnectionString_pick_up_this_change()
        {
            If_connection_is_changed_then_operations_that_use_OriginalConnectionString_pick_up_this_change(
                c => new MutatingConnectionContext4a(c),
                SimpleConnectionString("MutatingConnectionContext4_Mutated"));
        }

        public class MutatingConnectionContext4b : MutatingConnectionContext4<MutatingConnectionContext4b>
        {
            public MutatingConnectionContext4b(string connectionString)
                : base(connectionString)
            {
            }
        }

#if !NET40

        [Fact]
        public void If_connection_is_changed_to_point_to_different_server_then_operations_that_use_OriginalConnectionString_pick_up_this_change()
        {
            var changedServer = DatabaseTestHelpers.IsLocalDb(SimpleConnectionString("")) ? @".\SQLEXPRESS" : @"(localdb)\v11.0";
            var changedConnectionString = string.Format(
                CultureInfo.InvariantCulture,
                @"Data Source={0};Initial Catalog=MutatingConnectionContext4;Integrated Security=True", changedServer);
         
            If_connection_is_changed_then_operations_that_use_OriginalConnectionString_pick_up_this_change(
                c => new MutatingConnectionContext4b(c),
                changedConnectionString);
        }

#endif

        private void If_connection_is_changed_then_operations_that_use_OriginalConnectionString_pick_up_this_change(
            Func<string, DbContext> createContext, string changedConnectionString)
        {
            var startingConnectionString = SimpleConnectionString("MutatingConnectionContext4");
            
            Database.Delete(startingConnectionString);
            Database.Delete(changedConnectionString);

            using (var context = createContext(startingConnectionString))
            {
                context.Database.Initialize(force: false);

                Assert.True(Database.Exists(startingConnectionString));
                Assert.False(Database.Exists(changedConnectionString));
                Assert.True(context.Database.Exists());

                context.Database.Connection.ConnectionString = changedConnectionString;

                Assert.False(context.Database.Exists());

                context.Database.Create();

                Assert.True(context.Database.Exists());
                Assert.True(Database.Exists(changedConnectionString));
            }
        }

        public class MutatingConnectionContext5 : SimpleModelContext
        {
            public static string ChangedConnectionString = SimpleConnectionString("MutatingConnectionContext5_Mutated");

            static MutatingConnectionContext5()
            {
                Database.SetInitializer(new DropCreateDatabaseAlways<MutatingConnectionContext5>());
            }

            public MutatingConnectionContext5()
                : base("Initial Catalog=Binky")
            {
                Database.Connection.ConnectionString = ChangedConnectionString;
            }
        }

        [Fact]
        public void Connection_string_can_be_set_inside_DbContext_constructor_when_initial_connection_string_is_invalid()
        {
            Database.Delete(MutatingConnectionContext5.ChangedConnectionString);

            using (var context = new MutatingConnectionContext5())
            {
                context.Products.Load();

                Assert.Equal(
                    RemovePasswordAndSemicolonsFromConnectionString(MutatingConnectionContext5.ChangedConnectionString),
                    RemovePasswordAndSemicolonsFromConnectionString(context.Database.Connection.ConnectionString));
                Assert.True(Database.Exists(MutatingConnectionContext5.ChangedConnectionString));
            }
        }

        #endregion

        private string GetAttachDbFilename(DbContext context)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(context.Database.Connection.ConnectionString);
            return connectionStringBuilder.AttachDBFilename;
        }
    }
}
