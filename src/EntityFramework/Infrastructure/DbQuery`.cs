namespace System.Data.Entity.Infrastructure
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Internal.Linq;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    ///     Represents a LINQ to Entities query against a DbContext.
    /// </summary>
    /// <typeparam name = "TResult">The type of entity to query for.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly",
        Justification = "Casing is intentional")]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix",
        Justification = "Name is intentional")]
    public class DbQuery<TResult> : IOrderedQueryable<TResult>, IListSource, IInternalQueryAdapter
    {
        #region Fields and constructors

        // Handles the underlying ObjectQuery that backs the query.
        private readonly IInternalQuery<TResult> _internalQuery;
        private IQueryProvider _provider;

        /// <summary>
        ///     Creates a new query that will be backed by the given internal query object.
        /// </summary>
        /// <param name = "internalQuery">The backing query.</param>
        internal DbQuery(IInternalQuery<TResult> internalQuery)
        {
            Contract.Requires(internalQuery != null);

            _internalQuery = internalQuery;
        }

        #endregion

        #region Include

        /// <summary>
        ///     Specifies the related objects to include in the query results.
        /// </summary>
        /// <remarks>
        ///     Paths are all-inclusive. For example, if an include call indicates Include("Orders.OrderLines"), not only will
        ///     OrderLines be included, but also Orders.  When you call the Include method, the query path is only valid on
        ///     the returned instance of the DbQuery<T>. Other instances of DbQuery<T> and the object context itself are not affected.
        ///                                                                            Because the Include method returns the query object, you can call this method multiple times on an DbQuery<T> to
        ///                                                                                                                                                                                          specify multiple paths for the query.
        /// </remarks>
        /// <param name = "path">The dot-separated list of related objects to return in the query results.</param>
        /// <returns>A new DbQuery<T> with the defined query path.</returns>
        public DbQuery<TResult> Include(string path)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(path));

            return new DbQuery<TResult>(_internalQuery.Include(path));
        }

        #endregion

        #region AsNoTracking

        /// <summary>
        ///     Returns a new query where the entities returned will not be cached in the <see cref = "DbContext" />.
        /// </summary>
        /// <returns> A new query with NoTracking applied.</returns>
        public DbQuery<TResult> AsNoTracking()
        {
            return new DbQuery<TResult>(_internalQuery.AsNoTracking());
        }

        #endregion

        #region Data binding

        /// <summary>
        ///     Returns <c>false</c>.
        /// </summary>
        /// <returns><c>false</c>.</returns>
        bool IListSource.ContainsListCollection
        {
            get { return false; }
        }

        /// <summary>
        ///     Throws an exception indicating that binding directly to a store query is not supported.
        ///     Instead populate a DbSet with data, for example by using the Load extension method, and
        ///     then bind to local data.  For WPF bind to DbSet.Local.  For Windows Forms bind to
        ///     DbSet.Local.ToBindingList().
        /// </summary>
        /// <returns>
        ///     Never returns; always throws.
        /// </returns>
        IList IListSource.GetList()
        {
            throw Error.DbQuery_BindingToDbQueryNotSupported();
        }

        #endregion

        #region IEnumerable

        /// <summary>
        ///     Gets the enumeration of this query causing it to be executed against the store.
        /// </summary>
        /// <returns>An enumerator for the query</returns>
        IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
        {
            return _internalQuery.GetEnumerator();
        }

        /// <summary>
        ///     Gets the enumeration of this query causing it to be executed against the store.
        /// </summary>
        /// <returns>An enumerator for the query</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _internalQuery.GetEnumerator();
        }

        #endregion

        #region IQueryable

        /// <summary>
        ///     The IQueryable element type.
        /// </summary>
        Type IQueryable.ElementType
        {
            get { return _internalQuery.ElementType; }
        }

        /// <summary>
        ///     The IQueryable LINQ Expression.
        /// </summary>
        Expression IQueryable.Expression
        {
            get { return _internalQuery.Expression; }
        }

        /// <summary>
        ///     The IQueryable provider.
        /// </summary>
        IQueryProvider IQueryable.Provider
        {
            get
            {
                return _provider ?? (_provider = new DbQueryProvider(
                                                     _internalQuery.InternalContext,
                                                     _internalQuery.ObjectQueryProvider));
            }
        }

        #endregion

        #region Internal query

        /// <summary>
        ///     The internal query object that is backing this DbQuery
        /// </summary>
        IInternalQuery IInternalQueryAdapter.InternalQuery
        {
            get { return _internalQuery; }
        }

        /// <summary>
        ///     The internal query object that is backing this DbQuery
        /// </summary>
        internal IInternalQuery<TResult> InternalQuery
        {
            get { return _internalQuery; }
        }

        #endregion

        #region ToString

        /// <summary>
        ///     Returns a <see cref = "System.String" /> representation of the underlying query.
        /// </summary>
        /// <returns>
        ///     The query string.
        /// </returns>
        public override string ToString()
        {
            return _internalQuery.ToString();
        }

        #endregion

        #region Conversion to non-generic

        /// <summary>
        ///     Returns a new instance of the non-generic <see cref = "DbQuery" /> class for this query.
        /// </summary>
        /// <returns>A non-generic version.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates",
            Justification = "Intentionally just implicit to reduce API clutter.")]
        public static implicit operator DbQuery(DbQuery<TResult> entry)
        {
            return new InternalDbQuery<TResult>(entry._internalQuery);
        }

        #endregion

        #region Hidden Object methods

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }

        #endregion
    }
}
