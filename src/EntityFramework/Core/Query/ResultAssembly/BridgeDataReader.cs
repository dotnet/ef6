// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.ResultAssembly
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Query.PlanCompiler;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    // <summary>
    // DbDataReader functionality for the bridge.
    // </summary>
    internal class BridgeDataReader : DbDataReader, IExtendedDataRecord
    {
        #region Private state

        // <summary>
        // Object that holds the state needed by the coordinator and the root enumerator
        // </summary>
        private Shaper<RecordState> _shaper;

        // <summary>
        // Enumerator over shapers for NextResult() calls.
        // Null for nested data readers (depth > 0);
        // </summary>
        private IEnumerator<KeyValuePair<Shaper<RecordState>, CoordinatorFactory<RecordState>>> _nextResultShaperInfoEnumerator;

        // <summary>
        // The coordinator we're responsible for returning results for.
        // </summary>
        private CoordinatorFactory<RecordState> _coordinatorFactory;

        // <summary>
        // The default record (pre-read/past-end) state
        // </summary>
        private RecordState _defaultRecordState;

        // <summary>
        // We delegate to this on our getters, to avoid duplicate code.
        // </summary>
        private BridgeDataRecord _dataRecord;

        // <summary>
        // Do we have a row to read?  Determined in the constructor and
        // should not be changed.
        // </summary>
        private bool _hasRows;

        // <summary>
        // Set to true only when we've been closed through the Close() method
        // </summary>
        private bool _isClosed;

        // <summary>
        // 0 if initialization hasn't been performed, 1 otherwise
        // </summary>
        private int _initialized;

        private readonly Action _initialize;

#if !NET40

        private readonly Func<CancellationToken, Task> _initializeAsync;

#endif

        #endregion

        #region Constructors

        internal BridgeDataReader(
            Shaper<RecordState> shaper, CoordinatorFactory<RecordState> coordinatorFactory, int depth,
            IEnumerator<KeyValuePair<Shaper<RecordState>, CoordinatorFactory<RecordState>>> nextResultShaperInfos)
        {
            DebugCheck.NotNull(shaper);
            DebugCheck.NotNull(coordinatorFactory);
            Debug.Assert(depth == 0 || nextResultShaperInfos == null, "Nested data readers should not have multiple result sets.");

            _nextResultShaperInfoEnumerator = nextResultShaperInfos;
            _initialize = () => SetShaper(shaper, coordinatorFactory, depth);

#if !NET40

            _initializeAsync = ct => SetShaperAsync(shaper, coordinatorFactory, depth, ct);

#endif
        }

        #endregion

        #region Helpers

        // <summary>
        // Runs the initialization if it hasn't been run
        // </summary>
        protected virtual void EnsureInitialized()
        {
            if (Interlocked.CompareExchange(ref _initialized, 1, 0) == 0)
            {
                _initialize();
            }
        }

#if !NET40

        // <summary>
        // An asynchronous version of <see cref="EnsureInitialized" />, which
        // runs the initialization if it hasn't been run
        // </summary>
        protected virtual Task EnsureInitializedAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Interlocked.CompareExchange(ref _initialized, 1, 0) == 0
                       ? _initializeAsync(cancellationToken)
                       : Task.FromResult<object>(null);
        }

#endif

        private void SetShaper(Shaper<RecordState> shaper, CoordinatorFactory<RecordState> coordinatorFactory, int depth)
        {
            _shaper = shaper;
            _coordinatorFactory = coordinatorFactory;
            _dataRecord = new BridgeDataRecord(shaper, depth);

            if (!_shaper.DataWaiting)
            {
                _shaper.DataWaiting = _shaper.RootEnumerator.MoveNext();
            }

            InitializeHasRows();
        }

#if !NET40

        private async Task SetShaperAsync(
            Shaper<RecordState> shaper, CoordinatorFactory<RecordState> coordinatorFactory,
            int depth, CancellationToken cancellationToken)
        {
            _shaper = shaper;
            _coordinatorFactory = coordinatorFactory;
            _dataRecord = new BridgeDataRecord(shaper, depth);

            if (!_shaper.DataWaiting)
            {
                _shaper.DataWaiting =
                    await _shaper.RootEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }

            InitializeHasRows();
        }

#endif

        private void InitializeHasRows()
        {
            // To determine whether there are any rows for this coordinator at this place in 
            // the root enumerator, we pretty much just look at it's current record (we'll read 
            // one if there isn't one waiting) and if it matches our coordinator, we've got rows.
            _hasRows = false;

            if (_shaper.DataWaiting)
            {
                var currentRecord = _shaper.RootEnumerator.Current;

                if (null != currentRecord)
                {
                    _hasRows = (currentRecord.CoordinatorFactory == _coordinatorFactory);
                }
            }

            // Once we've created the root enumerator, we can get the default record state
            _defaultRecordState = _coordinatorFactory.GetDefaultRecordState(_shaper);
            Debug.Assert(null != _defaultRecordState, "no default?");
        }

        // <summary>
        // Ensures that the reader is actually open, and throws an exception if not
        // </summary>
        private void AssertReaderIsOpen(string methodName)
        {
            if (IsClosed)
            {
                if (_dataRecord.IsImplicitlyClosed)
                {
                    throw Error.ADP_ImplicitlyClosedDataReaderError();
                }
                if (_dataRecord.IsExplicitlyClosed)
                {
                    throw Error.ADP_DataReaderClosed(methodName);
                }
            }
        }

        // <summary>
        // Implicitly close this (nested) data reader; will be called whenever
        // the user has done a GetValue() or a Read() on a parent reader/record
        // to ensure that we consume all our results.  We do that because we
        // our design requires us to be positioned at the next nested reader's
        // first row.
        // </summary>
        internal void CloseImplicitly()
        {
            EnsureInitialized();
            Consume();
            _dataRecord.CloseImplicitly();
        }

#if !NET40

        // <summary>
        // An asynchronous version of <see cref="CloseImplicitly" />, which
        // implicitly closes this (nested) data reader; will be called whenever
        // the user has done a GetValue() or a ReadAsync() on a parent reader/record
        // to ensure that we consume all our results.  We do that because we
        // our design requires us to be positioned at the next nested reader's
        // first row.
        // </summary>
        internal async Task CloseImplicitlyAsync(CancellationToken cancellationToken)
        {
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            await ConsumeAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            await _dataRecord.CloseImplicitlyAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

#endif

        // <summary>
        // Reads to the end of the source enumerator provided
        // </summary>
        private void Consume()
        {
            while (ReadInternal())
            {
            }
        }

#if !NET40

        // <summary>
        // An asynchronous version of <see cref="Consume" />, which
        // reads to the end of the source enumerator provided
        // </summary>
        private async Task ConsumeAsync(CancellationToken cancellationToken)
        {
            while (await ReadInternalAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
            {
            }
        }

#endif

        // <summary>
        // Figure out the CLR type from the TypeMetadata object; For scalars,
        // we can get this from the metadata workspace, but for the rest, we
        // just guess at "Object".  You need to use the DataRecordInfo property
        // to get better information for those.
        // </summary>
        internal static Type GetClrTypeFromTypeMetadata(TypeUsage typeUsage)
        {
            Type result;

            PrimitiveType primitiveType;
            if (TypeHelpers.TryGetEdmType(typeUsage, out primitiveType))
            {
                result = primitiveType.ClrEquivalentType;
            }
            else
            {
                if (TypeSemantics.IsReferenceType(typeUsage))
                {
                    result = typeof(EntityKey);
                }
                else if (TypeUtils.IsStructuredType(typeUsage))
                {
                    result = typeof(DbDataRecord);
                }
                else if (TypeUtils.IsCollectionType(typeUsage))
                {
                    result = typeof(DbDataReader);
                }
                else if (TypeUtils.IsEnumerationType(typeUsage))
                {
                    result = ((EnumType)typeUsage.EdmType).UnderlyingType.ClrEquivalentType;
                }
                else
                {
                    result = typeof(object);
                }
            }
            return result;
        }

        #endregion

        #region DbDataReader implementation

        // <inheritdoc />
        public override int Depth
        {
            get
            {
                EnsureInitialized();
                AssertReaderIsOpen("Depth");
                return _dataRecord.Depth;
            }
        }

        // <inheritdoc />
        public override bool HasRows
        {
            get
            {
                EnsureInitialized();
                AssertReaderIsOpen("HasRows");
                return _hasRows;
            }
        }

        // <inheritdoc />
        public override bool IsClosed
        {
            get
            {
                EnsureInitialized();
                // Rather that try and track this in two places; we just delegate
                // to the data record that we constructed; it has more reasons to 
                // have to know this than we do in the data reader.  (Of course, 
                // we look at our own closed state too...)
                return ((_isClosed) || _dataRecord.IsClosed);
            }
        }

        // <inheritdoc />
        public override int RecordsAffected
        {
            get
            {
                EnsureInitialized();

                var result = -1; // For nested readers, return -1 which is the default for queries.

                // We defer to the store reader for rows affected count. Note that for queries,
                // the provider is generally expected to return -1.
                // FUTURE: when DML is supported, we will need to compute this value ourselves.
                if (_dataRecord.Depth == 0)
                {
                    result = _shaper.Reader.RecordsAffected;
                }
                return result;
            }
        }

        // <inheritdoc />
        public override void Close()
        {
            EnsureInitialized();

            // Make sure we explicitly closed the data record, since that's what
            // where using to track closed state.
            _dataRecord.CloseExplicitly();

            if (!_isClosed)
            {
                _isClosed = true;

                if (0 == _dataRecord.Depth)
                {
                    // If we're the root collection, we want to ensure the remainder of
                    // the result column hierarchy is closed out, to avoid dangling
                    // references to it, should it be reused. We also want to physically 
                    // close out the source reader as well.
                    _shaper.Reader.Close();
                }
                else
                {
                    // For non-root collections, we have to consume all the data, or we'll
                    // not be positioned propertly for what comes afterward.
                    Consume();
                }
            }

            if (_nextResultShaperInfoEnumerator != null)
            {
                _nextResultShaperInfoEnumerator.Dispose();
                _nextResultShaperInfoEnumerator = null;
            }
        }

        // <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override IEnumerator GetEnumerator()
        {
            // Not calling EnsureInitialized() here. It will be called when the DbEnumerator is used
            IEnumerator result = new DbEnumerator(this, closeReader: true);
            return result;
        }

        // <inheritdoc />
        public override DataTable GetSchemaTable()
        {
            throw new NotSupportedException(Strings.ADP_GetSchemaTableIsNotSupported);
        }

        // <inheritdoc />
        public override bool NextResult()
        {
            EnsureInitialized();
            AssertReaderIsOpen("NextResult");

            // If there is a next result set available, serve it.
            if (_nextResultShaperInfoEnumerator != null
                && _shaper.Reader.NextResult()
                && _nextResultShaperInfoEnumerator.MoveNext())
            {
                Debug.Assert(_dataRecord.Depth == 0, "Nested data readers should not have multiple result sets.");
                var nextResultShaperInfo = _nextResultShaperInfoEnumerator.Current;
                _dataRecord.CloseImplicitly();
                SetShaper(nextResultShaperInfo.Key, nextResultShaperInfo.Value, depth: 0);
                return true;
            }

            if (0 == _dataRecord.Depth)
            {
                // This is required to ensure that output parameter values 
                // are set in SQL Server, and other providers where they come after
                // the results.
                CommandHelper.ConsumeReader(_shaper.Reader);
            }
            else
            {
                // For nested readers, make sure we're positioned properly for 
                // the following columns...
                Consume();
            }

            // Ensure we close the records that may be outstanding.
            // Do this after we consume the underlying reader 
            // so we don't run result assembly through it.
            CloseImplicitly();

            // Reset any state on our attached data record, since we've now
            // gone past the end of the reader.
            _dataRecord.SetRecordSource(null, false);

            return false;
        }

#if !NET40

        // <inheritdoc />
        public override async Task<bool> NextResultAsync(CancellationToken cancellationToken)
        {
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            AssertReaderIsOpen("NextResult");

            // If there is a next result set available, serve it.
            if (_nextResultShaperInfoEnumerator != null
                && await _shaper.Reader.NextResultAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)
                && _nextResultShaperInfoEnumerator.MoveNext())
            {
                Debug.Assert(_dataRecord.Depth == 0, "Nested data readers should not have multiple result sets.");
                var nextResultShaperInfo = _nextResultShaperInfoEnumerator.Current;
                await _dataRecord.CloseImplicitlyAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                SetShaper(nextResultShaperInfo.Key, nextResultShaperInfo.Value, depth: 0);
                return true;
            }

            if (0 == _dataRecord.Depth)
            {
                // This is required to ensure that output parameter values 
                // are set in SQL Server, and other providers where they come after
                // the results.
                await CommandHelper.ConsumeReaderAsync(_shaper.Reader, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }
            else
            {
                // For nested readers, make sure we're positioned properly for 
                // the following columns...
                await ConsumeAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }

            // Ensure we close the records that may be outstanding.
            // Do this after we consume the underlying reader 
            // so we don't run result assembly through it.
            await CloseImplicitlyAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

            // Reset any state on our attached data record, since we've now
            // gone past the end of the reader.
            _dataRecord.SetRecordSource(null, false);

            return false;
        }

#endif

        // <inheritdoc />
        public override bool Read()
        {
            EnsureInitialized();
            AssertReaderIsOpen("Read");

            // First of all we need to inform each of the nested records that
            // have been returned that they're "implicitly" closed -- that is 
            // we've moved on.  This will also ensure that any records remaining
            // in any active nested readers are consumed
            _dataRecord.CloseImplicitly();

            // OK, now go ahead and advance the source enumerator and set the 
            // record source up 
            var result = ReadInternal();
            _dataRecord.SetRecordSource(_shaper.RootEnumerator.Current, result);
            return result;
        }

#if !NET40

        // <inheritdoc />
        public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            AssertReaderIsOpen("Read");

            // First of all we need to inform each of the nested records that
            // have been returned that they're "implicitly" closed -- that is 
            // we've moved on.  This will also ensure that any records remaining
            // in any active nested readers are consumed
            await _dataRecord.CloseImplicitlyAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

            // OK, now go ahead and advance the source enumerator and set the 
            // record source up 
            var result = await ReadInternalAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            _dataRecord.SetRecordSource(_shaper.RootEnumerator.Current, result);
            return result;
        }

#endif

        // <summary>
        // Internal read method; does the work of advancing the root enumerator
        // as needed and determining whether it's current record is for our
        // coordinator. The public Read method does the assertions and such that
        // we don't want to do when we're called from internal methods to do things
        // like consume the rest of the reader's contents.
        // </summary>
        private bool ReadInternal()
        {
            var result = false;

            // If there's nothing waiting for the root enumerator, then attempt
            // to advance it. 
            if (!_shaper.DataWaiting)
            {
                _shaper.DataWaiting = _shaper.RootEnumerator.MoveNext();
            }

            // If we have some data (we may have just read it above) then figure
            // out who it belongs to-- us or someone else. We also skip over any
            // records that are for our children (nested readers); if we're being
            // asked to read, it's too late for them to read them.
            while (_shaper.DataWaiting
                   && _shaper.RootEnumerator.Current.CoordinatorFactory != _coordinatorFactory
                   && _shaper.RootEnumerator.Current.CoordinatorFactory.Depth > _coordinatorFactory.Depth)
            {
                _shaper.DataWaiting = _shaper.RootEnumerator.MoveNext();
            }

            if (_shaper.DataWaiting)
            {
                // We found something, go ahead and indicate to the shaper we want 
                // this record, set up the data record, etc.
                if (_shaper.RootEnumerator.Current.CoordinatorFactory == _coordinatorFactory)
                {
                    _shaper.DataWaiting = false;
                    _shaper.RootEnumerator.Current.AcceptPendingValues();
                    result = true;
                }
            }
            return result;
        }

#if !NET40

        // See ReadInternal
        private async Task<bool> ReadInternalAsync(CancellationToken cancellationToken)
        {
            var result = false;

            // If there's nothing waiting for the root enumerator, then attempt
            // to advance it. 
            if (!_shaper.DataWaiting)
            {
                _shaper.DataWaiting =
                    await _shaper.RootEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }

            // If we have some data (we may have just read it above) then figure
            // out who it belongs to-- us or someone else. We also skip over any
            // records that are for our children (nested readers); if we're being
            // asked to read, it's too late for them to read them.
            while (_shaper.DataWaiting
                   && _shaper.RootEnumerator.Current.CoordinatorFactory != _coordinatorFactory
                   && _shaper.RootEnumerator.Current.CoordinatorFactory.Depth > _coordinatorFactory.Depth)
            {
                _shaper.DataWaiting =
                    await _shaper.RootEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }

            if (_shaper.DataWaiting)
            {
                // We found something, go ahead and indicate to the shaper we want 
                // this record, set up the data record, etc.
                if (_shaper.RootEnumerator.Current.CoordinatorFactory == _coordinatorFactory)
                {
                    _shaper.DataWaiting = false;
                    _shaper.RootEnumerator.Current.AcceptPendingValues();
                    result = true;
                }
            }
            return result;
        }

#endif

        // <inheritdoc />
        public override int FieldCount
        {
            get
            {
                EnsureInitialized();
                AssertReaderIsOpen("FieldCount");

                // In this method, we need to return a constant value, regardless
                // of how polymorphic the result is, because there is a lot of code
                // in the wild that expects it to be constant; Ideally, we'd return
                // the number of columns in the actual type that we have, but since
                // that would probably break folks, I'm leaving it at returning the
                // base set of columns that all rows will have.

                var result = _defaultRecordState.ColumnCount;
                return result;
            }
        }

        // <inheritdoc />
        public override string GetDataTypeName(int ordinal)
        {
            EnsureInitialized();
            AssertReaderIsOpen("GetDataTypeName");
            string result;
            if (_dataRecord.HasData)
            {
                result = _dataRecord.GetDataTypeName(ordinal);
            }
            else
            {
                result = _defaultRecordState.GetTypeUsage(ordinal).ToString();
            }
            return result;
        }

        // <inheritdoc />
        public override Type GetFieldType(int ordinal)
        {
            EnsureInitialized();
            AssertReaderIsOpen("GetFieldType");
            Type result;
            if (_dataRecord.HasData)
            {
                result = _dataRecord.GetFieldType(ordinal);
            }
            else
            {
                result = GetClrTypeFromTypeMetadata(_defaultRecordState.GetTypeUsage(ordinal));
            }
            return result;
        }

        // <inheritdoc />
        public override string GetName(int ordinal)
        {
            EnsureInitialized();
            AssertReaderIsOpen("GetName");
            string result;
            if (_dataRecord.HasData)
            {
                result = _dataRecord.GetName(ordinal);
            }
            else
            {
                result = _defaultRecordState.GetName(ordinal);
            }
            return result;
        }

        // <inheritdoc />
        public override int GetOrdinal(string name)
        {
            EnsureInitialized();
            AssertReaderIsOpen("GetOrdinal");
            int result;
            if (_dataRecord.HasData)
            {
                result = _dataRecord.GetOrdinal(name);
            }
            else
            {
                result = _defaultRecordState.GetOrdinal(name);
            }
            return result;
        }

        // <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Type GetProviderSpecificFieldType(int ordinal)
        {
            throw new NotSupportedException();
        }

        ////////////////////////////////////////////////////////////////////////
        //
        // The remaining methods on this class delegate to the inner data record.
        //
        ////////////////////////////////////////////////////////////////////////

        // <inheritdoc />
        public override object this[int ordinal]
        {
            get
            {
                EnsureInitialized();
                return _dataRecord[ordinal];
            }
        }

        // <inheritdoc />
        public override object this[string name]
        {
            get
            {
                EnsureInitialized();
                var ordinal = GetOrdinal(name);
                return _dataRecord[ordinal];
            }
        }

        // <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override object GetProviderSpecificValue(int ordinal)
        {
            throw new NotSupportedException();
        }

        // <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetProviderSpecificValues(object[] values)
        {
            throw new NotSupportedException();
        }

        // <inheritdoc />
        public override Object GetValue(int ordinal)
        {
            EnsureInitialized();
            return _dataRecord.GetValue(ordinal);
        }

#if !NET40

        // <inheritdoc />
        public override async Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
        {
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            return await base.GetFieldValueAsync<T>(ordinal, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

#endif

        // <inheritdoc />
        public override int GetValues(object[] values)
        {
            EnsureInitialized();
            return _dataRecord.GetValues(values);
        }

        // <inheritdoc />
        public override bool GetBoolean(int ordinal)
        {
            EnsureInitialized();
            return _dataRecord.GetBoolean(ordinal);
        }

        // <inheritdoc />
        public override byte GetByte(int ordinal)
        {
            EnsureInitialized();
            return _dataRecord.GetByte(ordinal);
        }

        // <inheritdoc />
        public override char GetChar(int ordinal)
        {
            EnsureInitialized();
            return _dataRecord.GetChar(ordinal);
        }

        // <inheritdoc />
        public override DateTime GetDateTime(int ordinal)
        {
            EnsureInitialized();
            return _dataRecord.GetDateTime(ordinal);
        }

        // <inheritdoc />
        public override Decimal GetDecimal(int ordinal)
        {
            EnsureInitialized();
            return _dataRecord.GetDecimal(ordinal);
        }

        // <inheritdoc />
        public override double GetDouble(int ordinal)
        {
            EnsureInitialized();
            return _dataRecord.GetDouble(ordinal);
        }

        // <inheritdoc />
        public override float GetFloat(int ordinal)
        {
            EnsureInitialized();
            return _dataRecord.GetFloat(ordinal);
        }

        // <inheritdoc />
        public override Guid GetGuid(int ordinal)
        {
            EnsureInitialized();
            return _dataRecord.GetGuid(ordinal);
        }

        // <inheritdoc />
        public override Int16 GetInt16(int ordinal)
        {
            EnsureInitialized();
            return _dataRecord.GetInt16(ordinal);
        }

        // <inheritdoc />
        public override Int32 GetInt32(int ordinal)
        {
            EnsureInitialized();
            return _dataRecord.GetInt32(ordinal);
        }

        // <inheritdoc />
        public override Int64 GetInt64(int ordinal)
        {
            EnsureInitialized();
            return _dataRecord.GetInt64(ordinal);
        }

        // <inheritdoc />
        public override String GetString(int ordinal)
        {
            EnsureInitialized();
            return _dataRecord.GetString(ordinal);
        }

        // <inheritdoc />
        public override bool IsDBNull(int ordinal)
        {
            EnsureInitialized();
            return _dataRecord.IsDBNull(ordinal);
        }

        // <inheritdoc />
        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            EnsureInitialized();
            return _dataRecord.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        // <inheritdoc />
        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            EnsureInitialized();
            return _dataRecord.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        // <inheritdoc />
        protected override DbDataReader GetDbDataReader(int ordinal)
        {
            EnsureInitialized();
            return (DbDataReader)_dataRecord.GetData(ordinal);
        }

        #endregion

        #region IExtendedDataRecord implementation

        // <inheritdoc />
        public DataRecordInfo DataRecordInfo
        {
            get
            {
                EnsureInitialized();
                AssertReaderIsOpen("DataRecordInfo");

                DataRecordInfo result;
                if (_dataRecord.HasData)
                {
                    result = _dataRecord.DataRecordInfo;
                }
                else
                {
                    result = _defaultRecordState.DataRecordInfo;
                }
                return result;
            }
        }

        // <inheritdoc />
        public DbDataRecord GetDataRecord(int ordinal)
        {
            EnsureInitialized();
            return _dataRecord.GetDataRecord(ordinal);
        }

        // <inheritdoc />
        public DbDataReader GetDataReader(int ordinal)
        {
            EnsureInitialized();
            return GetDbDataReader(ordinal);
        }

        #endregion
    }
}
