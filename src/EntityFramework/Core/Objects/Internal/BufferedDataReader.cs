// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
#if !NET40
    using System.Threading;
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// A wrapper over a <see cref="DbDataReader" /> that will consume and close the supplied reader
    /// when <see cref="Initialize" /> is called.
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
                throw new NotSupportedException();
            }
        }

        public override object this[int ordinal]
        {
            get
            {
                throw new NotSupportedException();
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

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "nullableColumns")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "columnTypes")]
        internal void Initialize(
            string providerManifestToken, DbProviderServices providerSerivces, Type[] columnTypes, bool[] nullableColumns)
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
                    _bufferedDataRecords.Add(BufferedDataRecord.Initialize(providerManifestToken, providerSerivces, reader));
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

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "nullableColumns")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "columnTypes")]
        internal async Task InitializeAsync(
            string providerManifestToken, DbProviderServices providerSerivces, Type[] columnTypes, bool[] nullableColumns,
            CancellationToken cancellationToken)
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
                    _bufferedDataRecords.Add(
                        await BufferedDataRecord.InitializeAsync(providerManifestToken, providerSerivces, reader, cancellationToken)
                                  .ConfigureAwait(continueOnCapturedContext: false));
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
    }
}
