// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


#if !NET40

namespace System.Data.Entity.Infrastructure
{
    using System.Collections;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Asynchronous version of the <see cref="IEnumerator" /> interface that allows elements to be retrieved asynchronously.
    /// This interface is used to interact with Entity Framework queries and shouldn't be implemented by custom classes.
    /// </summary>
    public interface IDbAsyncEnumerator : IDisposable
    {
        /// <summary>
        /// Advances the enumerator to the next element in the sequence, returning the result asynchronously.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the sequence.
        /// </returns>
        Task<bool> MoveNextAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the current element in the iteration.
        /// </summary>
        object Current { get; }
    }
}

#endif
