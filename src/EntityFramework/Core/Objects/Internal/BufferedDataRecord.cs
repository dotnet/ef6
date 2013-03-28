// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

#if !NET40

#endif

    internal class BufferedDataRecord
    {
        private int _currentRowNumber = -1;
        private object[] _currentRow;

        private readonly List<object[]> _resultSet;
        private readonly int _resultSetCount;
        private readonly string[] _dataTypeNames;
        private readonly Type[] _fieldTypes;
        private readonly string[] _columnNames;
        private readonly Lazy<FieldNameLookup> _fieldNameLookup;

        public BufferedDataRecord(List<object[]> resultSet, string[] dataTypeNames, Type[] fieldTypes, string[] columnNames)
        {
            DebugCheck.NotNull(resultSet);
            DebugCheck.NotNull(dataTypeNames);
            DebugCheck.NotNull(fieldTypes);
            DebugCheck.NotNull(columnNames);
            Debug.Assert(dataTypeNames.Length == fieldTypes.Length);
            Debug.Assert(fieldTypes.Length == columnNames.Length);

            _resultSet = resultSet;
            _resultSetCount = _resultSet.Count;
            _dataTypeNames = dataTypeNames;
            _fieldTypes = fieldTypes;
            _columnNames = columnNames;
            _fieldNameLookup = new Lazy<FieldNameLookup>(
                () => new FieldNameLookup(new ReadOnlyCollection<string>(columnNames), -1), isThreadSafe: false);
        }

        public object this[string name]
        {
            get { return GetValue(GetOrdinal(name)); }
        }

        public object this[int ordinal]
        {
            get { return GetValue(ordinal); }
        }

        public bool IsDataReady { get; private set; }

        public bool HasRows
        {
            get { return _resultSetCount > 0; }
        }

        public int FieldCount
        {
            get { return _dataTypeNames.Length; }
        }

        public bool GetBoolean(int ordinal)
        {
            return GetFieldValue<bool>(ordinal);
        }

        public byte GetByte(int ordinal)
        {
            return GetFieldValue<byte>(ordinal);
        }

        public char GetChar(int ordinal)
        {
            return GetFieldValue<char>(ordinal);
        }

        public DateTime GetDateTime(int ordinal)
        {
            return GetFieldValue<DateTime>(ordinal);
        }

        public decimal GetDecimal(int ordinal)
        {
            return GetFieldValue<decimal>(ordinal);
        }

        public double GetDouble(int ordinal)
        {
            return GetFieldValue<double>(ordinal);
        }

        public float GetFloat(int ordinal)
        {
            return GetFieldValue<float>(ordinal);
        }

        public Guid GetGuid(int ordinal)
        {
            return GetFieldValue<Guid>(ordinal);
        }

        public short GetInt16(int ordinal)
        {
            return GetFieldValue<short>(ordinal);
        }

        public int GetInt32(int ordinal)
        {
            return GetFieldValue<int>(ordinal);
        }

        public long GetInt64(int ordinal)
        {
            return GetFieldValue<long>(ordinal);
        }

        public string GetString(int ordinal)
        {
            return GetFieldValue<string>(ordinal);
        }

        public T GetFieldValue<T>(int ordinal)
        {
            return (T)_currentRow[ordinal];
        }

#if !NET40

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
        public Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
        {
            return Task.FromResult((T)_currentRow[ordinal]);
        }

#endif

        public object GetValue(int ordinal)
        {
            return GetFieldValue<object>(ordinal);
        }

        public int GetValues(object[] values)
        {
            var count = Math.Min(values.Length, FieldCount);
            for (var i = 0; i < count; ++i)
            {
                values[i] = GetValue(i);
            }
            return count;
        }

        public bool IsDBNull(int ordinal)
        {
            return DBNull.Value == _currentRow[ordinal];
        }

#if !NET40

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
        public Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
        {
            return Task.FromResult(IsDBNull(ordinal));
        }

#endif

        public string GetDataTypeName(int ordinal)
        {
            return _dataTypeNames[ordinal];
        }

        public Type GetFieldType(int ordinal)
        {
            return _fieldTypes[ordinal];
        }

        public string GetName(int ordinal)
        {
            return _columnNames[ordinal];
        }

        public int GetOrdinal(string name)
        {
            return _fieldNameLookup.Value.GetOrdinal(name);
        }

        public bool Read()
        {
            if (++_currentRowNumber < _resultSetCount)
            {
                _currentRow = _resultSet[_currentRowNumber];
                IsDataReady = true;
            }
            else
            {
                _currentRow = null;
                IsDataReady = false;
            }

            return IsDataReady;
        }

#if !NET40

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
        public Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Read());
        }

#endif
    }
}
