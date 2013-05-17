// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System.Data;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text.RegularExpressions;
    using SimpleModel;
    using Xunit;

    /// <summary>
    ///     Tests for creating and managing connections at the DbContext, DbContextFactory, and Database level.
    ///     Note that the LazyConnectionTests in the UnitTests project cover more low-level cases that don't require an actual context.
    /// </summary>
    public class ConnectionTests : FunctionalTestBase
    {
        #region Infrastructure/setup

        public ConnectionTests()
        {
            CreateMetadataFilesForSimpleModel();
        }

        #endregion

        #region  Using EntityConnection objects and EF connection strings

        public class SimpleModelContextForModelFirst : SimpleModelContext
        {
            public SimpleModelContextForModelFirst(DbCompiledModel model)
                : base(model)
            {
            }

            public SimpleModelContextForModelFirst(string nameOrConnectionString)
                : base(nameOrConnectionString)
            {
            }

            public SimpleModelContextForModelFirst(DbConnection existingConnection, bool contextOwnsConnection = false)
                : base(existingConnection, contextOwnsConnection)
            {
            }
        }

        [Fact]
        public void Entity_connection_string_on_derived_context_can_be_used_to_create_context_for_existing_model()
        {
            using (var context = new SimpleModelContextForModelFirst(SimpleModelEntityConnectionString))
            {
                Assert.NotNull(context.Products);
                VerifySimpleModelContext(context);
            }
        }

        [Fact]
        public void Entity_connection_string_on_empty_context_can_be_used_to_create_context_for_existing_model()
        {
            using (var context = new DbContext(SimpleModelEntityConnectionString))
            {
                VerifySimpleModelContext(context);
            }
        }

        [Fact]
        public void Named_connection_string_on_derived_context_can_be_used_to_create_context_for_existing_model()
        {
            using (var context = new SimpleModelContextForModelFirst("EntityConnectionForSimpleModel"))
            {
                Assert.NotNull(context.Products);
                VerifySimpleModelContext(context);
            }
        }

        [Fact]
        public void Named_connection_string_on_empty_context_can_be_used_to_create_context_for_existing_model()
        {
            using (var context = new DbContext("EntityConnectionForSimpleModel"))
            {
                VerifySimpleModelContext(context);
            }
        }

        [Fact]
        public void Named_connection_string_using_name_keyword_on_derived_context_can_be_used_to_create_context_for_existing_model()
        {
            using (var context = new SimpleModelContextForModelFirst("name=EntityConnectionForSimpleModel"))
            {
                Assert.NotNull(context.Products);
                VerifySimpleModelContext(context);
            }
        }

        [Fact]
        public void
            Named_connection_string_using_name_keywordon_empty_context_can_be_used_to_create_context_for_existing_model()
        {
            using (var context = new DbContext("name=EntityConnectionForSimpleModel"))
            {
                VerifySimpleModelContext(context);
            }
        }

        [Fact]
        public void EntityConnection_object_with_derived_context_can_be_used_to_create_context_for_existing_model()
        {
            using (var connection = new EntityConnection(SimpleModelEntityConnectionString))
            {
                using (var context = new SimpleModelContextForModelFirst(connection))
                {
                    Assert.NotNull(context.Products);
                    VerifySimpleModelContext(context);
                }
            }
        }

        [Fact]
        public void EntityConnection_object_with_empty_context_can_be_used_to_create_context_for_existing_model()
        {
            using (
                var context = new DbContext(
                    new EntityConnection(SimpleModelEntityConnectionString),
                    contextOwnsConnection: true))
            {
                VerifySimpleModelContext(context);
            }
        }

        [Fact]
        public void GetMetadataWorkspace_returns_an_initialized_workspace_when_connection_string_constructor_is_used()
        {
            using (var connection = new EntityConnection(SimpleModelEntityConnectionString))
            {
                var workspace = connection.GetMetadataWorkspace();

                Assert.NotNull(workspace.GetItemCollection(DataSpace.OSpace));
                Assert.NotNull(workspace.GetItemCollection(DataSpace.OCSpace));
                Assert.NotNull(workspace.GetItemCollection(DataSpace.SSpace));
                Assert.NotNull(workspace.GetItemCollection(DataSpace.CSSpace));
                Assert.NotNull(workspace.GetItemCollection(DataSpace.CSpace));
            }
        }

        [Fact]
        public void Context_name_can_be_used_to_find_entity_connection_string_in_app_config()
        {
            using (var context = new EntityConnectionForSimpleModel())
            {
                VerifySimpleModelContext(context);
            }
        }

        private void VerifySimpleModelContext(DbContext context)
        {
            var product = context.Set<Product>().Find(1);
            Assert.NotNull(product);
            context.Entry(product).Reference(p => p.Category).Load();
            Assert.NotNull(product.Category);

            // Ensure that Database.Connection always returns the store connection
            Assert.NotNull(context.Database.Connection);
            Assert.IsNotType<EntityConnection>(context.Database.Connection);
        }

        #endregion

        #region Positive existing connection tests

        [Fact]
        public void Existing_connection_is_closed_if_it_started_closed()
        {
            Existing_connection_is_same_state_it_started_in_after_use(ConnectionState.Closed);
        }

        [Fact]
        public void Existing_connection_is_open_if_it_started_open()
        {
            Existing_connection_is_same_state_it_started_in_after_use(ConnectionState.Open);
        }

        private void Existing_connection_is_same_state_it_started_in_after_use(ConnectionState initialState)
        {
            using (var connection = SimpleConnection<SimpleModelContext>())
            {
                if (initialState == ConnectionState.Open)
                {
                    // if connection string contains password (i.e. not Integrated Security)
                    // this scenairo will not work since it requires connection to master
                    // and the connection will have been stripped of credentials by then
                    if (ConnectionStringContainsPassword(connection.ConnectionString))
                    {
                        return;
                    }

                    connection.Open();
                }
                Assert.Equal(initialState, connection.State);

                var opened = false;
                connection.StateChange += (_, e) =>
                    {
                        if (e.CurrentState
                            == ConnectionState.Open)
                        {
                            opened = true;
                        }
                    };

                using (var context = new SimpleModelContext(connection))
                {
                    context.Products.FirstOrDefault();

                    Assert.Equal(initialState == ConnectionState.Closed, opened);
                }

                Assert.Equal(initialState, connection.State);
                connection.Close();
            }
        }

        private bool ConnectionStringContainsPassword(string connectionString)
        {
            var regex = new Regex("Password=[^;]*", RegexOptions.IgnoreCase);
            return regex.IsMatch(connectionString);
        }

        [Fact]
        public void Existing_connection_is_not_disposed_after_use()
        {
            var disposed = false;
            using (var connection = SimpleConnection<SimpleModelContext>())
            {
                connection.Disposed += (_, __) => disposed = true;

                using (var context = new SimpleModelContext(connection))
                {
                    context.Products.FirstOrDefault();
                }

                Assert.False(disposed);
            }
            Assert.True(disposed);
        }

        [Fact]
        public void Existing_EntityConnection_is_not_disposed_after_use()
        {
            var disposed = false;
            using (var connection = new EntityConnection(SimpleModelEntityConnectionString))
            {
                connection.Disposed += (_, __) => disposed = true;

                using (var context = new DbContext(connection, contextOwnsConnection: false))
                {
                    VerifySimpleModelContext(context);
                }

                Assert.False(disposed);
            }
            Assert.True(disposed);
        }

        public class SimpleModelContextWithDispose : SimpleModelContext
        {
            private readonly DbConnection _connection;

            public SimpleModelContextWithDispose(DbConnection connection)
                : base(connection, contextOwnsConnection: false)
            {
                _connection = connection;
            }

            protected override void Dispose(bool disposing)
            {
                _connection.Dispose();
                base.Dispose(disposing);
            }
        }

        [Fact]
        public void Overriding_Dispose_can_be_used_to_dispose_a_passed_DbConnection()
        {
            var disposed = false;
            var connection = SimpleConnection<SimpleModelContextWithDispose>();

            connection.Disposed += (_, __) => disposed = true;

            using (var context = new SimpleModelContextWithDispose(connection))
            {
                context.Products.FirstOrDefault();
            }

            Assert.True(disposed);
        }

        [Fact]
        public void Existing_connection_is_disposed_after_use_if_DbContext_owns_connection()
        {
            var disposed = false;
            var connection = SimpleConnection<SimpleModelContext>();

            connection.Disposed += (_, __) => disposed = true;

            using (var context = new SimpleModelContext(connection, contextOwnsConnection: true))
            {
                context.Products.FirstOrDefault();
            }

            Assert.True(disposed);
        }

        public class SimpleModelEntityConnectionContext : SimpleModelContext
        {
            public SimpleModelEntityConnectionContext(EntityConnection entityConnection, bool contextOwnsConnection = false)
                : base(entityConnection, contextOwnsConnection)
            {
            }
        }

        [Fact]
        public void Existing_EntityConnection_is_disposed_after_use_if_DbContext_owns_connection()
        {
            var disposed = false;
            var connection = new EntityConnection(SimpleModelEntityConnectionString);

            connection.Disposed += (_, __) => disposed = true;

            using (var context = new SimpleModelEntityConnectionContext(connection, contextOwnsConnection: true))
            {
                VerifySimpleModelContext(context);
            }

            Assert.True(disposed);
        }

        [Fact]
        public void Existing_connection_used_with_an_existing_model_is_disposed_after_use_if_DbContext_owns_connection()
        {
            var connection = SimpleConnection<SimpleModelContext>();

            var model = SimpleModelContext.CreateBuilder().Build(connection).Compile();

            var disposed = false;

            connection.Disposed += (_, __) => disposed = true;

            using (var context = new DbContext(connection, model, contextOwnsConnection: true))
            {
                context.Set<Product>().FirstOrDefault();
            }

            Assert.True(disposed);
        }

        #endregion

        #region Positive ObjectContext connection tests

        [Fact]
        public void DbContext_connection_is_same_as_ObjectContext_connection()
        {
            using (var outerContext = new SimpleModelContext())
            {
                var objectContextConnection =
                    ((EntityConnection)GetObjectContext(outerContext).Connection).StoreConnection;
                using (var context = new SimpleModelContext(GetObjectContext(outerContext)))
                {
                    Assert.Same(objectContextConnection, context.Database.Connection);
                }
            }
        }

        [Fact]
        public void ObjectContext_connection_is_not_disposed_after_use()
        {
            ObjectContext_connection_is_not_disposed_after_use_implementation(dbContextOwnsObjectContext: false);
        }

        [Fact]
        public void
            ObjectContext_connection_is_not_disposed_after_use_when_DbContext_owns_ObjectContext_when_ObjectContext_does_not_own_connection(
            
            )
        {
            ObjectContext_connection_is_not_disposed_after_use_implementation(dbContextOwnsObjectContext: true);
        }

        private void ObjectContext_connection_is_not_disposed_after_use_implementation(bool dbContextOwnsObjectContext)
        {
            var disposed = false;
            using (var outerContext = new SimpleModelContext())
            {
                var connection = outerContext.Database.Connection;
                connection.Disposed += (_, __) => disposed = true;

                var objectContext = GetObjectContext(outerContext);
                using (var context = new SimpleModelContext(objectContext, dbContextOwnsObjectContext))
                {
                    context.Products.FirstOrDefault();
                }

                Assert.False(disposed);
            }
            Assert.True(disposed);
        }

        #endregion

        #region Positive app.config connection tests

        private void EnsureAppConfigDatabaseExists()
        {
            using (var context = new SimpleModelContext("name=SimpleModelInAppConfig"))
            {
                if (!context.Database.Exists())
                {
                    context.Database.Initialize(force: true);
                }
            }
        }

        [Fact]
        public void Connection_name_is_found_in_app_config()
        {
            EnsureAppConfigDatabaseExists();

            using (var context = new SimpleModelContext("SimpleModelInAppConfig"))
            {
                Assert.Equal("SimpleModel.SimpleModel", context.Database.Connection.Database);
                Assert.IsType<SqlConnection>(context.Database.Connection);
            }
        }

        [Fact]
        public void Connection_name_is_found_in_app_config_when_using_name_keyword()
        {
            EnsureAppConfigDatabaseExists();

            using (var context = new SimpleModelContext("name=SimpleModelInAppConfig"))
            {
                Assert.Equal("SimpleModel.SimpleModel", context.Database.Connection.Database);
                Assert.IsType<SqlConnection>(context.Database.Connection);
            }
        }

        [Fact]
        public void Connection_created_from_app_config_is_disposed_after_use()
        {
            EnsureAppConfigDatabaseExists();

            var disposed = false;

            using (var context = new SimpleModelContext("SimpleModelInAppConfig"))
            {
                var connection = context.Database.Connection;
                connection.Disposed += (_, __) => disposed = true;
            }

            Assert.True(disposed);
        }

        [Fact]
        public void EntityConnection_created_from_app_config_is_disposed_after_use()
        {
            var disposed = false;

            using (var context = new EntityConnectionForSimpleModel())
            {
                var connection = context.Database.Connection;
                connection.Disposed += (_, __) => disposed = true;
            }

            Assert.True(disposed);
        }

        #endregion

        #region Positive hard-coded connection string tests

        [Fact]
        public void Hard_coded_connection_string_passed_to_context_uses_default_provider()
        {
            using (var context = new SimpleModelContext(SimpleConnectionString<SimpleModelContext>()))
            {
                Assert.Equal("SimpleModel.SimpleModelContext", context.Database.Connection.Database);
                Assert.IsType<SqlConnection>(context.Database.Connection);
            }
        }

        [Fact]
        public void Connection_created_from_hard_coded_connection_string_is_disposed_after_use()
        {
            var disposed = false;

            using (var context = new SimpleModelContext(SimpleConnectionString<SimpleModelContext>()))
            {
                var connection = context.Database.Connection;
                connection.Disposed += (_, __) => disposed = true;
            }

            Assert.True(disposed);
        }

        [Fact]
        public void EntityConnection_created_from_hard_coded_connection_string_is_disposed_after_use()
        {
            var disposed = false;

            using (var context = new SimpleModelContext(SimpleModelEntityConnectionString))
            {
                var connection = context.Database.Connection;
                connection.Disposed += (_, __) => disposed = true;
            }

            Assert.True(disposed);
        }

        #endregion

        #region Positive IDbConnectionFactory tests

        [Fact]
        public void Default_IDbConnectionFactory_is_used_to_create_connection()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.Equal("SimpleModel.SimpleModelContext", context.Database.Connection.Database);
                Assert.IsType<SqlConnection>(context.Database.Connection);
            }
        }

        [Fact]
        public void Explicit_name_is_used_without_stripping_Context()
        {
            using (var context = new SimpleModelContext("SimpleModel.ExplicitlyNamedContext"))
            {
                Assert.Equal("SimpleModel.ExplicitlyNamedContext", context.Database.Connection.Database);
            }
        }

        [Fact]
        public void By_convention_name_does_not_have_Context_stripped()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.Equal("SimpleModel.SimpleModelContext", context.Database.Connection.Database);
            }
        }

        [Fact]
        public void Context_created_from_factory_is_disposed_after_use()
        {
            var disposed = false;

            using (var context = new SimpleModelContext())
            {
                var connection = context.Database.Connection;
                connection.Disposed += (_, __) => disposed = true;
            }

            Assert.True(disposed);
        }

        #endregion

        #region EntityConnection following opening/closing store connection

        public abstract class OnModelConnectionContext<TContext> : DbContext
            where TContext : DbContext
        {
            static OnModelConnectionContext()
            {
                Database.SetInitializer<TContext>(null);
            }

            protected OnModelConnectionContext()
            {
            }

            protected OnModelConnectionContext(DbConnection existingConnection)
                : base(existingConnection, contextOwnsConnection: true)
            {
            }

            public bool ModelCreated { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                var connection = Database.Connection;

                Assert.Equal(GetType().FullName, connection.Database);

                ModelCreated = true;
            }
        }

        public class OnModelLazyConnectionContext : OnModelConnectionContext<OnModelLazyConnectionContext>
        {
        }

        [Fact]
        public void Accessing_Connection_in_OnModelCreating_does_not_cause_ObjectContext_to_be_created()
        {
            using (var context = new OnModelLazyConnectionContext())
            {
                context.Database.Initialize(force: false);

                Assert.True(context.ModelCreated);
            }
        }

        public class OnModelEagerConnectionContext : OnModelConnectionContext<OnModelEagerConnectionContext>
        {
            public OnModelEagerConnectionContext()
                : base(SimpleConnection<OnModelEagerConnectionContext>())
            {
            }
        }

        [Fact]
        public void Accessing_existing_Connection_in_OnModelCreating_does_not_cause_ObjectContext_to_be_created()
        {
            using (var context = new OnModelEagerConnectionContext())
            {
                context.Database.Initialize(force: false);

                Assert.True(context.ModelCreated);
            }
        }

        public class OnModelConnectionContextWithOpenAndClose :
            OnModelConnectionContext<OnModelConnectionContextWithOpenAndClose>
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                var connection = Database.Connection;

                // Do something intersting with the connection--like create a database
                using (var context = new SimpleModelContext(connection, contextOwnsConnection: false))
                {
                    context.Database.Delete();
                    context.Database.Create();
                }

                // Now do some simple explicit open and close stuff
                connection.Open();

                Assert.Equal(ConnectionState.Open, connection.State);

                connection.Close();

                Assert.Equal(ConnectionState.Closed, connection.State);

                ModelCreated = true;
            }
        }

        [Fact]
        public void Connection_can_be_opened_and_closed_in_OnModelCreating_so_long_as_it_is_left_closed()
        {
            using (var context = new OnModelConnectionContextWithOpenAndClose())
            {
                context.Database.Initialize(force: false);

                Assert.True(context.ModelCreated);
            }
        }

        #endregion

        #region Getting Connection without initializing context

        public class InitializerForNonInitializingConnectionTest : IDatabaseInitializer<ShouldNotBeInitializedContext>
        {
            public static bool WasUsed { get; set; }

            public void InitializeDatabase(ShouldNotBeInitializedContext context)
            {
                // If this method is called it means that at some point context initialization happend,
                // which is expected not to ever happen. 
                WasUsed = true;
            }
        }

        public class ShouldNotBeInitializedContext : SimpleModelContext
        {
            static ShouldNotBeInitializedContext()
            {
                Database.SetInitializer(new InitializerForNonInitializingConnectionTest());
            }

            public ShouldNotBeInitializedContext()
            {
            }

            public ShouldNotBeInitializedContext(string nameOrConnectionString)
                : base(nameOrConnectionString)
            {
            }

            public ShouldNotBeInitializedContext(DbConnection existingConnection)
                : base(existingConnection)
            {
            }

            public ShouldNotBeInitializedContext(ObjectContext objectContext)
                : base(objectContext)
            {
            }
        }

        [Fact]
        public void Connection_can_be_obtained_without_initializing_the_context_when_using_existing_store_connection()
        {
            using (var connection = SimpleConnection<SimpleModelContext>())
            {
                using (var context = new ShouldNotBeInitializedContext(connection))
                {
                    var readConnection = context.Database.Connection;
                    Assert.False(InitializerForNonInitializingConnectionTest.WasUsed);
                }
            }
        }

        [Fact]
        public void Connection_can_be_obtained_without_initializing_the_context_when_using_existing_EntityConnection()
        {
            using (var connection = new EntityConnection(SimpleModelEntityConnectionString))
            {
                using (var context = new ShouldNotBeInitializedContext(connection))
                {
                    var readConnection = context.Database.Connection;
                    Assert.False(InitializerForNonInitializingConnectionTest.WasUsed);
                }
            }
        }

        [Fact]
        public void
            Connection_can_be_obtained_without_initializing_the_context_when_using_connection_created_by_convention()
        {
            using (var context = new ShouldNotBeInitializedContext())
            {
                var readConnection = context.Database.Connection;
                Assert.False(InitializerForNonInitializingConnectionTest.WasUsed);
            }
        }

        [Fact]
        public void Connection_can_be_obtained_without_initializing_the_context_when_using_connection_built_from_connection_string()
        {
            using (var context = new ShouldNotBeInitializedContext(SimpleConnectionString<SimpleModelContext>()))
            {
                var readConnection = context.Database.Connection;
                Assert.False(InitializerForNonInitializingConnectionTest.WasUsed);
            }
        }

        [Fact]
        public void Connection_can_be_obtained_without_initializing_the_context_when_using_EntityConnection_built_from_connection_string()
        {
            using (var context = new ShouldNotBeInitializedContext(SimpleModelEntityConnectionString))
            {
                var readConnection = context.Database.Connection;
                Assert.False(InitializerForNonInitializingConnectionTest.WasUsed);
            }
        }

        [Fact]
        public void Connection_can_be_obtained_without_initializing_the_context_when_using_connection_from_app_config()
        {
            EnsureAppConfigDatabaseExists();

            using (var context = new ShouldNotBeInitializedContext("name=SimpleModelInAppConfig"))
            {
                var readConnection = context.Database.Connection;
                Assert.False(InitializerForNonInitializingConnectionTest.WasUsed);
            }
        }

        [Fact]
        public void
            Connection_can_be_obtained_without_initializing_the_context_when_using_EntityConnection_from_app_config()
        {
            using (var context = new ShouldNotBeInitializedContext("name=EntityConnectionForSimpleModel"))
            {
                var readConnection = context.Database.Connection;
                Assert.False(InitializerForNonInitializingConnectionTest.WasUsed);
            }
        }

        private void Connection_can_be_obtained_after_initializing_the_context(DbContext context)
        {
            context.Database.Initialize(force: false);

            var readConnection = context.Database.Connection;

            Assert.True(readConnection.ConnectionString.Contains("SimpleModel"));
        }

        [Fact]
        public void Connection_can_be_obtained_after_initializing_the_context_when_using_existing_store_connection()
        {
            using (var connection = SimpleConnection<SimpleModelContext>())
            {
                using (var context = new SimpleModelContext(connection))
                {
                    Connection_can_be_obtained_after_initializing_the_context(context);
                }
            }
        }

        [Fact]
        public void Connection_can_be_obtained_after_initializing_the_context_when_using_existing_EntityConnection()
        {
            using (var connection = new EntityConnection(SimpleModelEntityConnectionString))
            {
                using (var context = new SimpleModelContextForModelFirst(connection))
                {
                    Connection_can_be_obtained_after_initializing_the_context(context);
                }
            }
        }

        [Fact]
        public void
            Connection_can_be_obtained_after_initializing_the_context_when_using_connection_created_by_convention()
        {
            using (var context = new SimpleModelContext())
            {
                Connection_can_be_obtained_after_initializing_the_context(context);
            }
        }

        [Fact]
        public void
            Connection_can_be_obtained_after_initializing_the_context_when_using_connection_built_from_connection_string
            ()
        {
            using (var context = new SimpleModelContext(SimpleConnectionString<SimpleModelContext>()))
            {
                Connection_can_be_obtained_after_initializing_the_context(context);
            }
        }

        [Fact]
        public void
            Connection_can_be_obtained_after_initializing_the_context_when_using_EntityConnection_built_from_connection_string
            ()
        {
            using (var context = new SimpleModelContextForModelFirst(SimpleModelEntityConnectionString))
            {
                Connection_can_be_obtained_after_initializing_the_context(context);
            }
        }

        [Fact]
        public void Connection_can_be_obtained_after_initializing_the_context_when_using_connection_from_app_config()
        {
            EnsureAppConfigDatabaseExists();

            using (var context = new SimpleModelContext("name=SimpleModelInAppConfig"))
            {
                Connection_can_be_obtained_after_initializing_the_context(context);
            }
        }

        [Fact]
        public void
            Connection_can_be_obtained_after_initializing_the_context_when_using_EntityConnection_from_app_config()
        {
            using (var context = new SimpleModelContextForModelFirst("name=EntityConnectionForSimpleModel"))
            {
                Connection_can_be_obtained_after_initializing_the_context(context);
            }
        }

        [Fact]
        public void Connection_can_be_obtained_from_DbContext_created_with_existing_ObjectContext()
        {
            using (var outerContext = new SimpleModelContext())
            {
                using (var context = new SimpleModelContext(GetObjectContext(outerContext)))
                {
                    Connection_can_be_obtained_after_initializing_the_context(context);
                }
            }
        }

        #endregion

        #region Exception thrown for bad connection

        public class BadMvcContext : DbContext
        {
        }

        // [Fact(Skip = @"DbConnection timeout issue.")]
        // Ignored for now because even with Connection Timeout=1 the connection to a bad server
        // can still take over 20 seconds to fail. This is apparently because of the way timeouts work on
        // SqlConnection which is that the timeout specified is a minimum value and if the APIs being called
        // just take longer to fail than the timeout then so be it.
        // The code is left in place so it can be run manually to check this is working correctly anytime the
        // GetProviderManifestTokenChecked method is changed.
        public void Useful_exception_is_thrown_if_model_creation_happens_with_bad_MVC4_connection_string()
        {
            try
            {
                MutableResolver.AddResolver<IDbConnectionFactory>(
                    k => new SqlConnectionFactory(
                             "Data Source=(localdb)\v11.0; Integrated Security=True; MultipleActiveResultSets=True; Connection Timeout=1;"));

                using (var context = new BadMvcContext())
                {
                    Assert.Throws<ProviderIncompatibleException>(() => context.Database.Initialize(force: false)).ValidateMessage(
                        "BadLocalDBDatabaseName");
                }
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }
        }

        public class BadConnectionStringContext : DbContext
        {
            public BadConnectionStringContext()
                : base("Data Source=SomeServerThatSurelyDoesntExist; Database=Bingo; Connection Timeout=1")
            {
            }
        }

        // [Fact(Skip = @"DbConnection timeout issue.")]
        // Ignored for now because even with Connection Timeout=1 the connection to a bad server
        // can still take over 20 seconds to fail. This is apparently because of the way timeouts work on
        // SqlConnection which is that the timeout specified is a minimum value and if the APIs being called
        // just take longer to fail than the timeout then so be it.
        // The code is left in place so it can be run manually to check this is working correctly anytime the
        // GetProviderManifestTokenChecked method is changed.
        public void Useful_exception_is_thrown_if_model_creation_happens_with_general_bad_connection_string()
        {
            using (var context = new BadConnectionStringContext())
            {
                Assert.Throws<ProviderIncompatibleException>(() => context.Database.Initialize(force: false)).ValidateMessage(
                    "FailedToGetProviderInformation");
            }
        }

        #endregion
    }
}
