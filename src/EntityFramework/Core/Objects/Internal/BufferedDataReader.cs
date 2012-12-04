// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
#if !NET40
    using System.Threading;
    using System.Threading.Tasks;
#endif

    /// <summary>
    ///     A wrapper over a <see cref="DbDataReader"/> that will consume and close the supplied reader
    ///     when <see cref="Initialize"/> is called.
    /// </summary>
    internal class BufferedDataReader : DbDataReader
    {
        private DbDataReader _underlyingReader;
        private List<BufferedDataRecord> _bufferedDataRecords = new List<BufferedDataRecord>();
        private int _currentResultSet;
        private int _recordsAffected;

        public BufferedDataReader(DbDataReader reader)
        {
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
                AssertReaderIsOpenWithData();
                return _bufferedDataRecords[_currentResultSet][name];
            }
        }

        public override object this[int ordinal]
        {
            get
            {
                AssertReaderIsOpenWithData();
                return _bufferedDataRecords[_currentResultSet][ordinal];
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
                return _bufferedDataRecords[_currentResultSet].FieldCount;
            }
        }

        public override bool HasRows
        {
            get
            {
                AssertReaderIsOpen();
                return _bufferedDataRecords[_currentResultSet].HasRows;
            }
        }

        public override bool IsClosed
        {
            get { return _bufferedDataRecords == null; }
        }

        private void AssertReaderIsOpen()
        {
            Debug.Assert(_underlyingReader == null, "The reader wasn't initialized");

            if (IsClosed)
            {
                throw Error.ADP_ClosedDataReaderError();
            }
        }

        private void AssertReaderIsOpenWithData()
        {
            AssertReaderIsOpen();

            if (!_bufferedDataRecords[_currentResultSet].HasData)
            {
                throw Error.ADP_NoData();
            }
        }

        internal void Initialize()
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
                    var dataTypeNames = new string[reader.FieldCount];
                    var types = new Type[reader.FieldCount];
                    var columnNames = new string[reader.FieldCount];
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        dataTypeNames[i] = reader.GetDataTypeName(i);
                        types[i] = reader.GetFieldType(i);
                        columnNames[i] = reader.GetName(i);
                    }

                    var resultSet = new List<object[]>();
                    while (reader.Read())
                    {
                        var row = new object[reader.FieldCount];
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            row[i] = reader.GetValue(i);
                        }
                        resultSet.Add(row);
                    }

                    _bufferedDataRecords.Add(new BufferedDataRecord(resultSet, dataTypeNames, types, columnNames));
                }
                while (reader.NextResult());

                _recordsAffected = reader.RecordsAffected;
            }
            finally
            {
                reader.Dispose();
            }
        }

#if !NET40

        internal async Task InitializeAsync(CancellationToken cancellationToken)
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
                    var dataTypeNames = new string[reader.FieldCount];
                    var types = new Type[reader.FieldCount];
                    var columnNames = new string[reader.FieldCount];
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        dataTypeNames[i] = reader.GetDataTypeName(i);
                        types[i] = reader.GetFieldType(i);
                        columnNames[i] = reader.GetName(i);
                    }

                    var resultSet = new List<object[]>();
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        var row = new object[reader.FieldCount];
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            row[i] =
                                await
                                reader.GetFieldValueAsync<object>(i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                        }
                        resultSet.Add(row);
                    }

                    _bufferedDataRecords.Add(new BufferedDataRecord(resultSet, dataTypeNames, types, columnNames));
                }
                while (await reader.NextResultAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false));

                _recordsAffected = reader.RecordsAffected;
            }
            finally
            {
                reader.Dispose();
            }
        }

#endif

        public override void Close()
        {
            _bufferedDataRecords = null;

            var reader = _underlyingReader;
            if (reader != null)
            {
                _underlyingReader = null;
                reader.Dispose();
            }
        }

        public override bool GetBoolean(int ordinal)
        {
            AssertReaderIsOpenWithData();
            return _bufferedDataRecords[_currentResultSet].GetBoolean(ordinal);
        }

        public override byte GetByte(int ordinal)
        {
            AssertReaderIsOpenWithData();
            return _bufferedDataRecords[_currentResultSet].GetByte(ordinal);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotSupportedException();
        }

        public override char GetChar(int ordinal)
        {
            AssertReaderIsOpenWithData();
            return _bufferedDataRecords[_currentResultSet].GetChar(ordinal);
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotSupportedException();
        }

        public override DateTime GetDateTime(int ordinal)
        {
            AssertReaderIsOpenWithData();
            return _bufferedDataRecords[_currentResultSet].GetDateTime(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            AssertReaderIsOpenWithData();
            return _bufferedDataRecords[_currentResultSet].GetDecimal(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            AssertReaderIsOpenWithData();
            return _bufferedDataRecords[_currentResultSet].GetDouble(ordinal);
        }

        public override float GetFloat(int ordinal)
        {
            AssertReaderIsOpenWithData();
            return _bufferedDataRecords[_currentResultSet].GetFloat(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            AssertReaderIsOpenWithData();
            return _bufferedDataRecords[_currentResultSet].GetGuid(ordinal);
        }

        public override short GetInt16(int ordinal)
        {
            AssertReaderIsOpenWithData();
            return _bufferedDataRecords[_currentResultSet].GetInt16(ordinal);
        }

        public override int GetInt32(int ordinal)
        {
            AssertReaderIsOpenWithData();
            return _bufferedDataRecords[_currentResultSet].GetInt32(ordinal);
        }

        public override long GetInt64(int ordinal)
        {
            AssertReaderIsOpenWithData();
            return _bufferedDataRecords[_currentResultSet].GetInt64(ordinal);
        }

        public override string GetString(int ordinal)
        {
            AssertReaderIsOpenWithData();
            return _bufferedDataRecords[_currentResultSet].GetString(ordinal);
        }

#if NET40
        public T GetFieldValue<T>(int ordinal)
#else
        public override T GetFieldValue<T>(int ordinal)
#endif
        {
            AssertReaderIsOpenWithData();
            return _bufferedDataRecords[_currentResultSet].GetFieldValue<T>(ordinal);
        }

#if !NET40

        public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
        {
            AssertReaderIsOpenWithData();
            return _bufferedDataRecords[_currentResultSet].GetFieldValueAsync<T>(ordinal, cancellationToken);
        }

#endif

        public override object GetValue(int ordinal)
        {
            AssertReaderIsOpenWithData();
            return _bufferedDataRecords[_currentResultSet].GetValue(ordinal);
        }

        public override int GetValues(object[] values)
        {
            Check.NotNull(values, "values");
            AssertReaderIsOpenWithData();
            return _bufferedDataRecords[_currentResultSet].GetValues(values);
        }

        public override string GetDataTypeName(int ordinal)
        {
            AssertReaderIsOpen();
            return _bufferedDataRecords[_currentResultSet].GetDataTypeName(ordinal);
        }

        public override Type GetFieldType(int ordinal)
        {
            AssertReaderIsOpen();
            return _bufferedDataRecords[_currentResultSet].GetFieldType(ordinal);
        }

        public override string GetName(int ordinal)
        {
            AssertReaderIsOpen();
            return _bufferedDataRecords[_currentResultSet].GetName(ordinal);
        }

        public override int GetOrdinal(string name)
        {
            Check.NotNull(name, "name");
            AssertReaderIsOpen();
            return _bufferedDataRecords[_currentResultSet].GetOrdinal(name);
        }

        public override bool IsDBNull(int ordinal)
        {
            AssertReaderIsOpenWithData();
            return _bufferedDataRecords[_currentResultSet].IsDBNull(ordinal);
        }

#if !NET40

        public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
        {
            AssertReaderIsOpenWithData();
            return _bufferedDataRecords[_currentResultSet].IsDBNullAsync(ordinal, cancellationToken);
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
            return ++_currentResultSet < _bufferedDataRecords.Count;
        }

#if !NET40

        public override Task<bool> NextResultAsync(CancellationToken cancellationToken)
        {
            AssertReaderIsOpen();
            return Task.FromResult(++_currentResultSet < _bufferedDataRecords.Count);
        }

#endif

        public override bool Read()
        {
            AssertReaderIsOpen();
            return _bufferedDataRecords[_currentResultSet].Read();
        }

#if !NET40

        public override Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            AssertReaderIsOpen();
            return _bufferedDataRecords[_currentResultSet].ReadAsync(cancellationToken);
        }

#endif
    }
}
