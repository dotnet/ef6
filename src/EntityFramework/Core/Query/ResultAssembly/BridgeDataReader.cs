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
    using System.Diagnostics;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// DbDataReader functionality for the bridge.
    /// </summary>
    internal class BridgeDataReader : DbDataReader, IExtendedDataRecord
    {
        #region Private state

        /// <summary>
        /// Object that holds the state needed by the coordinator and the root enumerator
        /// </summary>
        private Shaper<RecordState> _shaper;

        /// <summary>
        /// Enumerator over shapers for NextResult() calls. 
        /// Null for nested data readers (depth > 0);
        /// </summary>
        private IEnumerator<KeyValuePair<Shaper<RecordState>, CoordinatorFactory<RecordState>>> _nextResultShaperInfoEnumerator;

        /// <summary>
        /// The coordinator we're responsible for returning results for.
        /// </summary>
        private CoordinatorFactory<RecordState> _coordinatorFactory;

        /// <summary>
        /// The default record (pre-read/past-end) state
        /// </summary>
        private RecordState _defaultRecordState;

        /// <summary>
        /// We delegate to this on our getters, to avoid duplicate code.
        /// </summary>
        private BridgeDataRecord _dataRecord;

        /// <summary>
        /// Do we have a row to read?  Determined in the constructor and
        /// should not be changed.
        /// </summary>
        private bool _hasRows;

        /// <summary>
        /// Set to true only when we've been closed through the Close() method
        /// </summary>
        private bool _isClosed;

        #endregion

        #region Constructors

        internal BridgeDataReader(
            Shaper<RecordState> shaper, CoordinatorFactory<RecordState> coordinatorFactory, int depth,
            IEnumerator<KeyValuePair<Shaper<RecordState>, CoordinatorFactory<RecordState>>> nextResultShaperInfos)
        {
            Contract.Requires(null != shaper);
            Contract.Requires(null != coordinatorFactory);
            Contract.Requires(depth == 0 || nextResultShaperInfos == null, "Nested data readers should not have multiple result sets.");

            _nextResultShaperInfoEnumerator = nextResultShaperInfos;
            SetShaper(shaper, coordinatorFactory, depth);
        }

        private void SetShaper(Shaper<RecordState> shaper, CoordinatorFactory<RecordState> coordinatorFactory, int depth)
        {
            _shaper = shaper;
            _coordinatorFactory = coordinatorFactory;
            _dataRecord = new BridgeDataRecord(shaper, depth);

            // To determine whether there are any rows for this coordinator at this place in 
            // the root enumerator, we pretty much just look at it's current record (we'll read 
            // one if there isn't one waiting) and if it matches our coordinator, we've got rows.
            _hasRows = false;

            if (!_shaper.DataWaiting)
            {
                _shaper.DataWaiting = _shaper.RootEnumerator.MoveNext();
            }

            if (_shaper.DataWaiting)
            {
                var currentRecord = _shaper.RootEnumerator.Current;

                if (null != currentRecord)
                {
                    _hasRows = (currentRecord.CoordinatorFactory == _coordinatorFactory);
                }
            }

            // Once we've created the root enumerator, we can get the default record state
            _defaultRecordState = coordinatorFactory.GetDefaultRecordState(_shaper);
            Debug.Assert(null != _defaultRecordState, "no default?");
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Ensures that the reader is actually open, and throws an exception if not
        /// </summary>
        private void AssertReaderIsOpen(string methodName)
        {
            if (IsClosed)
            {
                if (_dataRecord.IsImplicitlyClosed)
                {
                    throw new InvalidOperationException(Strings.ADP_ImplicitlyClosedDataReaderError);
                }
                if (_dataRecord.IsExplicitlyClosed)
                {
                    throw new InvalidOperationException(Strings.ADP_DataReaderClosed(methodName));
                }
            }
        }

        /// <summary>
        /// Implicitly close this (nested) data reader; will be called whenever 
        /// the user has done a GetValue() or a Read() on a parent reader/record
        /// to ensure that we consume all our results.  We do that because we 
        /// our design requires us to be positioned at the next nested reader's
        /// first row.
        /// </summary>
        internal void CloseImplicitly()
        {
            Consume();
            _dataRecord.CloseImplicitly();
        }

        /// <summary>
        /// Reads to the end of the source enumerator provided
        /// </summary>
        private void Consume()
        {
            while (ReadInternal())
            {
            }
        }

        /// <summary>
        /// Figure out the CLR type from the TypeMetadata object; For scalars, 
        /// we can get this from the metadata workspace, but for the rest, we 
        /// just guess at "Object".  You need to use the DataRecordInfo property 
        /// to get better information for those.
        /// </summary>
        /// <param name="typeUsage"></param>
        /// <returns></returns>
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

        public override int Depth
        {
            get
            {
                AssertReaderIsOpen("Depth");
                return _dataRecord.Depth;
            }
        }

        public override bool HasRows
        {
            get
            {
                AssertReaderIsOpen("HasRows");
                return _hasRows;
            }
        }

        public override bool IsClosed
        {
            get
            {
                // Rather that try and track this in two places; we just delegate
                // to the data record that we constructed; it has more reasons to 
                // have to know this than we do in the data reader.  (Of course, 
                // we look at our own closed state too...)
                return ((_isClosed) || _dataRecord.IsClosed);
            }
        }

        public override int RecordsAffected
        {
            get
            {
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

        public override void Close()
        {
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override IEnumerator GetEnumerator()
        {
            IEnumerator result = new DbEnumerator(this, true); // We always want to close the reader; 
            return result;
        }

        public override DataTable GetSchemaTable()
        {
            throw new NotSupportedException(Strings.ADP_GetSchemaTableIsNotSupported);
        }

        public override bool NextResult()
        {
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

        public override bool Read()
        {
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

        /// <summary>
        /// Internal read method; does the work of advancing the root enumerator
        /// as needed and determining whether it's current record is for our
        /// coordinator.  The public Read method does the assertions and such that
        /// we don't want to do when we're called from internal methods to do things
        /// like consume the rest of the reader's contents.
        /// </summary>
        /// <param name="rootEnumerator"></param>
        /// <returns></returns>
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

        public override int FieldCount
        {
            get
            {
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

        public override string GetDataTypeName(int ordinal)
        {
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

        public override Type GetFieldType(int ordinal)
        {
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

        public override string GetName(int ordinal)
        {
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

        public override int GetOrdinal(string name)
        {
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Type GetProviderSpecificFieldType(int ordinal)
        {
            throw new NotSupportedException();
        }

        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        //
        // The remaining methods on this class delegate to the inner data record
        //
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////

        public override object this[int ordinal]
        {
            get { return _dataRecord[ordinal]; }
        }

        public override object this[string name]
        {
            get
            {
                var ordinal = GetOrdinal(name);
                return _dataRecord[ordinal];
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override object GetProviderSpecificValue(int ordinal)
        {
            throw new NotSupportedException();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetProviderSpecificValues(object[] values)
        {
            throw new NotSupportedException();
        }

        public override Object GetValue(int ordinal)
        {
            return _dataRecord.GetValue(ordinal);
        }

        public override int GetValues(object[] values)
        {
            return _dataRecord.GetValues(values);
        }

        public override bool GetBoolean(int ordinal)
        {
            return _dataRecord.GetBoolean(ordinal);
        }

        public override byte GetByte(int ordinal)
        {
            return _dataRecord.GetByte(ordinal);
        }

        public override char GetChar(int ordinal)
        {
            return _dataRecord.GetChar(ordinal);
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return _dataRecord.GetDateTime(ordinal);
        }

        public override Decimal GetDecimal(int ordinal)
        {
            return _dataRecord.GetDecimal(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            return _dataRecord.GetDouble(ordinal);
        }

        public override float GetFloat(int ordinal)
        {
            return _dataRecord.GetFloat(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            return _dataRecord.GetGuid(ordinal);
        }

        public override Int16 GetInt16(int ordinal)
        {
            return _dataRecord.GetInt16(ordinal);
        }

        public override Int32 GetInt32(int ordinal)
        {
            return _dataRecord.GetInt32(ordinal);
        }

        public override Int64 GetInt64(int ordinal)
        {
            return _dataRecord.GetInt64(ordinal);
        }

        public override String GetString(int ordinal)
        {
            return _dataRecord.GetString(ordinal);
        }

        public override bool IsDBNull(int ordinal)
        {
            return _dataRecord.IsDBNull(ordinal);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            return _dataRecord.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return _dataRecord.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        protected override DbDataReader GetDbDataReader(int ordinal)
        {
            return (DbDataReader)_dataRecord.GetData(ordinal);
        }

        #endregion

        #region IExtendedDataRecord implementation

        public DataRecordInfo DataRecordInfo
        {
            get
            {
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

        public DbDataRecord GetDataRecord(int ordinal)
        {
            return _dataRecord.GetDataRecord(ordinal);
        }

        public DbDataReader GetDataReader(int ordinal)
        {
            return GetDbDataReader(ordinal);
        }

        #endregion
    }
}
