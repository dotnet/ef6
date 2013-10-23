// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using System.Collections.Generic;

    // Represents multiple Properties info stored in Clipboard
    [Serializable]
    internal class PropertiesClipboardFormat
    {
        private readonly List<PropertyClipboardFormat> _properties = new List<PropertyClipboardFormat>();

        internal PropertiesClipboardFormat(IEnumerable<Property> properties)
        {
            foreach (var property in properties)
            {
                _properties.Add(new PropertyClipboardFormat(property));
            }
        }

        internal List<PropertyClipboardFormat> ClipboardProperties
        {
            get { return _properties; }
        }
    }
}
