// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;

    // <summary>
    // An implementation of <see cref="IPropertyValuesItem" /> for an item in a <see cref="DbDataRecordPropertyValues" />.
    // </summary>
    internal class DbDataRecordPropertyValuesItem : IPropertyValuesItem
    {
        #region Constructors and fields

        private readonly DbUpdatableDataRecord _dataRecord;
        private readonly int _ordinal;
        private object _value;

        // <summary>
        // Initializes a new instance of the <see cref="DbDataRecordPropertyValuesItem" /> class.
        // </summary>
        // <param name="dataRecord"> The data record. </param>
        // <param name="ordinal"> The ordinal. </param>
        // <param name="value"> The value. </param>
        public DbDataRecordPropertyValuesItem(DbUpdatableDataRecord dataRecord, int ordinal, object value)
        {
            _dataRecord = dataRecord;
            _ordinal = ordinal;
            _value = value;
        }

        #endregion

        #region IPropertyValuesItem implementation

        // <summary>
        // Gets or sets the value of the property represented by this item.
        // </summary>
        // <value> The value. </value>
        public object Value
        {
            get { return _value; }
            set
            {
                _dataRecord.SetValue(_ordinal, value);
                _value = value;
            }
        }

        // <summary>
        // Gets the name of the property.
        // </summary>
        // <value> The name. </value>
        public string Name
        {
            get { return _dataRecord.GetName(_ordinal); }
        }

        // <summary>
        // Gets a value indicating whether this item represents a complex property.
        // </summary>
        // <value>
        // <c>true</c> If this instance represents a complex property; otherwise, <c>false</c> .
        // </value>
        public bool IsComplex
        {
            get
            {
                return _dataRecord.DataRecordInfo.FieldMetadata[_ordinal].FieldType.TypeUsage.EdmType.BuiltInTypeKind
                       == BuiltInTypeKind.ComplexType;
            }
        }

        // <summary>
        // Gets the type of the underlying property.
        // </summary>
        // <value> The property type. </value>
        public Type Type
        {
            get { return _dataRecord.GetFieldType(_ordinal); }
        }

        #endregion
    }
}
