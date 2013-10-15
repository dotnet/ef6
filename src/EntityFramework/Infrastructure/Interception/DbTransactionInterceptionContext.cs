// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Common;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Represents contextual information associated with calls to <see cref="DbTransaction"/> that don't return any results.
    /// </summary>
    public class DbTransactionInterceptionContext : MutableInterceptionContext
    {
        /// <summary>
        /// Constructs a new <see cref="DbTransactionInterceptionContext" /> with no state.
        /// </summary>
        public DbTransactionInterceptionContext()
        {
        }

        /// <summary>
        /// Creates a new <see cref="DbTransactionInterceptionContext" /> by copying immutable state from the given
        /// interception context. Also see <see cref="Clone" />
        /// </summary>
        /// <param name="copyFrom">The context from which to copy state.</param>
        public DbTransactionInterceptionContext(DbInterceptionContext copyFrom)
            : base(copyFrom)
        {
            Check.NotNull(copyFrom, "copyFrom");
        }

        /// <summary>
        /// Creates a new <see cref="DbTransactionInterceptionContext" /> that contains all the contextual information in this
        /// interception context together with the <see cref="DbInterceptionContext.IsAsync" /> flag set to true.
        /// </summary>
        /// <returns>A new interception context associated with the async flag set.</returns>
        public new DbTransactionInterceptionContext AsAsync()
        {
            return (DbTransactionInterceptionContext)base.AsAsync();
        }

        /// <summary>
        /// Creates a new <see cref="DbTransactionInterceptionContext" /> that contains all the contextual information in this
        /// interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new DbTransactionInterceptionContext WithDbContext(DbContext context)
        {
            Check.NotNull(context, "context");

            return (DbTransactionInterceptionContext)base.WithDbContext(context);
        }

        /// <summary>
        /// Creates a new <see cref="DbTransactionInterceptionContext" /> that contains all the contextual information in this
        /// interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new DbTransactionInterceptionContext WithObjectContext(ObjectContext context)
        {
            Check.NotNull(context, "context");

            return (DbTransactionInterceptionContext)base.WithObjectContext(context);
        }

        /// <inheritdoc />
        protected override DbInterceptionContext Clone()
        {
            return new DbTransactionInterceptionContext(this);
        }
    }
}
