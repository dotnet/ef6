namespace System.Data.Entity.Core.EntityClient.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;

    internal class InternalEntityTransaction : IDisposable
    {
        private bool _disposed = false;
        private readonly EntityConnection _connection;
        private readonly DbTransaction _storeTransaction;

        /// <summary>
        /// Constructs the EntityTransaction object with an associated connection and the underlying store transaction
        /// </summary>
        /// <param name="connection">The EntityConnetion object owning this transaction</param>
        /// <param name="storeTransaction">The underlying transaction object</param>
        internal InternalEntityTransaction(EntityConnection connection, DbTransaction storeTransaction)
        {
            Contract.Requires(connection != null);
            Contract.Requires(storeTransaction != null);

            _connection = connection;
            _storeTransaction = storeTransaction;
        }

        /// <summary>
        /// Wrapper on the parent class, for accessing its protected members (via proxy method) 
        /// or when the parent class is a parameter to another method/constructor
        /// </summary>
        internal EntityTransaction EntityTransactionWrapper { get; set; }

        /// <summary>
        /// The connection object owning this transaction object
        /// </summary>
        public EntityConnection Connection
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
        internal DbConnection DbConnection
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
        public IsolationLevel IsolationLevel
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
        public void Commit()
        {
            try
            {
                _storeTransaction.Commit();
            }
            catch (Exception e)
            {
                if (EntityUtil.IsCatchableExceptionType(e))
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
        public void Rollback()
        {
            try
            {
                _storeTransaction.Rollback();
            }
            catch (Exception e)
            {
                if (EntityUtil.IsCatchableExceptionType(e))
                {
                    throw new EntityException(Strings.EntityClient_ProviderSpecificError(@"Rollback"), e);
                }

                throw;
            }

            ClearCurrentTransaction();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Cleans up this transaction object
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources</param>
        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    ClearCurrentTransaction();
                    _storeTransaction.Dispose();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Helper method to wrap EntityConnection.ClearCurrentTransaction()
        /// </summary>
        private void ClearCurrentTransaction()
        {
            if (_connection.CurrentTransaction == this.EntityTransactionWrapper)
            {
                _connection.ClearCurrentTransaction();
            }
        }


    }
}
