// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    /// Convention to ensure an invalid/unsupported mapping is not created when mapping inherited properties
    /// </summary>
    public class MappingInheritedPropertiesSupportConvention : IDbMappingConvention
    {
        void IDbMappingConvention.Apply(DbDatabaseMapping databaseMapping)
        {
            Check.NotNull(databaseMapping, "databaseMapping");

            databaseMapping.EntityContainerMappings
                           .SelectMany(ecm => ecm.EntitySetMappings)
                           .Each(
                               esm =>
                                   {
                                       foreach (var etm in esm.EntityTypeMappings)
                                       {
                                           if (RemapsInheritedProperties(databaseMapping, etm)
                                               && HasBaseWithIsTypeOf(esm, etm.EntityType))
                                           {
                                               throw Error.UnsupportedHybridInheritanceMapping(etm.EntityType.Name);
                                           }
                                       }
                                   });
        }

        private static bool RemapsInheritedProperties(
            DbDatabaseMapping databaseMapping, EntityTypeMapping entityTypeMapping)
        {
            var inheritedProperties = entityTypeMapping.EntityType.Properties
                                                       .Except(entityTypeMapping.EntityType.DeclaredProperties)
                                                       .Except(entityTypeMapping.EntityType.GetKeyProperties());

            foreach (var property in inheritedProperties)
            {
                var fragment = GetFragmentForPropertyMapping(entityTypeMapping, property);

                if (fragment != null)
                {
                    // find if this inherited property is mapped to another table by a base type
                    var baseType = (EntityType)entityTypeMapping.EntityType.BaseType;
                    while (baseType != null)
                    {
                        if (databaseMapping.GetEntityTypeMappings(baseType)
                                           .Select(baseTypeMapping => GetFragmentForPropertyMapping(baseTypeMapping, property))
                                           .Any(
                                               baseFragment => baseFragment != null
                                                               && baseFragment.Table != fragment.Table))
                        {
                            return true;
                        }
                        baseType = (EntityType)baseType.BaseType;
                    }
                }
            }
            return false;
        }

        private static MappingFragment GetFragmentForPropertyMapping(
            EntityTypeMapping entityTypeMapping, EdmProperty property)
        {
            return entityTypeMapping.MappingFragments
                                    .SingleOrDefault(tmf => tmf.ColumnMappings.Any(pm => pm.PropertyPath.Last() == property));
        }

        private static bool HasBaseWithIsTypeOf(EntitySetMapping entitySetMapping, EntityType entityType)
        {
            var baseType = entityType.BaseType;

            while (baseType != null)
            {
                if (entitySetMapping.EntityTypeMappings
                                    .Where(etm => etm.EntityType == baseType)
                                    .Any(etm => etm.IsHierarchyMapping))
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            return false;
        }
    }
}
