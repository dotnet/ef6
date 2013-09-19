// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Services
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;

    internal class PropertyMappingGenerator : StructuralTypeMappingGenerator
    {
        public PropertyMappingGenerator(DbProviderManifest providerManifest)
            : base(providerManifest)
        {
        }

        public void Generate(
            EntityType entityType,
            IEnumerable<EdmProperty> properties,
            EntitySetMapping entitySetMapping,
            MappingFragment entityTypeMappingFragment,
            IList<EdmProperty> propertyPath,
            bool createNewColumn)
        {
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(properties);
            DebugCheck.NotNull(entityTypeMappingFragment);
            DebugCheck.NotNull(propertyPath);

            var rootDeclaredProperties = entityType.GetRootType().DeclaredProperties;

            foreach (var property in properties)
            {
                if (property.IsComplexType
                    && propertyPath.Any(
                        p => p.IsComplexType
                             && (p.ComplexType == property.ComplexType)))
                {
                    throw Error.CircularComplexTypeHierarchy();
                }

                propertyPath.Add(property);

                if (property.IsComplexType)
                {
                    Generate(
                        entityType,
                        property.ComplexType.Properties,
                        entitySetMapping,
                        entityTypeMappingFragment,
                        propertyPath,
                        createNewColumn);
                }
                else
                {
                    var tableColumn
                        = entitySetMapping.EntityTypeMappings
                                          .SelectMany(etm => etm.MappingFragments)
                                          .SelectMany(etmf => etmf.ColumnMappings)
                                          .Where(pm => pm.PropertyPath.SequenceEqual(propertyPath))
                                          .Select(pm => pm.ColumnProperty)
                                          .FirstOrDefault();

                    if (tableColumn == null || createNewColumn)
                    {
                        var columnName
                            = string.Join("_", propertyPath.Select(p => p.Name));

                        tableColumn
                            = MapTableColumn(
                                property,
                                columnName,
                                !rootDeclaredProperties.Contains(propertyPath.First()));

                        entityTypeMappingFragment.Table.AddColumn(tableColumn);

                        if (entityType.KeyProperties().Contains(property))
                        {
                            entityTypeMappingFragment.Table.AddKeyMember(tableColumn);
                        }
                    }

                    entityTypeMappingFragment.AddColumnMapping(
                        new ColumnMappingBuilder(tableColumn, propertyPath.ToList()));
                }

                propertyPath.Remove(property);
            }
        }
    }
}
