// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;

    internal sealed class DbDataRecordList : Collection<DbDataRecord>, ITypedList
    {
        // <summary>
        // Cache of the property descriptors for the element type of the root list wrapped by ObjectView.
        // </summary>
        private readonly PropertyDescriptorCollection _propertyDescriptorsCache;

        // <summary>
        // EDM RowType that describes the shape of record elements.
        // </summary>
        private readonly RowType _rowType;

        internal DbDataRecordList(IList<DbDataRecord> recordList, RowType rowType)
            : base(recordList)
        {
            _rowType = rowType;
            _propertyDescriptorsCache = MaterializedDataRecord.CreatePropertyDescriptorCollection(_rowType, typeof(DbDataRecord), true);
        }

        #region ITypedList Members

        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            return _rowType.Name;
        }

        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            if (listAccessors == null
                || listAccessors.Length == 0)
            {
                // Caller is requesting property descriptors for the root element type.
                return _propertyDescriptorsCache;
            }

            return DataRecordObjectView.GetItemProperties(listAccessors);
        }

        #endregion
    }
}
