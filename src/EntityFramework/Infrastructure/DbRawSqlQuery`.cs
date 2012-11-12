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
    ///     each time it is enumerated, for example by using <c>foreach</c>.
    ///     SQL queries for entities are created using <see cref="DbSet{TEntity}.SqlQuery" />.
    ///     See <see cref="DbRawSqlQuery" /> for a non-generic version of this class.
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

        /// <summary>
        ///     Asynchronously executes the provided action on each element of the query result.
        /// </summary>
        /// <param name="action"> The action to be executed. </param>
        /// <returns> A Task representing the asynchronous operation. </returns>
        public Task ForEachAsync(Action<TElement> action)
        {
            Contract.Requires(action != null);
            Contract.Ensures(Contract.Result<Task>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ForEachAsync(action, CancellationToken.None);
        }

        /// <summary>
        ///     Asynchronously executes the provided action on each element of the query result.
        /// </summary>
        /// <param name="action"> The action to be executed. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A Task representing the asynchronous operation. </returns>
        public Task ForEachAsync(Action<TElement> action, CancellationToken cancellationToken)
        {
            Contract.Requires(action != null);
            Contract.Ensures(Contract.Result<Task>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ForEachAsync(action, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the result as a <see cref="List{TElement}" />.
        /// </summary>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing the query result. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<List<TElement>> ToListAsync()
        {
            Contract.Ensures(Contract.Result<Task<List<TElement>>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToListAsync();
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the result as a <see cref="List{TElement}" />.
        /// </summary>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing the query result. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<List<TElement>> ToListAsync(CancellationToken cancellationToken)
        {
            return ((IDbAsyncEnumerable<TElement>)this).ToListAsync(cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the result as an array.
        /// </summary>
        /// <returns> A <see cref="Task" /> containing the query result. </returns>
        public Task<TElement[]> ToArrayAsync()
        {
            Contract.Ensures(Contract.Result<Task<TElement[]>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToArrayAsync();
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the result as an array.
        /// </summary>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing the query result. </returns>
        public Task<TElement[]> ToArrayAsync(CancellationToken cancellationToken)
        {
            Contract.Ensures(Contract.Result<Task<TElement[]>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the result as a <see cref="Dictionary{TKey, TElement}" />
        ///     according to a specified key selector function.
        /// </summary>
        /// <typeparam name="TKey"> The type of the key returned by <paramref name="keySelector" /> . </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <returns> A <see cref="Task" /> containing the query result. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey>(Func<TElement, TKey> keySelector)
        {
            Contract.Requires(keySelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the result as a <see cref="Dictionary{TKey, TElement}" />
        ///     according to a specified key selector function.
        /// </summary>
        /// <typeparam name="TKey"> The type of the key returned by <paramref name="keySelector" /> . </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing the query result. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey>(
            Func<TElement, TKey> keySelector, CancellationToken cancellationToken)
        {
            Contract.Requires(keySelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the result as a <see cref="Dictionary{TKey, TElement}" />
        ///     according to a specified key selector function and a comparer.
        /// </summary>
        /// <typeparam name="TKey"> The type of the key returned by <paramref name="keySelector" /> . </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="comparer"> An <see cref="IEqualityComparer{TKey}" /> to compare keys. </param>
        /// <returns> A <see cref="Task" /> containing the query result. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey>(Func<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            Contract.Requires(keySelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector, comparer);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the result as a <see cref="Dictionary{TKey, TElement}" />
        ///     according to a specified key selector function and a comparer.
        /// </summary>
        /// <typeparam name="TKey"> The type of the key returned by <paramref name="keySelector" /> . </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="comparer"> An <see cref="IEqualityComparer{TKey}" /> to compare keys. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing the query result. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey>(
            Func<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
        {
            Contract.Requires(keySelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector, comparer, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the result as a <see cref="Dictionary{TKey, TResult}" />
        ///     according to a specified key selector and an element selector function.
        /// </summary>
        /// <typeparam name="TKey"> The type of the key returned by <paramref name="keySelector" /> . </typeparam>
        /// <typeparam name="TResult"> The type of the value returned by <paramref name="elementSelector" /> . </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <returns> A <see cref="Task" /> containing the query result. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TResult>> ToDictionaryAsync<TKey, TResult>(
            Func<TElement, TKey> keySelector, Func<TElement, TResult> elementSelector)
        {
            Contract.Requires(keySelector != null);
            Contract.Requires(elementSelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector, elementSelector);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the result as a <see cref="Dictionary{TKey, TResult}" />
        ///     according to a specified key selector and an element selector function.
        /// </summary>
        /// <typeparam name="TKey"> The type of the key returned by <paramref name="keySelector" /> . </typeparam>
        /// <typeparam name="TResult"> The type of the value returned by <paramref name="elementSelector" /> . </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing the query result. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TResult>> ToDictionaryAsync<TKey, TResult>(
            Func<TElement, TKey> keySelector, Func<TElement, TResult> elementSelector, CancellationToken cancellationToken)
        {
            Contract.Requires(keySelector != null);
            Contract.Requires(elementSelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector, elementSelector, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the result as a <see cref="Dictionary{TKey, TResult}" />
        ///     according to a specified key selector function, a comparer, and an element selector function.
        /// </summary>
        /// <typeparam name="TKey"> The type of the key returned by <paramref name="keySelector" /> . </typeparam>
        /// <typeparam name="TResult"> The type of the value returned by <paramref name="elementSelector" /> . </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <param name="comparer"> An <see cref="IEqualityComparer{TKey}" /> to compare keys. </param>
        /// <returns> A <see cref="Task" /> containing the query result. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TResult>> ToDictionaryAsync<TKey, TResult>(
            Func<TElement, TKey> keySelector, Func<TElement, TResult> elementSelector, IEqualityComparer<TKey> comparer)
        {
            Contract.Requires(keySelector != null);
            Contract.Requires(elementSelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector, elementSelector, comparer);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the result as a <see cref="Dictionary{TKey, TResult}" />
        ///     according to a specified key selector function, a comparer, and an element selector function.
        /// </summary>
        /// <typeparam name="TKey"> The type of the key returned by <paramref name="keySelector" /> . </typeparam>
        /// <typeparam name="TResult"> The type of the value returned by <paramref name="elementSelector" /> . </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <param name="comparer"> An <see cref="IEqualityComparer{TKey}" /> to compare keys. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing the query result. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TResult>> ToDictionaryAsync<TKey, TResult>(
            Func<TElement, TKey> keySelector, Func<TElement, TResult> elementSelector, IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken)
        {
            Contract.Requires(keySelector != null);
            Contract.Requires(elementSelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector, elementSelector, comparer, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the first element of the result.
        /// </summary>
        /// <returns> A <see cref="Task" /> containing the first element in the query result. </returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public Task<TElement> FirstAsync()
        {
            Contract.Ensures(Contract.Result<Task>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).FirstAsync();
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the first element of the result.
        /// </summary>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing the first element in the query result. </returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public Task<TElement> FirstAsync(CancellationToken cancellationToken)
        {
            Contract.Ensures(Contract.Result<Task>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).FirstAsync(cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the first element of the result that satisfies a specified condition.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns> A <see cref="Task" /> containing the first element in the query result that satisfies a specified condition. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="predicate" />
        ///     is
        ///     <c>null</c>
        ///     .</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public Task<TElement> FirstAsync(Func<TElement, bool> predicate)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).FirstAsync(predicate);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the first element of the result that satisfies a specified condition.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing the first element in the query result that satisfies a specified condition. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="predicate" />
        ///     is
        ///     <c>null</c>
        ///     .</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public Task<TElement> FirstAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).FirstAsync(predicate, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the first element or a default value if no such element is found.
        /// </summary>
        /// <returns> A <see cref="Task" /> containing <c>default</c> ( <typeparamref name="TElement" /> ) if query result is empty; otherwise, the first element in the query result. </returns>
        public Task<TElement> FirstOrDefaultAsync()
        {
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).FirstOrDefaultAsync();
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the first element or a default value if no such element is found.
        /// </summary>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing <c>default</c> ( <typeparamref name="TElement" /> ) if query result is empty; otherwise, the first element in the query result. </returns>
        public Task<TElement> FirstOrDefaultAsync(CancellationToken cancellationToken)
        {
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the first element of the result that satisfies a specified condition
        ///     or a default value if no such element is found.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns> A <see cref="Task" /> containing <c>default</c> ( <typeparamref name="TElement" /> ) if query result is empty or if no element passes the test specified by <paramref
        ///      name="predicate" /> ; otherwise, the first element in the query result that passes the test specified by <paramref
        ///      name="predicate" /> . </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="predicate" />
        ///     is
        ///     <c>null</c>
        ///     .</exception>
        public Task<TElement> FirstOrDefaultAsync(Func<TElement, bool> predicate)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the first element of the result that satisfies a specified condition
        ///     or a default value if no such element is found.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing <c>default</c> ( <typeparamref name="TElement" /> ) if query result is empty or if no element passes the test specified by <paramref
        ///      name="predicate" /> ; otherwise, the first element in the query result that passes the test specified by <paramref
        ///      name="predicate" /> . </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="predicate" />
        ///     is
        ///     <c>null</c>
        ///     .</exception>
        public Task<TElement> FirstOrDefaultAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).FirstOrDefaultAsync(predicate, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the only element of the result
        ///     and throws an exception if there is not exactly one such element.
        /// </summary>
        /// <returns> A <see cref="Task" /> containing the single element of the query result. </returns>
        /// <exception cref="InvalidOperationException">The query result has more than one element.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public Task<TElement> SingleAsync()
        {
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).SingleAsync();
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the only element of the result
        ///     and throws an exception if there is not exactly one such element.
        /// </summary>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing the single element of the query result. </returns>
        /// <exception cref="InvalidOperationException">The query result has more than one element.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public Task<TElement> SingleAsync(CancellationToken cancellationToken)
        {
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).SingleAsync(cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the only element of the result that satisfies a specified condition
        ///     and throws an exception if there is not exactly one such element.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns> A <see cref="Task" /> containing the single element of the query result that satisfies the condition in <paramref
        ///      name="predicate" /> . </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="predicate" />
        ///     is
        ///     <c>null</c>
        ///     .</exception>
        /// <exception cref="InvalidOperationException">No element satisfies the condition in
        ///     <paramref name="predicate" />
        ///     .</exception>
        /// <exception cref="InvalidOperationException">More than one element satisfies the condition in
        ///     <paramref name="predicate" />
        ///     .</exception>
        public Task<TElement> SingleAsync(Func<TElement, bool> predicate)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).SingleAsync(predicate);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the only element of the result that satisfies a specified condition
        ///     and throws an exception if there is not exactly one such element.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing the single element of the query result that satisfies the condition in <paramref
        ///      name="predicate" /> . </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="predicate" />
        ///     is
        ///     <c>null</c>
        ///     .</exception>
        /// <exception cref="InvalidOperationException">No element satisfies the condition in
        ///     <paramref name="predicate" />
        ///     .</exception>
        /// <exception cref="InvalidOperationException">More than one element satisfies the condition in
        ///     <paramref name="predicate" />
        ///     .</exception>
        public Task<TElement> SingleAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).SingleAsync(predicate, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the only element of the result or
        ///     a default value if no such element exists, and throws an exception if there is more than one such element.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns> A <see cref="Task" /> containing the single element of the query result or <c>default</c> ( <typeparamref
        ///      name="TElement" /> ) if no such element is found. </returns>
        /// <exception cref="InvalidOperationException">The query result has more than one element.</exception>
        public Task<TElement> SingleOrDefaultAsync()
        {
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).SingleOrDefaultAsync();
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the only element of the result or
        ///     a default value if no such element exists, and throws an exception if there is more than one such element.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing the single element of the query result or <c>default</c> ( <typeparamref
        ///      name="TElement" /> ) if no such element is found. </returns>
        /// <exception cref="InvalidOperationException">The query result has more than one element.</exception>
        public Task<TElement> SingleOrDefaultAsync(CancellationToken cancellationToken)
        {
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).SingleOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the only element of the result that satisfies a specified condition or
        ///     a default value if no such element exists, and throws an exception if there is more than one such element.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns> A <see cref="Task" /> containing the single element of the query result that satisfies the condition in <paramref
        ///      name="predicate" /> , or <c>default</c> ( <typeparamref name="TElement" /> ) if no such element is found. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="predicate" />
        ///     is
        ///     <c>null</c>
        ///     .</exception>
        /// <exception cref="InvalidOperationException">More than one element satisfies the condition in
        ///     <paramref name="predicate" />
        ///     .</exception>
        public Task<TElement> SingleOrDefaultAsync(Func<TElement, bool> predicate)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).SingleOrDefaultAsync(predicate);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the only element of the result that satisfies a specified condition or
        ///     a default value if no such element exists, and throws an exception if there is more than one such element.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing the single element of the query result that satisfies the condition in <paramref
        ///      name="predicate" /> , or <c>default</c> ( <typeparamref name="TElement" /> ) if no such element is found. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="predicate" />
        ///     is
        ///     <c>null</c>
        ///     .</exception>
        /// <exception cref="InvalidOperationException">More than one element satisfies the condition in
        ///     <paramref name="predicate" />
        ///     .</exception>
        public Task<TElement> SingleOrDefaultAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).SingleOrDefaultAsync(predicate, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and determines whether the result contains a specified element by using the default equality comparer.
        /// </summary>
        /// <param name="value"> The object to locate in the query result. </param>
        /// <returns> A <see cref="Task" /> containing <c>true</c> if the query result contains the specified value; otherwise, <c>false</c> . </returns>
        public Task<bool> ContainsAsync(TElement value)
        {
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ContainsAsync(value);
        }

        /// <summary>
        ///     Asynchronously executes the query and determines whether the result contains a specified element by using the default equality comparer.
        /// </summary>
        /// <param name="value"> The object to locate in the query result. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing <c>true</c> if the query result contains the specified value; otherwise, <c>false</c> . </returns>
        public Task<bool> ContainsAsync(TElement value, CancellationToken cancellationToken)
        {
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).ContainsAsync(value, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and determines whether the result contains any elements.
        /// </summary>
        /// <returns> A <see cref="Task" /> containing <c>true</c> if the query result contains any elements; otherwise, <c>false</c> . </returns>
        public Task<bool> AnyAsync()
        {
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).AnyAsync();
        }

        /// <summary>
        ///     Asynchronously executes the query and determines whether the result contains any elements.
        /// </summary>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing <c>true</c> if the query result contains any elements; otherwise, <c>false</c> . </returns>
        public Task<bool> AnyAsync(CancellationToken cancellationToken)
        {
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).AnyAsync(cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and determines whether any element of the result satisfies a condition.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns> A <see cref="Task" /> containing <c>true</c> if any elements in the query result pass the test in the specified predicate; otherwise, <c>false</c> . </returns>
        public Task<bool> AnyAsync(Func<TElement, bool> predicate)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).AnyAsync(predicate);
        }

        /// <summary>
        ///     Asynchronously executes the query and determines whether any element of the result satisfies a condition.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing <c>true</c> if any elements in the query result pass the test in the specified predicate; otherwise, <c>false</c> . </returns>
        public Task<bool> AnyAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).AnyAsync(predicate, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and determines whether any element of the result satisfies a condition.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns> A <see cref="Task" /> containing <c>true</c> if every element of the query result passes the test in the specified predicate; otherwise, <c>false</c> . </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="predicate" />
        ///     is
        ///     <c>null</c>
        ///     .</exception>
        public Task<bool> AllAsync(Func<TElement, bool> predicate)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).AllAsync(predicate);
        }

        /// <summary>
        ///     Asynchronously executes the query and determines whether any element of the result satisfies a condition.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing <c>true</c> if every element of the query result passes the test in the specified predicate; otherwise, <c>false</c> . </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="predicate" />
        ///     is
        ///     <c>null</c>
        ///     .</exception>
        public Task<bool> AllAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).AllAsync(predicate, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the number of elements in the result.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns> A <see cref="Task" /> containing the number of elements in the query result. </returns>
        /// <exception cref="OverflowException">The number of elements in the query result is larger than
        ///     <see cref="Int32.MaxValue" />
        ///     .</exception>
        public Task<int> CountAsync()
        {
            Contract.Ensures(Contract.Result<Task<int>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).CountAsync();
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the number of elements in the result.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing the number of elements in the query result. </returns>
        /// <exception cref="OverflowException">The number of elements in the query result is larger than
        ///     <see cref="Int32.MaxValue" />
        ///     .</exception>
        public Task<int> CountAsync(CancellationToken cancellationToken)
        {
            Contract.Ensures(Contract.Result<Task<int>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).CountAsync(cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the number of elements in the result that satisfy a condition.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns> A <see cref="Task" /> containing the number of elements in the query result that satisfies the condition in the predicate function. </returns>
        /// <exception cref="OverflowException">The number of elements in the query result that satisfy the condition in the predicate function
        ///     is larger than
        ///     <see cref="Int32.MaxValue" />
        ///     .</exception>
        public Task<int> CountAsync(Func<TElement, bool> predicate)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<int>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).CountAsync(predicate);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the number of elements in the result that satisfy a condition.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing the number of elements in the query result that satisfies the condition in the predicate function. </returns>
        /// <exception cref="OverflowException">The number of elements in the query result that satisfy the condition in the predicate function
        ///     is larger than
        ///     <see cref="Int32.MaxValue" />
        ///     .</exception>
        public Task<int> CountAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<int>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).CountAsync(predicate, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the number of elements in the result.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing the number of elements in the query result. </returns>
        /// <exception cref="OverflowException">The number of elements in the query result is larger than
        ///     <see cref="Int64.MaxValue" />
        ///     .</exception>
        public Task<long> LongCountAsync()
        {
            Contract.Ensures(Contract.Result<Task<long>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).LongCountAsync();
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the number of elements in the result.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing the number of elements in the query result. </returns>
        /// <exception cref="OverflowException">The number of elements in the query result is larger than
        ///     <see cref="Int64.MaxValue" />
        ///     .</exception>
        public Task<long> LongCountAsync(CancellationToken cancellationToken)
        {
            Contract.Ensures(Contract.Result<Task<long>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).LongCountAsync(cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the number of elements in the result that satisfy a condition.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns> A <see cref="Task" /> containing the number of elements in the query result that satisfies the condition in the predicate function. </returns>
        /// <exception cref="OverflowException">The number of elements in the query result that satisfy the condition in the predicate function
        ///     is larger than
        ///     <see cref="Int64.MaxValue" />
        ///     .</exception>
        public Task<long> LongCountAsync(Func<TElement, bool> predicate)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<long>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).LongCountAsync(predicate);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the number of elements in the result that satisfy a condition.
        /// </summary>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing the number of elements in the query result that satisfies the condition in the predicate function. </returns>
        /// <exception cref="OverflowException">The number of elements in the query result that satisfy the condition in the predicate function
        ///     is larger than
        ///     <see cref="Int64.MaxValue" />
        ///     .</exception>
        public Task<long> LongCountAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<long>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).LongCountAsync(predicate, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the minimum value of the result.
        /// </summary>
        /// <returns> A <see cref="Task" /> containing the minimum value in the query result. </returns>
        public Task<TElement> MinAsync()
        {
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).MinAsync();
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the minimum value of the result.
        /// </summary>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing the minimum value in the query result. </returns>
        public Task<TElement> MinAsync(CancellationToken cancellationToken)
        {
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).MinAsync(cancellationToken);
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the maximum value of the result.
        /// </summary>
        /// <returns> A <see cref="Task" /> containing the minimum value in the query result. </returns>
        public Task<TElement> MaxAsync()
        {
            Contract.Ensures(Contract.Result<Task<TElement>>() != null);

            return ((IDbAsyncEnumerable<TElement>)this).MaxAsync();
        }

        /// <summary>
        ///     Asynchronously executes the query and returns the maximum value of the result.
        /// </summary>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task" /> containing the minimum value in the query result. </returns>
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
