namespace System.Data.Entity.Core.EntityClient.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Core.Mapping.Update.Internal;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Resources;
    using System.Diagnostics;

    internal class EntityAdapter : IEntityAdapter
    {
        private bool _acceptChangesDuringUpdate = true;
        private EntityConnection _connection;

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
        public Int32? CommandTimeout { get; set; }

        /// <summary>
        /// Persist modifications described in the given cache.
        /// </summary>
        /// <param name="entityCache">Entity cache containing changes to persist to the store.</param>
        /// <returns>Number of cache entries affected by the udpate.</returns>
        public Int32 Update(IEntityStateManager entityCache)
        {
            if (!IsStateManagerDirty(entityCache))
            {
                return 0;
            }

            // Check that we have a connection before we proceed
            if (_connection == null)
            {
                throw new InvalidOperationException(Strings.EntityClient_NoConnectionForAdapter);
            }

            // Check that the store connection is available
            if (_connection.StoreProviderFactory == null || _connection.StoreConnection == null)
            {
                throw new InvalidOperationException(Strings.EntityClient_NoStoreConnectionForUpdate);
            }

            // Check that the connection is open before we proceed
            if (ConnectionState.Open != _connection.State)
            {
                throw new InvalidOperationException(Strings.EntityClient_ClosedConnectionForUpdate);
            }

            var updateTranslator = new UpdateTranslator(entityCache, this);
            return updateTranslator.Update();
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
            foreach (ObjectStateEntry entry in entityCache.GetEntityStateEntries(EntityState.Added | EntityState.Deleted | EntityState.Modified))
            {
                hasChanges = true;
                break;
            }

            return hasChanges;
        }
    }
}
