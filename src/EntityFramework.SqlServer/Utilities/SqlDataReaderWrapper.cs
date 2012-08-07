// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.Utilities
{
    using System.Collections;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    /// <summary>
    ///     This is a wrapper for <see cref="SqlDataReader" /> that allows a mock implementation to be used.
    /// </summary>
    internal class SqlDataReaderWrapper : MarshalByRefObject
    {
        private readonly SqlDataReader _sqlDataReader;

        protected SqlDataReaderWrapper()
        {
        }

        public SqlDataReaderWrapper(SqlDataReader sqlDataReader)
        {
            _sqlDataReader = sqlDataReader;
        }

        public virtual IDataReader GetData(int i)
        {
            return ((IDataRecord)_sqlDataReader).GetData(i);
        }

        public virtual void Dispose()
        {
            _sqlDataReader.Dispose();
        }

        public virtual Task<T> GetFieldValueAsync<T>(int ordinal)
        {
            return _sqlDataReader.GetFieldValueAsync<T>(ordinal);
        }

        public virtual Task<bool> IsDBNullAsync(int ordinal)
        {
            return _sqlDataReader.IsDBNullAsync(ordinal);
        }

        public virtual Task<bool> ReadAsync()
        {
            return _sqlDataReader.ReadAsync();
        }

        public virtual Task<bool> NextResultAsync()
        {
            return _sqlDataReader.NextResultAsync();
        }

        public virtual void Close()
        {
            _sqlDataReader.Close();
        }

        public virtual string GetDataTypeName(int i)
        {
            return _sqlDataReader.GetDataTypeName(i);
        }

        public virtual IEnumerator GetEnumerator()
        {
            return _sqlDataReader.GetEnumerator();
        }

        public virtual Type GetFieldType(int i)
        {
            return _sqlDataReader.GetFieldType(i);
        }

        public virtual string GetName(int i)
        {
            return _sqlDataReader.GetName(i);
        }

        public virtual Type GetProviderSpecificFieldType(int i)
        {
            return _sqlDataReader.GetProviderSpecificFieldType(i);
        }

        public virtual int GetOrdinal(string name)
        {
            return _sqlDataReader.GetOrdinal(name);
        }

        public virtual object GetProviderSpecificValue(int i)
        {
            return _sqlDataReader.GetProviderSpecificValue(i);
        }

        public virtual int GetProviderSpecificValues(object[] values)
        {
            return _sqlDataReader.GetProviderSpecificValues(values);
        }

        public virtual DataTable GetSchemaTable()
        {
            return _sqlDataReader.GetSchemaTable();
        }

        public virtual bool GetBoolean(int i)
        {
            return _sqlDataReader.GetBoolean(i);
        }

        public virtual XmlReader GetXmlReader(int i)
        {
            return _sqlDataReader.GetXmlReader(i);
        }

        public virtual Stream GetStream(int i)
        {
            return _sqlDataReader.GetStream(i);
        }

        public virtual byte GetByte(int i)
        {
            return _sqlDataReader.GetByte(i);
        }

        public virtual long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            return _sqlDataReader.GetBytes(i, dataIndex, buffer, bufferIndex, length);
        }

        public virtual TextReader GetTextReader(int i)
        {
            return _sqlDataReader.GetTextReader(i);
        }

        public virtual char GetChar(int i)
        {
            return _sqlDataReader.GetChar(i);
        }

        public virtual long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            return _sqlDataReader.GetChars(i, dataIndex, buffer, bufferIndex, length);
        }

        public virtual DateTime GetDateTime(int i)
        {
            return _sqlDataReader.GetDateTime(i);
        }

        public virtual decimal GetDecimal(int i)
        {
            return _sqlDataReader.GetDecimal(i);
        }

        public virtual double GetDouble(int i)
        {
            return _sqlDataReader.GetDouble(i);
        }

        public virtual float GetFloat(int i)
        {
            return _sqlDataReader.GetFloat(i);
        }

        public virtual Guid GetGuid(int i)
        {
            return _sqlDataReader.GetGuid(i);
        }

        public virtual short GetInt16(int i)
        {
            return _sqlDataReader.GetInt16(i);
        }

        public virtual int GetInt32(int i)
        {
            return _sqlDataReader.GetInt32(i);
        }

        public virtual long GetInt64(int i)
        {
            return _sqlDataReader.GetInt64(i);
        }

        public virtual SqlBoolean GetSqlBoolean(int i)
        {
            return _sqlDataReader.GetSqlBoolean(i);
        }

        public virtual SqlBinary GetSqlBinary(int i)
        {
            return _sqlDataReader.GetSqlBinary(i);
        }

        public virtual SqlByte GetSqlByte(int i)
        {
            return _sqlDataReader.GetSqlByte(i);
        }

        public virtual SqlBytes GetSqlBytes(int i)
        {
            return _sqlDataReader.GetSqlBytes(i);
        }

        public virtual SqlChars GetSqlChars(int i)
        {
            return _sqlDataReader.GetSqlChars(i);
        }

        public virtual SqlDateTime GetSqlDateTime(int i)
        {
            return _sqlDataReader.GetSqlDateTime(i);
        }

        public virtual SqlDecimal GetSqlDecimal(int i)
        {
            return _sqlDataReader.GetSqlDecimal(i);
        }

        public virtual SqlGuid GetSqlGuid(int i)
        {
            return _sqlDataReader.GetSqlGuid(i);
        }

        public virtual SqlDouble GetSqlDouble(int i)
        {
            return _sqlDataReader.GetSqlDouble(i);
        }

        public virtual SqlInt16 GetSqlInt16(int i)
        {
            return _sqlDataReader.GetSqlInt16(i);
        }

        public virtual SqlInt32 GetSqlInt32(int i)
        {
            return _sqlDataReader.GetSqlInt32(i);
        }

        public virtual SqlInt64 GetSqlInt64(int i)
        {
            return _sqlDataReader.GetSqlInt64(i);
        }

        public virtual SqlMoney GetSqlMoney(int i)
        {
            return _sqlDataReader.GetSqlMoney(i);
        }

        public virtual SqlSingle GetSqlSingle(int i)
        {
            return _sqlDataReader.GetSqlSingle(i);
        }

        public virtual SqlString GetSqlString(int i)
        {
            return _sqlDataReader.GetSqlString(i);
        }

        public virtual SqlXml GetSqlXml(int i)
        {
            return _sqlDataReader.GetSqlXml(i);
        }

        public virtual object GetSqlValue(int i)
        {
            return _sqlDataReader.GetSqlValue(i);
        }

        public virtual int GetSqlValues(object[] values)
        {
            return _sqlDataReader.GetSqlValues(values);
        }

        public virtual string GetString(int i)
        {
            return _sqlDataReader.GetString(i);
        }

        public virtual T GetFieldValue<T>(int i)
        {
            return _sqlDataReader.GetFieldValue<T>(i);
        }

        public virtual object GetValue(int i)
        {
            return _sqlDataReader.GetValue(i);
        }

        public virtual TimeSpan GetTimeSpan(int i)
        {
            return _sqlDataReader.GetTimeSpan(i);
        }

        public virtual DateTimeOffset GetDateTimeOffset(int i)
        {
            return _sqlDataReader.GetDateTimeOffset(i);
        }

        public virtual int GetValues(object[] values)
        {
            return _sqlDataReader.GetValues(values);
        }

        public virtual bool IsDBNull(int i)
        {
            return _sqlDataReader.IsDBNull(i);
        }

        public virtual bool NextResult()
        {
            return _sqlDataReader.NextResult();
        }

        public virtual bool Read()
        {
            return _sqlDataReader.Read();
        }

        public virtual Task<bool> NextResultAsync(CancellationToken cancellationToken)
        {
            return _sqlDataReader.NextResultAsync(cancellationToken);
        }

        public virtual Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            return _sqlDataReader.ReadAsync(cancellationToken);
        }

        public virtual Task<bool> IsDBNullAsync(int i, CancellationToken cancellationToken)
        {
            return _sqlDataReader.IsDBNullAsync(i, cancellationToken);
        }

        public virtual Task<T> GetFieldValueAsync<T>(int i, CancellationToken cancellationToken)
        {
            return _sqlDataReader.GetFieldValueAsync<T>(i, cancellationToken);
        }

        public virtual int Depth
        {
            get { return _sqlDataReader.Depth; }
        }

        public virtual int FieldCount
        {
            get { return _sqlDataReader.FieldCount; }
        }

        public virtual bool HasRows
        {
            get { return _sqlDataReader.HasRows; }
        }

        public virtual bool IsClosed
        {
            get { return _sqlDataReader.IsClosed; }
        }

        public virtual int RecordsAffected
        {
            get { return _sqlDataReader.RecordsAffected; }
        }

        public virtual int VisibleFieldCount
        {
            get { return _sqlDataReader.VisibleFieldCount; }
        }

        public virtual object this[int i]
        {
            get { return _sqlDataReader[i]; }
        }

        public virtual object this[string name]
        {
            get { return _sqlDataReader[name]; }
        }
    }
}
