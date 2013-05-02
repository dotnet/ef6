// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common.CommandTrees;

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
        public virtual void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext interceptionContext)
        {
        }

        /// <inheritdoc />
        public virtual int NonQueryExecuted(DbCommand command, int result, DbCommandInterceptionContext interceptionContext)
        {
            return result;
        }

        /// <inheritdoc />
        public virtual void ReaderExecuting(DbCommand command, DbCommandInterceptionContext interceptionContext)
        {
        }

        /// <inheritdoc />
        public virtual DbDataReader ReaderExecuted(
            DbCommand command, DbDataReader result, DbCommandInterceptionContext interceptionContext)
        {
            return result;
        }

        /// <inheritdoc />
        public virtual void ScalarExecuting(DbCommand command, DbCommandInterceptionContext interceptionContext)
        {
        }

        /// <inheritdoc />
        public virtual object ScalarExecuted(DbCommand command, object result, DbCommandInterceptionContext interceptionContext)
        {
            return result;
        }
    }
}
