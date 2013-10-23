// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;

    // Helper class for getting/setting clipboard objects
    internal static class CopyPasteUtils
    {
        internal static void CopyToClipboard(ICollection<EntityTypeShape> entityTypeShapes)
        {
            CopyToClipboard(new EntitiesClipboardFormat(entityTypeShapes));
        }

        internal static void CopyToClipboard(
            ICollection<EntityType> entities, ICollection<Association> associations, IDictionary<EntityType, EntityType> inheritances)
        {
            CopyToClipboard(new EntitiesClipboardFormat(entities, associations, inheritances));
        }

        internal static void CopyToClipboard(ICollection<Property> properties)
        {
            CopyToClipboard(new PropertiesClipboardFormat(properties));
        }

        internal static void CopyToClipboard(ComplexType complexType)
        {
            CopyToClipboard(new ComplexTypeClipboardFormat(complexType));
        }

        internal static void CopyToClipboard(EnumType enumType)
        {
            CopyToClipboard(new EnumTypeClipboardFormat(enumType));
        }

        internal static EntitiesClipboardFormat GetEntitiesFromClipboard()
        {
            return GetFromClipboard<EntitiesClipboardFormat>();
        }

        internal static PropertiesClipboardFormat GetPropertiesFromClipboard()
        {
            return GetFromClipboard<PropertiesClipboardFormat>();
        }

        internal static ComplexTypeClipboardFormat GetComplexTypeFromClipboard()
        {
            return GetFromClipboard<ComplexTypeClipboardFormat>();
        }

        internal static EnumTypeClipboardFormat GetEnumTypeFromClipboard()
        {
            return GetFromClipboard<EnumTypeClipboardFormat>();
        }

        private static void CopyToClipboard<T>(T obj)
        {
            Debug.Assert(obj != null, "Cannot copy null to clipboard");
            if (obj != null)
            {
                var copyDataObject = new DataObject();
                var t = typeof(T);
                copyDataObject.SetData(DataFormats.GetFormat(t.FullName).Name, obj);
                Clipboard.SetDataObject(copyDataObject);
            }
        }

        private static T GetFromClipboard<T>() where T : class
        {
            var copyDataObject = Clipboard.GetDataObject() as DataObject;
            var t = typeof(T);
            if (copyDataObject != null)
            {
                return copyDataObject.GetData(t.FullName) as T;
            }

            return null;
        }
    }
}
