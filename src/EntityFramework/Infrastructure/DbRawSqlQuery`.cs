// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
    ///     Represents a SQL query for non-entities that is created from a <see cref="DbContext" /> 
    ///     and is executed using the connection from that context.
    ///     Instances of this class are obtained from the <see cref="DbContext.Database" /> instance.
    ///     The query is not executed when this object is created; it is executed
    ///     each time it is enumerated, for example by using foreach.
    ///     SQL queries for entities are created using <see cref="DbSet{TEntity}.SqlQuery" />.
    ///     See <see cref="DbRawSqlQuery{TElement}" /> for a non-generic version of this class.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class DbRawSqlQuery<TElement> : IEnumerable<TElement>, IListSource
#if !NET40
        , IDbAsyncEnumerable<TElement>
#endif
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

#if !NET40

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

#endif

        #endregion

        #region Access to IDbAsyncEnumerable extensions

#if !NET40

        public Task ForEachAsync(Action<TElement> action)
        {
            Contract.Requires(action != null);
            Contract.Ensures(Contract.Result<Task>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ForEachAsync(action, CancellationToken.None);
        }

        public Task ForEachAsync(Action<TElement> action, CancellationToken cancellationToken)
        {
            Contract.Requires(action != null);
            Contract.Ensures(Contract.Result<Task>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ForEachAsync(action, cancellationToken);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<List<TElement>> ToListAsync()
        {
            Contract.Ensures(Contract.Result<Task<List<TElement>>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToListAsync();
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<List<TElement>> ToListAsync(CancellationToken cancellationToken)
        {
            return ((IDbAsyncEnumerable<TElement>)this).ToListAsync(cancellationToken);
        }

        public Task<TElement[]> ToArrayAsync()
        {
            Contract.Ensures(Contract.Result<Task<TElement[]>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToArrayAsync();
        }

        public Task<TElement[]> ToArrayAsync(CancellationToken cancellationToken)
        {
            Contract.Ensures(Contract.Result<Task<TElement[]>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToArrayAsync(cancellationToken);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey>(Func<TElement, TKey> keySelector)
        {
            Contract.Requires(keySelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey>(Func<TElement, TKey> keySelector, CancellationToken cancellationToken)
        {
            Contract.Requires(keySelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector, cancellationToken);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey>(Func<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            Contract.Requires(keySelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector, comparer);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey>(Func<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
        {
            Contract.Requires(keySelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector, comparer, cancellationToken);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey>(Func<TElement, TKey> keySelector, Func<TElement, TElement> elementSelector)
        {
            Contract.Requires(keySelector != null);
            Contract.Requires(elementSelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector, elementSelector);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey>(Func<TElement, TKey> keySelector, Func<TElement, TElement> elementSelector, CancellationToken cancellationToken)
        {
            Contract.Requires(keySelector != null);
            Contract.Requires(elementSelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector, elementSelector, cancellationToken);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey>(Func<TElement, TKey> keySelector, Func<TElement, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            Contract.Requires(keySelector != null);
            Contract.Requires(elementSelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector, elementSelector, comparer);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey>(Func<TElement, TKey> keySelector, Func<TElement, TElement> elementSelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
        {
            Contract.Requires(keySelector != null);
            Contract.Requires(elementSelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector, elementSelector, comparer, cancellationToken);
        }

        public Task<TElement> FirstAsync()
        {
            Contract.Ensures(Contract.Result<Task>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).FirstAsync();
        }

        public Task<TElement> FirstAsync(Func<TElement, bool> predicate)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).FirstAsync(predicate);
        }

        public Task<TElement> FirstAsync(CancellationToken cancellationToken)
        {
            Contract.Ensures(Contract.Result<Task>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).FirstAsync(cancellationToken);
        }

        public Task<TElement> FirstAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).FirstAsync(predicate, cancellationToken);
        }

        public Task<TElement> FirstOrDefaultAsync()
        {
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).FirstOrDefaultAsync();
        }

        public Task<TElement> FirstOrDefaultAsync(Func<TElement, bool> predicate)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).FirstOrDefaultAsync(predicate);
        }

        public Task<TElement> FirstOrDefaultAsync(CancellationToken cancellationToken)
        {
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).FirstOrDefaultAsync(cancellationToken);
        }

        public Task<TElement> FirstOrDefaultAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public Task<TElement> SingleAsync()
        {
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).SingleAsync();
        }

        public Task<TElement> SingleAsync(CancellationToken cancellationToken)
        {
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).SingleAsync(cancellationToken);
        }

        public Task<TElement> SingleAsync(Func<TElement, bool> predicate)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).SingleAsync(predicate);
        }

        public Task<TElement> SingleAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).SingleAsync(predicate, cancellationToken);
        }

        public Task<TElement> SingleOrDefaultAsync()
        {
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).SingleOrDefaultAsync();
        }

        public Task<TElement> SingleOrDefaultAsync(CancellationToken cancellationToken)
        {
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).SingleOrDefaultAsync(cancellationToken);
        }

        public Task<TElement> SingleOrDefaultAsync(Func<TElement, bool> predicate)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).SingleOrDefaultAsync(predicate);
        }

        public Task<TElement> SingleOrDefaultAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).SingleOrDefaultAsync(predicate, cancellationToken);
        }

        public Task<bool> ContainsAsync(TElement value)
        {
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ContainsAsync(value);
        }

        public Task<bool> ContainsAsync(TElement value, CancellationToken cancellationToken)
        {
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ContainsAsync(value, cancellationToken);
        }

        public Task<bool> AnyAsync()
        {
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).AnyAsync();
        }

        public Task<bool> AnyAsync(CancellationToken cancellationToken)
        {
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).AnyAsync(cancellationToken);
        }

        public Task<bool> AnyAsync(Func<TElement, bool> predicate)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).AnyAsync(predicate);
        }

        public Task<bool> AnyAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).AnyAsync(predicate, cancellationToken);
        }

        public Task<bool> AllAsync(Func<TElement, bool> predicate)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).AllAsync(predicate);
        }

        public Task<bool> AllAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).AllAsync(predicate, cancellationToken);
        }

        public Task<int> CountAsync()
        {
            Contract.Ensures(Contract.Result<Task<int>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).CountAsync();
        }

        public Task<int> CountAsync(CancellationToken cancellationToken)
        {
            Contract.Ensures(Contract.Result<Task<int>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).CountAsync(cancellationToken);
        }

        public Task<int> CountAsync(Func<TElement, bool> predicate)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<int>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).CountAsync(predicate);
        }

        public Task<int> CountAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<int>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).CountAsync(predicate, cancellationToken);
        }

        public Task<long> LongCountAsync()
        {
            Contract.Ensures(Contract.Result<Task<long>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).LongCountAsync();
        }

        public Task<long> LongCountAsync(CancellationToken cancellationToken)
        {
            Contract.Ensures(Contract.Result<Task<long>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).LongCountAsync(cancellationToken);
        }

        public Task<long> LongCountAsync(Func<TElement, bool> predicate)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<long>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).LongCountAsync(predicate);
        }

        public Task<long> LongCountAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<long>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).LongCountAsync(predicate, cancellationToken);
        }

        public Task<TElement> MinAsync()
        {
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).MinAsync();
        }

        public Task<TElement> MinAsync(CancellationToken cancellationToken)
        {
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).MinAsync(cancellationToken);
        }

        public Task<TElement> MaxAsync()
        {
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).MaxAsync();
        }

        public Task<TElement> MaxAsync(CancellationToken cancellationToken)
        {
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).MaxAsync(cancellationToken);
        }

#endif

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
