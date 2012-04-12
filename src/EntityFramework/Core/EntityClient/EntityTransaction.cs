namespace System.Data.Entity.Core.EntityClient
{
    using System.Data.Common;
    using System.Diagnostics;

    /// <summary>
    /// Class representing a transaction for the conceptual layer
    /// </summary>
    public sealed class EntityTransaction : DbTransaction
    {
        private readonly EntityConnection _connection;
        private readonly DbTransaction _storeTransaction;

        /// <summary>
        /// Constructs the EntityTransaction object with an associated connection and the underlying store transaction
        /// </summary>
        /// <param name="connection">The EntityConnetion object owning this transaction</param>
        /// <param name="storeTransaction">The underlying transaction object</param>
        internal EntityTransaction(EntityConnection connection, DbTransaction storeTransaction)
        {
            Debug.Assert(connection != null && storeTransaction != null);

            _connection = connection;
            _storeTransaction = storeTransaction;
        }

        /// <summary>
        /// The connection object owning this transaction object
        /// </summary>
        public new EntityConnection Connection
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
        internal DbTransaction StoreTransaction
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
                if (EntityUtil.IsCatchableExceptionType(e))
                {
                    throw EntityUtil.Provider(@"Commit", e);
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
                if (EntityUtil.IsCatchableExceptionType(e))
                {
                    throw EntityUtil.Provider(@"Rollback", e);
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
