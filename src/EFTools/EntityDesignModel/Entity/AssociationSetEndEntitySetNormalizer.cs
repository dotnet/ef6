// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Diagnostics;

    /// <summary>
    ///     The refName for these ends cannot be already normalized.  The Role attribute points to
    ///     an End of the Association from the AssociationSet, so it already has its scope set in stone.
    ///     The EntitySet attribute points to an EntitySet that must be in the current EntityContainer
    ///     and EntitySet names don't use the schema alias or namespace.
    /// </summary>
    internal static class AssociationSetEndEntitySetNormalizer
    {
        internal static NormalizedName NameNormalizer(EFElement parent, string refName)
        {
            Debug.Assert(parent != null, "parent should not be null");

            if (refName == null)
            {
                return null;
            }

            // cast the parameter to what this really is
            var end = parent as AssociationSetEnd;
            Debug.Assert(end != null, "parent should be an AssociationSetEnd");

            // get the assoc set
            var set = end.Parent as AssociationSet;
            Debug.Assert(set != null, "association set end parent should be an AssociationSet");

            // get the entity container name
            string entityContainerName = null;
            var ec = set.Parent as BaseEntityContainer;
            if (ec != null)
            {
                entityContainerName = ec.EntityContainerName;
            }

            Debug.Assert(ec != null, "AssociationSet parent should be a subclass of BaseEntityContainer");

            // the normalized name for an EntitySet is 'EntityContainerName + # + EntitySetName'
            var symbol = new Symbol(entityContainerName, refName);

            var normalizedName = new NormalizedName(symbol, null, null, refName);
            return normalizedName;
        }
    }
}
