// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// Provides access to the original values of object data. The DbUpdatableDataRecord implements methods that allow updates to the original values of an object.
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

        /// <summary>Gets the number of fields in the record.</summary>
        /// <returns>An integer value that is the field count.</returns>
        public override int FieldCount
        {
            get
            {
                Debug.Assert(_cacheEntry != null, "CacheEntry is required.");
                return _cacheEntry.GetFieldCount(_metadata);
            }
        }

        /// <summary>Returns a value that has the given field ordinal.</summary>
        /// <returns>The value that has the given field ordinal.</returns>
        /// <param name="i">The ordinal of the field.</param>
        public override object this[int i]
        {
            get { return GetValue(i); }
        }

        /// <summary>Gets a value that has the given field name.</summary>
        /// <returns>The field value.</returns>
        /// <param name="name">The name of the field.</param>
        public override object this[string name]
        {
            get { return GetValue(GetOrdinal(name)); }
        }

        /// <summary>Retrieves the field value as a Boolean.</summary>
        /// <returns>The field value as a Boolean.</returns>
        /// <param name="i">The ordinal of the field.</param>
        public override bool GetBoolean(int i)
        {
            return (bool)GetValue(i);
        }

        /// <summary>Retrieves the field value as a byte.</summary>
        /// <returns>The field value as a byte.</returns>
        /// <param name="i">The ordinal of the field.</param>
        public override byte GetByte(int i)
        {
            return (byte)GetValue(i);
        }

        /// <summary>Retrieves the field value as a byte array.</summary>
        /// <returns>The number of bytes copied.</returns>
        /// <param name="i">The ordinal of the field.</param>
        /// <param name="dataIndex">The index at which to start copying data.</param>
        /// <param name="buffer">The destination buffer where data is copied.</param>
        /// <param name="bufferIndex">The index in the destination buffer where copying will begin.</param>
        /// <param name="length">The number of bytes to copy.</param>
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
                throw new ArgumentOutOfRangeException(
                    "dataIndex", Strings.ADP_InvalidSourceBufferIndex(
                        tempBuffer.Length.ToString(CultureInfo.InvariantCulture), ((long)srcIndex).ToString(CultureInfo.InvariantCulture)));
            }
            else if ((bufferIndex < 0)
                     || (bufferIndex > 0 && bufferIndex >= buffer.Length))
            {
                throw new ArgumentOutOfRangeException(
                    "bufferIndex", Strings.ADP_InvalidDestinationBufferIndex(
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

        /// <summary>Retrieves the field value as a char.</summary>
        /// <returns>The field value as a char.</returns>
        /// <param name="i">The ordinal of the field.</param>
        public override char GetChar(int i)
        {
            return (char)GetValue(i);
        }

        /// <summary>Retrieves the field value as a char array.</summary>
        /// <returns>The number of characters copied.</returns>
        /// <param name="i">The ordinal of the field.</param>
        /// <param name="dataIndex">The index at which to start copying data.</param>
        /// <param name="buffer">The destination buffer where data is copied.</param>
        /// <param name="bufferIndex">The index in the destination buffer where copying will begin.</param>
        /// <param name="length">The number of characters to copy.</param>
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
                throw new ArgumentOutOfRangeException(
                    "dataIndex", Strings.ADP_InvalidSourceBufferIndex(
                        tempBuffer.Length.ToString(CultureInfo.InvariantCulture), ((long)srcIndex).ToString(CultureInfo.InvariantCulture)));
            }
            else if ((bufferIndex < 0)
                     || (bufferIndex > 0 && bufferIndex >= buffer.Length))
            {
                throw new ArgumentOutOfRangeException(
                    "bufferIndex", Strings.ADP_InvalidDestinationBufferIndex(
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

        /// <summary>
        /// Retrieves the field value as an <see cref="T:System.Data.IDataReader" />.
        /// </summary>
        /// <returns>
        /// The field value as an <see cref="T:System.Data.IDataReader" />.
        /// </returns>
        /// <param name="ordinal">The ordinal of the field.</param>
        IDataReader IDataRecord.GetData(int ordinal)
        {
            return GetDbDataReader(ordinal);
        }

        /// <summary>
        /// Retrieves the field value as a <see cref="T:System.Data.Common.DbDataReader" />
        /// </summary>
        /// <returns>
        /// The field value as a <see cref="T:System.Data.Common.DbDataReader" />.
        /// </returns>
        /// <param name="i">The ordinal of the field.</param>
        protected override DbDataReader GetDbDataReader(int i)
        {
            throw new NotSupportedException();
        }

        /// <summary>Retrieves the name of the field data type.</summary>
        /// <returns>The name of the field data type.</returns>
        /// <param name="i">The ordinal of the field.</param>
        public override string GetDataTypeName(int i)
        {
            return (GetFieldType(i)).Name;
        }

        /// <summary>
        /// Retrieves the field value as a <see cref="T:System.DateTime" />.
        /// </summary>
        /// <returns>
        /// The field value as a <see cref="T:System.DateTime" />.
        /// </returns>
        /// <param name="i">The ordinal of the field.</param>
        public override DateTime GetDateTime(int i)
        {
            return (DateTime)GetValue(i);
        }

        /// <summary>Retrieves the field value as a decimal.</summary>
        /// <returns>The field value as a decimal.</returns>
        /// <param name="i">The ordinal of the field.</param>
        public override Decimal GetDecimal(int i)
        {
            return (Decimal)GetValue(i);
        }

        /// <summary>Retrieves the field value as a double.</summary>
        /// <returns>The field value as a double.</returns>
        /// <param name="i">The ordinal of the field.</param>
        public override double GetDouble(int i)
        {
            return (double)GetValue(i);
        }

        /// <summary>Retrieves the type of a field.</summary>
        /// <returns>The field type.</returns>
        /// <param name="i">The ordinal of the field.</param>
        public override Type GetFieldType(int i)
        {
            Debug.Assert(_cacheEntry != null, "CacheEntry is required.");
            return _cacheEntry.GetFieldType(i, _metadata);
        }

        /// <summary>Retrieves the field value as a float.</summary>
        /// <returns>The field value as a float.</returns>
        /// <param name="i">The ordinal of the field.</param>
        public override float GetFloat(int i)
        {
            return (float)GetValue(i);
        }

        /// <summary>
        /// Retrieves the field value as a <see cref="T:System.Guid" />.
        /// </summary>
        /// <returns>
        /// The field value as a <see cref="T:System.Guid" />.
        /// </returns>
        /// <param name="i">The ordinal of the field.</param>
        public override Guid GetGuid(int i)
        {
            return (Guid)GetValue(i);
        }

        /// <summary>
        /// Retrieves the field value as an <see cref="T:System.Int16" />.
        /// </summary>
        /// <returns>
        /// The field value as an <see cref="T:System.Int16" />.
        /// </returns>
        /// <param name="i">The ordinal of the field.</param>
        public override Int16 GetInt16(int i)
        {
            return (Int16)GetValue(i);
        }

        /// <summary>
        /// Retrieves the field value as an <see cref="T:System.Int32" />.
        /// </summary>
        /// <returns>
        /// The field value as an <see cref="T:System.Int32" />.
        /// </returns>
        /// <param name="i">The ordinal of the field.</param>
        public override Int32 GetInt32(int i)
        {
            return (Int32)GetValue(i);
        }

        /// <summary>
        /// Retrieves the field value as an <see cref="T:System.Int64" />.
        /// </summary>
        /// <returns>
        /// The field value as an <see cref="T:System.Int64" />.
        /// </returns>
        /// <param name="i">The ordinal of the field.</param>
        public override Int64 GetInt64(int i)
        {
            return (Int64)GetValue(i);
        }

        /// <summary>Retrieves the name of a field.</summary>
        /// <returns>The name of the field.</returns>
        /// <param name="i">The ordinal of the field.</param>
        public override string GetName(int i)
        {
            Debug.Assert(_cacheEntry != null, "CacheEntry is required.");
            return _cacheEntry.GetCLayerName(i, _metadata);
        }

        /// <summary>Retrieves the ordinal of a field by using the name of the field.</summary>
        /// <returns>The ordinal of the field.</returns>
        /// <param name="name">The name of the field.</param>
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

        /// <summary>Retrieves the field value as a string.</summary>
        /// <returns>The field value.</returns>
        /// <param name="i">The ordinal of the field.</param>
        public override string GetString(int i)
        {
            return (string)GetValue(i);
        }

        /// <summary>Retrieves the value of a field.</summary>
        /// <returns>The field value.</returns>
        /// <param name="i">The ordinal of the field.</param>
        public override object GetValue(int i)
        {
            return GetRecordValue(i);
        }

        /// <summary>Retrieves the value of a field.</summary>
        /// <returns>The field value.</returns>
        /// <param name="ordinal">The ordinal of the field.</param>
        protected abstract object GetRecordValue(int ordinal);

        /// <summary>Populates an array of objects with the field values of the current record.</summary>
        /// <returns>The number of field values returned.</returns>
        /// <param name="values">An array of objects to store the field values.</param>
        public override int GetValues(object[] values)
        {
            Check.NotNull(values, "values");

            var minValue = Math.Min(values.Length, FieldCount);
            for (var i = 0; i < minValue; i++)
            {
                values[i] = GetValue(i);
            }
            return minValue;
        }

        /// <summary>
        /// Returns whether the specified field is set to <see cref="T:System.DBNull" />.
        /// </summary>
        /// <returns>
        /// true if the field is set to <see cref="T:System.DBNull" />; otherwise false.
        /// </returns>
        /// <param name="i">The ordinal of the field.</param>
        public override bool IsDBNull(int i)
        {
            return (GetValue(i) == DBNull.Value);
        }

        /// <summary>Sets the value of a field in a record.</summary>
        /// <param name="ordinal">The ordinal of the field.</param>
        /// <param name="value">The value of the field.</param>
        public void SetBoolean(int ordinal, bool value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>Sets the value of a field in a record.</summary>
        /// <param name="ordinal">The ordinal of the field.</param>
        /// <param name="value">The value of the field.</param>
        public void SetByte(int ordinal, byte value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>Sets the value of a field in a record.</summary>
        /// <param name="ordinal">The ordinal of the field.</param>
        /// <param name="value">The value of the field.</param>
        public void SetChar(int ordinal, char value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>Sets the value of a field in a record.</summary>
        /// <param name="ordinal">The ordinal of the field.</param>
        /// <param name="value">The value of the field.</param>
        public void SetDataRecord(int ordinal, IDataRecord value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>Sets the value of a field in a record.</summary>
        /// <param name="ordinal">The ordinal of the field.</param>
        /// <param name="value">The value of the field.</param>
        public void SetDateTime(int ordinal, DateTime value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>Sets the value of a field in a record.</summary>
        /// <param name="ordinal">The ordinal of the field.</param>
        /// <param name="value">The value of the field.</param>
        public void SetDecimal(int ordinal, Decimal value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>Sets the value of a field in a record.</summary>
        /// <param name="ordinal">The ordinal of the field.</param>
        /// <param name="value">The value of the field.</param>
        public void SetDouble(int ordinal, Double value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>Sets the value of a field in a record.</summary>
        /// <param name="ordinal">The ordinal of the field.</param>
        /// <param name="value">The value of the field.</param>
        public void SetFloat(int ordinal, float value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>Sets the value of a field in a record.</summary>
        /// <param name="ordinal">The ordinal of the field.</param>
        /// <param name="value">The value of the field.</param>
        public void SetGuid(int ordinal, Guid value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>Sets the value of a field in a record.</summary>
        /// <param name="ordinal">The ordinal of the field.</param>
        /// <param name="value">The value of the field.</param>
        public void SetInt16(int ordinal, Int16 value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>Sets the value of a field in a record.</summary>
        /// <param name="ordinal">The ordinal of the field.</param>
        /// <param name="value">The value of the field.</param>
        public void SetInt32(int ordinal, Int32 value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>Sets the value of a field in a record.</summary>
        /// <param name="ordinal">The ordinal of the field.</param>
        /// <param name="value">The value of the field.</param>
        public void SetInt64(int ordinal, Int64 value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>Sets the value of a field in a record.</summary>
        /// <param name="ordinal">The ordinal of the field.</param>
        /// <param name="value">The value of the field.</param>
        public void SetString(int ordinal, string value)
        {
            SetValue(ordinal, value);
        }

        /// <summary>Sets the value of a field in a record.</summary>
        /// <param name="ordinal">The ordinal of the field.</param>
        /// <param name="value">The value of the field.</param>
        public void SetValue(int ordinal, object value)
        {
            SetRecordValue(ordinal, value);
        }

        /// <summary>Sets field values in a record.</summary>
        /// <returns>The number of the fields that were set.</returns>
        /// <param name="values">The values of the field.</param>
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
        /// Sets a field to the <see cref="T:System.DBNull" /> value.
        /// </summary>
        /// <param name="ordinal">The ordinal of the field.</param>
        public void SetDBNull(int ordinal)
        {
            SetRecordValue(ordinal, DBNull.Value);
        }

        /// <summary>Gets data record information.</summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Common.DataRecordInfo" /> object.
        /// </returns>
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
        /// Retrieves a field value as a <see cref="T:System.Data.Common.DbDataRecord" />.
        /// </summary>
        /// <returns>
        /// A field value as a <see cref="T:System.Data.Common.DbDataRecord" />.
        /// </returns>
        /// <param name="i">The ordinal of the field.</param>
        public DbDataRecord GetDataRecord(int i)
        {
            return (DbDataRecord)GetValue(i);
        }

        /// <summary>
        /// Retrieves the field value as a <see cref="T:System.Data.Common.DbDataReader" />.
        /// </summary>
        /// <returns>
        /// The field value as a <see cref="T:System.Data.Common.DbDataReader" />.
        /// </returns>
        /// <param name="i">The ordinal of the field.</param>
        public DbDataReader GetDataReader(int i)
        {
            return GetDbDataReader(i);
        }

        /// <summary>Sets the value of a field in a record.</summary>
        /// <param name="ordinal">The ordinal of the field.</param>
        /// <param name="value">The value of the field.</param>
        protected abstract void SetRecordValue(int ordinal, object value);
    }
}
