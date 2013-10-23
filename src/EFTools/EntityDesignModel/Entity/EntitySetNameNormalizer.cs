// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal static class EntitySetNameNormalizer
    {
        internal static NormalizedName NameNormalizer(EFElement parent, string refName)
        {
            Debug.Assert(parent != null, "parent should not be null");

            Symbol symbol = null;

            if (refName == null)
            {
                return null;
            }

            var entityContainerName = string.Empty;

            var parentEntitySet = parent as EntitySet;
            var parentEntitySetMapping = parent as EntitySetMapping;
            var parentMappingFragment = parent as MappingFragment;
            var parentAssociationSetMapping = parent as AssociationSetMapping;

            if (parentEntitySet != null)
            {
                // we are trying to normalize the name of actual entity set in the EC
                var ec = parentEntitySet.Parent as BaseEntityContainer;
                if (ec != null)
                {
                    entityContainerName = ec.EntityContainerName;
                }
            }
            else if (parentEntitySetMapping != null)
            {
                // we are trying to normalize the name reference to an EntitySet in the
                // C-space from an EntitySetMapping
                var ecm = parentEntitySetMapping.Parent as EntityContainerMapping;
                if (ecm != null)
                {
                    entityContainerName = ecm.CdmEntityContainer.RefName;
                }
            }
            else if (parentMappingFragment != null)
            {
                // we are trying to normalize the name reference to an EntitySet in the
                // S-space from a MappingFragment
                var ecm = parentMappingFragment.GetParentOfType(typeof(EntityContainerMapping)) as EntityContainerMapping;
                if (ecm != null)
                {
                    entityContainerName = ecm.StorageEntityContainer.RefName;
                }
            }
            else if (parentAssociationSetMapping != null)
            {
                // we are trying to normalize the name reference "TableName" in an
                // AssociationSetMapping back to an EntitySet in S-Space
                var ecm = parentAssociationSetMapping.GetParentOfType(typeof(EntityContainerMapping)) as EntityContainerMapping;
                if (ecm != null)
                {
                    entityContainerName = ecm.StorageEntityContainer.RefName;
                }
            }

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
