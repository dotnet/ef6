// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Utilities;

    public class DbCommandTreeInterceptionContext : DbInterceptionContext, IDbInterceptionContextWithResult<DbCommandTree>
    {
        private bool _isResultSet;
        private DbCommandTree _result;

        /// <summary>
        ///     Constructs a new <see cref="DbCommandInterceptionContext" /> with no state.
        /// </summary>
        public DbCommandTreeInterceptionContext()
        {
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandTreeInterceptionContext" /> by copying state from the given
        ///     interception context. Also see <see cref="DbCommandTreeInterceptionContext.Clone" />
        /// </summary>
        /// <param name="copyFrom">The context from which to copy state.</param>
        public DbCommandTreeInterceptionContext(DbInterceptionContext copyFrom)
            : base(copyFrom)
        {
            var asResultType = copyFrom as IDbInterceptionContextWithResult<DbCommandTree>;
            if (asResultType != null)
            {
                _isResultSet = asResultType.IsResultSet;
                _result = asResultType.Result;
            }
        }

        /// <summary>
        ///     The result of executing the operation.
        /// </summary>
        /// <remarks>
        ///     Changing this property will change the tree that is used by EF.
        /// </remarks>
        public DbCommandTree Result
        {
            get { return _result; }
            set
            {
                _result = value;
                _isResultSet = true;
            }
        }

        internal bool IsResultSet
        {
            get { return ((IDbInterceptionContextWithResult<DbCommandTree>)this).IsResultSet; }
        }

        bool IDbInterceptionContextWithResult<DbCommandTree>.IsResultSet
        {
            get { return _isResultSet; }
        }

        /// <inheritdoc />
        protected override DbInterceptionContext Clone()
        {
            return new DbCommandTreeInterceptionContext(this);
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandTreeInterceptionContext" /> that contains all the contextual information in this
        ///     interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new DbCommandTreeInterceptionContext WithDbContext(DbContext context)
        {
            Check.NotNull(context, "context");

            return (DbCommandTreeInterceptionContext)base.WithDbContext(context);
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandTreeInterceptionContext" /> that contains all the contextual information in this
        ///     interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new DbCommandTreeInterceptionContext WithObjectContext(ObjectContext context)
        {
            Check.NotNull(context, "context");

            return (DbCommandTreeInterceptionContext)base.WithObjectContext(context);
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandTreeInterceptionContext" /> that contains all the contextual information in this
        ///     interception context with the addition of the given <see cref="Exception" />.
        ///     Note that associating an exception with an interception context indicates that the intercepted
        ///     operation failed.
        /// </summary>
        /// <param name="exception">The exception to associate.</param>
        /// <returns>A new interception context associated with the given exception.</returns>
        public new DbCommandTreeInterceptionContext WithException(Exception exception)
        {
            return (DbCommandTreeInterceptionContext)base.WithException(exception);
        }
    }
}
