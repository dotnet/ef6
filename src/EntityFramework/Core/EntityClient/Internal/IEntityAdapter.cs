// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.EntityClient.Internal
{
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The IEntityAdapter interface allows adapters to support updates of entities stored in an IEntityCache.
    /// </summary>
    internal interface IEntityAdapter
    {
        /// <summary>
        /// Gets or sets the connection used by this adapter.
        /// </summary>
        DbConnection Connection { get; set; }

        /// <summary>
        /// Gets or sets whether the IEntityCache.AcceptChanges should be called during a call to IEntityAdapter.Update.
        /// </summary>
        bool AcceptChangesDuringUpdate { get; set; }

        /// <summary>
        /// Gets of sets the command timeout for update operations. If null, indicates that the default timeout
        /// for the provider should be used.
        /// </summary>
        Int32? CommandTimeout { get; set; }

        /// <summary>
        /// Persists the changes made in the entity cache to the store.
        /// </summary>
        Int32 Update(IEntityStateManager cache);

        /// <summary>
        /// An asynchronous version of Update, which
        /// persists modifications described in the given cache.
        /// </summary>
        /// <param name="entityCache">Entity cache containing changes to persist to the store.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task containing the number of cache entries affected by the update.</returns>
        Task<int> UpdateAsync(IEntityStateManager entityCache, CancellationToken cancellationToken);
    }
}
