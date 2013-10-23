// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal static class FunctionNameNormalizer
    {
        internal static NormalizedName NameNormalizer(EFElement parent, string refName)
        {
            Debug.Assert(parent != null, "parent should not be null");

            NormalizedName normalizedName = null;

            if (refName == null)
            {
                return null;
            }

            var modfunc = parent as ModificationFunction;
            if (modfunc != null)
            {
                normalizedName = EFNormalizableItemDefaults.DefaultNameNormalizerForMSL(modfunc, refName);
            }
            else
            {
                var parentItem = parent;
                normalizedName = EFNormalizableItemDefaults.DefaultNameNormalizerForEDM(parentItem, refName);
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
