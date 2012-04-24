namespace System.Data.Entity.Core.EntityClient
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Class representing a data adapter for the conceptual layer
    /// </summary>
    internal sealed class EntityAdapter : IEntityAdapter
    {
        private InternalEntityAdapter _internalEntityAdapter;
  
        public EntityAdapter()
            : this(new InternalEntityAdapter())
        {
        }

        public EntityAdapter(InternalEntityAdapter internalEntityAdapter)
        {
            _internalEntityAdapter = internalEntityAdapter;
            _internalEntityAdapter.EntityAdapterWrapper = this;
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
            get { return _internalEntityAdapter.Connection; }
            set { _internalEntityAdapter.Connection = value; }
        }

        /// <summary>
        /// Gets or sets whether the IEntityCache.AcceptChanges should be called during a call to IEntityAdapter.Update.
        /// </summary>
        public bool AcceptChangesDuringUpdate
        {
            get { return _internalEntityAdapter.AcceptChangesDuringUpdate; }
            set { _internalEntityAdapter.AcceptChangesDuringUpdate = value; }
        }

        /// <summary>
        /// Gets of sets the command timeout for update operations. If null, indicates that the default timeout
        /// for the provider should be used.
        /// </summary>
        Int32? IEntityAdapter.CommandTimeout { get; set; }

        /// <summary>
        /// Persist modifications described in the given cache.
        /// </summary>
        /// <param name="entityCache">Entity cache containing changes to persist to the store.</param>
        /// <returns>Number of cache entries affected by the udpate.</returns>
        public Int32 Update(IEntityStateManager entityCache)
        {
            Contract.Requires(entityCache != null);

            return _internalEntityAdapter.Update(entityCache);
        }
    }
}
