// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Utilities;
    using System.Reflection;

    // <summary>
    // Class representing a metadata property on an item. Supports
    // redirection from MetadataProperty instance to item property value.
    // </summary>
    internal sealed class MetadataPropertyValue
    {
        internal MetadataPropertyValue(PropertyInfo propertyInfo, MetadataItem item)
        {
            DebugCheck.NotNull(propertyInfo);
            DebugCheck.NotNull(item);
            _propertyInfo = propertyInfo;
            _item = item;
        }

        private readonly PropertyInfo _propertyInfo;
        private readonly MetadataItem _item;

        internal object GetValue()
        {
            return _propertyInfo.GetValue(_item, new object[] { });
        }
    }
}
