// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;

    // Represents EnumType info stored in Clipboard
    [Serializable]
    internal class EnumTypeClipboardFormat : AnnotatableElementClipboardFormat
    {
        private readonly string _name;
        private readonly string _underlyingType;
        private readonly bool _isFlag;
        private readonly string _externalTypeName;
        private readonly EnumTypeMembersClipboardFormat _members;

        public EnumTypeClipboardFormat(EnumType enumType)
            : base(enumType)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }

            _name = enumType.LocalName.Value;
            _underlyingType = enumType.UnderlyingType.Value;
            _isFlag = enumType.IsFlags.Value;
            _externalTypeName = enumType.ExternalTypeName.Value;
            _members = new EnumTypeMembersClipboardFormat(enumType.Members());
        }

        public string Name
        {
            get { return _name; }
        }

        public string UnderlyingType
        {
            get { return _underlyingType; }
        }

        public string ExternalTypeName
        {
            get { return _externalTypeName; }
        }

        public bool IsFlag
        {
            get { return _isFlag; }
        }

        public EnumTypeMembersClipboardFormat Members
        {
            get { return _members; }
        }
    }
}
