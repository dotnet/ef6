// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal static class EntityTypeNameNormalizer
    {
        internal static NormalizedName NameNormalizer(EFElement parent, string refName)
        {
            Debug.Assert(parent != null, "parent should not be null");

            NormalizedName normalizedName = null;

            if (refName == null)
            {
                return null;
            }

            var parentEndProperty = parent as EndProperty;
            if (parentEndProperty != null)
            {
                normalizedName = EFNormalizableItemDefaults.DefaultNameNormalizerForMSL(parentEndProperty, refName);
            }
            else
            {
                var parentItem = parent;
                normalizedName = EFNormalizableItemDefaults.DefaultNameNormalizerForEDM(parentItem, refName);
            }

            if (normalizedName == null)
            {
                normalizedName = new NormalizedName(new Symbol(refName), null, null, refName);
            }

            return normalizedName;
        }
    }
}
