// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


#if !NET40

namespace System.Data.Entity.Infrastructure
{
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    public static class IDbAsyncEnumeratorExtensions
    {
        /// <summary>
        ///     Advances the enumerator to the next element in the sequence, returning the result asynchronously.
        /// </summary>
        /// <returns> A Task containing the result of the operation: true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the sequence. </returns>
        public static Task<bool> MoveNextAsync(this IDbAsyncEnumerator enumerator)
        {
            Contract.Requires(enumerator != null);

            return enumerator.MoveNextAsync(CancellationToken.None);
        }

        internal static IDbAsyncEnumerator<TResult> Cast<TResult>(this IDbAsyncEnumerator source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<IDbAsyncEnumerator<TResult>>() != null);

            return new CastDbAsyncEnumerator<TResult>(source);
        }

        private class CastDbAsyncEnumerator<TResult> : IDbAsyncEnumerator<TResult>
        {
            private readonly IDbAsyncEnumerator _underlyingEnumerator;

            public CastDbAsyncEnumerator(IDbAsyncEnumerator sourceEnumerator)
            {
                Contract.Requires(sourceEnumerator != null);

                _underlyingEnumerator = sourceEnumerator;
            }

            public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
            {
                return _underlyingEnumerator.MoveNextAsync(cancellationToken);
            }

            public TResult Current
            {
                get { return (TResult)_underlyingEnumerator.Current; }
            }

            object IDbAsyncEnumerator.Current
            {
                get { return _underlyingEnumerator.Current; }
            }

            public void Dispose()
            {
                _underlyingEnumerator.Dispose();
            }
        }
    }
}

#endif
