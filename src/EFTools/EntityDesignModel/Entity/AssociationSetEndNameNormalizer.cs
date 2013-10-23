// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     This normalizer is different from most in that there isn't a "Name" property
    ///     on an End element in an AssociationSet.  This method takes the refName sent and
    ///     uses it to come up with a unique symbol that can be used to identify the End.
    /// </summary>
    internal static class AssociationSetEndNameNormalizer
    {
        internal static NormalizedName NameNormalizer(EFElement parent, string refName)
        {
            Debug.Assert(parent != null, "parent should not be null");

            if (refName == null)
            {
                return null;
            }

            var parentAssociationSetEnd = parent as AssociationSetEnd;
            var parentEndProperty = parent as EndProperty;
            var parentFunctionAssociationEnd = parent as FunctionAssociationEnd;

            var symbol = new Symbol();

            if (parentAssociationSetEnd != null)
            {
                // we are coming up with the object's name for the first time
                var assocSet = parentAssociationSetEnd.Parent as AssociationSet;
                if (assocSet != null)
                {
                    var ec = assocSet.Parent as BaseEntityContainer;
                    if (ec != null)
                    {
                        symbol = new Symbol(ec.EntityContainerName, assocSet.LocalName.Value, refName);
                    }
                }
            }
            else if (parentEndProperty != null)
            {
                // this end is inside an AssociationSetMapping, so we derive the end's name based on the set's name
                var asm = parentEndProperty.Parent as AssociationSetMapping;
                if (asm.Name.Status == BindingStatus.Known)
                {
                    symbol = new Symbol(asm.Name.Target.NormalizedName, refName);
                }
            }
            else if (parentFunctionAssociationEnd != null)
            {
                // this end is inside of a function mapping
                if (parentFunctionAssociationEnd.AssociationSet.Status == BindingStatus.Known)
                {
                    symbol = new Symbol(parentFunctionAssociationEnd.AssociationSet.Target.NormalizedName, refName);
                }
            }

            var normalizedName = new NormalizedName(symbol, null, null, refName);
            return normalizedName;
        }
    }
}
