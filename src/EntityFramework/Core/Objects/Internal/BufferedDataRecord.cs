// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Common;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
#if !NET40
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
#endif

    internal class BufferedDataRecord : DbDataRecord
    {
        private int _currentRow = -1;

        private readonly List<object[]> _resultSet;
        private readonly string[] _dataTypeNames;
        private readonly Type[] _fieldTypes;
        private readonly string[] _columnNames;
        private readonly FieldNameLookup _fieldNameLookup;

        public BufferedDataRecord(List<object[]> resultSet, string[] dataTypeNames, Type[] fieldTypes, string[] columnNames)
        {
            DebugCheck.NotNull(resultSet);
            DebugCheck.NotNull(dataTypeNames);
            DebugCheck.NotNull(fieldTypes);
            DebugCheck.NotNull(columnNames);
            Debug.Assert(dataTypeNames.Length == fieldTypes.Length);
            Debug.Assert(fieldTypes.Length == columnNames.Length);

            _resultSet = resultSet;
            _dataTypeNames = dataTypeNames;
            _fieldTypes = fieldTypes;
            _columnNames = columnNames;
            _fieldNameLookup = new FieldNameLookup(new ReadOnlyCollection<string>(columnNames), -1);
        }

        public override object this[string name]
        {
            get { return GetValue(GetOrdinal(name)); }
        }

        public override object this[int ordinal]
        {
            get { return GetValue(ordinal); }
        }
        
        public bool HasData
        {
            get { return 0 <= _currentRow && _currentRow < _resultSet.Count; }
        }

        public bool HasRows
        {
            get
            {
                return _resultSet.Count > 0;
            }
        }

        public override int FieldCount
        {
            get { return _dataTypeNames.Length; }
        }

        public override bool GetBoolean(int ordinal)
        {
            return GetFieldValue<bool>(ordinal);
        }

        public override byte GetByte(int ordinal)
        {
            return GetFieldValue<byte>(ordinal);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotSupportedException();
        }

        public override char GetChar(int ordinal)
        {
            return GetFieldValue<char>(ordinal);
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotSupportedException();
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return GetFieldValue<DateTime>(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            return GetFieldValue<decimal>(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            return GetFieldValue<double>(ordinal);
        }

        public override float GetFloat(int ordinal)
        {
            return GetFieldValue<float>(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            return GetFieldValue<Guid>(ordinal);
        }

        public override short GetInt16(int ordinal)
        {
            return GetFieldValue<short>(ordinal);
        }

        public override int GetInt32(int ordinal)
        {
            return GetFieldValue<int>(ordinal);
        }

        public override long GetInt64(int ordinal)
        {
            return GetFieldValue<long>(ordinal);
        }

        public override string GetString(int ordinal)
        {
            return GetFieldValue<string>(ordinal);
        }

        public T GetFieldValue<T>(int ordinal)
        {
            return (T)_resultSet[_currentRow][ordinal];
        }

#if !NET40

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
        public Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
        {
            return Task.FromResult((T)_resultSet[_currentRow][ordinal]);
        }

#endif

        public override object GetValue(int ordinal)
        {
            return GetFieldValue<object>(ordinal);
        }

        public override int GetValues(object[] values)
        {
            var count = Math.Min(values.Length, FieldCount);
            for (var i = 0; i < count; ++i)
            {
                values[i] = GetValue(i);
            }
            return count;
        }

        public override bool IsDBNull(int ordinal)
        {
            return DBNull.Value.Equals(_resultSet[_currentRow][ordinal]);
        }

#if !NET40

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
        public Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
        {
            return Task.FromResult(IsDBNull(ordinal));
        }

#endif

        public override string GetDataTypeName(int ordinal)
        {
            return _dataTypeNames[ordinal];
        }

        public override Type GetFieldType(int ordinal)
        {
            return _fieldTypes[ordinal];
        }

        public override string GetName(int ordinal)
        {
            return _columnNames[ordinal];
        }

        public override int GetOrdinal(string name)
        {
            return _fieldNameLookup.GetOrdinal(name);
        }

        public bool Read()
        {
            return ++_currentRow < _resultSet.Count;
        }

#if !NET40

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
        public Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(++_currentRow < _resultSet.Count);
        }

#endif
    }
}
