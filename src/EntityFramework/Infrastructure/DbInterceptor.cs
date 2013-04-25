// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Threading.Tasks;

    /// <summary>
    ///     Base class that implements all public <see cref="IDbInterceptor" /> interfaces. Application classes can
    ///     derive from this class and override the desired methods.
    /// </summary>
    /// <remarks>
    ///     Note that use of this base class instead of just implementing the interfaces needed may result
    ///     in less performance in some cases since Entity Framework may be making callbacks in cases where they
    ///     are not used.
    /// </remarks>
    public class DbInterceptor : IDbCommandInterceptor, IDbCommandTreeInterceptor
    {
        /// <inheritdoc />
        public virtual DbCommandTree TreeCreated(DbCommandTree commandTree, DbInterceptionContext interceptionContext)
        {
            return commandTree;
        }

        /// <inheritdoc />
        public virtual void NonQueryExecuting(DbCommand command, DbInterceptionContext interceptionContext)
        {
        }

        /// <inheritdoc />
        public virtual int NonQueryExecuted(DbCommand command, int result, DbInterceptionContext interceptionContext)
        {
            return result;
        }

        /// <inheritdoc />
        public virtual void ReaderExecuting(DbCommand command, CommandBehavior behavior, DbInterceptionContext interceptionContext)
        {
        }

        /// <inheritdoc />
        public virtual DbDataReader ReaderExecuted(
            DbCommand command, CommandBehavior behavior, DbDataReader result, DbInterceptionContext interceptionContext)
        {
            return result;
        }

        /// <inheritdoc />
        public virtual void ScalarExecuting(DbCommand command, DbInterceptionContext interceptionContext)
        {
        }

        /// <inheritdoc />
        public virtual object ScalarExecuted(DbCommand command, object result, DbInterceptionContext interceptionContext)
        {
            return result;
        }

        /// <inheritdoc />
        public virtual void AsyncNonQueryExecuting(DbCommand command, DbInterceptionContext interceptionContext)
        {
        }

        /// <inheritdoc />
        public virtual Task<int> AsyncNonQueryExecuted(DbCommand command, Task<int> result, DbInterceptionContext interceptionContext)
        {
            return result;
        }

        /// <inheritdoc />
        public virtual void AsyncReaderExecuting(DbCommand command, CommandBehavior behavior, DbInterceptionContext interceptionContext)
        {
        }

        /// <inheritdoc />
        public virtual Task<DbDataReader> AsyncReaderExecuted(
            DbCommand command, CommandBehavior behavior, Task<DbDataReader> result, DbInterceptionContext interceptionContext)
        {
            return result;
        }

        /// <inheritdoc />
        public virtual void AsyncScalarExecuting(DbCommand command, DbInterceptionContext interceptionContext)
        {
        }

        /// <inheritdoc />
        public virtual Task<object> AsyncScalarExecuted(DbCommand command, Task<object> result, DbInterceptionContext interceptionContext)
        {
            return result;
        }
    }
}
