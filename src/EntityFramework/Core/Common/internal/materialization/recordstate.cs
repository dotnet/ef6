namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Text;

    /// <summary>
    /// The RecordState class is responsible for tracking state about a record
    /// that should be returned from a data reader.
    /// </summary>
    internal class RecordState
    {
        #region state

        /// <summary>
        /// Where to find the static information about this record
        /// </summary>
        private readonly RecordStateFactory RecordStateFactory;

        /// <summary>
        /// The coordinator factory (essentially, the reader) that we're a part of.
        /// </summary>
        internal readonly CoordinatorFactory CoordinatorFactory;

        /// <summary>
        /// True when the record is supposed to be null. (Null Structured Types...)
        /// </summary>
        private bool _pendingIsNull;

        private bool _currentIsNull;

        /// <summary>
        /// An EntityRecordInfo, with EntityKey and EntitySet populated; set 
        /// by the GatherData expression.
        /// </summary>
        private EntityRecordInfo _currentEntityRecordInfo;

        private EntityRecordInfo _pendingEntityRecordInfo;

        /// <summary>
        /// The column values; set by the GatherData expression. Really ought 
        /// to be in the Shaper.State.
        /// </summary>
        internal object[] CurrentColumnValues;

        internal object[] PendingColumnValues;

        #endregion

        #region constructor

        internal RecordState(RecordStateFactory recordStateFactory, CoordinatorFactory coordinatorFactory)
        {
            RecordStateFactory = recordStateFactory;
            CoordinatorFactory = coordinatorFactory;
            CurrentColumnValues = new object[RecordStateFactory.ColumnCount];
            PendingColumnValues = new object[RecordStateFactory.ColumnCount];
        }

        #endregion

        #region "public" surface area

        /// <summary>
        /// Move the PendingValues to the CurrentValues for this record and all nested
        /// records.  We keep the pending values separate from the current ones because
        /// we may have a nested reader in the middle, and while we're reading forward
        /// on the nested reader we we'll blast over the pending values.
        /// 
        /// This should be called as part of the data reader's Read() method.
        /// </summary>
        internal void AcceptPendingValues()
        {
            var temp = CurrentColumnValues;
            CurrentColumnValues = PendingColumnValues;
            PendingColumnValues = temp;

            _currentEntityRecordInfo = _pendingEntityRecordInfo;
            _pendingEntityRecordInfo = null;

            _currentIsNull = _pendingIsNull;

            // CONSIDER(SteveSta): If additional perforamnce is needed, here's something 
            // we could probably optimize by building an expression and compiling it.
            if (RecordStateFactory.HasNestedColumns)
            {
                for (var ordinal = 0; ordinal < CurrentColumnValues.Length; ordinal++)
                {
                    if (RecordStateFactory.IsColumnNested[ordinal])
                    {
                        var recordState = CurrentColumnValues[ordinal] as RecordState;
                        if (null != recordState)
                        {
                            recordState.AcceptPendingValues();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Return the number of columns
        /// </summary>
        internal int ColumnCount
        {
            get { return RecordStateFactory.ColumnCount; }
        }

        /// <summary>
        /// Return the DataRecordInfo for this record; if we had an EntityRecordInfo
        /// set, then return it otherwise return the static one from the factory.
        /// </summary>
        internal DataRecordInfo DataRecordInfo
        {
            get
            {
                DataRecordInfo result = _currentEntityRecordInfo;
                if (null == result)
                {
                    result = RecordStateFactory.DataRecordInfo;
                }
                return result;
            }
        }

        /// <summary>
        /// Is the record NULL?
        /// </summary>
        internal bool IsNull
        {
            get { return _currentIsNull; }
        }

        /// <summary>
        /// Implementation of DataReader's GetBytes method
        /// </summary>
        internal long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            var byteValue = (byte[])CurrentColumnValues[ordinal];
            var valueLength = byteValue.Length;
            var sourceOffset = (int)dataOffset;
            var byteCount = valueLength - sourceOffset;

            if (null != buffer)
            {
                byteCount = Math.Min(byteCount, length);

                if (0 < byteCount)
                {
                    Buffer.BlockCopy(byteValue, sourceOffset, buffer, bufferOffset, byteCount);
                }
            }
            return Math.Max(0, byteCount);
        }

        /// <summary>
        /// Implementation of DataReader's GetChars method
        /// </summary>
        internal long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            var stringValue = CurrentColumnValues[ordinal] as string;
            char[] charValue;

            if (stringValue != null)
            {
                charValue = stringValue.ToCharArray();
            }
            else
            {
                charValue = (char[])CurrentColumnValues[ordinal];
            }

            var valueLength = charValue.Length;
            var sourceOffset = (int)dataOffset;
            var charCount = valueLength - sourceOffset;

            if (null != buffer)
            {
                charCount = Math.Min(charCount, length);

                if (0 < charCount)
                {
                    Buffer.BlockCopy(
                        charValue, sourceOffset * UnicodeEncoding.CharSize,
                        buffer, bufferOffset * UnicodeEncoding.CharSize,
                        charCount * UnicodeEncoding.CharSize);
                }
            }
            return Math.Max(0, charCount);
        }

        /// <summary>
        /// Return the name of the column at the ordinal specified.
        /// </summary>
        internal string GetName(int ordinal)
        {
            // Some folks are picky about the exception we throw
            if (ordinal < 0
                || ordinal >= RecordStateFactory.ColumnCount)
            {
                throw new ArgumentOutOfRangeException("ordinal");
            }
            return RecordStateFactory.ColumnNames[ordinal];
        }

        /// <summary>
        /// This is where the GetOrdinal method for DbDataReader/DbDataRecord end up.
        /// </summary>
        internal int GetOrdinal(string name)
        {
            return RecordStateFactory.FieldNameLookup.GetOrdinal(name);
        }

        /// <summary>
        /// Return the type of the column at the ordinal specified.
        /// </summary>
        internal TypeUsage GetTypeUsage(int ordinal)
        {
            return RecordStateFactory.TypeUsages[ordinal];
        }

        /// <summary>
        /// Returns true when the column at the ordinal specified is 
        /// a record or reader column that requires special handling.
        /// </summary>
        internal bool IsNestedObject(int ordinal)
        {
            return RecordStateFactory.IsColumnNested[ordinal];
        }

        /// <summary>
        /// Called whenever we hand this record state out as the default state for
        /// a data reader; we will have already handled any existing data back to
        /// the previous group of records (that is, we couldn't be using it from two
        /// distinct readers at the same time).
        /// </summary>
        internal void ResetToDefaultState()
        {
            _currentEntityRecordInfo = null;
        }

        #endregion

        #region called from Shaper's Element Expression

        /// <summary>
        /// Called from the Element expression on the Coordinator to gather all 
        /// the data for the record; we just turn around and call the expression
        /// we build on the RecordStateFactory.
        /// </summary>
        internal RecordState GatherData(Shaper shaper)
        {
            RecordStateFactory.GatherData(shaper);
            _pendingIsNull = false;
            return this;
        }

        /// <summary>
        /// Called by the GatherData expression to set the data for the 
        /// specified column value
        /// </summary>
        internal bool SetColumnValue(int ordinal, object value)
        {
            PendingColumnValues[ordinal] = value;
            return true;
        }

        /// <summary>
        /// Called by the GatherData expression to set the data for the 
        /// EntityRecordInfo
        /// </summary>
        internal bool SetEntityRecordInfo(EntityKey entityKey, EntitySet entitySet)
        {
            _pendingEntityRecordInfo = new EntityRecordInfo(RecordStateFactory.DataRecordInfo, entityKey, entitySet);
            return true;
        }

        /// <summary>
        /// Called from the Element expression on the Coordinator to indicate that
        /// the record should be NULL.
        /// </summary>
        internal RecordState SetNullRecord()
        {
            // CONSIDER(SteveSta): If additional, performance is needed, we could make these
            // singleton objects on the RecordStateFactory, but that has additional overhead 
            // and working set that we may not want to have.
            for (var i = 0; i < PendingColumnValues.Length; i++)
            {
                PendingColumnValues[i] = DBNull.Value;
            }
            _pendingEntityRecordInfo = null; // the default is already setup correctly on the record state factory
            _pendingIsNull = true;
            return this;
        }

        #endregion
    }
}
