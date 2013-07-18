// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Common;

    /// <summary>
    ///     Base class that implements <see cref="IDbCommandInterceptor" />. This class is a convenience for
    ///     use when only one or two methods of the interface actually need to have any implementation.
    /// </summary>
    public class DbCommandInterceptor : IDbCommandInterceptor
    {
        /// <inheritdoc />
        public virtual void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
        }

        /// <inheritdoc />
        public virtual void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
        }

        /// <inheritdoc />
        public virtual void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
        }

        /// <inheritdoc />
        public virtual void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
        }

        /// <inheritdoc />
        public virtual void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
        }

        /// <inheritdoc />
        public virtual void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
        }
    }
}
