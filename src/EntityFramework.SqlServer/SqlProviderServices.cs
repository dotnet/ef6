// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer.Resources;
    using System.Data.Entity.SqlServer.SqlGen;
    using System.Data.Entity.SqlServer.Utilities;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// The DbProviderServices implementation for the SqlClient provider for SQL Server.
    /// </summary>
    /// <remarks>
    /// Note that instance of this type also resolve additional provider services for Microsoft SQL Server
    /// when this type is registered as an EF provider either using an entry in the application's config file
    /// or through code-based registration in <see cref="DbConfiguration" />.
    /// The services resolved are:
    /// Requests for <see cref="IDbConnectionFactory" /> are resolved to a Singleton instance of
    /// <see cref="SqlConnectionFactory" /> to create connections to SQL Express by default.
    /// Requests for <see cref="Func{IDbExecutionStrategy}" /> for the invariant name "System.Data.SqlClient"
    /// for any server name are resolved to a delegate that returns a <see cref="DefaultSqlExecutionStrategy" />
    /// to provide a non-retrying policy for SQL Server.
    /// Requests for <see cref="MigrationSqlGenerator" /> for the invariant name "System.Data.SqlClient" are
    /// resolved to <see cref="SqlServerMigrationSqlGenerator" /> instances to provide default Migrations SQL
    /// generation for SQL Server.
    /// Requests for <see cref="DbSpatialServices" /> for the invariant name "System.Data.SqlClient" are
    /// resolved to a Singleton instance of <see cref="SqlSpatialServices" /> to provide default spatial
    /// services for SQL Server.
    /// </remarks>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public sealed class SqlProviderServices : DbProviderServices
    {
        /// <summary>
        /// This is the well-known string using in configuration files and code-based configuration as
        /// the "provider invariant name" used to specify Microsoft SQL Server for ADO.NET and
        /// Entity Framework provider services.
        /// </summary>
        public const string ProviderInvariantName = "System.Data.SqlClient";

        private ConcurrentDictionary<string, SqlProviderManifest> _providerManifests =
            new ConcurrentDictionary<string, SqlProviderManifest>();

        // <summary>
        // Private constructor to ensure only Singleton instance is created.
        // </summary>
        private SqlProviderServices()
        {
            AddDependencyResolver(new SingletonDependencyResolver<IDbConnectionFactory>(new SqlConnectionFactory()));

            AddDependencyResolver(
                new ExecutionStrategyResolver<DefaultSqlExecutionStrategy>(
                    ProviderInvariantName, null, () => new DefaultSqlExecutionStrategy()));

            AddDependencyResolver(
                new SingletonDependencyResolver<Func<MigrationSqlGenerator>>(
                    () => new SqlServerMigrationSqlGenerator(), ProviderInvariantName));

            // Spatial provider will be returned if the key is null (meaning that a default provider was requested)
            // or if a key was provided and the invariant name matches.
            AddDependencyResolver(
                new SingletonDependencyResolver<DbSpatialServices>(
                    SqlSpatialServices.Instance,
                    k =>
                    {
                        if (k == null)
                        {
                            return true;
                        }

                        var asSpatialKey = k as DbProviderInfo;
                        return asSpatialKey != null
                               && asSpatialKey.ProviderInvariantName == ProviderInvariantName
                               && SupportsSpatial(asSpatialKey.ProviderManifestToken);
                    }));
        }

        // <summary>
        // Singleton object
        // </summary>
        private static readonly SqlProviderServices _providerInstance = new SqlProviderServices();

        private static bool _truncateDecimalsToScale = true;

        /// <summary>
        /// The Singleton instance of the SqlProviderServices type.
        /// </summary>
        public static SqlProviderServices Instance
        {
            get { return _providerInstance; }
        }

        /// <summary>
        /// Set this flag to false to prevent <see cref="decimal" /> values from being truncated to
        /// the scale (number of decimal places) defined for the column. The default value is true,
        /// indicating that decimal values will be truncated, in order to prevent breaking existing
        /// applications that depend on this behavior.
        /// </summary>
        /// <remarks>
        /// With this flag set to true <see cref="SqlParameter" /> objects are created with their Scale
        /// properties set. When this flag is set to false then the Scale properties are not set, meaning
        /// that the truncation behavior of SqlParameter is avoided.
        /// </remarks>
        public static bool TruncateDecimalsToScale
        {
            get { return _truncateDecimalsToScale; }
            set { _truncateDecimalsToScale = value; }
        }

        /// <summary>
        /// Registers a handler to process non-error messages coming from the database provider.
        /// </summary>
        /// <param name="connection"> The connection to receive information for. </param>
        /// <param name="handler"> The handler to process messages. </param>
        public override void RegisterInfoMessageHandler(DbConnection connection, Action<string> handler)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(handler, "handler");

            var sqlConnection = (connection as SqlConnection);

            if (sqlConnection == null)
            {
                throw new ArgumentException(Strings.Mapping_Provider_WrongConnectionType(typeof(SqlConnection)));
            }

            sqlConnection.InfoMessage
                += (_, e)
                   =>
                       {
                           if (!string.IsNullOrWhiteSpace(e.Message))
                           {
                               handler(e.Message);
                           }
                       };
        }

        /// <summary>
        /// Create a Command Definition object, given the connection and command tree
        /// </summary>
        /// <param name="providerManifest"> provider manifest that was determined from metadata </param>
        /// <param name="commandTree"> command tree for the statement </param>
        /// <returns> an executable command definition object </returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree)
        {
            Check.NotNull(providerManifest, "providerManifest");
            Check.NotNull(commandTree, "commandTree");

            var prototype = CreateCommand(providerManifest, commandTree);
            var result = CreateCommandDefinition(prototype);
            return result;
        }

        // <summary>
        // Create a SqlCommand object, given the provider manifest and command tree
        // </summary>
        // <param name="providerManifest"> provider manifest </param>
        // <param name="commandTree"> command tree for the statement </param>
        // <returns> a command object </returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private static DbCommand CreateCommand(DbProviderManifest providerManifest, DbCommandTree commandTree)
        {
            DebugCheck.NotNull(providerManifest);
            DebugCheck.NotNull(commandTree);

            var sqlManifest = (providerManifest as SqlProviderManifest);
            if (sqlManifest == null)
            {
                throw new ArgumentException(Strings.Mapping_Provider_WrongManifestType(typeof(SqlProviderManifest)));
            }

            var sqlVersion = sqlManifest.SqlVersion;
            var command = new SqlCommand();

            List<SqlParameter> parameters;
            CommandType commandType;
            HashSet<string> paramsToForceNonUnicode;
            command.CommandText = SqlGenerator.GenerateSql(
                commandTree, sqlVersion, out parameters, out commandType, out paramsToForceNonUnicode);
            command.CommandType = commandType;

            // Get the function (if any) implemented by the command tree since this influences our interpretation of parameters
            EdmFunction function = null;
            if (commandTree.CommandTreeKind
                == DbCommandTreeKind.Function)
            {
                function = ((DbFunctionCommandTree)commandTree).EdmFunction;
            }
            // Now make sure we populate the command's parameters from the CQT's parameters:
            foreach (var queryParameter in commandTree.Parameters)
            {
                SqlParameter parameter;

                // Use the corresponding function parameter TypeUsage where available (currently, the SSDL facets and 
                // type trump user-defined facets and type in the EntityCommand).
                FunctionParameter functionParameter;
                if (null != function
                    && function.Parameters.TryGetValue(queryParameter.Key, false, out functionParameter))
                {
                    const bool preventTruncation = false;
                    parameter = CreateSqlParameter(
                        functionParameter.Name, functionParameter.TypeUsage, functionParameter.Mode, DBNull.Value, preventTruncation,
                        sqlVersion);
                }
                else
                {
                    //Reached when a Function Command Tree is passed an incorrect parameter name by the user.
                    var parameterType = paramsToForceNonUnicode != null
                                        && paramsToForceNonUnicode.Contains(queryParameter.Key)
                                            ? queryParameter.Value.ForceNonUnicode()
                                            : queryParameter.Value;

                    const bool preventTruncation = false;
                    parameter = CreateSqlParameter(
                        queryParameter.Key, parameterType, ParameterMode.In, DBNull.Value, preventTruncation, sqlVersion);
                }
                command.Parameters.Add(parameter);
            }

            // Now add parameters added as part of SQL gen (note: this feature is only safe for DML SQL gen which
            // does not support user parameters, where there is no risk of name collision)
            if (null != parameters
                && 0 < parameters.Count)
            {
                if (commandTree.CommandTreeKind != DbCommandTreeKind.Delete
                    && commandTree.CommandTreeKind != DbCommandTreeKind.Insert
                    && commandTree.CommandTreeKind != DbCommandTreeKind.Update)
                {
                    throw new InvalidOperationException(
                        Strings.ADP_InternalProviderError(1017 /*InternalErrorCode.SqlGenParametersNotPermitted*/));
                }
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }

        /// <summary>
        /// Sets the parameter value and appropriate facets for the given <see cref="TypeUsage"/>.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="parameterType">The type of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        protected override void SetDbParameterValue(DbParameter parameter, TypeUsage parameterType, object value)
        {
            Check.NotNull(parameter, "parameter");
            Check.NotNull(parameterType, "parameterType");

            // Ensure a value that can be used with SqlParameter
            value = EnsureSqlParameterValue(value);

            if (parameterType.IsPrimitiveType(PrimitiveTypeKind.String)
                || parameterType.IsPrimitiveType(PrimitiveTypeKind.Binary))
            {
                var size = GetParameterSize(parameterType, ((parameter.Direction & ParameterDirection.Output) == ParameterDirection.Output));
                if (!size.HasValue)
                {
                    // Remember the current Size
                    var previousSize = parameter.Size;

                    // Infer the Size from the value
                    parameter.Size = 0;
                    parameter.Value = value;

                    if (previousSize > -1)
                    {
                        // The 'max' length was chosen as a specific value for the parameter's Size property on Sql8 (4000 or 8000)
                        // because no MaxLength was specified in the TypeUsage and the provider is Sql8. 
                        // If the value's length is less than or equal to this preset size, then the Size value can be retained, 
                        // otherwise this preset size must be removed in favor of the Size inferred from the value itself.

                        // If the inferred Size is less than the preset 'max' size, restore that preset size
                        if (parameter.Size < previousSize)
                        {
                            parameter.Size = previousSize;
                        }
                    }
                    else
                    {
                        // -1 was chosen as the parameter's size because no MaxLength was specified in the TypeUsage and the 
                        // provider is more recent than Sql8. However, it is more optimal to specify a non-max (-1) value for
                        // the size where possible, since 'max' parameters may prevent, for example, filter pushdown.
                        // (see Dev10#617447 for more details)
                        var suggestedLength = GetNonMaxLength(((SqlParameter)parameter).SqlDbType);
                        if (parameter.Size < suggestedLength)
                        {
                            parameter.Size = suggestedLength;
                        }
                        else if (parameter.Size > suggestedLength)
                        {
                            // The parameter size is greater than the suggested length, so the suggested length cannot be used.
                            // Since the provider is Sql9 or newer, set the size to max (-1) instead of the inferred size for better plan reuse.
                            parameter.Size = -1;
                        }
                    }
                }
                else
                {
                    // Just set the value
                    parameter.Value = value;
                }
            }
            else
            {
                // Not a string or binary parameter - just set the value
                parameter.Value = value;
            }
        }

        /// <summary>
        /// Returns provider manifest token for a given connection.
        /// </summary>
        /// <param name="connection"> Connection to find manifest token from. </param>
        /// <returns> The provider manifest token for the specified connection. </returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected override string GetDbProviderManifestToken(DbConnection connection)
        {
            Check.NotNull(connection, "connection");

            if (string.IsNullOrEmpty(connection.ConnectionString))
            {
                throw new ArgumentException(Strings.UnableToDetermineStoreVersion);
            }

            string providerManifestToken = null;
            // Try to get the provider manifest token from the database connection
            // That failing, try using connection to master database (in case the database doesn't exist yet)
            try
            {
                UsingConnection(connection, conn => { providerManifestToken = QueryForManifestToken(conn); });
            }
            catch
            {
                UsingMasterConnection(connection, conn => { providerManifestToken = QueryForManifestToken(conn); });
            }
            return providerManifestToken;
        }

        private static string QueryForManifestToken(DbConnection conn)
        {
            var sqlVersion = SqlVersionUtils.GetSqlVersion(conn);
            var serverType = sqlVersion >= SqlVersion.Sql11 ? SqlVersionUtils.GetServerType(conn) : ServerType.OnPremises;
            return SqlVersionUtils.GetVersionHint(sqlVersion, serverType);
        }

        /// <summary>
        /// Returns the provider manifest by using the specified version information.
        /// </summary>
        /// <param name="versionHint"> The token information associated with the provider manifest. </param>
        /// <returns> The provider manifest by using the specified version information. </returns>
        protected override DbProviderManifest GetDbProviderManifest(string versionHint)
        {
            if (string.IsNullOrEmpty(versionHint))
            {
                throw new ArgumentException(Strings.UnableToDetermineStoreVersion);
            }

            return _providerManifests.GetOrAdd(versionHint, s => new SqlProviderManifest(s));
        }

        /// <summary>
        /// Gets a spatial data reader for SQL Server.
        /// </summary>
        /// <param name="fromReader"> The reader where the spatial data came from. </param>
        /// <param name="versionHint"> The manifest token associated with the provider manifest. </param>
        /// <returns> The spatial data reader. </returns>
        protected override DbSpatialDataReader GetDbSpatialDataReader(DbDataReader fromReader, string versionHint)
        {
            var underlyingReader = fromReader as SqlDataReader;
            if (underlyingReader == null)
            {
                throw new ProviderIncompatibleException(Strings.SqlProvider_NeedSqlDataReader(fromReader.GetType()));
            }

            return SupportsSpatial(versionHint)
                       ? new SqlSpatialDataReader(
                             GetSpatialServices(new DbProviderInfo(ProviderInvariantName, versionHint)),
                             new SqlDataReaderWrapper(underlyingReader))
                       : null;
        }

        /// <summary>
        /// Gets a spatial data reader for SQL Server.
        /// </summary>
        /// <param name="versionHint"> The manifest token associated with the provider manifest. </param>
        /// <returns> The spatial data reader. </returns>
        [Obsolete(
            "Return DbSpatialServices from the GetService method. See http://go.microsoft.com/fwlink/?LinkId=260882 for more information.")]
        protected override DbSpatialServices DbGetSpatialServices(string versionHint)
        {
            return SupportsSpatial(versionHint)
                       ? SqlSpatialServices.Instance
                       : null;
        }

        private static bool SupportsSpatial(string versionHint)
        {
            if (string.IsNullOrEmpty(versionHint))
            {
                throw new ArgumentException(Strings.UnableToDetermineStoreVersion);
            }

            // GetSqlVersion will throw ArgumentException if manifestToken is null, empty, or not recognized.
            var tokenVersion = SqlVersionUtils.GetSqlVersion(versionHint);

            // SQL spatial support is only available for SQL Server 2008 and later
            return tokenVersion >= SqlVersion.Sql10;
        }

        // <summary>
        // Creates a SqlParameter given a name, type, and direction
        // </summary>
        internal static SqlParameter CreateSqlParameter(
            string name, TypeUsage type, ParameterMode mode, object value, bool preventTruncation, SqlVersion version)
        {
            int? size;
            byte? precision;
            byte? scale;
            string udtTypeName;

            value = EnsureSqlParameterValue(value);

            var result = new SqlParameter(name, value);

            // .Direction
            var direction = ParameterModeToParameterDirection(mode);
            if (result.Direction != direction)
            {
                result.Direction = direction;
            }

            // .Size, .Precision, .Scale and .SqlDbType
            // output parameters are handled differently (we need to ensure there is space for return
            // values where the user has not given a specific Size/MaxLength)
            var isOutParam = mode != ParameterMode.In;
            var sqlDbType = GetSqlDbType(type, isOutParam, version, out size, out precision, out scale, out udtTypeName);

            if (result.SqlDbType != sqlDbType)
            {
                result.SqlDbType = sqlDbType;
            }

            if (sqlDbType == SqlDbType.Udt)
            {
                result.UdtTypeName = udtTypeName;
            }

            // Note that we overwrite 'facet' parameters where either the value is different or
            // there is an output parameter. This is because output parameters in SqlClient have their
            // facets clobbered if they are implicitly set (e.g. if the Size was implicitly set
            // by setting the value)
            if (size.HasValue)
            {
                // size.HasValue is always true for Output parameters
                if ((isOutParam || result.Size != size.Value))
                {
                    if (preventTruncation && size.Value != -1)
                    {
                        // To prevent truncation, set the Size of the parameter to the larger of either
                        // the declared length or the actual length for the parameter. This allows SQL
                        // Server to complain if a value is too long while preventing cache misses for
                        // values within the range.
                        result.Size = Math.Max(result.Size, size.Value);
                    }
                    else
                    {
                        result.Size = size.Value;
                    }
                }
            }
            else
            {
                var typeKind = ((PrimitiveType)type.EdmType).PrimitiveTypeKind;
                if (typeKind == PrimitiveTypeKind.String)
                {
                    result.Size = GetDefaultStringMaxLength(version, sqlDbType);
                }
                else if (typeKind == PrimitiveTypeKind.Binary)
                {
                    result.Size = GetDefaultBinaryMaxLength(version);
                }
            }
            if (precision.HasValue
                && (isOutParam || (result.Precision != precision.Value && _truncateDecimalsToScale)))
            {
                result.Precision = precision.Value;
            }
            if (scale.HasValue
                && (isOutParam || (result.Scale != scale.Value && _truncateDecimalsToScale)))
            {
                result.Scale = scale.Value;
            }

            // .IsNullable
            var isNullable = type.IsNullable();
            if (isOutParam || isNullable != result.IsNullable)
            {
                result.IsNullable = isNullable;
            }

            return result;
        }

        // Returns ParameterDirection corresponding to given ParameterMode
        private static ParameterDirection ParameterModeToParameterDirection(ParameterMode mode)
        {
            switch (mode)
            {
                case ParameterMode.In:
                    return ParameterDirection.Input;

                case ParameterMode.InOut:
                    return ParameterDirection.InputOutput;

                case ParameterMode.Out:
                    return ParameterDirection.Output;

                case ParameterMode.ReturnValue:
                    return ParameterDirection.ReturnValue;

                default:
                    Debug.Fail("unrecognized mode " + mode.ToString());
                    return default(ParameterDirection);
            }
        }

        // <summary>
        // Validates that the specified value is compatible with SqlParameter and if not, attempts to return an appropriate value that is.
        // Currently only spatial values (DbGeography/DbGeometry) may not be directly usable with SqlParameter. For these types, an instance
        // of the corresponding SQL Server CLR spatial UDT will be manufactured based on the spatial data contained in
        // <paramref name="value" />.
        // If <paramref name="value" /> is an instance of DbGeography/DbGeometry that was read from SQL Server by this provider, then the wrapped
        // CLR UDT value is available via the ProviderValue property (see SqlSpatialServices for the full conversion process from instances of
        // DbGeography/DbGeometry to instances of the CLR SqlGeography/SqlGeometry UDTs)
        // </summary>
        internal static object EnsureSqlParameterValue(object value)
        {
            if (value != null
                && value != DBNull.Value
                && value.GetType().IsClass())
            {
                // If the parameter is being created based on an actual value (typically for constants found in DML expressions) then a DbGeography/DbGeometry
                // value must be replaced by an an appropriate Microsoft.SqlServer.Types.SqlGeography/SqlGeometry instance. Since the DbGeography/DbGeometry
                // value may not have been originally created by this SqlClient provider services implementation, just using the ProviderValue is not sufficient.
                var geographyValue = value as DbGeography;
                if (geographyValue != null)
                {
                    value = SqlTypesAssemblyLoader.DefaultInstance.GetSqlTypesAssembly().ConvertToSqlTypesGeography(geographyValue);
                }
                else
                {
                    var geometryValue = value as DbGeometry;
                    if (geometryValue != null)
                    {
                        value = SqlTypesAssemblyLoader.DefaultInstance.GetSqlTypesAssembly().ConvertToSqlTypesGeometry(geometryValue);
                    }
                }
            }

            return value;
        }

        // <summary>
        // Determines SqlDbType for the given primitive type. Extracts facet
        // information as well.
        // </summary>
        private static SqlDbType GetSqlDbType(
            TypeUsage type, bool isOutParam, SqlVersion version, out int? size, out byte? precision, out byte? scale, out string udtName)
        {
            // only supported for primitive type
            var primitiveTypeKind = ((PrimitiveType)type.EdmType).PrimitiveTypeKind;

            size = default(int?);
            precision = default(byte?);
            scale = default(byte?);
            udtName = default(string);

            // CONSIDER(CMeek):: add logic for Xml here
            switch (primitiveTypeKind)
            {
                case PrimitiveTypeKind.Binary:
                    // for output parameters, ensure there is space...
                    size = GetParameterSize(type, isOutParam);
                    return GetBinaryDbType(type);

                case PrimitiveTypeKind.Boolean:
                    return SqlDbType.Bit;

                case PrimitiveTypeKind.Byte:
                    return SqlDbType.TinyInt;

                case PrimitiveTypeKind.Time:
                    if (!SqlVersionUtils.IsPreKatmai(version))
                    {
                        precision = GetKatmaiDateTimePrecision(type, isOutParam);
                    }
                    return SqlDbType.Time;

                case PrimitiveTypeKind.DateTimeOffset:
                    if (!SqlVersionUtils.IsPreKatmai(version))
                    {
                        precision = GetKatmaiDateTimePrecision(type, isOutParam);
                    }
                    return SqlDbType.DateTimeOffset;

                case PrimitiveTypeKind.DateTime:
                    //For katmai pick the type with max precision which is datetime2
                    if (!SqlVersionUtils.IsPreKatmai(version))
                    {
                        precision = GetKatmaiDateTimePrecision(type, isOutParam);
                        return SqlDbType.DateTime2;
                    }
                    else
                    {
                        return SqlDbType.DateTime;
                    }

                case PrimitiveTypeKind.Decimal:
                    precision = GetParameterPrecision(type, null);
                    scale = GetScale(type);
                    return SqlDbType.Decimal;

                case PrimitiveTypeKind.Double:
                    return SqlDbType.Float;

                case PrimitiveTypeKind.Geography:
                    {
                        udtName = "geography";
                        return SqlDbType.Udt;
                    }

                case PrimitiveTypeKind.Geometry:
                    {
                        udtName = "geometry";
                        return SqlDbType.Udt;
                    }

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
                    size = GetParameterSize(type, isOutParam);
                    return GetStringDbType(type);

                default:
                    Debug.Fail("unknown PrimitiveTypeKind " + primitiveTypeKind);
                    return SqlDbType.Variant;
            }
        }

        // <summary>
        // Determines preferred value for SqlParameter.Size. Returns null
        // where there is no preference.
        // </summary>
        private static int? GetParameterSize(TypeUsage type, bool isOutParam)
        {
            Facet maxLengthFacet;
            if (type.Facets.TryGetValue(DbProviderManifest.MaxLengthFacetName, false, out maxLengthFacet)
                &&
                null != maxLengthFacet.Value)
            {
                if (maxLengthFacet.IsUnbounded)
                {
                    return -1;
                }
                else
                {
                    return (int?)maxLengthFacet.Value;
                }
            }
            else if (isOutParam)
            {
                // if the parameter is a return/out/inout parameter, ensure there 
                // is space for any value
                return -1;
            }
            else
            {
                // no value
                return default(int?);
            }
        }

        private static int GetNonMaxLength(SqlDbType type)
        {
            var result = -1;
            if (type == SqlDbType.NChar
                || type == SqlDbType.NVarChar)
            {
                result = 4000;
            }
            else if (type == SqlDbType.Char
                     || type == SqlDbType.VarChar
                     ||
                     type == SqlDbType.Binary
                     || type == SqlDbType.VarBinary)
            {
                result = 8000;
            }
            return result;
        }

        private static int GetDefaultStringMaxLength(SqlVersion version, SqlDbType type)
        {
            int result;
            if (version < SqlVersion.Sql9)
            {
                if (type == SqlDbType.NChar
                    || type == SqlDbType.NVarChar)
                {
                    result = 4000;
                }
                else
                {
                    result = 8000;
                }
            }
            else
            {
                result = -1;
            }
            return result;
        }

        private static int GetDefaultBinaryMaxLength(SqlVersion version)
        {
            int result;
            if (version < SqlVersion.Sql9)
            {
                result = 8000;
            }
            else
            {
                result = -1;
            }
            return result;
        }

        // <summary>
        // Returns SqlParameter.Precision where the type facet exists. Otherwise,
        // returns null or the maximum available precision to avoid truncation (which can occur
        // for output parameters).
        // </summary>
        private static byte? GetKatmaiDateTimePrecision(TypeUsage type, bool isOutParam)
        {
            var defaultIfUndefined = isOutParam ? 7 : (byte?)null;
            return GetParameterPrecision(type, defaultIfUndefined);
        }

        // <summary>
        // Returns SqlParameter.Precision where the type facet exists. Otherwise,
        // returns null.
        // </summary>
        private static byte? GetParameterPrecision(TypeUsage type, byte? defaultIfUndefined)
        {
            byte precision;
            if (type.TryGetPrecision(out precision))
            {
                return precision;
            }
            else
            {
                return defaultIfUndefined;
            }
        }

        // <summary>
        // Returns SqlParameter.Scale where the type facet exists. Otherwise,
        // returns null.
        // </summary>
        private static byte? GetScale(TypeUsage type)
        {
            byte scale;
            if (type.TryGetScale(out scale))
            {
                return scale;
            }
            else
            {
                return default(byte?);
            }
        }

        // <summary>
        // Chooses the appropriate SqlDbType for the given string type.
        // </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private static SqlDbType GetStringDbType(TypeUsage type)
        {
            Debug.Assert(
                type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType &&
                PrimitiveTypeKind.String == ((PrimitiveType)type.EdmType).PrimitiveTypeKind, "only valid for string type");

            SqlDbType dbType;
            if (type.EdmType.Name.ToLowerInvariant() == "xml")
            {
                dbType = SqlDbType.Xml;
            }
            else
            {
                // Specific type depends on whether the string is a unicode string and whether it is a fixed length string.
                // By default, assume widest type (unicode) and most common type (variable length)
                bool unicode;

                if (!type.TryGetIsUnicode(out unicode))
                {
                    unicode = true;
                }

                if (type.IsFixedLength())
                {
                    dbType = (unicode ? SqlDbType.NChar : SqlDbType.Char);
                }
                else
                {
                    dbType = (unicode ? SqlDbType.NVarChar : SqlDbType.VarChar);
                }
            }
            return dbType;
        }

        // <summary>
        // Chooses the appropriate SqlDbType for the given binary type.
        // </summary>
        private static SqlDbType GetBinaryDbType(TypeUsage type)
        {
            Debug.Assert(
                type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType &&
                PrimitiveTypeKind.Binary == ((PrimitiveType)type.EdmType).PrimitiveTypeKind, "only valid for binary type");

            // Specific type depends on whether the binary value is fixed length. By default, assume variable length.

            return type.IsFixedLength() ? SqlDbType.Binary : SqlDbType.VarBinary;
        }

        /// <summary>
        /// Generates a data definition language (DDL) script that creates schema objects 
        /// (tables, primary keys, foreign keys) based on the contents of the StoreItemCollection 
        /// parameter and targeted for the version of the database corresponding to the provider manifest token.
        /// </summary>
        /// <param name="providerManifestToken"> The provider manifest token identifying the target version. </param>
        /// <param name="storeItemCollection"> The structure of the database. </param>
        /// <returns>
        /// A DDL script that creates schema objects based on the contents of the StoreItemCollection parameter 
        /// and targeted for the version of the database corresponding to the provider manifest token.
        /// </returns>
        protected override string DbCreateDatabaseScript(string providerManifestToken, StoreItemCollection storeItemCollection)
        {
            Check.NotNull(providerManifestToken, "providerManifestToken");
            Check.NotNull(storeItemCollection, "storeItemCollection");

            var version = SqlVersionUtils.GetSqlVersion(providerManifestToken);
            return CreateObjectsScript(version, storeItemCollection);
        }

        /// <summary>
        /// Create the database and the database objects.
        /// If initial catalog is not specified, but AttachDBFilename is specified, we generate a random database name based on the AttachDBFilename.
        /// Note: this causes pollution of the db, as when the connection string is later used, the mdf will get attached under a different name.
        /// However if we try to replicate the name under which it would be attached, the following scenario would fail:
        /// The file does not exist, but registered with database.
        /// The user calls:  If (DatabaseExists) DeleteDatabase
        /// CreateDatabase
        /// For further details on the behavior when AttachDBFilename is specified see Dev10# 188936
        /// </summary>
        /// <param name="connection">Connection to a non-existent database that needs to be created and populated with the store objects indicated with the storeItemCollection parameter.</param>
        /// <param name="commandTimeout">Execution timeout for any commands needed to create the database.</param>
        /// <param name="storeItemCollection">The collection of all store items based on which the script should be created.</param>
        protected override void DbCreateDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(storeItemCollection, "storeItemCollection");

            var sqlConnection = SqlProviderUtilities.GetRequiredSqlConnection(connection);
            string databaseName, dataFileName, logFileName;
            GetOrGenerateDatabaseNameAndGetFileNames(sqlConnection, out databaseName, out dataFileName, out logFileName);

            var createDatabaseScript = SqlDdlBuilder.CreateDatabaseScript(databaseName, dataFileName, logFileName);
            var sqlVersion = CreateDatabaseFromScript(commandTimeout, sqlConnection, createDatabaseScript);

            // Create database already succeeded. If there is a failure from this point on, the user should be informed.
            try
            {
                // Clear connection pool for the database connection since after the 'create database' call, a previously
                // invalid connection may now be valid.
                SqlConnection.ClearPool(sqlConnection);

                var setDatabaseOptionsScript = SqlDdlBuilder.SetDatabaseOptionsScript(sqlVersion, databaseName);
                if (!String.IsNullOrEmpty(setDatabaseOptionsScript))
                {
                    UsingMasterConnection(
                        sqlConnection, conn =>
                            {
                                // set database options
                                using (var command = CreateCommand(conn, setDatabaseOptionsScript, commandTimeout))
                                {
                                    DbInterception.Dispatch.Command.NonQuery(command, new DbCommandInterceptionContext());
                                }
                            });
                }

                var createObjectsScript = CreateObjectsScript(sqlVersion, storeItemCollection);
                if (!string.IsNullOrWhiteSpace(createObjectsScript))
                {
                    UsingConnection(
                        sqlConnection, conn =>
                            {
                                // create database objects
                                using (var command = CreateCommand(conn, createObjectsScript, commandTimeout))
                                {
                                    DbInterception.Dispatch.Command.NonQuery(command, new DbCommandInterceptionContext());
                                }
                            });
                }
            }
            catch (Exception e)
            {
                // Try to drop the database
                try
                {
                    DropDatabase(sqlConnection, commandTimeout, databaseName);
                }
                catch (Exception ie)
                {
                    // The creation of the database succeeded, the creation of the database objects failed, and the dropping of the database failed.
                    throw new InvalidOperationException(
                        Strings.SqlProvider_IncompleteCreateDatabase,
                        new AggregateException(Strings.SqlProvider_IncompleteCreateDatabaseAggregate, e, ie));
                }
                // The creation of the database succeeded, the creation of the database objects failed, the database was dropped, no reason to wrap the exception
                throw;
            }
        }

        private static void GetOrGenerateDatabaseNameAndGetFileNames(
            SqlConnection sqlConnection, out string databaseName, out string dataFileName, out string logFileName)
        {
            DebugCheck.NotNull(sqlConnection);

            var connectionStringBuilder = new SqlConnectionStringBuilder(sqlConnection.ConnectionString);

            // Get the file names
            var attachDBFile = connectionStringBuilder.AttachDBFilename;
            if (string.IsNullOrEmpty(attachDBFile))
            {
                dataFileName = null;
                logFileName = null;
            }
            else
            {
                //Handle the other cases
                dataFileName = GetMdfFileName(attachDBFile);
                logFileName = GetLdfFileName(dataFileName);
            }

            // Get the database name
            if (!string.IsNullOrEmpty(connectionStringBuilder.InitialCatalog))
            {
                databaseName = connectionStringBuilder.InitialCatalog;
            }
            else if (dataFileName != null)
            {
                //generate the database name here
                databaseName = GenerateDatabaseName(dataFileName);
            }
            else
            {
                throw new InvalidOperationException(Strings.SqlProvider_DdlGeneration_MissingInitialCatalog);
            }
        }

        // <summary>
        // Get the Ldf name given the Mdf full name
        // </summary>
        private static string GetLdfFileName(string dataFileName)
        {
            string logFileName;
            var directory = new FileInfo(dataFileName).Directory;
            logFileName = Path.Combine(directory.FullName, String.Concat(Path.GetFileNameWithoutExtension(dataFileName), "_log.ldf"));
            return logFileName;
        }

        // <summary>
        // Generates database name based on the given mdfFileName.
        // The logic is replicated from System.Web.DataAccess.SqlConnectionHelper
        // </summary>
        private static string GenerateDatabaseName(string mdfFileName)
        {
            var toUpperFileName = mdfFileName.ToUpper(CultureInfo.InvariantCulture);
            var strippedFileNameChars = Path.GetFileNameWithoutExtension(toUpperFileName).ToCharArray();

            for (var iter = 0; iter < strippedFileNameChars.Length; iter++)
            {
                if (!char.IsLetterOrDigit(strippedFileNameChars[iter]))
                {
                    strippedFileNameChars[iter] = '_';
                }
            }

            var strippedFileName = new string(strippedFileNameChars);
            strippedFileName = strippedFileName.Length > 30 ? strippedFileName.Substring(0, 30) : strippedFileName;

            var databaseName =
                String.Format(
                    CultureInfo.InvariantCulture, "{0}_{1}", strippedFileName, Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
            return databaseName;
        }

        // <summary>
        // Get the full mdf file name given the attachDBFile value from the connection string
        // </summary>
        private static string GetMdfFileName(string attachDBFile)
        {
            DebugCheck.NotEmpty(attachDBFile);

            return ExpandDataDirectory(attachDBFile);
        }

        internal static SqlVersion CreateDatabaseFromScript(int? commandTimeout, DbConnection sqlConnection, string createDatabaseScript)
        {
            SqlVersion sqlVersion = 0;
            UsingMasterConnection(
                sqlConnection, conn =>
                    {
                        // create database
                        using (var command = CreateCommand(conn, createDatabaseScript, commandTimeout))
                        {
                            DbInterception.Dispatch.Command.NonQuery(command, new DbCommandInterceptionContext());
                        }
                        sqlVersion = SqlVersionUtils.GetSqlVersion(conn);
                    });

            Debug.Assert(sqlVersion != 0);
            return sqlVersion;
        }

        /// <summary>
        /// Determines whether the database for the given connection exists.
        /// There are three cases:
        /// 1.  Initial Catalog = X, AttachDBFilename = null:   (SELECT Count(*) FROM sys.databases WHERE [name]= X) > 0
        /// 2.  Initial Catalog = X, AttachDBFilename = F:      if (SELECT Count(*) FROM sys.databases WHERE [name]= X) >  true,
        /// if not, try to open the connection and then return (SELECT Count(*) FROM sys.databases WHERE [name]= X) > 0
        /// 3.  Initial Catalog = null, AttachDBFilename = F:   Try to open the connection. If that succeeds the result is true, otherwise
        /// if the there are no databases corresponding to the given file return false, otherwise throw.
        /// Note: We open the connection to cover the scenario when the mdf exists, but is not attached.
        /// Given that opening the connection would auto-attach it, it would not be appropriate to return false in this case.
        /// Also note that checking for the existence of the file does not work for a remote server.  (Dev11 #290487)
        /// For further details on the behavior when AttachDBFilename is specified see Dev10# 188936
        /// </summary>
        /// <param name="connection">Connection to a database whose existence is checked by this method.</param>
        /// <param name="commandTimeout">Execution timeout for any commands needed to determine the existence of the database.</param>
        /// <param name="storeItemCollection">The collection of all store items from the model. This parameter is no longer used for determining database existence.</param>
        /// <returns>True if the provider can deduce the database only based on the connection.</returns>
        protected override bool DbDatabaseExists(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(storeItemCollection, "storeItemCollection");

            var sqlConnection = SqlProviderUtilities.GetRequiredSqlConnection(connection);
            var connectionBuilder = new SqlConnectionStringBuilder(sqlConnection.ConnectionString);

            if (string.IsNullOrEmpty(connectionBuilder.InitialCatalog)
                && string.IsNullOrEmpty(connectionBuilder.AttachDBFilename))
            {
                throw new InvalidOperationException(Strings.SqlProvider_DdlGeneration_MissingInitialCatalog);
            }

            if (!string.IsNullOrEmpty(connectionBuilder.InitialCatalog))
            {
                if (CheckDatabaseExists(sqlConnection, commandTimeout, connectionBuilder.InitialCatalog))
                {
                    //Avoid further processing
                    return true;
                }
            }

            if (!string.IsNullOrEmpty(connectionBuilder.AttachDBFilename))
            {
                try
                {
                    UsingConnection(sqlConnection, con => { });
                    return true;
                }
                catch (SqlException e)
                {
                    if (!string.IsNullOrEmpty(connectionBuilder.InitialCatalog))
                    {
                        return CheckDatabaseExists(sqlConnection, commandTimeout, connectionBuilder.InitialCatalog);
                    }
                    // Initial catalog not specified
                    var fileName = GetMdfFileName(connectionBuilder.AttachDBFilename);
                    var databaseDoesNotExistInSysTables = false;
                    UsingMasterConnection(
                        sqlConnection, conn =>
                            {
                                var sqlVersion = SqlVersionUtils.GetSqlVersion(conn);
                                var databaseExistsScript = SqlDdlBuilder.CreateCountDatabasesBasedOnFileNameScript(
                                    fileName, useDeprecatedSystemTable: sqlVersion == SqlVersion.Sql8);
                                using (var command = CreateCommand(conn, databaseExistsScript, commandTimeout))
                                {
                                    var rowsAffected = (int)DbInterception.Dispatch.Command.Scalar(
                                        command, new DbCommandInterceptionContext());

                                    databaseDoesNotExistInSysTables = (rowsAffected == 0);
                                }
                            });
                    if (databaseDoesNotExistInSysTables)
                    {
                        return false;
                    }
                    throw new InvalidOperationException(Strings.SqlProvider_DdlGeneration_CannotTellIfDatabaseExists, e);
                }
            }

            // CheckDatabaseExists returned false and no AttachDBFilename is specified
            return false;
        }

        private static bool CheckDatabaseExists(SqlConnection sqlConnection, int? commandTimeout, string databaseName)
        {
            var databaseExistsInSysTables = false;
            UsingMasterConnection(
                sqlConnection, conn =>
                    {
                        var sqlVersion = SqlVersionUtils.GetSqlVersion(conn);
                        var databaseExistsScript = SqlDdlBuilder.CreateDatabaseExistsScript(
                            databaseName, useDeprecatedSystemTable: sqlVersion == SqlVersion.Sql8);
                        using (var command = CreateCommand(conn, databaseExistsScript, commandTimeout))
                        {
                            var rowsAffected = (int)DbInterception.Dispatch.Command.Scalar(
                                command, new DbCommandInterceptionContext());

                            databaseExistsInSysTables = (rowsAffected > 0);
                        }
                    });
            return databaseExistsInSysTables;
        }

        /// <summary>
        /// Delete the database for the given connection.
        /// There are three cases:
        /// 1.  If Initial Catalog is specified (X) drop database X
        /// 2.  Else if AttachDBFilename is specified (F) drop all the databases corresponding to F
        /// if none throw
        /// 3.  If niether the catalog not the file name is specified - throw
        /// Note that directly deleting the files does not work for a remote server.  However, even for not attached
        /// databases the current logic would work assuming the user does: if (DatabaseExists) DeleteDatabase
        /// </summary>
        /// <param name="connection"> Connection </param>
        /// <param name="commandTimeout"> Timeout for internal commands. </param>
        /// <param name="storeItemCollection"> Item Collection. </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void DbDeleteDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(storeItemCollection, "storeItemCollection");

            var sqlConnection = SqlProviderUtilities.GetRequiredSqlConnection(connection);

            var connectionBuilder = new SqlConnectionStringBuilder(sqlConnection.ConnectionString);
            var initialCatalog = connectionBuilder.InitialCatalog;
            var attachDBFile = connectionBuilder.AttachDBFilename;

            if (!string.IsNullOrEmpty(initialCatalog))
            {
                DropDatabase(sqlConnection, commandTimeout, initialCatalog);
            }

                // initial catalog not specified
            else if (!string.IsNullOrEmpty(attachDBFile))
            {
                var fullFileName = GetMdfFileName(attachDBFile);

                var databaseNames = new List<string>();
                UsingMasterConnection(
                    sqlConnection, conn =>
                        {
                            var sqlVersion = SqlVersionUtils.GetSqlVersion(conn);
                            var getDatabaseNamesScript =
                                SqlDdlBuilder.CreateGetDatabaseNamesBasedOnFileNameScript(
                                    fullFileName, sqlVersion == SqlVersion.Sql8);
                            var command = CreateCommand(conn, getDatabaseNamesScript, commandTimeout);

                            using (
                                var reader = DbInterception.Dispatch.Command.Reader(
                                    command, new DbCommandInterceptionContext()))
                            {
                                while (reader.Read())
                                {
                                    databaseNames.Add(reader.GetString(0));
                                }
                            }
                        });
                if (databaseNames.Count > 0)
                {
                    foreach (var databaseName in databaseNames)
                    {
                        DropDatabase(sqlConnection, commandTimeout, databaseName);
                    }
                }
                else
                {
                    throw new InvalidOperationException(Strings.SqlProvider_DdlGeneration_CannotDeleteDatabaseNoInitialCatalog);
                }
            }
            // neither initial catalog nor attachDB file name are specified
            else
            {
                throw new InvalidOperationException(Strings.SqlProvider_DdlGeneration_MissingInitialCatalog);
            }
        }

        private static void DropDatabase(SqlConnection sqlConnection, int? commandTimeout, string databaseName)
        {
            // clear the connection pools in case someone's holding on to the database still
            SqlConnection.ClearAllPools();

            var dropDatabaseScript = SqlDdlBuilder.DropDatabaseScript(databaseName);

            try
            {
                UsingMasterConnection(
                    sqlConnection, conn =>
                        {
                            using (var command = CreateCommand(conn, dropDatabaseScript, commandTimeout))
                            {
                                DbInterception.Dispatch.Command.NonQuery(command, new DbCommandInterceptionContext());
                            }
                        });
            }
            catch (SqlException sqlException)
            {
                foreach (SqlError err in sqlException.Errors)
                {
                    // Unable to open the physical file %0.
                    // Operating system error 2: "2(The system cannot find the file specified.)".
                    if (err.Number == 5120)
                    {
                        return;
                    }
                }
                throw;
            }
        }

        private static string CreateObjectsScript(SqlVersion version, StoreItemCollection storeItemCollection)
        {
            return SqlDdlBuilder.CreateObjectsScript(storeItemCollection, createSchemas: version != SqlVersion.Sql8);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller")]
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private static DbCommand CreateCommand(DbConnection sqlConnection, string commandText, int? commandTimeout)
        {
            DebugCheck.NotNull(sqlConnection);
            if (string.IsNullOrEmpty(commandText))
            {
                // SqlCommand will complain if the command text is empty
                commandText = Environment.NewLine;
            }
            var command = sqlConnection.CreateCommand();
            command.CommandText = commandText;
            if (commandTimeout.HasValue)
            {
                command.CommandTimeout = commandTimeout.Value;
            }
            return command;
        }

        private static void UsingConnection(DbConnection sqlConnection, Action<DbConnection> act)
        {
            // remember the connection string so that we can reset if credentials are wiped
            var holdConnectionString = sqlConnection.ConnectionString;
            var openingConnection = sqlConnection.State == ConnectionState.Closed;
            if (openingConnection)
            {
                DbConfiguration.DependencyResolver.GetService<Func<IDbExecutionStrategy>>(
                    new ExecutionStrategyKey(ProviderInvariantName, sqlConnection.DataSource))()
                    .Execute(
                        () =>
                        {
                            // If Open() fails the original credentials need to be restored before retrying
                            if (sqlConnection.State == ConnectionState.Closed
                                && !sqlConnection.ConnectionString.Equals(holdConnectionString, StringComparison.Ordinal))
                            {
                                sqlConnection.ConnectionString = holdConnectionString;
                            }

                            sqlConnection.Open();
                        });
            }
            try
            {
                act(sqlConnection);
            }
            finally
            {
                if (openingConnection && sqlConnection.State == ConnectionState.Open)
                {
                    // if we opened the connection, we should close it
                    sqlConnection.Close();

                    // Can only change the connection string if the connection is closed
                    if (!sqlConnection.ConnectionString.Equals(holdConnectionString, StringComparison.Ordinal))
                    {
                        Debug.Assert(true);
                        sqlConnection.ConnectionString = holdConnectionString;
                    }
                }
            }
        }

        private static void UsingMasterConnection(DbConnection sqlConnection, Action<DbConnection> act)
        {
            var connectionBuilder = new SqlConnectionStringBuilder(sqlConnection.ConnectionString)
                {
                    InitialCatalog = "master",
                    AttachDBFilename = string.Empty, // any AttachDB path specified is not relevant to master
                };

            try
            {
                using (var masterConnection = GetProviderFactory(sqlConnection).CreateConnection())
                {
                    masterConnection.ConnectionString = connectionBuilder.ConnectionString;
                    UsingConnection(masterConnection, act);
                }
            }
            catch (SqlException e)
            {
                // if it appears that the credentials have been removed from the connection string, use an alternate explanation
                if (!connectionBuilder.IntegratedSecurity
                    && (string.IsNullOrEmpty(connectionBuilder.UserID) || string.IsNullOrEmpty(connectionBuilder.Password)))
                {
                    throw new InvalidOperationException(Strings.SqlProvider_CredentialsMissingForMasterConnection, e);
                }
                throw;
            }
        }
    }
}
