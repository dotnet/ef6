// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// Describes the location of a member within an entity or association type structure.
    /// </summary>
    public sealed class ModificationFunctionMemberPath : MappingItem
    {
        private readonly ReadOnlyCollection<EdmMember> _members;
        private readonly AssociationSetEnd _associationSetEnd;

        /// <summary>
        /// Initializes a new ModificationFunctionMemberPath instance.
        /// </summary>
        /// <param name="members">Gets the members in the path from the leaf (the member being bound)
        /// to the root of the structure.</param>
        /// <param name="associationSet">Gets the association set to which we are navigating 
        /// via this member. If the value is null, this is not a navigation member path.</param>
        public ModificationFunctionMemberPath(IEnumerable<EdmMember> members, AssociationSet associationSet)
        {
            Check.NotNull(members, "members");

            _members = new ReadOnlyCollection<EdmMember>(new List<EdmMember>(members));

            if (null != associationSet)
            {
                Debug.Assert(2 == Members.Count, "Association bindings must always consist of the end and the key");

                // find the association set end
                _associationSetEnd = associationSet.AssociationSetEnds[Members[1].Name];
            }
        }

        /// <summary>
        /// Gets the members in the path from the leaf (the member being bound)
        /// to the Root of the structure.
        /// </summary>
        public ReadOnlyCollection<EdmMember> Members
        {
            get { return _members; }
        }

        /// <summary>
        /// Gets the association set to which we are navigating via this member. If the value
        /// is null, this is not a navigation member path.
        /// </summary>
        public AssociationSetEnd AssociationSetEnd
        {
            get { return _associationSetEnd; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return String.Format(
                CultureInfo.InvariantCulture, "{0}{1}",
                null == AssociationSetEnd ? String.Empty : "[" + AssociationSetEnd.ParentAssociationSet + "]",
                StringUtil.BuildDelimitedList(Members, null, "."));
        }
    }
}
