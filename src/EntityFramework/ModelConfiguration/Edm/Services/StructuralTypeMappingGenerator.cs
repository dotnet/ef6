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

        protected void MapTableColumn(
            EdmProperty property,
            DbTableColumnMetadata tableColumnMetadata,
            bool isInstancePropertyOnDerivedType,
            bool isKeyProperty = false)
        {
            Contract.Requires(property != null);
            Contract.Requires(tableColumnMetadata != null);

            var storeTypeUsage = _providerManifest.GetStoreType(GetEdmTypeUsage(property));

            tableColumnMetadata.TypeName = storeTypeUsage.EdmType.Name;
            tableColumnMetadata.IsPrimaryKeyColumn = isKeyProperty;
            tableColumnMetadata.IsNullable = isInstancePropertyOnDerivedType || property.Nullable;

            if (tableColumnMetadata.IsPrimaryKeyColumn)
            {
                tableColumnMetadata.IsNullable = false;
            }

            var storeGeneratedPattern = property.GetStoreGeneratedPattern();

            if (storeGeneratedPattern != null)
            {
                tableColumnMetadata.StoreGeneratedPattern = storeGeneratedPattern.Value;
            }

            MapPrimitivePropertyFacets(property, tableColumnMetadata.Facets, storeTypeUsage);
        }

        private static TypeUsage GetEdmTypeUsage(EdmProperty property)
        {
            Contract.Requires(property != null);

            var primitiveTypeKind = property.UnderlyingPrimitiveType.PrimitiveTypeKind;

            var primitiveType = PrimitiveType.GetEdmPrimitiveType(primitiveTypeKind);

            if (property.PrimitiveType
                == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))
            {
                if ((property.IsUnicode != null)
                    && (property.IsFixedLength != null))
                {
                    return (property.MaxLength != null)
                               ? TypeUsage.CreateStringTypeUsage(
                                   primitiveType,
                                   property.IsUnicode.Value,
                                   property.IsFixedLength.Value,
                                   property.MaxLength.Value)
                               : TypeUsage.CreateStringTypeUsage(
                                   primitiveType,
                                   property.IsUnicode.Value,
                                   property.IsFixedLength.Value);
                }
            }

            if (property.PrimitiveType
                == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary))
            {
                if (property.IsFixedLength != null)
                {
                    return (property.MaxLength != null)
                               ? TypeUsage.CreateBinaryTypeUsage(
                                   primitiveType,
                                   property.IsFixedLength.Value,
                                   property.MaxLength.Value)
                               : TypeUsage.CreateBinaryTypeUsage(
                                   primitiveType,
                                   property.IsFixedLength.Value);
                }
            }

            if ((property.PrimitiveType == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Time))
                && (property.Precision != null))
            {
                return TypeUsage.CreateTimeTypeUsage(
                    primitiveType,
                    property.Precision);
            }

            if ((property.PrimitiveType == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.DateTime))
                && (property.Precision != null))
            {
                return TypeUsage.CreateDateTimeTypeUsage(
                    primitiveType,
                    property.Precision);
            }

            if ((property.PrimitiveType == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.DateTimeOffset))
                && (property.Precision != null))
            {
                return TypeUsage.CreateDateTimeOffsetTypeUsage(
                    primitiveType,
                    property.Precision);
            }

            if (property.PrimitiveType
                == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Decimal))
            {
                return ((property.Precision != null)
                        && property.Scale != null)
                           ? TypeUsage.CreateDecimalTypeUsage(
                               primitiveType,
                               property.Precision.Value,
                               property.Scale.Value)
                           : TypeUsage.CreateDecimalTypeUsage(primitiveType);
            }

            return TypeUsage.CreateDefaultTypeUsage(primitiveType);
        }

        internal static void MapPrimitivePropertyFacets(
            EdmProperty property, DbPrimitiveTypeFacets dbPrimitiveTypeFacets, TypeUsage typeUsage)
        {
            Contract.Requires(property != null);
            Contract.Requires(dbPrimitiveTypeFacets != null);
            Contract.Requires(typeUsage != null);

            if (IsValidFacet(typeUsage, SsdlConstants.Attribute_FixedLength))
            {
                dbPrimitiveTypeFacets.IsFixedLength = property.IsFixedLength;
            }

            if (IsValidFacet(typeUsage, SsdlConstants.Attribute_MaxLength))
            {
                dbPrimitiveTypeFacets.IsMaxLength = property.IsMaxLength;
                dbPrimitiveTypeFacets.MaxLength = property.MaxLength;
            }

            if (IsValidFacet(typeUsage, SsdlConstants.Attribute_Unicode))
            {
                dbPrimitiveTypeFacets.IsUnicode = property.IsUnicode;
            }

            if (IsValidFacet(typeUsage, SsdlConstants.Attribute_Precision))
            {
                dbPrimitiveTypeFacets.Precision = property.Precision;
            }

            if (IsValidFacet(typeUsage, SsdlConstants.Attribute_Scale))
            {
                dbPrimitiveTypeFacets.Scale = property.Scale;
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
