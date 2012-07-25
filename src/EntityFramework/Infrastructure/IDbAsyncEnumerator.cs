// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Infrastructure
{
    using System.Collections;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Asynchronous version of the <see cref="IEnumerator"/> interface that allows elements to be retrieved asynchronously.
    /// It is used to interact with Entity Framework queries and shouldn't be implemented by custom classes.
    /// </summary>
    public interface IDbAsyncEnumerator : IDisposable
    {
        /// <summary>
        /// Advances the enumerator to the next element in the sequence, returning the result asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A Task containing the result of the operation:
        /// true if the enumerator was successfully advanced to the next element;
        /// false if the enumerator has passed the end of the sequence.
        /// </returns>
        Task<bool> MoveNextAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the current element in the iteration. 
        /// </summary>
        object Current { get; }
    }
}
