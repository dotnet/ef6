namespace System.Data.Entity.SqlServer.Utilities
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.Contracts;

    internal static class PrimitiveTypeExtensions
    {
        internal static bool IsSpatialType(this PrimitiveType type)
        {
            Contract.Requires(type != null);

            var kind = type.PrimitiveTypeKind;

            return kind >= PrimitiveTypeKind.Geometry && kind <= PrimitiveTypeKind.GeographyCollection;
        }
    }
}
