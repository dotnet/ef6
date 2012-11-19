// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.Utilities
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;

    internal static class TypeUsageExtensions
    {
        internal static byte GetPrecision(this TypeUsage type)
        {
            DebugCheck.NotNull(type);

            return type.GetFacetValue<byte>(DbProviderManifest.PrecisionFacetName);
        }

        internal static byte GetScale(this TypeUsage type)
        {
            DebugCheck.NotNull(type);

            return type.GetFacetValue<byte>(DbProviderManifest.ScaleFacetName);
        }

        internal static int GetMaxLength(this TypeUsage type)
        {
            DebugCheck.NotNull(type);

            return type.GetFacetValue<int>(DbProviderManifest.MaxLengthFacetName);
        }

        internal static T GetFacetValue<T>(this TypeUsage type, string facetName)
        {
            DebugCheck.NotNull(type);

            return (T)type.Facets[facetName].Value;
        }

        internal static bool IsFixedLength(this TypeUsage type)
        {
            DebugCheck.NotNull(type);

            var facet = type.Facets.SingleOrDefault(f => f.Name == DbProviderManifest.FixedLengthFacetName);
            return facet != null && facet.Value != null && (bool)facet.Value;
        }

        internal static bool TryGetPrecision(this TypeUsage type, out byte precision)
        {
            // Null type okay--returns false

            if (!IsPrimitiveType(type, PrimitiveTypeKind.Decimal))
            {
                precision = 0;
                return false;
            }

            return TryGetFacetValue(type, DbProviderManifest.PrecisionFacetName, out precision);
        }

        internal static bool TryGetScale(this TypeUsage type, out byte scale)
        {
            // Null type okay--returns false

            if (!IsPrimitiveType(type, PrimitiveTypeKind.Decimal))
            {
                scale = 0;
                return false;
            }

            return TryGetFacetValue(type, DbProviderManifest.ScaleFacetName, out scale);
        }

        internal static bool TryGetFacetValue<T>(this TypeUsage type, string facetName, out T value)
        {
            DebugCheck.NotNull(type);

            value = default(T);
            Facet facet;
            if (type.Facets.TryGetValue(facetName, false, out facet)
                && facet.Value is T)
            {
                value = (T)facet.Value;
                return true;
            }

            return false;
        }

        internal static bool IsPrimitiveType(this TypeUsage type, PrimitiveTypeKind primitiveTypeKind)
        {
            // Null type okay--returns false

            return type.IsPrimitiveType() && ((PrimitiveType)type.EdmType).PrimitiveTypeKind == primitiveTypeKind;
        }

        internal static bool IsPrimitiveType(this TypeUsage type)
        {
            // Null type okay--returns false

            return type != null && type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType;
        }

        internal static bool IsNullable(this TypeUsage type)
        {
            DebugCheck.NotNull(type);

            var facet = type.Facets.SingleOrDefault(f => f.Name == DbProviderManifest.NullableFacetName);
            return facet != null && facet.Value != null && (bool)facet.Value;
        }

        internal static PrimitiveTypeKind GetPrimitiveTypeKind(this TypeUsage type)
        {
            DebugCheck.NotNull(type);

            return ((PrimitiveType)type.EdmType).PrimitiveTypeKind;
        }

        internal static bool TryGetIsUnicode(this TypeUsage type, out bool isUnicode)
        {
            // Null type okay--returns false

            if (!IsPrimitiveType(type, PrimitiveTypeKind.String))
            {
                isUnicode = false;
                return false;
            }

            return TryGetFacetValue(type, DbProviderManifest.UnicodeFacetName, out isUnicode);
        }

        internal static bool TryGetMaxLength(this TypeUsage type, out int maxLength)
        {
            // Null type okay--returns false

            if (!IsPrimitiveType(type, PrimitiveTypeKind.String)
                && !IsPrimitiveType(type, PrimitiveTypeKind.Binary))
            {
                maxLength = 0;
                return false;
            }

            // Binary and String FixedLength facets share the same name
            return TryGetFacetValue(type, DbProviderManifest.MaxLengthFacetName, out maxLength);
        }

        internal static IEnumerable<EdmProperty> GetProperties(this TypeUsage type)
        {
            DebugCheck.NotNull(type);

            var edmType = type.EdmType;
            switch (edmType.BuiltInTypeKind)
            {
                case BuiltInTypeKind.ComplexType:
                    return ((ComplexType)edmType).Properties;
                case BuiltInTypeKind.EntityType:
                    return ((EntityType)edmType).Properties;
                case BuiltInTypeKind.RowType:
                    return ((RowType)edmType).Properties;
                default:
                    return Enumerable.Empty<EdmProperty>();
            }
        }

        internal static TypeUsage GetElementTypeUsage(this TypeUsage type)
        {
            DebugCheck.NotNull(type);

            var edmType = type.EdmType;

            if (BuiltInTypeKind.CollectionType
                == edmType.BuiltInTypeKind)
            {
                return ((CollectionType)edmType).TypeUsage;
            }

            if (BuiltInTypeKind.RefType
                == edmType.BuiltInTypeKind)
            {
                return TypeUsage.CreateDefaultTypeUsage(((RefType)edmType).ElementType);
            }

            return null;
        }

        internal static bool MustFacetBeConstant(this TypeUsage type, string facetName)
        {
            DebugCheck.NotNull(type);

            return ((PrimitiveType)type.EdmType).FacetDescriptions.Single(f => f.FacetName == facetName).IsConstant;
        }

        internal static bool IsSpatialType(this TypeUsage type)
        {
            DebugCheck.NotNull(type);

            return (type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType && ((PrimitiveType)type.EdmType).IsSpatialType());
        }

        internal static bool IsSpatialType(this TypeUsage type, out PrimitiveTypeKind spatialType)
        {
            DebugCheck.NotNull(type);

            if (IsSpatialType(type))
            {
                spatialType = ((PrimitiveType)type.EdmType).PrimitiveTypeKind;
                return true;
            }

            spatialType = default(PrimitiveTypeKind);
            return false;
        }

        internal static TypeUsage ForceNonUnicode(this TypeUsage typeUsage)
        {
            // Obtain a non-unicode facet
            var nonUnicodeString = TypeUsage.CreateStringTypeUsage(
                (PrimitiveType)typeUsage.EdmType,
                isUnicode: false,
                isFixedLength: false);

            // Copy all existing facets except replace the non-unicode facet
            return TypeUsage.Create(
                typeUsage.EdmType,
                typeUsage.Facets
                         .Where(f => f.Name != DbProviderManifest.UnicodeFacetName)
                         .Union(
                             nonUnicodeString.Facets.Where(f => f.Name == DbProviderManifest.UnicodeFacetName)));
        }
    }
}
