// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Services
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
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

        private readonly DbProviderInfo _providerInfo;
        private readonly DbProviderManifest _providerManifest;

        public DatabaseMappingGenerator(DbProviderInfo providerInfo, DbProviderManifest providerManifest)
        {
            DebugCheck.NotNull(providerInfo);
            DebugCheck.NotNull(providerManifest);

            _providerInfo = providerInfo;
            _providerManifest = providerManifest;
        }

        public DbDatabaseMapping Generate(EdmModel conceptualModel)
        {
            DebugCheck.NotNull(conceptualModel);

            var databaseMapping = InitializeDatabaseMapping(conceptualModel);

            GenerateEntityTypes(databaseMapping);
            GenerateDiscriminators(databaseMapping);
            GenerateAssociationTypes(databaseMapping);

            return databaseMapping;
        }

        private DbDatabaseMapping InitializeDatabaseMapping(EdmModel conceptualModel)
        {
            DebugCheck.NotNull(conceptualModel);

            var storeModel = EdmModel.CreateStoreModel(
                _providerInfo, _providerManifest, conceptualModel.SchemaVersion);

            return new DbDatabaseMapping().Initialize(conceptualModel, storeModel);
        }

        private static void GenerateEntityTypes(DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(databaseMapping);

            foreach (var entityType in databaseMapping.Model.EntityTypes)
            {
                if (entityType.Abstract
                    && databaseMapping.Model.EntityTypes.All(e => e.BaseType != entityType))
                {
                    throw new InvalidOperationException(Strings.UnmappedAbstractType(entityType.GetClrType()));
                }

                new TableMappingGenerator(databaseMapping.ProviderManifest).
                    Generate(entityType, databaseMapping);
            }
        }

        private static void GenerateDiscriminators(DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(databaseMapping);

            foreach (var entitySetMapping in databaseMapping.GetEntitySetMappings())
            {
                if (entitySetMapping.EntityTypeMappings.Count() <= 1)
                {
                    continue;
                }

                var typeUsage
                    = databaseMapping.ProviderManifest.GetStoreType(DiscriminatorTypeUsage);

                var discriminatorColumn
                    = new EdmProperty(DiscriminatorColumnName, typeUsage)
                        {
                            Nullable = false,
                            DefaultValue = "(Undefined)"
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
                    // Abstract classes don't need a discriminator as they won't be directly materialized
                    if (entityTypeMapping.EntityType.Abstract)
                    {
                        continue;
                    }

                    var entityTypeMappingFragment = entityTypeMapping.MappingFragments.Single();

                    entityTypeMappingFragment.SetDefaultDiscriminator(discriminatorColumn);

                    entityTypeMappingFragment
                        .AddDiscriminatorCondition(discriminatorColumn, entityTypeMapping.EntityType.Name);
                }
            }
        }

        private static void GenerateAssociationTypes(DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(databaseMapping);

            foreach (var associationType in databaseMapping.Model.AssociationTypes)
            {
                new AssociationTypeMappingGenerator(databaseMapping.ProviderManifest)
                    .Generate(associationType, databaseMapping);
            }
        }
    }
}
