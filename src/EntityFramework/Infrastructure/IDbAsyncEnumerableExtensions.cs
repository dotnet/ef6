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
    public static class IDbAsyncEnumerableExtensions
    {
        /// <summary>
        ///     Executes the provided action on each element of the <see cref="IDbAsyncEnumerable" />.
        /// </summary>
        /// <param name="action"> The action to be executed. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A Task representing the asynchronous operation. </returns>
        internal static async Task ForEachAsync(
            this IDbAsyncEnumerable enumerable, Action<object> action, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(enumerable != null);
            //Contract.Requires(action != null);
            //Contract.Ensures(Contract.Result<Task>() != null);

            using (var enumerator = enumerable.GetAsyncEnumerator())
            {
                if (await enumerator.MoveNextAsync(cancellationToken))
                {
                    Task<bool> moveNextTask;
                    do
                    {
                        var current = enumerator.Current;
                        moveNextTask = enumerator.MoveNextAsync(cancellationToken);
                        action(current);
                    }
                    while (await moveNextTask);
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
            this IDbAsyncEnumerable<T> enumerable, Action<T> action, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(enumerable != null);
            //Contract.Requires(action != null);
            //Contract.Ensures(Contract.Result<Task>() != null);

            using (var enumerator = enumerable.GetAsyncEnumerator())
            {
                if (await enumerator.MoveNextAsync(cancellationToken))
                {
                    Task<bool> moveNextTask;
                    do
                    {
                        var current = enumerator.Current;
                        moveNextTask = enumerator.MoveNextAsync(cancellationToken);
                        action(current);
                    }
                    while (await moveNextTask);
                }
            }
        }

        /// <summary>
        ///     Creates a <see cref="List{T}" /> from the <see cref="IDbAsyncEnumerable{T}" />.
        /// </summary>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A Task containing a <see cref="List{T}" /> that contains elements from the input sequence. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Task<List<T>> ToListAsync<T>(this IDbAsyncEnumerable<T> enumerable)
        {
            Contract.Requires(enumerable != null);
            Contract.Ensures(Contract.Result<Task<List<T>>>() != null);

            return enumerable.ToListAsync(CancellationToken.None);
        }

        /// <summary>
        ///     Creates a <see cref="List{T}" /> from the <see cref="IDbAsyncEnumerable{T}" />.
        /// </summary>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A Task containing a <see cref="List{T}" /> that contains elements from the input sequence. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static async Task<List<T>> ToListAsync<T>(this IDbAsyncEnumerable<T> enumerable, CancellationToken cancellationToken)
        {
            Contract.Requires(enumerable != null);
            Contract.Ensures(Contract.Result<Task<List<T>>>() != null);

            var list = new List<T>();
            await enumerable.ForEachAsync(list.Add, cancellationToken);
            return list;
        }

        internal static Task<TSource> FirstAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
        {
            Contract.Requires(source != null);

            return source.FirstAsync(CancellationToken.None);
        }

        internal static async Task<TSource> FirstAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);

            using (var e = source.GetAsyncEnumerator())
            {
                if (await e.MoveNextAsync(cancellationToken))
                {
                    return e.Current;
                }
            }

            throw Error.EmptySequence();
        }

        internal static Task<TSource> FirstOrDefaultAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
        {
            Contract.Requires(source != null);

            return source.FirstOrDefaultAsync(CancellationToken.None);
        }

        internal static async Task<TSource> FirstOrDefaultAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);

            using (var e = source.GetAsyncEnumerator())
            {
                if (await e.MoveNextAsync(cancellationToken))
                {
                    return e.Current;
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
                if (!await e.MoveNextAsync(cancellationToken))
                {
                    throw Error.EmptySequence();
                }
                var result = e.Current;
                if (!await e.MoveNextAsync(cancellationToken))
                {
                    return result;
                }
            }

            throw Error.MoreThanOneElement();
        }

        internal static Task<TSource> SingleOrDefaultAsync<TSource>(this IDbAsyncEnumerable<TSource> source)
        {
            Contract.Requires(source != null);

            return source.SingleOrDefaultAsync(CancellationToken.None);
        }

        internal static async Task<TSource> SingleOrDefaultAsync<TSource>(
            this IDbAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);

            using (var e = source.GetAsyncEnumerator())
            {
                if (!await e.MoveNextAsync(cancellationToken))
                {
                    return default(TSource);
                }
                var result = e.Current;
                if (!await e.MoveNextAsync(cancellationToken))
                {
                    return result;
                }
            }

            throw Error.MoreThanOneElement();
        }

        internal static IDbAsyncEnumerable<TResult> Cast<TResult>(this IDbAsyncEnumerable source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<IDbAsyncEnumerable<TResult>>() != null);

            return new CastDbAsyncEnumerable<TResult>(source);
        }

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
    }
}
