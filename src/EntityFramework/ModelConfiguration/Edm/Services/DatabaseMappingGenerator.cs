// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Services
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Data.Entity.Utilities;
    using System.Linq;

    internal class DatabaseMappingGenerator
    {
        private const string DiscriminatorColumnName = "Discriminator";
        public const int DiscriminatorMaxLength = 128;

        public static TypeUsage DiscriminatorTypeUsage
            = TypeUsage.CreateStringTypeUsage(
                PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String),
                isUnicode: true,
                isFixedLength: false,
                maxLength: DiscriminatorMaxLength);

        private readonly DbProviderManifest _providerManifest;

        public DatabaseMappingGenerator(DbProviderManifest providerManifest)
        {
            DebugCheck.NotNull(providerManifest);

            _providerManifest = providerManifest;
        }

        public DbDatabaseMapping Generate(EdmModel model)
        {
            DebugCheck.NotNull(model);

            var databaseMapping = InitializeDatabaseMapping(model);

            GenerateEntityTypes(model, databaseMapping);
            GenerateDiscriminators(databaseMapping);
            GenerateAssociationTypes(model, databaseMapping);

            return databaseMapping;
        }

        private static DbDatabaseMapping InitializeDatabaseMapping(EdmModel model)
        {
            DebugCheck.NotNull(model);

            var databaseMapping
                = new DbDatabaseMapping()
                    .Initialize(model, new EdmModel().DbInitialize(model.Version));

            return databaseMapping;
        }

        private void GenerateEntityTypes(EdmModel model, DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(databaseMapping);

            foreach (var entityType in model.GetEntityTypes())
            {
                if (!entityType.Abstract)
                {
                    new EntityTypeMappingGenerator(_providerManifest).
                        Generate(entityType, databaseMapping);
                }
            }
        }

        private void GenerateDiscriminators(DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(databaseMapping);

            foreach (var entitySetMapping in databaseMapping.GetEntitySetMappings())
            {
                if (entitySetMapping.EntityTypeMappings.Count() == 1)
                {
                    continue;
                }

                var typeUsage
                    = _providerManifest.GetStoreType(DiscriminatorTypeUsage);

                var discriminatorColumn
                    = new EdmProperty(DiscriminatorColumnName, typeUsage)
                          {
                              Nullable = false
                          };

                entitySetMapping
                    .EntityTypeMappings
                    .First()
                    .MappingFragments
                    .Single()
                    .Table
                    .AddColumn(discriminatorColumn);

                foreach (var entityTypeMapping in entitySetMapping.EntityTypeMappings)
                {
                    var entityTypeMappingFragment = entityTypeMapping.MappingFragments.Single();

                    entityTypeMappingFragment.SetDefaultDiscriminator(discriminatorColumn);

                    entityTypeMappingFragment
                        .AddDiscriminatorCondition(discriminatorColumn, entityTypeMapping.EntityType.Name);
                }
            }
        }

        private void GenerateAssociationTypes(EdmModel model, DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(databaseMapping);

            foreach (var associationType in model.GetAssociationTypes())
            {
                new AssociationTypeMappingGenerator(_providerManifest)
                    .Generate(associationType, databaseMapping);
            }
        }
    }
}
