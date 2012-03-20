namespace System.Data.Entity.Internal.MockingProxies
{
    using System.Data.Common;
    using System.Data.EntityClient;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Acts as a proxy for <see cref="EntityConnection"/> that for the most part just passes calls
    /// through to the real object but uses virtual methods/properties such that uses of the object
    /// can be mocked.
    /// </summary>
    internal class EntityConnectionProxy
    {
        private readonly EntityConnection _entityConnection;

        protected EntityConnectionProxy()
        {
        }

        public EntityConnectionProxy(EntityConnection entityConnection)
        {
            Contract.Requires(entityConnection != null);

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

        public virtual EntityConnectionProxy CreateNew(DbConnection storeConnection)
        {
            return new EntityConnectionProxy(new EntityConnection(_entityConnection.GetMetadataWorkspace(), storeConnection));
        }
    }
}