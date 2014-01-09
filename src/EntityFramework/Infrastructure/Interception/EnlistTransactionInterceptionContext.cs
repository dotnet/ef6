// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Transactions;

    /// <summary>
    /// Represents contextual information associated with calls to <see cref="DbConnection.EnlistTransaction" />
    /// implementations.
    /// </summary>
    /// <remarks>
    /// Instances of this class are publicly immutable for contextual information. To add
    /// contextual information use one of the With... or As... methods to create a new
    /// interception context containing the new information.
    /// </remarks>
    public class EnlistTransactionInterceptionContext : DbConnectionInterceptionContext
    {
        private Transaction _transaction;

        /// <summary>
        /// Constructs a new <see cref="EnlistTransactionInterceptionContext" /> with no state.
        /// </summary>
        public EnlistTransactionInterceptionContext()
        {
        }

        /// <summary>
        /// Creates a new <see cref="EnlistTransactionInterceptionContext" /> by copying immutable state from the given
        /// interception context. Also see <see cref="Clone" />
        /// </summary>
        /// <param name="copyFrom">The context from which to copy state.</param>
        public EnlistTransactionInterceptionContext(DbInterceptionContext copyFrom)
            : base(copyFrom)
        {
            Check.NotNull(copyFrom, "copyFrom");

            var asThisType = copyFrom as EnlistTransactionInterceptionContext;
            if (asThisType != null)
            {
                _transaction = asThisType._transaction;
            }
        }

        /// <summary>
        /// Creates a new <see cref="EnlistTransactionInterceptionContext" /> that contains all the contextual information in this
        /// interception context together with the <see cref="DbInterceptionContext.IsAsync" /> flag set to true.
        /// </summary>
        /// <returns>A new interception context associated with the async flag set.</returns>
        public new EnlistTransactionInterceptionContext AsAsync()
        {
            return (EnlistTransactionInterceptionContext)base.AsAsync();
        }

        /// <summary>
        /// The <see cref="Transaction" /> that will be used or has been used to enlist a connection.
        /// </summary>
        public Transaction Transaction
        {
            get { return _transaction; }
        }

        /// <summary>
        /// Creates a new <see cref="EnlistTransactionInterceptionContext" /> that contains all the contextual information in this
        /// interception context together with the given <see cref="Transaction" />.
        /// </summary>
        /// <param name="transaction">The transaction to be used in the <see cref="DbConnection.EnlistTransaction" /> invocation.</param>
        /// <returns>A new interception context associated with the given isolation level.</returns>
        public EnlistTransactionInterceptionContext WithTransaction(Transaction transaction)
        {
            var copy = TypedClone();
            copy._transaction = transaction;
            return copy;
        }

        private EnlistTransactionInterceptionContext TypedClone()
        {
            return (EnlistTransactionInterceptionContext)Clone();
        }

        /// <inheritdoc />
        protected override DbInterceptionContext Clone()
        {
            return new EnlistTransactionInterceptionContext(this);
        }

        /// <summary>
        /// Creates a new <see cref="EnlistTransactionInterceptionContext" /> that contains all the contextual information in this
        /// interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new EnlistTransactionInterceptionContext WithDbContext(DbContext context)
        {
            Check.NotNull(context, "context");

            return (EnlistTransactionInterceptionContext)base.WithDbContext(context);
        }

        /// <summary>
        /// Creates a new <see cref="EnlistTransactionInterceptionContext" /> that contains all the contextual information in this
        /// interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new EnlistTransactionInterceptionContext WithObjectContext(ObjectContext context)
        {
            Check.NotNull(context, "context");

            return (EnlistTransactionInterceptionContext)base.WithObjectContext(context);
        }

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

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
