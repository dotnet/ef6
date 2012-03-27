namespace ProductivityApiTests
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.EntityClient;
    using System.Data.Objects;
    using System.Linq;
    using System.Transactions;
    using SimpleModel;
    using Xunit;

    public class DisposeTests : FunctionalTestBase
    {
        #region Infrastructure/setup

        /// <summary>
        /// Asserts that an operation on a set from a disposed context throws, but only when the operation is
        /// executed, not when the set is created.
        /// </summary>
        /// <param name="entityType">The type of entity for which the set is for.</param>
        /// <param name="test">The test to run on the set.</param>
        private void ThrowsWithDisposedContextNonGeneric(Type entityType, Action<DbSet> test)
        {
            var context = CreateDisposedContext();
            // Does not throw even though context is disposed.
            var set = context.Set(entityType);
            // Now throws
            Assert.Throws<InvalidOperationException>(() => test(set)).ValidateMessage("DbContext_Disposed");
        }

        /// <summary>
        /// Asserts that an operation on a set from a disposed context throws, but only when the operation is
        /// executed, not when the set is created.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity for which a set should be created.</typeparam>
        /// <param name="test">The test to run on the set.</param>
        private void ThrowsWithDisposedContext<TEntity>(Action<IDbSet<TEntity>> test) where TEntity : class
        {
            var context = CreateDisposedContext();
            // Does not throw even though context is disposed.
            var set = context.Set<TEntity>();
            // Now throws
            Assert.Throws<InvalidOperationException>(() => test(set)).ValidateMessage("DbContext_Disposed");
        }

        /// <summary>
        /// Creates a disposed context.
        /// </summary>
        private SimpleModelContext CreateDisposedContext()
        {
            SimpleModelContext context;
            using (context = new SimpleModelContext())
            {
                // Ensures that the context actually gets initialized.
                var objectContext = GetObjectContext(context);
            }
            return context;
        }

        /// <summary>
        /// Asserts that an operation on a disposed context throws.
        /// </summary>
        /// <param name="test">The test to run on the context.</param>
        private void ThrowsWithDisposedContext(Action<SimpleModelContext> test)
        {
            var context = CreateDisposedContext();
            Assert.Throws<InvalidOperationException>(() => test(context)).ValidateMessage("DbContext_Disposed");
        }

        #endregion

        #region Dispose tests

        [Fact]
        public void DbContext_construction_throws_for_disposed_object_context()
        {
            ObjectContext objectContext;

            using (var context = new SimpleModelContext())
            {
                objectContext = GetObjectContext(context);
            }

            Assert.Throws<ObjectDisposedException>(() => new SimpleModelContext(objectContext)).ValidateMessage(
                SystemDataEntityAssembly, "ObjectContext_ObjectDisposed");
        }

        [Fact]
        public void SaveChanges_throws_on_disposed_context()
        {
            ThrowsWithDisposedContext(context => context.SaveChanges());
        }

        [Fact]
        public void ObjectContext_property_throws_on_disposed_context()
        {
            ThrowsWithDisposedContext(context => { var oc = GetObjectContext(context); });
        }

        [Fact]
        public void Connection_property_throws_on_disposed_context()
        {
            ThrowsWithDisposedContext(context => { var conn = context.Database.Connection; });
        }

        [Fact]
        public void Dispose_is_noop_if_context_already_disposed()
        {
            SimpleModelContext context;
            using (context = new SimpleModelContext())
            {
                var product = context.Products.Find(1);
            }
            context.Dispose();
        }

        public class ContextForDisposeTests : SimpleModelContext
        {
            public ContextForDisposeTests()
            {
            }

            public ContextForDisposeTests(string nameOrConnectionString)
                : base(nameOrConnectionString)
            {
            }

            public ContextForDisposeTests(DbConnection existingConnection, bool contextOwnsConnection = false)
                : base(existingConnection, contextOwnsConnection)
            {
            }
        }

        [Fact]
        public void Dispose_disposes_underlying_ObjectContext_when_created_by_code_first_convention()
        {
            Dispose_disposes_underlying_ObjectContext_if_DbContext_owns_it(() => new ContextForDisposeTests());
        }

        [Fact]
        public void Dispose_disposes_underlying_ObjectContext_created_with_entity_connection_string()
        {
            Dispose_disposes_underlying_ObjectContext_if_DbContext_owns_it(() => new ContextForDisposeTests(
                                                                                     SimpleModelEntityConnectionString.
                                                                                         Replace(
                                                                                             DefaultDbName
                                                                                                 <SimpleModelContext>(),
                                                                                             DefaultDbName
                                                                                                 <ContextForDisposeTests
                                                                                                 >())));
        }

        [Fact]
        public void Dispose_disposes_underlying_ObjectContext_created_with_EntityConnection()
        {
            Dispose_disposes_underlying_ObjectContext_if_DbContext_owns_it(
                () =>
                new ContextForDisposeTests(new EntityConnection(SimpleModelEntityConnectionString),
                                           contextOwnsConnection: true));
        }

        private void Dispose_disposes_underlying_ObjectContext_if_DbContext_owns_it(
            Func<SimpleModelContext> createContext)
        {
            EnsureDatabaseInitialized(() => new ContextForDisposeTests());
            ObjectContext objectContext;
            DbConnection storeConnection;
            using (var context = createContext())
            {
                context.Products.Find(1);
                objectContext = GetObjectContext(context);
                storeConnection = context.Database.Connection;
            }
            Assert.Throws<ObjectDisposedException>(() => objectContext.SaveChanges()).ValidateMessage(
                SystemDataEntityAssembly, "ObjectContext_ObjectDisposed");
            Assert.True(storeConnection.State == ConnectionState.Closed &&
                        storeConnection.ConnectionString.Equals(string.Empty));
        }

        [Fact]
        public void Dispose_does_not_dispose_underlying_ObjectContext_if_DbContext_does_not_own_it()
        {
            using (new TransactionScope())
            {
                using (var outerContext = new SimpleModelContext())
                {
                    var objectContext = GetObjectContext(outerContext);
                    using (var context = new SimpleModelContext(objectContext))
                    {
                        var product = context.Products.Find(1);
                    }
                    // Should not throw since objectContext should not be disposed
                    objectContext.SaveChanges();
                }
            }
        }

        [Fact]
        public void Dispose_disposes_underlying_ObjectContext_created_externally_if_DbContext_owns_it()
        {
            var objectContext = GetObjectContext(new SimpleModelContext());
            using (var context = new SimpleModelContext(objectContext, dbContextOwnsObjectContext: true))
            {
                var product = context.Products.Find(1);
            }

            Assert.Throws<ObjectDisposedException>(() => objectContext.SaveChanges()).ValidateMessage(
                SystemDataEntityAssembly, "ObjectContext_ObjectDisposed");
        }

        [Fact]
        public void Dispose_disposes_underlying_ObjectContext_if_DbContext_owns_it_when_constructed_using_DbCompiledModel_constructor()
        {
            // Arrange
            var builder = new DbModelBuilder();
            ObjectContext objectContext = null;
            DbConnection storeConnection = null;
            using (
                var context =
                    new SimpleModelContextWithNoData(builder.Build(ProviderRegistry.Sql2008_ProviderInfo).Compile()))
            {
                objectContext = GetObjectContext(context);
                storeConnection = context.Database.Connection;
            }

            // Assert
            Assert.Throws<ObjectDisposedException>(() => objectContext.SaveChanges()).ValidateMessage(
                SystemDataEntityAssembly, "ObjectContext_ObjectDisposed");
            Assert.True(storeConnection.State == ConnectionState.Closed &&
                        storeConnection.ConnectionString.Equals(string.Empty));
        }

        [Fact]
        public void Disposing_DbContext_constructed_using_database_name_connection_string_ctor_will_dispose_underlying_Object_Context_and_connection()
        {
            DbContext_should_dispose_underlying_context_and_connection_if_it_does_not_own_it(
                ConnectionStringFormat.DatabaseName);
        }

        [Fact]
        public void DbContext_construction_using_named_connection_string_constructor_will_dispose_underlying_Object_Context_and_connection()
        {
            DbContext_should_dispose_underlying_context_and_connection_if_it_does_not_own_it(
                ConnectionStringFormat.NamedConnectionString);
        }

        [Fact]
        public void DbContext_construction_using_provider_connection_string_constructor_will_dispose_underlying_Object_Context_and_connection()
        {
            DbContext_should_dispose_underlying_context_and_connection_if_it_does_not_own_it(
                ConnectionStringFormat.ProviderConnectionString);
        }

        [Fact]
        public void DbContext_construction_using_database_name_and_db_model_constructor_will_dispose_underlying_Object_Context_and_connection()
        {
            DbContext_should_dispose_underlying_context_and_connection_if_it_does_not_own_it(
                ConnectionStringFormat.DatabaseName,
                new DbModelBuilder().Build(ProviderRegistry.Sql2008_ProviderInfo).Compile());
        }

        [Fact]
        public void DbContext_construction_using_named_connection_string_db_model_constructor_will_dispose_underlying_Object_Context_and_connection()
        {
            DbContext_should_dispose_underlying_context_and_connection_if_it_does_not_own_it(
                ConnectionStringFormat.DatabaseName,
                new DbModelBuilder().Build(ProviderRegistry.Sql2008_ProviderInfo).Compile());
        }

        [Fact]
        public void DbContext_construction_using_provider_connection_string_and_db_model_constructor_will_dispose_underlying_Object_Context_and_connection()
        {
            DbContext_should_dispose_underlying_context_and_connection_if_it_does_not_own_it(
                ConnectionStringFormat.DatabaseName,
                new DbModelBuilder().Build(ProviderRegistry.Sql2008_ProviderInfo).Compile());
        }

        private void DbContext_should_dispose_underlying_context_and_connection_if_it_does_not_own_it(
            ConnectionStringFormat connStringFormat, DbCompiledModel model = null)
        {
            // Arrange
            string connectionString = null;
            switch (connStringFormat)
            {
                case ConnectionStringFormat.DatabaseName:
                    connectionString = DefaultDbName<SimpleModelContextWithNoData>();
                    break;
                case ConnectionStringFormat.NamedConnectionString:
                    connectionString = "SimpleModelWithNoDataFromAppConfig";
                    break;
                case ConnectionStringFormat.ProviderConnectionString:
                    connectionString = SimpleConnectionString<SimpleModelContextWithNoData>();
                    break;
                default:
                    throw new ArgumentException("Invalid Connection String Format specified"
                                                + connStringFormat);
            }

            ObjectContext objectContext = null;
            DbConnection connection = null;

            // Act
            using (
                var context = model == null
                                  ? new SimpleModelContextWithNoData(connectionString)
                                  : new SimpleModelContextWithNoData(connectionString, model))
            {
                objectContext = GetObjectContext(context);
                connection = context.Database.Connection;
            }

            // Assert
            Assert.Throws<ObjectDisposedException>(() => objectContext.SaveChanges()).ValidateMessage(
                SystemDataEntityAssembly, "ObjectContext_ObjectDisposed");
            Assert.True(connection.State == ConnectionState.Closed && connection.ConnectionString.Equals(string.Empty));
        }

        [Fact]
        public void Dispose_does_not_dispose_underlying_connection_if_DbContext_does_not_own_it()
        {
            using (var connection = SimpleConnection<SimpleModelContext>())
            {
                // Arrange
                var connectionDisposed = false;
                connection.Disposed += (sender, e) => { connectionDisposed = true; };

                // Act
                using (var context = new SimpleModelContext(connection, contextOwnsConnection: false))
                {
                    var product = context.Products.Find(1);
                }

                // Assert
                Assert.False(connectionDisposed);
            }
        }

        [Fact]
        public void Dispose_disposes_underlying_connection_created_externally_if_DbContext_owns_it()
        {
            // Arrange
            var connectionDisposed = false;
            var connection = SimpleConnection<SimpleModelContext>();
            connection.Disposed += (sender, e) => { connectionDisposed = true; };

            // Act
            using (var context = new SimpleModelContext(connection, contextOwnsConnection: true))
            {
                var product = context.Products.Find(1);
            }

            // Assert
            Assert.True(connectionDisposed);
        }

        [Fact]
        public void Set_throws_only_when_used_if_context_is_disposed()
        {
            ThrowsWithDisposedContext<FeaturedProduct>(set => set.FirstOrDefault());
        }

        [Fact]
        public void Non_generic_Set_throws_only_when_used_if_context_is_disposed()
        {
            ThrowsWithDisposedContextNonGeneric(typeof(FeaturedProduct),
                                                set => set.Cast<FeaturedProduct>().FirstOrDefault());
        }

        [Fact]
        public void Add_throws_when_context_is_disposed()
        {
            ThrowsWithDisposedContext<Product>(set => set.Add(new Product() { Name = "Fruity Sauce" }));
        }

        [Fact]
        public void Non_generic_Add_throws_when_context_is_disposed()
        {
            ThrowsWithDisposedContextNonGeneric(typeof(Product), set => set.Add(new Product() { Name = "Fruity Sauce" }));
        }

        [Fact]
        public void Attach_throws_when_context_is_disposed()
        {
            ThrowsWithDisposedContext<Product>(set => set.Attach(new Product() { Name = "Fruity Sauce" }));
        }

        [Fact]
        public void Non_generic_Attach_throws_when_context_is_disposed()
        {
            ThrowsWithDisposedContextNonGeneric(typeof(Product),
                                                set => set.Attach(new Product() { Name = "Fruity Sauce" }));
        }

        [Fact]
        public void Remove_throws_when_context_is_disposed()
        {
            ThrowsWithDisposedContext<Product>(set => set.Remove(new Product() { Name = "Fruity Sauce" }));
        }

        [Fact]
        public void Non_generic_Remove_throws_when_context_is_disposed()
        {
            ThrowsWithDisposedContextNonGeneric(typeof(Product),
                                                set => set.Remove(new Product() { Name = "Fruity Sauce" }));
        }

        #endregion
    }
}