// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
#if !NET40

#endif

    /// <summary>
    ///     A wrapper over a <see cref="DbDataReader" /> that will consume and close the supplied reader
    ///     when <see cref="Initialize" /> is called.
    /// </summary>
    internal class BufferedDataReader : DbDataReader
    {
        private DbDataReader _underlyingReader;
        private List<BufferedDataRecord> _bufferedDataRecords = new List<BufferedDataRecord>();
        private BufferedDataRecord _currentResultSet;
        private int _currentResultSetNumber;
        private int _recordsAffected;
        private bool _disposed;
        private bool _isClosed;

        public BufferedDataReader(DbDataReader reader)
        {
            DebugCheck.NotNull(reader);

            _underlyingReader = reader;
        }

        public override int RecordsAffected
        {
            get { return _recordsAffected; }
        }

        public override object this[string name]
        {
            get
            {
                Check.NotNull(name, "name");
                AssertReaderIsOpenWithData();
                return _currentResultSet[name];
            }
        }

        public override object this[int ordinal]
        {
            get
            {
                AssertFieldIsReady(ordinal);
                return _currentResultSet[ordinal];
            }
        }

        public override int Depth
        {
            get { throw new NotSupportedException(); }
        }

        public override int FieldCount
        {
            get
            {
                AssertReaderIsOpen();
                return _currentResultSet.FieldCount;
            }
        }

        public override bool HasRows
        {
            get
            {
                AssertReaderIsOpen();
                return _currentResultSet.HasRows;
            }
        }

        public override bool IsClosed
        {
            get { return _isClosed; }
        }

        private void AssertReaderIsOpen()
        {
            Debug.Assert(_underlyingReader == null, "The reader wasn't initialized");

            if (_isClosed)
            {
                throw Error.ADP_ClosedDataReaderError();
            }
        }

        private void AssertReaderIsOpenWithData()
        {
            Debug.Assert(_underlyingReader == null, "The reader wasn't initialized");

            if (_isClosed)
            {
                throw Error.ADP_ClosedDataReaderError();
            }

            if (!_currentResultSet.IsDataReady)
            {
                throw Error.ADP_NoData();
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        private void AssertFieldIsReady(int ordinal)
        {
            Debug.Assert(_underlyingReader == null, "The reader wasn't initialized");

            if (_isClosed)
            {
                throw Error.ADP_ClosedDataReaderError();
            }

            if (!_currentResultSet.IsDataReady)
            {
                throw Error.ADP_NoData();
            }

            if (0 > ordinal
                || ordinal > _currentResultSet.FieldCount)
            {
                throw new IndexOutOfRangeException();
            }
        }

        internal void Initialize(string providerManifestToken, DbProviderServices providerSerivces)
        {
            var reader = _underlyingReader;
            if (reader == null)
            {
                return;
            }
            _underlyingReader = null;

            try
            {
                do
                {
                    var metadata = ReadMetadata(providerManifestToken, providerSerivces, reader);

                    var resultSet = new List<object[]>();
                    if (metadata.HasSpatialColumns)
                    {
                        while (reader.Read())
                        {
                            var row = new object[metadata.FieldCount];
                            for (var i = 0; i < metadata.FieldCount; i++)
                            {
                                if (reader.IsDBNull(i))
                                {
                                    row[i] = DBNull.Value;
                                }
                                else if (metadata.GeographyColumns[i])
                                {
                                    row[i] = metadata.SpatialDataReader.GetGeography(i);
                                }
                                else if (metadata.GeometryColumns[i])
                                {
                                    row[i] = metadata.SpatialDataReader.GetGeometry(i);
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
                            var row = new object[metadata.FieldCount];
                            reader.GetValues(row);
                            resultSet.Add(row);
                        }
                    }

                    _bufferedDataRecords.Add(
                        new BufferedDataRecord(resultSet, metadata.DataTypeNames, metadata.ColumnTypes, metadata.ColumnNames));
                }
                while (reader.NextResult());

                _recordsAffected = reader.RecordsAffected;
                _currentResultSet = _bufferedDataRecords[_currentResultSetNumber];
            }
            finally
            {
                reader.Dispose();
            }
        }

#if !NET40

        internal async Task InitializeAsync(
            string providerManifestToken, DbProviderServices providerSerivces, CancellationToken cancellationToken)
        {
            var reader = _underlyingReader;
            if (reader == null)
            {
                return;
            }
            _underlyingReader = null;

            try
            {
                do
                {
                    var metadata = ReadMetadata(providerManifestToken, providerSerivces, reader);

                    var resultSet = new List<object[]>();
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        var row = new object[metadata.FieldCount];
                        for (var i = 0; i < metadata.FieldCount; i++)
                        {
                            if (await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                            {
                                row[i] = DBNull.Value;
                            }
                            else if (metadata.HasSpatialColumns
                                     && metadata.GeographyColumns[i])
                            {
                                row[i] = await metadata.SpatialDataReader.GetGeographyAsync(i, cancellationToken)
                                                       .ConfigureAwait(continueOnCapturedContext: false);
                            }
                            else if (metadata.HasSpatialColumns
                                     && metadata.GeometryColumns[i])
                            {
                                row[i] = await metadata.SpatialDataReader.GetGeometryAsync(i, cancellationToken)
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

                    _bufferedDataRecords.Add(
                        new BufferedDataRecord(resultSet, metadata.DataTypeNames, metadata.ColumnTypes, metadata.ColumnNames));
                }
                while (await reader.NextResultAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false));

                _recordsAffected = reader.RecordsAffected;
                _currentResultSet = _bufferedDataRecords[_currentResultSetNumber];
            }
            finally
            {
                reader.Dispose();
            }
        }

#endif

        private static ReaderMetadata ReadMetadata(string providerManifestToken, DbProviderServices providerSerivces, DbDataReader reader)
        {
            var fieldCount = reader.FieldCount;
            var hasSpatialColumns = false;
            DbSpatialDataReader spatialDataReader = null;
            bool[] geographyColumns = null;
            bool[] geometryColumns = null;
            try
            {
                spatialDataReader = providerSerivces.GetSpatialDataReader(reader, providerManifestToken);
                geographyColumns = new bool[fieldCount];
                geometryColumns = new bool[fieldCount];
            }
            catch (ProviderIncompatibleException)
            {
            }

            var dataTypeNames = new string[fieldCount];
            var columnTypes = new Type[fieldCount];
            var columnNames = new string[fieldCount];
            for (var i = 0; i < fieldCount; i++)
            {
                dataTypeNames[i] = reader.GetDataTypeName(i);
                columnTypes[i] = reader.GetFieldType(i);
                columnNames[i] = reader.GetName(i);
                if (spatialDataReader != null)
                {
                    geographyColumns[i] = spatialDataReader.IsGeographyColumn(i);
                    geometryColumns[i] = spatialDataReader.IsGeometryColumn(i);
                    hasSpatialColumns = hasSpatialColumns || geographyColumns[i] || geometryColumns[i];
                    Debug.Assert(!geographyColumns[i] || !geometryColumns[i]);
                }
            }

            return new ReaderMetadata(
                fieldCount, dataTypeNames, columnTypes, columnNames, hasSpatialColumns, spatialDataReader, geographyColumns, geometryColumns);
        }

        public override void Close()
        {
            _bufferedDataRecords = null;
            _isClosed = true;

            var reader = _underlyingReader;
            if (reader != null)
            {
                _underlyingReader = null;
                reader.Dispose();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed
                && disposing
                && !IsClosed)
            {
                Close();
            }
            _disposed = true;

            base.Dispose(disposing);
        }

        public override bool GetBoolean(int ordinal)
        {
            AssertFieldIsReady(ordinal);
            return _currentResultSet.GetBoolean(ordinal);
        }

        public override byte GetByte(int ordinal)
        {
            AssertFieldIsReady(ordinal);
            return _currentResultSet.GetByte(ordinal);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotSupportedException();
        }

        public override char GetChar(int ordinal)
        {
            AssertFieldIsReady(ordinal);
            return _currentResultSet.GetChar(ordinal);
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotSupportedException();
        }

        public override DateTime GetDateTime(int ordinal)
        {
            AssertFieldIsReady(ordinal);
            return _currentResultSet.GetDateTime(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            AssertFieldIsReady(ordinal);
            return _currentResultSet.GetDecimal(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            AssertFieldIsReady(ordinal);
            return _currentResultSet.GetDouble(ordinal);
        }

        public override float GetFloat(int ordinal)
        {
            AssertFieldIsReady(ordinal);
            return _currentResultSet.GetFloat(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            AssertFieldIsReady(ordinal);
            return _currentResultSet.GetGuid(ordinal);
        }

        public override short GetInt16(int ordinal)
        {
            AssertFieldIsReady(ordinal);
            return _currentResultSet.GetInt16(ordinal);
        }

        public override int GetInt32(int ordinal)
        {
            AssertFieldIsReady(ordinal);
            return _currentResultSet.GetInt32(ordinal);
        }

        public override long GetInt64(int ordinal)
        {
            AssertFieldIsReady(ordinal);
            return _currentResultSet.GetInt64(ordinal);
        }

        public override string GetString(int ordinal)
        {
            AssertFieldIsReady(ordinal);
            return _currentResultSet.GetString(ordinal);
        }

#if NET40
        public T GetFieldValue<T>(int ordinal)
#else
        public override T GetFieldValue<T>(int ordinal)
#endif
        {
            AssertFieldIsReady(ordinal);
            return _currentResultSet.GetFieldValue<T>(ordinal);
        }

#if !NET40

        public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
        {
            AssertFieldIsReady(ordinal);
            return _currentResultSet.GetFieldValueAsync<T>(ordinal, cancellationToken);
        }

#endif

        public override object GetValue(int ordinal)
        {
            AssertFieldIsReady(ordinal);
            return _currentResultSet.GetValue(ordinal);
        }

        public override int GetValues(object[] values)
        {
            Check.NotNull(values, "values");
            AssertReaderIsOpenWithData();
            return _currentResultSet.GetValues(values);
        }

        public override string GetDataTypeName(int ordinal)
        {
            AssertReaderIsOpen();
            return _currentResultSet.GetDataTypeName(ordinal);
        }

        public override Type GetFieldType(int ordinal)
        {
            AssertReaderIsOpen();
            return _currentResultSet.GetFieldType(ordinal);
        }

        public override string GetName(int ordinal)
        {
            AssertReaderIsOpen();
            return _currentResultSet.GetName(ordinal);
        }

        public override int GetOrdinal(string name)
        {
            Check.NotNull(name, "name");
            AssertReaderIsOpen();
            return _currentResultSet.GetOrdinal(name);
        }

        public override bool IsDBNull(int ordinal)
        {
            AssertFieldIsReady(ordinal);
            return _currentResultSet.IsDBNull(ordinal);
        }

#if !NET40

        public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
        {
            AssertFieldIsReady(ordinal);
            return _currentResultSet.IsDBNullAsync(ordinal, cancellationToken);
        }

#endif

        public override IEnumerator GetEnumerator()
        {
            return new DbEnumerator(this);
        }

        public override DataTable GetSchemaTable()
        {
            throw new NotSupportedException();
        }

        public override bool NextResult()
        {
            AssertReaderIsOpen();
            if (++_currentResultSetNumber < _bufferedDataRecords.Count)
            {
                _currentResultSet = _bufferedDataRecords[_currentResultSetNumber];
                return true;
            }
            else
            {
                _currentResultSet = null;
                return false;
            }
        }

#if !NET40

        public override Task<bool> NextResultAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(NextResult());
        }

#endif

        public override bool Read()
        {
            AssertReaderIsOpen();
            return _currentResultSet.Read();
        }

#if !NET40

        public override Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            AssertReaderIsOpen();
            return _currentResultSet.ReadAsync(cancellationToken);
        }

#endif

        private class ReaderMetadata
        {
            public readonly int FieldCount;
            public readonly Type[] ColumnTypes;
            public readonly string[] ColumnNames;
            public readonly string[] DataTypeNames;
            public readonly bool HasSpatialColumns;
            public readonly bool[] GeographyColumns;
            public readonly bool[] GeometryColumns;
            public readonly DbSpatialDataReader SpatialDataReader;

            public ReaderMetadata(
                int fieldCount, string[] dataTypeNames, Type[] types, string[] columnNames, bool hasSpatialColumn,
                DbSpatialDataReader spatialDataReader, bool[] geographyColumns, bool[] geometryColumns)
            {
                FieldCount = fieldCount;
                DataTypeNames = dataTypeNames;
                ColumnTypes = types;
                ColumnNames = columnNames;
                HasSpatialColumns = hasSpatialColumn;
                SpatialDataReader = spatialDataReader;
                GeographyColumns = geographyColumns;
                GeometryColumns = geometryColumns;
            }
        }
    }
}
