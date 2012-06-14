namespace System.Data.Entity.Core.EntityClient
{
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Class representing a transaction for the conceptual layer
    /// </summary>
    public class EntityTransaction : DbTransaction
    {
        private readonly EntityConnection _connection;
        private readonly DbTransaction _storeTransaction;

        internal EntityTransaction()
        {
        }

        /// <summary>
        /// Constructs the EntityTransaction object with an associated connection and the underlying store transaction
        /// </summary>
        /// <param name="connection">The EntityConnetion object owning this transaction</param>
        /// <param name="storeTransaction">The underlying transaction object</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
            Justification = "Object is in fact passed to property of the class and gets Disposed properly in the Dispose() method.")]
        internal EntityTransaction(EntityConnection connection, DbTransaction storeTransaction)
        {
            Contract.Requires(connection != null);
            Contract.Requires(storeTransaction != null);

            _connection = connection;
            _storeTransaction = storeTransaction;
        }

        /// <summary>
        /// The connection object owning this transaction object
        /// </summary>
        public new virtual EntityConnection Connection
        {
            get
            {
                // follow the store transaction behavior
                return ((null != _storeTransaction.Connection) ? _connection : null);
            }
        }

        /// <summary>
        /// The connection object owning this transaction object
        /// </summary>
        protected override DbConnection DbConnection
        {
            get
            {
                // follow the store transaction behavior
                return ((null != _storeTransaction.Connection) ? _connection : null);
            }
        }

        /// <summary>
        /// The isolation level of this transaction
        /// </summary>
        public override IsolationLevel IsolationLevel
        {
            get { return _storeTransaction.IsolationLevel; }
        }

        /// <summary>
        /// Gets the DbTransaction for the underlying provider transaction
        /// </summary>
        internal virtual DbTransaction StoreTransaction
        {
            get { return _storeTransaction; }
        }

        /// <summary>
        /// Commits the transaction
        /// </summary>
        public override void Commit()
        {
            try
            {
                _storeTransaction.Commit();
            }
            catch (Exception e)
            {
                if (e.IsCatchableExceptionType())
                {
                    throw new EntityException(Strings.EntityClient_ProviderSpecificError(@"Commit"), e);
                }

                throw;
            }

            ClearCurrentTransaction();
        }

        /// <summary>
        /// Rolls back the transaction
        /// </summary>
        public override void Rollback()
        {
            try
            {
                _storeTransaction.Rollback();
            }
            catch (Exception e)
            {
                if (e.IsCatchableExceptionType())
                {
                    throw new EntityException(Strings.EntityClient_ProviderSpecificError(@"Rollback"), e);
                }

                throw;
            }

            ClearCurrentTransaction();
        }

        /// <summary>
        /// Cleans up this transaction object
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearCurrentTransaction();
                _storeTransaction.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Helper method to wrap EntityConnection.ClearCurrentTransaction()
        /// </summary>
        private void ClearCurrentTransaction()
        {
            if (_connection.CurrentTransaction
                == this)
            {
                _connection.ClearCurrentTransaction();
            }
        }
    }
}
