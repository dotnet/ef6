// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal static class AssociationNameNormalizer
    {
        internal static NormalizedName NameNormalizer(EFElement parent, string refName)
        {
            Debug.Assert(parent != null, "parent should not be null");

            if (refName == null)
            {
                return null;
            }

            NormalizedName normalizedName = null;

            var parentAssociation = parent as Association;
            var parentAssociationSet = parent as AssociationSet;
            var parentAssociationSetMapping = parent as AssociationSetMapping;
            var parentNavigationProperty = parent as NavigationProperty;

            if (parentAssociation != null)
            {
                var model = parentAssociation.Parent as BaseEntityModel;
                if (model != null)
                {
                    // we are coming up with the object's name for the first time
                    var symbol = new Symbol(model.NamespaceValue, parentAssociation.LocalName.Value);
                    normalizedName = new NormalizedName(symbol, null, null, parentAssociation.LocalName.Value);
                }
            }
            else if (parentAssociationSet != null)
            {
                // we are wanting to resolve a reference from an Association Set that may or may not
                // use the alias defined in the EntityModel
                normalizedName = EFNormalizableItemDefaults.DefaultNameNormalizerForEDM(parentAssociationSet, refName);
            }
            else if (parentAssociationSetMapping != null)
            {
                normalizedName = EFNormalizableItemDefaults.DefaultNameNormalizerForMSL(parentAssociationSetMapping, refName);
            }
            else if (parentNavigationProperty != null)
            {
                normalizedName = EFNormalizableItemDefaults.DefaultNameNormalizerForEDM(parentNavigationProperty, refName);
            }

            if (normalizedName == null)
            {
                var symbol = new Symbol(refName);
                normalizedName = new NormalizedName(symbol, null, null, refName);
            }

            return normalizedName;
        }
    }
}
