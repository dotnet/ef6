// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Represents contextual information associated with calls into <see cref="IDbInterceptor" />
    ///     implementations. For example, if SQL is being executed by a <see cref="DbContext" />, then
    ///     the DbContext will be contained in the DbInterceptionContext instance that is passed to
    ///     the methods off <see cref="IDbCommandInterceptor" />.
    /// </summary>
    /// <remarks>
    ///     Instances of this class are immutable. To add contextual information use one of the
    ///     With... methods to create a new interception context containing the new information.
    /// </remarks>
    public sealed class DbInterceptionContext
    {
        private readonly IList<DbContext> _dbContexts = new List<DbContext>();
        private readonly IList<ObjectContext> _objectContexts = new List<ObjectContext>();

        /// <summary>
        ///     Constructs a new <see cref="DbInterceptionContext" /> with no state.
        /// </summary>
        public DbInterceptionContext()
        {
        }

        private DbInterceptionContext(DbInterceptionContext copyFrom)
        {
            _dbContexts = copyFrom.DbContexts.Where(c => !c.InternalContext.IsDisposed).ToList();
            _objectContexts = copyFrom.ObjectContexts.Where(c => !c.IsDisposed).ToList();
        }

        private DbInterceptionContext(IEnumerable<DbInterceptionContext> copyFrom)
        {
            _dbContexts = copyFrom.SelectMany(c => c.DbContexts)
                                  .Distinct()
                                  .Where(c => !c.InternalContext.IsDisposed).ToList();

            _objectContexts = copyFrom.SelectMany(c => c.ObjectContexts)
                                      .Distinct()
                                      .Where(c => !c.IsDisposed).ToList();
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

            var copy = new DbInterceptionContext(this);
            if (!copy._objectContexts.Contains(context, new ObjectReferenceEqualityComparer()))
            {
                copy._objectContexts.Add(context);
            }
            return copy;
        }

        /// <summary>
        ///     Creates a new <see cref="DbInterceptionContext" /> that contains all the contextual information in this
        ///     interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public DbInterceptionContext WithDbContext(DbContext context)
        {
            Check.NotNull(context, "context");

            var copy = new DbInterceptionContext(this);
            if (!copy._dbContexts.Contains(context, new ObjectReferenceEqualityComparer()))
            {
                copy._dbContexts.Add(context);
            }
            return copy;
        }

        /// <summary>
        ///     Combines contextual information from all the given <see cref="DbInterceptionContext" /> instances
        ///     into a single instance.
        /// </summary>
        /// <param name="interceptionContexts">The contexts to combine.</param>
        /// <returns>A new context containing all information from the given contexts.</returns>
        public static DbInterceptionContext Combine(IEnumerable<DbInterceptionContext> interceptionContexts)
        {
            Check.NotNull(interceptionContexts, "interceptionContexts");

            return new DbInterceptionContext(interceptionContexts);
        }
    }
}
