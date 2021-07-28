// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.Utilities
{
    using System.Data.Entity.Core.Metadata.Edm;

    internal static class PrimitiveTypeExtensions
    {
        internal static bool IsSpatialType(this PrimitiveType type)
        {
            DebugCheck.NotNull(type);

            var kind = type.PrimitiveTypeKind;

            return kind >= PrimitiveTypeKind.Geometry && kind <= PrimitiveTypeKind.GeographyCollection;
        }

        internal static bool IsHierarchyIdType(this PrimitiveType type)
        {
            DebugCheck.NotNull(type);

            var kind = type.PrimitiveTypeKind;

            return kind == PrimitiveTypeKind.HierarchyId;
        }
    }
}
