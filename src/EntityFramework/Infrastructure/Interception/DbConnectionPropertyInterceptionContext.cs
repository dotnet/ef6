// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Common;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Represents contextual information associated with calls to <see cref="DbConnection"/> that don't return any results.
    /// </summary>
    /// <typeparam name="TValue">The type of the target property.</typeparam>
    public class DbConnectionPropertyInterceptionContext<TValue> : PropertyInterceptionContext<TValue>
    {
        /// <summary>
        /// Constructs a new <see cref="DbConnectionPropertyInterceptionContext{TValue}" /> with no state.
        /// </summary>
        public DbConnectionPropertyInterceptionContext()
        {
        }

        /// <summary>
        /// Creates a new <see cref="DbConnectionPropertyInterceptionContext{TValue}" /> by copying immutable state from the given
        /// interception context. Also see <see cref="Clone" />
        /// </summary>
        /// <param name="copyFrom">The context from which to copy state.</param>
        public DbConnectionPropertyInterceptionContext(DbInterceptionContext copyFrom)
            : base(copyFrom)
        {
            Check.NotNull(copyFrom, "copyFrom");
        }

        /// <summary>
        /// Creates a new <see cref="BeginTransactionInterceptionContext" /> that contains all the contextual information in this
        /// interception context together with the given property value.
        /// </summary>
        /// <param name="value">The value that will be assigned to the target property.</param>
        /// <returns>A new interception context associated with the given property value.</returns>
        public new DbConnectionPropertyInterceptionContext<TValue> WithValue(TValue value)
        {
            return (DbConnectionPropertyInterceptionContext<TValue>)base.WithValue(value);
        }

        /// <inheritdoc />
        protected override DbInterceptionContext Clone()
        {
            return new DbConnectionPropertyInterceptionContext<TValue>(this);
        }

        /// <summary>
        /// Creates a new <see cref="DbConnectionPropertyInterceptionContext{Value}" /> that contains all the contextual information in this
        /// interception context together with the <see cref="DbInterceptionContext.IsAsync" /> flag set to true.
        /// </summary>
        /// <returns>A new interception context associated with the async flag set.</returns>
        public new DbConnectionPropertyInterceptionContext<TValue> AsAsync()
        {
            return (DbConnectionPropertyInterceptionContext<TValue>)base.AsAsync();
        }

        /// <summary>
        /// Creates a new <see cref="DbConnectionPropertyInterceptionContext{TValue}" /> that contains all the contextual information in this
        /// interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new DbConnectionPropertyInterceptionContext<TValue> WithDbContext(DbContext context)
        {
            Check.NotNull(context, "context");

            return (DbConnectionPropertyInterceptionContext<TValue>)base.WithDbContext(context);
        }

        /// <summary>
        /// Creates a new <see cref="DbConnectionPropertyInterceptionContext{TValue}" /> that contains all the contextual information in this
        /// interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new DbConnectionPropertyInterceptionContext<TValue> WithObjectContext(ObjectContext context)
        {
            Check.NotNull(context, "context");

            return (DbConnectionPropertyInterceptionContext<TValue>)base.WithObjectContext(context);
        }
    }
}
