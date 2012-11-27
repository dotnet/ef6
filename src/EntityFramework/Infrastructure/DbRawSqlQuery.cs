// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Represents a SQL query for non-entities that is created from a <see cref="DbContext" />
    ///     and is executed using the connection from that context.
    ///     Instances of this class are obtained from the <see cref="DbContext.Database" /> instance.
    ///     The query is not executed when this object is created; it is executed
    ///     each time it is enumerated, for example by using foreach.
    ///     SQL queries for entities are created using <see cref="DbSet.SqlQuery" />.
    ///     See <see cref="DbRawSqlQuery{TElement}" /> for a generic version of this class.
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbRawSqlQuery" /> class.
        /// </summary>
        /// <param name="internalQuery"> The internal query. </param>
        internal DbRawSqlQuery(InternalSqlQuery internalQuery)
        {
            _internalQuery = internalQuery;
        }

        #endregion

        #region IEnumerable implementation

        /// <summary>
        ///     Returns an <see cref="IEnumerator" /> which when enumerated will execute the SQL query against the database.
        /// </summary>
        /// <returns>
        ///     An <see cref="IEnumerator" /> object that can be used to iterate through the elements.
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            return _internalQuery.GetEnumerator();
        }

        #endregion

        #region IDbAsyncEnumerable implementation

#if !NET40

        /// <summary>
        ///     Returns an <see cref="IDbAsyncEnumerable" /> which when enumerated will execute the SQL query against the database.
        /// </summary>
        /// <returns>
        ///     An <see cref="IDbAsyncEnumerable" /> object that can be used to iterate through the elements.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
        {
            return _internalQuery.GetAsyncEnumerator();
        }

#endif

        #endregion

        #region Access to IDbAsyncEnumerable extensions

#if !NET40

        public Task ForEachAsync(Action<object> action)
        {
            Check.NotNull(action, "action");

            return ((IDbAsyncEnumerable)this).ForEachAsync(action, CancellationToken.None);
        }

        public Task ForEachAsync(Action<object> action, CancellationToken cancellationToken)
        {
            Check.NotNull(action, "action");

            return ((IDbAsyncEnumerable)this).ForEachAsync(action, cancellationToken);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<List<T>> ToListAsync<T>()
        {
            return ((IDbAsyncEnumerable)this).ToListAsync<T>();
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<List<T>> ToListAsync<T>(CancellationToken cancellationToken)
        {
            return ((IDbAsyncEnumerable)this).ToListAsync<T>(cancellationToken);
        }

#endif

        #endregion

        #region ToString

        /// <summary>
        ///     Returns a <see cref="System.String" /> that contains the SQL string that was set
        ///     when the query was created.  The parameters are not included.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
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
        /// <returns>
        ///     <c>false</c> .
        /// </returns>
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
