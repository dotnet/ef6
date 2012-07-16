namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Infrastructure;
    using System.Threading;
    using System.Threading.Tasks;

    internal class LazyAsyncEnumerator<T> : IDbAsyncEnumerator<T>
    {
        private readonly Lazy<Task<IDbAsyncEnumerator<T>>> _lazyAsyncEnumerator;

        /// <summary>
        ///     Initializes a new instance of <see cref="LazyAsyncEnumerator{T}"/>
        /// </summary>
        /// <param name="getEnumeratorAsync">Function that returns a Task containing the <see cref="IDbAsyncEnumerator{T}"/>.
        /// Should not return null.</param>
        // TODO: Currently we are not accepting a CancellationToken parameter because we are relying on Lazy<T>
        // which doesn't support TAP
        public LazyAsyncEnumerator(Func<Task<IDbAsyncEnumerator<T>>> getEnumeratorAsync)
        {
            _lazyAsyncEnumerator = new Lazy<Task<IDbAsyncEnumerator<T>>>(getEnumeratorAsync);
        }

        public T Current
        {
            get { return _lazyAsyncEnumerator.Value.Result.Current; }
        }

        object IDbAsyncEnumerator.Current
        {
            get { return Current; }
        }

        public void Dispose()
        {
            if (_lazyAsyncEnumerator.IsValueCreated)
            {
                _lazyAsyncEnumerator.Value.Result.Dispose();
            }
        }

        public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            var enumerator = await _lazyAsyncEnumerator.Value;
            return await enumerator.MoveNextAsync(cancellationToken);
        }
    }
}
