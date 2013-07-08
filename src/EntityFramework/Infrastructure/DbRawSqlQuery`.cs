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

        #region AsStreaming

        /// <summary>
        ///     Returns a new query that will stream the results instead of buffering.
        /// </summary>
        /// <returns> A new query with AsStreaming applied. </returns>
        public virtual DbRawSqlQuery<TElement> AsStreaming()
        {
            return _internalQuery == null ? this : new DbRawSqlQuery<TElement>(_internalQuery.AsStreaming());
        }

        #endregion

        #region IEnumerable implementation

        /// <summary>
        ///     Returns an <see cref="IEnumerator{TEntity}" /> which when enumerated will execute the SQL query against the database.
        /// </summary>
        /// <returns>
        ///     An <see cref="IEnumerator{TEntity}" /> object that can be used to iterate through the elements.
        /// </returns>
        public virtual IEnumerator<TElement> GetEnumerator()
        {
            return (IEnumerator<TElement>)GetInternalQueryWithCheck("GetEnumerator").GetEnumerator();
        }

        /// <summary>
        ///     Returns an <see cref="IEnumerator" /> which when enumerated will execute the SQL query against the database.
        /// </summary>
        /// <returns>
        ///     An <see cref="IEnumerator" /> object that can be used to iterate through the elements.
        /// </returns>
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
        /// <returns>
        ///     An <see cref="IDbAsyncEnumerable{T}" /> object that can be used to iterate through the elements.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IDbAsyncEnumerator<TElement> IDbAsyncEnumerable<TElement>.GetAsyncEnumerator()
        {
            return (IDbAsyncEnumerator<TElement>)GetInternalQueryWithCheck("IDbAsyncEnumerable<TElement>.GetAsyncEnumerator").GetAsyncEnumerator();
        }

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

        /// <summary>
        ///     Asynchronously enumerates the query results and performs the specified action on each element.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="action"> The action to be executed. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public Task ForEachAsync(Action<TElement> action)
        {
            Check.NotNull(action, "action");

            return ((IDbAsyncEnumerable<TElement>)this).ForEachAsync(action, CancellationToken.None);
        }

        /// <summary>
        ///     Asynchronously enumerates the query results and performs the specified action on each element.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="action"> The action to be executed. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public Task ForEachAsync(Action<TElement> action, CancellationToken cancellationToken)
        {
            Check.NotNull(action, "action");

            return ((IDbAsyncEnumerable<TElement>)this).ForEachAsync(action, cancellationToken);
        }

        /// <summary>
        ///     Creates a <see cref="List{T}" /> from the query by enumerating it asynchronously.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="List{T}" /> that contains elements from the input sequence.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<List<TElement>> ToListAsync()
        {
            return ((IDbAsyncEnumerable<TElement>)this).ToListAsync();
        }

        /// <summary>
        ///     Creates a <see cref="List{T}" /> from the query by enumerating it asynchronously.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="List{T}" /> that contains elements from the input sequence.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<List<TElement>> ToListAsync(CancellationToken cancellationToken)
        {
            return ((IDbAsyncEnumerable<TElement>)this).ToListAsync(cancellationToken);
        }

        /// <summary>
        ///     Creates an array from the query by enumerating it asynchronously.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains an array that contains elements from the input sequence.
        /// </returns>
        public Task<TElement[]> ToArrayAsync()
        {
            return ((IDbAsyncEnumerable<TElement>)this).ToArrayAsync();
        }

        /// <summary>
        ///     Creates an array from the query by enumerating it asynchronously.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains an array that contains elements from the input sequence.
        /// </returns>
        public Task<TElement[]> ToArrayAsync(CancellationToken cancellationToken)
        {
            return ((IDbAsyncEnumerable<TElement>)this).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from the query by enumerating it asynchronously
        ///     according to a specified key selector function.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TKey">
        ///     The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey>(Func<TElement, TKey> keySelector)
        {
            Check.NotNull(keySelector, "keySelector");

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector);
        }

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from the query by enumerating it asynchronously
        ///     according to a specified key selector function.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TKey">
        ///     The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey>(
            Func<TElement, TKey> keySelector, CancellationToken cancellationToken)
        {
            Check.NotNull(keySelector, "keySelector");

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector, cancellationToken);
        }

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from the query by enumerating it asynchronously
        ///     according to a specified key selector function and a comparer.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TKey">
        ///     The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="comparer">
        ///     An <see cref="IEqualityComparer{TKey}" /> to compare keys.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey>(Func<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            Check.NotNull(keySelector, "keySelector");

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector, comparer);
        }

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from the query by enumerating it asynchronously
        ///     according to a specified key selector function and a comparer.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TKey">
        ///     The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="comparer">
        ///     An <see cref="IEqualityComparer{TKey}" /> to compare keys.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey>(
            Func<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
        {
            Check.NotNull(keySelector, "keySelector");

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector, comparer, cancellationToken);
        }

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from the query by enumerating it asynchronously
        ///     according to a specified key selector and an element selector function.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TKey">
        ///     The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <typeparam name="TResult">
        ///     The type of the value returned by <paramref name="elementSelector" />.
        /// </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="Dictionary{TKey, TResult}" /> that contains values of type
        ///     <typeparamref name="TResult" /> selected from the query.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TResult>> ToDictionaryAsync<TKey, TResult>(
            Func<TElement, TKey> keySelector, Func<TElement, TResult> elementSelector)
        {
            Check.NotNull(keySelector, "keySelector");
            Check.NotNull(elementSelector, "elementSelector");

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector, elementSelector);
        }

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from the query by enumerating it asynchronously
        ///     according to a specified key selector and an element selector function.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TKey">
        ///     The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <typeparam name="TResult">
        ///     The type of the value returned by <paramref name="elementSelector" />.
        /// </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="Dictionary{TKey, TResult}" /> that contains values of type
        ///     <typeparamref name="TResult" /> selected from the query.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TResult>> ToDictionaryAsync<TKey, TResult>(
            Func<TElement, TKey> keySelector, Func<TElement, TResult> elementSelector, CancellationToken cancellationToken)
        {
            Check.NotNull(keySelector, "keySelector");
            Check.NotNull(elementSelector, "elementSelector");

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector, elementSelector, cancellationToken);
        }

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from the query by enumerating it asynchronously
        ///     according to a specified key selector function, a comparer, and an element selector function.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TKey">
        ///     The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <typeparam name="TResult">
        ///     The type of the value returned by <paramref name="elementSelector" />.
        /// </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <param name="comparer">
        ///     An <see cref="IEqualityComparer{TKey}" /> to compare keys.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="Dictionary{TKey, TResult}" /> that contains values of type
        ///     <typeparamref name="TResult" /> selected from the input sequence.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TResult>> ToDictionaryAsync<TKey, TResult>(
            Func<TElement, TKey> keySelector, Func<TElement, TResult> elementSelector, IEqualityComparer<TKey> comparer)
        {
            Check.NotNull(keySelector, "keySelector");
            Check.NotNull(elementSelector, "elementSelector");

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector, elementSelector, comparer);
        }

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from the query by enumerating it asynchronously
        ///     according to a specified key selector function, a comparer, and an element selector function.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TKey">
        ///     The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <typeparam name="TResult">
        ///     The type of the value returned by <paramref name="elementSelector" />.
        /// </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <param name="comparer">
        ///     An <see cref="IEqualityComparer{TKey}" /> to compare keys.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="Dictionary{TKey, TResult}" /> that contains values of type
        ///     <typeparamref name="TResult" /> selected from the input sequence.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<Dictionary<TKey, TResult>> ToDictionaryAsync<TKey, TResult>(
            Func<TElement, TKey> keySelector, Func<TElement, TResult> elementSelector, IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken)
        {
            Check.NotNull(keySelector, "keySelector");
            Check.NotNull(elementSelector, "elementSelector");

            return ((IDbAsyncEnumerable<TElement>)this).ToDictionaryAsync(keySelector, elementSelector, comparer, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously returns the first element of the query.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the first element in the query result.
        /// </returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public Task<TElement> FirstAsync()
        {
            return ((IDbAsyncEnumerable<TElement>)this).FirstAsync();
        }

        /// <summary>
        ///     Asynchronously returns the first element of the query.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the first element in the query result.
        /// </returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public Task<TElement> FirstAsync(CancellationToken cancellationToken)
        {
            return ((IDbAsyncEnumerable<TElement>)this).FirstAsync(cancellationToken);
        }

        /// <summary>
        ///     Asynchronously returns the first element of the query that satisfies a specified condition.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the first element in the query result that satisfies a specified condition.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="predicate" />
        ///     is
        ///     <c>null</c>
        ///     .
        /// </exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public Task<TElement> FirstAsync(Func<TElement, bool> predicate)
        {
            Check.NotNull(predicate, "predicate");

            return ((IDbAsyncEnumerable<TElement>)this).FirstAsync(predicate);
        }

        /// <summary>
        ///     Asynchronously returns the first element of the query that satisfies a specified condition.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the first element in the query result that satisfies a specified condition.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="predicate" />
        ///     is
        ///     <c>null</c>
        ///     .
        /// </exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public Task<TElement> FirstAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Check.NotNull(predicate, "predicate");

            return ((IDbAsyncEnumerable<TElement>)this).FirstAsync(predicate, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously returns the first element of the query, or a default value if the the query result contains no elements.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains <c>default</c> ( <typeparamref name="TElement" /> ) if query result is empty;
        ///     otherwise, the first element in the query result.
        /// </returns>
        public Task<TElement> FirstOrDefaultAsync()
        {
            return ((IDbAsyncEnumerable<TElement>)this).FirstOrDefaultAsync();
        }

        /// <summary>
        ///     Asynchronously returns the first element of the query, or a default value if the the query result contains no elements.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains <c>default</c> ( <typeparamref name="TElement" /> ) if query result is empty;
        ///     otherwise, the first element in the query result.
        /// </returns>
        public Task<TElement> FirstOrDefaultAsync(CancellationToken cancellationToken)
        {
            return ((IDbAsyncEnumerable<TElement>)this).FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        ///     Asynchronously returns the first element of the query that satisfies a specified condition
        ///     or a default value if no such element is found.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains <c>default</c> ( <typeparamref name="TElement" /> ) if query result is empty
        ///     or if no element passes the test specified by <paramref name="predicate" />; otherwise, the first element
        ///     in the query result that passes the test specified by <paramref name="predicate" /> .
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="predicate" />
        ///     is
        ///     <c>null</c>
        ///     .
        /// </exception>
        public Task<TElement> FirstOrDefaultAsync(Func<TElement, bool> predicate)
        {
            Check.NotNull(predicate, "predicate");

            return ((IDbAsyncEnumerable<TElement>)this).FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        ///     Asynchronously returns the first element of the query that satisfies a specified condition
        ///     or a default value if no such element is found.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains <c>default</c> ( <typeparamref name="TElement" /> ) if query result is empty
        ///     or if no element passes the test specified by <paramref name="predicate" />; otherwise, the first element
        ///     in the query result that passes the test specified by <paramref name="predicate" /> .
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="predicate" />
        ///     is
        ///     <c>null</c>
        ///     .
        /// </exception>
        public Task<TElement> FirstOrDefaultAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Check.NotNull(predicate, "predicate");

            return ((IDbAsyncEnumerable<TElement>)this).FirstOrDefaultAsync(predicate, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously returns the only element of the query, and throws an exception
        ///     if there is not exactly one element in the sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the single element of the query result.
        /// </returns>
        /// <exception cref="InvalidOperationException">The query result has more than one element.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public Task<TElement> SingleAsync()
        {
            return ((IDbAsyncEnumerable<TElement>)this).SingleAsync();
        }

        /// <summary>
        ///     Asynchronously returns the only element of the query, and throws an exception
        ///     if there is not exactly one element in the sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the single element of the query result.
        /// </returns>
        /// <exception cref="InvalidOperationException">The query result has more than one element.</exception>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public Task<TElement> SingleAsync(CancellationToken cancellationToken)
        {
            return ((IDbAsyncEnumerable<TElement>)this).SingleAsync(cancellationToken);
        }

        /// <summary>
        ///     Asynchronously returns the only element of the query that satisfies a specified condition,
        ///     and throws an exception if more than one such element exists.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the single element of the query result that satisfies the condition in
        ///     <paramref name="predicate" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="predicate" />
        ///     is
        ///     <c>null</c>
        ///     .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     No element satisfies the condition in
        ///     <paramref name="predicate" />
        ///     .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     More than one element satisfies the condition in
        ///     <paramref name="predicate" />
        ///     .
        /// </exception>
        public Task<TElement> SingleAsync(Func<TElement, bool> predicate)
        {
            Check.NotNull(predicate, "predicate");

            return ((IDbAsyncEnumerable<TElement>)this).SingleAsync(predicate);
        }

        /// <summary>
        ///     Asynchronously returns the only element of the query that satisfies a specified condition,
        ///     and throws an exception if more than one such element exists.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the single element of the query result that satisfies the condition in
        ///     <paramref name="predicate" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="predicate" />
        ///     is
        ///     <c>null</c>
        ///     .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     No element satisfies the condition in
        ///     <paramref name="predicate" />
        ///     .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     More than one element satisfies the condition in
        ///     <paramref name="predicate" />
        ///     .
        /// </exception>
        public Task<TElement> SingleAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Check.NotNull(predicate, "predicate");

            return ((IDbAsyncEnumerable<TElement>)this).SingleAsync(predicate, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously returns the only element of a sequence, or a default value if the sequence is empty;
        ///     this method throws an exception if there is more than one element in the sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the single element of the query result, or <c>default</c> (<typeparamref name="TElement" />)
        ///     if the sequence contains no elements.
        /// </returns>
        /// <exception cref="InvalidOperationException">The query result has more than one element.</exception>
        public Task<TElement> SingleOrDefaultAsync()
        {
            return ((IDbAsyncEnumerable<TElement>)this).SingleOrDefaultAsync();
        }

        /// <summary>
        ///     Asynchronously returns the only element of a sequence, or a default value if the sequence is empty;
        ///     this method throws an exception if there is more than one element in the sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the single element of the query result, or <c>default</c> (<typeparamref name="TElement" />)
        ///     if the sequence contains no elements.
        /// </returns>
        /// <exception cref="InvalidOperationException">The query result has more than one element.</exception>
        public Task<TElement> SingleOrDefaultAsync(CancellationToken cancellationToken)
        {
            return ((IDbAsyncEnumerable<TElement>)this).SingleOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        ///     Asynchronously returns the only element of the query that satisfies a specified condition or
        ///     a default value if no such element exists; this method throws an exception if more than one element
        ///     satisfies the condition.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the single element of the query result that satisfies the condition in
        ///     <paramref name="predicate" />, or <c>default</c> ( <typeparamref name="TElement" /> ) if no such element is found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="predicate" />
        ///     is
        ///     <c>null</c>
        ///     .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     More than one element satisfies the condition in
        ///     <paramref name="predicate" />
        ///     .
        /// </exception>
        public Task<TElement> SingleOrDefaultAsync(Func<TElement, bool> predicate)
        {
            Check.NotNull(predicate, "predicate");

            return ((IDbAsyncEnumerable<TElement>)this).SingleOrDefaultAsync(predicate);
        }

        /// <summary>
        ///     Asynchronously returns the only element of the query that satisfies a specified condition or
        ///     a default value if no such element exists; this method throws an exception if more than one element
        ///     satisfies the condition.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the single element of the query result that satisfies the condition in
        ///     <paramref name="predicate" />, or <c>default</c> ( <typeparamref name="TElement" /> ) if no such element is found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="predicate" />
        ///     is
        ///     <c>null</c>
        ///     .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     More than one element satisfies the condition in
        ///     <paramref name="predicate" />
        ///     .
        /// </exception>
        public Task<TElement> SingleOrDefaultAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Check.NotNull(predicate, "predicate");

            return ((IDbAsyncEnumerable<TElement>)this).SingleOrDefaultAsync(predicate, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously determines whether the query contains a specified element by using the default equality comparer.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="value"> The object to locate in the query result. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains <c>true</c> if the query result contains the specified value; otherwise, <c>false</c>.
        /// </returns>
        public Task<bool> ContainsAsync(TElement value)
        {
            return ((IDbAsyncEnumerable<TElement>)this).ContainsAsync(value);
        }

        /// <summary>
        ///     Asynchronously determines whether the query contains a specified element by using the default equality comparer.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="value"> The object to locate in the query result. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains <c>true</c> if the query result contains the specified value; otherwise, <c>false</c>.
        /// </returns>
        public Task<bool> ContainsAsync(TElement value, CancellationToken cancellationToken)
        {
            return ((IDbAsyncEnumerable<TElement>)this).ContainsAsync(value, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously determines whether the query contains any elements.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains <c>true</c> if the query result contains any elements; otherwise, <c>false</c>.
        /// </returns>
        public Task<bool> AnyAsync()
        {
            return ((IDbAsyncEnumerable<TElement>)this).AnyAsync();
        }

        /// <summary>
        ///     Asynchronously determines whether the query contains any elements.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains <c>true</c> if the query result contains any elements; otherwise, <c>false</c>.
        /// </returns>
        public Task<bool> AnyAsync(CancellationToken cancellationToken)
        {
            return ((IDbAsyncEnumerable<TElement>)this).AnyAsync(cancellationToken);
        }

        /// <summary>
        ///     Asynchronously determines whether any element of the query satisfies a condition.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains <c>true</c> if any elements in the query result pass the test in the specified predicate; otherwise, <c>false</c>.
        /// </returns>
        public Task<bool> AnyAsync(Func<TElement, bool> predicate)
        {
            Check.NotNull(predicate, "predicate");

            return ((IDbAsyncEnumerable<TElement>)this).AnyAsync(predicate);
        }

        /// <summary>
        ///     Asynchronously determines whether any element of the query satisfies a condition.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains <c>true</c> if any elements in the query result pass the test in the specified predicate; otherwise, <c>false</c>.
        /// </returns>
        public Task<bool> AnyAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Check.NotNull(predicate, "predicate");

            return ((IDbAsyncEnumerable<TElement>)this).AnyAsync(predicate, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously determines whether all the elements of the query satisfy a condition.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains <c>true</c> if every element of the query result passes the test in the specified predicate; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="predicate" />
        ///     is
        ///     <c>null</c>
        ///     .
        /// </exception>
        public Task<bool> AllAsync(Func<TElement, bool> predicate)
        {
            Check.NotNull(predicate, "predicate");

            return ((IDbAsyncEnumerable<TElement>)this).AllAsync(predicate);
        }

        /// <summary>
        ///     Asynchronously determines whether all the elements of the query satisfy a condition.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains <c>true</c> if every element of the query result passes the test in the specified predicate; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="predicate" />
        ///     is
        ///     <c>null</c>
        ///     .
        /// </exception>
        public Task<bool> AllAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Check.NotNull(predicate, "predicate");

            return ((IDbAsyncEnumerable<TElement>)this).AllAsync(predicate, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously returns the number of elements in the query.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the number of elements in the query result.
        /// </returns>
        /// <exception cref="OverflowException">
        ///     The number of elements in the query result is larger than
        ///     <see cref="Int32.MaxValue" />
        ///     .
        /// </exception>
        public Task<int> CountAsync()
        {
            return ((IDbAsyncEnumerable<TElement>)this).CountAsync();
        }

        /// <summary>
        ///     Asynchronously returns the number of elements in the query.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the number of elements in the query result.
        /// </returns>
        /// <exception cref="OverflowException">
        ///     The number of elements in the query result is larger than
        ///     <see cref="Int32.MaxValue" />
        ///     .
        /// </exception>
        public Task<int> CountAsync(CancellationToken cancellationToken)
        {
            return ((IDbAsyncEnumerable<TElement>)this).CountAsync(cancellationToken);
        }

        /// <summary>
        ///     Asynchronously returns the number of elements in the query that satisfy a condition.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the number of elements in the query result that satisfy the condition in the predicate function.
        /// </returns>
        /// <exception cref="OverflowException">
        ///     The number of elements in the query result that satisfy the condition in the predicate function
        ///     is larger than
        ///     <see cref="Int32.MaxValue" />
        ///     .
        /// </exception>
        public Task<int> CountAsync(Func<TElement, bool> predicate)
        {
            Check.NotNull(predicate, "predicate");

            return ((IDbAsyncEnumerable<TElement>)this).CountAsync(predicate);
        }

        /// <summary>
        ///     Asynchronously returns the number of elements in the query that satisfy a condition.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the number of elements in the query result that satisfy the condition in the predicate function.
        /// </returns>
        /// <exception cref="OverflowException">
        ///     The number of elements in the query result that satisfy the condition in the predicate function
        ///     is larger than
        ///     <see cref="Int32.MaxValue" />
        ///     .
        /// </exception>
        public Task<int> CountAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Check.NotNull(predicate, "predicate");

            return ((IDbAsyncEnumerable<TElement>)this).CountAsync(predicate, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously returns an <see cref="Int64" /> that represents the total number of elements in the query.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the number of elements in the query result.
        /// </returns>
        /// <exception cref="OverflowException">
        ///     The number of elements in the query result is larger than
        ///     <see cref="Int64.MaxValue" />
        ///     .
        /// </exception>
        public Task<long> LongCountAsync()
        {
            return ((IDbAsyncEnumerable<TElement>)this).LongCountAsync();
        }

        /// <summary>
        ///     Asynchronously returns an <see cref="Int64" /> that represents the total number of elements in the query.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the number of elements in the query result.
        /// </returns>
        /// <exception cref="OverflowException">
        ///     The number of elements in the query result is larger than
        ///     <see cref="Int64.MaxValue" />
        ///     .
        /// </exception>
        public Task<long> LongCountAsync(CancellationToken cancellationToken)
        {
            return ((IDbAsyncEnumerable<TElement>)this).LongCountAsync(cancellationToken);
        }

        /// <summary>
        ///     Asynchronously returns an <see cref="Int64" /> that represents the number of elements in the query
        ///     that satisfy a condition.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the number of elements in the query result that satisfy the condition in the predicate function.
        /// </returns>
        /// <exception cref="OverflowException">
        ///     The number of elements in the query result that satisfy the condition in the predicate function
        ///     is larger than
        ///     <see cref="Int64.MaxValue" />
        ///     .
        /// </exception>
        public Task<long> LongCountAsync(Func<TElement, bool> predicate)
        {
            Check.NotNull(predicate, "predicate");

            return ((IDbAsyncEnumerable<TElement>)this).LongCountAsync(predicate);
        }

        /// <summary>
        ///     Asynchronously returns an <see cref="Int64" /> that represents the number of elements in the query
        ///     that satisfy a condition.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the number of elements in the query result that satisfy the condition in the predicate function.
        /// </returns>
        /// <exception cref="OverflowException">
        ///     The number of elements in the query result that satisfy the condition in the predicate function
        ///     is larger than
        ///     <see cref="Int64.MaxValue" />
        ///     .
        /// </exception>
        public Task<long> LongCountAsync(Func<TElement, bool> predicate, CancellationToken cancellationToken)
        {
            Check.NotNull(predicate, "predicate");

            return ((IDbAsyncEnumerable<TElement>)this).LongCountAsync(predicate, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously returns the minimum value of the query.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the minimum value in the query result.
        /// </returns>
        public Task<TElement> MinAsync()
        {
            return ((IDbAsyncEnumerable<TElement>)this).MinAsync();
        }

        /// <summary>
        ///     Asynchronously returns the minimum value of the query.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the minimum value in the query result.
        /// </returns>
        public Task<TElement> MinAsync(CancellationToken cancellationToken)
        {
            return ((IDbAsyncEnumerable<TElement>)this).MinAsync(cancellationToken);
        }

        /// <summary>
        ///     Asynchronously returns the maximum value of the query.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the maximum value in the query result.
        /// </returns>
        public Task<TElement> MaxAsync()
        {
            return ((IDbAsyncEnumerable<TElement>)this).MaxAsync();
        }

        /// <summary>
        ///     Asynchronously returns the maximum value of the query.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the maximum value in the query result.
        /// </returns>
        public Task<TElement> MaxAsync(CancellationToken cancellationToken)
        {
            return ((IDbAsyncEnumerable<TElement>)this).MaxAsync(cancellationToken);
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
            return _internalQuery == null ? base.ToString() : _internalQuery.ToString();
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

        private InternalSqlQuery GetInternalQueryWithCheck(string memberName)
        {
            if (_internalQuery == null)
            {
                throw new NotImplementedException(Strings.TestDoubleNotImplemented(memberName, GetType().Name, typeof(DbSqlQuery<>).Name));
            }

            return _internalQuery;
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
            get { return false; }
        }

        /// <summary>
        ///     Throws an exception indicating that binding directly to a store query is not supported.
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
