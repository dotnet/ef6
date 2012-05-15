namespace System.Data.Entity.Infrastructure
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Internal;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Represents a SQL query for entities that is created from a <see cref = "DbContext" /> 
    ///     and is executed using the connection from that context.
    ///     Instances of this class are obtained from the <see cref = "DbSet" /> instance for the 
    ///     entity type. The query is not executed when this object is created; it is executed
    ///     each time it is enumerated, for example by using foreach.
    ///     SQL queries for non-entities are created using the <see cref = "DbContext.Database" />.
    ///     See <see cref = "DbSqlQuery{TEntity}" /> for a generic version of this class.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    public class DbSqlQuery : IEnumerable, IListSource, IDbAsyncEnumerable<object>
    {
        #region Constructors and fields

        private readonly InternalSqlQuery _internalQuery;

        /// <summary>
        ///     Initializes a new instance of the <see cref = "DbSqlQuery" /> class.
        /// </summary>
        /// <param name = "internalQuery">The internal query.</param>
        internal DbSqlQuery(InternalSqlQuery internalQuery)
        {
            _internalQuery = internalQuery;
        }

        #endregion

        #region IEnumerable implementation

        /// <summary>
        ///     Executes the query and returns an enumerator for the elements.
        /// </summary>
        /// <returns>
        ///     An <see cref = "T:System.Collections.IEnumerator" /> object that can be used to iterate through the elements.
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            return _internalQuery.GetEnumerator();
        }

        #endregion

        #region IDbAsyncEnumerable implementation

        /// <summary>
        /// Gets an enumerator that can be used to asynchronously enumerate the sequence. 
        /// </summary>
        /// <returns>Enumerator for asynchronous enumeration over the sequence.</returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IDbAsyncEnumerator<object> IDbAsyncEnumerable<object>.GetAsyncEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDbAsyncEnumerable extension methods

        /// <summary>
        ///     Enumerates the SQL query asynchronously and executes the provided action on each element.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public Task ForEachAsync(Action<object> action)
        {
            Contract.Requires(action != null);

            throw new NotImplementedException();
        }

        /// <summary>
        ///     Enumerates the SQL query asynchronously and executes the provided action on each element.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public Task ForEachAsync(Action<object> action, CancellationToken cancellationToken)
        {
            Contract.Requires(action != null);

            throw new NotImplementedException();
        }

        /// <summary>
        ///     Creates a <see cref = "List{Object}" /> from the SQL query by enumerating it asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task containing a <see cref = "List{Object}" /> that contains elements from the input sequence.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public Task<List<object>> ToListAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Creates a <see cref = "List{Object}" /> from the SQL query by enumerating it asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task containing a <see cref = "List{Object}" /> that contains elements from the input sequence.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public Task<List<object>> ToListAsync(CancellationToken cancellationToken)
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
        public DbSqlQuery AsNoTracking()
        {
            return new DbSqlQuery(InternalQuery.AsNoTracking());
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
