// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


#if !NET40

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    internal class LazyAsyncEnumerator<T> : IDbAsyncEnumerator<T>
    {
        private readonly Func<CancellationToken, Task<IDbAsyncEnumerator<T>>> _getEnumeratorAsync;
        private volatile Task<IDbAsyncEnumerator<T>> _asyncEnumeratorTask;

        /// <summary>
        ///     Initializes a new instance of <see cref="LazyAsyncEnumerator{T}" />
        /// </summary>
        /// <param name="getEnumeratorAsync">
        ///     Function that returns a Task containing the <see cref="IDbAsyncEnumerator{T}" /> . Should not return null.
        /// </param>
        public LazyAsyncEnumerator(Func<CancellationToken, Task<IDbAsyncEnumerator<T>>> getEnumeratorAsync)
        {
            DebugCheck.NotNull(getEnumeratorAsync);

            _getEnumeratorAsync = getEnumeratorAsync;
        }

        private Task<IDbAsyncEnumerator<T>> GetEnumeratorAsync(CancellationToken cancellationToken)
        {
            if (_asyncEnumeratorTask == null)
            {
                lock (_getEnumeratorAsync)
                {
                    if (_asyncEnumeratorTask == null)
                    {
                        _asyncEnumeratorTask = _getEnumeratorAsync(cancellationToken);
                    }
                }
            }

            return _asyncEnumeratorTask;
        }

        public T Current
        {
            get { return GetEnumeratorAsync(CancellationToken.None).Result.Current; }
        }

        object IDbAsyncEnumerator.Current
        {
            get { return Current; }
        }

        public void Dispose()
        {
            if (_asyncEnumeratorTask != null
                && !_asyncEnumeratorTask.IsCanceled
                && !_asyncEnumeratorTask.IsFaulted)
            {
                Debug.Assert(
                    _asyncEnumeratorTask.IsCompleted,
                    "Task hasn't completed, this means that the tasks returned from MoveNextAsync wasn't awaited on.");
                _asyncEnumeratorTask.Result.Dispose();
            }
        }

        public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            var enumerator = await GetEnumeratorAsync(CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
            return await enumerator.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}

#endif
