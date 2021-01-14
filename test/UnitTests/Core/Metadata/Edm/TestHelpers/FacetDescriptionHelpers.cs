// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Core.Metadata.Edm.Provider;

    public static class FacetDescriptionHelpers
    {
        public static FacetDescription GetFacetDescription<T>(string facetName)
        {
            var type = typeof(T);
            EdmType edmType;
            PrimitiveTypeKind primitiveTypeKind;
            if (ClrProviderManifest.TryGetPrimitiveTypeKind(type, out primitiveTypeKind))
            {
                edmType = new PrimitiveType { PrimitiveTypeKind = primitiveTypeKind };
            }
            else
            {
                edmType = new EnumType(type);
            }

            return new FacetDescription(facetName, edmType, null, null, null);
        }
    }
}
