// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.MockingProxies
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    /// <summary>
    ///     Acts as a proxy for <see cref="ObjectContext" /> that for the most part just passes calls
    ///     through to the real object but uses virtual methods/properties such that uses of the object
    ///     can be mocked.
    /// </summary>
    internal class ObjectContextProxy : IDisposable
    {
        private readonly ObjectContext _objectContext;
        private ObjectItemCollection _objectItemCollection;

        protected ObjectContextProxy()
        {
        }

        public ObjectContextProxy(ObjectContext objectContext)
        {
            Contract.Requires(objectContext != null);

            _objectContext = objectContext;
        }

        public static implicit operator ObjectContext(ObjectContextProxy proxy)
        {
            return proxy == null ? null : proxy._objectContext;
        }

        public virtual EntityConnectionProxy Connection
        {
            get { return new EntityConnectionProxy((EntityConnection)_objectContext.Connection); }
        }

        public virtual string DefaultContainerName
        {
            get { return _objectContext.DefaultContainerName; }
            set { _objectContext.DefaultContainerName = value; }
        }

        public virtual void Dispose()
        {
            _objectContext.Dispose();
        }

        public virtual IEnumerable<GlobalItem> GetObjectItemCollection()
        {
            return
                _objectItemCollection =
                (ObjectItemCollection)_objectContext.MetadataWorkspace.GetItemCollection(DataSpace.OSpace);
        }

        public virtual Type GetClrType(StructuralType item)
        {
            return _objectItemCollection.GetClrType(item);
        }

        public virtual Type GetClrType(EnumType item)
        {
            return _objectItemCollection.GetClrType(item);
        }

        public virtual void LoadFromAssembly(Assembly assembly)
        {
            _objectContext.MetadataWorkspace.LoadFromAssembly(assembly);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public virtual ObjectContextProxy CreateNew(EntityConnectionProxy entityConnection)
        {
            return new ObjectContextProxy(new ObjectContext(entityConnection));
        }

        public virtual void CopyContextOptions(ObjectContextProxy source)
        {
            _objectContext.ContextOptions.LazyLoadingEnabled = source._objectContext.ContextOptions.LazyLoadingEnabled;
            _objectContext.ContextOptions.ProxyCreationEnabled = source._objectContext.ContextOptions.ProxyCreationEnabled;
            _objectContext.ContextOptions.UseCSharpNullComparisonBehavior = source._objectContext.ContextOptions.UseCSharpNullComparisonBehavior;
            _objectContext.ContextOptions.UseConsistentNullReferenceBehavior = source._objectContext.ContextOptions.UseConsistentNullReferenceBehavior;
            _objectContext.ContextOptions.UseLegacyPreserveChangesBehavior = source._objectContext.ContextOptions.UseLegacyPreserveChangesBehavior;
        }
    }
}
