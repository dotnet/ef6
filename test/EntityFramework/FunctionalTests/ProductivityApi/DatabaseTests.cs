// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Data.Entity.Core;
    using System.Data;
    using System.Data.Entity;
    using SimpleModel;
    using Xunit;

    /// <summary>
    ///     Functional tests for Database.  Unit tests also exist in the unit tests project.
    /// </summary>
    public class DatabaseTests : FunctionalTestBase
    {
        #region Infrastructure/setup

        static DatabaseTests()
        {
            using (var context = new SimpleModelContext())
            {
                context.Database.Initialize(force: false);
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

        private void AttachableDatabaseTest(Action<AttachedContext> testMethod)
        {
            using (var context = new AttachedContext(SimpleAttachConnectionString<AttachedContext>()))
            {
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
        public void DatabaseExists_returns_true_for_existing_attached_database_when_using_Database_obtained_from_context()
        {
            AttachableDatabaseTest(
                (context) =>
                    {
                        // Ensure database is initialized
                        context.Database.Initialize(force: true);

                        Assert.True(context.Database.Exists());
                    });
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

        // Skipped due to Dev10.882884  DCR: PI: DbContext cannot be created with an already-open existing connection 
        // EntityConnection is created in the call "Database.Exists(connection)"
        // [Fact(Skip = "Dev10.882884")]
        // See http://entityframework.codeplex.com/workitem/45
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
        public void Can_delete_attached_database_if_exists_using_Database_obtained_from_context()
        {
            AttachableDatabaseTest(
                (context) =>
                    {
                        // Ensure database is initialized
                        context.Database.Initialize(force: true);

                        Assert.True(context.Database.Delete());
                        Assert.False(context.Database.Exists());
                    });
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
            Database.Delete("SimpleModelInAppConfig");
            using (var context = new SimpleModelContext("SimpleModelInAppConfig"))
            {
                context.Database.Create();
                Assert.True(Database.Exists("SimpleModelInAppConfig"));
            }

            // Act- Assert
            Database.Delete("SimpleModelInAppConfig");
            Assert.False(Database.Exists("SimpleModelInAppConfig"));
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
        public void Can_create_attached_database_using_Database_obtained_from_context()
        {
            AttachableDatabaseTest(
                (context) =>
                    {
                        // Ensure database is initialized
                        context.Database.Initialize(force: true);

                        Can_create_database(context.Database);
                    });
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
                    MutatingConnectionContext1.ChangedConnectionString,
                    context.Database.Connection.ConnectionString);

                Assert.False(Database.Exists(MutatingConnectionContext1.StartingConnectionString));
                Assert.True(Database.Exists(MutatingConnectionContext1.ChangedConnectionString));
            }
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
                    MutatingConnectionContext2.ChangedConnectionString,
                    context.Database.Connection.ConnectionString);

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

        private const string ConnectionStringTemplate = @"Data Source={0};Initial Catalog={1};Integrated Security=True";

        public class MutatingConnectionContext4a : MutatingConnectionContext4<MutatingConnectionContext4a>
        {
            public MutatingConnectionContext4a(string connectionString)
                : base(connectionString)
            {
            }
        }

        [Fact]
        public void
            If_connection_is_changed_to_point_to_different_database_then_operations_that_use_OriginalConnectionString_pick_up_this_change()
        {
            If_connection_is_changed_then_operations_that_use_OriginalConnectionString_pick_up_this_change(
                c => new MutatingConnectionContext4a(c),
                string.Format(ConnectionStringTemplate, @".\SQLEXPRESS", "MutatingConnectionContext4_Mutated"));
        }

        public class MutatingConnectionContext4b : MutatingConnectionContext4<MutatingConnectionContext4b>
        {
            public MutatingConnectionContext4b(string connectionString)
                : base(connectionString)
            {
            }
        }

        [Fact]
        public void
            If_connection_is_changed_to_point_to_different_server_then_operations_that_use_OriginalConnectionString_pick_up_this_change()
        {
            If_connection_is_changed_then_operations_that_use_OriginalConnectionString_pick_up_this_change(
                c => new MutatingConnectionContext4b(c),
                string.Format(ConnectionStringTemplate, @"(localdb)\v11.0", "MutatingConnectionContext4"));
        }

        private void If_connection_is_changed_then_operations_that_use_OriginalConnectionString_pick_up_this_change(
            Func<string, DbContext> createContext, string changedConnectionString)
        {
            var startingConnectionString = string.Format(
                ConnectionStringTemplate, @".\SQLEXPRESS",
                "MutatingConnectionContext4");

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
                    MutatingConnectionContext5.ChangedConnectionString,
                    context.Database.Connection.ConnectionString);
                Assert.True(Database.Exists(MutatingConnectionContext5.ChangedConnectionString));
            }
        }

        #endregion
    }
}
