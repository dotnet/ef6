// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;

    internal static class ColumnMappingBuilderExtensions
    {
        public static void SyncNullabilityCSSpace(
            this ColumnMappingBuilder propertyMappingBuilder,
            DbDatabaseMapping databaseMapping,
            EntityType toTable)
        {
            DebugCheck.NotNull(propertyMappingBuilder);

            var property = propertyMappingBuilder.PropertyPath.Last();

            EntitySetMapping setMapping = null;

            var baseType = (EntityType)property.DeclaringType.BaseType;
            if (baseType != null)
            {
                setMapping = GetEntitySetMapping(databaseMapping, baseType);
            }

            while (baseType != null)
            {
                if (toTable == setMapping.EntityTypeMappings.First(m => m.EntityType == baseType).GetPrimaryTable())
                {
                    // CodePlex 2254: If current table is part of TPH mapping below the TPT mapping we are processing, then
                    // don't change the nullability because the TPH nullability calculated previously is still correct.
                    return;
                }

                baseType = (EntityType)baseType.BaseType;
            }

            propertyMappingBuilder.ColumnProperty.Nullable = property.Nullable;
        }

        private static EntitySetMapping GetEntitySetMapping(DbDatabaseMapping databaseMapping, EntityType cSpaceEntityType)
        {
            while (cSpaceEntityType.BaseType != null)
            {
                cSpaceEntityType = (EntityType)cSpaceEntityType.BaseType;
            }

            var cSpaceEntitySet = databaseMapping
                .Model
                .Container
                .EntitySets
                .First(s => s.ElementType == cSpaceEntityType);

            return databaseMapping
                .EntityContainerMappings
                .First()
                .EntitySetMappings
                .First(m => m.EntitySet == cSpaceEntitySet);
        }
    }
}
