namespace System.Data.Entity.ModelConfiguration.Edm.Services
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal class PropertyMappingGenerator : StructuralTypeMappingGenerator
    {
        public PropertyMappingGenerator(DbProviderManifest providerManifest)
            : base(providerManifest)
        {
        }

        public void Generate(
            EdmEntityType entityType,
            IEnumerable<EdmProperty> properties,
            DbEntitySetMapping entitySetMapping,
            DbEntityTypeMappingFragment entityTypeMappingFragment,
            IList<EdmProperty> propertyPath,
            bool createNewColumn)
        {
            Contract.Requires(entityType != null);
            Contract.Requires(properties != null);
            Contract.Requires(entityTypeMappingFragment != null);
            Contract.Requires(propertyPath != null);

            var rootDeclaredProperties = entityType.GetRootType().DeclaredProperties;

            foreach (var property in properties)
            {
                if (property.PropertyType.IsComplexType
                    && propertyPath.Any(
                        p => p.PropertyType.IsComplexType
                             && (p.PropertyType.ComplexType == property.PropertyType.ComplexType)))
                {
                    throw Error.CircularComplexTypeHierarchy();
                }

                propertyPath.Add(property);

                if (property.PropertyType.IsComplexType)
                {
                    Generate(
                        entityType,
                        property.PropertyType.ComplexType.DeclaredProperties,
                        entitySetMapping,
                        entityTypeMappingFragment,
                        propertyPath,
                        createNewColumn);
                }
                else
                {
                    var tableColumn
                        = entitySetMapping.EntityTypeMappings
                            .SelectMany(etm => etm.TypeMappingFragments)
                            .SelectMany(etmf => etmf.PropertyMappings)
                            .Where(pm => pm.PropertyPath.SequenceEqual(propertyPath))
                            .Select(pm => pm.Column)
                            .FirstOrDefault();

                    if (tableColumn == null || createNewColumn)
                    {
                        tableColumn = entityTypeMappingFragment.Table.AddColumn(
                            string.Join("_", propertyPath.Select(p => p.Name)));

                        MapTableColumn(
                            property,
                            tableColumn,
                            !rootDeclaredProperties.Contains(propertyPath.First()),
                            entityType.KeyProperties().Contains(property));
                    }

                    entityTypeMappingFragment.PropertyMappings.Add(
                        new DbEdmPropertyMapping
                            {
                                Column = tableColumn,
                                PropertyPath = propertyPath.ToList()
                            });
                }

                propertyPath.Remove(property);
            }
        }
    }
}
