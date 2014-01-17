// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Used for dispatching operations to a <see cref="DbConnection" /> such that any <see cref="IDbConnectionInterceptor" />
    /// registered on <see cref="DbInterception" /> will be notified before and after the
    /// operation executes.
    /// Instances of this class are obtained through the the <see cref="DbInterception.Dispatch" /> fluent API.
    /// </summary>
    /// <remarks>
    /// This class is used internally by Entity Framework when interacting with <see cref="DbConnection" />.
    /// It is provided publicly so that code that runs outside of the core EF assemblies can opt-in to command
    /// interception/tracing. This is typically done by EF providers that are executing commands on behalf of EF.
    /// </remarks>
    public class DbConnectionDispatcher
    {
        private readonly InternalDispatcher<IDbConnectionInterceptor> _internalDispatcher
            = new InternalDispatcher<IDbConnectionInterceptor>();

        internal InternalDispatcher<IDbConnectionInterceptor> InternalDispatcher
        {
            get { return _internalDispatcher; }
        }

        internal DbConnectionDispatcher()
        {
        }

        /// <summary>
        /// Sends <see cref="IDbConnectionInterceptor.BeginningTransaction" /> and
        /// <see cref="IDbConnectionInterceptor.BeganTransaction" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after making a
        /// call to <see cref="DbConnection.BeginTransaction(IsolationLevel)" />.
        /// </summary>
        /// <remarks>
        /// Note that the result of executing the command is returned by this method. The result is not available
        /// in the interception context passed into this method since the interception context is cloned before
        /// being passed to interceptors.
        /// </remarks>
        /// <param name="connection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual DbTransaction BeginTransaction(
            DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new BeginTransactionInterceptionContext(interceptionContext);

            return InternalDispatcher.Dispatch(
                () => connection.BeginTransaction(clonedInterceptionContext.IsolationLevel),
                clonedInterceptionContext,
                i => i.BeginningTransaction(connection, clonedInterceptionContext),
                i => i.BeganTransaction(connection, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbConnectionInterceptor.Closing" /> and
        /// <see cref="IDbConnectionInterceptor.Closed" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after making a
        /// call to <see cref="DbConnection.Close" />.
        /// </summary>
        /// <param name="connection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        public virtual void Close(
            DbConnection connection, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionInterceptionContext(interceptionContext);

            InternalDispatcher.Dispatch(
                () => connection.Close(),
                clonedInterceptionContext,
                i => i.Closing(connection, clonedInterceptionContext),
                i => i.Closed(connection, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbConnectionInterceptor.Disposing" /> and
        /// <see cref="IDbConnectionInterceptor.Disposed" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after making a
        /// call to <see cref="Component.Dispose()" />.
        /// </summary>
        /// <param name="connection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        public virtual void Dispose(
            DbConnection connection, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionInterceptionContext(interceptionContext);

            InternalDispatcher.Dispatch(
                () =>
                {
                    // using dynamic here to emulate the behavior of the using statement
                    dynamic dynamicConnection = connection;
                    dynamicConnection.Dispose();
                },
                clonedInterceptionContext,
                i => i.Disposing(connection, clonedInterceptionContext),
                i => i.Disposed(connection, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbConnectionInterceptor.ConnectionStringGetting" /> and
        /// <see cref="IDbConnectionInterceptor.ConnectionStringGot" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after
        /// getting <see cref="DbConnection.ConnectionString" />.
        /// </summary>
        /// <remarks>
        /// Note that the value of the property is returned by this method. The result is not available
        /// in the interception context passed into this method since the interception context is cloned before
        /// being passed to interceptors.
        /// </remarks>
        /// <param name="connection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual string GetConnectionString(DbConnection connection, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionInterceptionContext<string>(interceptionContext);

            return InternalDispatcher.Dispatch(
                () => connection.ConnectionString,
                clonedInterceptionContext,
                i => i.ConnectionStringGetting(connection, clonedInterceptionContext),
                i => i.ConnectionStringGot(connection, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbConnectionInterceptor.ConnectionStringSetting" /> and
        /// <see cref="IDbConnectionInterceptor.ConnectionStringSet" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after
        /// setting <see cref="DbConnection.ConnectionString" />.
        /// </summary>
        /// <param name="connection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Information about the context of the call being made, including the value to be set.</param>
        public virtual void SetConnectionString(
            DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionPropertyInterceptionContext<string>(interceptionContext);

            InternalDispatcher.Dispatch<DbConnectionPropertyInterceptionContext<string>>(
                () => connection.ConnectionString = clonedInterceptionContext.Value,
                clonedInterceptionContext,
                i => i.ConnectionStringSetting(connection, clonedInterceptionContext),
                i => i.ConnectionStringSet(connection, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbConnectionInterceptor.ConnectionTimeoutGetting" /> and
        /// <see cref="IDbConnectionInterceptor.ConnectionTimeoutGot" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after
        /// getting <see cref="DbConnection.ConnectionTimeout" />.
        /// </summary>
        /// <remarks>
        /// Note that the value of the property is returned by this method. The result is not available
        /// in the interception context passed into this method since the interception context is cloned before
        /// being passed to interceptors.
        /// </remarks>
        /// <param name="connection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual int GetConnectionTimeout(DbConnection connection, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionInterceptionContext<int>(interceptionContext);

            return InternalDispatcher.Dispatch(
                () => connection.ConnectionTimeout,
                clonedInterceptionContext,
                i => i.ConnectionTimeoutGetting(connection, clonedInterceptionContext),
                i => i.ConnectionTimeoutGot(connection, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbConnectionInterceptor.DatabaseGetting" /> and
        /// <see cref="IDbConnectionInterceptor.DatabaseGot" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after
        /// getting <see cref="DbConnection.Database" />.
        /// </summary>
        /// <remarks>
        /// Note that the value of the property is returned by this method. The result is not available
        /// in the interception context passed into this method since the interception context is cloned before
        /// being passed to interceptors.
        /// </remarks>
        /// <param name="connection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual string GetDatabase(DbConnection connection, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionInterceptionContext<string>(interceptionContext);

            return InternalDispatcher.Dispatch(
                () => connection.Database,
                clonedInterceptionContext,
                i => i.DatabaseGetting(connection, clonedInterceptionContext),
                i => i.DatabaseGot(connection, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbConnectionInterceptor.DataSourceGetting" /> and
        /// <see cref="IDbConnectionInterceptor.DataSourceGot" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after
        /// getting <see cref="DbConnection.DataSource" />.
        /// </summary>
        /// <remarks>
        /// Note that the value of the property is returned by this method. The result is not available
        /// in the interception context passed into this method since the interception context is cloned before
        /// being passed to interceptors.
        /// </remarks>
        /// <param name="connection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual string GetDataSource(DbConnection connection, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionInterceptionContext<string>(interceptionContext);

            return InternalDispatcher.Dispatch(
                () => connection.DataSource,
                clonedInterceptionContext,
                i => i.DataSourceGetting(connection, clonedInterceptionContext),
                i => i.DataSourceGot(connection, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbConnectionInterceptor.EnlistingTransaction" /> and
        /// <see cref="IDbConnectionInterceptor.EnlistedTransaction" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after making a
        /// call to <see cref="DbConnection.EnlistTransaction" />.
        /// </summary>
        /// <param name="connection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        public virtual void EnlistTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new EnlistTransactionInterceptionContext(interceptionContext);

            InternalDispatcher.Dispatch(
                () => connection.EnlistTransaction(clonedInterceptionContext.Transaction),
                clonedInterceptionContext,
                i => i.EnlistingTransaction(connection, clonedInterceptionContext),
                i => i.EnlistedTransaction(connection, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbConnectionInterceptor.Opening" /> and
        /// <see cref="IDbConnectionInterceptor.Opened" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after making a
        /// call to <see cref="DbConnection.Open" />.
        /// </summary>
        /// <param name="connection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        public virtual void Open(
            DbConnection connection, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionInterceptionContext(interceptionContext);

            InternalDispatcher.Dispatch(
                () => connection.Open(),
                clonedInterceptionContext,
                i => i.Opening(connection, clonedInterceptionContext),
                i => i.Opened(connection, clonedInterceptionContext));
        }

#if !NET40
        /// <summary>
        /// Sends <see cref="IDbConnectionInterceptor.Opening" /> and
        /// <see cref="IDbConnectionInterceptor.Opened" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after making a
        /// call to <see cref="DbConnection.Open" />.
        /// </summary>
        /// <param name="connection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public virtual Task OpenAsync(
            DbConnection connection, DbInterceptionContext interceptionContext, CancellationToken cancellationToken)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionInterceptionContext(interceptionContext);

            if (!clonedInterceptionContext.IsAsync)
            {
                clonedInterceptionContext = clonedInterceptionContext.AsAsync();
            }

            return InternalDispatcher.DispatchAsync(
                () => connection.OpenAsync(cancellationToken),
                clonedInterceptionContext,
                i => i.Opening(connection, clonedInterceptionContext),
                i => i.Opened(connection, clonedInterceptionContext));
        }
#endif

        /// <summary>
        /// Sends <see cref="IDbConnectionInterceptor.ServerVersionGetting" /> and
        /// <see cref="IDbConnectionInterceptor.ServerVersionGot" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after
        /// getting <see cref="DbConnection.ServerVersion" />.
        /// </summary>
        /// <remarks>
        /// Note that the value of the property is returned by this method. The result is not available
        /// in the interception context passed into this method since the interception context is cloned before
        /// being passed to interceptors.
        /// </remarks>
        /// <param name="connection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual string GetServerVersion(DbConnection connection, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionInterceptionContext<string>(interceptionContext);

            return InternalDispatcher.Dispatch(
                () => connection.ServerVersion,
                clonedInterceptionContext,
                i => i.ServerVersionGetting(connection, clonedInterceptionContext),
                i => i.ServerVersionGot(connection, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbConnectionInterceptor.StateGetting" /> and
        /// <see cref="IDbConnectionInterceptor.StateGot" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after
        /// getting <see cref="DbConnection.State" />.
        /// </summary>
        /// <remarks>
        /// Note that the value of the property is returned by this method. The result is not available
        /// in the interception context passed into this method since the interception context is cloned before
        /// being passed to interceptors.
        /// </remarks>
        /// <param name="connection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual ConnectionState GetState(DbConnection connection, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionInterceptionContext<ConnectionState>(interceptionContext);

            return InternalDispatcher.Dispatch(
                () => connection.State,
                clonedInterceptionContext,
                i => i.StateGetting(connection, clonedInterceptionContext),
                i => i.StateGot(connection, clonedInterceptionContext));
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
