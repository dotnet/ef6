namespace System.Data.Entity.Internal.Linq
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     An instance of this internal class is created whenever an instance of the public <see cref = "DbQuery" />
    ///     class is needed. This allows the public surface to be non-generic, while the runtime type created
    ///     still implements <see cref = "IQueryable{T}" />.
    /// </summary>
    /// <typeparam name = "TElement">The type of the element.</typeparam>
    internal class InternalDbQuery<TElement> : DbQuery, IOrderedQueryable<TElement>
    {
        #region Fields and constructors

        // Handles the underlying ObjectQuery that backs the query.
        private readonly IInternalQuery<TElement> _internalQuery;

        /// <summary>
        ///     Creates a new query that will be backed by the given internal query object.
        /// </summary>
        /// <param name = "internalQuery">The backing query.</param>
        public InternalDbQuery(IInternalQuery<TElement> internalQuery)
        {
            Contract.Requires(internalQuery != null);

            _internalQuery = internalQuery;
        }

        #endregion

        #region Implementation of abstract methods defined on DbQuery

        /// <summary>
        ///     Gets the underlying internal query object.
        /// </summary>
        /// <value>The internal query.</value>
        internal override IInternalQuery InternalQuery
        {
            get { return _internalQuery; }
        }

        /// <summary>
        ///     See comments in <see cref = "DbQuery" />.
        /// </summary>
        public override DbQuery Include(string path)
        {
            // We need this because the Code Contract gets compiled out in the release build even though
            // this method is effectively on the public surface because it overrides the abstract method on DbSet.
            DbHelpers.ThrowIfNullOrWhitespace(path, "path");

            return new InternalDbQuery<TElement>(_internalQuery.Include(path));
        }

        /// <summary>
        ///     See comments in <see cref = "DbQuery" />.
        /// </summary>
        public override DbQuery AsNoTracking()
        {
            return new InternalDbQuery<TElement>(_internalQuery.AsNoTracking());
        }

        #endregion

        #region GetEnumerator

        /// <summary>
        ///     Gets the enumeration of this query causing it to be executed against the store.
        /// </summary>
        /// <returns>An enumerator for the query</returns>
        public IEnumerator<TElement> GetEnumerator()
        {
            return _internalQuery.GetEnumerator();
        }

        #endregion
    }
}
