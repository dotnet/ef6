// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    /// Represents an Entity Data Model (EDM) created by the <see cref="DbModelBuilder" />.
    /// The Compile method can be used to go from this EDM representation to a <see cref="DbCompiledModel" />
    /// which is a compiled snapshot of the model suitable for caching and creation of
    /// <see cref="DbContext" /> or <see cref="T:System.Data.Objects.ObjectContext" /> instances.
    /// </summary>
#pragma warning disable 618
    public class DbModel : IEdmModelAdapter
#pragma warning restore 618
    {
        private readonly DbDatabaseMapping _databaseMapping;
        private readonly DbModelBuilder _cachedModelBuilder;

        // <summary>
        // Initializes a new instance of the <see cref="DbModel" /> class.
        // </summary>
        internal DbModel(DbDatabaseMapping databaseMapping, DbModelBuilder modelBuilder)
        {
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(modelBuilder);

            _databaseMapping = databaseMapping;
            _cachedModelBuilder = modelBuilder;
        }

        internal DbModel(DbProviderInfo providerInfo, DbProviderManifest providerManifest)
        {
            DebugCheck.NotNull(providerInfo);
            DebugCheck.NotNull(providerManifest);

            _databaseMapping = new DbDatabaseMapping().Initialize(
                EdmModel.CreateConceptualModel(),
                EdmModel.CreateStoreModel(providerInfo, providerManifest));
        }

        // <summary>
        // For test purpose only.
        // </summary>
        internal DbModel(EdmModel conceptualModel, EdmModel storeModel)
        {
            _databaseMapping = new DbDatabaseMapping { Model = conceptualModel, Database = storeModel };
        }

        /// <summary>
        /// Gets the provider information.
        /// </summary>
        public DbProviderInfo ProviderInfo
        {
            get { return StoreModel.ProviderInfo; }
        }

        /// <summary>
        /// Gets the provider manifest.
        /// </summary>
        public DbProviderManifest ProviderManifest
        {
            get { return StoreModel.ProviderManifest; }
        }

        /// <summary>
        /// Gets the conceptual model.
        /// </summary>
        public EdmModel ConceptualModel
        {
            get { return _databaseMapping.Model; }
        }

        /// <summary>
        /// Gets the store model.
        /// </summary>
        public EdmModel StoreModel
        {
            get { return _databaseMapping.Database; }
        }

        /// <summary>
        /// Gets the mapping model.
        /// </summary>
        public EntityContainerMapping ConceptualToStoreMapping
        {
            get { return _databaseMapping.EntityContainerMappings.SingleOrDefault(); }
        }

        // <summary>
        // A snapshot of the <see cref="DbModelBuilder" /> that was used to create this compiled model.
        // </summary>
        internal DbModelBuilder CachedModelBuilder
        {
            get { return _cachedModelBuilder; }
        }

        internal DbDatabaseMapping DatabaseMapping
        {
            get { return _databaseMapping; }
        }

        /// <summary>
        /// Creates a <see cref="DbCompiledModel" /> for this mode which is a compiled snapshot
        /// suitable for caching and creation of <see cref="DbContext" /> instances.
        /// </summary>
        /// <returns> The compiled model. </returns>
        public DbCompiledModel Compile()
        {
            return new DbCompiledModel(this);
        }
    }
}
