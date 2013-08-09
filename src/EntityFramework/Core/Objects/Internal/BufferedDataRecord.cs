// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Spatial;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
#if !NET40
    using System.Threading;
    using System.Threading.Tasks;
#endif

    internal class BufferedDataRecord
    {
        private int _currentRowNumber = -1;
        private object[] _currentRow;

        private List<object[]> _resultSet;
        private DbSpatialDataReader _spatialDataReader;
        private bool[] _geographyColumns;
        private bool[] _geometryColumns;
        private int _rowCount;
        private string[] _dataTypeNames;
        private Type[] _fieldTypes;
        private string[] _columnNames;
        private Lazy<FieldNameLookup> _fieldNameLookup;

        protected BufferedDataRecord()
        {
        }

        internal static BufferedDataRecord Initialize(
            string providerManifestToken, DbProviderServices providerSerivces, DbDataReader reader)
        {
            var record = new BufferedDataRecord();
            record.ReadMetadata(providerManifestToken, providerSerivces, reader);

            var fieldCount = record.FieldCount;
            var resultSet = new List<object[]>();
            if (record._spatialDataReader != null)
            {
                while (reader.Read())
                {
                    var row = new object[fieldCount];
                    for (var i = 0; i < fieldCount; i++)
                    {
                        if (reader.IsDBNull(i))
                        {
                            row[i] = DBNull.Value;
                        }
                        else if (record._geographyColumns[i])
                        {
                            row[i] = record._spatialDataReader.GetGeography(i);
                        }
                        else if (record._geometryColumns[i])
                        {
                            row[i] = record._spatialDataReader.GetGeometry(i);
                        }
                        else
                        {
                            row[i] = reader.GetValue(i);
                        }
                    }
                    resultSet.Add(row);
                }
            }
            else
            {
                while (reader.Read())
                {
                    var row = new object[fieldCount];
                    reader.GetValues(row);
                    resultSet.Add(row);
                }
            }

            record._rowCount = resultSet.Count;
            record._resultSet = resultSet;
            return record;
        }

#if !NET40

        internal static async Task<BufferedDataRecord> InitializeAsync(
            string providerManifestToken, DbProviderServices providerSerivces, DbDataReader reader, CancellationToken cancellationToken)
        {
            var record = new BufferedDataRecord();
            record.ReadMetadata(providerManifestToken, providerSerivces, reader);

            var fieldCount = record.FieldCount;
            var resultSet = new List<object[]>();
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
            {
                var row = new object[fieldCount];
                for (var i = 0; i < fieldCount; i++)
                {
                    if (await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        row[i] = DBNull.Value;
                    }
                    else if (record._spatialDataReader != null
                             && record._geographyColumns[i])
                    {
                        row[i] = await record._spatialDataReader.GetGeographyAsync(i, cancellationToken)
                                           .ConfigureAwait(continueOnCapturedContext: false);
                    }
                    else if (record._spatialDataReader != null
                             && record._geometryColumns[i])
                    {
                        row[i] = await record._spatialDataReader.GetGeometryAsync(i, cancellationToken)
                                           .ConfigureAwait(continueOnCapturedContext: false);
                    }
                    else
                    {
                        row[i] = await reader.GetFieldValueAsync<object>(i, cancellationToken)
                                           .ConfigureAwait(continueOnCapturedContext: false);
                    }
                }
                resultSet.Add(row);
            }

            record._rowCount = resultSet.Count;
            record._resultSet = resultSet;
            return record;
        }

#endif

        protected void ReadMetadata(string providerManifestToken, DbProviderServices providerServices, DbDataReader reader)
        {
            var fieldCount = reader.FieldCount;
            var dataTypeNames = new string[fieldCount];
            var columnTypes = new Type[fieldCount];
            var columnNames = new string[fieldCount];
            for (var i = 0; i < fieldCount; i++)
            {
                dataTypeNames[i] = reader.GetDataTypeName(i);
                columnTypes[i] = reader.GetFieldType(i);
                columnNames[i] = reader.GetName(i);
            }

            _dataTypeNames = dataTypeNames;
            _fieldTypes = columnTypes;
            _columnNames = columnNames;
            _fieldNameLookup = new Lazy<FieldNameLookup>(
                () => new FieldNameLookup(new ReadOnlyCollection<string>(columnNames), -1), isThreadSafe: false);
        
            var hasSpatialColumns = false;
            DbSpatialDataReader spatialDataReader = null;
            if (fieldCount > 0)
            {
                // FieldCount == 0 indicates NullDataReader
                spatialDataReader = providerServices.GetSpatialDataReader(reader, providerManifestToken);
            }

            if (spatialDataReader != null)
            {
                _geographyColumns = new bool[fieldCount];
                _geometryColumns = new bool[fieldCount];

                for (var i = 0; i < fieldCount; i++)
                {
                    _geographyColumns[i] = spatialDataReader.IsGeographyColumn(i);
                    _geometryColumns[i] = spatialDataReader.IsGeometryColumn(i);
                    hasSpatialColumns = hasSpatialColumns || _geographyColumns[i] || _geometryColumns[i];
                    Debug.Assert(!_geographyColumns[i] || !_geometryColumns[i]);
                }
            }

            _spatialDataReader = hasSpatialColumns ? spatialDataReader : null;
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
            get { return _rowCount > 0; }
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
            if (_currentRow.Length == 0)
            {
                // Reader is being intercepted
                return true;
            }

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
            if (++_currentRowNumber < _rowCount)
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
