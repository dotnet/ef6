// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Provides access to all dispatchers through the the <see cref="DbInterception.Dispatch" /> fluent API.
    /// </summary>
    public class DbDispatchers
    {
        private readonly DbCommandTreeDispatcher _commandTreeDispatcher = new DbCommandTreeDispatcher();
        private readonly DbCommandDispatcher _commandDispatcher = new DbCommandDispatcher();
        private readonly DbTransactionDispatcher _transactionDispatcher = new DbTransactionDispatcher();
        private readonly DbConnectionDispatcher _dbConnectionDispatcher = new DbConnectionDispatcher();

        private readonly CancelableEntityConnectionDispatcher _cancelableEntityConnectionDispatcher =
            new CancelableEntityConnectionDispatcher();

        private readonly CancelableDbCommandDispatcher _cancelableCommandDispatcher = new CancelableDbCommandDispatcher();

        internal DbDispatchers()
        {
        }

        internal virtual DbCommandTreeDispatcher CommandTree
        {
            get { return _commandTreeDispatcher; }
        }

        /// <summary>
        /// Provides methods for dispatching to <see cref="IDbCommandInterceptor" /> interceptors for
        /// interception of methods on <see cref="DbCommand" />.
        /// </summary>
        public virtual DbCommandDispatcher Command
        {
            get { return _commandDispatcher; }
        }

        /// <summary>
        /// Provides methods for dispatching to <see cref="IDbCommandInterceptor" /> interceptors for
        /// interception of methods on <see cref="DbTransaction" />.
        /// </summary>
        public virtual DbTransactionDispatcher Transaction
        {
            get { return _transactionDispatcher; }
        }

        /// <summary>
        /// Provides methods for dispatching to <see cref="IDbCommandInterceptor" /> interceptors for
        /// interception of methods on <see cref="DbConnection" />.
        /// </summary>
        public virtual DbConnectionDispatcher Connection
        {
            get { return _dbConnectionDispatcher; }
        }

        internal virtual CancelableEntityConnectionDispatcher CancelableEntityConnection
        {
            get { return _cancelableEntityConnectionDispatcher; }
        }

        internal virtual CancelableDbCommandDispatcher CancelableCommand
        {
            get { return _cancelableCommandDispatcher; }
        }

        internal virtual void AddInterceptor(IDbInterceptor interceptor)
        {
            DebugCheck.NotNull(interceptor);

            _commandTreeDispatcher.InternalDispatcher.Add(interceptor);
            _commandDispatcher.InternalDispatcher.Add(interceptor);
            _transactionDispatcher.InternalDispatcher.Add(interceptor);
            _dbConnectionDispatcher.InternalDispatcher.Add(interceptor);
            _cancelableEntityConnectionDispatcher.InternalDispatcher.Add(interceptor);
            _cancelableCommandDispatcher.InternalDispatcher.Add(interceptor);
        }

        internal virtual void RemoveInterceptor(IDbInterceptor interceptor)
        {
            DebugCheck.NotNull(interceptor);

            _commandTreeDispatcher.InternalDispatcher.Remove(interceptor);
            _commandDispatcher.InternalDispatcher.Remove(interceptor);
            _transactionDispatcher.InternalDispatcher.Remove(interceptor);
            _dbConnectionDispatcher.InternalDispatcher.Remove(interceptor);
            _cancelableEntityConnectionDispatcher.InternalDispatcher.Remove(interceptor);
            _cancelableCommandDispatcher.InternalDispatcher.Remove(interceptor);
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

        /// <summary>
        /// Gets the <see cref="Type" /> of the current instance.
        /// </summary>
        /// <returns>The exact runtime type of the current instance.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
