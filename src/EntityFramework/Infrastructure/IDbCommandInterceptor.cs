// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     An object that implements this interface can be registered with <see cref="Interception" /> to
    ///     receive notifications when Entity Framework executes commands.
    /// </summary>
    public interface IDbCommandInterceptor : IDbInterceptor
    {
        /// <summary>
        ///     This method is called before a call to <see cref="DbCommand.ExecuteNonQuery" /> is made.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void NonQueryExecuting(DbCommand command, DbInterceptionContext interceptionContext);

        /// <summary>
        ///     This method is called after a call to <see cref="DbCommand.ExecuteNonQuery" /> is made.
        ///     This method should return the given result. However, the result used by Entity Framework
        ///     can be changed by returning a different value.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="result">The result of the command execution.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <returns>The result to be used by Entity Framework.</returns>
        int NonQueryExecuted(DbCommand command, int result, DbInterceptionContext interceptionContext);

        /// <summary>
        ///     This method is called before a call to <see cref="DbCommand.ExecuteReader(CommandBehavior)" /> is made.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="behavior">The command behavior.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void ReaderExecuting(DbCommand command, CommandBehavior behavior, DbInterceptionContext interceptionContext);

        /// <summary>
        ///     This method is called after a call to <see cref="DbCommand.ExecuteReader(CommandBehavior)" /> is made.
        ///     This method should return the given result. However, the result used by Entity Framework
        ///     can be changed by returning a different value.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="behavior">The command behavior.</param>
        /// <param name="result">The result of the command execution.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <returns>The result to be used by Entity Framework.</returns>
        DbDataReader ReaderExecuted(
            DbCommand command, CommandBehavior behavior, DbDataReader result, DbInterceptionContext interceptionContext);

        /// <summary>
        ///     This method is called before a call to <see cref="DbCommand.ExecuteScalar" /> is made.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void ScalarExecuting(DbCommand command, DbInterceptionContext interceptionContext);

        /// <summary>
        ///     This method is called after a call to <see cref="DbCommand.ExecuteScalar" /> is made.
        ///     This method should return the given result. However, the result used by Entity Framework
        ///     can be changed by returning a different value.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="result">The result of the command execution.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <returns>The result to be used by Entity Framework.</returns>
        object ScalarExecuted(DbCommand command, object result, DbInterceptionContext interceptionContext);

#if NET40
        /// <summary>
        ///     This method is never called for an application that targets on .NET 4.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void AsyncNonQueryExecuting(DbCommand command, DbInterceptionContext interceptionContext);

        /// <summary>
        ///     This method is never called for an application that targets on .NET 4.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="result">The result of the command execution.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <returns>The result to be used by Entity Framework.</returns>
        Task<int> AsyncNonQueryExecuted(DbCommand command, Task<int> result, DbInterceptionContext interceptionContext);

        /// <summary>
        ///     This method is never called for an application that targets on .NET 4.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="behavior">The command behavior.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void AsyncReaderExecuting(DbCommand command, CommandBehavior behavior, DbInterceptionContext interceptionContext);

        /// <summary>
        ///     This method is never called for an application that targets on .NET 4.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="behavior">The command behavior.</param>
        /// <param name="result">The result of the command execution.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <returns>The result to be used by Entity Framework.</returns>
        Task<DbDataReader> AsyncReaderExecuted(
            DbCommand command, CommandBehavior behavior, Task<DbDataReader> result, DbInterceptionContext interceptionContext);

        /// <summary>
        ///     This method is never called for an application that targets on .NET 4.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void AsyncScalarExecuting(DbCommand command, DbInterceptionContext interceptionContext);

        /// <summary>
        ///     This method is never called for an application that targets on .NET 4.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="result">The result of the command execution.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <returns>The result to be used by Entity Framework.</returns>
        Task<object> AsyncScalarExecuted(DbCommand command, Task<object> result, DbInterceptionContext interceptionContext);
#else
        /// <summary>
        ///     This method is called before a call to <see cref="DbCommand.ExecuteNonQueryAsync(CancellationToken)" /> is made.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void AsyncNonQueryExecuting(DbCommand command, DbInterceptionContext interceptionContext);

        /// <summary>
        ///     This method is called after a call to <see cref="DbCommand.ExecuteNonQueryAsync(CancellationToken)" /> is made.
        ///     This method should return the given result. However, the result used by Entity Framework
        ///     can be changed by returning a different value.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="result">The result of the command execution.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <returns>The result to be used by Entity Framework.</returns>
        Task<int> AsyncNonQueryExecuted(DbCommand command, Task<int> result, DbInterceptionContext interceptionContext);

        /// <summary>
        ///     This method is called before a call to
        ///     <see cref="DbCommand.ExecuteReaderAsync(CommandBehavior, CancellationToken)" /> is made.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="behavior">The command behavior.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void AsyncReaderExecuting(DbCommand command, CommandBehavior behavior, DbInterceptionContext interceptionContext);

        /// <summary>
        ///     This method is called after a call to
        ///     <see cref="DbCommand.ExecuteReaderAsync(CommandBehavior, CancellationToken)" /> is made.
        ///     This method should return the given result. However, the result used by Entity Framework
        ///     can be changed by returning a different value.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="behavior">The command behavior.</param>
        /// <param name="result">The result of the command execution.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <returns>The result to be used by Entity Framework.</returns>
        Task<DbDataReader> AsyncReaderExecuted(
            DbCommand command, CommandBehavior behavior, Task<DbDataReader> result, DbInterceptionContext interceptionContext);

        /// <summary>
        ///     This method is called before a call to <see cref="DbCommand.ExecuteScalarAsync(CancellationToken)" /> is made.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        void AsyncScalarExecuting(DbCommand command, DbInterceptionContext interceptionContext);

        /// <summary>
        ///     This method is called after a call to <see cref="DbCommand.ExecuteScalarAsync(CancellationToken)" /> is made.
        ///     This method should return the given result. However, the result used by Entity Framework
        ///     can be changed by returning a different value.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="result">The result of the command execution.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        /// <returns>The result to be used by Entity Framework.</returns>
        Task<object> AsyncScalarExecuted(DbCommand command, Task<object> result, DbInterceptionContext interceptionContext);
#endif
    }
}
