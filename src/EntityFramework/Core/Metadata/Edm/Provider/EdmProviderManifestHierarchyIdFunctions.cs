// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm.Provider
{
    internal static class EdmProviderManifestHierarchyIdFunctions
    {
        internal static void AddFunctions(EdmProviderManifestFunctionBuilder functions)
        {
            // HierarchyId Functions
            functions.AddFunction(PrimitiveTypeKind.HierarchyId, "HierarchyIdGetRoot");
            functions.AddFunction(PrimitiveTypeKind.HierarchyId, "HierarchyIdParse", PrimitiveTypeKind.String, "input");
            functions.AddFunction(
                PrimitiveTypeKind.HierarchyId, "GetAncestor", PrimitiveTypeKind.HierarchyId, "hierarchyIdValue",
                PrimitiveTypeKind.Int32, "n");
            functions.AddFunction(
                PrimitiveTypeKind.HierarchyId, "GetDescendant", PrimitiveTypeKind.HierarchyId, "hierarchyIdValue",
                PrimitiveTypeKind.HierarchyId, "child1", PrimitiveTypeKind.HierarchyId, "child2");
            functions.AddFunction(
                PrimitiveTypeKind.Int16, "GetLevel", PrimitiveTypeKind.HierarchyId, "hierarchyIdValue");
            functions.AddFunction(
                PrimitiveTypeKind.Boolean, "IsDescendantOf", PrimitiveTypeKind.HierarchyId, "hierarchyIdValue",
                PrimitiveTypeKind.HierarchyId, "parent");
            functions.AddFunction(
                PrimitiveTypeKind.HierarchyId, "GetReparentedValue", PrimitiveTypeKind.HierarchyId, "hierarchyIdValue",
                PrimitiveTypeKind.HierarchyId, "oldRoot", PrimitiveTypeKind.HierarchyId, "newRoot");
        }
    }
}
