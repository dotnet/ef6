// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A transaction handler that allows to gracefully recover from connection failures
    /// during transaction commit by storing transaction tracing information in the database.
    /// It needs to be registered by using <see cref="DbConfiguration.SetDefaultTransactionHandler" />.
    /// </summary>
    /// <remarks>
    /// This transaction handler uses <see cref="TransactionContext"/> to store the transaction information
    /// the schema used can be configured by creating a class derived from <see cref="TransactionContext"/>
    /// that overrides <see cref="DbContext.OnModelCreating"/> and passing it to the constructor of this class.
    /// </remarks>
    public class CommitFailureHandler : TransactionHandler
    {
        // Doesn't need to be thread-safe since transactions can't run concurrently on the same connection
        private readonly Dictionary<DbTransaction, TransactionRow> _transactions = new Dictionary<DbTransaction, TransactionRow>();

        private readonly HashSet<TransactionRow> _rowsToDelete = new HashSet<TransactionRow>();

        private readonly Func<DbConnection, TransactionContext> _transactionContextFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommitFailureHandler"/> class using the default <see cref="TransactionContext"/>.
        /// </summary>
        /// <remarks>
        /// One of the Initialize methods needs to be called before this instance can be used.
        /// </remarks>
        public CommitFailureHandler()
            : this(c => new TransactionContext(c))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommitFailureHandler"/> class.
        /// </summary>
        /// <param name="transactionContextFactory">The transaction context factory.</param>
        /// <remarks>
        /// One of the Initialize methods needs to be called before this instance can be used.
        /// </remarks>
        public CommitFailureHandler(Func<DbConnection, TransactionContext> transactionContextFactory)
        {
            Check.NotNull(transactionContextFactory, "transactionContextFactory");

            _transactionContextFactory = transactionContextFactory;
        }

        /// <summary>
        /// Gets the transaction context.
        /// </summary>
        /// <value>
        /// The transaction context.
        /// </value>
        protected internal TransactionContext TransactionContext { get; private set; }

        /// <inheritdoc/>
        public override void Initialize(ObjectContext context)
        {
            base.Initialize(context);
            var connection = ((EntityConnection)ObjectContext.Connection).StoreConnection;

            Initialize(connection);
        }

        /// <inheritdoc/>
        public override void Initialize(DbContext context, DbConnection connection)
        {
            base.Initialize(context, connection);

            Initialize(connection);
        }

        private void Initialize(DbConnection connection)
        {
            TransactionContext = _transactionContextFactory(connection);
            if (TransactionContext != null)
            {
                TransactionContext.Configuration.LazyLoadingEnabled = false;
                TransactionContext.Configuration.AutoDetectChangesEnabled = false;
            }
        }

        /// <summary>
        /// Gets the number of transactions to be executed on the context before the transaction log will be cleaned.
        /// The default value is 20.
        /// </summary>
        protected virtual int PruningLimit
        {
            get { return 20; }
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed
                && disposing
                && TransactionContext != null)
            {
                if (_rowsToDelete.Any())
                {
                    try
                    {
                        PruneTransactionHistory(force: true, useExecutionStrategy: false);
                    }
                    catch (Exception)
                    {
                    }
                }
                TransactionContext.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override string BuildDatabaseInitializationScript()
        {
            if (TransactionContext != null)
            {
                var sqlStatements = TransactionContextInitializer<TransactionContext>.GenerateMigrationStatements(TransactionContext);

                var sqlBuilder = new StringBuilder();
                MigratorScriptingDecorator.BuildSqlScript(sqlStatements, sqlBuilder);

                return sqlBuilder.ToString();
            }

            return null;
        }

        /// <summary>
        /// Stores the tracking information for the new transaction to the database in the same transaction.
        /// </summary>
        /// <param name="connection">The connection that began the transaction.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbConnectionInterceptor.BeganTransaction" />
        public override void BeganTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
        {
            if (TransactionContext == null
                || !MatchesParentContext(connection, interceptionContext)
                || interceptionContext.Result == null)
            {
                return;
            }

            var transactionId = Guid.NewGuid();
            var savedSuccesfully = false;
            var reinitializedDatabase = false;
            var objectContext = ((IObjectContextAdapter)TransactionContext).ObjectContext;
            ((EntityConnection)objectContext.Connection).UseStoreTransaction(interceptionContext.Result);
            while (!savedSuccesfully)
            {
                Debug.Assert(!_transactions.ContainsKey(interceptionContext.Result), "The transaction has already been registered");
                var transactionRow = new TransactionRow { Id = transactionId, CreationTime = DateTime.Now };
                _transactions.Add(interceptionContext.Result, transactionRow);

                TransactionContext.Transactions.Add(transactionRow);
                try
                {
                    objectContext.SaveChangesInternal(SaveOptions.AcceptAllChangesAfterSave, executeInExistingTransaction: true);
                    savedSuccesfully = true;
                }
                catch (UpdateException)
                {
                    _transactions.Remove(interceptionContext.Result);
                    TransactionContext.Entry(transactionRow).State = EntityState.Detached;

                    if (reinitializedDatabase)
                    {
                        throw;
                    }

                    try
                    {
                        var existingTransaction =
                            TransactionContext.Transactions
                                .AsNoTracking()
                                .WithExecutionStrategy(new DefaultExecutionStrategy())
                                .FirstOrDefault(t => t.Id == transactionId);

                        if (existingTransaction != null)
                        {
                            transactionId = Guid.NewGuid();
                            Debug.Assert(false, "Duplicate GUID! this should never happen");
                        }
                        else
                        {
                            // Unknown exception cause
                            throw;
                        }
                    }
                    catch (EntityCommandExecutionException)
                    {
                        // The necessary tables are not present.
                        // This can happen if the database was deleted after TransactionContext has been initialized
                        TransactionContext.Database.Initialize(force: true);

                        reinitializedDatabase = true;
                    }
                }
            }
        }

        /// <summary>
        /// If there was an exception thrown checks the database for this transaction and rethrows it if not found.
        /// Otherwise marks the commit as succeeded and queues the transaction information to be deleted.
        /// </summary>
        /// <param name="transaction">The transaction that was commited.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbTransactionInterceptor.Committed" />
        public override void Committed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
        {
            TransactionRow transactionRow;
            if (TransactionContext == null
                || (interceptionContext.Connection != null && !MatchesParentContext(interceptionContext.Connection, interceptionContext))
                || !_transactions.TryGetValue(transaction, out transactionRow))
            {
                return;
            }

            _transactions.Remove(transaction);
            if (interceptionContext.Exception != null)
            {
                TransactionRow existingTransactionRow = null;
                try
                {
                    existingTransactionRow = TransactionContext.Transactions
                        .AsNoTracking()
                        .WithExecutionStrategy(new DefaultExecutionStrategy())
                        .SingleOrDefault(t => t.Id == transactionRow.Id);
                }
                catch (EntityCommandExecutionException)
                {
                    // Transaction table doesn't exist
                }

                if (existingTransactionRow != null)
                {
                    // The transaction id is still in the database, so the commit succeeded
                    interceptionContext.Exception = null;

                    PruneTransactionHistory(transactionRow);
                }
                else
                {
                    TransactionContext.Entry(transactionRow).State = EntityState.Detached;
                }
            }
            else
            {
                PruneTransactionHistory(transactionRow);
            }
        }

        /// <summary>
        /// Stops tracking the transaction that was rolled back.
        /// </summary>
        /// <param name="transaction">The transaction that was rolled back.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbTransactionInterceptor.RolledBack" />
        public override void RolledBack(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
        {
            TransactionRow transactionRow;
            if (TransactionContext == null
                || (interceptionContext.Connection != null && !MatchesParentContext(interceptionContext.Connection, interceptionContext))
                || !_transactions.TryGetValue(transaction, out transactionRow))
            {
                return;
            }

            _transactions.Remove(transaction);
            TransactionContext.Entry(transactionRow).State = EntityState.Detached;
        }

        /// <summary>
        /// Stops tracking the transaction that was disposed.
        /// </summary>
        /// <param name="transaction">The transaction that was disposed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <seealso cref="IDbTransactionInterceptor.Disposed" />
        public override void Disposed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
        {
            RolledBack(transaction, interceptionContext);
        }

        /// <summary>
        /// Removes all the transaction history.
        /// </summary>
        /// <remarks>
        /// This method should only be invoked when there are no active transactions to remove any leftover history
        /// that was not deleted due to catastrophic failures
        /// </remarks>
        public virtual void ClearTransactionHistory()
        {
            foreach (var transactionRow in TransactionContext.Transactions)
            {
                MarkTransactionForPruning(transactionRow);
            }
            PruneTransactionHistory(force: true, useExecutionStrategy: true);
        }

#if !NET40
        /// <summary>
        /// Asynchronously removes all the transaction history.
        /// </summary>
        /// <remarks>
        /// This method should only be invoked when there are no active transactions to remove any leftover history
        /// that was not deleted due to catastrophic failures
        /// </remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task ClearTransactionHistoryAsync()
        {
            return ClearTransactionHistoryAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously removes all the transaction history.
        /// </summary>
        /// <remarks>
        /// This method should only be invoked when there are no active transactions to remove any leftover history
        /// that was not deleted due to catastrophic failures
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public virtual async Task ClearTransactionHistoryAsync(CancellationToken cancellationToken)
        {
            await TransactionContext.Transactions.ForEachAsync(MarkTransactionForPruning, cancellationToken)
                .ConfigureAwait(continueOnCapturedContext: false);
            await
                PruneTransactionHistoryAsync( /*force:*/ true, /*useExecutionStrategy:*/ true, cancellationToken)
                    .ConfigureAwait(continueOnCapturedContext: false);
        }
#endif

        /// <summary>
        /// Adds the specified transaction to the list of transactions that can be removed from the database
        /// </summary>
        /// <param name="transaction">The transaction to be removed from the database.</param>
        protected virtual void MarkTransactionForPruning(TransactionRow transaction)
        {
            Check.NotNull(transaction, "transaction");

            if (!_rowsToDelete.Contains(transaction))
            {
                _rowsToDelete.Add(transaction);
            }
        }

        /// <summary>
        /// Removes the transactions marked for deletion.
        /// </summary>
        public void PruneTransactionHistory()
        {
            PruneTransactionHistory(force: true, useExecutionStrategy: true);
        }

#if !NET40
        /// <summary>
        /// Asynchronously removes the transactions marked for deletion.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task PruneTransactionHistoryAsync()
        {
            return PruneTransactionHistoryAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously removes the transactions marked for deletion.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task PruneTransactionHistoryAsync(CancellationToken cancellationToken)
        {
            return PruneTransactionHistoryAsync( /*force:*/ true, /*useExecutionStrategy:*/ true, cancellationToken);
        }
#endif

        /// <summary>
        /// Removes the transactions marked for deletion if their number exceeds <see cref="PruningLimit"/>.
        /// </summary>
        /// <param name="force">
        /// if set to <c>true</c> will remove all the old transactions even if their number does not exceed <see cref="PruningLimit"/>.
        /// </param>
        /// <param name="useExecutionStrategy">
        /// if set to <c>true</c> the operation will be executed using the associated execution strategy
        /// </param>
        protected virtual void PruneTransactionHistory(bool force, bool useExecutionStrategy)
        {
            if (_rowsToDelete.Count > 0
                && (force || _rowsToDelete.Count > PruningLimit))
            {
                foreach (var rowToDelete in TransactionContext.Transactions.ToList())
                {
                    if (_rowsToDelete.Contains(rowToDelete))
                    {
                        TransactionContext.Transactions.Remove(rowToDelete);
                    }
                }

                var objectContext = ((IObjectContextAdapter)TransactionContext).ObjectContext;

                try
                {
                    objectContext.SaveChangesInternal(SaveOptions.None, executeInExistingTransaction: !useExecutionStrategy);
                    _rowsToDelete.Clear();
                }
                finally
                {
                    // If SaveChanges failed we don't know whether the changes went through, so we will assume they did,
                    // but will retry the next time this method is called.
                    objectContext.AcceptAllChanges();
                }
            }
        }

#if !NET40
        /// <summary>
        /// Removes the transactions marked for deletion if their number exceeds <see cref="PruningLimit"/>.
        /// </summary>
        /// <param name="force">
        /// if set to <c>true</c> will remove all the old transactions even if their number does not exceed <see cref="PruningLimit"/>.
        /// </param>
        /// <param name="useExecutionStrategy">
        /// if set to <c>true</c> the operation will be executed using the associated execution strategy
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected virtual async Task PruneTransactionHistoryAsync(
            bool force, bool useExecutionStrategy, CancellationToken cancellationToken)
        {
            if (_rowsToDelete.Count > 0
                && (force || _rowsToDelete.Count > PruningLimit))
            {
                foreach (var rowToDelete in TransactionContext.Transactions.ToList())
                {
                    if (_rowsToDelete.Contains(rowToDelete))
                    {
                        TransactionContext.Transactions.Remove(rowToDelete);
                    }
                }

                var objectContext = ((IObjectContextAdapter)TransactionContext).ObjectContext;

                try
                {
                    await ((IObjectContextAdapter)TransactionContext).ObjectContext
                        .SaveChangesInternalAsync(
                            SaveOptions.None, /*executeInExistingTransaction:*/ !useExecutionStrategy, cancellationToken)
                        .ConfigureAwait(continueOnCapturedContext: false);
                    _rowsToDelete.Clear();
                }
                finally
                {
                    // If SaveChanges failed we don't know whether the changes went through, so we will assume they did,
                    // but will retry the next time this method is called.
                    objectContext.AcceptAllChanges();
                }
            }
        }
#endif

        private void PruneTransactionHistory(TransactionRow transaction)
        {
            MarkTransactionForPruning(transaction);

            try
            {
                PruneTransactionHistory(force: false, useExecutionStrategy: false);
            }
            catch (DataException)
            {
            }
        }

        /// <summary>
        /// Gets the <see cref="CommitFailureHandler"/> associated with the <paramref name="context"/> if there is one;
        /// otherwise returns <c>null</c>.
        /// </summary>
        /// <param name="context">The context</param>
        /// <returns>The associated <see cref="CommitFailureHandler"/>.</returns>
        public static CommitFailureHandler FromContext(DbContext context)
        {
            Check.NotNull(context, "context");

            return FromContext(((IObjectContextAdapter)context).ObjectContext);
        }

        /// <summary>
        /// Gets the <see cref="CommitFailureHandler"/> associated with the <paramref name="context"/> if there is one;
        /// otherwise returns <c>null</c>.
        /// </summary>
        /// <param name="context">The context</param>
        /// <returns>The associated <see cref="CommitFailureHandler"/>.</returns>
        public static CommitFailureHandler FromContext(ObjectContext context)
        {
            Check.NotNull(context, "context");

            return context.TransactionHandler as CommitFailureHandler;
        }
    }
}
