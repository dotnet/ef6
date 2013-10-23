// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Diagnostics;
    using System.Xml.Linq;

    internal sealed class Key : PropertyRefContainer
    {
        internal static readonly string ElementName = "Key";

        internal Key(EFContainer parent, XElement element)
            : base(parent, element)
        {
        }

        internal override SingleItemBinding<Property>.NameNormalizer GetNameNormalizerForPropertyRef()
        {
            return NameNormalizer;
        }

        internal static NormalizedName NameNormalizer(EFElement parent, string refName)
        {
            Debug.Assert(parent != null, "parent should not be null");

            if (refName == null)
            {
                return null;
            }

            var pr = parent as PropertyRef;
            var key = pr.Parent as Key;
            var entityType = key.Parent as EntityType;

            var em = entityType.Parent as BaseEntityModel;
            Symbol symbol = null;
            if (em != null)
            {
                symbol = new Symbol(em.NamespaceValue, entityType.LocalName.Value, refName);
            }

            if (symbol == null)
            {
                symbol = new Symbol(refName);
            }

            var normalizedName = new NormalizedName(symbol, null, null, refName);

            return normalizedName;
        }
    }
}
