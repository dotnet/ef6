// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Common;

    /// <summary>
    /// An object that implements this interface can be registered with <see cref="DbInterception" /> to
    /// receive notifications when Entity Framework commits or rollbacks a transaction.
    /// </summary>
    /// <remarks>
    /// Interceptors can also be registered in the config file of the application.
    /// See http://go.microsoft.com/fwlink/?LinkId=260883 for more information about Entity Framework configuration.
    /// </remarks>
    public interface IDbTransactionInterceptor : IDbInterceptor
    {
        /// <summary>
        /// Called before <see cref="DbTransaction.Connection" /> is retrieved.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void ConnectionGetting(DbTransaction transaction, DbTransactionInterceptionContext<DbConnection> interceptionContext);

        /// <summary>
        /// Called after <see cref="DbTransaction.Connection" /> is retrieved.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void ConnectionGot(DbTransaction transaction, DbTransactionInterceptionContext<DbConnection> interceptionContext);

        /// <summary>
        /// Called before <see cref="DbTransaction.IsolationLevel" /> is retrieved.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void IsolationLevelGetting(DbTransaction transaction, DbTransactionInterceptionContext<IsolationLevel> interceptionContext);

        /// <summary>
        /// Called after <see cref="DbTransaction.IsolationLevel" /> is retrieved.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void IsolationLevelGot(DbTransaction transaction, DbTransactionInterceptionContext<IsolationLevel> interceptionContext);

        /// <summary>
        /// This method is called before <see cref="DbTransaction.Commit" /> is invoked.
        /// </summary>
        /// <param name="transaction">The transaction being commited.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void Committing(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext);

        /// <summary>
        /// This method is called after <see cref="DbTransaction.Commit" /> is invoked.
        /// </summary>
        /// <param name="transaction">The transaction that was commited.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void Committed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext);

        /// <summary>
        /// This method is called before <see cref="DbTransaction.Dispose()" /> is invoked.
        /// </summary>
        /// <param name="transaction">The transaction being disposed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void Disposing(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext);

        /// <summary>
        /// This method is called after <see cref="DbTransaction.Dispose()" /> is invoked.
        /// </summary>
        /// <param name="transaction">The transaction that was disposed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void Disposed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext);

        /// <summary>
        /// This method is called before <see cref="DbTransaction.Rollback" /> is invoked.
        /// </summary>
        /// <param name="transaction">The transaction being rolled back.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void RollingBack(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext);

        /// <summary>
        /// This method is called after <see cref="DbTransaction.Rollback" /> is invoked.
        /// </summary>
        /// <param name="transaction">The transaction that was rolled back.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void RolledBack(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext);
    }
}
