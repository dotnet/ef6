namespace System.Data.Entity.Core.Objects
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Diagnostics;

    internal sealed class ObjectStateEntryDbDataRecord : DbDataRecord, IExtendedDataRecord
    {
        private readonly StateManagerTypeMetadata _metadata;
        private readonly ObjectStateEntry _cacheEntry;
        private readonly object _userObject;
        private DataRecordInfo _recordInfo;

        internal ObjectStateEntryDbDataRecord(EntityEntry cacheEntry, StateManagerTypeMetadata metadata, object userObject)
        {
            EntityUtil.CheckArgumentNull(cacheEntry, "cacheEntry");
            EntityUtil.CheckArgumentNull(userObject, "userObject");
            EntityUtil.CheckArgumentNull(metadata, "metadata");
            Debug.Assert(!cacheEntry.IsKeyEntry, "Cannot create an ObjectStateEntryDbDataRecord for a key entry");
            switch (cacheEntry.State)
            {
                case EntityState.Unchanged:
                case EntityState.Modified:
                case EntityState.Deleted:
                    _cacheEntry = cacheEntry;
                    _userObject = userObject;
                    _metadata = metadata;
                    break;
                default:
                    Debug.Assert(false, "A DbDataRecord cannot be created for an entity object that is in an added or detached state.");
                    break;
            }
        }

        internal ObjectStateEntryDbDataRecord(RelationshipEntry cacheEntry)
        {
            EntityUtil.CheckArgumentNull(cacheEntry, "cacheEntry");
            Debug.Assert(!cacheEntry.IsKeyEntry, "Cannot create an ObjectStateEntryDbDataRecord for a key entry");
            switch (cacheEntry.State)
            {
                case EntityState.Unchanged:
                case EntityState.Modified:
                case EntityState.Deleted:
                    _cacheEntry = cacheEntry;
                    break;
                default:
                    Debug.Assert(false, "A DbDataRecord cannot be created for an entity object that is in an added or detached state.");
                    break;
            }
        }

        public override int FieldCount
        {
            get
            {
                Debug.Assert(_cacheEntry != null, "CacheEntry is required.");
                return _cacheEntry.GetFieldCount(_metadata);
            }
        }

        public override object this[int ordinal]
        {
            get { return GetValue(ordinal); }
        }

        public override object this[string name]
        {
            get { return GetValue(GetOrdinal(name)); }
        }

        public override bool GetBoolean(int ordinal)
        {
            return (bool)GetValue(ordinal);
        }

        public override byte GetByte(int ordinal)
        {
            return (byte)GetValue(ordinal);
        }

        public override long GetBytes(int ordinal, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            byte[] tempBuffer;
            tempBuffer = (byte[])GetValue(ordinal);

            if (buffer == null)
            {
                return tempBuffer.Length;
            }
            var srcIndex = (int)dataIndex;
            var byteCount = Math.Min(tempBuffer.Length - srcIndex, length);
            if (srcIndex < 0)
            {
                throw EntityUtil.InvalidSourceBufferIndex(tempBuffer.Length, srcIndex, "dataIndex");
            }
            else if ((bufferIndex < 0)
                     || (bufferIndex > 0 && bufferIndex >= buffer.Length))
            {
                throw EntityUtil.InvalidDestinationBufferIndex(buffer.Length, bufferIndex, "bufferIndex");
            }

            if (0 < byteCount)
            {
                Array.Copy(tempBuffer, dataIndex, buffer, bufferIndex, byteCount);
            }
            else if (length < 0)
            {
                throw EntityUtil.InvalidDataLength(length);
            }
            else
            {
                byteCount = 0;
            }
            return byteCount;
        }

        public override char GetChar(int ordinal)
        {
            return (char)GetValue(ordinal);
        }

        public override long GetChars(int ordinal, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            char[] tempBuffer;
            tempBuffer = (char[])GetValue(ordinal);

            if (buffer == null)
            {
                return tempBuffer.Length;
            }

            var srcIndex = (int)dataIndex;
            var charCount = Math.Min(tempBuffer.Length - srcIndex, length);
            if (srcIndex < 0)
            {
                throw EntityUtil.InvalidSourceBufferIndex(buffer.Length, bufferIndex, "bufferIndex");
            }
            else if ((bufferIndex < 0)
                     || (bufferIndex > 0 && bufferIndex >= buffer.Length))
            {
                throw EntityUtil.InvalidDestinationBufferIndex(buffer.Length, bufferIndex, "bufferIndex");
            }

            if (0 < charCount)
            {
                Array.Copy(tempBuffer, dataIndex, buffer, bufferIndex, charCount);
            }
            else if (length < 0)
            {
                throw EntityUtil.InvalidDataLength(length);
            }
            else
            {
                charCount = 0;
            }
            return charCount;
        }

        protected override DbDataReader GetDbDataReader(int ordinal)
        {
            throw EntityUtil.NotSupported();
        }

        public override string GetDataTypeName(int ordinal)
        {
            return (GetFieldType(ordinal)).Name;
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return (DateTime)GetValue(ordinal);
        }

        public override Decimal GetDecimal(int ordinal)
        {
            return (Decimal)GetValue(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            return (Double)GetValue(ordinal);
        }

        public override Type GetFieldType(int ordinal)
        {
            return _cacheEntry.GetFieldType(ordinal, _metadata);
        }

        public override float GetFloat(int ordinal)
        {
            return (float)GetValue(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            return (Guid)GetValue(ordinal);
        }

        public override Int16 GetInt16(int ordinal)
        {
            return (Int16)GetValue(ordinal);
        }

        public override Int32 GetInt32(int ordinal)
        {
            return (Int32)GetValue(ordinal);
        }

        public override Int64 GetInt64(int ordinal)
        {
            return (Int64)GetValue(ordinal);
        }

        public override string GetName(int ordinal)
        {
            return _cacheEntry.GetCLayerName(ordinal, _metadata);
        }

        public override int GetOrdinal(string name)
        {
            var ordinal = _cacheEntry.GetOrdinalforCLayerName(name, _metadata);
            if (ordinal == -1)
            {
                throw EntityUtil.ArgumentOutOfRange("name");
            }
            return ordinal;
        }

        public override string GetString(int ordinal)
        {
            return (string)GetValue(ordinal);
        }

        public override object GetValue(int ordinal)
        {
            if (_cacheEntry.IsRelationship)
            {
                return (_cacheEntry as RelationshipEntry).GetOriginalRelationValue(ordinal);
            }
            else
            {
                return (_cacheEntry as EntityEntry).GetOriginalEntityValue(
                    _metadata, ordinal, _userObject, ObjectStateValueRecord.OriginalReadonly);
            }
        }

        public override int GetValues(object[] values)
        {
            if (values == null)
            {
                throw EntityUtil.ArgumentNull("values");
            }
            var minValue = Math.Min(values.Length, FieldCount);
            for (var i = 0; i < minValue; i++)
            {
                values[i] = GetValue(i);
            }
            return minValue;
        }

        public override bool IsDBNull(int ordinal)
        {
            return (GetValue(ordinal) == DBNull.Value);
        }

        public DataRecordInfo DataRecordInfo
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

        public DbDataRecord GetDataRecord(int ordinal)
        {
            return (DbDataRecord)GetValue(ordinal);
        }

        public DbDataReader GetDataReader(int i)
        {
            return GetDbDataReader(i);
        }
    }
}
