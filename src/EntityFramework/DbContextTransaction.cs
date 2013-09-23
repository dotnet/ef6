// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Wraps access to the transaction object on the underlying store connection and ensures that the
    /// Entity Framework executes commands on the database within the context of that transaction.
    /// An instance of this class is retrieved by calling BeginTransaction() on the <see cref="DbContext" />
    /// <see
    ///     cref="Database" />
    /// object.
    /// </summary>
    public class DbContextTransaction : IDisposable
    {
        private readonly EntityConnection _connection;
        private readonly EntityTransaction _entityTransaction;
        private bool _shouldCloseConnection;
        private bool _isDisposed;

        // <summary>
        // Constructs the DbContextTransaction object with the associated connection object
        // </summary>
        // <param name="connection">The EntityConnection object owning this transaction</param>
        internal DbContextTransaction(EntityConnection connection)
        {
            DebugCheck.NotNull(connection);
            _connection = connection;
            EnsureOpenConnection();
            _entityTransaction = _connection.BeginTransaction();
        }

        // <summary>
        // Constructs the DbContextTransaction object with the associated connection object
        // and with the given isolation level
        // </summary>
        // <param name="connection">The EntityConnection object owning this transaction </param>
        // <param name="isolationLevel">The database isolation level with which the underlying store transaction will be created</param>
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
        /// Gets the database (store) transaction that is underlying this context transaction.
        /// </summary>
        public DbTransaction UnderlyingTransaction
        {
            get { return _entityTransaction.StoreTransaction; }
        }

        /// <summary>
        /// Commits the underlying store transaction
        /// </summary>
        public void Commit()
        {
            _entityTransaction.Commit();
        }

        /// <summary>
        /// Rolls back the underlying store transaction
        /// </summary>
        public void Rollback()
        {
            _entityTransaction.Rollback();
        }

        /// <summary>
        /// Cleans up this transaction object and ensures the Entity Framework
        /// is no longer using that transaction.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cleans up this transaction object
        /// </summary>
        /// <param name="disposing"> true to release both managed and unmanaged resources; false to release only unmanaged resources </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_isDisposed)
                {
                    _connection.ClearCurrentTransaction();

                    _entityTransaction.Dispose();

                    if (_shouldCloseConnection)
                    {
                        if (ConnectionState.Closed != _connection.State)
                        {
                            _connection.Close();
                        }
                    }

                    _isDisposed = true;
                }
            }
        }

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

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
