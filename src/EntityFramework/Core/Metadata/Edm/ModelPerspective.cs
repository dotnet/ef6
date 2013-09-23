// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Utilities;

    // <summary>
    // Internal helper class for query
    // </summary>
    internal class ModelPerspective : Perspective
    {
        // <summary>
        // Creates a new instance of perspective class so that query can work
        // ignorant of all spaces
        // </summary>
        // <param name="metadataWorkspace"> runtime metadata container </param>
        internal ModelPerspective(MetadataWorkspace metadataWorkspace)
            : base(metadataWorkspace, DataSpace.CSpace)
        {
        }

        // <summary>
        // Look up a type in the target data space based upon the fullName
        // </summary>
        // <param name="fullName"> fullName </param>
        // <param name="ignoreCase"> true for case-insensitive lookup </param>
        // <param name="typeUsage"> The type usage object to return </param>
        // <returns> True if the retrieval succeeded </returns>
        internal override bool TryGetTypeByName(string fullName, bool ignoreCase, out TypeUsage typeUsage)
        {
            Check.NotEmpty(fullName, "fullName");
            typeUsage = null;
            EdmType edmType = null;
            if (MetadataWorkspace.TryGetItem(fullName, ignoreCase, TargetDataspace, out edmType))
            {
                if (Helper.IsPrimitiveType(edmType))
                {
                    typeUsage = MetadataWorkspace.GetCanonicalModelTypeUsage(((PrimitiveType)edmType).PrimitiveTypeKind);
                }
                else
                {
                    typeUsage = TypeUsage.Create(edmType);
                }
            }
            return typeUsage != null;
        }
    }
}
