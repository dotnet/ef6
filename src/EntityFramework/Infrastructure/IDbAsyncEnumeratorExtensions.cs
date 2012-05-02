namespace System.Data.Entity.Infrastructure
{
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    public static class IDbAsyncEnumeratorExtensions
    {
        /// <summary>
        /// Advances the enumerator to the next element in the sequence, returning the result asynchronously.
        /// </summary>
        /// <returns>
        /// A Task containing the result of the operation:
        /// true if the enumerator was successfully advanced to the next element;
        /// false if the enumerator has passed the end of the sequence.
        /// </returns>
        public static Task<bool> MoveNextAsync<T>(this IDbAsyncEnumerator<T> enumerator)
        {
            Contract.Requires(enumerator != null);

            return enumerator.MoveNextAsync(CancellationToken.None);
        }
    }
}
