// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Utilities;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Used for dispatching operations to a <see cref="DbCommand" /> such that any <see cref="IDbCommandInterceptor" />
    ///     interceptors that are registered on <see cref="Interception" /> will be notified before and after the
    ///     operation executes.
    ///     Instances of this class are obtained through the the <see cref="Interception.Dispatch" /> fluent API.
    /// </summary>
    /// <remarks>
    ///     This class is used internally by Entity Framework when executing commands. It is provided publicly so that
    ///     code that runs outside of the core EF assemblies can opt-in to command interception/tracing. This is
    ///     typically done by EF providers that are executing commands on behalf of EF.
    /// </remarks>
    public class DbCommandDispatcher : DispatcherBase<IDbCommandInterceptor>
    {
        internal DbCommandDispatcher()
        {
        }

        /// <summary>
        ///     Sends <see cref="IDbCommandInterceptor.NonQueryExecuting" /> and
        ///     <see cref="IDbCommandInterceptor.NonQueryExecuted" /> to any  <see cref="IDbCommandInterceptor" />
        ///     interceptors that are registered on <see cref="Interception" /> before/after making a
        ///     call to <see cref="DbCommand.ExecuteNonQuery" />.
        /// </summary>
        /// <param name="command">The command on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual int NonQuery(DbCommand command, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            return InternalDispatcher.Dispatch(
                command.ExecuteNonQuery,
                i => i.NonQueryExecuting(command, interceptionContext),
                (r, i) => i.NonQueryExecuted(command, r, interceptionContext));
        }

        /// <summary>
        ///     Sends <see cref="IDbCommandInterceptor.ScalarExecuting" /> and
        ///     <see cref="IDbCommandInterceptor.ScalarExecuted" /> to any  <see cref="IDbCommandInterceptor" />
        ///     interceptors that are registered on <see cref="Interception" /> before/after making a
        ///     call to <see cref="DbCommand.ExecuteScalar" />.
        /// </summary>
        /// <param name="command">The command on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual object Scalar(DbCommand command, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            return InternalDispatcher.Dispatch(
                command.ExecuteScalar,
                i => i.ScalarExecuting(command, interceptionContext),
                (r, i) => i.ScalarExecuted(command, r, interceptionContext));
        }

        /// <summary>
        ///     Sends <see cref="IDbCommandInterceptor.ReaderExecuting" /> and
        ///     <see cref="IDbCommandInterceptor.ReaderExecuted" /> to any  <see cref="IDbCommandInterceptor" />
        ///     interceptors that are registered on <see cref="Interception" /> before/after making a
        ///     call to <see cref="DbCommand.ExecuteReader(CommandBehavior)" />.
        /// </summary>
        /// <param name="command">The command on which the operation will be executed.</param>
        /// <param name="behavior">The command behavior to use for the operation.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual DbDataReader Reader(
            DbCommand command, CommandBehavior behavior, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            return InternalDispatcher.Dispatch(
                () => command.ExecuteReader(behavior),
                i => i.ReaderExecuting(command, behavior, interceptionContext),
                (r, i) => i.ReaderExecuted(command, behavior, r, interceptionContext));
        }

#if !NET40
        /// <summary>
        ///     Sends <see cref="IDbCommandInterceptor.AsyncNonQueryExecuting" /> and
        ///     <see cref="IDbCommandInterceptor.AsyncNonQueryExecuted" /> to any  <see cref="IDbCommandInterceptor" />
        ///     interceptors that are registered on <see cref="Interception" /> before/after making a
        ///     call to <see cref="DbCommand.ExecuteNonQueryAsync(CancellationToken)" />.
        /// </summary>
        /// <param name="command">The command on which the operation will be executed.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual Task<int> AsyncNonQuery(
            DbCommand command, CancellationToken cancellationToken, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            return InternalDispatcher.Dispatch(
                () => command.ExecuteNonQueryAsync(cancellationToken),
                i => i.AsyncNonQueryExecuting(command, interceptionContext),
                (r, i) => i.AsyncNonQueryExecuted(command, r, interceptionContext));
        }

        /// <summary>
        ///     Sends <see cref="IDbCommandInterceptor.AsyncScalarExecuting" /> and
        ///     <see cref="IDbCommandInterceptor.AsyncScalarExecuted" /> to any  <see cref="IDbCommandInterceptor" />
        ///     interceptors that are registered on <see cref="Interception" /> before/after making a
        ///     call to <see cref="DbCommand.ExecuteScalarAsync(CancellationToken)" />.
        /// </summary>
        /// <param name="command">The command on which the operation will be executed.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual Task<object> AsyncScalar(
            DbCommand command, CancellationToken cancellationToken, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            return InternalDispatcher.Dispatch(
                () => command.ExecuteScalarAsync(cancellationToken),
                i => i.AsyncScalarExecuting(command, interceptionContext),
                (r, i) => i.AsyncScalarExecuted(command, r, interceptionContext));
        }

        /// <summary>
        ///     Sends <see cref="IDbCommandInterceptor.AsyncReaderExecuting" /> and
        ///     <see cref="IDbCommandInterceptor.AsyncReaderExecuted" /> to any  <see cref="IDbCommandInterceptor" />
        ///     interceptors that are registered on <see cref="Interception" /> before/after making a
        ///     call to <see cref="DbCommand.ExecuteReaderAsync(CommandBehavior, CancellationToken)" />.
        /// </summary>
        /// <param name="command">The command on which the operation will be executed.</param>
        /// <param name="behavior">The command behavior to use for the operation.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual Task<DbDataReader> AsyncReader(
            DbCommand command, CommandBehavior behavior, CancellationToken cancellationToken, DbInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            return InternalDispatcher.Dispatch(
                () => command.ExecuteReaderAsync(behavior, cancellationToken),
                i => i.AsyncReaderExecuting(command, behavior, interceptionContext),
                (r, i) => i.AsyncReaderExecuted(command, behavior, r, interceptionContext));
        }
#endif
    }
}
