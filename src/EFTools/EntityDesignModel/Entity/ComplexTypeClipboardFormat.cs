// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;

    // Represents ComplexType info stored in Clipboard
    [Serializable]
    internal class ComplexTypeClipboardFormat : AnnotatableElementClipboardFormat
    {
        private readonly string _name;
        private readonly PropertiesClipboardFormat _properties;

        internal ComplexTypeClipboardFormat(ComplexType complexType)
            : base(complexType)
        {
            if (complexType == null)
            {
                throw new ArgumentNullException("complexType");
            }

            _name = complexType.LocalName.Value;
            _properties = new PropertiesClipboardFormat(complexType.Properties());
        }

        internal string Name
        {
            get { return _name; }
        }

        internal PropertiesClipboardFormat Properties
        {
            get { return _properties; }
        }
    }
}
