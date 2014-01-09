// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// The base class for interceptors that handle the transaction operations. Derived classes can be registered using
    /// <see cref="DbConfiguration.SetTransactionHandler(System.Func{TransactionHandler})" />.
    /// </summary>
    public abstract class TransactionHandler : IDbTransactionInterceptor, IDbConnectionInterceptor, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionHandler"/> class.
        /// </summary>
        /// <remarks>
        /// One of the Initialize methods needs to be called before this instance can be used.
        /// </remarks>
        protected TransactionHandler()
        {
            DbInterception.Add(this);
        }

        /// <summary>
        /// Initializes this instance using the specified context.
        /// </summary>
        /// <param name="context">The context for which transaction operations will be handled.</param>
        public virtual void Initialize(ObjectContext context)
        {
            Check.NotNull(context, "context");
            if (ObjectContext != null
                || DbContext != null
                || Connection != null)
            {
                throw new InvalidOperationException(Strings.TransactionHandler_AlreadyInitialized);
            }

            ObjectContext = context;
            DbContext = context.InterceptionContext.DbContexts.FirstOrDefault();
            Connection = ((EntityConnection)ObjectContext.Connection).StoreConnection;
        }

        /// <summary>
        /// Initializes this instance using the specified context.
        /// </summary>
        /// <param name="context">The context for which transaction operations will be handled.</param>
        /// <param name="connection">The connection to use for the initialization.</param>
        /// <remarks>
        /// This method is called by migrations. It is important that no action is performed on the
        /// specified context that causes it to be initialized.
        /// </remarks>
        public virtual void Initialize(DbContext context, DbConnection connection)
        {
            Check.NotNull(context, "context");
            Check.NotNull(connection, "connection");
            if (ObjectContext != null
                || DbContext != null
                || Connection != null)
            {
                throw new InvalidOperationException(Strings.TransactionHandler_AlreadyInitialized);
            }

            DbContext = context;
            Connection = connection;
        }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>
        /// The <see cref="ObjectContext"/> for which the transaction operations will be handled.
        /// </value>
        public ObjectContext ObjectContext { get; private set; }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>
        /// The <see cref="DbContext"/> for which the transaction operations will be handled, could be null.
        /// </value>
        public DbContext DbContext { get; private set; }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <value>
        /// The <see cref="DbConnection"/> for which the transaction operations will be handled.
        /// </value>
        /// <remarks>
        /// This connection object is only used to determine whether a particular operation needs to be handled
        /// in cases where a context is not available.
        /// </remarks>
        public DbConnection Connection { get; private set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this transaction handler is disposed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if disposed; otherwise, <c>false</c>.
        /// </value>
        protected bool IsDisposed { get; set; }

        /// <summary>
        /// Releases the resources used by this transaction handler.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                DbInterception.Remove(this);
            }
            IsDisposed = true;
        }

        /// <summary>
        /// Checks whether the supplied interception context contains the target context
        /// or the supplied connection is the same as the one used by the target context.
        /// </summary>
        /// <param name="connection">A connection.</param>
        /// <param name="interceptionContext">An interception context.</param>
        /// <returns>
        /// <c>true</c> if the supplied interception context contains the target context or
        /// the supplied connection is the same as the one used by the target context if
        /// the supplied interception context doesn't contain any contexts; <c>false</c> otherwise.
        /// </returns>
        /// <remarks>
        /// Note that calling this method will trigger initialization of any DbContext referenced from the <paramref name="interceptionContext"/>
        /// </remarks>
        protected internal virtual bool MatchesParentContext(DbConnection connection, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(interceptionContext, "interceptionContext");

            if (DbContext != null
                && interceptionContext.DbContexts.Contains(DbContext, ReferenceEquals))
            {
                return true;
            }

            if (ObjectContext != null
                && interceptionContext.ObjectContexts.Contains(ObjectContext, ReferenceEquals))
            {
                return true;
            }

            if (Connection != null
                && !interceptionContext.ObjectContexts.Any()
                && !interceptionContext.DbContexts.Any())
            {
                return ReferenceEquals(connection, Connection);
            }

            return false;
        }

        /// <summary>
        /// When implemented in a derived class returns the script to prepare the database
        /// for this transaction handler.
        /// </summary>
        /// <returns>A script to change the database schema for this transaction handler.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public abstract string BuildDatabaseInitializationScript();

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection beginning the transaction.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.BeginningTransaction"/>
        public virtual void BeginningTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection that began the transaction.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.BeganTransaction"/>
        public virtual void BeganTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection being closed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.Closing"/>
        public virtual void Closing(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection that was closed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.Closed"/>
        public virtual void Closed(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.ConnectionStringGetting"/>
        public virtual void ConnectionStringGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.ConnectionStringGot"/>
        public virtual void ConnectionStringGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.ConnectionStringSetting"/>
        public virtual void ConnectionStringSetting(
            DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.ConnectionStringSet"/>
        public virtual void ConnectionStringSet(
            DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.ConnectionTimeoutGetting"/>
        public virtual void ConnectionTimeoutGetting(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.ConnectionTimeoutGot"/>
        public virtual void ConnectionTimeoutGot(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.DatabaseGetting"/>
        public virtual void DatabaseGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.DatabaseGot"/>
        public virtual void DatabaseGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.DataSourceGetting"/>
        public virtual void DataSourceGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.DataSourceGot"/>
        public virtual void DataSourceGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection being disposed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        public virtual void Disposing(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection that was disposed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        public virtual void Disposed(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.EnlistingTransaction"/>
        public virtual void EnlistingTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.EnlistedTransaction"/>
        public virtual void EnlistedTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection being opened.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.Opening"/>
        public virtual void Opening(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection that was opened.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.Opened"/>
        public virtual void Opened(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.ServerVersionGetting"/>
        public virtual void ServerVersionGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.ServerVersionGot"/>
        public virtual void ServerVersionGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.StateGetting"/>
        public virtual void StateGetting(DbConnection connection, DbConnectionInterceptionContext<ConnectionState> interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.StateGot"/>
        public virtual void StateGot(DbConnection connection, DbConnectionInterceptionContext<ConnectionState> interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbTransactionInterceptor.ConnectionGetting"/>
        public virtual void ConnectionGetting(DbTransaction transaction, DbTransactionInterceptionContext<DbConnection> interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbTransactionInterceptor.ConnectionGot"/>
        public virtual void ConnectionGot(DbTransaction transaction, DbTransactionInterceptionContext<DbConnection> interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbTransactionInterceptor.IsolationLevelGetting"/>
        public virtual void IsolationLevelGetting(
            DbTransaction transaction, DbTransactionInterceptionContext<IsolationLevel> interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbTransactionInterceptor.IsolationLevelGot"/>
        public virtual void IsolationLevelGot(
            DbTransaction transaction, DbTransactionInterceptionContext<IsolationLevel> interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="transaction">The transaction being commited.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbTransactionInterceptor.Committing"/>
        public virtual void Committing(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="transaction">The transaction that was commited.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbTransactionInterceptor.Committed"/>
        public virtual void Committed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="transaction">The transaction being disposed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbTransactionInterceptor.Disposing"/>
        public virtual void Disposing(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="transaction">The transaction that was disposed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbTransactionInterceptor.Disposed"/>
        public virtual void Disposed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="transaction">The transaction being rolled back.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbTransactionInterceptor.RollingBack"/>
        public virtual void RollingBack(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
        {
        }

        /// <summary>
        /// Can be implemented in a derived class.
        /// </summary>
        /// <param name="transaction">The transaction that was rolled back.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbTransactionInterceptor.RolledBack"/>
        public virtual void RolledBack(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
        {
        }
    }
}
