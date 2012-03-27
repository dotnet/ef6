namespace System.Data.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Common;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.Infrastructure;
    using System.Data.EntityClient;
    using System.Data.Metadata.Edm;
    using System.Data.Objects;
    using System.Data.SqlClient;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Transactions;

    public class TestBase
    {
        internal DbDatabaseMapping BuildMapping(DbModelBuilder modelBuilder)
        {
            // Build and clone to check for idempotency issues.

            modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            var clone = modelBuilder.Clone();

            return clone.Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping;
        }

        internal DbDatabaseMapping BuildCeMapping(DbModelBuilder modelBuilder)
        {
            // Build and clone to check for idempotency issues.

            modelBuilder.Build(ProviderRegistry.SqlCe4_ProviderInfo);

            var clone = modelBuilder.Clone();

            return clone.Build(ProviderRegistry.SqlCe4_ProviderInfo).DatabaseMapping;
        }

        #region Assemblies and exceptions

        /// <summary>
        /// The assembly containing Code First and the Productivity API.
        /// </summary>
        public static Assembly CodeFirstAssembly
        {
            get { return typeof(DbModelBuilder).Assembly; }
        }

        /// <summary>
        /// System.Data.Entity.
        /// </summary>
        public static Assembly SystemDataEntityAssembly
        {
            get { return typeof(ObjectContext).Assembly; }
        }

        /// <summary>
        /// System.ComponentModel.DataAnnotations.
        /// </summary>
        public static Assembly SystemComponentModelDataAnnotationsAssembly
        {
            get { return typeof(ValidationAttribute).Assembly; }
        }

        /// <summary>
        /// Gets an embedded resource string from the specified assembly
        /// </summary>
        public static string LookupString(Assembly assembly, string resourceTable, string resourceKey)
        {
            return new AssemblyResourceLookup(assembly, resourceTable).LookupString(resourceKey);
        }

        /// <summary>
        /// Executes the given delegate and returns the exception that it throws.
        /// </summary>
        protected Exception GenerateException(Action willThrow)
        {
            return ExceptionHelpers.GenerateException(willThrow);
        }

        /// <summary>
        /// Executes the given test multiple times in parallel.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="count">The number of copies to run in parallel.</param>
        protected void ExecuteInParallel(Action test, int count = 20)
        {
            var tests = new Action[count];
            for (var i = 0; i < count; i++)
            {
                tests[i] = test;
            }
            Parallel.Invoke(tests);
        }

        #endregion
        
        #region GetObjectContext

        /// <summary>
        ///     Returns the ObjectContext for the given DbContext.
        /// </summary>
        public static ObjectContext GetObjectContext(DbContext context)
        {
            return ((IObjectContextAdapter)context).ObjectContext;
        }

        #endregion

        #region State entry helpers

        /// <summary>
        /// Gets all GetStateEntries for the given DbContext,.
        /// </summary>
        /// <param name="dbContext">A DbContext instance.</param>
        /// <returns>All state entries in the ObjectStateManager.</returns>
        protected static IEnumerable<ObjectStateEntry> GetStateEntries(DbContext dbContext)
        {
            return ModelHelpers.GetStateEntries(dbContext);
        }

        /// <summary>
        /// Gets all GetStateEntries for the given ObjectContext,.
        /// </summary>
        /// <param name="objectContext">A ObjectContext instance.</param>
        /// <returns>All state entries in the ObjectStateManager.</returns>
        protected static IEnumerable<ObjectStateEntry> GetStateEntries(ObjectContext objectContext)
        {
            return ModelHelpers.GetStateEntries(objectContext);
        }

        /// <summary>
        /// Gets the ObjectStateEntry for the given entity in the given DbContext.
        /// </summary>
        /// <param name="dbContext">A DbContext instance.</param>
        /// <param name="entity">The entity to lookup.</param>
        /// <returns>The ObjectStateEntry.</returns>
        protected static ObjectStateEntry GetStateEntry(DbContext dbContext, object entity)
        {
            return ModelHelpers.GetStateEntry(dbContext, entity);
        }

        /// <summary>
        /// Gets the ObjectStateEntry for the given entity in the given ObjectContext.
        /// </summary>
        /// <param name="objectContext">A ObjectContext instance.</param>
        /// <param name="entity">The entity to lookup.</param>
        /// <returns>The ObjectStateEntry.</returns>
        protected static ObjectStateEntry GetStateEntry(ObjectContext objectContext, object entity)
        {
            return ModelHelpers.GetStateEntry(objectContext, entity);
        }

        /// <summary>
        /// Asserts that there's no ObjectStateEntry for the given entity in the given DbContext.
        /// </summary>
        /// <param name="dbContext">A DbContext instance.</param>
        /// <param name="entity">The entity to lookup.</param>
        public static void AssertNoStateEntry(DbContext dbContext, object entity)
        {
            ModelHelpers.AssertNoStateEntry(dbContext, entity);
        }

        /// <summary>
        /// Asserts that there's no ObjectStateEntry for the given entity in the given ObjectContext.
        /// </summary>
        /// <param name="objectContext">A ObjectContext instance.</param>
        /// <param name="entity">The entity to lookup.</param>
        public static void AssertNoStateEntry(ObjectContext objectContext, object entity)
        {
            ModelHelpers.AssertNoStateEntry(objectContext, entity);
        }

        #endregion

        #region Connection helpers

        /// <summary>
        /// Returns a simple SQL Server connection string to the local machine with the given database name.
        /// </summary>
        /// <param name="databaseName">The database name.</param>
        /// <returns>The connection string.</returns>
        protected static string SimpleConnectionString(string databaseName)
        {
            return ModelHelpers.SimpleConnectionString(databaseName);
        }

        /// <summary>
        /// Returns the default name that will be created for the context of the given type.
        /// </summary>
        /// <typeparam name="TContext">The type of the context to create a name for.</typeparam>
        /// <returns>The name.</returns>
        protected static string DefaultDbName<TContext>() where TContext : DbContext
        {
            return ModelHelpers.DefaultDbName<TContext>();
        }

        /// <summary>
        /// Returns the transaction count from the server.
        /// </summary>
        /// <param name="connection">Database connection to connect against.</param>
        /// <returns>The transaction count.</returns>
        protected int GetTransactionCount(DbConnection connection)
        {
            bool closeconn = false;

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
                closeconn = true;
            }

            var cmd = connection.CreateCommand();
            cmd.CommandText = "select @@TranCount";
            int trancount = (int)cmd.ExecuteScalar();

            if (closeconn)
            {
                connection.Close();
            }

            return trancount;
        }

        /// <summary>
        /// Returns a local transaction.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected DbTransaction BeginLocalTransaction(DbContext context)
        {
            return OpenEntityConnection(context).BeginTransaction();
        }

        /// <summary>
        /// Opens the underlying <see cref="EntityConnection"/> obtained from the underlying
        /// <see cref="ObjectContext"/> and creates a new <see cref="CommittableTransaction"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected CommittableTransaction BeginCommittableTransaction(DbContext context)
        {
            OpenEntityConnection(context);

            return new CommittableTransaction();
        }

        /// <summary>
        /// Opens the underlying <see cref="EntityConnection"/> obtained from the underlying
        /// <see cref="ObjectContext"/> and returns it.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The connection.</returns>
        private static EntityConnection OpenEntityConnection(DbContext context)
        {
            var connection = (EntityConnection)((IObjectContextAdapter)context).ObjectContext.Connection;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            return connection;
        }

        /// <summary>
        /// Closes the underlying <see cref="EntityConnection"/> obtained from the underlying
        /// <see cref="ObjectContext"/> if the connection is open.
        /// </summary>
        protected void CloseEntityConnection(DbContext context)
        {
            var connection = ((IObjectContextAdapter)context).ObjectContext.Connection;
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        /// <summary>
        /// Returns a simple SQL Server connection string to the local machine for the given context type.
        /// </summary>
        /// <typeparam name="TContext">The type of the context to create a connection string for.</typeparam>
        /// <returns>The connection string.</returns>
        protected static string SimpleConnectionString<TContext>() where TContext : DbContext
        {
            return ModelHelpers.SimpleConnectionString<TContext>();
        }

        /// <summary>
        /// Returns a simple SQL Server connection string to the local machine using attached database for the given context type.
        /// </summary>
        /// <typeparam name="TContext">The type of the context to create a connection string for.</typeparam>
        /// <returns>The connection string.</returns>
        protected static string SimpleAttachConnectionString<TContext>() where TContext : DbContext
        {
            return ModelHelpers.SimpleAttachConnectionString<TContext>();
        }

        /// <summary>
        /// Returns a simple SQLCE connection string to the local machine for the given context type.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <returns>The connection string.</returns>
        protected static string SimpleCeConnectionString<TContext>() where TContext : DbContext
        {
            return ModelHelpers.SimpleCeConnectionString<TContext>();
        }

        /// <summary>
        /// Returns a simple SQL Server connection to the local machine for the given context type.
        /// </summary>
        /// <typeparam name="TContext">The type of the context to create a connection for.</typeparam>
        /// <returns>The connection.</returns>
        protected static SqlConnection SimpleConnection<TContext>() where TContext : DbContext
        {
            return ModelHelpers.SimpleConnection<TContext>();
        }

        /// <summary>
        /// Returns a simple SQL CE connection for the given context type.
        /// </summary>
        /// <typeparam name="TContext">The type of the context to create a connection for.</typeparam>
        /// <returns>The connection.</returns>
        protected static DbConnection SimpleCeConnection<TContext>() where TContext : DbContext
        {
            return ModelHelpers.SimpleCeConnection<TContext>();
        }
        #endregion

        #region Entity set name helpers

        /// <summary>
        /// Gets the entity set name for the given CLR type, assuming no MEST.
        /// </summary>
        /// <param name="dbContext">The context to look in.</param>
        /// <param name="clrType">The type to lookup.</param>
        /// <returns>The entity set name.</returns>
        protected static string GetEntitySetName(DbContext dbContext, Type clrType)
        {
            return ModelHelpers.GetEntitySetName(dbContext, clrType);
        }

        /// <summary>
        /// Gets the entity set name for the given CLR type, assuming no MEST.
        /// </summary>
        /// <param name="objetContext">The context to look in.</param>
        /// <param name="clrType">The type to lookup.</param>
        /// <returns>The entity set name.</returns>
        protected static string GetEntitySetName(ObjectContext objetContext, Type clrType)
        {
            return ModelHelpers.GetEntitySetName(objetContext, clrType);
        }

        #endregion

        #region Entity Type helpers

        /// <summary>
        /// Gets the EntityType of the given CLR type
        /// </summary>
        /// <param name="dbContext">The context to look in.</param>
        /// <param name="clrType">The CLR type</param>
        /// <returns>The entity type corresponding to the CLR type</returns>
        protected static EntityType GetEntityType(DbContext dbContext, Type clrType)
        {
            return ModelHelpers.GetEntityType(dbContext, clrType);
        }

        /// <summary>
        /// Gets the EntityType of the given CLR type
        /// </summary>
        /// <param name="objectContext">The context to look in.</param>
        /// <param name="clrType">The CLR type</param>
        /// <returns>The entity type corresponding to the CLR type</returns>
        protected static EntityType GetEntityType(ObjectContext objectContext, Type clrType)
        {
            return ModelHelpers.GetStructuralType<EntityType>(objectContext, clrType);
        }

        #endregion
    }
}
