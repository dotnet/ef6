// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;

    // <summary>
    // Helps answer mapping questions since we don't have a good API for mapping information
    // </summary>
    internal static class MappingMetadataHelper
    {
        internal static IEnumerable<TypeMapping> GetMappingsForEntitySetAndType(
            StorageMappingItemCollection mappingCollection, EntityContainer container, EntitySetBase entitySet, EntityTypeBase entityType)
        {
            DebugCheck.NotNull(entityType);
            var containerMapping = GetEntityContainerMap(mappingCollection, container);
            var extentMap = containerMapping.GetSetMapping(entitySet.Name);

            //The Set may have no mapping
            if (extentMap != null)
            {
                //for each mapping fragment of Type we are interested in within the given set
                //Check use of IsOfTypes in Code review
                foreach (var typeMap in extentMap.TypeMappings.Where(map => map.Types.Union(map.IsOfTypes).Contains(entityType)))
                {
                    yield return typeMap;
                }
            }
        }

        // <summary>
        // Returns all mapping fragments for the given entity set's types and their parent types.
        // </summary>
        internal static IEnumerable<TypeMapping> GetMappingsForEntitySetAndSuperTypes(
            StorageMappingItemCollection mappingCollection, EntityContainer container, EntitySetBase entitySet,
            EntityTypeBase childEntityType)
        {
            return MetadataHelper.GetTypeAndParentTypesOf(childEntityType, true /*includeAbstractTypes*/).SelectMany(
                edmType =>
                    {
                        var entityTypeBase = edmType as EntityTypeBase;
                        return edmType.EdmEquals(childEntityType)
                                   ? GetMappingsForEntitySetAndType(mappingCollection, container, entitySet, entityTypeBase)
                                   : GetIsTypeOfMappingsForEntitySetAndType(
                                       mappingCollection, container, entitySet, entityTypeBase, childEntityType);
                    }).ToList();
        }

        // <summary>
        // Returns mappings for the given set/type only if the mapping applies also to childEntityType either via IsTypeOf or explicitly specifying multiple types in mapping fragments.
        // </summary>
        private static IEnumerable<TypeMapping> GetIsTypeOfMappingsForEntitySetAndType(
            StorageMappingItemCollection mappingCollection, EntityContainer container, EntitySetBase entitySet, EntityTypeBase entityType,
            EntityTypeBase childEntityType)
        {
            foreach (var mapping in GetMappingsForEntitySetAndType(mappingCollection, container, entitySet, entityType))
            {
                if (mapping.IsOfTypes.Any(parentType => parentType.IsAssignableFrom(childEntityType))
                    || mapping.Types.Contains(childEntityType))
                {
                    yield return mapping;
                }
            }
        }

        internal static IEnumerable<EntityTypeModificationFunctionMapping> GetModificationFunctionMappingsForEntitySetAndType(
            StorageMappingItemCollection mappingCollection, EntityContainer container, EntitySetBase entitySet, EntityTypeBase entityType)
        {
            var containerMapping = GetEntityContainerMap(mappingCollection, container);

            var extentMap = containerMapping.GetSetMapping(entitySet.Name);
            var entitySetMapping = extentMap as EntitySetMapping;

            //The Set may have no mapping
            if (entitySetMapping != null)
            {
                if (entitySetMapping != null) //could be association set mapping
                {
                    foreach (
                        var v in
                            entitySetMapping.ModificationFunctionMappings.Where(functionMap => functionMap.EntityType.Equals(entityType)))
                    {
                        yield return v;
                    }
                }
            }
        }

        internal static EntityContainerMapping GetEntityContainerMap(
            StorageMappingItemCollection mappingCollection, EntityContainer entityContainer)
        {
            var entityContainerMaps = mappingCollection.GetItems<EntityContainerMapping>();
            EntityContainerMapping entityContainerMap = null;
            foreach (var map in entityContainerMaps)
            {
                if ((entityContainer.Equals(map.EdmEntityContainer))
                    || (entityContainer.Equals(map.StorageEntityContainer)))
                {
                    entityContainerMap = map;
                    break;
                }
            }
            if (entityContainerMap == null)
            {
                throw new MappingException(Strings.Mapping_NotFound_EntityContainer(entityContainer.Name));
            }
            return entityContainerMap;
        }
    }
}
