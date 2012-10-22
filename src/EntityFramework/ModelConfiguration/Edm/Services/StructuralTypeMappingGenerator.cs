// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Services
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Ssdl;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal abstract class StructuralTypeMappingGenerator
    {
        protected readonly DbProviderManifest _providerManifest;

        protected StructuralTypeMappingGenerator(DbProviderManifest providerManifest)
        {
            Contract.Requires(providerManifest != null);

            _providerManifest = providerManifest;
        }

        protected EdmProperty MapTableColumn(
            EdmProperty property,
            string columnName,
            bool isInstancePropertyOnDerivedType)
        {
            Contract.Requires(property != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(columnName));

            var underlyingTypeUsage
                = TypeUsage.Create(property.UnderlyingPrimitiveType, property.TypeUsage.Facets);

            var storeTypeUsage = _providerManifest.GetStoreType(underlyingTypeUsage);

            var tableColumnMetadata
                = new EdmProperty(columnName, storeTypeUsage)
                      {
                          Nullable = isInstancePropertyOnDerivedType || property.Nullable
                      };

            if (tableColumnMetadata.IsPrimaryKeyColumn)
            {
                tableColumnMetadata.Nullable = false;
            }

            var storeGeneratedPattern = property.GetStoreGeneratedPattern();

            if (storeGeneratedPattern != null)
            {
                tableColumnMetadata.StoreGeneratedPattern = storeGeneratedPattern.Value;
            }

            MapPrimitivePropertyFacets(property, tableColumnMetadata, storeTypeUsage);

            return tableColumnMetadata;
        }

        internal static void MapPrimitivePropertyFacets(
            EdmProperty property, EdmProperty column, TypeUsage typeUsage)
        {
            Contract.Requires(property != null);
            Contract.Requires(column != null);
            Contract.Requires(typeUsage != null);

            if (IsValidFacet(typeUsage, SsdlConstants.Attribute_FixedLength))
            {
                column.IsFixedLength = property.IsFixedLength;
            }

            if (IsValidFacet(typeUsage, SsdlConstants.Attribute_MaxLength))
            {
                column.IsMaxLength = property.IsMaxLength;
                column.MaxLength = property.MaxLength;
            }

            if (IsValidFacet(typeUsage, SsdlConstants.Attribute_Unicode))
            {
                column.IsUnicode = property.IsUnicode;
            }

            if (IsValidFacet(typeUsage, SsdlConstants.Attribute_Precision))
            {
                column.Precision = property.Precision;
            }

            if (IsValidFacet(typeUsage, SsdlConstants.Attribute_Scale))
            {
                column.Scale = property.Scale;
            }
        }

        private static bool IsValidFacet(TypeUsage typeUsage, string name)
        {
            Contract.Requires(typeUsage != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            Facet facet;

            return typeUsage.Facets.TryGetValue(name, false, out facet)
                   && !facet.Description.IsConstant;
        }

        protected static DbEntityTypeMapping GetEntityTypeMappingInHierarchy(
            DbDatabaseMapping databaseMapping, EntityType entityType)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(entityType != null);

            var entityTypeMapping = databaseMapping.GetEntityTypeMapping(entityType);

            if (entityTypeMapping == null)
            {
                var entitySetMapping =
                    databaseMapping.GetEntitySetMapping(databaseMapping.Model.GetEntitySet(entityType));

                if (entitySetMapping != null)
                {
                    entityTypeMapping = entitySetMapping
                        .EntityTypeMappings
                        .First(
                            etm => entityType.DeclaredProperties.All(
                                dp => etm.TypeMappingFragments.First()
                                          .PropertyMappings.Select(pm => pm.PropertyPath.First()).Contains(dp)));
                }
            }

            if (entityTypeMapping == null)
            {
                throw Error.UnmappedAbstractType(entityType.GetClrType());
            }

            return entityTypeMapping;
        }
    }
}
