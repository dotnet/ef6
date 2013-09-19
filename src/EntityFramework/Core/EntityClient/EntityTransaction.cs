// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityClient
{
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

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
        /// <param name="connection"> The EntityConnetion object owning this transaction </param>
        /// <param name="storeTransaction"> The underlying transaction object </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
            Justification = "Object is in fact passed to property of the class and gets Disposed properly in the Dispose() method.")]
        internal EntityTransaction(EntityConnection connection, DbTransaction storeTransaction)
        {
            DebugCheck.NotNull(connection);
            DebugCheck.NotNull(storeTransaction);

            _connection = connection;
            _storeTransaction = storeTransaction;
        }

        /// <summary>
        /// Gets <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnection" /> for this
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityTransaction" />
        /// .
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnection" /> to the underlying data source.
        /// </returns>
        public new virtual EntityConnection Connection
        {
            get { return (EntityConnection)DbConnection; }
        }

        /// <summary>
        /// The connection object owning this transaction object
        /// </summary>
        protected override DbConnection DbConnection
        {
            // follow the store transaction behavior
            get { return (((_storeTransaction != null ? _storeTransaction.Connection : null) != null) ? _connection : null); }
        }

        /// <summary>
        /// Gets the isolation level of this <see cref="T:System.Data.Entity.Core.EntityClient.EntityTransaction" />.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.IsolationLevel" /> enumeration value that represents the isolation level of the underlying transaction.
        /// </returns>
        public override IsolationLevel IsolationLevel
        {
            get
            {
                return _storeTransaction != null
                           ? _storeTransaction.IsolationLevel
                           : default(IsolationLevel);
            }
        }

        /// <summary>
        /// Gets the DbTransaction for the underlying provider transaction
        /// </summary>
        internal virtual DbTransaction StoreTransaction
        {
            get { return _storeTransaction; }
        }

        /// <summary>Commits the underlying transaction.</summary>
        public override void Commit()
        {
            try
            {
                if (_storeTransaction != null)
                {
                    _storeTransaction.Commit();
                }
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

        /// <summary>Rolls back the underlying transaction.</summary>
        public override void Rollback()
        {
            try
            {
                if (_storeTransaction != null)
                {
                    _storeTransaction.Rollback();
                }
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
        /// <param name="disposing"> true to release both managed and unmanaged resources; false to release only unmanaged resources </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearCurrentTransaction();

                if (_storeTransaction != null)
                {
                    _storeTransaction.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Helper method to wrap EntityConnection.ClearCurrentTransaction()
        /// </summary>
        private void ClearCurrentTransaction()
        {
            if ((_connection != null)
                && (_connection.CurrentTransaction == this))
            {
                _connection.ClearCurrentTransaction();
            }
        }
    }
}
