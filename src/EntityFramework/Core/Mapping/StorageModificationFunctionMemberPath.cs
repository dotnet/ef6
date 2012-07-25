// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Globalization;

    /// <summary>
    /// Describes the location of a member within an entity or association type structure.
    /// </summary>
    internal sealed class StorageModificationFunctionMemberPath
    {
        internal StorageModificationFunctionMemberPath(IEnumerable<EdmMember> members, AssociationSet associationSetNavigation)
        {
            Contract.Requires(members != null);

            Members = new ReadOnlyCollection<EdmMember>(new List<EdmMember>(members));

            if (null != associationSetNavigation)
            {
                Debug.Assert(2 == Members.Count, "Association bindings must always consist of the end and the key");

                // find the association set end
                AssociationSetEnd = associationSetNavigation.AssociationSetEnds[Members[1].Name];
            }
        }

        /// <summary>
        /// Gets the members in the path from the leaf (the member being bound)
        /// to the Root of the structure.
        /// </summary>
        internal readonly ReadOnlyCollection<EdmMember> Members;

        /// <summary>
        /// Gets the association set to which we are navigating via this member. If the value
        /// is null, this is not a navigation member path.
        /// </summary>
        internal readonly AssociationSetEnd AssociationSetEnd;

        public override string ToString()
        {
            return String.Format(
                CultureInfo.InvariantCulture, "{0}{1}",
                null == AssociationSetEnd ? String.Empty : "[" + AssociationSetEnd.ParentAssociationSet + "]",
                StringUtil.BuildDelimitedList(Members, null, "."));
        }
    }
}
