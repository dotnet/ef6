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
        /// <param name="dbConnection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual DbTransaction BeginTransaction(
            DbConnection dbConnection, BeginTransactionInterceptionContext interceptionContext)
        {
            Check.NotNull(dbConnection, "dbConnection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new BeginTransactionInterceptionContext(interceptionContext);

            return InternalDispatcher.Dispatch(
                () => dbConnection.BeginTransaction(clonedInterceptionContext.IsolationLevel),
                clonedInterceptionContext,
                i => i.BeginningTransaction(dbConnection, clonedInterceptionContext),
                i => i.BeganTransaction(dbConnection, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbConnectionInterceptor.Closing" /> and
        /// <see cref="IDbConnectionInterceptor.Closed" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after making a
        /// call to <see cref="DbConnection.Close" />.
        /// </summary>
        /// <param name="dbConnection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        public virtual void Close(
            DbConnection dbConnection, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(dbConnection, "dbConnection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionInterceptionContext(interceptionContext);

            InternalDispatcher.Dispatch(
                () => dbConnection.Close(),
                clonedInterceptionContext,
                i => i.Closing(dbConnection, clonedInterceptionContext),
                i => i.Closed(dbConnection, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbConnectionInterceptor.Disposing" /> and
        /// <see cref="IDbConnectionInterceptor.Disposed" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after making a
        /// call to <see cref="Component.Dispose()" />.
        /// </summary>
        /// <param name="dbConnection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        public virtual void Dispose(
            DbConnection dbConnection, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(dbConnection, "dbConnection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionInterceptionContext(interceptionContext);

            InternalDispatcher.Dispatch(
                () =>
                {
                    // using dynamic here to emulate the behavior of the using statement
                    dynamic connection = dbConnection;
                    connection.Dispose();
                },
                clonedInterceptionContext,
                i => i.Disposing(dbConnection, clonedInterceptionContext),
                i => i.Disposed(dbConnection, clonedInterceptionContext));
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
        /// <param name="dbConnection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual string GetConnectionString(DbConnection dbConnection, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(dbConnection, "dbConnection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionInterceptionContext<string>(interceptionContext);

            return InternalDispatcher.Dispatch(
                () => dbConnection.ConnectionString,
                clonedInterceptionContext,
                i => i.ConnectionStringGetting(dbConnection, clonedInterceptionContext),
                i => i.ConnectionStringGot(dbConnection, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbConnectionInterceptor.ConnectionStringSetting" /> and
        /// <see cref="IDbConnectionInterceptor.ConnectionStringSet" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after
        /// setting <see cref="DbConnection.ConnectionString" />.
        /// </summary>
        /// <param name="dbConnection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Information about the context of the call being made, including the value to be set.</param>
        public virtual void SetConnectionString(
            DbConnection dbConnection, DbConnectionPropertyInterceptionContext<string> interceptionContext)
        {
            Check.NotNull(dbConnection, "dbConnection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionPropertyInterceptionContext<string>(interceptionContext);

            InternalDispatcher.Dispatch<DbConnectionPropertyInterceptionContext<string>>(
                () => dbConnection.ConnectionString = clonedInterceptionContext.Value,
                clonedInterceptionContext,
                i => i.ConnectionStringSetting(dbConnection, clonedInterceptionContext),
                i => i.ConnectionStringSet(dbConnection, clonedInterceptionContext));
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
        /// <param name="dbConnection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual int GetConnectionTimeout(DbConnection dbConnection, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(dbConnection, "dbConnection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionInterceptionContext<int>(interceptionContext);

            return InternalDispatcher.Dispatch(
                () => dbConnection.ConnectionTimeout,
                clonedInterceptionContext,
                i => i.ConnectionTimeoutGetting(dbConnection, clonedInterceptionContext),
                i => i.ConnectionTimeoutGot(dbConnection, clonedInterceptionContext));
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
        /// <param name="dbConnection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual string GetDatabase(DbConnection dbConnection, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(dbConnection, "dbConnection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionInterceptionContext<string>(interceptionContext);

            return InternalDispatcher.Dispatch(
                () => dbConnection.Database,
                clonedInterceptionContext,
                i => i.DatabaseGetting(dbConnection, clonedInterceptionContext),
                i => i.DatabaseGot(dbConnection, clonedInterceptionContext));
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
        /// <param name="dbConnection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual string GetDataSource(DbConnection dbConnection, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(dbConnection, "dbConnection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionInterceptionContext<string>(interceptionContext);

            return InternalDispatcher.Dispatch(
                () => dbConnection.DataSource,
                clonedInterceptionContext,
                i => i.DataSourceGetting(dbConnection, clonedInterceptionContext),
                i => i.DataSourceGot(dbConnection, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbConnectionInterceptor.EnlistingTransaction" /> and
        /// <see cref="IDbConnectionInterceptor.EnlistedTransaction" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after making a
        /// call to <see cref="DbConnection.EnlistTransaction" />.
        /// </summary>
        /// <param name="dbConnection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        public virtual void EnlistTransaction(DbConnection dbConnection, EnlistTransactionInterceptionContext interceptionContext)
        {
            Check.NotNull(dbConnection, "dbConnection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new EnlistTransactionInterceptionContext(interceptionContext);

            InternalDispatcher.Dispatch(
                () => dbConnection.EnlistTransaction(clonedInterceptionContext.Transaction),
                clonedInterceptionContext,
                i => i.EnlistingTransaction(dbConnection, clonedInterceptionContext),
                i => i.EnlistedTransaction(dbConnection, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbConnectionInterceptor.Opening" /> and
        /// <see cref="IDbConnectionInterceptor.Opened" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after making a
        /// call to <see cref="DbConnection.Open" />.
        /// </summary>
        /// <param name="dbConnection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        public virtual void Open(
            DbConnection dbConnection, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(dbConnection, "dbConnection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionInterceptionContext(interceptionContext);

            InternalDispatcher.Dispatch(
                () => dbConnection.Open(),
                clonedInterceptionContext,
                i => i.Opening(dbConnection, clonedInterceptionContext),
                i => i.Opened(dbConnection, clonedInterceptionContext));
        }

#if !NET40
        /// <summary>
        /// Sends <see cref="IDbConnectionInterceptor.Opening" /> and
        /// <see cref="IDbConnectionInterceptor.Opened" /> to any <see cref="IDbConnectionInterceptor" />
        /// registered on <see cref="DbInterception" /> before/after making a
        /// call to <see cref="DbConnection.Open" />.
        /// </summary>
        /// <param name="dbConnection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public virtual Task OpenAsync(
            DbConnection dbConnection, DbInterceptionContext interceptionContext, CancellationToken cancellationToken)
        {
            Check.NotNull(dbConnection, "dbConnection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionInterceptionContext(interceptionContext);

            if (!clonedInterceptionContext.IsAsync)
            {
                clonedInterceptionContext = clonedInterceptionContext.AsAsync();
            }

            return InternalDispatcher.DispatchAsync(
                () => dbConnection.OpenAsync(cancellationToken),
                clonedInterceptionContext,
                i => i.Opening(dbConnection, clonedInterceptionContext),
                i => i.Opened(dbConnection, clonedInterceptionContext));
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
        /// <param name="dbConnection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual string GetServerVersion(DbConnection dbConnection, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(dbConnection, "dbConnection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionInterceptionContext<string>(interceptionContext);

            return InternalDispatcher.Dispatch(
                () => dbConnection.ServerVersion,
                clonedInterceptionContext,
                i => i.ServerVersionGetting(dbConnection, clonedInterceptionContext),
                i => i.ServerVersionGot(dbConnection, clonedInterceptionContext));
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
        /// <param name="dbConnection">The connection on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual ConnectionState GetState(DbConnection dbConnection, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(dbConnection, "dbConnection");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbConnectionInterceptionContext<ConnectionState>(interceptionContext);

            return InternalDispatcher.Dispatch(
                () => dbConnection.State,
                clonedInterceptionContext,
                i => i.StateGetting(dbConnection, clonedInterceptionContext),
                i => i.StateGot(dbConnection, clonedInterceptionContext));
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
