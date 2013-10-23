// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;

    [Serializable]
    internal class EnumTypeMemberClipboardFormat : AnnotatableElementClipboardFormat
    {
        private string _memberName;
        private string _memberValue;

        public EnumTypeMemberClipboardFormat(EnumTypeMember enumTypeMember)
            : base(enumTypeMember)
        {
            _memberName = enumTypeMember.Name.Value;
            _memberValue = enumTypeMember.Value.Value;
        }

        internal string MemberValue
        {
            get { return _memberValue; }
            set { _memberValue = value; }
        }

        internal string MemberName
        {
            get { return _memberName; }
            set { _memberName = value; }
        }
    }
}
