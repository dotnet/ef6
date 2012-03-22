namespace System.Data.Entity.ModelConfiguration.Edm.Services
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal class EntityTypeMappingGenerator : StructuralTypeMappingGenerator
    {
        public EntityTypeMappingGenerator(DbProviderManifest providerManifest)
            : base(providerManifest)
        {
        }

        public void Generate(EdmEntityType entityType, DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(entityType != null);
            Contract.Requires(databaseMapping != null);

            var entitySet = databaseMapping.Model.GetEntitySet(entityType);

            var entitySetMapping
                = databaseMapping.GetEntitySetMapping(entitySet)
                  ?? databaseMapping.AddEntitySetMapping(entitySet);

            var table
                = entitySetMapping.EntityTypeMappings.Any()
                      ? entitySetMapping.EntityTypeMappings.First().TypeMappingFragments.First().Table
                      : databaseMapping.Database.AddTable(entityType.GetRootType().Name);

            var entityTypeMappingFragment = new DbEntityTypeMappingFragment
                                                {
                                                    Table = table
                                                };

            var entityTypeMapping = new DbEntityTypeMapping
                                        {
                                            EntityType = entityType,
                                            IsHierarchyMapping = false
                                        };
            entityTypeMapping.TypeMappingFragments.Add(entityTypeMappingFragment);
            entityTypeMapping.SetClrType(entityType.GetClrType());

            entitySetMapping.EntityTypeMappings.Add(entityTypeMapping);

            new PropertyMappingGenerator(_providerManifest)
                .Generate(
                    entityType,
                    entityType.Properties,
                    entitySetMapping,
                    entityTypeMappingFragment,
                    new List<EdmProperty>(),
                    false);
        }
    }
}
