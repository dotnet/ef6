// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.MockingProxies
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    // <summary>
    // Acts as a proxy for <see cref="EntityConnection" /> that for the most part just passes calls
    // through to the real object but uses virtual methods/properties such that uses of the object
    // can be mocked.
    // </summary>
    internal class EntityConnectionProxy
    {
        private readonly EntityConnection _entityConnection;

        protected EntityConnectionProxy()
        {
        }

        public EntityConnectionProxy(EntityConnection entityConnection)
        {
            DebugCheck.NotNull(entityConnection);

            _entityConnection = entityConnection;
        }

        public static implicit operator EntityConnection(EntityConnectionProxy proxy)
        {
            return proxy._entityConnection;
        }

        public virtual DbConnection StoreConnection
        {
            get { return _entityConnection.StoreConnection; }
        }

        public virtual void Dispose()
        {
            _entityConnection.Dispose();
        }
        
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public virtual EntityConnectionProxy CreateNew(DbConnection storeConnection)
        {
            var clonedConnection = new EntityConnection(_entityConnection.GetMetadataWorkspace(), storeConnection);

            var currentTransaction = _entityConnection.CurrentTransaction;
            if (currentTransaction != null
                && DbInterception.Dispatch.Transaction.GetConnection(
                    currentTransaction.StoreTransaction, _entityConnection.InterceptionContext) == storeConnection)
            {
                clonedConnection.UseStoreTransaction(currentTransaction.StoreTransaction);
            }

            return new EntityConnectionProxy(clonedConnection);
        }
    }
}
