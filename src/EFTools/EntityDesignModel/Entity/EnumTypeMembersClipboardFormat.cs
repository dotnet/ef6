// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    internal class EnumTypeMembersClipboardFormat
    {
        private readonly List<EnumTypeMemberClipboardFormat> _members = new List<EnumTypeMemberClipboardFormat>();

        public EnumTypeMembersClipboardFormat(IEnumerable<EnumTypeMember> members)
        {
            foreach (var member in members)
            {
                _members.Add(new EnumTypeMemberClipboardFormat(member));
            }
        }

        public List<EnumTypeMemberClipboardFormat> ClipboardMembers
        {
            get { return _members; }
        }
    }
}
