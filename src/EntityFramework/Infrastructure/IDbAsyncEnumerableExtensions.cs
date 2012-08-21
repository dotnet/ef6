// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    // The methods in this class are internal so they don't conflict with the extension methods for IQueryable
    internal static class IDbAsyncEnumerableExtensions
    {
        /// <summary>
        ///     Executes the provided action on each element of the <see cref="IDbAsyncEnumerable" />.
        /// </summary>
        /// <param name="action"> The action to be executed. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A Task representing the asynchronous operation. </returns>
        internal static async Task ForEachAsync(
            this IDbAsyncEnumerable source, Action<object> action, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Requires(action != null);
            //Contract.Ensures(Contract.Result<Task>() != null);

            using (var enumerator = source.GetAsyncEnumerator())
            {
                if (await enumerator.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    Task<bool> moveNextTask;
                    do
                    {
                        var current = enumerator.Current;
                        moveNextTask = enumerator.MoveNextAsync(cancellationToken);
                        action(current);
                    }
                    while (await moveNextTask.ConfigureAwait(continueOnCapturedContext: false));
                }
            }
        }

        /// <summary>
        ///     Executes the provided action on each element of the <see cref="IDbAsyncEnumerable{T}" />.
        /// </summary>
        /// <param name="action"> The action to be executed. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A Task representing the asynchronous operation. </returns>
        internal static async Task ForEachAsync<T>(
            this IDbAsyncEnumerable<T> source, Action<T> action, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Requires(action != null);
            //Contract.Ensures(Contract.Result<Task>() != null);

            using (var enumerator = source.GetAsyncEnumerator())
            {
                if (await enumerator.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    Task<bool> moveNextTask;
                    do
                    {
                        var current = enumerator.Current;
                        moveNextTask = enumerator.MoveNextAsync(cancellationToken);
                        action(current);
                    }
                    while (await moveNextTask.ConfigureAwait(continueOnCapturedContext: false));
                }
            }
        }

        /// <summary>
        ///     Creates a <see cref="List{T}" /> from the <see cref="IDbAsyncEnumerable" />.
        /// </summary>
        /// <typeparam name="T"> The type that the elements will be cast to. </typeparam>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A Task containing a <see cref="List{T}" /> that contains elements from the input sequence. </returns>
        internal static Task<List<T>> ToListAsync<T>(this IDbAsyncEnumerable source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<List<object>>>() != null);

            return source.ToListAsync<T>(CancellationToken.None);
        }

        /// <summary>
        ///     Creates a <see cref="List{T}" /> from the <see cref="IDbAsyncEnumerable" />.
        /// </summary>
        /// <typeparam name="T">The type that the elements will be cast to.</typeparam>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A Task containing a <see cref="List{T}" /> that contains elements from the input sequence. </returns>
        internal static async Task<List<T>> ToListAsync<T>(this IDbAsyncEnumerable source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<List<object>>>() != null);

            var list = new List<T>();
            await source.ForEachAsync(e => list.Add((T)e), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            return list;
        }

        /// <summary>
        ///     Creates a <see cref="List{T}" /> from the <see cref="IDbAsyncEnumerable{T}" />.
        /// </summary>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A Task containing a <see cref="List{T}" /> that contains elements from the input sequence. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Task<List<T>> ToListAsync<T>(this IDbAsyncEnumerable<T> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<List<T>>>() != null);

            return source.ToListAsync(CancellationToken.None);
        }

        /// <summary>
        ///     Creates a <see cref="List{T}" /> from the <see cref="IDbAsyncEnumerable{T}" />.
        /// </summary>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A Task containing a <see cref="List{T}" /> that contains elements from the input sequence. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static async Task<List<T>> ToListAsync<T>(this IDbAsyncEnumerable<T> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<List<T>>>() != null);

            var list = new List<T>();
            await source.ForEachAsync(list.Add, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            return list;
        }

        /// <summary>
        ///     Creates a T[] from an <see cref="IDbAsyncEnumerable{T}" /> by enumerating it asynchronously.
        ///     If the underlying type doesn't support asynchronous enumeration it will be enumerated synchronously.
        /// </summary>
        /// <typeparam name="T"> The type of the elements of <paramref name="source" /> . </typeparam>
        /// <returns> A Task containing a T[] that contains elements from the input sequence. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Task<T[]> ToArrayAsync<T>(this IDbAsyncEnumerable<T> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<T[]>>() != null);

            return source.ToArrayAsync(CancellationToken.None);
        }

        /// <summary>
        ///     Creates a T[] from an <see cref="IDbAsyncEnumerable{T}" /> by enumerating it asynchronously.
        ///     If the underlying type doesn't support asynchronous enumeration it will be enumerated synchronously.
        /// </summary>
        /// <typeparam name="T"> The type of the elements of <paramref name="source" /> . </typeparam>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A Task containing a T[] that contains elements from the input sequence. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static async Task<T[]> ToArrayAsync<T>(this IDbAsyncEnumerable<T> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<T[]>>() != null);

            var list = await source.ToListAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            return list.ToArray();
        }

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IDbAsyncEnumerable{TSource}" /> by enumerating it asynchronously
        ///     according to a specified key selector function.
        /// </summary>
        /// <typeparam name="TSource"> The type of the elements of <paramref name="source" /> . </typeparam>
        /// <typeparam name="TKey"> The type of the key returned by <paramref name="keySelector" /> . </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <returns> A Task containing a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            Contract.Requires(source != null);
            Contract.Requires(keySelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TSource>>>() != null);

            return ToDictionaryAsync(source, keySelector, IdentityFunction<TSource>.Instance, null, CancellationToken.None);
        }

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IDbAsyncEnumerable{TSource}" /> by enumerating it asynchronously
        ///     according to a specified key selector function.
        /// </summary>
        /// <typeparam name="TSource"> The type of the elements of <paramref name="source" /> . </typeparam>
        /// <typeparam name="TKey"> The type of the key returned by <paramref name="keySelector" /> . </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A Task containing a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(keySelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TSource>>>() != null);

            return ToDictionaryAsync(source, keySelector, IdentityFunction<TSource>.Instance, null, cancellationToken);
        }

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IDbAsyncEnumerable{TSource}" /> by enumerating it asynchronously
        ///     according to a specified key selector function and a comparer.
        /// </summary>
        /// <typeparam name="TSource"> The type of the elements of <paramref name="source" /> . </typeparam>
        /// <typeparam name="TKey"> The type of the key returned by <paramref name="keySelector" /> . </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="comparer"> An <see cref="IEqualityComparer{TKey}" /> to compare keys. </param>
        /// <returns> A Task containing a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            Contract.Requires(source != null);
            Contract.Requires(keySelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TSource>>>() != null);

            return ToDictionaryAsync(source, keySelector, IdentityFunction<TSource>.Instance, comparer, CancellationToken.None);
        }

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IDbAsyncEnumerable{TSource}" /> by enumerating it asynchronously
        ///     according to a specified key selector function and a comparer.
        /// </summary>
        /// <typeparam name="TSource"> The type of the elements of <paramref name="source" /> . </typeparam>
        /// <typeparam name="TKey"> The type of the key returned by <paramref name="keySelector" /> . </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="comparer"> An <see cref="IEqualityComparer{TKey}" /> to compare keys. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A Task containing a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(keySelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TSource>>>() != null);

            return ToDictionaryAsync(source, keySelector, IdentityFunction<TSource>.Instance, comparer, cancellationToken);
        }

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IDbAsyncEnumerable{TSource}" /> by enumerating it asynchronously
        ///     according to a specified key selector and an element selector function.
        /// </summary>
        /// <typeparam name="TSource"> The type of the elements of <paramref name="source" /> . </typeparam>
        /// <typeparam name="TKey"> The type of the key returned by <paramref name="keySelector" /> . </typeparam>
        /// <typeparam name="TElement"> The type of the value returned by <paramref name="elementSelector" /> . </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <param name="comparer"> An <see cref="IEqualityComparer{TKey}" /> to compare keys. </param>
        /// <returns> A Task containing a <see cref="Dictionary{TKey, TElement}" /> that contains values of type <typeparamref
        ///      name="TElement" /> selected from the input sequence. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            Contract.Requires(source != null);
            Contract.Requires(keySelector != null);
            Contract.Requires(elementSelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ToDictionaryAsync(source, keySelector, elementSelector, null, CancellationToken.None);
        }

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IDbAsyncEnumerable{TSource}" /> by enumerating it asynchronously
        ///     according to a specified key selector and an element selector function.
        /// </summary>
        /// <typeparam name="TSource"> The type of the elements of <paramref name="source" /> . </typeparam>
        /// <typeparam name="TKey"> The type of the key returned by <paramref name="keySelector" /> . </typeparam>
        /// <typeparam name="TElement"> The type of the value returned by <paramref name="elementSelector" /> . </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A Task containing a <see cref="Dictionary{TKey, TElement}" /> that contains values of type <typeparamref
        ///      name="TElement" /> selected from the input sequence. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector,
            CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(keySelector != null);
            Contract.Requires(elementSelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ToDictionaryAsync(source, keySelector, elementSelector, null, cancellationToken);
        }

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IDbAsyncEnumerable{TSource}" /> by enumerating it asynchronously
        ///     according to a specified key selector function, a comparer, and an element selector function.
        /// </summary>
        /// <typeparam name="TSource"> The type of the elements of <paramref name="source" /> . </typeparam>
        /// <typeparam name="TKey"> The type of the key returned by <paramref name="keySelector" /> . </typeparam>
        /// <typeparam name="TElement"> The type of the value returned by <paramref name="elementSelector" /> . </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <param name="comparer"> An <see cref="IEqualityComparer{TKey}" /> to compare keys. </param>
        /// <returns> A Task containing a <see cref="Dictionary{TKey, TElement}" /> that contains values of type <typeparamref
        ///      name="TElement" /> selected from the input sequence. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer)
        {
            Contract.Requires(source != null);
            Contract.Requires(keySelector != null);
            Contract.Requires(elementSelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ToDictionaryAsync(source, keySelector, elementSelector, comparer, CancellationToken.None);
        }

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IDbAsyncEnumerable{TSource}" /> by enumerating it asynchronously
        ///     according to a specified key selector function, a comparer, and an element selector function.
        /// </summary>
        /// <typeparam name="TSource"> The type of the elements of <paramref name="source" /> . </typeparam>
        /// <typeparam name="TKey"> The type of the key returned by <paramref name="keySelector" /> . </typeparam>
        /// <typeparam name="TElement"> The type of the value returned by <paramref name="elementSelector" /> . </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <param name="comparer"> An <see cref="IEqualityComparer{TKey}" /> to compare keys. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A Task containing a <see cref="Dictionary{TKey, TElement}" /> that contains values of type <typeparamref
        ///      name="TElement" /> selected from the input sequence. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static async Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Requires(keySelector != null);
            //Contract.Requires(elementSelector != null);
            //Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            var d = new Dictionary<TKey, TElement>(comparer);
            await source.ForEachAsync(element => d.Add(keySelector(element), elementSelector(element)), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            return d;
        }

        internal static IDbAsyncEnumerable<TResult> Cast<TResult>(this IDbAsyncEnumerable source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<IDbAsyncEnumerable<TResult>>() != null);

            return new CastDbAsyncEnumerable<TResult>(source);
        }

        internal static Task<TSource> FirstAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.FirstAsync(CancellationToken.None);
        }

        internal static Task<TSource> FirstAsync<TSource>(this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.FirstAsync(predicate, CancellationToken.None);
        }

        internal static async Task<TSource> FirstAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            using (var e = source.GetAsyncEnumerator())
            {
                if (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    return e.Current;
                }
            }

            throw Error.EmptySequence();
        }

        internal static async Task<TSource> FirstAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Requires(predicate != null);
            //Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            using (var e = source.GetAsyncEnumerator())
            {
                if (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    if (predicate(e.Current)) return e.Current;
                }
            }

            throw Error.NoMatch();
        }

        internal static Task<TSource> FirstOrDefaultAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
        {
            Contract.Requires(source != null);

            return source.FirstOrDefaultAsync(CancellationToken.None);
        }

        internal static Task<TSource> FirstOrDefaultAsync<TSource>(this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Contract.Requires(source != null);

            return source.FirstOrDefaultAsync(predicate, CancellationToken.None);
        }

        internal static async Task<TSource> FirstOrDefaultAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);

            using (var e = source.GetAsyncEnumerator())
            {
                if (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    return e.Current;
                }
            }

            return default(TSource);
        }

        internal static async Task<TSource> FirstOrDefaultAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);

            using (var e = source.GetAsyncEnumerator())
            {
                if (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    if (predicate(e.Current)) return e.Current;
                }
            }

            return default(TSource);
        }

        internal static Task<TSource> SingleAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
        {
            Contract.Requires(source != null);

            return source.SingleAsync(CancellationToken.None);
        }

        internal static async Task<TSource> SingleAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);

            using (var e = source.GetAsyncEnumerator())
            {
                if (!await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    throw Error.EmptySequence();
                }
                var result = e.Current;
                if (!await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    return result;
                }
            }

            throw Error.MoreThanOneElement();
        }

        internal static Task<TSource> SingleAsync<TSource>(this IDbAsyncEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.SingleAsync(predicate, CancellationToken.None);
        }

        internal static async Task<TSource> SingleAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Requires(predicate != null);
            //Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var result = default(TSource);
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    if (predicate(e.Current))
                    {
                        result = e.Current;
                        checked { count++; }
                    }
                }
            }

            switch (count)
            {
                case 0: throw Error.NoMatch();
                case 1: return result;
            }

            throw Error.MoreThanOneMatch();
        }

        internal static Task<TSource> SingleOrDefaultAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.SingleOrDefaultAsync(CancellationToken.None);
        }

        internal static async Task<TSource> SingleOrDefaultAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            using (var e = source.GetAsyncEnumerator())
            {
                if (!await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    return default(TSource);
                }
                var result = e.Current;
                if (!await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    return result;
                }
            }

            throw Error.MoreThanOneElement();
        }

        internal static Task<TSource> SingleOrDefaultAsync<TSource>(this IDbAsyncEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.SingleOrDefaultAsync(predicate, CancellationToken.None);
        }

        internal static async Task<TSource> SingleOrDefaultAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Requires(predicate != null);
            //Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var result = default(TSource);
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    if (predicate(e.Current))
                    {
                        result = e.Current;
                        checked { count++; }
                    }
                }
            }

            if (count < 2)
            {
                return result;
            }

            throw Error.MoreThanOneMatch();
        }

        internal static Task<bool> ContainsAsync<TSource>(this IDbAsyncEnumerable<TSource> source, TSource value)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return source.ContainsAsync(value, CancellationToken.None);
        }

        internal static async Task<bool> ContainsAsync<TSource>(this IDbAsyncEnumerable<TSource> source, TSource value, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<bool>>() != null);

            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    if (EqualityComparer<TSource>.Default.Equals(e.Current, value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal static Task<bool> AnyAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return source.AnyAsync(CancellationToken.None);
        }

        internal static async Task<bool> AnyAsync<TSource>(this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<bool>>() != null);

            using (var e = source.GetAsyncEnumerator())
            {
                if (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)) return true;
            }

            return false;
        }

        internal static Task<bool> AnyAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return source.AnyAsync(predicate, CancellationToken.None);
        }

        internal static async Task<bool> AnyAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Requires(predicate != null);
            //Contract.Ensures(Contract.Result<Task<bool>>() != null);

            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    if (predicate(e.Current))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal static Task<bool> AllAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return source.AllAsync(predicate, CancellationToken.None);
        }

        internal static async Task<bool> AllAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Requires(predicate != null);
            //Contract.Ensures(Contract.Result<Task<bool>>() != null);

            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    if (!predicate(e.Current))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        internal static Task<int> CountAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<int>>() != null);

            return source.CountAsync(CancellationToken.None);
        }

        internal static async Task<int> CountAsync<TSource>(this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<int>>() != null);

            int count = 0;

            using (var e = source.GetAsyncEnumerator())
            {
                checked
                {
                    while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        internal static Task<int> CountAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<int>>() != null);

            return source.CountAsync(predicate, CancellationToken.None);
        }


        internal static async Task<int> CountAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Requires(predicate != null);
            //Contract.Ensures(Contract.Result<Task<int>>() != null);

            int count = 0;

            using (var e = source.GetAsyncEnumerator())
            {
                checked
                {
                    while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        if (predicate(e.Current))
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }

        internal static Task<long> LongCountAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<long>>() != null);

            return source.LongCountAsync(CancellationToken.None);
        }

        internal static async Task<long> LongCountAsync<TSource>(this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<long>>() != null);

            long count = 0;

            using (var e = source.GetAsyncEnumerator())
            {
                checked
                {
                    while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        internal static Task<long> LongCountAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<long>>() != null);

            return source.LongCountAsync(predicate, CancellationToken.None);
        }

        internal static async Task<long> LongCountAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Requires(predicate != null);
            //Contract.Ensures(Contract.Result<Task<long>>() != null);

            long count = 0;

            using (var e = source.GetAsyncEnumerator())
            {
                checked
                {
                    while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        if (predicate(e.Current))
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }

        internal static Task<TSource> MinAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.MinAsync(CancellationToken.None);
        }

        internal static async Task<TSource> MinAsync<TSource>(this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            Comparer<TSource> comparer = Comparer<TSource>.Default;
            TSource value = default(TSource);
            if (value == null)
            {
                using (var e = source.GetAsyncEnumerator())
                {
                    while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        if (e.Current != null && (value == null || comparer.Compare(e.Current, value) < 0))
                        {
                            value = e.Current;
                        }
                    }
                }

                return value;
            }
            else
            {
                bool hasValue = false;

                using (var e = source.GetAsyncEnumerator())
                {
                    while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        if (hasValue)
                        {
                            if (comparer.Compare(e.Current, value) < 0)
                            {
                                value = e.Current;
                            }
                        }
                        else
                        {
                            value = e.Current;
                            hasValue = true;
                        }
                    }
                }

                if (hasValue) return value;
                throw Error.EmptySequence();
            }
        }

        internal static Task<TSource> MaxAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.MaxAsync(CancellationToken.None);
        }

        internal static async Task<TSource> MaxAsync<TSource>(this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            Comparer<TSource> comparer = Comparer<TSource>.Default;
            TSource value = default(TSource);
            if (value == null)
            {
                using (var e = source.GetAsyncEnumerator())
                {
                    while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        if (e.Current != null && (value == null || comparer.Compare(e.Current, value) > 0))
                        {
                            value = e.Current;
                        }
                    }
                }

                return value;
            }
            else
            {
                bool hasValue = false;

                using (var e = source.GetAsyncEnumerator())
                {
                    while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        if (hasValue)
                        {
                            if (comparer.Compare(e.Current, value) > 0)
                            {
                                value = e.Current;
                            }
                        }
                        else
                        {
                            value = e.Current;
                            hasValue = true;
                        }
                    }
                }

                if (hasValue) return value;
                throw Error.EmptySequence();
            }
        }

        internal static Task<int> SumAsync(this IDbAsyncEnumerable<int> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<int>>() != null);

            return source.SumAsync(CancellationToken.None);
        }

        internal static async Task<int> SumAsync(this IDbAsyncEnumerable<int> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<int>>() != null);

            long sum = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    checked
                    {
                        sum += e.Current;
                    }
                }
            }

            return (int)sum;
        }

        internal static Task<int?> SumAsync(this IDbAsyncEnumerable<int?> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<int?>>() != null);

            return source.SumAsync(CancellationToken.None);
        }

        internal static async Task<int?> SumAsync(this IDbAsyncEnumerable<int?> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<int?>>() != null);

            long sum = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    checked
                    {
                        if (e.Current.HasValue)
                        {
                            sum += e.Current.GetValueOrDefault();
                        }
                    }
                }
            }

            return (int)sum;
        }

        internal static Task<long> SumAsync(this IDbAsyncEnumerable<long> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<long>>() != null);

            return source.SumAsync(CancellationToken.None);
        }

        internal static async Task<long> SumAsync(this IDbAsyncEnumerable<long> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<long>>() != null);

            long sum = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    checked
                    {
                        sum += e.Current;
                    }
                }
            }

            return sum;
        }

        internal static Task<long?> SumAsync(this IDbAsyncEnumerable<long?> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<long?>>() != null);

            return source.SumAsync(CancellationToken.None);
        }

        internal static async Task<long?> SumAsync(this IDbAsyncEnumerable<long?> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<long?>>() != null);

            long sum = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    checked
                    {
                        if (e.Current.HasValue)
                        {
                            sum += e.Current.GetValueOrDefault();
                        }
                    }
                }
            }

            return sum;
        }

        internal static Task<float> SumAsync(this IDbAsyncEnumerable<float> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<float>>() != null);

            return source.SumAsync(CancellationToken.None);
        }

        internal static async Task<float> SumAsync(this IDbAsyncEnumerable<float> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<float>>() != null);

            double sum = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    checked
                    {
                        sum += e.Current;
                    }
                }
            }

            return (float)sum;
        }

        internal static Task<float?> SumAsync(this IDbAsyncEnumerable<float?> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<float>>() != null);

            return source.SumAsync(CancellationToken.None);
        }

        internal static async Task<float?> SumAsync(this IDbAsyncEnumerable<float?> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<float>>() != null);

            double sum = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    checked
                    {
                        if (e.Current.HasValue)
                        {
                            sum += e.Current.GetValueOrDefault();
                        }
                    }
                }
            }

            return (float)sum;
        }

        internal static Task<double> SumAsync(this IDbAsyncEnumerable<double> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);

            return source.SumAsync(CancellationToken.None);
        }

        internal static async Task<double> SumAsync(this IDbAsyncEnumerable<double> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<double>>() != null);

            double sum = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    checked
                    {
                        sum += e.Current;
                    }
                }
            }

            return sum;
        }

        internal static Task<double?> SumAsync(this IDbAsyncEnumerable<double?> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double?>>() != null);

            return source.SumAsync(CancellationToken.None);
        }

        internal static async Task<double?> SumAsync(this IDbAsyncEnumerable<double?> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<double?>>() != null);

            double sum = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    checked
                    {
                        if (e.Current.HasValue)
                        {
                            sum += e.Current.GetValueOrDefault();
                        }
                    }
                }
            }

            return sum;
        }

        internal static Task<decimal> SumAsync(this IDbAsyncEnumerable<decimal> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<decimal>>() != null);

            return source.SumAsync(CancellationToken.None);
        }

        internal static async Task<decimal> SumAsync(this IDbAsyncEnumerable<decimal> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<decimal>>() != null);

            decimal sum = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    checked
                    {
                        sum += e.Current;
                    }
                }
            }

            return sum;
        }

        internal static Task<decimal?> SumAsync(this IDbAsyncEnumerable<decimal?> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<decimal?>>() != null);

            return source.SumAsync(CancellationToken.None);
        }

        internal static async Task<decimal?> SumAsync(this IDbAsyncEnumerable<decimal?> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<decimal?>>() != null);

            decimal sum = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    checked
                    {
                        if (e.Current.HasValue)
                        {
                            sum += e.Current.GetValueOrDefault();
                        }
                    }
                }
            }

            return sum;
        }

        internal static Task<double> AverageAsync(this IDbAsyncEnumerable<int> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);

            return source.AverageAsync(CancellationToken.None);
        }

        internal static async Task<double> AverageAsync(this IDbAsyncEnumerable<int> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<double>>() != null);

            long sum = 0;
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    checked
                    {
                        sum += e.Current;
                        count++;
                    }
                }
            }

            if (count > 0) return (double)sum / count;
            throw Error.EmptySequence();
        }

        internal static Task<double?> AverageAsync(this IDbAsyncEnumerable<int?> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double?>>() != null);

            return source.AverageAsync(CancellationToken.None);
        }

        internal static async Task<double?> AverageAsync(this IDbAsyncEnumerable<int?> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<double?>>() != null);

            long sum = 0;
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    checked
                    {
                        if (e.Current.HasValue)
                        {
                            sum += e.Current.GetValueOrDefault();
                            count++;
                        }
                    }
                }
            }

            if (count > 0) return (double)sum / count;
            throw Error.EmptySequence();
        }

        internal static Task<double> AverageAsync(this IDbAsyncEnumerable<long> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);

            return source.AverageAsync(CancellationToken.None);
        }

        internal static async Task<double> AverageAsync(this IDbAsyncEnumerable<long> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<double>>() != null);

            long sum = 0;
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    checked
                    {
                        sum += e.Current;
                        count++;
                    }
                }
            }

            if (count > 0) return (double)sum / count;
            throw Error.EmptySequence();
        }

        internal static Task<double?> AverageAsync(this IDbAsyncEnumerable<long?> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double?>>() != null);

            return source.AverageAsync(CancellationToken.None);
        }

        internal static async Task<double?> AverageAsync(this IDbAsyncEnumerable<long?> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<double?>>() != null);

            long sum = 0;
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    checked
                    {
                        if (e.Current.HasValue)
                        {
                            sum += e.Current.GetValueOrDefault();
                            count++;
                        }
                    }
                }
            }

            if (count > 0) return (double)sum / count;
            throw Error.EmptySequence();
        }

        internal static Task<float> AverageAsync(this IDbAsyncEnumerable<float> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<float>>() != null);

            return source.AverageAsync(CancellationToken.None);
        }

        internal static async Task<float> AverageAsync(this IDbAsyncEnumerable<float> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<float>>() != null);

            double sum = 0;
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    checked
                    {
                        sum += e.Current;
                        count++;
                    }
                }
            }

            if (count > 0) return (float)(sum / count);
            throw Error.EmptySequence();
        }

        internal static Task<float?> AverageAsync(this IDbAsyncEnumerable<float?> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<float?>>() != null);

            return source.AverageAsync(CancellationToken.None);
        }

        internal static async Task<float?> AverageAsync(this IDbAsyncEnumerable<float?> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<float?>>() != null);

            double sum = 0;
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    checked
                    {
                        if (e.Current.HasValue)
                        {
                            sum += e.Current.GetValueOrDefault();
                            count++;
                        }
                    }
                }
            }

            if (count > 0) return (float)(sum / count);
            throw Error.EmptySequence();
        }

        internal static Task<double> AverageAsync(this IDbAsyncEnumerable<double> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);

            return source.AverageAsync(CancellationToken.None);
        }

        internal static async Task<double> AverageAsync(this IDbAsyncEnumerable<double> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<double>>() != null);

            double sum = 0;
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    checked
                    {
                        sum += e.Current;
                        count++;
                    }
                }
            }

            if (count > 0) return (float)(sum / count);
            throw Error.EmptySequence();
        }

        internal static Task<double?> AverageAsync(this IDbAsyncEnumerable<double?> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double?>>() != null);

            return source.AverageAsync(CancellationToken.None);
        }

        internal static async Task<double?> AverageAsync(this IDbAsyncEnumerable<double?> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<double?>>() != null);

            double sum = 0;
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    checked
                    {
                        if (e.Current.HasValue)
                        {
                            sum += e.Current.GetValueOrDefault();
                            count++;
                        }
                    }
                }
            }

            if (count > 0) return (float)(sum / count);
            throw Error.EmptySequence();
        }

        internal static Task<decimal> AverageAsync(this IDbAsyncEnumerable<decimal> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<decimal>>() != null);

            return source.AverageAsync(CancellationToken.None);
        }

        internal static async Task<decimal> AverageAsync(this IDbAsyncEnumerable<decimal> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<decimal>>() != null);

            decimal sum = 0;
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    checked
                    {
                        sum += e.Current;
                        count++;
                    }
                }
            }

            if (count > 0) return sum / count;
            throw Error.EmptySequence();
        }

        internal static Task<decimal?> AverageAsync(this IDbAsyncEnumerable<decimal?> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<decimal?>>() != null);

            return source.AverageAsync(CancellationToken.None);
        }

        internal static async Task<decimal?> AverageAsync(this IDbAsyncEnumerable<decimal?> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<decimal?>>() != null);

            decimal sum = 0;
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    checked
                    {
                        if (e.Current.HasValue)
                        {
                            sum += e.Current.GetValueOrDefault();
                            count++;
                        }
                    }
                }
            }

            if (count > 0) return sum / count;
            throw Error.EmptySequence();
        }

        #region Nested classes

        private class CastDbAsyncEnumerable<TResult> : IDbAsyncEnumerable<TResult>
        {
            private readonly IDbAsyncEnumerable _underlyingEnumerable;

            public CastDbAsyncEnumerable(IDbAsyncEnumerable sourceEnumerable)
            {
                Contract.Requires(sourceEnumerable != null);

                _underlyingEnumerable = sourceEnumerable;
            }

            public IDbAsyncEnumerator<TResult> GetAsyncEnumerator()
            {
                return _underlyingEnumerable.GetAsyncEnumerator().Cast<TResult>();
            }

            IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
            {
                return _underlyingEnumerable.GetAsyncEnumerator();
            }
        }

        private static class IdentityFunction<TElement>
        {
            internal static Func<TElement, TElement> Instance
            {
                get { return x => x; }
            }
        }

        #endregion
    }
}
