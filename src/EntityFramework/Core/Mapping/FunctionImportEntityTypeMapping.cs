// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal sealed class FunctionImportEntityTypeMapping : FunctionImportStructuralTypeMapping
    {
        internal FunctionImportEntityTypeMapping(
            IEnumerable<EntityType> isOfTypeEntityTypes,
            IEnumerable<EntityType> entityTypes, IEnumerable<FunctionImportEntityTypeMappingCondition> conditions,
            Collection<FunctionImportReturnTypePropertyMapping> columnsRenameList,
            LineInfo lineInfo)
            : base(columnsRenameList, lineInfo)
        {
            Contract.Requires(isOfTypeEntityTypes != null);
            Contract.Requires(entityTypes != null);
            Contract.Requires(conditions != null);

            IsOfTypeEntityTypes = new ReadOnlyCollection<EntityType>(isOfTypeEntityTypes.ToList());
            EntityTypes = new ReadOnlyCollection<EntityType>(entityTypes.ToList());
            Conditions = new ReadOnlyCollection<FunctionImportEntityTypeMappingCondition>(conditions.ToList());
        }

        internal readonly ReadOnlyCollection<FunctionImportEntityTypeMappingCondition> Conditions;
        internal readonly ReadOnlyCollection<EntityType> EntityTypes;
        internal readonly ReadOnlyCollection<EntityType> IsOfTypeEntityTypes;

        /// <summary>
        /// Gets all (concrete) entity types implied by this type mapping.
        /// </summary>
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
