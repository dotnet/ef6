// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityClient
{
    using System.Collections;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     A data reader class for the entity client provider
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class EntityDataReader : DbDataReader, IExtendedDataRecord
    {
        // The command object that owns this reader
        private EntityCommand _command;

        private readonly CommandBehavior _behavior;

        // Store data reader, _storeExtendedDataRecord points to the same reader as _storeDataReader, it's here to just
        // save the casting wherever it's used
        private readonly DbDataReader _storeDataReader;
        private readonly IExtendedDataRecord _storeExtendedDataRecord;

        private bool _disposed;

        /// <summary>
        ///     The constructor for the data reader, each EntityDataReader must always be associated with a EntityCommand and an underlying
        ///     DbDataReader.  It is expected that EntityDataReader only has a reference to EntityCommand and doesn't assume responsibility
        ///     of cleaning the command object, but it does assume responsibility of cleaning up the store data reader object.
        /// </summary>
        internal EntityDataReader(EntityCommand command, DbDataReader storeDataReader, CommandBehavior behavior)
        {
            DebugCheck.NotNull(command);
            DebugCheck.NotNull(storeDataReader);

            _command = command;
            _storeDataReader = storeDataReader;
            _storeExtendedDataRecord = storeDataReader as IExtendedDataRecord;
            _behavior = behavior;
        }

        /// <summary>
        ///     For test purposes only.
        /// </summary>
        internal EntityDataReader()
        {
        }

        /// <summary>
        ///     Get the depth of nesting for the current row
        /// </summary>
        public override int Depth
        {
            get { return _storeDataReader.Depth; }
        }

        /// <summary>
        ///     Get the number of columns in the current row
        /// </summary>
        public override int FieldCount
        {
            get { return _storeDataReader.FieldCount; }
        }

        /// <summary>
        ///     Get whether the data reader has any rows
        /// </summary>
        public override bool HasRows
        {
            get { return _storeDataReader.HasRows; }
        }

        /// <summary>
        ///     Get whether the data reader has been closed
        /// </summary>
        public override bool IsClosed
        {
            get { return _storeDataReader.IsClosed; }
        }

        /// <summary>
        ///     Get whether the data reader has any rows
        /// </summary>
        public override int RecordsAffected
        {
            get { return _storeDataReader.RecordsAffected; }
        }

        /// <summary>
        ///     Get the value of a column with the given ordinal
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column to retrieve the value </param>
        public override object this[int ordinal]
        {
            get { return _storeDataReader[ordinal]; }
        }

        /// <summary>
        ///     Get the value of a column with the given name
        /// </summary>
        /// <param name="name"> The name of the column to retrieve the value </param>
        public override object this[string name]
        {
            get
            {
                Check.NotNull(name, "name");
                return _storeDataReader[name];
            }
        }

        /// <summary>
        ///     Get the number of non-hidden fields in the reader
        /// </summary>
        public override int VisibleFieldCount
        {
            get { return _storeDataReader.VisibleFieldCount; }
        }

        /// <summary>
        ///     DataRecordInfo property describing the contents of the record.
        /// </summary>
        public DataRecordInfo DataRecordInfo
        {
            get
            {
                if (null == _storeExtendedDataRecord)
                {
                    // if a command has no results (e.g. FunctionImport with no return type),
                    // there is nothing to report.
                    return null;
                }

                return _storeExtendedDataRecord.DataRecordInfo;
            }
        }

        /// <summary>
        ///     Close this data reader
        /// </summary>
        public override void Close()
        {
            if (_command != null)
            {
                _storeDataReader.Close();

                // Notify the command object that we are closing, so clean up operations such as copying output parameters can be done
                _command.NotifyDataReaderClosing();
                if ((_behavior & CommandBehavior.CloseConnection)
                    == CommandBehavior.CloseConnection)
                {
                    Debug.Assert(_command.Connection != null);
                    _command.Connection.Close();
                }

                _command = null;
            }
        }

        /// <summary>
        ///     Releases the resources used by this data reader
        /// </summary>
        /// <param name="disposing"> true to release both managed and unmanaged resources, false to release only unmanaged resources </param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _storeDataReader.Dispose();
                }
            }
            _disposed = true;

            base.Dispose(disposing);
        }

        /// <summary>
        ///     Get the boolean value of a column with the given ordinal
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column to retrieve the value </param>
        /// <returns> The boolean value </returns>
        public override bool GetBoolean(int ordinal)
        {
            return _storeDataReader.GetBoolean(ordinal);
        }

        /// <summary>
        ///     Get the byte value of a column with the given ordinal
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column to retrieve the value </param>
        /// <returns> The byte value </returns>
        public override byte GetByte(int ordinal)
        {
            return _storeDataReader.GetByte(ordinal);
        }

        /// <summary>
        ///     Get the byte array value of a column with the given ordinal
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column to retrieve the value </param>
        /// <param name="dataOffset"> The index within the row to start reading </param>
        /// <param name="buffer"> The buffer to copy into </param>
        /// <param name="bufferOffset"> The index in the buffer indicating where the data is copied into </param>
        /// <param name="length"> The maximum number of bytes to read </param>
        /// <returns> The actual number of bytes read </returns>
        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            return _storeDataReader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        /// <summary>
        ///     Get the char value of a column with the given ordinal
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column to retrieve the value </param>
        /// <returns> The char value </returns>
        public override char GetChar(int ordinal)
        {
            return _storeDataReader.GetChar(ordinal);
        }

        /// <summary>
        ///     Get the char array value of a column with the given ordinal
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column to retrieve the value </param>
        /// <param name="dataOffset"> The index within the row to start reading </param>
        /// <param name="buffer"> The buffer to copy into </param>
        /// <param name="bufferOffset"> The index in the buffer indicating where the data is copied into </param>
        /// <param name="length"> The maximum number of bytes to read </param>
        /// <returns> The actual number of characters read </returns>
        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return _storeDataReader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        /// <summary>
        ///     Get the name of the data type of the column with the given ordinal
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column to retrieve the name of the data type </param>
        /// <returns> The name of the data type of the column </returns>
        public override string GetDataTypeName(int ordinal)
        {
            return _storeDataReader.GetDataTypeName(ordinal);
        }

        /// <summary>
        ///     Get the datetime value of a column with the given ordinal
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column to retrieve the value </param>
        /// <returns> The datetime value </returns>
        public override DateTime GetDateTime(int ordinal)
        {
            return _storeDataReader.GetDateTime(ordinal);
        }

        /// <summary>
        ///     Get the data reader of a column with the given ordinal
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column to retrieve the reader </param>
        /// <returns> The data reader </returns>
        protected override DbDataReader GetDbDataReader(int ordinal)
        {
            return _storeDataReader.GetData(ordinal);
        }

        /// <summary>
        ///     Get the decimal value of a column with the given ordinal
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column to retrieve the value </param>
        /// <returns> The decimal value </returns>
        public override decimal GetDecimal(int ordinal)
        {
            return _storeDataReader.GetDecimal(ordinal);
        }

        /// <summary>
        ///     Get the double value of a column with the given ordinal
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column to retrieve the value </param>
        /// <returns> The double value </returns>
        public override double GetDouble(int ordinal)
        {
            return _storeDataReader.GetDouble(ordinal);
        }

        /// <summary>
        ///     Get the data type of the column with the given ordinal
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column to retrieve the data type </param>
        /// <returns> The data type of the column </returns>
        public override Type GetFieldType(int ordinal)
        {
            return _storeDataReader.GetFieldType(ordinal);
        }

        /// <summary>
        ///     Get the float value of a column with the given ordinal
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column to retrieve the value </param>
        /// <returns> The float value </returns>
        public override float GetFloat(int ordinal)
        {
            return _storeDataReader.GetFloat(ordinal);
        }

        /// <summary>
        ///     Get the guid value of a column with the given ordinal
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column to retrieve the value </param>
        /// <returns> The guid value </returns>
        public override Guid GetGuid(int ordinal)
        {
            return _storeDataReader.GetGuid(ordinal);
        }

        /// <summary>
        ///     Get the int16 value of a column with the given ordinal
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column to retrieve the value </param>
        /// <returns> The int16 value </returns>
        public override short GetInt16(int ordinal)
        {
            return _storeDataReader.GetInt16(ordinal);
        }

        /// <summary>
        ///     Get the int32 value of a column with the given ordinal
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column to retrieve the value </param>
        /// <returns> The int32 value </returns>
        public override int GetInt32(int ordinal)
        {
            return _storeDataReader.GetInt32(ordinal);
        }

        /// <summary>
        ///     Get the int64 value of a column with the given ordinal
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column to retrieve the value </param>
        /// <returns> The int64 value </returns>
        public override long GetInt64(int ordinal)
        {
            return _storeDataReader.GetInt64(ordinal);
        }

        /// <summary>
        ///     Get the name of a column with the given ordinal
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column to retrieve the name </param>
        /// <returns> The name </returns>
        public override string GetName(int ordinal)
        {
            return _storeDataReader.GetName(ordinal);
        }

        /// <summary>
        ///     Get the ordinal of a column with the given name
        /// </summary>
        /// <param name="name"> The name of the column to retrieve the ordinal </param>
        /// <returns> The ordinal of the column </returns>
        public override int GetOrdinal(string name)
        {
            Check.NotNull(name, "name");

            return _storeDataReader.GetOrdinal(name);
        }

        /// <summary>
        ///     implementation for DbDataReader.GetProviderSpecificFieldType() method
        /// </summary>
        /// <param name="ordinal"> </param>
        /// <returns> </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Type GetProviderSpecificFieldType(int ordinal)
        {
            return _storeDataReader.GetProviderSpecificFieldType(ordinal);
        }

        /// <summary>
        ///     implementation for DbDataReader.GetProviderSpecificValue() method
        /// </summary>
        /// <param name="ordinal"> </param>
        /// <returns> </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override object GetProviderSpecificValue(int ordinal)
        {
            return _storeDataReader.GetProviderSpecificValue(ordinal);
        }

        /// <summary>
        ///     implementation for DbDataReader.GetProviderSpecificValues() method
        /// </summary>
        /// <param name="values"> </param>
        /// <returns> </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetProviderSpecificValues(object[] values)
        {
            return _storeDataReader.GetProviderSpecificValues(values);
        }

        /// <summary>
        ///     Get the DataTable that describes the columns of this data reader
        /// </summary>
        /// <returns> The DataTable describing the columns </returns>
        public override DataTable GetSchemaTable()
        {
            return _storeDataReader.GetSchemaTable();
        }

        /// <summary>
        ///     Get the string value of a column with the given ordinal
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column to retrieve the value </param>
        /// <returns> The string value </returns>
        public override string GetString(int ordinal)
        {
            return _storeDataReader.GetString(ordinal);
        }

        /// <summary>
        ///     Get the value of a column with the given ordinal
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column to retrieve the value </param>
        /// <returns> The value </returns>
        public override object GetValue(int ordinal)
        {
            return _storeDataReader.GetValue(ordinal);
        }

        /// <summary>
        ///     Get the values for all the columns and for the current row
        /// </summary>
        /// <param name="values"> The array where values are copied into </param>
        /// <returns> The number of System.Object instances in the array </returns>
        public override int GetValues(object[] values)
        {
            return _storeDataReader.GetValues(values);
        }

        /// <summary>
        ///     Get whether the value of a column is DBNull
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column to retrieve the value </param>
        /// <returns> true if the column value is DBNull </returns>
        public override bool IsDBNull(int ordinal)
        {
            return _storeDataReader.IsDBNull(ordinal);
        }

        /// <summary>
        ///     Move the reader to the next result set when reading a batch of statements
        /// </summary>
        /// <returns> true if there are more result sets; false otherwise </returns>
        public override bool NextResult()
        {
            try
            {
                return _storeDataReader.NextResult();
            }
            catch (Exception e)
            {
                throw new EntityCommandExecutionException(Strings.EntityClient_StoreReaderFailed, e);
            }
        }

#if !NET40

        /// <summary>
        ///     Asynchronously moves the reader to the next result set when reading a batch of statements
        /// </summary>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> 
        ///     A task that represents the asynchronous operation. 
        ///     The task result contains true if there are more result sets; false otherwise.
        /// </returns>
        public override async Task<bool> NextResultAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await _storeDataReader.NextResultAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception e)
            {
                throw new EntityCommandExecutionException(Strings.EntityClient_StoreReaderFailed, e);
            }
        }

#endif

        /// <summary>
        ///     Move the reader to the next row of the current result set
        /// </summary>
        /// <returns> true if there are more rows; false otherwise </returns>
        public override bool Read()
        {
            return _storeDataReader.Read();
        }

#if !NET40

        /// <summary>
        ///     Asynchronously moves the reader to the next row of the current result set
        /// </summary>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> 
        ///     A task that represents the asynchronous operation. 
        ///     The task result contains true if there are more rows; false otherwise. 
        /// </returns>
        public override Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            return _storeDataReader.ReadAsync(cancellationToken);
        }

#endif

        /// <summary>
        ///     Get an enumerator for enumerating results over this data reader
        /// </summary>
        /// <returns> An enumerator for this data reader </returns>
        public override IEnumerator GetEnumerator()
        {
            return _storeDataReader.GetEnumerator();
        }

        /// <summary>
        ///     Used to return a nested DbDataRecord.
        /// </summary>
        public DbDataRecord GetDataRecord(int i)
        {
            if (null == _storeExtendedDataRecord)
            {
                Debug.Assert(FieldCount == 0, "we have fields but no metadata?");

                // for a query with no results, any request is out of range...
                throw new ArgumentOutOfRangeException("i");
            }

            return _storeExtendedDataRecord.GetDataRecord(i);
        }

        /// <summary>
        ///     Used to return a nested result
        /// </summary>
        public DbDataReader GetDataReader(int i)
        {
            return GetDbDataReader(i);
        }
    }
}
