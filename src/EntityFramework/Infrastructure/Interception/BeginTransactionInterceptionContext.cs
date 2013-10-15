// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Common;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Represents contextual information associated with calls to <see cref="DbConnection.BeginTransaction(System.Data.IsolationLevel)" />
    /// implementations.
    /// </summary>
    /// <remarks>
    /// Instances of this class are publicly immutable for contextual information. To add
    /// contextual information use one of the With... or As... methods to create a new
    /// interception context containing the new information.
    /// </remarks>
    public class BeginTransactionInterceptionContext : DbConnectionInterceptionContext<DbTransaction>
    {
        private IsolationLevel _isolationLevel = IsolationLevel.Unspecified;

        /// <summary>
        /// Constructs a new <see cref="BeginTransactionInterceptionContext" /> with no state.
        /// </summary>
        public BeginTransactionInterceptionContext()
        {
        }

        /// <summary>
        /// Creates a new <see cref="BeginTransactionInterceptionContext" /> by copying immutable state from the given
        /// interception context. Also see <see cref="Clone" />
        /// </summary>
        /// <param name="copyFrom">The context from which to copy state.</param>
        public BeginTransactionInterceptionContext(DbInterceptionContext copyFrom)
            : base(copyFrom)
        {
            Check.NotNull(copyFrom, "copyFrom");

            var asThisType = copyFrom as BeginTransactionInterceptionContext;
            if (asThisType != null)
            {
                _isolationLevel = asThisType._isolationLevel;
            }
        }
        
        /// <summary>
        /// Creates a new <see cref="BeginTransactionInterceptionContext" /> that contains all the contextual information in this
        /// interception context together with the <see cref="DbInterceptionContext.IsAsync" /> flag set to true.
        /// </summary>
        /// <returns>A new interception context associated with the async flag set.</returns>
        public new BeginTransactionInterceptionContext AsAsync()
        {
            return (BeginTransactionInterceptionContext)base.AsAsync();
        }

        /// <summary>
        /// The <see cref="IsolationLevel" /> that will be used or has been used to start a transaction.
        /// </summary>
        public IsolationLevel IsolationLevel
        {
            get { return _isolationLevel; }
        }

        /// <summary>
        /// Creates a new <see cref="BeginTransactionInterceptionContext" /> that contains all the contextual information in this
        /// interception context together with the given <see cref="IsolationLevel" />.
        /// </summary>
        /// <param name="isolationLevel">The isolation level to associate.</param>
        /// <returns>A new interception context associated with the given isolation level.</returns>
        public BeginTransactionInterceptionContext WithIsolationLevel(IsolationLevel isolationLevel)
        {
            var copy = TypedClone();
            copy._isolationLevel = isolationLevel;
            return copy;
        }

        private BeginTransactionInterceptionContext TypedClone()
        {
            return (BeginTransactionInterceptionContext)Clone();
        }
        
        /// <inheritdoc />
        protected override DbInterceptionContext Clone()
        {
            return new BeginTransactionInterceptionContext(this);
        }

        /// <summary>
        /// Creates a new <see cref="BeginTransactionInterceptionContext" /> that contains all the contextual information in this
        /// interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new BeginTransactionInterceptionContext WithDbContext(DbContext context)
        {
            Check.NotNull(context, "context");

            return (BeginTransactionInterceptionContext)base.WithDbContext(context);
        }

        /// <summary>
        /// Creates a new <see cref="BeginTransactionInterceptionContext" /> that contains all the contextual information in this
        /// interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new BeginTransactionInterceptionContext WithObjectContext(ObjectContext context)
        {
            Check.NotNull(context, "context");

            return (BeginTransactionInterceptionContext)base.WithObjectContext(context);
        }
    }
}
