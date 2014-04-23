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

            return InternalDispatcher.Dispatch(
                connection,
                (t, c) => t.BeginTransaction(c.IsolationLevel),
                new BeginTransactionInterceptionContext(interceptionContext),
                (i, t, c) => i.BeginningTransaction(t, c),
                (i, t, c) => i.BeganTransaction(t, c));
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

            InternalDispatcher.Dispatch(
                connection,
                (t, c) => t.Close(),
                new DbConnectionInterceptionContext(interceptionContext),
                (i, t, c) => i.Closing(t, c),
                (i, t, c) => i.Closed(t, c));
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

            InternalDispatcher.Dispatch(
                connection,
                (t, c) =>
                {
                    // Will invoke the explicit IDisposable implementation if one exists
                    using (t)
                    {
                    }
                },
                new DbConnectionInterceptionContext(interceptionContext),
                (i, t, c) => i.Disposing(t, c),
                (i, t, c) => i.Disposed(t, c));
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

            return InternalDispatcher.Dispatch(
                connection,
                (t, c) => t.ConnectionString,
                new DbConnectionInterceptionContext<string>(interceptionContext),
                (i, t, c) => i.ConnectionStringGetting(t, c),
                (i, t, c) => i.ConnectionStringGot(t, c));
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

            InternalDispatcher.Dispatch<DbConnection, DbConnectionPropertyInterceptionContext<string>>(
                connection,
                (t, c) => t.ConnectionString = c.Value,
                new DbConnectionPropertyInterceptionContext<string>(interceptionContext),
                (i, t, c) => i.ConnectionStringSetting(t, c),
                (i, t, c) => i.ConnectionStringSet(t, c));
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

            return InternalDispatcher.Dispatch(
                connection,
                (t, c) => t.ConnectionTimeout,
                new DbConnectionInterceptionContext<int>(interceptionContext),
                (i, t, c) => i.ConnectionTimeoutGetting(t, c),
                (i, t, c) => i.ConnectionTimeoutGot(t, c));
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

            return InternalDispatcher.Dispatch(
                connection,
                (t, c) => t.Database,
                new DbConnectionInterceptionContext<string>(interceptionContext),
                (i, t, c) => i.DatabaseGetting(t, c),
                (i, t, c) => i.DatabaseGot(t, c));
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

            return InternalDispatcher.Dispatch(
                connection,
                (t, c) => t.DataSource,
                new DbConnectionInterceptionContext<string>(interceptionContext),
                (i, t, c) => i.DataSourceGetting(t, c),
                (i, t, c) => i.DataSourceGot(t, c));
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

            InternalDispatcher.Dispatch(
                connection,
                (t, c) => t.EnlistTransaction(c.Transaction),
                new EnlistTransactionInterceptionContext(interceptionContext),
                (i, t, c) => i.EnlistingTransaction(t, c),
                (i, t, c) => i.EnlistedTransaction(t, c));
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

            InternalDispatcher.Dispatch(
                connection,
                (t, c) => t.Open(),
                new DbConnectionInterceptionContext(interceptionContext),
                (i, t, c) => i.Opening(t, c),
                (i, t, c) => i.Opened(t, c));
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

            return InternalDispatcher.DispatchAsync(
                connection,
                (t, c, ct) => t.OpenAsync(ct),
                new DbConnectionInterceptionContext(interceptionContext).AsAsync(),
                (i, t, c) => i.Opening(t, c),
                (i, t, c) => i.Opened(t, c),
                cancellationToken);
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

            return InternalDispatcher.Dispatch(
                connection,
                (t, c) => t.ServerVersion,
                new DbConnectionInterceptionContext<string>(interceptionContext),
                (i, t, c) => i.ServerVersionGetting(t, c),
                (i, t, c) => i.ServerVersionGot(t, c));
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

            return InternalDispatcher.Dispatch(
                connection,
                (t, c) => t.State,
                new DbConnectionInterceptionContext<ConnectionState>(interceptionContext),
                (i, t, c) => i.StateGetting(t, c),
                (i, t, c) => i.StateGot(t, c));
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
