// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    public class EdmEnumType : EdmDataModelType
    {
        private readonly BackingList<EdmEnumTypeMember> membersList = new BackingList<EdmEnumTypeMember>();

        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
        public virtual bool IsFlags { get; set; }
        public virtual EdmPrimitiveType UnderlyingType { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<EdmEnumTypeMember> Members
        {
            get { return membersList.EnsureValue(); }
            set { membersList.SetValue(value); }
        }

        internal bool HasMembers
        {
            get { return membersList.HasValue; }
        }

        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.EnumType;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return membersList;
        }
    }
}
