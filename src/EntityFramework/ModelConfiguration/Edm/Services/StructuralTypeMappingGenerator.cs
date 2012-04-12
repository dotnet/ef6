namespace System.Data.Entity.ModelConfiguration.Edm.Services
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Ssdl;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using EdmProperty = System.Data.Entity.Edm.EdmProperty;

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

            var storeTypeUsage = _providerManifest.GetStoreType(GetEdmTypeUsage(property.PropertyType));

            tableColumnMetadata.TypeName = storeTypeUsage.EdmType.Name;
            tableColumnMetadata.IsPrimaryKeyColumn = isKeyProperty;

            if (isInstancePropertyOnDerivedType)
            {
                tableColumnMetadata.IsNullable = true;
            }
            else if (property.PropertyType.IsNullable != null)
            {
                tableColumnMetadata.IsNullable = property.PropertyType.IsNullable.Value;
            }

            if (tableColumnMetadata.IsPrimaryKeyColumn)
            {
                tableColumnMetadata.IsNullable = false;
            }

            var storeGeneratedPattern = property.GetStoreGeneratedPattern();

            if (storeGeneratedPattern != null)
            {
                tableColumnMetadata.StoreGeneratedPattern = storeGeneratedPattern.Value;
            }

            MapPrimitivePropertyFacets(
                property.PropertyType.PrimitiveTypeFacets, tableColumnMetadata.Facets, storeTypeUsage);
        }

        private static TypeUsage GetEdmTypeUsage(EdmTypeReference edmTypeReference)
        {
            Contract.Requires(edmTypeReference != null);

            var primitiveTypeFacets = edmTypeReference.PrimitiveTypeFacets;

            var primitiveTypeKind = edmTypeReference.UnderlyingPrimitiveType.PrimitiveTypeKind;

            var primitiveType = PrimitiveType.GetEdmPrimitiveType((PrimitiveTypeKind)primitiveTypeKind);

            if (edmTypeReference.PrimitiveType
                == EdmPrimitiveType.String)
            {
                if ((primitiveTypeFacets.IsUnicode != null)
                    && (primitiveTypeFacets.IsFixedLength != null))
                {
                    return (primitiveTypeFacets.MaxLength != null)
                               ? TypeUsage.CreateStringTypeUsage(
                                   primitiveType,
                                   primitiveTypeFacets.IsUnicode.Value,
                                   primitiveTypeFacets.IsFixedLength.Value,
                                   primitiveTypeFacets.MaxLength.Value)
                               : TypeUsage.CreateStringTypeUsage(
                                   primitiveType,
                                   primitiveTypeFacets.IsUnicode.Value,
                                   primitiveTypeFacets.IsFixedLength.Value);
                }
            }

            if (edmTypeReference.PrimitiveType
                == EdmPrimitiveType.Binary)
            {
                if (primitiveTypeFacets.IsFixedLength != null)
                {
                    return (primitiveTypeFacets.MaxLength != null)
                               ? TypeUsage.CreateBinaryTypeUsage(
                                   primitiveType,
                                   primitiveTypeFacets.IsFixedLength.Value,
                                   primitiveTypeFacets.MaxLength.Value)
                               : TypeUsage.CreateBinaryTypeUsage(
                                   primitiveType,
                                   primitiveTypeFacets.IsFixedLength.Value);
                }
            }

            if ((edmTypeReference.PrimitiveType == EdmPrimitiveType.Time)
                && (primitiveTypeFacets.Precision != null))
            {
                return TypeUsage.CreateTimeTypeUsage(
                    primitiveType,
                    primitiveTypeFacets.Precision);
            }

            if ((edmTypeReference.PrimitiveType == EdmPrimitiveType.DateTime)
                && (primitiveTypeFacets.Precision != null))
            {
                return TypeUsage.CreateDateTimeTypeUsage(
                    primitiveType,
                    primitiveTypeFacets.Precision);
            }

            if ((edmTypeReference.PrimitiveType == EdmPrimitiveType.DateTimeOffset)
                && (primitiveTypeFacets.Precision != null))
            {
                return TypeUsage.CreateDateTimeOffsetTypeUsage(
                    primitiveType,
                    primitiveTypeFacets.Precision);
            }

            if (edmTypeReference.PrimitiveType
                == EdmPrimitiveType.Decimal)
            {
                return ((primitiveTypeFacets.Precision != null)
                        && primitiveTypeFacets.Scale != null)
                           ? TypeUsage.CreateDecimalTypeUsage(
                               primitiveType,
                               primitiveTypeFacets.Precision.Value,
                               primitiveTypeFacets.Scale.Value)
                           : TypeUsage.CreateDecimalTypeUsage(primitiveType);
            }

            return TypeUsage.CreateDefaultTypeUsage(primitiveType);
        }

        internal static void MapPrimitivePropertyFacets(
            EdmPrimitiveTypeFacets primitiveTypeFacets, DbPrimitiveTypeFacets dbPrimitiveTypeFacets, TypeUsage typeUsage)
        {
            Contract.Requires(primitiveTypeFacets != null);
            Contract.Requires(dbPrimitiveTypeFacets != null);
            Contract.Requires(typeUsage != null);

            if (IsValidFacet(typeUsage, SsdlConstants.Attribute_FixedLength))
            {
                dbPrimitiveTypeFacets.IsFixedLength = primitiveTypeFacets.IsFixedLength;
            }

            if (IsValidFacet(typeUsage, SsdlConstants.Attribute_MaxLength))
            {
                dbPrimitiveTypeFacets.IsMaxLength = primitiveTypeFacets.IsMaxLength;
                dbPrimitiveTypeFacets.MaxLength = primitiveTypeFacets.MaxLength;
            }

            if (IsValidFacet(typeUsage, SsdlConstants.Attribute_Unicode))
            {
                dbPrimitiveTypeFacets.IsUnicode = primitiveTypeFacets.IsUnicode;
            }

            if (IsValidFacet(typeUsage, SsdlConstants.Attribute_Precision))
            {
                dbPrimitiveTypeFacets.Precision = primitiveTypeFacets.Precision;
            }

            if (IsValidFacet(typeUsage, SsdlConstants.Attribute_Scale))
            {
                dbPrimitiveTypeFacets.Scale = primitiveTypeFacets.Scale;
            }

            if (IsValidFacet(typeUsage, SsdlConstants.Attribute_Srid))
            {
                dbPrimitiveTypeFacets.IsVariableSrid = primitiveTypeFacets.IsVariableSrid;
                dbPrimitiveTypeFacets.Srid = primitiveTypeFacets.Srid;
            }

            if (IsValidFacet(typeUsage, SsdlConstants.Attribute_IsStrict))
            {
                dbPrimitiveTypeFacets.IsStrict = primitiveTypeFacets.IsStrict;
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
            DbDatabaseMapping databaseMapping, EdmEntityType entityType)
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
