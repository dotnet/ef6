// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal class AssociationSetNameNormalizer
    {
        internal static NormalizedName NameNormalizer(EFElement parent, string refName)
        {
            Debug.Assert(parent != null, "parent should not be null");

            if (refName == null)
            {
                return null;
            }

            var entityContainerName = string.Empty;

            var parentAssociationSet = parent as AssociationSet;
            var parentAssociationSetMapping = parent as AssociationSetMapping;
            var parentFunctionAssociationEnd = parent as FunctionAssociationEnd;

            if (parentAssociationSet != null)
            {
                // are we trying to normalize the name of actual association set in the EC?
                var ec = parentAssociationSet.Parent as BaseEntityContainer;
                if (ec != null)
                {
                    entityContainerName = ec.LocalName.Value;
                }
            }
            else if (parentAssociationSetMapping != null
                     || parentFunctionAssociationEnd != null)
            {
                // we need to resolve a reference from the MSL to the AssociationSet
                var ecm = parent.GetParentOfType(typeof(EntityContainerMapping)) as EntityContainerMapping;
                if (ecm != null)
                {
                    entityContainerName = ecm.CdmEntityContainer.RefName;
                }
            }

            Symbol symbol = null;
            if (!string.IsNullOrEmpty(entityContainerName))
            {
                symbol = new Symbol(entityContainerName, refName);
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
