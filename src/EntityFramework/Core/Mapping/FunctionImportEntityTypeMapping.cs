// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    /// Represents a function import entity type mapping.
    /// </summary>
    public sealed class FunctionImportEntityTypeMapping : FunctionImportStructuralTypeMapping
    {
        private readonly ReadOnlyCollection<EntityType> _entityTypes;
        private readonly ReadOnlyCollection<EntityType> _isOfTypeEntityTypes;
        private readonly ReadOnlyCollection<FunctionImportEntityTypeMappingCondition> _conditions;

        /// <summary>
        /// Initializes a new FunctionImportEntityTypeMapping instance.
        /// </summary>
        /// <param name="isOfTypeEntityTypes">The entity types at the base of 
        /// the type hierarchies to be mapped.</param>
        /// <param name="entityTypes">The entity types to be mapped.</param>
        /// <param name="properties">The property mappings for the result types of a function import.</param>
        /// <param name="conditions">The mapping conditions.</param>
        public FunctionImportEntityTypeMapping(
            IEnumerable<EntityType> isOfTypeEntityTypes,
            IEnumerable<EntityType> entityTypes, 
            Collection<FunctionImportReturnTypePropertyMapping> properties,
            IEnumerable<FunctionImportEntityTypeMappingCondition> conditions)
            : this(
                Check.NotNull(isOfTypeEntityTypes, "isOfTypeEntityTypes"), 
                Check.NotNull(entityTypes, "entityTypes"),
                Check.NotNull(conditions, "conditions"),
                Check.NotNull(properties, "properties"),
                LineInfo.Empty)
        {
        }

        internal FunctionImportEntityTypeMapping(
            IEnumerable<EntityType> isOfTypeEntityTypes,
            IEnumerable<EntityType> entityTypes, IEnumerable<FunctionImportEntityTypeMappingCondition> conditions,
            Collection<FunctionImportReturnTypePropertyMapping> columnsRenameList,
            LineInfo lineInfo)
            : base(columnsRenameList, lineInfo)
        {
            DebugCheck.NotNull(isOfTypeEntityTypes);
            DebugCheck.NotNull(entityTypes);
            DebugCheck.NotNull(conditions);

            _isOfTypeEntityTypes = new ReadOnlyCollection<EntityType>(isOfTypeEntityTypes.ToList());
            _entityTypes = new ReadOnlyCollection<EntityType>(entityTypes.ToList());
            _conditions = new ReadOnlyCollection<FunctionImportEntityTypeMappingCondition>(conditions.ToList());
        }

        /// <summary>
        /// Gets the entity types being mapped.
        /// </summary>
        public ReadOnlyCollection<EntityType> EntityTypes
        {
            get { return _entityTypes; } 
        }

        /// <summary>
        /// Gets the entity types at the base of the hierarchies being mapped.
        /// </summary>
        public ReadOnlyCollection<EntityType> IsOfTypeEntityTypes
        {
            get { return _isOfTypeEntityTypes; }
        }

        /// <summary>
        /// Gets the mapping conditions.
        /// </summary>
        public ReadOnlyCollection<FunctionImportEntityTypeMappingCondition> Conditions
        {
            get { return _conditions; }
        }

        // <summary>
        // Gets all (concrete) entity types implied by this type mapping.
        // </summary>
        internal IEnumerable<EntityType> GetMappedEntityTypes(ItemCollection itemCollection)
        {
            const bool includeAbstractTypes = false;
            return EntityTypes.Concat(
                IsOfTypeEntityTypes.SelectMany(
                    entityType =>
                    MetadataHelper.GetTypeAndSubtypesOf(entityType, itemCollection, includeAbstractTypes)
                                  .Cast<EntityType>()));
        }

        internal IEnumerable<String> GetDiscriminatorColumns()
        {
            return Conditions.Select(condition => condition.ColumnName);
        }
    }
}
