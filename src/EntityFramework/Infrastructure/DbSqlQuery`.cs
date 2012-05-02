namespace System.Data.Entity.Infrastructure
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Internal;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Represents a SQL query for entities that is created from a <see cref = "DbContext" /> 
    ///     and is executed using the connection from that context.
    ///     Instances of this class are obtained from the <see cref = "DbSet{TEntity}" /> instance for the 
    ///     entity type. The query is not executed when this object is created; it is executed
    ///     each time it is enumerated, for example by using foreach.
    ///     SQL queries for non-entities are created using the <see cref = "DbContext.Database" />.
    ///     See <see cref = "DbSqlQuery" /> for a non-generic version of this class.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class DbSqlQuery<TEntity> : IEnumerable<TEntity>, IListSource, IDbAsyncEnumerable<TEntity>
        where TEntity : class
    {
        #region Constructors and fields

        private readonly InternalSqlQuery _internalQuery;

        internal DbSqlQuery(InternalSqlQuery internalQuery)
        {
            _internalQuery = internalQuery;
        }

        #endregion

        #region IEnumerable implementation

        /// <summary>
        ///     Executes the query and returns an enumerator for the elements.
        /// </summary>
        /// An
        /// <see cref = "IEnumerator{T}" />
        /// object that can be used to iterate through the elements.
        public IEnumerator<TEntity> GetEnumerator()
        {
            return (IEnumerator<TEntity>)_internalQuery.GetEnumerator();
        }

        /// <summary>
        ///     Executes the query and returns an enumerator for the elements.
        /// </summary>
        /// <returns>
        ///     An <see cref = "T:System.Collections.IEnumerator" /> object that can be used to iterate through the elements.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IDbAsyncEnumerable implementation

        /// <summary>
        /// Gets an enumerator that can be used to asynchronously enumerate the sequence. 
        /// </summary>
        /// <returns>Enumerator for asynchronous enumeration over the sequence.</returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IDbAsyncEnumerator<TEntity> IDbAsyncEnumerable<TEntity>.GetAsyncEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region AsNoTracking

        /// <summary>
        ///     Returns a new query where the results of the query will not be tracked by the associated
        ///     <see cref = "DbContext" />.
        /// </summary>
        /// <returns>A new query with no-tracking applied.</returns>
        public DbSqlQuery<TEntity> AsNoTracking()
        {
            return new DbSqlQuery<TEntity>(InternalQuery.AsNoTracking());
        }

        #endregion

        #region ToString

        /// <summary>
        ///     Returns a <see cref = "System.String" /> that contains the SQL string that was set
        ///     when the query was created.  The parameters are not included.
        /// </summary>
        /// <returns>
        ///     A <see cref = "System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return _internalQuery.ToString();
        }

        #endregion

        #region Access to internal query

        /// <summary>
        ///     Gets the internal query.
        /// </summary>
        /// <value>The internal query.</value>
        internal InternalSqlQuery InternalQuery
        {
            get { return _internalQuery; }
        }

        #endregion

        #region IListSource implementation

        /// <summary>
        ///     Returns <c>false</c>.
        /// </summary>
        /// <returns><c>false</c>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        bool IListSource.ContainsListCollection
        {
            get
            {
                // Note that _internalQuery will always return false;
                return _internalQuery.ContainsListCollection;
            }
        }

        /// <summary>
        ///     Throws an exception indicating that binding directly to a store query is not supported.
        /// </summary>
        /// <returns>
        ///     Never returns; always throws.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IList IListSource.GetList()
        {
            // Note that _internalQuery will always throw;
            return _internalQuery.GetList();
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

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }

        #endregion
    }
}
