// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Services
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Diagnostics.Contracts;
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
            Contract.Requires(providerManifest != null);

            _providerManifest = providerManifest;
        }

        public DbDatabaseMapping Generate(EdmModel model)
        {
            Contract.Requires(model != null);

            var databaseMapping = InitializeDatabaseMapping(model);

            GenerateEntityTypes(model, databaseMapping);
            GenerateDiscriminators(databaseMapping);
            GenerateAssociationTypes(model, databaseMapping);

            return databaseMapping;
        }

        private static DbDatabaseMapping InitializeDatabaseMapping(EdmModel model)
        {
            Contract.Requires(model != null);

            var databaseMapping
                = new DbDatabaseMapping()
                    .Initialize(model, new EdmModel().DbInitialize(model.Version));

            databaseMapping.EntityContainerMappings.Single().EntityContainer = model.Containers.Single();

            return databaseMapping;
        }

        private void GenerateEntityTypes(EdmModel model, DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(model != null);
            Contract.Requires(databaseMapping != null);

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
            Contract.Requires(databaseMapping != null);

            foreach (var entitySetMapping in databaseMapping.GetEntitySetMappings())
            {
                if (entitySetMapping.EntityTypeMappings.Count == 1)
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
                    .TypeMappingFragments
                    .Single()
                    .Table
                    .AddColumn(discriminatorColumn);

                foreach (var entityTypeMapping in entitySetMapping.EntityTypeMappings)
                {
                    var entityTypeMappingFragment = entityTypeMapping.TypeMappingFragments.Single();

                    entityTypeMappingFragment.SetDefaultDiscriminator(discriminatorColumn);

                    entityTypeMappingFragment
                        .AddDiscriminatorCondition(discriminatorColumn, entityTypeMapping.EntityType.Name);
                }
            }
        }

        private void GenerateAssociationTypes(EdmModel model, DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(model != null);
            Contract.Requires(databaseMapping != null);

            foreach (var associationType in model.GetAssociationTypes())
            {
                new AssociationTypeMappingGenerator(_providerManifest)
                    .Generate(associationType, databaseMapping);
            }
        }
    }
}
