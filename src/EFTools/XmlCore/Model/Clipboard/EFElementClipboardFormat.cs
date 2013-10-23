// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;

    [Serializable]
    internal abstract class EFElementClipboardFormat
    {
        internal EFElementClipboardFormat(EFElement efElement)
        {
            var normalizableItem = efElement as EFNormalizableItem;

            NormalizedName = null;
            if (normalizableItem != null)
            {
                NormalizedName = normalizableItem.NormalizedName;
            }
        }

        internal Symbol NormalizedName { get; private set; }
    }
}
