namespace System.Data.Entity.Core.Objects
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// Instances of this class would be returned to user via Query&lt;T&gt;
    /// </summary>
    internal sealed class MaterializedDataRecord : DbDataRecord, IExtendedDataRecord, ICustomTypeDescriptor
    {
        private FieldNameLookup _fieldNameLookup;
        private DataRecordInfo _recordInfo;
        private readonly MetadataWorkspace _workspace;
        private readonly TypeUsage _edmUsage;
        private readonly object[] _values;

        /// <summary>
        ///
        /// </summary>
        internal MaterializedDataRecord(MetadataWorkspace workspace, TypeUsage edmUsage, object[] values)
        {
            Debug.Assert(null != edmUsage && null != values, "null recordType or values");
            _workspace = workspace;
            _edmUsage = edmUsage;
#if DEBUG
            for (var i = 0; i < values.Length; ++i)
            {
                Debug.Assert(null != values[i], "should have been DBNull.Value");
            }
#endif
            _values = values; // take ownership of the array
        }

        /// <summary>
        ///
        /// </summary>
        public DataRecordInfo DataRecordInfo
        {
            get
            {
                if (null == _recordInfo)
                {
                    // delay creation of DataRecordInfo until necessary
                    if (null == _workspace)
                    {
                        // When _workspace is null, we are materializing PODR.
                        // In this case, emdUsage describes a RowType.
                        Debug.Assert(Helper.IsRowType(_edmUsage.EdmType), "Edm type should be Row Type");
                        _recordInfo = new DataRecordInfo(_edmUsage);
                    }
                    else
                    {
                        _recordInfo = new DataRecordInfo(_workspace.GetOSpaceTypeUsage(_edmUsage));
                    }
                    Debug.Assert(_values.Length == _recordInfo.FieldMetadata.Count, "wrong values array size");
                }
                return _recordInfo;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public override int FieldCount
        {
            get { return _values.Length; }
        }

        /// <summary>
        ///
        /// </summary>
        public override object this[int ordinal]
        {
            get { return GetValue(ordinal); }
        }

        /// <summary>
        ///
        /// </summary>
        public override object this[string name]
        {
            get { return GetValue(GetOrdinal(name)); }
        }

        /// <summary>
        ///
        /// </summary>
        public override bool GetBoolean(int ordinal)
        {
            return ((bool)_values[ordinal]);
        }

        /// <summary>
        ///
        /// </summary>
        public override byte GetByte(int ordinal)
        {
            return ((byte)_values[ordinal]);
        }

        /// <summary>
        ///
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        public override long GetBytes(int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            var cbytes = 0;
            int ndataIndex;

            var data = (byte[])_values[ordinal];

            cbytes = data.Length;

            // since arrays can't handle 64 bit values and this interface doesn't
            // allow chunked access to data, a dataIndex outside the rang of Int32
            // is invalid
            if (fieldOffset > Int32.MaxValue)
            {
                throw new ArgumentOutOfRangeException(
                    "fieldOffset", Strings.ADP_InvalidSourceBufferIndex(
                        cbytes.ToString(CultureInfo.InvariantCulture), fieldOffset.ToString(CultureInfo.InvariantCulture)));
            }

            ndataIndex = (int)fieldOffset;

            // if no buffer is passed in, return the number of characters we have
            if (null == buffer)
            {
                return cbytes;
            }

            try
            {
                if (ndataIndex < cbytes)
                {
                    // help the user out in the case where there's less data than requested
                    if ((ndataIndex + length) > cbytes)
                    {
                        cbytes = cbytes - ndataIndex;
                    }
                    else
                    {
                        cbytes = length;
                    }
                }

                Array.Copy(data, ndataIndex, buffer, bufferOffset, cbytes);
            }
            catch (Exception e)
            {
                // UNDONE - should not be catching all exceptions!!!
                if (e.IsCatchableExceptionType())
                {
                    cbytes = data.Length;

                    if (length < 0)
                    {
                        throw new IndexOutOfRangeException(
                            Strings.ADP_InvalidDataLength(((long)length).ToString(CultureInfo.InvariantCulture)));
                    }

                    // if bad buffer index, throw
                    if (bufferOffset < 0
                        || bufferOffset >= buffer.Length)
                    {
                        throw new ArgumentOutOfRangeException(
                            "bufferOffset", Strings.ADP_InvalidDestinationBufferIndex(
                                length.ToString(CultureInfo.InvariantCulture), bufferOffset.ToString(CultureInfo.InvariantCulture)));
                    }

                    // if bad data index, throw
                    if (fieldOffset < 0
                        || fieldOffset >= cbytes)
                    {
                        throw new ArgumentOutOfRangeException(
                            "fieldOffset", Strings.ADP_InvalidSourceBufferIndex(
                                length.ToString(CultureInfo.InvariantCulture), fieldOffset.ToString(CultureInfo.InvariantCulture)));
                    }

                    // if there is not enough room in the buffer for data
                    if (cbytes + bufferOffset
                        > buffer.Length)
                    {
                        throw new IndexOutOfRangeException(
                            Strings.ADP_InvalidBufferSizeOrIndex(
                                cbytes.ToString(CultureInfo.InvariantCulture), bufferOffset.ToString(CultureInfo.InvariantCulture)));
                    }
                }

                throw;
            }

            return cbytes;
        }

        /// <summary>
        ///
        /// </summary>
        public override char GetChar(int ordinal)
        {
            return ((string)GetValue(ordinal))[0];
        }

        /// <summary>
        ///
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        public override long GetChars(int ordinal, long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            var cchars = 0;
            int ndataIndex;
            var data = (string)_values[ordinal];

            cchars = data.Length;

            // since arrays can't handle 64 bit values and this interface doesn't
            // allow chunked access to data, a dataIndex outside the rang of Int32
            // is invalid
            if (fieldOffset > Int32.MaxValue)
            {
                throw new ArgumentOutOfRangeException(
                    "fieldOffset", Strings.ADP_InvalidSourceBufferIndex(
                        cchars.ToString(CultureInfo.InvariantCulture), fieldOffset.ToString(CultureInfo.InvariantCulture)));
            }

            ndataIndex = (int)fieldOffset;

            // if no buffer is passed in, return the number of characters we have
            if (null == buffer)
            {
                return cchars;
            }

            try
            {
                if (ndataIndex < cchars)
                {
                    // help the user out in the case where there's less data than requested
                    if ((ndataIndex + length) > cchars)
                    {
                        cchars = cchars - ndataIndex;
                    }
                    else
                    {
                        cchars = length;
                    }
                }
                data.CopyTo(ndataIndex, buffer, bufferOffset, cchars);
            }
            catch (Exception e)
            {
                // UNDONE - should not be catching all exceptions!!!
                if (e.IsCatchableExceptionType())
                {
                    cchars = data.Length;

                    if (length < 0)
                    {
                        throw new IndexOutOfRangeException(
                            Strings.ADP_InvalidDataLength(((long)length).ToString(CultureInfo.InvariantCulture)));
                    }

                    // if bad buffer index, throw
                    if (bufferOffset < 0
                        || bufferOffset >= buffer.Length)
                    {
                        throw new ArgumentOutOfRangeException(
                            "bufferOffset", Strings.ADP_InvalidDestinationBufferIndex(
                                buffer.Length.ToString(CultureInfo.InvariantCulture), bufferOffset.ToString(CultureInfo.InvariantCulture)));
                    }

                    // if bad data index, throw
                    if (fieldOffset < 0
                        || fieldOffset >= cchars)
                    {
                        throw new ArgumentOutOfRangeException(
                            "fieldOffset", Strings.ADP_InvalidSourceBufferIndex(
                                cchars.ToString(CultureInfo.InvariantCulture), fieldOffset.ToString(CultureInfo.InvariantCulture)));
                    }

                    // if there is not enough room in the buffer for data
                    if (cchars + bufferOffset
                        > buffer.Length)
                    {
                        throw new IndexOutOfRangeException(
                            Strings.ADP_InvalidBufferSizeOrIndex(
                                cchars.ToString(CultureInfo.InvariantCulture), bufferOffset.ToString(CultureInfo.InvariantCulture)));
                    }
                }

                throw;
            }

            return cchars;
        }

        /// <summary>
        ///
        /// </summary>
        public DbDataRecord GetDataRecord(int ordinal)
        {
            return ((DbDataRecord)_values[ordinal]);
        }

        /// <summary>
        /// Used to return a nested result
        /// </summary>
        public DbDataReader GetDataReader(int i)
        {
            return GetDbDataReader(i);
        }

        /// <summary>
        ///
        /// </summary>
        public override string GetDataTypeName(int ordinal)
        {
            return GetMember(ordinal).TypeUsage.EdmType.Name;
        }

        /// <summary>
        ///
        /// </summary>
        public override DateTime GetDateTime(int ordinal)
        {
            return ((DateTime)_values[ordinal]);
        }

        /// <summary>
        ///
        /// </summary>
        public override Decimal GetDecimal(int ordinal)
        {
            return ((Decimal)_values[ordinal]);
        }

        /// <summary>
        ///
        /// </summary>
        public override double GetDouble(int ordinal)
        {
            return ((double)_values[ordinal]);
        }

        /// <summary>
        ///
        /// </summary>
        public override Type GetFieldType(int ordinal)
        {
            var edmMemberType = GetMember(ordinal).TypeUsage.EdmType;
            return edmMemberType.ClrType ?? typeof(Object);
        }

        /// <summary>
        ///
        /// </summary>
        public override float GetFloat(int ordinal)
        {
            return ((float)_values[ordinal]);
        }

        /// <summary>
        ///
        /// </summary>
        public override Guid GetGuid(int ordinal)
        {
            return ((Guid)_values[ordinal]);
        }

        /// <summary>
        ///
        /// </summary>
        public override Int16 GetInt16(int ordinal)
        {
            return ((Int16)_values[ordinal]);
        }

        /// <summary>
        ///
        /// </summary>
        public override Int32 GetInt32(int ordinal)
        {
            return ((Int32)_values[ordinal]);
        }

        /// <summary>
        ///
        /// </summary>
        public override Int64 GetInt64(int ordinal)
        {
            return ((Int64)_values[ordinal]);
        }

        /// <summary>
        ///
        /// </summary>
        public override string GetName(int ordinal)
        {
            return GetMember(ordinal).Name;
        }

        /// <summary>
        ///
        /// </summary>
        public override int GetOrdinal(string name)
        {
            if (null == _fieldNameLookup)
            {
                _fieldNameLookup = new FieldNameLookup(this, -1);
            }
            return _fieldNameLookup.GetOrdinal(name);
        }

        /// <summary>
        ///
        /// </summary>
        public override string GetString(int ordinal)
        {
            return ((string)_values[ordinal]);
        }

        /// <summary>
        ///
        /// </summary>
        public override object GetValue(int ordinal)
        {
            return _values[ordinal];
        }

        /// <summary>
        ///
        /// </summary>
        public override int GetValues(object[] values)
        {
            if (null == values)
            {
                throw new ArgumentNullException("values");
            }

            var copyLen = Math.Min(values.Length, FieldCount);
            for (var i = 0; i < copyLen; ++i)
            {
                values[i] = _values[i];
            }
            return copyLen;
        }

        private EdmMember GetMember(int ordinal)
        {
            return DataRecordInfo.FieldMetadata[ordinal].FieldType;
        }

        /// <summary>
        ///
        /// </summary>
        public override bool IsDBNull(int ordinal)
        {
            return (DBNull.Value == _values[ordinal]);
        }

        #region ICustomTypeDescriptor implementation

        //[barryfr] Reference: http://msdn.microsoft.com/msdnmag/issues/05/04/NETMatters/
        //Holds all of the PropertyDescriptors for the PrimitiveType objects in _values
        private PropertyDescriptorCollection _propertyDescriptors;
        private FilterCache _filterCache;
        //Stores an AttributeCollection for each PrimitiveType object in _values
        private Dictionary<object, AttributeCollection> _attrCache;

        //Holds the filtered properties and attributes last used when GetProperties(Attribute[]) was called.
        private class FilterCache
        {
            public Attribute[] Attributes;
            public PropertyDescriptorCollection FilteredProperties;
            //Verifies that this list of attributes matches the list passed into GetProperties(Attribute[])
            public bool IsValid(Attribute[] other)
            {
                if (other == null
                    || Attributes == null)
                {
                    return false;
                }

                if (Attributes.Length
                    != other.Length)
                {
                    return false;
                }

                for (var i = 0; i < other.Length; i++)
                {
                    if (!Attributes[i].Match(other[i]))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return null;
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return null;
        }

        /// <summary>
        /// Initialize the property descriptors for each PrimitiveType attribute.
        /// See similar functionality in DataRecordObjectView's ITypedList implementation.
        /// </summary>
        /// <returns></returns>
        private PropertyDescriptorCollection InitializePropertyDescriptors()
        {
            if (null == _values)
            {
                return null;
            }

            if (_propertyDescriptors == null
                && 0 < _values.Length)
            {
                // Create a new PropertyDescriptorCollection with read-only properties
                _propertyDescriptors = CreatePropertyDescriptorCollection(
                    DataRecordInfo.RecordType.EdmType as StructuralType,
                    typeof(MaterializedDataRecord), true);
            }

            return _propertyDescriptors;
        }

        /// <summary>
        /// Creates a PropertyDescriptorCollection based on a StructuralType definition
        /// Currently this includes a PropertyDescriptor for each primitive type property in the StructuralType
        /// </summary>
        /// <param name="structuralType">The structural type definition</param>
        /// <param name="componentType">The type to use as the component type</param>
        /// <param name="isReadOnly">Whether the properties in the collection should be read only or not</param>
        /// <returns></returns>
        internal static PropertyDescriptorCollection CreatePropertyDescriptorCollection(
            StructuralType structuralType, Type componentType, bool isReadOnly)
        {
            var pdList = new List<PropertyDescriptor>();
            if (structuralType != null)
            {
                foreach (var member in structuralType.Members)
                {
                    if (member.BuiltInTypeKind
                        == BuiltInTypeKind.EdmProperty)
                    {
                        var edmPropertyMember = (EdmProperty)member;

                        var fd = new FieldDescriptor(componentType, isReadOnly, edmPropertyMember);
                        pdList.Add(fd);
                    }
                }
            }
            return (new PropertyDescriptorCollection(pdList.ToArray()));
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(null);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            var filtering = (null != attributes && 0 < attributes.Length);

            var props = InitializePropertyDescriptors();
            if (props == null)
            {
                return props;
            }

            var cache = _filterCache;

            // Use a cached version if possible
            if (filtering && cache != null
                && cache.IsValid(attributes))
            {
                return cache.FilteredProperties;
            }
            else if (!filtering
                     && props != null)
            {
                return props;
            }

            //Build up the attribute cache, since our PropertyDescriptor doesn't store it internally.
            // _values is set only during construction.
            if (null == _attrCache && null != attributes
                && 0 < attributes.Length)
            {
                _attrCache = new Dictionary<object, AttributeCollection>();
                foreach (FieldDescriptor pd in _propertyDescriptors)
                {
                    var o = pd.GetValue(this);
                    var atts = o.GetType().GetCustomAttributes( /*inherit*/false); //atts will not be null (atts.Length==0)
                    var attrArray = new Attribute[atts.Length];
                    atts.CopyTo(attrArray, 0);
                    _attrCache.Add(pd, new AttributeCollection(attrArray));
                }
            }

            //Create the filter based on the attributes.
            props = new PropertyDescriptorCollection(null);
            foreach (PropertyDescriptor pd in _propertyDescriptors)
            {
                if (_attrCache[pd].Matches(attributes))
                {
                    props.Add(pd);
                }
            }

            // Store the computed properties
            if (filtering)
            {
                cache = new FilterCache();
                cache.Attributes = attributes;
                cache.FilteredProperties = props;
                _filterCache = cache;
            }

            return props;
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion
    }
}
