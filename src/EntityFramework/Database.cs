// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An instance of this class is obtained from an <see cref="DbContext" /> object and can be used
    /// to manage the actual database backing a DbContext or connection.
    /// This includes creating, deleting, and checking for the existence of a database.
    /// Note that deletion and checking for existence of a database can be performed using just a
    /// connection (i.e. without a full context) by using the static methods of this class.
    /// </summary>
    public class Database
    {
        #region Fields and constructors

        // The default factory object used to create a DbConnection from a database name.
        private static readonly Lazy<IDbConnectionFactory> _defaultDefaultConnectionFactory =
            new Lazy<IDbConnectionFactory>(
                () => AppConfig.DefaultInstance.TryGetDefaultConnectionFactory() ?? new SqlConnectionFactory(), isThreadSafe: true);

        private static volatile Lazy<IDbConnectionFactory> _defaultConnectionFactory = _defaultDefaultConnectionFactory;

        // The context that backs this instance.
        private readonly InternalContext _internalContext;

        /// <summary>
        /// Creates a Database backed by the given context.  This object can be used to create a database,
        /// check for database existence, and delete a database.
        /// </summary>
        internal Database(InternalContext internalContext)
        {
            DebugCheck.NotNull(internalContext);

            _internalContext = internalContext;
        }

        #endregion

        #region Transactions

        /// <summary>
        /// Enables the user to pass in a database transaction created outside of the <see cref="Database" /> object
        /// if you want the Entity Framework to execute commands within that external transaction.
        /// Alternatively, pass in null to clear the framework's knowledge of that transaction.
        /// </summary>
        /// <param name="transaction">the external transaction</param>
        /// <exception cref="InvalidOperationException">Thrown if the transaction is already completed</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the connection associated with the <see cref="Database" /> object is already enlisted in a
        /// <see
        ///     cref="System.Transactions.TransactionScope" />
        /// transaction
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the connection associated with the <see cref="Database" /> object is already participating in a transaction
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown if the connection associated with the transaction does not match the Entity Framework's connection</exception>
        public void UseTransaction(DbTransaction transaction)
        {
            ((EntityConnection)_internalContext.GetObjectContextWithoutDatabaseInitialization().Connection).UseStoreTransaction(transaction);
        }

        /// <summary>
        /// Begins a transaction on the underlying store connection
        /// </summary>
        /// <returns>
        /// a <see cref="DbContextTransaction" /> object wrapping access to the underlying store's transaction object
        /// </returns>
        public DbContextTransaction BeginTransaction()
        {
            return new DbContextTransaction((EntityConnection)_internalContext.ObjectContext.Connection);
        }

        /// <summary>
        /// Begins a transaction on the underlying store connection using the specified isolation level
        /// </summary>
        /// <returns>
        /// a <see cref="DbContextTransaction" /> object wrapping access to the underlying store's transaction object
        /// </returns>
        public DbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return new DbContextTransaction((EntityConnection)_internalContext.ObjectContext.Connection, isolationLevel);
        }

        #endregion

        #region Connection

        /// <summary>
        /// Returns the connection being used by this context.  This may cause the
        /// connection to be created if it does not already exist.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the context has been disposed.</exception>
        public DbConnection Connection
        {
            get { return _internalContext.Connection; }
        }

        #endregion

        #region Database creation strategy and seed data

        /// <summary>
        /// Sets the database initializer to use for the given context type.  The database initializer is called when a
        /// the given <see cref="DbContext" /> type is used to access a database for the first time.
        /// The default strategy for Code First contexts is an instance of <see cref="CreateDatabaseIfNotExists{TContext}" />.
        /// </summary>
        /// <typeparam name="TContext"> The type of the context. </typeparam>
        /// <param name="strategy"> The initializer to use, or null to disable initialization for the given context type. </param>
        public static void SetInitializer<TContext>(IDatabaseInitializer<TContext> strategy) where TContext : DbContext
        {
            DbConfigurationManager.Instance.EnsureLoadedForContext(typeof(TContext));

            InternalConfiguration.Instance.RootResolver.DatabaseInitializerResolver.SetInitializer(
                typeof(TContext), strategy ?? new NullDatabaseInitializer<TContext>());
        }

        /// <summary>
        /// Runs the the registered <see cref="IDatabaseInitializer{TContext}" /> on this context.
        /// If "force" is set to true, then the initializer is run regardless of whether or not it
        /// has been run before.  This can be useful if a database is deleted while an app is running
        /// and needs to be reinitialized.
        /// If "force" is set to false, then the initializer is only run if it has not already been
        /// run for this context, model, and connection in this app domain. This method is typically
        /// used when it is necessary to ensure that the database has been created and seeded
        /// before starting some operation where doing so lazily will cause issues, such as when the
        /// operation is part of a transaction.
        /// </summary>
        /// <param name="force">
        /// If set to <c>true</c> the initializer is run even if it has already been run.
        /// </param>
        public void Initialize(bool force)
        {
            if (force)
            {
                _internalContext.MarkDatabaseInitialized();
                _internalContext.PerformDatabaseInitialization();
            }
            else
            {
                _internalContext.Initialize();
            }
        }

        /// <summary>
        /// Checks whether or not the database is compatible with the the current Code First model.
        /// </summary>
        /// <remarks>
        /// Model compatibility currently uses the following rules.
        /// If the context was created using either the Model First or Database First approach then the
        /// model is assumed to be compatible with the database and this method returns true.
        /// For Code First the model is considered compatible if the model is stored in the database
        /// in the Migrations history table and that model has no differences from the current model as
        /// determined by Migrations model differ.
        /// If the model is not stored in the database but an EF 4.1/4.2 model hash is found instead,
        /// then this is used to check for compatibility.
        /// </remarks>
        /// <param name="throwIfNoMetadata">
        /// If set to <c>true</c> then an exception will be thrown if no model metadata is found in the database. If set to <c>false</c> then this method will return <c>true</c> if metadata is not found.
        /// </param>
        /// <returns> True if the model hash in the context and the database match; false otherwise. </returns>
        public bool CompatibleWithModel(bool throwIfNoMetadata)
        {
            return _internalContext.CompatibleWithModel(throwIfNoMetadata);
        }

        #endregion

        #region Instance DDL Operations using full context

        /// <summary>
        /// Creates a new database on the database server for the model defined in the backing context.
        /// Note that calling this method before the database initialization strategy has run will disable
        /// executing that strategy.
        /// </summary>
        public void Create()
        {
            Create(skipExistsCheck: false);
        }

        internal void Create(bool skipExistsCheck)
        {
            using (var clonedObjectContext = _internalContext.CreateObjectContextForDdlOps())
            {
                if (!skipExistsCheck
                    && _internalContext.DatabaseOperations.Exists(clonedObjectContext.ObjectContext))
                {
                    throw Error.Database_DatabaseAlreadyExists(_internalContext.Connection.Database);
                }
                _internalContext.CreateDatabase(clonedObjectContext.ObjectContext);
            }
        }

        /// <summary>
        /// Creates a new database on the database server for the model defined in the backing context, but only
        /// if a database with the same name does not already exist on the server.
        /// </summary>
        /// <returns> True if the database did not exist and was created; false otherwise. </returns>
        public bool CreateIfNotExists()
        {
            using (var clonedObjectContext = _internalContext.CreateObjectContextForDdlOps())
            {
                if (!_internalContext.DatabaseOperations.Exists(clonedObjectContext.ObjectContext))
                {
                    _internalContext.CreateDatabase(clonedObjectContext.ObjectContext);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks whether or not the database exists on the server.
        /// </summary>
        /// <returns> True if the database exists; false otherwise. </returns>
        public bool Exists()
        {
            using (var clonedObjectContext = _internalContext.CreateObjectContextForDdlOps())
            {
                return _internalContext.DatabaseOperations.Exists(clonedObjectContext.ObjectContext);
            }
        }

        /// <summary>
        /// Deletes the database on the database server if it exists, otherwise does nothing.
        /// Calling this method from outside of an initializer will mark the database as having
        /// not been initialized. This means that if an attempt is made to use the database again
        /// after it has been deleted, then any initializer set will run again and, usually, will
        /// try to create the database again automatically.
        /// </summary>
        /// <returns> True if the database did exist and was deleted; false otherwise. </returns>
        public bool Delete()
        {
            using (var clonedObjectContext = _internalContext.CreateObjectContextForDdlOps())
            {
                var deleted = _internalContext.DatabaseOperations.DeleteIfExists(clonedObjectContext.ObjectContext);

                if (deleted)
                {
                    _internalContext.MarkDatabaseNotInitialized();
                }

                return deleted;
            }
        }

        #endregion

        #region Static DDL operations using just a connection

        /// <summary>
        /// Checks whether or not the database exists on the server.
        /// The connection to the database is created using the given database name or connection string
        /// in the same way as is described in the documentation for the <see cref="DbContext" /> class.
        /// </summary>
        /// <param name="nameOrConnectionString"> The database name or a connection string to the database. </param>
        /// <returns> True if the database exists; false otherwise. </returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static bool Exists(string nameOrConnectionString)
        {
            Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString");

            return PerformDatabaseOp(
                new LazyInternalConnection(nameOrConnectionString), new DatabaseOperations().Exists);
        }

        /// <summary>
        /// Deletes the database on the database server if it exists, otherwise does nothing.
        /// The connection to the database is created using the given database name or connection string
        /// in the same way as is described in the documentation for the <see cref="DbContext" /> class.
        /// </summary>
        /// <param name="nameOrConnectionString"> The database name or a connection string to the database. </param>
        /// <returns> True if the database did exist and was deleted; false otherwise. </returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static bool Delete(string nameOrConnectionString)
        {
            Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString");

            return PerformDatabaseOp(
                new LazyInternalConnection(nameOrConnectionString), new DatabaseOperations().DeleteIfExists);
        }

        /// <summary>
        /// Checks whether or not the database exists on the server.
        /// </summary>
        /// <param name="existingConnection"> An existing connection to the database. </param>
        /// <returns> True if the database exists; false otherwise. </returns>
        public static bool Exists(DbConnection existingConnection)
        {
            Check.NotNull(existingConnection, "existingConnection");

            return PerformDatabaseOp(existingConnection, new DatabaseOperations().Exists);
        }

        /// <summary>
        /// Deletes the database on the database server if it exists, otherwise does nothing.
        /// </summary>
        /// <param name="existingConnection"> An existing connection to the database. </param>
        /// <returns> True if the database did exist and was deleted; false otherwise. </returns>
        public static bool Delete(DbConnection existingConnection)
        {
            Check.NotNull(existingConnection, "existingConnection");

            return PerformDatabaseOp(existingConnection, new DatabaseOperations().DeleteIfExists);
        }

        #endregion

        #region Connection conventions

        /// <summary>
        /// The connection factory to use when creating a <see cref="DbConnection" /> from just
        /// a database name or a connection string.
        /// </summary>
        /// <remarks>
        /// This is used when just a database name or connection string is given to <see cref="DbContext" /> or when
        /// the no database name or connection is given to DbContext in which case the name of
        /// the context class is passed to this factory in order to generate a DbConnection.
        /// By default, the <see cref="IDbConnectionFactory" /> instance to use is read from the application's .config
        /// file from the "EntityFramework DefaultConnectionFactory" entry in appSettings. If no entry is found in
        /// the config file then <see cref="SqlConnectionFactory" /> is used. Setting this property in code
        /// always overrides whatever value is found in the config file.
        /// </remarks>
        [Obsolete(
            "The default connection factory should be set in the config file or using the DbConfiguration class. (See http://go.microsoft.com/fwlink/?LinkId=260883)"
            )]
        public static IDbConnectionFactory DefaultConnectionFactory
        {
            get { return DbConfiguration.DependencyResolver.GetService<IDbConnectionFactory>(); }
            set
            {
                Check.NotNull(value, "value");

                _defaultConnectionFactory = new Lazy<IDbConnectionFactory>(() => value, isThreadSafe: true);
            }
        }

        /// <summary>
        /// The actual connection factory that was set, rather than the one that is returned by the resolver,
        /// which may have come from another source.
        /// </summary>
        internal static IDbConnectionFactory SetDefaultConnectionFactory
        {
            get { return _defaultConnectionFactory.Value; }
        }

        /// <summary>
        /// Checks whether or not the DefaultConnectionFactory has been set to something other than its default value.
        /// </summary>
        internal static bool DefaultConnectionFactoryChanged
        {
            get { return !ReferenceEquals(_defaultConnectionFactory, _defaultDefaultConnectionFactory); }
        }

        /// <summary>
        /// Resets the DefaultConnectionFactory to its initial value.
        /// Currently, this method is only used by test code.
        /// </summary>
        internal static void ResetDefaultConnectionFactory()
        {
            _defaultConnectionFactory = _defaultDefaultConnectionFactory;
        }

        #endregion

        #region Database operations

        /// <summary>
        /// Performs the operation defined by the given delegate using the given lazy connection, ensuring
        /// that the lazy connection is disposed after use.
        /// </summary>
        /// <param name="lazyConnection"> Information used to create a DbConnection. </param>
        /// <param name="operation"> The operation to perform. </param>
        /// <returns> The return value of the operation. </returns>
        private static bool PerformDatabaseOp(
            LazyInternalConnection lazyConnection, Func<ObjectContext, bool> operation)
        {
            using (lazyConnection)
            {
                return PerformDatabaseOp(lazyConnection.Connection, operation);
            }
        }

        /// <summary>
        /// Performs the operation defined by the given delegate against a connection.  The connection
        /// is either the connection accessed from the context backing this object, or is obtained from
        /// the connection information passed to one of the static methods.
        /// </summary>
        /// <param name="connection"> The connection to use. </param>
        /// <param name="operation"> The operation to perform. </param>
        /// <returns> The return value of the operation. </returns>
        private static bool PerformDatabaseOp(DbConnection connection, Func<ObjectContext, bool> operation)
        {
            using (var context = CreateEmptyObjectContext(connection))
            {
                return operation(context);
            }
        }

        /// <summary>
        /// Returns an empty ObjectContext that can be used to perform delete/exists operations.
        /// </summary>
        /// <param name="connection"> The connection for which to create an ObjectContext. </param>
        /// <returns> The empty context. </returns>
        private static ObjectContext CreateEmptyObjectContext(DbConnection connection)
        {
            // Unfortunately, we need to spin up an ObjectContext to do operations on the database
            // because the methods we need are defined on the context.  The easiest way to get an ObjectContext
            // is to use Code First with an empty model, so that's what we do.

            return new DbModelBuilder().Build(connection).Compile().CreateObjectContext<ObjectContext>(connection);
        }

        #endregion

        #region SQL query/command methods

        /// <summary>
        /// Creates a raw SQL query that will return elements of the given generic type.
        /// The type can be any type that has properties that match the names of the columns returned
        /// from the query, or can be a simple primitive type.  The type does not have to be an
        /// entity type. The results of this query are never tracked by the context even if the
        /// type of object returned is an entity type.  Use the <see cref="DbSet{TEntity}.SqlQuery" />
        /// method to return entities that are tracked by the context.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.Database.SqlQuery&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.Database.SqlQuery&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <typeparam name="TElement"> The type of object returned by the query. </typeparam>
        /// <param name="sql"> The SQL query string. </param>
        /// <param name="parameters"> The parameters to apply to the SQL query string. </param>
        /// <returns>
        /// A <see cref="DbRawSqlQuery{TElement}" /> object that will execute the query when it is enumerated.
        /// </returns>
        public DbRawSqlQuery<TElement> SqlQuery<TElement>(string sql, params object[] parameters)
        {
            Check.NotEmpty(sql, "sql");
            Check.NotNull(parameters, "parameters");

            return
                new DbRawSqlQuery<TElement>(
                    new InternalSqlNonSetQuery(_internalContext, typeof(TElement), sql, /*streaming:*/ false, parameters));
        }

        /// <summary>
        /// Creates a raw SQL query that will return elements of the given type.
        /// The type can be any type that has properties that match the names of the columns returned
        /// from the query, or can be a simple primitive type.  The type does not have to be an
        /// entity type. The results of this query are never tracked by the context even if the
        /// type of object returned is an entity type.  Use the <see cref="DbSet.SqlQuery" />
        /// method to return entities that are tracked by the context.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.Database.SqlQuery(typeof(Post), "SELECT * FROM dbo.Posts WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.Database.SqlQuery(typeof(Post), "SELECT * FROM dbo.Posts WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <param name="elementType"> The type of object returned by the query. </param>
        /// <param name="sql"> The SQL query string. </param>
        /// <param name="parameters"> The parameters to apply to the SQL query string. </param>
        /// <returns>
        /// A <see cref="DbRawSqlQuery" /> object that will execute the query when it is enumerated.
        /// </returns>
        public DbRawSqlQuery SqlQuery(Type elementType, string sql, params object[] parameters)
        {
            Check.NotNull(elementType, "elementType");
            Check.NotEmpty(sql, "sql");
            Check.NotNull(parameters, "parameters");

            return new DbRawSqlQuery(new InternalSqlNonSetQuery(_internalContext, elementType, sql, /*isNoTracking:*/ false, parameters));
        }

        /// <summary>
        /// Executes the given DDL/DML command against the database.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.Database.ExecuteSqlCommand("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.Database.ExecuteSqlCommand("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <remarks>
        /// If there isn't an existing local or ambient transaction a new transaction will be used
        /// to execute the command.
        /// </remarks>
        /// <param name="sql"> The command string. </param>
        /// <param name="parameters"> The parameters to apply to the command string. </param>
        /// <returns> The result returned by the database after executing the command. </returns>
        public int ExecuteSqlCommand(string sql, params object[] parameters)
        {
            return ExecuteSqlCommand(TransactionalBehavior.EnsureTransaction, sql, parameters);
        }

        /// <summary>
        /// Executes the given DDL/DML command against the database.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.Database.ExecuteSqlCommand("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.Database.ExecuteSqlCommand("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <param name="transactionalBehavior"> Controls the creation of a transaction for this command. </param>
        /// <param name="sql"> The command string. </param>
        /// <param name="parameters"> The parameters to apply to the command string. </param>
        /// <returns> The result returned by the database after executing the command. </returns>
        public int ExecuteSqlCommand(TransactionalBehavior transactionalBehavior, string sql, params object[] parameters)
        {
            Check.NotEmpty(sql, "sql");
            Check.NotNull(parameters, "parameters");

            return _internalContext.ExecuteSqlCommand(transactionalBehavior, sql, parameters);
        }

#if !NET40

        /// <summary>
        /// Asynchronously executes the given DDL/DML command against the database.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.Database.ExecuteSqlCommandAsync("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.Database.ExecuteSqlCommandAsync("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// 
        /// If there isn't an existing local transaction a new transaction will be used
        /// to execute the command.
        /// </remarks>
        /// <param name="sql"> The command string. </param>
        /// <param name="parameters"> The parameters to apply to the command string. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the result returned by the database after executing the command.
        /// </returns>
        public Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters)
        {
            return ExecuteSqlCommandAsync(TransactionalBehavior.EnsureTransaction, sql, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Asynchronously executes the given DDL/DML command against the database.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.Database.ExecuteSqlCommandAsync("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.Database.ExecuteSqlCommandAsync("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="transactionalBehavior"> Controls the creation of a transaction for this command. </param>
        /// <param name="sql"> The command string. </param>
        /// <param name="parameters"> The parameters to apply to the command string. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the result returned by the database after executing the command.
        /// </returns>
        public Task<int> ExecuteSqlCommandAsync(TransactionalBehavior transactionalBehavior, string sql, params object[] parameters)
        {
            return ExecuteSqlCommandAsync(transactionalBehavior, sql, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Asynchronously executes the given DDL/DML command against the database.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.Database.ExecuteSqlCommandAsync("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.Database.ExecuteSqlCommandAsync("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// 
        /// If there isn't an existing local transaction a new transaction will be used
        /// to execute the command.
        /// </remarks>
        /// <param name="sql"> The command string. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <param name="parameters"> The parameters to apply to the command string. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the result returned by the database after executing the command.
        /// </returns>
        public Task<int> ExecuteSqlCommandAsync(string sql, CancellationToken cancellationToken, params object[] parameters)
        {
            return ExecuteSqlCommandAsync(TransactionalBehavior.EnsureTransaction, sql, cancellationToken, parameters);
        }

        /// <summary>
        /// Asynchronously executes the given DDL/DML command against the database.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.Database.ExecuteSqlCommandAsync("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.Database.ExecuteSqlCommandAsync("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="transactionalBehavior"> Controls the creation of a transaction for this command. </param>
        /// <param name="sql"> The command string. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <param name="parameters"> The parameters to apply to the command string. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the result returned by the database after executing the command.
        /// </returns>
        public Task<int> ExecuteSqlCommandAsync(
            TransactionalBehavior transactionalBehavior, string sql, CancellationToken cancellationToken, params object[] parameters)
        {
            Check.NotEmpty(sql, "sql");
            Check.NotNull(parameters, "parameters");

            return _internalContext.ExecuteSqlCommandAsync(transactionalBehavior, sql, cancellationToken, parameters);
        }

#endif

        #endregion

        #region Hidden Object methods

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Gets the <see cref="Type" /> of the current instance.
        /// </summary>
        /// <returns>The exact runtime type of the current instance.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }

        #endregion

        /// <summary>
        /// Gets or sets the timeout value, in seconds, for all context operations.
        /// The default value is null, where null indicates that the default value of the underlying
        /// provider will be used.
        /// </summary>
        /// <value>
        /// The timeout, in seconds, or null to use the provider default.
        /// </value>
        public int? CommandTimeout
        {
            get { return _internalContext.CommandTimeout; }
            set
            {
                if (value.HasValue
                    && value < 0)
                {
                    throw new ArgumentException(Strings.ObjectContext_InvalidCommandTimeout);
                }

                _internalContext.CommandTimeout = value;
            }
        }

        /// <summary>
        /// Set this property to log the SQL generated by the <see cref="DbContext" /> to the given
        /// delegate. For example, to log to the console, set this property to <see cref="Console.Write(string)" />.
        /// </summary>
        /// <remarks>
        /// The format of the log text can be changed by creating a new formatter that derives from
        /// <see cref="DatabaseLogFormatter" /> and setting it with <see cref="DbConfiguration.SetDatabaseLogFormatter" />.
        /// For more low-level control over logging/interception see <see cref="IDbCommandInterceptor" /> and
        /// <see cref="DbInterception" />.
        /// </remarks>
        public Action<string> Log
        {
            get { return _internalContext.Log; }
            set { _internalContext.Log = value; }
        }
    }
}
