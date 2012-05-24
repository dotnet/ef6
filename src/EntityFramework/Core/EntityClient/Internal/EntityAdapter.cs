namespace System.Data.Entity.Core.EntityClient.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Core.Mapping.Update.Internal;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    internal class EntityAdapter : IEntityAdapter
    {
        private bool _acceptChangesDuringUpdate = true;
        private EntityConnection _connection;
        private readonly Func<IEntityStateManager, EntityAdapter, UpdateTranslator> _updateTranslatorFactory;

        public EntityAdapter()
            : this((stateManager, adapter) => new UpdateTranslator(stateManager, adapter))
        {
        }

        protected EntityAdapter(Func<IEntityStateManager, EntityAdapter, UpdateTranslator> updateTranslatorFactory)
        {
            _updateTranslatorFactory = updateTranslatorFactory;
        }

        /// <summary>
        /// Gets or sets the map connection used by this adapter.
        /// </summary>
        DbConnection IEntityAdapter.Connection
        {
            get { return Connection; }
            set { Connection = (EntityConnection)value; }
        }

        /// <summary>
        /// Gets or sets the map connection used by this adapter.
        /// </summary>
        public EntityConnection Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        /// <summary>
        /// Gets or sets whether the IEntityCache.AcceptChanges should be called during a call to IEntityAdapter.Update.
        /// </summary>
        public bool AcceptChangesDuringUpdate
        {
            get { return _acceptChangesDuringUpdate; }
            set { _acceptChangesDuringUpdate = value; }
        }

        /// <summary>
        /// Gets of sets the command timeout for update operations. If null, indicates that the default timeout
        /// for the provider should be used.
        /// </summary>
        public int? CommandTimeout { get; set; }

        /// <summary>
        /// Persist modifications described in the given cache.
        /// </summary>
        /// <param name="entityCache">Entity cache containing changes to persist to the store.</param>
        /// <returns>Number of cache entries affected by the udpate.</returns>
        public int Update(IEntityStateManager entityCache)
        {
            return Update(entityCache, 0, (ut) => ut.Update());
        }

        /// <summary>
        /// An asynchronous version of Update, which
        /// persists modifications described in the given cache.
        /// </summary>
        /// <param name="entityCache">Entity cache containing changes to persist to the store.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task containing the number of cache entries affected by the update.</returns>
        public Task<int> UpdateAsync(IEntityStateManager entityCache, CancellationToken cancellationToken)
        {
            return Update(entityCache, Task.FromResult(0), (ut) => ut.UpdateAsync(cancellationToken));
        }

        private T Update<T>(IEntityStateManager entityCache, T noChangesResult, Func<UpdateTranslator, T> updateFunction)
        {
            if (!IsStateManagerDirty(entityCache))
            {
                return noChangesResult;
            }

            // Check that we have a connection before we proceed
            if (_connection == null)
            {
                throw Error.EntityClient_NoConnectionForAdapter();
            }

            // Check that the store connection is available
            if (_connection.StoreProviderFactory == null
                || _connection.StoreConnection == null)
            {
                throw Error.EntityClient_NoStoreConnectionForUpdate();
            }

            // Check that the connection is open before we proceed
            if (ConnectionState.Open
                != _connection.State)
            {
                throw Error.EntityClient_ClosedConnectionForUpdate();
            }

            var updateTranslator = _updateTranslatorFactory(entityCache, this);
            return updateFunction(updateTranslator);
        }

        /// <summary>
        /// Determine whether the cache has changes to apply.
        /// </summary>
        /// <param name="entityCache">ObjectStateManager to check. Must not be null.</param>
        /// <returns>true if cache contains changes entries; false otherwise</returns>
        private static bool IsStateManagerDirty(IEntityStateManager entityCache)
        {
            Debug.Assert(null != entityCache);
            var hasChanges = false;

            // this call to GetCacheEntries is constant time (the ObjectStateManager implementation
            // maintains an explicit list of entries in each state)
            foreach (var entry in entityCache.GetEntityStateEntries(EntityState.Added | EntityState.Deleted | EntityState.Modified))
            {
                hasChanges = true;
                break;
            }

            return hasChanges;
        }
    }
}
