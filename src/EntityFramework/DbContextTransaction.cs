// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Wraps access to the transaction object on the underlying store connection and ensures that the
    /// Entity Framework executes commands on the database within the context of that transaction.
    /// An instance of this class is retrieved by calling BeginTransaction() on the <see cref="DbContext"/> <see cref="Database"/> object.
    /// </summary>
    public class DbContextTransaction : IDisposable
    {
        private readonly EntityConnection _connection;
        private EntityTransaction _entityTransaction;
        private bool _shouldCloseConnection;

        /// <summary>
        ///     Constructs the DbContextTransaction object with the associated connection object
        /// </summary>
        /// <param name="connection">The EntityConnection object owning this transaction</param>
        internal DbContextTransaction(EntityConnection connection)
        {
            DebugCheck.NotNull(connection);
            _connection = connection;
            EnsureOpenConnection();
            _entityTransaction = _connection.BeginTransaction();
        }

        /// <summary>
        ///     Constructs the DbContextTransaction object with the associated connection object
        ///     and with the given isolation level
        /// </summary>
        /// <param name="connection">The EntityConnection object owning this transaction </param>
        /// <param name="isolationLevel">The database isolation level with which the underlying store transaction will be created</param>
        internal DbContextTransaction(EntityConnection connection, IsolationLevel isolationLevel)
        {
            DebugCheck.NotNull(connection);
            _connection = connection;
            EnsureOpenConnection();
            _entityTransaction = _connection.BeginTransaction(isolationLevel);
        }

        private void EnsureOpenConnection()
        {
            if (ConnectionState.Open != _connection.State)
            {
                _connection.Open();
                _shouldCloseConnection = true;
            }
        }

        /// <summary>
        ///     Gets the underlying store's transaction
        /// </summary>
        public DbTransaction StoreTransaction
        {
            get { return _entityTransaction == null ? null : _entityTransaction.StoreTransaction; }
        }

        /// <summary>
        ///     Commits the underlying store transaction
        /// </summary>
        public void Commit()
        {
            if (_entityTransaction != null)
            {
                _entityTransaction.Commit();
            }
        }

        /// <summary>
        ///     Rolls back the underlying store transaction
        /// </summary>
        public void Rollback()
        {
            if (_entityTransaction != null)
            {
                _entityTransaction.Rollback();
            }
        }

        /// <summary>
        ///     Cleans up this transaction object and ensures the Entity Framework
        ///     is no longer using that transaction.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Cleans up this transaction object
        /// </summary>
        /// <param name="disposing"> true to release both managed and unmanaged resources; false to release only unmanaged resources </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connection.ClearCurrentTransaction();

                if (null != _entityTransaction)
                {
                    _entityTransaction.Dispose();
                }

                _entityTransaction = null;

                if (_shouldCloseConnection)
                {
                    if (ConnectionState.Closed != _connection.State)
                    {
                        _connection.Close();
                    }
                }
            }
        }
    }
}
