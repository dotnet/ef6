// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Diagnostics;

    /// <summary>
    ///     The refName for these ends cannot be already normalized.  The Role attribute points to
    ///     an End of the Assocation from the AssociationSet, so it already has its scope set in stone.
    ///     The EntitySet attribute points to an EntitySet that must be in the current EntityContainer
    ///     and EntitySet names don't use the schema alias or namespace.
    /// </summary>
    internal static class AssociationSetEndRoleNormalizer
    {
        internal static NormalizedName NameNormalizer(EFElement parent, string refName)
        {
            Debug.Assert(parent != null, "parent should not be null");

            if (refName == null)
            {
                return null;
            }

            NormalizedName normalizedName = null;

            // cast the parameter to what this really is
            var end = parent as AssociationSetEnd;
            Debug.Assert(end != null, "parent should be an AssociationSetEnd");

            // get the assoc set
            var set = end.Parent as AssociationSet;
            Debug.Assert(set != null, "end.Parent should be an AssociationSet");

            // the "Role" attribute points to a "Role" attribute in an Association End.  The trick
            // is that this attribute is optional and defaults to the raw name of the Entity pointed
            // to by the "Type" attribute of the Association End, and that might not be fully resolved
            // yet.  So, lets just get the Normalized Name for the assocation from the assocation set 
            //and tack on the Role name.
            if (set.Association.Status == BindingStatus.Known)
            {
                var assocNameRef = AssociationNameNormalizer.NameNormalizer(set.Association.Target, set.Association.RefName);
                var assocSymbol = assocNameRef.Symbol;
                var assocSetEndRoleSymbol = new Symbol(assocSymbol, refName);
                normalizedName = new NormalizedName(assocSetEndRoleSymbol, null, null, refName);
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
