// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Used for dispatching operations to a <see cref="DbTransaction" /> such that any <see cref="IDbTransactionInterceptor" />
    /// registered on <see cref="DbInterception" /> will be notified before and after the
    /// operation executes.
    /// Instances of this class are obtained through the the <see cref="DbInterception.Dispatch" /> fluent API.
    /// </summary>
    /// <remarks>
    /// This class is used internally by Entity Framework when interacting with <see cref="DbTransaction" />.
    /// It is provided publicly so that code that runs outside of the core EF assemblies can opt-in to command
    /// interception/tracing. This is typically done by EF providers that are executing commands on behalf of EF.
    /// </remarks>
    public class DbTransactionDispatcher
    {
        private readonly InternalDispatcher<IDbTransactionInterceptor> _internalDispatcher
            = new InternalDispatcher<IDbTransactionInterceptor>();

        internal InternalDispatcher<IDbTransactionInterceptor> InternalDispatcher
        {
            get { return _internalDispatcher; }
        }

        internal DbTransactionDispatcher()
        {
        }

        /// <summary>
        /// Sends <see cref="IDbTransactionInterceptor.ConnectionGetting" /> and
        /// <see cref="IDbTransactionInterceptor.ConnectionGot" /> to any <see cref="IDbTransactionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after
        /// getting <see cref="DbTransaction.Connection" />.
        /// </summary>
        /// <remarks>
        /// Note that the value of the property is returned by this method. The result is not available
        /// in the interception context passed into this method since the interception context is cloned before
        /// being passed to interceptors.
        /// </remarks>
        /// <param name="transaction">The transaction on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual DbConnection GetConnection(DbTransaction transaction, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(transaction, "transaction");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbTransactionInterceptionContext<DbConnection>(interceptionContext);

            return InternalDispatcher.Dispatch(
                () => transaction.Connection,
                clonedInterceptionContext,
                i => i.ConnectionGetting(transaction, clonedInterceptionContext),
                i => i.ConnectionGot(transaction, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbTransactionInterceptor.IsolationLevelGetting" /> and
        /// <see cref="IDbTransactionInterceptor.IsolationLevelGot" /> to any <see cref="IDbTransactionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after
        /// getting <see cref="DbTransaction.IsolationLevel" />.
        /// </summary>
        /// <remarks>
        /// Note that the value of the property is returned by this method. The result is not available
        /// in the interception context passed into this method since the interception context is cloned before
        /// being passed to interceptors.
        /// </remarks>
        /// <param name="transaction">The transaction on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual IsolationLevel GetIsolationLevel(DbTransaction transaction, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(transaction, "transaction");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbTransactionInterceptionContext<IsolationLevel>(interceptionContext);

            return InternalDispatcher.Dispatch(
                () => transaction.IsolationLevel,
                clonedInterceptionContext,
                i => i.IsolationLevelGetting(transaction, clonedInterceptionContext),
                i => i.IsolationLevelGot(transaction, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbTransactionInterceptor.Committing" /> and
        /// <see cref="IDbTransactionInterceptor.Committed" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after making a
        /// call to <see cref="DbTransaction.Commit" />.
        /// </summary>
        /// <param name="transaction">The transaction on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        public virtual void Commit(DbTransaction transaction, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(transaction, "transaction");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbTransactionInterceptionContext(interceptionContext);

            InternalDispatcher.Dispatch(
                transaction.Commit,
                clonedInterceptionContext,
                i => i.Committing(transaction, clonedInterceptionContext),
                i => i.Committed(transaction, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbTransactionInterceptor.Disposing" /> and
        /// <see cref="IDbTransactionInterceptor.Disposed" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after making a
        /// call to <see cref="DbTransaction.Dispose()" />.
        /// </summary>
        /// <param name="transaction">The transaction on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        public virtual void Dispose(DbTransaction transaction, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(transaction, "transaction");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbTransactionInterceptionContext(interceptionContext);

            InternalDispatcher.Dispatch(
                transaction.Dispose,
                clonedInterceptionContext,
                i => i.Disposing(transaction, clonedInterceptionContext),
                i => i.Disposed(transaction, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbTransactionInterceptor.RollingBack" /> and
        /// <see cref="IDbTransactionInterceptor.RolledBack" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after making a
        /// call to <see cref="DbTransaction.Rollback" />.
        /// </summary>
        /// <param name="transaction">The transaction on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        public virtual void Rollback(DbTransaction transaction, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(transaction, "transaction");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbTransactionInterceptionContext(interceptionContext);

            InternalDispatcher.Dispatch(
                transaction.Rollback,
                clonedInterceptionContext,
                i => i.RollingBack(transaction, clonedInterceptionContext),
                i => i.RolledBack(transaction, clonedInterceptionContext));
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
