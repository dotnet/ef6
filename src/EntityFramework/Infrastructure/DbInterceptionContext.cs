// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    ///     Represents contextual information associated with calls into <see cref="IDbInterceptor" />
    ///     implementations.
    /// </summary>
    /// <remarks>
    ///     Note that specific types/operations that can be intercepted may use a more specific
    ///     interception context derived from this class. For example, if SQL is being executed by
    ///     a <see cref="DbContext" />, then the DbContext will be contained in the
    ///     <see cref="DbCommandInterceptionContext" /> instance that is passed to the methods
    ///     of <see cref="IDbCommandInterceptor" />.
    ///     Instances of this class are publicly immutable for contextual information. To add
    ///     contextual information use one of the With... or As... methods to create a new
    ///     interception context containing the new information.
    /// </remarks>
    public class DbInterceptionContext
    {
        private readonly IList<DbContext> _dbContexts;
        private readonly IList<ObjectContext> _objectContexts;
        private bool _isAsync;

        /// <summary>
        ///     Constructs a new <see cref="DbInterceptionContext" /> with no state.
        /// </summary>
        public DbInterceptionContext()
        {
            _dbContexts = new List<DbContext>();
            _objectContexts = new List<ObjectContext>();
        }

        /// <summary>
        ///     Creates a new <see cref="DbInterceptionContext" /> by copying state from the given
        ///     interception context. See <see cref="DbInterceptionContext.Clone" />
        /// </summary>
        /// <param name="copyFrom">The context from which to copy state.</param>
        protected DbInterceptionContext(DbInterceptionContext copyFrom)
        {
            _dbContexts = copyFrom.DbContexts.Where(c => !c.InternalContext.IsDisposed).ToList();
            _objectContexts = copyFrom.ObjectContexts.Where(c => !c.IsDisposed).ToList();
            _isAsync = copyFrom._isAsync;
        }

        private DbInterceptionContext(IEnumerable<DbInterceptionContext> copyFrom)
        {
            Debug.Assert(
                copyFrom.All(c => c.GetType() == typeof(DbInterceptionContext)),
                "Combining derived interception contexts will lose state.");

            _dbContexts = copyFrom.SelectMany(c => c.DbContexts)
                .Distinct()
                .Where(c => !c.InternalContext.IsDisposed).ToList();

            _objectContexts = copyFrom.SelectMany(c => c.ObjectContexts)
                .Distinct()
                .Where(c => !c.IsDisposed).ToList();

            _isAsync = copyFrom.Any(c => c.IsAsync);
        }

        /// <summary>
        ///     Gets all the <see cref="DbContext" /> instances associated with this interception context.
        /// </summary>
        /// <remarks>
        ///     This list usually contains zero or one items. However, it can contain more than one item if
        ///     a single <see cref="ObjectContext" /> has been used to construct multiple <see cref="DbContext" />
        ///     instances.
        /// </remarks>
        public IEnumerable<DbContext> DbContexts
        {
            get { return _dbContexts; }
        }

        /// <summary>
        ///     Creates a new <see cref="DbInterceptionContext" /> that contains all the contextual information in this
        ///     interception context with the addition of the given <see cref="DbContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public DbInterceptionContext WithDbContext(DbContext context)
        {
            Check.NotNull(context, "context");

            var copy = Clone();
            if (!copy._dbContexts.Contains(context, new ObjectReferenceEqualityComparer()))
            {
                copy._dbContexts.Add(context);
            }
            return copy;
        }

        /// <summary>
        ///     Gets all the <see cref="ObjectContext" /> instances associated with this interception context.
        /// </summary>
        /// <remarks>
        ///     This list usually contains zero or one items. However, it can contain more than one item when
        ///     EF has created a new <see cref="ObjectContext" /> for use in database creation and initialization, or
        ///     if a single <see cref="EntityConnection" /> is used with multiple <see cref="ObjectContexts" />.
        /// </remarks>
        public IEnumerable<ObjectContext> ObjectContexts
        {
            get { return _objectContexts; }
        }

        /// <summary>
        ///     Creates a new <see cref="DbInterceptionContext" /> that contains all the contextual information in this
        ///     interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public DbInterceptionContext WithObjectContext(ObjectContext context)
        {
            Check.NotNull(context, "context");

            var copy = Clone();
            if (!copy._objectContexts.Contains(context, new ObjectReferenceEqualityComparer()))
            {
                copy._objectContexts.Add(context);
            }
            return copy;
        }

        /// <summary>
        ///     True if the operation is being executed asynchronously, otherwise false.
        /// </summary>
        public bool IsAsync
        {
            get { return _isAsync; }
        }

        /// <summary>
        ///     Creates a new <see cref="DbInterceptionContext" /> that contains all the contextual information in this
        ///     interception context the <see cref="DbInterceptionContext.IsAsync" /> flag set to true.
        /// </summary>
        /// <returns>A new interception context associated with the async flag set.</returns>
        public DbInterceptionContext AsAsync()
        {
            var copy = Clone();
            copy._isAsync = true;
            return copy;
        }

        /// <summary>
        ///     Call this method when creating a copy of an interception context in order to add new state
        ///     to it. Using this method instead of calling the constructor directly ensures virtual dispatch
        ///     so that the new type will have the same type (and any specialized state) as the context that
        ///     is being cloned.
        /// </summary>
        /// <returns>A new context with all state copied.</returns>
        protected virtual DbInterceptionContext Clone()
        {
            return new DbInterceptionContext(this);
        }

        internal static DbInterceptionContext Combine(IEnumerable<DbInterceptionContext> interceptionContexts)
        {
            DebugCheck.NotNull(interceptionContexts);

            return new DbInterceptionContext(interceptionContexts);
        }
    }
}
