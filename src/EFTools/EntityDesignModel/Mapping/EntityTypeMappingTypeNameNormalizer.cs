// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Mapping
{
    using System.Diagnostics;

    internal static class EntityTypeMappingTypeNameNormalizer
    {
        // an example use of an alias in a typename
        //
        //  <Alias cdm:Key="CNorthwind" cdm:Value="Test.Simple.Model" />
        //  ...
        //    <EntityTypeMapping cdm:TypeName="CNorthwind.CCategory">
        //      ...
        // 
        // we need to resolve CNorthwind.CCategory to Test.Simple.Model.CCategory so we
        // can look it up in the symbol table
        internal static NormalizedName NameNormalizer(EFElement parent, string refName)
        {
            Debug.Assert(parent != null, "parent should not be null");

            if (refName == null)
            {
                return null;
            }

            // go get the needed parent objects
            var parentItem = parent;
            Debug.Assert(parentItem != null, "parent should not be an EFElement");

            refName = refName.Trim();
            refName = EntityTypeMapping.StripOffIsTypeOf(refName);

            return EFNormalizableItemDefaults.DefaultNameNormalizerForMSL(parentItem, refName);
        }
    }
}
