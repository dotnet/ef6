// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a SQL query for non-entities that is created from a <see cref="DbContext" />
    /// and is executed using the connection from that context.
    /// Instances of this class are obtained from the <see cref="DbContext.Database" /> instance.
    /// The query is not executed when this object is created; it is executed
    /// each time it is enumerated, for example by using foreach.
    /// SQL queries for entities are created using <see cref="DbSet.SqlQuery" />.
    /// See <see cref="DbRawSqlQuery{TElement}" /> for a generic version of this class.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    public class DbRawSqlQuery : IEnumerable, IListSource
#if !NET40
, IDbAsyncEnumerable
#endif
    {
        #region Constructors and fields

        private readonly InternalSqlQuery _internalQuery;

        // <summary>
        // Initializes a new instance of the <see cref="DbRawSqlQuery" /> class.
        // </summary>
        // <param name="internalQuery"> The internal query. </param>
        internal DbRawSqlQuery(InternalSqlQuery internalQuery)
        {
            _internalQuery = internalQuery;
        }

        #endregion

        #region AsStreaming

        /// <summary>
        /// Returns a new query that will stream the results instead of buffering.
        /// </summary>
        /// <returns> A new query with AsStreaming applied. </returns>
        [Obsolete("Queries are now streaming by default unless a retrying ExecutionStrategy is used. Calling this method will have no effect.")]
        public virtual DbRawSqlQuery AsStreaming()
        {
            return _internalQuery == null ? this : new DbRawSqlQuery(_internalQuery.AsStreaming());
        }

        #endregion

        #region IEnumerable implementation

        /// <summary>
        /// Returns an <see cref="IEnumerator" /> which when enumerated will execute the SQL query against the database.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the elements.
        /// </returns>
        public virtual IEnumerator GetEnumerator()
        {
            return GetInternalQueryWithCheck("GetEnumerator").GetEnumerator();
        }

        #endregion

        #region IDbAsyncEnumerable implementation

#if !NET40

        /// <summary>
        /// Returns an <see cref="IDbAsyncEnumerable" /> which when enumerated will execute the SQL query against the database.
        /// </summary>
        /// <returns>
        /// An <see cref="IDbAsyncEnumerable" /> object that can be used to iterate through the elements.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
        {
            return GetInternalQueryWithCheck("IDbAsyncEnumerable.GetAsyncEnumerator").GetAsyncEnumerator();
        }

#endif

        #endregion

        #region Access to IDbAsyncEnumerable extensions

#if !NET40

        /// <summary>
        /// Asynchronously enumerates the query results and performs the specified action on each element.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="action"> The action to perform on each element. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public virtual Task ForEachAsync(Action<object> action)
        {
            Check.NotNull(action, "action");

            return ((IDbAsyncEnumerable)this).ForEachAsync(action, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously enumerates the query results and performs the specified action on each element.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="action"> The action to perform on each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public virtual Task ForEachAsync(Action<object> action, CancellationToken cancellationToken)
        {
            Check.NotNull(action, "action");

            return ((IDbAsyncEnumerable)this).ForEachAsync(action, cancellationToken);
        }

        /// <summary>
        /// Creates a <see cref="List{T}" /> from the query by enumerating it asynchronously.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="List{T}" /> that contains elements from the query.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public virtual Task<List<object>> ToListAsync()
        {
            return ((IDbAsyncEnumerable)this).ToListAsync<object>();
        }

        /// <summary>
        /// Creates a <see cref="List{T}" /> from the query by enumerating it asynchronously.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="List{T}" /> that contains elements from the query.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public virtual Task<List<object>> ToListAsync(CancellationToken cancellationToken)
        {
            return ((IDbAsyncEnumerable)this).ToListAsync<object>(cancellationToken);
        }

#endif

        #endregion

        #region ToString

        /// <summary>
        /// Returns a <see cref="System.String" /> that contains the SQL string that was set
        /// when the query was created.  The parameters are not included.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return _internalQuery == null ? base.ToString() : _internalQuery.ToString();
        }

        #endregion

        #region Access to internal query

        // <summary>
        // Gets the internal query.
        // </summary>
        // <value> The internal query. </value>
        internal InternalSqlQuery InternalQuery
        {
            get { return _internalQuery; }
        }

        private InternalSqlQuery GetInternalQueryWithCheck(string memberName)
        {
            if (_internalQuery == null)
            {
                throw new NotImplementedException(Strings.TestDoubleNotImplemented(memberName, GetType().Name, typeof(DbSqlQuery).Name));
            }

            return _internalQuery;
        }

        #endregion

        #region IListSource implementation

        /// <summary>
        /// Returns <c>false</c>.
        /// </summary>
        /// <returns>
        /// <c>false</c> .
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        bool IListSource.ContainsListCollection
        {
            get { return false; }
        }

        /// <summary>
        /// Throws an exception indicating that binding directly to a store query is not supported.
        /// </summary>
        /// <returns> Never returns; always throws. </returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IList IListSource.GetList()
        {
            throw Error.DbQuery_BindingToDbQueryNotSupported();
        }

        #endregion

        #region Hidden Object methods

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

        #endregion
    }
}
