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
    /// Used for dispatching operations to a <see cref="DbCommand" /> such that any <see cref="IDbCommandInterceptor" />
    /// interceptors that are registered on <see cref="DbInterception" /> will be notified before and after the
    /// operation executes.
    /// Instances of this class are obtained through the the <see cref="DbInterception.Dispatch" /> fluent API.
    /// </summary>
    /// <remarks>
    /// This class is used internally by Entity Framework when executing commands. It is provided publicly so that
    /// code that runs outside of the core EF assemblies can opt-in to command interception/tracing. This is
    /// typically done by EF providers that are executing commands on behalf of EF.
    /// </remarks>
    public class DbCommandDispatcher
    {
        private readonly InternalDispatcher<IDbCommandInterceptor> _internalDispatcher
            = new InternalDispatcher<IDbCommandInterceptor>();

        internal InternalDispatcher<IDbCommandInterceptor> InternalDispatcher
        {
            get { return _internalDispatcher; }
        }

        internal DbCommandDispatcher()
        {
        }

        /// <summary>
        /// Sends <see cref="IDbCommandInterceptor.NonQueryExecuting" /> and
        /// <see cref="IDbCommandInterceptor.NonQueryExecuted" /> to any  <see cref="IDbCommandInterceptor" />
        /// interceptors that are registered on <see cref="DbInterception" /> before/after making a
        /// call to <see cref="DbCommand.ExecuteNonQuery" />.
        /// </summary>
        /// <remarks>
        /// Note that the result of executing the command is returned by this method. The result is not available
        /// in the interception context passed into this method since the interception context is cloned before
        /// being passed to interceptors.
        /// </remarks>
        /// <param name="command">The command on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual int NonQuery(DbCommand command, DbCommandInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbCommandInterceptionContext<int>(interceptionContext);

            return _internalDispatcher.Dispatch(
                command.ExecuteNonQuery,
                clonedInterceptionContext,
                i => i.NonQueryExecuting(command, clonedInterceptionContext),
                i => i.NonQueryExecuted(command, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbCommandInterceptor.ScalarExecuting" /> and
        /// <see cref="IDbCommandInterceptor.ScalarExecuted" /> to any  <see cref="IDbCommandInterceptor" />
        /// interceptors that are registered on <see cref="DbInterception" /> before/after making a
        /// call to <see cref="DbCommand.ExecuteScalar" />.
        /// </summary>
        /// <remarks>
        /// Note that the result of executing the command is returned by this method. The result is not available
        /// in the interception context passed into this method since the interception context is cloned before
        /// being passed to interceptors.
        /// </remarks>
        /// <param name="command">The command on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual object Scalar(DbCommand command, DbCommandInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbCommandInterceptionContext<object>(interceptionContext);

            return _internalDispatcher.Dispatch(
                command.ExecuteScalar,
                clonedInterceptionContext,
                i => i.ScalarExecuting(command, clonedInterceptionContext),
                i => i.ScalarExecuted(command, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbCommandInterceptor.ReaderExecuting" /> and
        /// <see cref="IDbCommandInterceptor.ReaderExecuted" /> to any  <see cref="IDbCommandInterceptor" />
        /// interceptors that are registered on <see cref="DbInterception" /> before/after making a
        /// call to <see cref="DbCommand.ExecuteReader(CommandBehavior)" />.
        /// </summary>
        /// <remarks>
        /// Note that the result of executing the command is returned by this method. The result is not available
        /// in the interception context passed into this method since the interception context is cloned before
        /// being passed to interceptors.
        /// </remarks>
        /// <param name="command">The command on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual DbDataReader Reader(
            DbCommand command, DbCommandInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbCommandInterceptionContext<DbDataReader>(interceptionContext);

            return _internalDispatcher.Dispatch(
                () => command.ExecuteReader(clonedInterceptionContext.CommandBehavior),
                clonedInterceptionContext,
                i => i.ReaderExecuting(command, clonedInterceptionContext),
                i => i.ReaderExecuted(command, clonedInterceptionContext));
        }

#if !NET40
        /// <summary>
        /// Sends <see cref="IDbCommandInterceptor.NonQueryExecuting" /> and
        /// <see cref="IDbCommandInterceptor.NonQueryExecuted" /> to any  <see cref="IDbCommandInterceptor" />
        /// interceptors that are registered on <see cref="DbInterception" /> before/after making a
        /// call to <see cref="DbCommand.ExecuteNonQueryAsync(CancellationToken)" />.
        /// </summary>
        /// <remarks>
        /// Note that the result of executing the command is returned by this method. The result is not available
        /// in the interception context passed into this method since the interception context is cloned before
        /// being passed to interceptors.
        /// </remarks>
        /// <param name="command">The command on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual Task<int> NonQueryAsync(
            DbCommand command, DbCommandInterceptionContext interceptionContext, CancellationToken cancellationToken)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbCommandInterceptionContext<int>(interceptionContext);

            if (!clonedInterceptionContext.IsAsync)
            {
                clonedInterceptionContext = clonedInterceptionContext.AsAsync();
            }

            return _internalDispatcher.DispatchAsync(
                () => command.ExecuteNonQueryAsync(cancellationToken),
                clonedInterceptionContext,
                i => i.NonQueryExecuting(command, clonedInterceptionContext),
                i => i.NonQueryExecuted(command, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbCommandInterceptor.ScalarExecuting" /> and
        /// <see cref="IDbCommandInterceptor.ScalarExecuted" /> to any  <see cref="IDbCommandInterceptor" />
        /// interceptors that are registered on <see cref="DbInterception" /> before/after making a
        /// call to <see cref="DbCommand.ExecuteScalarAsync(CancellationToken)" />.
        /// </summary>
        /// <remarks>
        /// Note that the result of executing the command is returned by this method. The result is not available
        /// in the interception context passed into this method since the interception context is cloned before
        /// being passed to interceptors.
        /// </remarks>
        /// <param name="command">The command on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual Task<object> ScalarAsync(
            DbCommand command, DbCommandInterceptionContext interceptionContext, CancellationToken cancellationToken)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbCommandInterceptionContext<object>(interceptionContext);

            if (!clonedInterceptionContext.IsAsync)
            {
                clonedInterceptionContext = clonedInterceptionContext.AsAsync();
            }

            return _internalDispatcher.DispatchAsync(
                () => command.ExecuteScalarAsync(cancellationToken),
                clonedInterceptionContext,
                i => i.ScalarExecuting(command, clonedInterceptionContext),
                i => i.ScalarExecuted(command, clonedInterceptionContext));
        }

        /// <summary>
        /// Sends <see cref="IDbCommandInterceptor.ReaderExecuting" /> and
        /// <see cref="IDbCommandInterceptor.ReaderExecuted" /> to any  <see cref="IDbCommandInterceptor" />
        /// interceptors that are registered on <see cref="DbInterception" /> before/after making a
        /// call to <see cref="DbCommand.ExecuteReaderAsync(CommandBehavior, CancellationToken)" />.
        /// </summary>
        /// <remarks>
        /// Note that the result of executing the command is returned by this method. The result is not available
        /// in the interception context passed into this method since the interception context is cloned before
        /// being passed to interceptors.
        /// </remarks>
        /// <param name="command">The command on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual Task<DbDataReader> ReaderAsync(
            DbCommand command, DbCommandInterceptionContext interceptionContext, CancellationToken cancellationToken)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            var clonedInterceptionContext = new DbCommandInterceptionContext<DbDataReader>(interceptionContext);

            if (!clonedInterceptionContext.IsAsync)
            {
                clonedInterceptionContext = clonedInterceptionContext.AsAsync();
            }

            return _internalDispatcher.DispatchAsync(
                () => command.ExecuteReaderAsync(clonedInterceptionContext.CommandBehavior, cancellationToken),
                clonedInterceptionContext,
                i => i.ReaderExecuting(command, clonedInterceptionContext),
                i => i.ReaderExecuted(command, clonedInterceptionContext));
        }
#endif

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
