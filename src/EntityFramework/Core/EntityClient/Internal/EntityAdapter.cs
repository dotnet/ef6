// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityClient.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Core.Mapping.Update.Internal;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal class EntityAdapter : IEntityAdapter
    {
        private bool _acceptChangesDuringUpdate = true;
        private EntityConnection _connection;
        private readonly ObjectContext _context;
        private readonly Func<EntityAdapter, UpdateTranslator> _updateTranslatorFactory;

        public EntityAdapter(ObjectContext context)
            : this(context, a => new UpdateTranslator(a))
        {
        }

        protected EntityAdapter(ObjectContext context, Func<EntityAdapter, UpdateTranslator> updateTranslatorFactory)
        {
            DebugCheck.NotNull(context);
            DebugCheck.NotNull(updateTranslatorFactory);

            _context = context;
            _updateTranslatorFactory = updateTranslatorFactory;
        }

        public ObjectContext Context
        {
            get { return _context; }
        }

        /// <summary>
        ///     Gets or sets the map connection used by this adapter.
        /// </summary>
        DbConnection IEntityAdapter.Connection
        {
            get { return Connection; }
            set { Connection = (EntityConnection)value; }
        }

        /// <summary>
        ///     Gets or sets the map connection used by this adapter.
        /// </summary>
        public EntityConnection Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        /// <summary>
        ///     Gets or sets whether the IEntityCache.AcceptChanges should be called during a call to IEntityAdapter.Update.
        /// </summary>
        public bool AcceptChangesDuringUpdate
        {
            get { return _acceptChangesDuringUpdate; }
            set { _acceptChangesDuringUpdate = value; }
        }

        /// <summary>
        ///     Gets of sets the command timeout for update operations. If null, indicates that the default timeout
        ///     for the provider should be used.
        /// </summary>
        public int? CommandTimeout { get; set; }

        public int Update(bool throwOnClosedConnection = true)
        {
            return Update(0, ut => ut.Update(), throwOnClosedConnection);
        }

#if !NET40

        public Task<int> UpdateAsync(CancellationToken cancellationToken)
        {
            return Update(Task.FromResult(0), ut => ut.UpdateAsync(cancellationToken), true);
        }

#endif

        private T Update<T>(
            T noChangesResult,
            Func<UpdateTranslator, T> updateFunction,
            bool throwOnClosedConnection)

        {
            if (!IsStateManagerDirty(_context.ObjectStateManager))
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
            if (throwOnClosedConnection
                && (ConnectionState.Open != _connection.State))
            {
                throw Error.EntityClient_ClosedConnectionForUpdate();
            }

            var updateTranslator = _updateTranslatorFactory(this);

            return updateFunction(updateTranslator);
        }

        /// <summary>
        ///     Determine whether the cache has changes to apply.
        /// </summary>
        /// <param name="entityCache"> ObjectStateManager to check. Must not be null. </param>
        /// <returns> true if cache contains changes entries; false otherwise </returns>
        private static bool IsStateManagerDirty(IEntityStateManager entityCache)
        {
            DebugCheck.NotNull(entityCache);

            // this call to GetCacheEntries is constant time (the ObjectStateManager implementation
            // maintains an explicit list of entries in each state)
            return entityCache.GetEntityStateEntries(EntityState.Added | EntityState.Deleted | EntityState.Modified).Any();
        }
    }
}
