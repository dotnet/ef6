// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


#if !NET40

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    // The methods in this class are internal so they don't conflict with the extension methods for IQueryable
    internal static class IDbAsyncEnumerableExtensions
    {
        // <summary>
        // Asynchronously executes the provided action on each element of the <see cref="IDbAsyncEnumerable" />.
        // </summary>
        // <param name="action"> The action to be executed. </param>
        // <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        // <returns> A Task representing the asynchronous operation. </returns>
        internal static async Task ForEachAsync(
            this IDbAsyncEnumerable source, Action<object> action, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(action);

            cancellationToken.ThrowIfCancellationRequested();

            using (var enumerator = source.GetAsyncEnumerator())
            {
                if (await enumerator.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    Task<bool> moveNextTask;
                    do
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var current = enumerator.Current;
                        moveNextTask = enumerator.MoveNextAsync(cancellationToken);
                        action(current);
                    }
                    while (await moveNextTask.ConfigureAwait(continueOnCapturedContext: false));
                }
            }
        }

        // <summary>
        // Asynchronously executes the provided action on each element of the <see cref="IDbAsyncEnumerable{T}" />.
        // </summary>
        // <param name="action"> The action to be executed. </param>
        // <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        // <returns> A Task representing the asynchronous operation. </returns>
        internal static Task ForEachAsync<T>(
            this IDbAsyncEnumerable<T> source, Action<T> action, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(action);
           
            return ForEachAsync(source.GetAsyncEnumerator(), action, cancellationToken);
        }

        private static async Task ForEachAsync<T>(
            IDbAsyncEnumerator<T> enumerator, Action<T> action, CancellationToken cancellationToken)
        {
            using (enumerator)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (await enumerator.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    Task<bool> moveNextTask;
                    do
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var current = enumerator.Current;
                        moveNextTask = enumerator.MoveNextAsync(cancellationToken);
                        action(current);
                    }
                    while (await moveNextTask.ConfigureAwait(continueOnCapturedContext: false));
                }
            }
        }

        // <summary>
        // Asynchronously creates a <see cref="List{T}" /> from the <see cref="IDbAsyncEnumerable" />.
        // </summary>
        // <typeparam name="T"> The type that the elements will be cast to. </typeparam>
        // <returns>
        // A <see cref="Task" /> containing a <see cref="List{T}" /> that contains elements from the input sequence.
        // </returns>
        internal static Task<List<T>> ToListAsync<T>(this IDbAsyncEnumerable source)
        {
            DebugCheck.NotNull(source);

            return source.ToListAsync<T>(CancellationToken.None);
        }

        // <summary>
        // Asynchronously creates a <see cref="List{T}" /> from the <see cref="IDbAsyncEnumerable" />.
        // </summary>
        // <typeparam name="T"> The type that the elements will be cast to. </typeparam>
        // <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        // <returns>
        // A <see cref="Task" /> containing a <see cref="List{T}" /> that contains elements from the input sequence.
        // </returns>
        internal static async Task<List<T>> ToListAsync<T>(this IDbAsyncEnumerable source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            var list = new List<T>();
            await source.ForEachAsync(e => list.Add((T)e), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            return list;
        }

        // <summary>
        // Asynchronously creates a <see cref="List{T}" /> from the <see cref="IDbAsyncEnumerable{T}" />.
        // </summary>
        // <returns>
        // A <see cref="Task" /> containing a <see cref="List{T}" /> that contains elements from the input sequence.
        // </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Task<List<T>> ToListAsync<T>(this IDbAsyncEnumerable<T> source)
        {
            DebugCheck.NotNull(source);

            return source.ToListAsync(CancellationToken.None);
        }

        // <summary>
        // Asynchronously creates a <see cref="List{T}" /> from the <see cref="IDbAsyncEnumerable{T}" />.
        // </summary>
        // <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        // <returns>
        // A <see cref="Task" /> containing a <see cref="List{T}" /> that contains elements from the input sequence.
        // </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Task<List<T>> ToListAsync<T>(this IDbAsyncEnumerable<T> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            var tcs = new TaskCompletionSource<List<T>>();
            var list = new List<T>();
            source.ForEachAsync(list.Add, cancellationToken).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        tcs.TrySetException(t.Exception.InnerExceptions);
                    }
                    else if (t.IsCanceled)
                    {
                        tcs.TrySetCanceled();
                    }
                    else
                    {
                        tcs.TrySetResult(list);
                    }
                }, TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }

        // <summary>
        // Asynchronously creates a T[] from an <see cref="IDbAsyncEnumerable{T}" /> by enumerating it asynchronously.
        // </summary>
        // <typeparam name="T">
        // The type of the elements of <paramref name="source" /> .
        // </typeparam>
        // <returns>
        // A <see cref="Task" /> containing a T[] that contains elements from the input sequence.
        // </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Task<T[]> ToArrayAsync<T>(this IDbAsyncEnumerable<T> source)
        {
            DebugCheck.NotNull(source);

            return source.ToArrayAsync(CancellationToken.None);
        }

        // <summary>
        // Asynchronously creates a T[] from an <see cref="IDbAsyncEnumerable{T}" /> by enumerating it asynchronously.
        // </summary>
        // <typeparam name="T">
        // The type of the elements of <paramref name="source" /> .
        // </typeparam>
        // <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        // <returns>
        // A <see cref="Task" /> containing a T[] that contains elements from the input sequence.
        // </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static async Task<T[]> ToArrayAsync<T>(this IDbAsyncEnumerable<T> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            var list = await source.ToListAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            return list.ToArray();
        }

        // <summary>
        // Asynchronously creates a <see cref="Dictionary{TKey, TSource}" /> from an <see cref="IDbAsyncEnumerable{TSource}" />
        // by enumerating it asynchronously according to a specified key selector function.
        // </summary>
        // <typeparam name="TSource">
        // The type of the elements of <paramref name="source" /> .
        // </typeparam>
        // <typeparam name="TKey">
        // The type of the key returned by <paramref name="keySelector" /> .
        // </typeparam>
        // <param name="keySelector"> A function to extract a key from each element. </param>
        // <returns>
        // A <see cref="Task" /> containing a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values.
        // </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(keySelector);

            return ToDictionaryAsync(source, keySelector, IdentityFunction<TSource>.Instance, null, CancellationToken.None);
        }

        // <summary>
        // Asynchronously creates a <see cref="Dictionary{TKey, TSource}" /> from an <see cref="IDbAsyncEnumerable{TSource}" />
        // by enumerating it asynchronously according to a specified key selector function.
        // </summary>
        // <typeparam name="TSource">
        // The type of the elements of <paramref name="source" /> .
        // </typeparam>
        // <typeparam name="TKey">
        // The type of the key returned by <paramref name="keySelector" /> .
        // </typeparam>
        // <param name="keySelector"> A function to extract a key from each element. </param>
        // <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        // <returns>
        // A <see cref="Task" /> containing a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values.
        // </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(keySelector);

            return ToDictionaryAsync(source, keySelector, IdentityFunction<TSource>.Instance, null, cancellationToken);
        }

        // <summary>
        // Asynchronously creates a <see cref="Dictionary{TKey, TSource}" /> from an <see cref="IDbAsyncEnumerable{TSource}" />
        // by enumerating it asynchronously according to a specified key selector function and a comparer.
        // </summary>
        // <typeparam name="TSource">
        // The type of the elements of <paramref name="source" /> .
        // </typeparam>
        // <typeparam name="TKey">
        // The type of the key returned by <paramref name="keySelector" /> .
        // </typeparam>
        // <param name="keySelector"> A function to extract a key from each element. </param>
        // <param name="comparer">
        // An <see cref="IEqualityComparer{TKey}" /> to compare keys.
        // </param>
        // <returns>
        // A <see cref="Task" /> containing a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values.
        // </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(keySelector);

            return ToDictionaryAsync(source, keySelector, IdentityFunction<TSource>.Instance, comparer, CancellationToken.None);
        }

        // <summary>
        // Asynchronously creates a <see cref="Dictionary{TKey, TSource}" /> from an <see cref="IDbAsyncEnumerable{TSource}" />
        // by enumerating it asynchronously according to a specified key selector function and a comparer.
        // </summary>
        // <typeparam name="TSource">
        // The type of the elements of <paramref name="source" /> .
        // </typeparam>
        // <typeparam name="TKey">
        // The type of the key returned by <paramref name="keySelector" /> .
        // </typeparam>
        // <param name="keySelector"> A function to extract a key from each element. </param>
        // <param name="comparer">
        // An <see cref="IEqualityComparer{TKey}" /> to compare keys.
        // </param>
        // <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        // <returns>
        // A <see cref="Task" /> containing a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values.
        // </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(keySelector);

            return ToDictionaryAsync(source, keySelector, IdentityFunction<TSource>.Instance, comparer, cancellationToken);
        }

        // <summary>
        // Asynchronously creates a <see cref="Dictionary{TKey, TElement}" /> from an <see cref="IDbAsyncEnumerable{TSource}" />
        // by enumerating it asynchronously according to a specified key selector and an element selector function.
        // </summary>
        // <typeparam name="TSource">
        // The type of the elements of <paramref name="source" /> .
        // </typeparam>
        // <typeparam name="TKey">
        // The type of the key returned by <paramref name="keySelector" /> .
        // </typeparam>
        // <typeparam name="TElement">
        // The type of the value returned by <paramref name="elementSelector" /> .
        // </typeparam>
        // <param name="keySelector"> A function to extract a key from each element. </param>
        // <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        // <returns>
        // A <see cref="Task" /> containing a <see cref="Dictionary{TKey, TElement}" /> that contains values of type
        // <typeparamref
        //     name="TElement" />
        // selected from the input sequence.
        // </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(keySelector);
            DebugCheck.NotNull(elementSelector);

            return ToDictionaryAsync(source, keySelector, elementSelector, null, CancellationToken.None);
        }

        // <summary>
        // Asynchronously creates a <see cref="Dictionary{TKey, TElement}" /> from an <see cref="IDbAsyncEnumerable{TSource}" />
        // by enumerating it asynchronously according to a specified key selector and an element selector function.
        // </summary>
        // <typeparam name="TSource">
        // The type of the elements of <paramref name="source" /> .
        // </typeparam>
        // <typeparam name="TKey">
        // The type of the key returned by <paramref name="keySelector" /> .
        // </typeparam>
        // <typeparam name="TElement">
        // The type of the value returned by <paramref name="elementSelector" /> .
        // </typeparam>
        // <param name="keySelector"> A function to extract a key from each element. </param>
        // <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        // <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        // <returns>
        // A <see cref="Task" /> containing a <see cref="Dictionary{TKey, TElement}" /> that contains values of type
        // <typeparamref
        //     name="TElement" />
        // selected from the input sequence.
        // </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector,
            CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(keySelector);
            DebugCheck.NotNull(elementSelector);

            return ToDictionaryAsync(source, keySelector, elementSelector, null, cancellationToken);
        }

        // <summary>
        // Asynchronously creates a <see cref="Dictionary{TKey, TElement}" /> from an <see cref="IDbAsyncEnumerable{TSource}" />
        // by enumerating it asynchronously according to a specified key selector function, a comparer, and an element selector function.
        // </summary>
        // <typeparam name="TSource">
        // The type of the elements of <paramref name="source" /> .
        // </typeparam>
        // <typeparam name="TKey">
        // The type of the key returned by <paramref name="keySelector" /> .
        // </typeparam>
        // <typeparam name="TElement">
        // The type of the value returned by <paramref name="elementSelector" /> .
        // </typeparam>
        // <param name="keySelector"> A function to extract a key from each element. </param>
        // <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        // <param name="comparer">
        // An <see cref="IEqualityComparer{TKey}" /> to compare keys.
        // </param>
        // <returns>
        // A <see cref="Task" /> containing a <see cref="Dictionary{TKey, TElement}" /> that contains values of type
        // <typeparamref
        //     name="TElement" />
        // selected from the input sequence.
        // </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(keySelector);
            DebugCheck.NotNull(elementSelector);

            return ToDictionaryAsync(source, keySelector, elementSelector, comparer, CancellationToken.None);
        }

        // <summary>
        // Asynchronously creates a <see cref="Dictionary{TKey, TElement}" /> from an <see cref="IDbAsyncEnumerable{TSource}" />
        // by enumerating it asynchronously according to a specified key selector function, a comparer, and an element selector function.
        // </summary>
        // <typeparam name="TSource">
        // The type of the elements of <paramref name="source" /> .
        // </typeparam>
        // <typeparam name="TKey">
        // The type of the key returned by <paramref name="keySelector" /> .
        // </typeparam>
        // <typeparam name="TElement">
        // The type of the value returned by <paramref name="elementSelector" /> .
        // </typeparam>
        // <param name="keySelector"> A function to extract a key from each element. </param>
        // <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        // <param name="comparer">
        // An <see cref="IEqualityComparer{TKey}" /> to compare keys.
        // </param>
        // <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        // <returns>
        // A <see cref="Task" /> containing a <see cref="Dictionary{TKey, TElement}" /> that contains values of type
        // <typeparamref
        //     name="TElement" />
        // selected from the input sequence.
        // </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static async Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(keySelector);
            DebugCheck.NotNull(elementSelector);

            var d = new Dictionary<TKey, TElement>(comparer);
            await
                source.ForEachAsync(element => d.Add(keySelector(element), elementSelector(element)), cancellationToken).ConfigureAwait(
                    continueOnCapturedContext: false);
            return d;
        }

        internal static IDbAsyncEnumerable<TResult> Cast<TResult>(this IDbAsyncEnumerable source)
        {
            DebugCheck.NotNull(source);

            return new CastDbAsyncEnumerable<TResult>(source);
        }

        internal static Task<TSource> FirstAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
        {
            DebugCheck.NotNull(source);

            return source.FirstAsync(CancellationToken.None);
        }

        internal static Task<TSource> FirstAsync<TSource>(this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(predicate);

            return source.FirstAsync(predicate, CancellationToken.None);
        }

        internal static async Task<TSource> FirstAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

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
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(predicate);

            cancellationToken.ThrowIfCancellationRequested();

            using (var e = source.GetAsyncEnumerator())
            {
                if (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    if (predicate(e.Current))
                    {
                        return e.Current;
                    }
                }
            }

            throw Error.NoMatch();
        }

        internal static Task<TSource> FirstOrDefaultAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
        {
            DebugCheck.NotNull(source);

            return source.FirstOrDefaultAsync(CancellationToken.None);
        }

        internal static Task<TSource> FirstOrDefaultAsync<TSource>(this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            DebugCheck.NotNull(source);

            return source.FirstOrDefaultAsync(predicate, CancellationToken.None);
        }

        internal static async Task<TSource> FirstOrDefaultAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

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
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            using (var e = source.GetAsyncEnumerator())
            {
                if (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    if (predicate(e.Current))
                    {
                        return e.Current;
                    }
                }
            }

            return default(TSource);
        }

        internal static Task<TSource> SingleAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
        {
            DebugCheck.NotNull(source);

            return source.SingleAsync(CancellationToken.None);
        }

        internal static async Task<TSource> SingleAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            using (var e = source.GetAsyncEnumerator())
            {
                bool sequenceEmpty = !await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

                cancellationToken.ThrowIfCancellationRequested();

                if (sequenceEmpty)
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

        internal static Task<TSource> SingleAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(predicate);

            return source.SingleAsync(predicate, CancellationToken.None);
        }

        internal static async Task<TSource> SingleAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(predicate);

            cancellationToken.ThrowIfCancellationRequested();

            var result = default(TSource);
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (predicate(e.Current))
                    {
                        result = e.Current;
                        checked
                        {
                            count++;
                        }
                    }
                }
            }

            switch (count)
            {
                case 0:
                    throw Error.NoMatch();
                case 1:
                    return result;
            }

            throw Error.MoreThanOneMatch();
        }

        internal static Task<TSource> SingleOrDefaultAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
        {
            DebugCheck.NotNull(source);

            return source.SingleOrDefaultAsync(CancellationToken.None);
        }

        internal static async Task<TSource> SingleOrDefaultAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            using (var e = source.GetAsyncEnumerator())
            {
                if (!await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    return default(TSource);
                }

                cancellationToken.ThrowIfCancellationRequested();

                var result = e.Current;

                if (!await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    return result;
                }
            }

            throw Error.MoreThanOneElement();
        }

        internal static Task<TSource> SingleOrDefaultAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(predicate);

            return source.SingleOrDefaultAsync(predicate, CancellationToken.None);
        }

        internal static async Task<TSource> SingleOrDefaultAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(predicate);

            cancellationToken.ThrowIfCancellationRequested();

            var result = default(TSource);
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (predicate(e.Current))
                    {
                        result = e.Current;
                        checked
                        {
                            count++;
                        }
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
            DebugCheck.NotNull(source);

            return source.ContainsAsync(value, CancellationToken.None);
        }

        internal static async Task<bool> ContainsAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, TSource value, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

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
            DebugCheck.NotNull(source);

            return source.AnyAsync(CancellationToken.None);
        }

        internal static async Task<bool> AnyAsync<TSource>(this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            using (var e = source.GetAsyncEnumerator())
            {
                if (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    return true;
                }
            }

            return false;
        }

        internal static Task<bool> AnyAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(predicate);

            return source.AnyAsync(predicate, CancellationToken.None);
        }

        internal static async Task<bool> AnyAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(predicate);

            cancellationToken.ThrowIfCancellationRequested();

            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

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
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(predicate);

            return source.AllAsync(predicate, CancellationToken.None);
        }

        internal static async Task<bool> AllAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(predicate);

            cancellationToken.ThrowIfCancellationRequested();

            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

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
            DebugCheck.NotNull(source);

            return source.CountAsync(CancellationToken.None);
        }

        internal static async Task<int> CountAsync<TSource>(this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            var count = 0;

            using (var e = source.GetAsyncEnumerator())
            {
                checked
                {
                    while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        count++;
                    }
                }
            }

            return count;
        }

        internal static Task<int> CountAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(predicate);

            return source.CountAsync(predicate, CancellationToken.None);
        }

        internal static async Task<int> CountAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(predicate);

            cancellationToken.ThrowIfCancellationRequested();

            var count = 0;

            using (var e = source.GetAsyncEnumerator())
            {
                checked
                {
                    while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

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
            DebugCheck.NotNull(source);

            return source.LongCountAsync(CancellationToken.None);
        }

        internal static async Task<long> LongCountAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            long count = 0;

            using (var e = source.GetAsyncEnumerator())
            {
                checked
                {
                    while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        count++;
                    }
                }
            }

            return count;
        }

        internal static Task<long> LongCountAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(predicate);

            return source.LongCountAsync(predicate, CancellationToken.None);
        }

        internal static async Task<long> LongCountAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(predicate);

            cancellationToken.ThrowIfCancellationRequested();

            long count = 0;

            using (var e = source.GetAsyncEnumerator())
            {
                checked
                {
                    while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

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
            DebugCheck.NotNull(source);

            return source.MinAsync(CancellationToken.None);
        }

        internal static async Task<TSource> MinAsync<TSource>(this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            var comparer = Comparer<TSource>.Default;
            var value = default(TSource);
            if (value == null)
            {
                using (var e = source.GetAsyncEnumerator())
                {
                    while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (e.Current != null
                            && (value == null || comparer.Compare(e.Current, value) < 0))
                        {
                            value = e.Current;
                        }
                    }
                }

                return value;
            }
            else
            {
                var hasValue = false;

                using (var e = source.GetAsyncEnumerator())
                {
                    while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

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

                if (hasValue)
                {
                    return value;
                }
                throw Error.EmptySequence();
            }
        }

        internal static Task<TSource> MaxAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
        {
            DebugCheck.NotNull(source);

            return source.MaxAsync(CancellationToken.None);
        }

        internal static async Task<TSource> MaxAsync<TSource>(this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            var comparer = Comparer<TSource>.Default;
            var value = default(TSource);
            if (value == null)
            {
                using (var e = source.GetAsyncEnumerator())
                {
                    while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (e.Current != null
                            && (value == null || comparer.Compare(e.Current, value) > 0))
                        {
                            value = e.Current;
                        }
                    }
                }

                return value;
            }
            else
            {
                var hasValue = false;

                using (var e = source.GetAsyncEnumerator())
                {
                    while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

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

                if (hasValue)
                {
                    return value;
                }
                throw Error.EmptySequence();
            }
        }

        internal static Task<int> SumAsync(this IDbAsyncEnumerable<int> source)
        {
            DebugCheck.NotNull(source);

            return source.SumAsync(CancellationToken.None);
        }

        internal static async Task<int> SumAsync(this IDbAsyncEnumerable<int> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            long sum = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

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
            DebugCheck.NotNull(source);

            return source.SumAsync(CancellationToken.None);
        }

        internal static async Task<int?> SumAsync(this IDbAsyncEnumerable<int?> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            long sum = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

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
            DebugCheck.NotNull(source);

            return source.SumAsync(CancellationToken.None);
        }

        internal static async Task<long> SumAsync(this IDbAsyncEnumerable<long> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            long sum = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

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
            DebugCheck.NotNull(source);

            return source.SumAsync(CancellationToken.None);
        }

        internal static async Task<long?> SumAsync(this IDbAsyncEnumerable<long?> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            long sum = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

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
            DebugCheck.NotNull(source);

            return source.SumAsync(CancellationToken.None);
        }

        internal static async Task<float> SumAsync(this IDbAsyncEnumerable<float> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            double sum = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

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
            DebugCheck.NotNull(source);

            return source.SumAsync(CancellationToken.None);
        }

        internal static async Task<float?> SumAsync(this IDbAsyncEnumerable<float?> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            double sum = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

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
            DebugCheck.NotNull(source);

            return source.SumAsync(CancellationToken.None);
        }

        internal static async Task<double> SumAsync(this IDbAsyncEnumerable<double> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            double sum = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

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
            DebugCheck.NotNull(source);

            return source.SumAsync(CancellationToken.None);
        }

        internal static async Task<double?> SumAsync(this IDbAsyncEnumerable<double?> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            double sum = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

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
            DebugCheck.NotNull(source);

            return source.SumAsync(CancellationToken.None);
        }

        internal static async Task<decimal> SumAsync(this IDbAsyncEnumerable<decimal> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            decimal sum = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

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
            DebugCheck.NotNull(source);

            return source.SumAsync(CancellationToken.None);
        }

        internal static async Task<decimal?> SumAsync(this IDbAsyncEnumerable<decimal?> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            decimal sum = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

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
            DebugCheck.NotNull(source);

            return source.AverageAsync(CancellationToken.None);
        }

        internal static async Task<double> AverageAsync(this IDbAsyncEnumerable<int> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            long sum = 0;
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    checked
                    {
                        sum += e.Current;
                        count++;
                    }
                }
            }

            if (count > 0)
            {
                return (double)sum / count;
            }
            throw Error.EmptySequence();
        }

        internal static Task<double?> AverageAsync(this IDbAsyncEnumerable<int?> source)
        {
            DebugCheck.NotNull(source);

            return source.AverageAsync(CancellationToken.None);
        }

        internal static async Task<double?> AverageAsync(this IDbAsyncEnumerable<int?> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            long sum = 0;
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

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

            if (count > 0)
            {
                return (double)sum / count;
            }
            throw Error.EmptySequence();
        }

        internal static Task<double> AverageAsync(this IDbAsyncEnumerable<long> source)
        {
            DebugCheck.NotNull(source);

            return source.AverageAsync(CancellationToken.None);
        }

        internal static async Task<double> AverageAsync(this IDbAsyncEnumerable<long> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            long sum = 0;
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    checked
                    {
                        sum += e.Current;
                        count++;
                    }
                }
            }

            if (count > 0)
            {
                return (double)sum / count;
            }
            throw Error.EmptySequence();
        }

        internal static Task<double?> AverageAsync(this IDbAsyncEnumerable<long?> source)
        {
            DebugCheck.NotNull(source);

            return source.AverageAsync(CancellationToken.None);
        }

        internal static async Task<double?> AverageAsync(this IDbAsyncEnumerable<long?> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            long sum = 0;
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

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

            if (count > 0)
            {
                return (double)sum / count;
            }
            throw Error.EmptySequence();
        }

        internal static Task<float> AverageAsync(this IDbAsyncEnumerable<float> source)
        {
            DebugCheck.NotNull(source);

            return source.AverageAsync(CancellationToken.None);
        }

        internal static async Task<float> AverageAsync(this IDbAsyncEnumerable<float> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            double sum = 0;
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    checked
                    {
                        sum += e.Current;
                        count++;
                    }
                }
            }

            if (count > 0)
            {
                return (float)(sum / count);
            }
            throw Error.EmptySequence();
        }

        internal static Task<float?> AverageAsync(this IDbAsyncEnumerable<float?> source)
        {
            DebugCheck.NotNull(source);

            return source.AverageAsync(CancellationToken.None);
        }

        internal static async Task<float?> AverageAsync(this IDbAsyncEnumerable<float?> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            double sum = 0;
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

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

            if (count > 0)
            {
                return (float)(sum / count);
            }
            throw Error.EmptySequence();
        }

        internal static Task<double> AverageAsync(this IDbAsyncEnumerable<double> source)
        {
            DebugCheck.NotNull(source);

            return source.AverageAsync(CancellationToken.None);
        }

        internal static async Task<double> AverageAsync(this IDbAsyncEnumerable<double> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            double sum = 0;
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    checked
                    {
                        sum += e.Current;
                        count++;
                    }
                }
            }

            if (count > 0)
            {
                return (float)(sum / count);
            }
            throw Error.EmptySequence();
        }

        internal static Task<double?> AverageAsync(this IDbAsyncEnumerable<double?> source)
        {
            DebugCheck.NotNull(source);

            return source.AverageAsync(CancellationToken.None);
        }

        internal static async Task<double?> AverageAsync(this IDbAsyncEnumerable<double?> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            double sum = 0;
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

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

            if (count > 0)
            {
                return (float)(sum / count);
            }
            throw Error.EmptySequence();
        }

        internal static Task<decimal> AverageAsync(this IDbAsyncEnumerable<decimal> source)
        {
            DebugCheck.NotNull(source);

            return source.AverageAsync(CancellationToken.None);
        }

        internal static async Task<decimal> AverageAsync(this IDbAsyncEnumerable<decimal> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            decimal sum = 0;
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    checked
                    {
                        sum += e.Current;
                        count++;
                    }
                }
            }

            if (count > 0)
            {
                return sum / count;
            }
            throw Error.EmptySequence();
        }

        internal static Task<decimal?> AverageAsync(this IDbAsyncEnumerable<decimal?> source)
        {
            DebugCheck.NotNull(source);

            return source.AverageAsync(CancellationToken.None);
        }

        internal static async Task<decimal?> AverageAsync(this IDbAsyncEnumerable<decimal?> source, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(source);

            cancellationToken.ThrowIfCancellationRequested();

            decimal sum = 0;
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

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

            if (count > 0)
            {
                return sum / count;
            }
            throw Error.EmptySequence();
        }

        #region Nested classes

        private class CastDbAsyncEnumerable<TResult> : IDbAsyncEnumerable<TResult>
        {
            private readonly IDbAsyncEnumerable _underlyingEnumerable;

            public CastDbAsyncEnumerable(IDbAsyncEnumerable sourceEnumerable)
            {
                DebugCheck.NotNull(sourceEnumerable);

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

#endif
