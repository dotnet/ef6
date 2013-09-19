// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Internal.MockingProxies;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    /// Encapsulates a cloned <see cref="ObjectContext" /> and store <see cref="DbConnection" />. Note that these
    /// objects are disposable and should be used in a using block to ensure both the cloned context and the
    /// cloned connection are disposed.
    /// </summary>
    internal class ClonedObjectContext : IDisposable
    {
        private ObjectContextProxy _objectContext;

        /// <summary>
        /// For mocking.
        /// </summary>
        protected ClonedObjectContext()
        {
        }

        /// <summary>
        /// Creates a clone of the given <see cref="ObjectContext" />. The underlying <see cref="DbConnection" /> of
        /// the context is also cloned and the given connection string is used for the connection string of
        /// the cloned connection.
        /// </summary>
        public ClonedObjectContext(
            ObjectContextProxy objectContext, string connectionString, bool transferLoadedAssemblies = true)
        {
            DebugCheck.NotNull(objectContext);
            // connectionString may be null when connection has been created from DbContextInfo using just a provider

            var clonedConnection =
                DbProviderServices.GetProviderFactory(objectContext.Connection.StoreConnection).CreateConnection();
            clonedConnection.ConnectionString = connectionString;

            var clonedEntityConnection = objectContext.Connection.CreateNew(clonedConnection);

            _objectContext = objectContext.CreateNew(clonedEntityConnection);
            _objectContext.CopyContextOptions(objectContext);

            if (!String.IsNullOrWhiteSpace(objectContext.DefaultContainerName))
            {
                _objectContext.DefaultContainerName = objectContext.DefaultContainerName;
            }

            if (transferLoadedAssemblies)
            {
                TransferLoadedAssemblies(objectContext);
            }
        }

        /// <summary>
        /// The cloned context.
        /// </summary>
        public virtual ObjectContextProxy ObjectContext
        {
            get { return _objectContext; }
        }

        /// <summary>
        /// This is always the store connection of the underlying ObjectContext.
        /// </summary>
        public virtual DbConnection Connection
        {
            get { return _objectContext.Connection.StoreConnection; }
        }

        /// <summary>
        /// Finds the assemblies that were used for loading o-space types in the source context
        /// and loads those assemblies in the cloned context.
        /// </summary>
        private void TransferLoadedAssemblies(ObjectContextProxy source)
        {
            DebugCheck.NotNull(source);

            var objectItemCollection = source.GetObjectItemCollection();

            var assemblies = objectItemCollection
                .Where(i => i is EntityType || i is ComplexType)
                .Select(i => source.GetClrType((StructuralType)i).Assembly)
                .Union(
                    objectItemCollection.OfType<EnumType>()
                                        .Select(i => source.GetClrType(i).Assembly))
                .Distinct();

            foreach (var assembly in assemblies)
            {
                _objectContext.LoadFromAssembly(assembly);
            }
        }

        /// <summary>
        /// Disposes both the underlying ObjectContext and its store connection.
        /// </summary>
        public void Dispose()
        {
            if (_objectContext != null)
            {
                var tempContext = _objectContext;
                var connection = Connection;

                _objectContext = null;

                tempContext.Dispose();
                connection.Dispose();
            }
        }
    }
}
