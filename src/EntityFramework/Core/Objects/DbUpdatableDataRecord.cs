namespace System.Data.Entity.Core.Objects
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// 
    /// </summary>
    public abstract class DbUpdatableDataRecord : DbDataRecord, IExtendedDataRecord
    {
        internal readonly StateManagerTypeMetadata _metadata;
        internal readonly ObjectStateEntry _cacheEntry;
        internal readonly object _userObject;
        internal DataRecordInfo _recordInfo;

        internal DbUpdatableDataRecord(ObjectStateEntry cacheEntry, StateManagerTypeMetadata metadata, object userObject)
        {
            _cacheEntry = cacheEntry;
            _userObject = userObject;
            _metadata = metadata;
        }

        internal DbUpdatableDataRecord(ObjectStateEntry cacheEntry)
            :
                this(cacheEntry, null, null)
        {
        }

        /// <summary>
        /// Returns the number of fields in the record.
        /// </summary>
        public override int FieldCount
        {
            get
            {
                Debug.Assert(_cacheEntry != null, "CacheEntry is required.");
                return _cacheEntry.GetFieldCount(_metadata);
            }
        }

        /// <summary>
        /// Retrieves a value with the given field ordinal
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <returns>The field value</returns>
        public override object this[int i]
        {
            get { return GetValue(i); }
        }

        /// <summary>
        /// Retrieves a value with the given field name
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <returns>The field value</returns>
        public override object this[string name]
        {
            get { return GetValue(GetOrdinal(name)); }
        }

        /// <summary>
        /// Retrieves the field value as a boolean
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <returns>The field value as a boolean</returns>
        public override bool GetBoolean(int i)
        {
            return (bool)GetValue(i);
        }

        /// <summary>
        /// Retrieves the field value as a byte
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <returns>The field value as a byte</returns>
        public override byte GetByte(int i)
        {
            return (byte)GetValue(i);
        }

        /// <summary>
        /// Retrieves the field value as a byte array
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <param name="dataIndex">The index at which to start copying data</param>
        /// <param name="buffer">The destination buffer where data is copied to</param>
        /// <param name="bufferIndex">The index in the destination buffer where copying will begin</param>
        /// <param name="length">The number of bytes to copy</param>
        /// <returns>The number of bytes copied</returns>
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        public override long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            byte[] tempBuffer;
            tempBuffer = (byte[])GetValue(i);

            if (buffer == null)
            {
                return tempBuffer.Length;
            }
            var srcIndex = (int)dataIndex;
            var byteCount = Math.Min(tempBuffer.Length - srcIndex, length);
            if (srcIndex < 0)
            {
                throw new ArgumentOutOfRangeException("dataIndex", Strings.ADP_InvalidSourceBufferIndex(
                    tempBuffer.Length.ToString(CultureInfo.InvariantCulture), ((long)srcIndex).ToString(CultureInfo.InvariantCulture)));
            }
            else if ((bufferIndex < 0)
                     || (bufferIndex > 0 && bufferIndex >= buffer.Length))
            {
                throw new ArgumentOutOfRangeException("bufferIndex", Strings.ADP_InvalidDestinationBufferIndex(
                    buffer.Length.ToString(CultureInfo.InvariantCulture), bufferIndex.ToString(CultureInfo.InvariantCulture)));
            }

            if (0 < byteCount)
            {
                Array.Copy(tempBuffer, dataIndex, buffer, bufferIndex, byteCount);
            }
            else if (length < 0)
            {
                throw new IndexOutOfRangeException(Strings.ADP_InvalidDataLength(((long)length).ToString(CultureInfo.InvariantCulture)));
            }
            else
            {
                byteCount = 0;
            }
            return byteCount;
        }

        /// <summary>
        /// Retrieves the field value as a char
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <returns>The field value as a char</returns>
        public override char GetChar(int i)
        {
            return (char)GetValue(i);
        }

        /// <summary>
        /// Retrieves the field value as a char array
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <param name="dataIndex">The index at which to start copying data</param>
        /// <param name="buffer">The destination buffer where data is copied to</param>
        /// <param name="bufferIndex">The index in the destination buffer where copying will begin</param>
        /// <param name="length">The number of chars to copy</param>
        /// <returns>The number of chars copied</returns>
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        public override long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            char[] tempBuffer;
            tempBuffer = (char[])GetValue(i);

            if (buffer == null)
            {
                return tempBuffer.Length;
            }

            var srcIndex = (int)dataIndex;
            var charCount = Math.Min(tempBuffer.Length - srcIndex, length);
            if (srcIndex < 0)
            {
                throw new ArgumentOutOfRangeException("dataIndex", Strings.ADP_InvalidSourceBufferIndex(
                    tempBuffer.Length.ToString(CultureInfo.InvariantCulture), ((long)srcIndex).ToString(CultureInfo.InvariantCulture)));
            }
            else if ((bufferIndex < 0)
                     || (bufferIndex > 0 && bufferIndex >= buffer.Length))
            {
                throw new ArgumentOutOfRangeException("bufferIndex", Strings.ADP_InvalidDestinationBufferIndex(
                    buffer.Length.ToString(CultureInfo.InvariantCulture), bufferIndex.ToString(CultureInfo.InvariantCulture)));
            }

            if (0 < charCount)
            {
                Array.Copy(tempBuffer, dataIndex, buffer, bufferIndex, charCount);
            }
            else if (length < 0)
            {
                throw new IndexOutOfRangeException(Strings.ADP_InvalidDataLength(((long)length).ToString(CultureInfo.InvariantCulture)));
            }
            else
            {
                charCount = 0;
            }
            return charCount;
        }

        IDataReader IDataRecord.GetData(int ordinal)
        {
            return GetDbDataReader(ordinal);
        }

        /// <summary>
        /// Retrieves the field value as a DbDataReader
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <returns></returns>
        protected override DbDataReader GetDbDataReader(int i)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Retrieves the name of the field data type
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <returns>The name of the field data type</returns>
        public override string GetDataTypeName(int i)
        {
            return (GetFieldType(i)).Name;
        }

        /// <summary>
        /// Retrieves the field value as a DateTime
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <returns>The field value as a DateTime</returns>
        public override DateTime GetDateTime(int i)
        {
            return (DateTime)GetValue(i);
        }

        /// <summary>
        /// Retrieves the field value as a decimal
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <returns>The field value as a decimal</returns>
        public override Decimal GetDecimal(int i)
        {
            return (Decimal)GetValue(i);
        }

        /// <summary>
        /// Retrieves the field value as a double
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <returns>The field value as a double</returns>
        public override double GetDouble(int i)
        {
            return (double)GetValue(i);
        }

        /// <summary>
        /// Retrieves the type of a field
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <returns>The field type</returns>
        public override Type GetFieldType(int i)
        {
            Debug.Assert(_cacheEntry != null, "CacheEntry is required.");
            return _cacheEntry.GetFieldType(i, _metadata);
        }

        /// <summary>
        /// Retrieves the field value as a float
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <returns>The field value as a float</returns>
        public override float GetFloat(int i)
        {
            return (float)GetValue(i);
        }

        /// <summary>
        /// Retrieves the field value as a Guid
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <returns>The field value as a Guid</returns>
        public override Guid GetGuid(int i)
        {
            return (Guid)GetValue(i);
        }

        /// <summary>
        /// Retrieves the field value as an Int16
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <returns>The field value as an Int16</returns>
        public override Int16 GetInt16(int i)
        {
            return (Int16)GetValue(i);
        }

        /// <summary>
        /// Retrieves the field value as an Int32
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <returns>The field value as an Int32</returns>
        public override Int32 GetInt32(int i)
        {
            return (Int32)GetValue(i);
        }

        /// <summary>
        /// Retrieves the field value as an Int64
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <returns>The field value as an Int64</returns>
        public override Int64 GetInt64(int i)
        {
            return (Int64)GetValue(i);
        }

        /// <summary>
        /// Retrieves the name of a field
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <returns>The name of the field</returns>
        public override string GetName(int i)
        {
            Debug.Assert(_cacheEntry != null, "CacheEntry is required.");
            return _cacheEntry.GetCLayerName(i, _metadata);
        }

        /// <summary>
        /// Retrieves the ordinal of a field by name
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <returns>The ordinal of the field</returns>
        public override int GetOrdinal(string name)
        {
            Debug.Assert(_cacheEntry != null, "CacheEntry is required.");
            var ordinal = _cacheEntry.GetOrdinalforCLayerName(name, _metadata);
            if (ordinal == -1)
            {
                throw new ArgumentOutOfRangeException("name");
            }
            return ordinal;
        }

        /// <summary>
        /// Retrieves the field value as a string
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <returns>The field value as a string</returns>
        public override string GetString(int i)
        {
            return (string)GetValue(i);
        }

        /// <summary>
        /// Retrieves the value of a field
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <returns>The field value</returns>
        public override object GetValue(int i)
        {
            return GetRecordValue(i);
        }

        /// <summary>
        /// In derived classes, retrieves the record value for a field
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <returns>The field value</returns>
        protected abstract object GetRecordValue(int ordinal);

        /// <summary>
        /// Retrieves all field values in the record into an object array
        /// </summary>
        /// <param name="values">An array of objects to store the field values</param>
        /// <returns>The number of field values returned</returns>
        public override int GetValues(object[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            var minValue = Math.Min(values.Length, FieldCount);
            for (var i = 0; i < minValue; i++)
            {
                values[i] = GetValue(i);
            }
            return minValue;
        }

        /// <summary>
        /// Determines if a field has a DBNull value
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <returns>True if the field has a DBNull value</returns>
        public override bool IsDBNull(int i)
        {
            return (GetValue(i) == DBNull.Value);
        }

        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value"></param>
        public void SetBoolean(int ordinal, bool value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value"></param>
        public void SetByte(int ordinal, byte value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetChar(int ordinal, char value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetDataRecord(int ordinal, IDataRecord value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetDateTime(int ordinal, DateTime value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetDecimal(int ordinal, Decimal value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetDouble(int ordinal, Double value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetFloat(int ordinal, float value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetGuid(int ordinal, Guid value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetInt16(int ordinal, Int16 value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetInt32(int ordinal, Int32 value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetInt64(int ordinal, Int64 value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetString(int ordinal, string value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>
        /// Sets the value of a field in a record
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        /// <param name="value">The new field value</param>
        public void SetValue(int ordinal, object value)
        {
            SetRecordValue(ordinal, value);
        }

        /// <summary>
        /// Sets field values in a record
        /// </summary>
        /// <param name="values"></param>
        /// <returns>The number of fields that were set</returns>
        public int SetValues(params Object[] values)
        {
            var minValue = Math.Min(values.Length, FieldCount);
            for (var i = 0; i < minValue; i++)
            {
                SetRecordValue(i, values[i]);
            }
            return minValue;
        }

        /// <summary>
        /// Sets a field to the DBNull value
        /// </summary>
        /// <param name="ordinal">The ordinal of the field</param>
        public void SetDBNull(int ordinal)
        {
            SetRecordValue(ordinal, DBNull.Value);
        }

        /// <summary>
        /// Retrieve data record information
        /// </summary>
        public virtual DataRecordInfo DataRecordInfo
        {
            get
            {
                if (null == _recordInfo)
                {
                    Debug.Assert(_cacheEntry != null, "CacheEntry is required.");
                    _recordInfo = _cacheEntry.GetDataRecordInfo(_metadata, _userObject);
                }
                return _recordInfo;
            }
        }

        /// <summary>
        /// Retrieves a field value as a DbDataRecord
        /// </summary>
        /// <param name="i">The ordinal of the field</param>
        /// <returns>The field value as a DbDataRecord</returns>
        public DbDataRecord GetDataRecord(int i)
        {
            return (DbDataRecord)GetValue(i);
        }

        /// <summary>
        /// Used to return a nested result
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public DbDataReader GetDataReader(int i)
        {
            return GetDbDataReader(i);
        }

        /// <summary>
        /// Sets the field value for a given ordinal
        /// </summary>
        /// <param name="ordinal">in the cspace mapping</param>
        /// <param name="value">in CSpace</param>
        protected abstract void SetRecordValue(int ordinal, object value);
    }
}