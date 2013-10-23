// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Mapping
{
    using System.Diagnostics;

    internal static class FunctionImportProperyMappingNameNormalizer
    {
        internal static NormalizedName NameNormalizer(EFElement parent, string refName)
        {
            Debug.Assert(parent != null, "parent should not be null");

            if (refName == null)
            {
                return null;
            }

            var parentItem = parent.Parent as EFElement;
            Debug.Assert(parentItem != null, "parent.Parent should be an EFElement");

            // some asserts to verify we're using this name normalizer in the correct context

            Debug.Assert(
                parentItem.GetParentOfType(typeof(ComplexProperty)) == null,
                "Use the PropertyMappingNameNormalizer to normalize children of " + parent.GetType().FullName);

            Debug.Assert(
                parentItem.GetParentOfType(typeof(FunctionComplexProperty)) == null,
                "Use the FunctionPropertyMappingNameNormalizer to normalize children of " + parent.GetType().FullName);

            Debug.Assert(
                parentItem.GetParentOfType(typeof(EntityTypeMapping)) == null,
                "Use the PropertyMappingNameNormalizer or FunctionPropertyMappingNameNormalizer to normalize children of "
                + parent.GetType().FullName);

            Debug.Assert(
                parentItem.GetParentOfType(typeof(FunctionAssociationEnd)) == null,
                "Use the FunctionPropertyMappingNameNormalizer to normalize children of " + parent.GetType().FullName);

            Debug.Assert(
                parentItem.GetParentOfType(typeof(AssociationSetMapping)) == null,
                "Use the PropertyNameNormalizer to normalize children of " + parent.GetType().FullName);
            Debug.Assert(
                parentItem.GetParentOfType(typeof(EndProperty)) == null,
                "Use the PropertyNameNormalizer to normalize children of " + parent.GetType().FullName);

            //
            // try to normalize for a FunctionImportTypeMapping
            //
            var fitm = parentItem.GetParentOfType(typeof(FunctionImportTypeMapping)) as FunctionImportTypeMapping;
            if (fitm != null)
            {
                if (fitm.TypeName.Status == BindingStatus.Known)
                {
                    var symbol = new Symbol(fitm.TypeName.Target.NormalizedName, refName);
                    return new NormalizedName(symbol, null, null, refName);
                }
            }

            return new NormalizedName(new Symbol(refName), null, null, refName);
        }
    }
}
