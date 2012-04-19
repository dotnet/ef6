namespace System.Data.Entity.Core.EntityClient
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a transaction for the conceptual layer
    /// </summary>
    public sealed class EntityTransaction : DbTransaction
    {
        bool _disposed = false;
        private InternalEntityTransaction _internalEntityTransaction;

        /// <summary>
        /// Constructs the EntityTransaction object with an associated connection and the underlying store transaction
        /// </summary>
        /// <param name="connection">The EntityConnetion object owning this transaction</param>
        /// <param name="storeTransaction">The underlying transaction object</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
            Justification = "Object is in fact passed to property of the class and gets Disposed properly in the Dispose() method.")]
        internal EntityTransaction(EntityConnection connection, DbTransaction storeTransaction)
            : this(new InternalEntityTransaction(connection, storeTransaction))
        {
        }

        internal EntityTransaction(InternalEntityTransaction internalEntityTransaction)
        {
            _internalEntityTransaction = internalEntityTransaction;
            _internalEntityTransaction.EntityTransactionWrapper = this;
        }

        /// <summary>
        /// The connection object owning this transaction object
        /// </summary>
        public new EntityConnection Connection
        {
            get { return _internalEntityTransaction.Connection; }
        }

        /// <summary>
        /// The connection object owning this transaction object
        /// </summary>
        protected override DbConnection DbConnection
        {
            get { return Connection; }
        }

        /// <summary>
        /// The isolation level of this transaction
        /// </summary>
        public override IsolationLevel IsolationLevel
        {
            get { return _internalEntityTransaction.IsolationLevel; }
        }

        /// <summary>
        /// Gets the DbTransaction for the underlying provider transaction
        /// </summary>
        internal DbTransaction StoreTransaction
        {
            get { return _internalEntityTransaction.StoreTransaction; }
        }

        /// <summary>
        /// Commits the transaction
        /// </summary>
        public override void Commit()
        {
            _internalEntityTransaction.Commit();
        }

        /// <summary>
        /// Rolls back the transaction
        /// </summary>
        public override void Rollback()
        {
            _internalEntityTransaction.Rollback();
        }

        /// <summary>
        /// Cleans up this transaction object
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _internalEntityTransaction.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
