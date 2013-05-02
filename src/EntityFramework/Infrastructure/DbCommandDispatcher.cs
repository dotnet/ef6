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
        public virtual int NonQuery(DbCommand command, DbCommandInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            return InternalDispatcher.Dispatch(
                command.ExecuteNonQuery,
                interceptionContext,
                i => i.NonQueryExecuting(command, interceptionContext),
                (r, i, c) => i.NonQueryExecuted(command, r, c));
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
        public virtual object Scalar(DbCommand command, DbCommandInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            return InternalDispatcher.Dispatch(
                command.ExecuteScalar,
                interceptionContext,
                i => i.ScalarExecuting(command, interceptionContext),
                (r, i, c) => i.ScalarExecuted(command, r, c));
        }

        /// <summary>
        ///     Sends <see cref="IDbCommandInterceptor.ReaderExecuting" /> and
        ///     <see cref="IDbCommandInterceptor.ReaderExecuted" /> to any  <see cref="IDbCommandInterceptor" />
        ///     interceptors that are registered on <see cref="Interception" /> before/after making a
        ///     call to <see cref="DbCommand.ExecuteReader(CommandBehavior)" />.
        /// </summary>
        /// <param name="command">The command on which the operation will be executed.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual DbDataReader Reader(
            DbCommand command, DbCommandInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            return InternalDispatcher.Dispatch(
                () => command.ExecuteReader(interceptionContext.CommandBehavior),
                interceptionContext,
                i => i.ReaderExecuting(command, interceptionContext),
                (r, i, c) => i.ReaderExecuted(command, r, c));
        }

#if !NET40
        /// <summary>
        ///     Sends <see cref="IDbCommandInterceptor.NonQueryExecuting" /> and
        ///     <see cref="IDbCommandInterceptor.NonQueryExecuted" /> to any  <see cref="IDbCommandInterceptor" />
        ///     interceptors that are registered on <see cref="Interception" /> before/after making a 
        ///     call to <see cref="DbCommand.ExecuteNonQueryAsync(CancellationToken)" />.
        /// </summary>
        /// <param name="command">The command on which the operation will be executed.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual Task<int> AsyncNonQuery(
            DbCommand command, CancellationToken cancellationToken, DbCommandInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            if (!interceptionContext.IsAsync)
            {
                interceptionContext = interceptionContext.AsAsync();
            }

            return InternalDispatcher.Dispatch(
                () => command.ExecuteNonQueryAsync(cancellationToken),
                interceptionContext,
                i => i.NonQueryExecuting(command, interceptionContext),
                (r, i, c) => i.NonQueryExecuted(command, r, c),
                UpdateInterceptionContext);
        }

        /// <summary>
        ///     Sends <see cref="IDbCommandInterceptor.ScalarExecuting" /> and
        ///     <see cref="IDbCommandInterceptor.ScalarExecuted" /> to any  <see cref="IDbCommandInterceptor" />
        ///     interceptors that are registered on <see cref="Interception" /> before/after making a
        ///     call to <see cref="DbCommand.ExecuteScalarAsync(CancellationToken)" />.
        /// </summary>
        /// <param name="command">The command on which the operation will be executed.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual Task<object> AsyncScalar(
            DbCommand command, CancellationToken cancellationToken, DbCommandInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            if (!interceptionContext.IsAsync)
            {
                interceptionContext = interceptionContext.AsAsync();
            }

            return InternalDispatcher.Dispatch(
                () => command.ExecuteScalarAsync(cancellationToken),
                interceptionContext,
                i => i.ScalarExecuting(command, interceptionContext),
                (r, i, c) => i.ScalarExecuted(command, r, c),
                UpdateInterceptionContext);
        }

        /// <summary>
        ///     Sends <see cref="IDbCommandInterceptor.ReaderExecuting" /> and
        ///     <see cref="IDbCommandInterceptor.ReaderExecuted" /> to any  <see cref="IDbCommandInterceptor" />
        ///     interceptors that are registered on <see cref="Interception" /> before/after making a
        ///     call to <see cref="DbCommand.ExecuteReaderAsync(CommandBehavior, CancellationToken)" />.
        /// </summary>
        /// <param name="command">The command on which the operation will be executed.</param>
        /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
        /// <param name="interceptionContext">Optional information about the context of the call being made.</param>
        /// <returns>The result of the operation, which may have been modified by interceptors.</returns>
        public virtual Task<DbDataReader> AsyncReader(
            DbCommand command, CancellationToken cancellationToken, DbCommandInterceptionContext interceptionContext)
        {
            Check.NotNull(command, "command");
            Check.NotNull(interceptionContext, "interceptionContext");

            if (!interceptionContext.IsAsync)
            {
                interceptionContext = interceptionContext.AsAsync();
            }

            return InternalDispatcher.Dispatch(
                () => command.ExecuteReaderAsync(interceptionContext.CommandBehavior, cancellationToken),
                interceptionContext,
                i => i.ReaderExecuting(command, interceptionContext),
                (r, i, c) => i.ReaderExecuted(command, r, c),
                UpdateInterceptionContext);
        }

        private static DbCommandInterceptionContext UpdateInterceptionContext(DbCommandInterceptionContext interceptionContext, Task t)
        {
            return interceptionContext.WithTaskStatus(t.Status);
        }
#endif
    }
}
