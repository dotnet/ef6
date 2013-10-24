// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A transaction handler that allows to gracefully recover from connection failures
    /// during transaction commit by storing transaction tracing information in the database.
    /// It needs to be registered by using <see cref="DbConfiguration.SetTransactionHandler(System.Func{TransactionHandler})" />.
    /// </summary>
    /// <remarks>
    /// This transaction handler uses <see cref="TransactionContext"/> that can be changed using
    /// <see cref="DbConfiguration.SetTransactionContext(System.Func{DbConnection, TransactionContext})" />.
    /// </remarks>
    public class CommitFailureHandler : TransactionHandler
    {
        // Doesn't need to be thread-safe since transactions can't run concurrently on the same connection
        private readonly Dictionary<DbTransaction, TransactionRow> _transactions = new Dictionary<DbTransaction, TransactionRow>();

        private readonly List<TransactionRow> _rowsToDelete = new List<TransactionRow>();

        /// <summary>
        /// Gets the transaction context.
        /// </summary>
        /// <value>
        /// The transaction context.
        /// </value>
        protected TransactionContext TransactionContext { get; private set; }

        /// <inheritdoc/>
        public override void Initialize(ObjectContext context)
        {
            base.Initialize(context);

            var storeMetadata = (StoreItemCollection)context.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
            var connection = ((EntityConnection)ObjectContext.Connection).StoreConnection;

            Initialize(connection, storeMetadata.ProviderFactory);
        }

        /// <inheritdoc/>
        public override void Initialize(DbContext context, DbConnection connection)
        {
            base.Initialize(context, connection);

            var providerFactory = DbProviderServices.GetProviderFactory(connection);

            Initialize(connection, providerFactory);
        }

        private void Initialize(DbConnection connection, DbProviderFactory providerFactory)
        {
            var providerInvariantName =
                DbConfiguration.DependencyResolver.GetService<IProviderInvariantName>(providerFactory).Name;

            var dataSource = DbInterception.Dispatch.Connection.GetDataSource(connection, new DbInterceptionContext());

            var transactionContextFactory = DbConfiguration.DependencyResolver.GetService<Func<DbConnection, TransactionContext>>(
                new StoreKey(providerInvariantName, dataSource));

            TransactionContext = transactionContextFactory(connection);
            if (TransactionContext != null)
            {
                TransactionContext.Configuration.LazyLoadingEnabled = false;
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
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed
                && disposing
                && TransactionContext != null)
            {
                if (_rowsToDelete.Any())
                {
                    PruneTransactionRows(force: true);
                }
                TransactionContext.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override string BuildDatabaseInitializationScript()
        {
            return TransactionContext == null ? null : ((IObjectContextAdapter)TransactionContext).ObjectContext.CreateDatabaseScript();
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
                || !MatchesParentContext(connection, interceptionContext))
            {
                return;
            }

            var transactionId = Guid.NewGuid();
            var savedSuccesfully = false;
            var reinitializedDatabase = false;
            while (!savedSuccesfully)
            {
                Debug.Assert(!_transactions.ContainsKey(interceptionContext.Result), "The transaction has already been registered");
                var transactionRow = new TransactionRow { Id = transactionId, CreationTime = DateTime.Now };
                _transactions.Add(interceptionContext.Result, transactionRow);
                TransactionContext.Transactions.Add(transactionRow);

                var objectContext = ((IObjectContextAdapter)TransactionContext).ObjectContext;
                ((EntityConnection)objectContext.Connection).UseStoreTransaction(interceptionContext.Result);
                try
                {
                    objectContext.SaveChanges(SaveOptions.DetectChangesBeforeSave, executeInExistingTransaction: true);
                    savedSuccesfully = true;
                }
                catch (UpdateException)
                {
                    _transactions.Remove(interceptionContext.Result);
                    TransactionContext.Transactions.Remove(transactionRow);

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
                    }
                    catch (EntityCommandExecutionException)
                    {
                        // The necessary tables are not present.
                        // This can happen if the database was deleted after TransactionContext has been initialized
                        TransactionContext.Database.Initialize(force: true);

                        reinitializedDatabase = true;
                    }

                    interceptionContext.Result.Rollback();
                    interceptionContext.Result = connection.BeginTransaction(interceptionContext.IsolationLevel);
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
            if (TransactionContext == null
                || !MatchesParentContext(transaction.Connection, interceptionContext))
            {
                return;
            }

            TransactionRow transactionRow;
            if (_transactions.TryGetValue(transaction, out transactionRow))
            {
                _transactions.Remove(transaction);
                if (interceptionContext.Exception != null)
                {
                    var existingTransactionRow = TransactionContext.Transactions
                        .AsNoTracking()
                        .WithExecutionStrategy(new DefaultExecutionStrategy())
                        .SingleOrDefault(t => t.Id == transactionRow.Id);

                    if (existingTransactionRow != null)
                    {
                        // The transaction id is still in the database, so the commit succeeded
                        interceptionContext.Exception = null;

                        PruneTransactionRows(transactionRow);
                    }
                    else
                    {
                        TransactionContext.Transactions.Local.Remove(transactionRow);
                    }
                }
                else
                {
                    PruneTransactionRows(transactionRow);
                }
            }
            else
            {
                Debug.Assert(false, "Expected the transaction to be registered");
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
            if (TransactionContext == null
                || !MatchesParentContext(transaction.Connection, interceptionContext))
            {
                return;
            }

            TransactionRow transactionRow;
            if (_transactions.TryGetValue(transaction, out transactionRow))
            {
                _transactions.Remove(transaction);
                TransactionContext.Transactions.Local.Remove(transactionRow);
            }
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
        /// Adds the specified transaction to the list of transactions that can be removed from the database
        /// </summary>
        /// <param name="transaction">The transaction to be removed from the database.</param>
        protected virtual void MarkTransactionForPruning(TransactionRow transaction)
        {
            Check.NotNull(transaction, "transaction");

            _rowsToDelete.Add(transaction);
        }

        /// <summary>
        /// Removes the transactions marked for deletion.
        /// </summary>
        public void PruneTransactionRows()
        {
            PruneTransactionRows(force: true);
        }

#if !NET40
        /// <summary>
        /// Asynchronously removes the transactions marked for deletion.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task PruneTransactionRowsAsync()
        {
            return PruneTransactionRowsAsync( /*force:*/ true, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously removes the transactions marked for deletion.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task PruneTransactionRowsAsync(CancellationToken cancellationToken)
        {
            return PruneTransactionRowsAsync( /*force:*/ true, cancellationToken);
        }
#endif

        /// <summary>
        /// Removes the transactions marked for deletion if their number exceeds <see cref="PruningLimit"/>.
        /// </summary>
        /// <param name="force">
        /// if set to <c>true</c> will remove all the old transactions even if their number does not exceed <see cref="PruningLimit"/>.
        /// </param>
        protected virtual void PruneTransactionRows(bool force)
        {
            if (force || _rowsToDelete.Count > PruningLimit)
            {
                foreach (var rowToDelete in _rowsToDelete)
                {
                    TransactionContext.Transactions.Remove(rowToDelete);
                }

                _rowsToDelete.Clear();

                try
                {
                    TransactionContext.SaveChanges();
                }
                catch (EntityException)
                {
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected virtual async Task PruneTransactionRowsAsync(bool force, CancellationToken cancellationToken)
        {
            if (force || _rowsToDelete.Count > PruningLimit)
            {
                foreach (var rowToDelete in _rowsToDelete)
                {
                    TransactionContext.Transactions.Remove(rowToDelete);
                }

                _rowsToDelete.Clear();

                try
                {
                    await TransactionContext.SaveChangesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                }
                catch (EntityException)
                {
                }
            }
        }
#endif

        private void PruneTransactionRows(TransactionRow transaction)
        {
            MarkTransactionForPruning(transaction);
            PruneTransactionRows(force: false);
        }
    }
}
