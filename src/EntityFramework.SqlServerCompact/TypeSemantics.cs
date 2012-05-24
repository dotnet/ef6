namespace System.Data.Entity.SqlServerCompact.SqlGen
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    internal static class TypeSemantics
    {
        // TypeUsage
        //
        internal static bool IsCollectionType(TypeUsage type)
        {
            return IsCollectionType(type.EdmType);
        }

        internal static bool IsRowType(TypeUsage type)
        {
            return IsRowType(type.EdmType);
        }

        internal static bool IsPrimitiveType(TypeUsage type)
        {
            return IsPrimitiveType(type.EdmType);
        }

        internal static bool IsReferenceType(TypeUsage type)
        {
            return IsRefType(type.EdmType);
        }

        // EdmType
        //
        internal static bool IsCollectionType(EdmType type)
        {
            return (BuiltInTypeKind.CollectionType == type.BuiltInTypeKind);
        }

        internal static bool IsRowType(EdmType type)
        {
            return (BuiltInTypeKind.RowType == type.BuiltInTypeKind);
        }

        internal static bool IsPrimitiveType(EdmType type)
        {
            return (BuiltInTypeKind.PrimitiveType == type.BuiltInTypeKind);
        }

        internal static bool IsRefType(EdmType type)
        {
            return (BuiltInTypeKind.RefType == type.BuiltInTypeKind);
        }

        internal static bool IsPrimitiveType(TypeUsage type, PrimitiveTypeKind primitiveType)
        {
            PrimitiveTypeKind typeKind;
            if (TypeHelpers.TryGetPrimitiveTypeKind(type, out typeKind))
            {
                return (typeKind == primitiveType);
            }
            return false;
        }

        internal static bool IsNullable(TypeUsage type)
        {
            Facet nullableFacet;
            if (type.Facets.TryGetValue(ProviderManifest.NullableFacetName, false, out nullableFacet))
            {
                return (bool)nullableFacet.Value;
            }

            return false;
        }

        // requires: typeUsage wraps a primitive type
        internal static PrimitiveTypeKind GetPrimitiveTypeKind(TypeUsage typeUsage)
        {
            Debug.Assert(
                null != typeUsage && null != typeUsage.EdmType && typeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType);

            var primitiveType = (PrimitiveType)typeUsage.EdmType;

            return primitiveType.PrimitiveTypeKind;
        }

        internal static DbType GetDbType(PrimitiveTypeKind primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveTypeKind.Binary:
                    return DbType.Binary;
                case PrimitiveTypeKind.Boolean:
                    return DbType.Boolean;
                case PrimitiveTypeKind.Byte:
                    return DbType.Byte;
                case PrimitiveTypeKind.DateTime:
                    return DbType.DateTime;
                case PrimitiveTypeKind.Decimal:
                    return DbType.Decimal;
                case PrimitiveTypeKind.Double:
                    return DbType.Double;
                case PrimitiveTypeKind.Single:
                    return DbType.Single;
                case PrimitiveTypeKind.Guid:
                    return DbType.Guid;
                case PrimitiveTypeKind.Int16:
                    return DbType.Int16;
                case PrimitiveTypeKind.Int32:
                    return DbType.Int32;
                case PrimitiveTypeKind.Int64:
                    return DbType.Int64;
                case PrimitiveTypeKind.String:
                    return DbType.String;
                default:
                    Debug.Fail("unknown PrimitiveTypeKind" + primitiveType.ToString());
                    throw ADP1.InvalidOperation(String.Empty);
            }
        }
    }
}