// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact.SqlGen
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Text;

    internal static class TypeHelpers
    {
        #region facets

        internal static bool TryGetTypeFacetDescriptionByName(EdmType edmType, string facetName, out FacetDescription facetDescription)
        {
            facetDescription = null;
            var primitiveType = edmType as PrimitiveType;
            if (null != primitiveType)
            {
                foreach (var fd in primitiveType.FacetDescriptions)
                {
                    if (facetName.Equals(fd.FacetName, StringComparison.OrdinalIgnoreCase))
                    {
                        facetDescription = fd;
                        return true;
                    }
                }
            }
            return false;
        }

        internal static bool TryGetBooleanFacetValue(TypeUsage type, string facetName, out bool boolValue)
        {
            boolValue = false;
            Facet boolFacet;
            if (type.Facets.TryGetValue(facetName, false, out boolFacet)
                && boolFacet.Value != null
                && !Helper.IsUnboundedFacetValue(boolFacet))
            {
                boolValue = (bool)boolFacet.Value;
                return true;
            }

            return false;
        }

        internal static bool TryGetIntFacetValue(TypeUsage type, string facetName, out int intValue)
        {
            intValue = 0;
            Facet intFacet;
            if (type.Facets.TryGetValue(facetName, false, out intFacet)
                && intFacet.Value != null
                && !Helper.IsUnboundedFacetValue(intFacet))
            {
                intValue = (int)intFacet.Value;
                return true;
            }

            return false;
        }

        internal static bool TryGetByteFacetValue(TypeUsage type, string facetName, out byte byteValue)
        {
            byteValue = 0;
            Facet byteFacet;
            if (type.Facets.TryGetValue(facetName, false, out byteFacet)
                && byteFacet.Value != null
                && !Helper.IsUnboundedFacetValue(byteFacet))
            {
                byteValue = (byte)byteFacet.Value;
                return true;
            }

            return false;
        }

        internal static bool TryGetPrimitiveTypeKind(TypeUsage type, out PrimitiveTypeKind typeKind)
        {
            if (type != null
                && type.EdmType != null
                && type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
            {
                typeKind = ((PrimitiveType)type.EdmType).PrimitiveTypeKind;
                return true;
            }

            typeKind = default(PrimitiveTypeKind);
            return false;
        }

        internal static bool TryGetPrecision(TypeUsage type, out byte precision)
        {
            if (!TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Decimal))
            {
                precision = 0;
                return false;
            }

            return TryGetByteFacetValue(type, ProviderManifest.PrecisionFacetName, out precision);
        }

        internal static bool TryGetScale(TypeUsage type, out byte scale)
        {
            if (!TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Decimal))
            {
                scale = 0;
                return false;
            }

            return TryGetByteFacetValue(type, ProviderManifest.ScaleFacetName, out scale);
        }

        internal static bool TryGetIsFixedLength(TypeUsage type, out bool isFixedLength)
        {
            if (!TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.String)
                &&
                !TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Binary))
            {
                isFixedLength = false;
                return false;
            }

            // Binary and String MaxLength facets share the same name
            return TryGetBooleanFacetValue(type, ProviderManifest.FixedLengthFacetName, out isFixedLength);
        }

        internal static bool TryGetIsUnicode(TypeUsage type, out bool isUnicode)
        {
            isUnicode = true;
            if (!TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.String))
            {
                isUnicode = false;
                return false;
            }

            return true;
        }

        internal static bool IsFacetValueConstant(TypeUsage type, string facetName)
        {
            // Binary and String FixedLength facets share the same name
            return Helper.GetFacet(((PrimitiveType)type.EdmType).FacetDescriptions, facetName).IsConstant;
        }

        internal static bool TryGetMaxLength(TypeUsage type, out int maxLength)
        {
            if (!TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.String)
                &&
                !TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Binary))
            {
                maxLength = 0;
                return false;
            }

            // Binary and String FixedLength facets share the same name
            return TryGetIntFacetValue(type, ProviderManifest.MaxLengthFacetName, out maxLength);
        }

        // <summary>
        // It will return true if there there non-boolean facets types that
        // are nulled out.
        // This function needs to be removed till the
        // HasNulledOutFacetValues of TypeUsage class become public
        // </summary>
        internal static bool HasNulledOutFacetValues(TypeUsage type)
        {
            var primitiveType = GetEdmType<PrimitiveType>(type);
            var hasFacet = true;
            var maxLength = 0;
            byte decimalPrecision = 0;
            byte decimalScale = 0;
            switch (primitiveType.PrimitiveTypeKind)
            {
                case PrimitiveTypeKind.Binary:
                case PrimitiveTypeKind.String:
                    hasFacet = TryGetMaxLength(type, out maxLength);
                    break;
                case PrimitiveTypeKind.Decimal:
                    hasFacet = TryGetPrecision(type, out decimalPrecision);
                    if (hasFacet)
                    {
                        hasFacet = TryGetScale(type, out decimalScale);
                    }
                    break;
                default:
                    hasFacet = true;
                    break;
            }
            return !hasFacet;
        }

        // <summary>
        // Returns the name of Primitive Data Type
        // </summary>
        internal static string PrimitiveTypeName(TypeUsage type)
        {
            var primitiveType = GetEdmType<PrimitiveType>(type);
            var typeName = primitiveType.Name;
            return typeName;
        }

        #endregion

        internal static IList<EdmProperty> GetProperties(TypeUsage typeUsage)
        {
            return GetProperties(typeUsage.EdmType);
        }

        internal static IList<EdmProperty> GetProperties(EdmType edmType)
        {
            switch (edmType.BuiltInTypeKind)
            {
                case BuiltInTypeKind.ComplexType:
                    return ((ComplexType)edmType).Properties;
                case BuiltInTypeKind.EntityType:
                    return ((EntityType)edmType).Properties;
                case BuiltInTypeKind.RowType:
                    return ((RowType)edmType).Properties;
                default:
                    return EmptyArrayEdmProperty;
            }
        }

        internal static TypeUsage GetElementTypeUsage(TypeUsage type)
        {
            if (TypeSemantics.IsCollectionType(type))
            {
                return ((CollectionType)type.EdmType).TypeUsage;
            }
            if (TypeSemantics.IsReferenceType(type))
            {
                return TypeUsage.Create(((RefType)type.EdmType).ElementType, type.Facets);
            }
            return null;
        }

        // Function to generate a string containing complete information about type including its facet values.
        // The return value would be "FullName(<FacetName>=<FacetValue>)"
        internal static string GetIdentity(TypeUsage type)
        {
            var identityFacets = new[] { "DefaultValue", "FixedLength", "MaxLength", "Nullable", "Precision", "Scale", "Unicode" };
            var builder = new StringBuilder();
            builder.Append(type.EdmType.FullName);
            builder.Append("(");
            var flag = true;
            for (var i = 0; i < type.Facets.Count; i++)
            {
                var facet = type.Facets[i];
                if (facet.Value == null)
                {
                    continue;
                }
                if (0 <= Array.BinarySearch(identityFacets, facet.Name, StringComparer.Ordinal))
                {
                    if (flag)
                    {
                        flag = false;
                    }
                    else
                    {
                        builder.Append(",");
                    }
                    builder.Append(facet.Name);
                    builder.Append("=");
                    builder.Append(facet.Value ?? string.Empty);
                }
            }
            builder.Append(")");
            return builder.ToString();
        }

        #region Canonical Function Helpers

        internal static bool IsCanonicalFunction(EdmFunction function)
        {
            MetadataProperty dataSpace;

            if (function.MetadataProperties.TryGetValue("DataSpace", false, out dataSpace)
                &&
                null != dataSpace.Value)
            {
                return (DataSpace)dataSpace.Value == DataSpace.CSpace;
            }

            return false;
        }

        #endregion

        #region Type Extractors

        internal static TEdmType GetEdmType<TEdmType>(TypeUsage typeUsage)
            where TEdmType : EdmType
        {
            return (TEdmType)typeUsage.EdmType;
        }

        internal static bool TryGetEdmType<TEdmType>(TypeUsage typeUsage, out TEdmType type)
            where TEdmType : EdmType
        {
            type = typeUsage.EdmType as TEdmType;
            return (type != null);
        }

        #endregion

        internal static readonly EdmProperty[] EmptyArrayEdmProperty = new EdmProperty[0];
    }
}
