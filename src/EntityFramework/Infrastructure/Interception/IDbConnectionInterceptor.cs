// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Common;

    /// <summary>
    /// An object that implements this interface can be registered with <see cref="DbInterception" /> to
    /// receive notifications when Entity Framework performs operations on a <see cref="DbTransaction"/>.
    /// </summary>
    public interface IDbConnectionInterceptor : IDbInterceptor
    {
        /// <summary>
        /// Called before <see cref="DbConnection.BeginTransaction(Data.IsolationLevel)" /> is invoked.
        /// </summary>
        /// <param name="connection">The connection beginning the transaction.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void BeginningTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext);

        /// <summary>
        /// Called after <see cref="DbConnection.BeginTransaction(Data.IsolationLevel)" /> is invoked.
        /// The transaction used by Entity Framework can be changed by setting
        /// <see cref="MutableInterceptionContext{TResult}.Result" />.
        /// </summary>
        /// <param name="connection">The connection that began the transaction.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void BeganTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext);

        /// <summary>
        /// Called before <see cref="DbConnection.Close" /> is invoked.
        /// </summary>
        /// <param name="connection">The connection being closed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void Closing(DbConnection connection, DbConnectionInterceptionContext interceptionContext);

        /// <summary>
        /// Called after <see cref="DbConnection.Close" /> is invoked.
        /// </summary>
        /// <param name="connection">The connection that was closed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void Closed(DbConnection connection, DbConnectionInterceptionContext interceptionContext);

        /// <summary>
        /// Called before <see cref="DbConnection.ConnectionString" /> is retrieved.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void ConnectionStringGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

        /// <summary>
        /// Called after <see cref="DbConnection.ConnectionString" /> is retrieved.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void ConnectionStringGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

        /// <summary>
        /// Called before <see cref="DbConnection.ConnectionString" /> is set.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void ConnectionStringSetting(DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext);

        /// <summary>
        /// Called after <see cref="DbConnection.ConnectionString" /> is set.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void ConnectionStringSet(DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext);

        /// <summary>
        /// Called before <see cref="DbConnection.ConnectionTimeout" /> is retrieved.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void ConnectionTimeoutGetting(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext);

        /// <summary>
        /// Called after <see cref="DbConnection.ConnectionTimeout" /> is retrieved.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void ConnectionTimeoutGot(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext);

        /// <summary>
        /// Called before <see cref="DbConnection.Database" /> is retrieved.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void DatabaseGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

        /// <summary>
        /// Called after <see cref="DbConnection.Database" /> is retrieved.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void DatabaseGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

        /// <summary>
        /// Called before <see cref="DbConnection.DataSource" /> is retrieved.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void DataSourceGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

        /// <summary>
        /// Called after <see cref="DbConnection.DataSource" /> is retrieved.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void DataSourceGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

        /// <summary>
        /// Called before <see cref="DbConnection.EnlistTransaction" /> is invoked.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void EnlistingTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext);

        /// <summary>
        /// Called after <see cref="DbConnection.EnlistTransaction" /> is invoked.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void EnlistedTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext);

        /// <summary>
        /// Called before <see cref="DbConnection.Open" /> or its async counterpart is invoked.
        /// </summary>
        /// <param name="connection">The connection being opened.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void Opening(DbConnection connection, DbConnectionInterceptionContext interceptionContext);

        /// <summary>
        /// Called after <see cref="DbConnection.Open" /> or its async counterpart is invoked.
        /// </summary>
        /// <param name="connection">The connection that was opened.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void Opened(DbConnection connection, DbConnectionInterceptionContext interceptionContext);

        /// <summary>
        /// Called before <see cref="DbConnection.ServerVersion" /> is retrieved.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void ServerVersionGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

        /// <summary>
        /// Called after <see cref="DbConnection.ServerVersion" /> is retrieved.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void ServerVersionGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

        /// <summary>
        /// Called before <see cref="DbConnection.State" /> is retrieved.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void StateGetting(DbConnection connection, DbConnectionInterceptionContext<ConnectionState> interceptionContext);

        /// <summary>
        /// Called after <see cref="DbConnection.State" /> is retrieved.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void StateGot(DbConnection connection, DbConnectionInterceptionContext<ConnectionState> interceptionContext);
    }
}
