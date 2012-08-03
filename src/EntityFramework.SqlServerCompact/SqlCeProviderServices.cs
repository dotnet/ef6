// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.SqlServerCompact
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.SqlServerCompact.Resources;
    using System.Data.Entity.SqlServerCompact.SqlGen;
    using System.Data.SqlServerCe;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Transactions;

    /// <summary>
    /// The ProviderServices object for the Sql CE provider
    /// </summary>
    internal sealed class SqlCeProviderServices : DbProviderServices
    {
        /// <summary>
        /// Singleton object;
        /// </summary>
        internal static readonly SqlCeProviderServices Instance = new SqlCeProviderServices();

        internal bool _isLocalProvider = true;

        #region CodeOnly Methods

        /// <summary>
        /// API for generating script for creating schema objects from the Store Item Collection.
        /// </summary>
        /// <param name="providerManifestToken">Provider manifest</param>
        /// <param name="storeItemCollection">Store items</param>
        /// <returns>T-SQL script for generating schema objects.</returns>
        protected override string DbCreateDatabaseScript(string providerManifestToken, StoreItemCollection storeItemCollection)
        {
            // Call the helper for creating schema objects.
            return string.Concat(SqlDdlBuilder.CreateObjectsScript(storeItemCollection, true).ToArray());
        }

        /// <summary>
        /// API for checkin whether database exists or not.
        /// This will internally only check whether the file that the connection points to exists or not.
        /// Note: In case of SQLCE, timeout and storeItemCollection parameters are ignored.
        /// </summary>
        /// <param name="connection">Connection</param>
        /// <param name="timeOut">Timeout for internal commands.</param>
        /// <param name="storeItemCollection">Item Collection.</param>
        /// <returns>Bool indicating whether database exists or not.</returns>
        protected override bool DbDatabaseExists(DbConnection connection, int? timeOut, StoreItemCollection storeItemCollection)
        {
            // Validate and cast the connection.
            ValidateConnection(connection);

            if (_isLocalProvider)
            {
                return CommonUtils.DatabaseExists(connection.DataSource);
            }
            else
            {
                Type rdpType;

                // If we are working with RDP, then we will need to invoke the APIs through reflection.
                var engine = RemoteProviderHelper.GetRemoteSqlCeEngine(connection.ConnectionString, out rdpType);
                Debug.Assert(engine != null);

                var mi = rdpType.GetMethod("FileExists", new[] { typeof(string), typeof(int?) });
                Debug.Assert(mi != null);

                // We will pass 'timeout' to RDP, this will be used as timeout period for connecting and executing on TDSServer.
                return (bool)(mi.Invoke(engine, new object[] { connection.DataSource, timeOut }));
            }
        }

        /// <summary>
        /// API for deleting the database.
        /// In SQLCE case, this will translate to File.Delete() call.
        /// Note: Timeout and storeItemCollection parameters are ignored.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="timeOut"></param>
        /// <param name="storeItemCollection"></param>
        protected override void DbDeleteDatabase(DbConnection connection, int? timeOut, StoreItemCollection storeItemCollection)
        {
            // Validate that connection is a SqlCeConnection.
            ValidateConnection(connection);

            // We don't support create/delete database operations inside a transaction as they can't be rolled back.
            if (InTransactionScope())
            {
                throw ADP1.DeleteDatabaseNotAllowedWithinTransaction();
            }

            // Throw an exception if connection is open.
            // We should not close the connection because user could have result sets/data readers associated with this connection.
            // Thus, it is users responsiblity to close the connection before calling delete database.
            //
            if (connection.State
                == ConnectionState.Open)
            {
                throw ADP1.DeleteDatabaseWithOpenConnection();
            }

            if (_isLocalProvider)
            {
                CommonUtils.DeleteDatabase(connection.DataSource);
            }
            else
            {
                try
                {
                    Type rdpType;

                    // If we are working with RDP, then we will need to invoke the APIs through reflection.
                    var engine = RemoteProviderHelper.GetRemoteSqlCeEngine(connection.ConnectionString, out rdpType);
                    Debug.Assert(engine != null);

                    // Invoke the required method on SqlCeEngine.
                    var mi = rdpType.GetMethod("DeleteDatabaseWithError", new[] { typeof(string), typeof(int?) });
                    Debug.Assert(mi != null);

                    // We will pass 'timeout' to RDP, this will be used as timeout period for connecting and executing on TDSServer.
                    mi.Invoke(engine, new object[] { connection.DataSource, timeOut });
                }
                catch (Exception e)
                {
                    throw e.GetBaseException();
                }
            }
        }

        /// <summary>
        /// API for creating the databse and schema objects given a StoreItemCollection.
        /// This will do following things:
        ///     1. Create a new database using SqlCeEngine.CreateDatabase().
        ///     2. Generate scripts for creating schema objects.
        ///     3. Execute the scrip generated in step2.
        /// </summary>
        /// <param name="connection">Connection</param>
        /// <param name="timeOut">Timeout for internal commands</param>
        /// <param name="storeItemCollection">Store Item Collection</param>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void DbCreateDatabase(DbConnection connection, int? timeOut, StoreItemCollection storeItemCollection)
        {
            #region Check arguments

            ADP1.CheckArgumentNull(connection, "Connection");
            ADP1.CheckArgumentNull(storeItemCollection, "StoreItemCollection");

            #endregion

            // Validate that connection is a SqlCeConnection.
            ValidateConnection(connection);

            // We don't support create/delete database operations inside a transaction as they can't be rolled back.
            if (InTransactionScope())
            {
                throw ADP1.CreateDatabaseNotAllowedWithinTransaction();
            }

            if (_isLocalProvider)
            {
                var engine = new SqlCeEngine(connection.ConnectionString);
                engine.CreateDatabase();
                engine.Dispose();
            }
            else
            {
                try
                {
                    Type rdpType;

                    // If we are working with RDP, then we will need to invoke the APIs through reflection.
                    var engine = RemoteProviderHelper.GetRemoteSqlCeEngine(connection.ConnectionString, out rdpType);
                    Debug.Assert(engine != null);

                    // Invoke the required method on SqlCeEngine.
                    var mi = rdpType.GetMethod("CreateDatabase", new[] { typeof(int?) });
                    Debug.Assert(mi != null);

                    // We will pass 'timeout' to RDP, this will be used as timeout period for connecting and executing on TDSServer.
                    mi.Invoke(engine, new object[] { timeOut });
                }
                catch (Exception e)
                {
                    throw e.GetBaseException();
                }
            }

            // Create the command object depending on provider.
            var command = connection.CreateCommand();

            // Create the command texts from StoreItemCollection.
            var commandTextCollection = SqlDdlBuilder.CreateObjectsScript(storeItemCollection, false);

            DbTransaction transaction = null;

            try
            {
                // Open the connection.
                connection.Open();

                // Open a transaction and attach to the command.
                transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                // Execute each statement.
                foreach (var text in commandTextCollection)
                {
                    command.CommandText = text;
                    command.ExecuteNonQuery();
                }

                // Commit the transaction.
                transaction.Commit();
            }
            catch (Exception e)
            {
                if (transaction != null)
                {
                    // Rollback the transaction.
                    transaction.Rollback();
                }

                // Throw IOE with SqlCeException embedded as inner exception.
                throw new InvalidOperationException(EntityRes.GetString(EntityRes.IncompleteDatabaseCreation), e);
            }
            finally
            {
                // Close connection and cleanup objects.
                if (command != null)
                {
                    command.Dispose();
                }
                if (transaction != null)
                {
                    transaction.Dispose();
                }
                if (connection != null)
                {
                    connection.Close();
                }
            }
        }

        #region Private Helpers

        // Private helper for validatingn the SqlCeConnection.
        private void ValidateConnection(DbConnection connection)
        {
            // Check whether it is a valid SqlCeConnection.
            var isValid = _isLocalProvider
                              ? connection is SqlCeConnection
                              : RemoteProviderHelper.CompareObjectEqualsToType(connection, RemoteProvider.SqlCeConnection);
            if (!isValid)
            {
                throw ADP1.InvalidConnectionType();
            }
        }

        // Check whether we are running under a TransactionScope.
        // Note(VipulHa): We can't use Connection.IsEnlisted or Connection.HasDelegatedTransaction, as we will get a closed connection.
        //                The connection could also be openend outside the transactionScope. But we should still not allow DDLs inside
        //                transaction scope as they can't rolled back.
        //
        private static bool InTransactionScope()
        {
            return (Transaction.Current != null);
        }

        #endregion

        #endregion

        /// <summary>
        /// Create a Command Definition object, given the connection and command tree
        /// </summary>
        /// <param name="providerManifest">provider manifest that was determined from metadata</param>
        /// <param name="commandTree">command tree for the statement</param>
        /// <returns>an executable command definition object</returns>
        protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree)
        {
            Debug.Assert(providerManifest != null, "CreateCommandDefinition passed null provider manifest to CreateDbCommandDefinition?");
            Debug.Assert(commandTree != null, "CreateCommandDefinition did not validate commandTree argument?");

            var prototype = CreateCommand(providerManifest, commandTree);
            var result = CreateCommandDefinition(prototype);
            return result;
        }

        /// <summary>
        /// Create a SqlCeCommand object, given the provider manifest and command tree
        /// </summary>
        /// <param name="providerManifest">provider manifest</param>
        /// <param name="commandTree">command tree for the statement</param>
        /// <returns>a command object</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private DbCommand CreateCommand(DbProviderManifest providerManifest, DbCommandTree commandTree)
        {
            // DEVNOTE/CAUTION: This method could be called either from Remote or Local Provider.
            // Ensure that the code works well with the both provider types.
            // The methods called from the below code also need to be capable
            // of handling both provider types.
            // 
            // NOTE: Remote Provider is loaded at runtime, if available.
            // This is done to remove hard dependency on Remote Provider and
            // it might not be present in all scenarios. All Remote Provider
            // type checks need to be done using RemoteProviderHelper class.
            //

            ADP1.CheckArgumentNull(providerManifest, "providerManifest");
            ADP1.CheckArgumentNull(commandTree, "commandTree");

            if (commandTree is DbFunctionCommandTree)
            {
                throw ADP1.NotSupported(EntityRes.GetString(EntityRes.StoredProceduresNotSupported));
            }

            var command = _isLocalProvider
                              ? new SqlCeMultiCommand()
                              : (DbCommand)RemoteProviderHelper.CreateRemoteProviderType(RemoteProvider.SqlCeCommand);
            command.Connection = null; // don't hold on to the connection when we're going to cache this forever;

            List<DbParameter> parameters;
            CommandType commandType;

            var commandTexts = SqlGenerator.GenerateSql(commandTree, out parameters, out commandType, _isLocalProvider);

            if (_isLocalProvider)
            {
                Debug.Assert(command is SqlCeMultiCommand, "SqlCeMultiCommand expected");
                // Set the multiple command texts for the command object
                ((SqlCeMultiCommand)command).CommandTexts = commandTexts;
            }
            else
            {
                // Set the command text for the RDP case.
                Debug.Assert(commandTexts.Length == 1, "BatchQueries are not supported in designer scenarios");
                command.CommandText = commandTexts[0];
            }

            command.CommandType = commandType;

            // Now make sure we populate the command's parameters from the CQT's parameters:
            //
            foreach (var queryParameter in commandTree.Parameters)
            {
                DbParameter parameter;
                const bool ignoreMaxLengthFacet = false;
                parameter = CreateSqlCeParameter(
                    queryParameter.Key, queryParameter.Value, DBNull.Value, ignoreMaxLengthFacet, _isLocalProvider);
                command.Parameters.Add(parameter);
            }

            // Now add parameters added as part of SQL gen (note: this feature is only safe for DML SQL gen which
            // does not support user parameters, where there is no risk of name collision)
            //
            if (null != parameters
                && 0 < parameters.Count)
            {
                if (!(commandTree is DbDeleteCommandTree ||
                      commandTree is DbInsertCommandTree ||
                      commandTree is DbUpdateCommandTree))
                {
                    throw ADP1.InternalError(ADP1.InternalErrorCode.SqlGenParametersNotPermitted);
                }

                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }

        // The provider manifest token helps to distinguish between store versions. 
        // We have only one backend version.
        // However, use the connection passed in to determine whether 
        // the provider is local provider or remote provider
        //
        protected override string GetDbProviderManifestToken(DbConnection connection)
        {
            // vamshikb: Do we need to validate the connection and connection string
            // before returning the ProviderManifestToken????

            // Determine the type of DbConnection
            // This method should never be called at runtime, so the provider
            // must be remote provider.
            // Throw if it is none.
            //
            if (connection.GetType()
                == typeof(SqlCeConnection))
            {
                _isLocalProvider = true;
            }
            else if (RemoteProviderHelper.CompareObjectEqualsToType(connection, RemoteProvider.SqlCeConnection))
            {
                _isLocalProvider = false;
            }
            else
            {
                throw ADP1.Argument(EntityRes.GetString(EntityRes.Mapping_Provider_WrongConnectionType, "SqlCeConnection"));
            }

            return SqlCeProviderManifest.Token40;
        }

        protected override DbProviderManifest GetDbProviderManifest(string versionHint)
        {
            // This method can be called at runtime or design time.
            //

            if (string.IsNullOrEmpty(versionHint))
            {
                throw ADP1.Argument(EntityRes.GetString(EntityRes.UnableToDetermineStoreVersion));
            }

            return new SqlCeProviderManifest(_isLocalProvider);
        }

        /// <summary>
        /// Constructs a SqlCeParameter 
        /// </summary>
        /// <param name="queryParameter"></param>
        /// <returns></returns>
        internal static DbParameter CreateSqlCeParameter(
            string name, TypeUsage type, object value, bool ignoreMaxLengthFacet, bool isLocalProvider)
        {
            var rdpSqlCeParameter = Type.GetType(RemoteProvider.SqlCeParameter);
            // No other parameter type is supported.
            //
            Debug.Assert(null != type, "no type mapping?");

            int? size;
            byte? precision;
            byte? scale;

            var result = isLocalProvider
                             ? new SqlCeParameter()
                             : (DbParameter)RemoteProviderHelper.CreateRemoteProviderType(RemoteProvider.SqlCeParameter);
            result.ParameterName = name;
            result.Value = value;

            // .Direction
            // parameter.Direction - take the default. we don't support output parameters.
            result.Direction = ParameterDirection.Input;

            // .Size, .Precision, .Scale and .SqlDbType
            var sqlDbType = GetSqlDbType(type, out size, out precision, out scale);

            // Skip guessing the parameter type (only for strings & blobs) if parameter size is not available
            // Instead, let QP take proper guess at execution time with available details.
            //
            if ((null != size)
                || (!TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.String) &&
                    !TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Binary)))
            {
                if (isLocalProvider)
                {
                    var sqlCeParameter = (SqlCeParameter)result;
                    if (sqlCeParameter.SqlDbType != sqlDbType)
                    {
                        sqlCeParameter.SqlDbType = sqlDbType;
                    }
                }
                else
                {
                    // Remote Provider is loaded by reflection. As SqlDbType is not part of the base interface
                    // We need to access this using reflection only.
                    var rdpType = RemoteProviderHelper.GetRemoteProviderType(RemoteProvider.SqlCeParameter);
                    var rdpInfo = rdpType.GetProperty("SqlDbType");
                    rdpInfo.SetValue(result, sqlDbType, null);
                }
            }

            // Note that we overwrite 'facet' parameters where either the value is different or
            // there is an output parameter. This is because output parameters in SqlClient have their
            // facets clobbered if they are implicitly set (e.g. if the Precision was implicitly set
            // by setting the value)
            if (!ignoreMaxLengthFacet && size.HasValue
                && (result.Size != size.Value))
            {
                result.Size = size.Value;
            }

            if (precision.HasValue
                && (((IDbDataParameter)result).Precision != precision.Value))
            {
                ((IDbDataParameter)result).Precision = precision.Value;
            }
            if (scale.HasValue
                && (((IDbDataParameter)result).Scale != scale.Value))
            {
                ((IDbDataParameter)result).Scale = scale.Value;
            }

            // .IsNullable
            var isNullable = TypeSemantics.IsNullable(type);
            if (isNullable != result.IsNullable)
            {
                result.IsNullable = isNullable;
            }

            return result;
        }

        /// <summary>
        /// Determines SqlDbType for the given primitive type. Extracts facet
        /// information as well.
        /// </summary>
        private static SqlDbType GetSqlDbType(TypeUsage type, out int? size, out byte? precision, out byte? scale)
        {
            // only supported for primitive type
            var primitiveTypeKind = TypeSemantics.GetPrimitiveTypeKind(type);

            size = default(int?);
            precision = default(byte?);
            scale = default(byte?);

            // CONSIDER(CMeek):: add logic for Xml here
            switch (primitiveTypeKind)
            {
                case PrimitiveTypeKind.Binary:
                    // for output parameters, ensure there is space...
                    size = GetParameterSize(type);
                    return GetBinaryDbType(type);

                case PrimitiveTypeKind.Boolean:
                    return SqlDbType.Bit;

                case PrimitiveTypeKind.Byte:
                    return SqlDbType.TinyInt;

                case PrimitiveTypeKind.Time:
                    throw ADP1.NotSupported(EntityRes.GetString(EntityRes.ProviderDoesNotSupportType, "Time"));

                case PrimitiveTypeKind.DateTimeOffset:
                    throw ADP1.NotSupported(EntityRes.GetString(EntityRes.ProviderDoesNotSupportType, "DateTimeOffset"));

                case PrimitiveTypeKind.DateTime:
                    return SqlDbType.DateTime;

                case PrimitiveTypeKind.Decimal:
                    precision = GetParameterPrecision(type, null);
                    scale = GetScale(type);
                    return SqlDbType.Decimal;

                case PrimitiveTypeKind.Double:
                    return SqlDbType.Float;

                case PrimitiveTypeKind.Guid:
                    return SqlDbType.UniqueIdentifier;

                case PrimitiveTypeKind.Int16:
                    return SqlDbType.SmallInt;

                case PrimitiveTypeKind.Int32:
                    return SqlDbType.Int;

                case PrimitiveTypeKind.Int64:
                    return SqlDbType.BigInt;

                case PrimitiveTypeKind.SByte:
                    return SqlDbType.SmallInt;

                case PrimitiveTypeKind.Single:
                    return SqlDbType.Real;

                case PrimitiveTypeKind.String:
                    size = GetParameterSize(type);
                    return GetStringDbType(type);

                default:
                    Debug.Fail("unknown PrimitiveTypeKind " + primitiveTypeKind);
                    return SqlDbType.Variant;
            }
        }

        /// <summary>
        /// Determines preferred value for SqlParameter.Size. Returns null
        /// where there is no preference.
        /// </summary>
        private static int? GetParameterSize(TypeUsage type)
        {
            int maxLength;
            if (TypeHelpers.TryGetMaxLength(type, out maxLength))
            {
                // if the MaxLength facet has a specific value use it
                return maxLength;
            }
                // SQLCE doesn't support output parameters. So
                // excluding the logic for isOutParam 
            else
            {
                //we use to default(int?) to identify isMaxLength
                return default(int?);
            }
        }

        /// <summary>
        /// Returns SqlParameter.Precision where the type facet exists. Otherwise,
        /// returns null.
        /// </summary>
        private static byte? GetParameterPrecision(TypeUsage type, byte? defaultIfUndefined)
        {
            byte precision;
            if (TypeHelpers.TryGetPrecision(type, out precision))
            {
                return precision;
            }
            else
            {
                return defaultIfUndefined;
            }
        }

        /// <summary>
        /// Returns SqlParameter.Scale where the type facet exists. Otherwise,
        /// returns null.
        /// </summary>
        private static byte? GetScale(TypeUsage type)
        {
            byte scale;
            if (TypeHelpers.TryGetScale(type, out scale))
            {
                return scale;
            }
            else
            {
                return default(byte?);
            }
        }

        /// <summary>
        /// Chooses the appropriate SqlDbType for the given string type.
        /// </summary>
        private static SqlDbType GetStringDbType(TypeUsage type)
        {
            Debug.Assert(
                type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType &&
                PrimitiveTypeKind.String == ((PrimitiveType)type.EdmType).PrimitiveTypeKind, "only valid for string type");

            SqlDbType dbType;
            if (type.EdmType.Name.ToUpperInvariant() == "XML")
            {
                // vamshikb: throw as SQLCE doesn't support XML datatype.
                throw ADP1.NotSupported(EntityRes.GetString(EntityRes.ProviderDoesNotSupportType, "XML"));
            }
            else
            {
                // Specific type depends on whether the string is a unicode string and whether it is a fixed length string.
                // By default, assume widest type (unicode) and most common type (variable length)
                bool unicode, fixedLength, isMaxLength = false;
                int maxLength;
                if (!TypeHelpers.TryGetIsFixedLength(type, out fixedLength))
                {
                    fixedLength = false;
                }

                if (!TypeHelpers.TryGetIsUnicode(type, out unicode))
                {
                    unicode = true;
                }

                if (!TypeHelpers.TryGetMaxLength(type, out maxLength))
                {
                    isMaxLength = true;
                }

                Debug.Assert(unicode, "SQLCE supports unicode strings only.");

                // vamshikb: SQLCE supports unicode datatypes only.
                // Include logic related to the unicode datatypes alone. (unicode == true)
                if (fixedLength)
                {
                    dbType = SqlDbType.NChar;
                }
                else
                {
                    dbType = isMaxLength ? SqlDbType.NText : SqlDbType.NVarChar;
                }
            }
            return dbType;
        }

        /// <summary>
        /// Chooses the appropriate SqlDbType for the given binary type.
        /// </summary>
        private static SqlDbType GetBinaryDbType(TypeUsage type)
        {
            Debug.Assert(
                type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType &&
                PrimitiveTypeKind.Binary == ((PrimitiveType)type.EdmType).PrimitiveTypeKind, "only valid for binary type");

            SqlDbType dbType;
            // Specific type depends on whether the binary value is fixed length. By default, assume variable length.
            bool fixedLength, isMaxLength = false;
            int maxLength;
            if (!TypeHelpers.TryGetIsFixedLength(type, out fixedLength))
            {
                fixedLength = false;
            }

            if (!TypeHelpers.TryGetMaxLength(type, out maxLength))
            {
                isMaxLength = true;
            }

            if (fixedLength)
            {
                dbType = SqlDbType.Binary;
            }
            else
            {
                dbType = isMaxLength ? SqlDbType.Image : SqlDbType.VarBinary;
            }
            return dbType;
        }
    }
}
