// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Internal;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Represents a SQL query for non-entities that is created from a <see cref="DbContext" /> 
    ///     and is executed using the connection from that context.
    ///     Instances of this class are obtained from the <see cref="DbContext.Database" /> instance.
    ///     The query is not executed when this object is created; it is executed
    ///     each time it is enumerated, for example by using foreach.
    ///     SQL queries for entities are created using <see cref="DbSet{TEntity}.SqlQuery" />.
    ///     See <see cref="DbRawSqlQuery{TElement}" /> for a non-generic version of this class.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class DbRawSqlQuery<TElement> : IEnumerable<TElement>, IDbAsyncEnumerable<TElement>, IListSource
    {
        #region Constructors and fields

        private readonly InternalSqlQuery _internalQuery;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbRawSqlQuery{TElement}" /> class.
        /// </summary>
        /// <param name="internalQuery"> The internal query. </param>
        internal DbRawSqlQuery(InternalSqlQuery internalQuery)
        {
            _internalQuery = internalQuery;
        }

        #endregion

        #region IEnumerable implementation

        /// <summary>
        ///     Returns an <see cref="IEnumerator{TEntity}" /> which when enumerated will execute the SQL query against the database.
        /// </summary>
        /// <returns> An <see cref="IEnumerator{TEntity}" /> object that can be used to iterate through the elements. </returns>
        public IEnumerator<TElement> GetEnumerator()
        {
            return (IEnumerator<TElement>)_internalQuery.GetEnumerator();
        }

        /// <summary>
        ///     Returns an <see cref="IEnumerator" /> which when enumerated will execute the SQL query against the database.
        /// </summary>
        /// <returns> An <see cref="IEnumerator" /> object that can be used to iterate through the elements. </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IDbAsyncEnumerable implementation

        /// <summary>
        ///     Returns an <see cref="IDbAsyncEnumerable{T}" /> which when enumerated will execute the SQL query against the database.
        /// </summary>
        /// <returns> An <see cref="IDbAsyncEnumerable{T}" /> object that can be used to iterate through the elements. </returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IDbAsyncEnumerator<TElement> IDbAsyncEnumerable<TElement>.GetAsyncEnumerator()
        {
            return (IDbAsyncEnumerator<TElement>)_internalQuery.GetAsyncEnumerator();
        }

        /// <summary>
        ///     Returns an <see cref="IDbAsyncEnumerable" /> which when enumerated will execute the SQL query against the database.
        /// </summary>
        /// <returns> An <see cref="IDbAsyncEnumerable" /> object that can be used to iterate through the elements. </returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
        {
            return _internalQuery.GetAsyncEnumerator();
        }

        #endregion

        #region ToString

        /// <summary>
        ///     Returns a <see cref="System.String" /> that contains the SQL string that was set
        ///     when the query was created.  The parameters are not included.
        /// </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString()
        {
            return _internalQuery.ToString();
        }

        #endregion

        #region Access to internal query

        /// <summary>
        ///     Gets the internal query.
        /// </summary>
        /// <value> The internal query. </value>
        internal InternalSqlQuery InternalQuery
        {
            get { return _internalQuery; }
        }

        #endregion

        #region IListSource implementation

        /// <summary>
        ///     Returns <c>false</c>.
        /// </summary>
        /// <returns> <c>false</c> . </returns>
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
        /// <returns> Never returns; always throws. </returns>
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
