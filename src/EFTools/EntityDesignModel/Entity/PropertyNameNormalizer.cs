// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Diagnostics;

    internal static class PropertyNameNormalizer
    {
        internal static NormalizedName NameNormalizer(EFElement parent, string refName)
        {
            Debug.Assert(parent != null, "parent should not be null");

            if (refName == null)
            {
                return null;
            }

            var entityType = parent as EntityType;
            var complexType = parent as ComplexType;
            var property = parent as Property;
            var navigationProperty = parent as NavigationProperty;

            Symbol symbol = null;

            if (entityType != null)
            {
                var em = entityType.Parent as BaseEntityModel;
                if (em != null)
                {
                    symbol = new Symbol(em.NamespaceValue, entityType.LocalName.Value, refName);
                }
            }
            else if (complexType != null)
            {
                var em = complexType.Parent as BaseEntityModel;
                if (em != null)
                {
                    symbol = new Symbol(em.NamespaceValue, complexType.LocalName.Value, refName);
                }
            }
            else if (property != null
                     || navigationProperty != null)
            {
                var et = parent.Parent as EntityType;
                if (et != null)
                {
                    var em = et.Parent as BaseEntityModel;
                    if (em != null)
                    {
                        symbol = new Symbol(em.NamespaceValue, et.LocalName.Value, refName);
                    }
                }
                else
                {
                    var ct = parent.Parent as ComplexType;
                    if (ct != null)
                    {
                        var em = ct.Parent as BaseEntityModel;
                        if (em != null)
                        {
                            symbol = new Symbol(em.NamespaceValue, ct.LocalName.Value, refName);
                        }
                    }
                }
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
