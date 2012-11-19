// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact.Resources
{
    using System.CodeDom.Compiler;
    using System.Data.Entity.SqlServerCompact.Utilities;
    using System.Globalization;
    using System.Resources;
    using System.Threading;

    /// <summary>
    ///     Strongly-typed and parameterized string resources.
    /// </summary>
    [GeneratedCode("Resources.SqlServerCompact.tt", "1.0.0.0")]
    internal static class Strings
    {
        /// <summary>
        ///     A string like "The argument '{0}' cannot be null, empty or contain only white space."
        /// </summary>
        internal static string ArgumentIsNullOrWhitespace(object p0)
        {
            return EntityRes.GetString(EntityRes.ArgumentIsNullOrWhitespace, p0);
        }

        /// <summary>
        ///     A string like "The precondition '{0}' failed. {1}"
        /// </summary>
        internal static string PreconditionFailed(object p0, object p1)
        {
            return EntityRes.GetString(EntityRes.PreconditionFailed, p0, p1);
        }

        /// <summary>
        ///     A string like "Records were updated, but the values were not retrieved back. See internal error for more details."
        /// </summary>
        internal static string ADP_CanNotRetrieveServerGeneratedKey
        {
            get { return EntityRes.GetString(EntityRes.ADP_CanNotRetrieveServerGeneratedKey); }
        }

        /// <summary>
        ///     A string like "SqlCeCommand.CommandTimeout does not support non-zero values."
        /// </summary>
        internal static string ADP_InvalidCommandTimeOut
        {
            get { return EntityRes.GetString(EntityRes.ADP_InvalidCommandTimeOut); }
        }

        /// <summary>
        ///     A string like "The CommandType enumeration value, {0}, is invalid."
        /// </summary>
        internal static string ADP_InvalidCommandType(object p0)
        {
            return EntityRes.GetString(EntityRes.ADP_InvalidCommandType, p0);
        }

        /// <summary>
        ///     A string like "Parameter '{0}' is not valid. String arguments cannot be empty."
        /// </summary>
        internal static string InvalidStringArgument(object p0)
        {
            return EntityRes.GetString(EntityRes.InvalidStringArgument, p0);
        }

        /// <summary>
        ///     A string like "The specified database doesn't exists."
        /// </summary>
        internal static string DatabaseDoesNotExist
        {
            get { return EntityRes.GetString(EntityRes.DatabaseDoesNotExist); }
        }

        /// <summary>
        ///     A string like "The connection given is not of type '{0}'."
        /// </summary>
        internal static string Mapping_Provider_WrongConnectionType(object p0)
        {
            return EntityRes.GetString(EntityRes.Mapping_Provider_WrongConnectionType, p0);
        }

        /// <summary>
        ///     A string like "Unable to update the EntitySet '{0}' because it has a DefiningQuery and no <{1}> element exists in the <{2}> element to support the current operation."
        /// </summary>
        internal static string Update_SqlEntitySetWithoutDmlFunctions(object p0, object p1, object p2)
        {
            return EntityRes.GetString(EntityRes.Update_SqlEntitySetWithoutDmlFunctions, p0, p1, p2);
        }

        /// <summary>
        ///     A string like "The provider manifest given is not of type '{0}'."
        /// </summary>
        internal static string Mapping_Provider_WrongManifestType(object p0)
        {
            return EntityRes.GetString(EntityRes.Mapping_Provider_WrongManifestType, p0);
        }

        /// <summary>
        ///     A string like "The provider returned null for the informationType '{0}'."
        /// </summary>
        internal static string ProviderReturnedNullForGetDbInformation(object p0)
        {
            return EntityRes.GetString(EntityRes.ProviderReturnedNullForGetDbInformation, p0);
        }

        /// <summary>
        ///     A string like "Could not determine store version; a valid store connection or a version hint is required."
        /// </summary>
        internal static string UnableToDetermineStoreVersion
        {
            get { return EntityRes.GetString(EntityRes.UnableToDetermineStoreVersion); }
        }

        /// <summary>
        ///     A string like "DATEPART argument to function '{0}.{1}' must be a literal string"
        /// </summary>
        internal static string InvalidDatePartArgumentExpression(object p0, object p1)
        {
            return EntityRes.GetString(EntityRes.InvalidDatePartArgumentExpression, p0, p1);
        }

        /// <summary>
        ///     A string like "'{0}' is not a valid value for DATEPART argument in '{1}.{2}' function"
        /// </summary>
        internal static string InvalidDatePartArgumentValue(object p0, object p1, object p2)
        {
            return EntityRes.GetString(EntityRes.InvalidDatePartArgumentValue, p0, p1, p2);
        }

        /// <summary>
        ///     A string like "Functions attributed as NiladicFunction='true' in the provider manifest cannot have parameter declarations"
        /// </summary>
        internal static string NiladicFunctionsCannotHaveParameters
        {
            get { return EntityRes.GetString(EntityRes.NiladicFunctionsCannotHaveParameters); }
        }

        /// <summary>
        ///     A string like "'{0}' is an unknown expression type."
        /// </summary>
        internal static string UnknownExpressionType(object p0)
        {
            return EntityRes.GetString(EntityRes.UnknownExpressionType, p0);
        }

        /// <summary>
        ///     A string like "FULL OUTER JOIN is not supported by SQL Server Compact."
        /// </summary>
        internal static string FullOuterJoinNotSupported
        {
            get { return EntityRes.GetString(EntityRes.FullOuterJoinNotSupported); }
        }

        /// <summary>
        ///     A string like "COLLATE subclause in the ORDER BY clause is not supported by SQL Server Compact."
        /// </summary>
        internal static string CollateInOrderByNotSupported
        {
            get { return EntityRes.GetString(EntityRes.CollateInOrderByNotSupported); }
        }

        /// <summary>
        ///     A string like "Server-generated keys and server-generated values are not supported by SQL Server Compact."
        /// </summary>
        internal static string DMLQueryCannotReturnResults
        {
            get { return EntityRes.GetString(EntityRes.DMLQueryCannotReturnResults); }
        }

        /// <summary>
        ///     A string like "SKIP clause is not supported by SQL Server Compact."
        /// </summary>
        internal static string SkipNotSupportedException
        {
            get { return EntityRes.GetString(EntityRes.SkipNotSupportedException); }
        }

        /// <summary>
        ///     A string like "WITH TIES subclause is not supported by SQL Server Compact."
        /// </summary>
        internal static string WithTiesNotSupportedException
        {
            get { return EntityRes.GetString(EntityRes.WithTiesNotSupportedException); }
        }

        /// <summary>
        ///     A string like "User defined functions are not supported by SQL Server Compact."
        /// </summary>
        internal static string UserDefinedFunctionsNotSupported
        {
            get { return EntityRes.GetString(EntityRes.UserDefinedFunctionsNotSupported); }
        }

        /// <summary>
        ///     A string like "Table valued functions are not supported by SQL Server Compact."
        /// </summary>
        internal static string TVFsNotSupported
        {
            get { return EntityRes.GetString(EntityRes.TVFsNotSupported); }
        }

        /// <summary>
        ///     A string like "DISTINCT attribute is not supported in aggregate functions by SQL Server Compact."
        /// </summary>
        internal static string DistinctAggregatesNotSupported
        {
            get { return EntityRes.GetString(EntityRes.DistinctAggregatesNotSupported); }
        }

        /// <summary>
        ///     A string like "The type '{0}' is not supported by SQL Server Compact."
        /// </summary>
        internal static string ProviderDoesNotSupportType(object p0)
        {
            return EntityRes.GetString(EntityRes.ProviderDoesNotSupportType, p0);
        }

        /// <summary>
        ///     A string like "There is no store type corresponding to the EDM type '{0}' of primitive type '{1}'."
        /// </summary>
        internal static string NoStoreTypeForEdmType(object p0, object p1)
        {
            return EntityRes.GetString(EntityRes.NoStoreTypeForEdmType, p0, p1);
        }

        /// <summary>
        ///     A string like "Stored procedures are not supported by SQL Server Compact."
        /// </summary>
        internal static string StoredProceduresNotSupported
        {
            get { return EntityRes.GetString(EntityRes.StoredProceduresNotSupported); }
        }

        /// <summary>
        ///     A string like "The function '{0}' is not supported by SQL Server Compact."
        /// </summary>
        internal static string FunctionNotSupported(object p0)
        {
            return EntityRes.GetString(EntityRes.FunctionNotSupported, p0);
        }

        /// <summary>
        ///     A string like "Failed when preparing table {0} for write as write-consistency cannot be guaranteed. Try adding a writable column to the table."
        /// </summary>
        internal static string UpdateStatementCannotBeGeneratedForAcquiringLock(object p0)
        {
            return EntityRes.GetString(EntityRes.UpdateStatementCannotBeGeneratedForAcquiringLock, p0);
        }

        /// <summary>
        ///     A string like "Server-generated keys are only supported for identity columns. More than one column is marked as server generated in table '{0}'."
        /// </summary>
        internal static string Update_NotSupportedServerGenKey(object p0)
        {
            return EntityRes.GetString(EntityRes.Update_NotSupportedServerGenKey, p0);
        }

        /// <summary>
        ///     A string like "Server-generated keys are only supported for identity columns. The column '{0}' has type '{1}', which is not a valid type for an identity column."
        /// </summary>
        internal static string Update_NotSupportedIdentityType(object p0, object p1)
        {
            return EntityRes.GetString(EntityRes.Update_NotSupportedIdentityType, p0, p1);
        }

        /// <summary>
        ///     A string like "Internal .NET Framework Data Provider error {0}."
        /// </summary>
        internal static string ADP_InternalProviderError(object p0)
        {
            return EntityRes.GetString(EntityRes.ADP_InternalProviderError, p0);
        }

        /// <summary>
        ///     A string like "The {0} enumeration value, {1}, is not valid."
        /// </summary>
        internal static string ADP_InvalidEnumerationValue(object p0, object p1)
        {
            return EntityRes.GetString(EntityRes.ADP_InvalidEnumerationValue, p0, p1);
        }

        /// <summary>
        ///     A string like "The {0} enumeration value, {1}, is not supported by the {2} method."
        /// </summary>
        internal static string ADP_NotSupportedEnumerationValue(object p0, object p1, object p2)
        {
            return EntityRes.GetString(EntityRes.ADP_NotSupportedEnumerationValue, p0, p1, p2);
        }

        /// <summary>
        ///     A string like "Format of the initialization string does not conform to specification starting at index {0}."
        /// </summary>
        internal static string ADP_ConnectionStringSyntax(object p0)
        {
            return EntityRes.GetString(EntityRes.ADP_ConnectionStringSyntax, p0);
        }

        /// <summary>
        ///     A string like "Value is not valid for key '{0}'."
        /// </summary>
        internal static string ADP_InvalidConnectionOptionValue(object p0)
        {
            return EntityRes.GetString(EntityRes.ADP_InvalidConnectionOptionValue, p0);
        }

        /// <summary>
        ///     A string like "The parameter specified for the connection is not supported and is not of the SqlCeConnection type."
        /// </summary>
        internal static string InvalidConnectionTypeException
        {
            get { return EntityRes.GetString(EntityRes.InvalidConnectionTypeException); }
        }

        /// <summary>
        ///     A string like "The store generated pattern 'Computed' is supported for properties that are of type 'timestamp'  or 'rowversion' only."
        /// </summary>
        internal static string ComputedColumnsNotSupported
        {
            get { return EntityRes.GetString(EntityRes.ComputedColumnsNotSupported); }
        }

        /// <summary>
        ///     A string like "The database creation succeeded, but the creation of the database objects did not. See inner exception for more details."
        /// </summary>
        internal static string IncompleteDatabaseCreation
        {
            get { return EntityRes.GetString(EntityRes.IncompleteDatabaseCreation); }
        }

        /// <summary>
        ///     A string like "Server generated GUID column "{0}" cannot be part of the key."
        /// </summary>
        internal static string ServerGeneratedGuidKeyNotSupported(object p0)
        {
            return EntityRes.GetString(EntityRes.ServerGeneratedGuidKeyNotSupported, p0);
        }

        /// <summary>
        ///     A string like "Database file cannot be deleted within a transaction scope."
        /// </summary>
        internal static string DeleteDatabaseNotAllowedWithinTransaction
        {
            get { return EntityRes.GetString(EntityRes.DeleteDatabaseNotAllowedWithinTransaction); }
        }

        /// <summary>
        ///     A string like "Database file cannot be created within a transaction scope."
        /// </summary>
        internal static string CreateDatabaseNotAllowedWithinTransaction
        {
            get { return EntityRes.GetString(EntityRes.CreateDatabaseNotAllowedWithinTransaction); }
        }

        /// <summary>
        ///     A string like "Database file cannot be deleted. Close all open connections before calling DeleteDatabase()."
        /// </summary>
        internal static string DeleteDatabaseWithOpenConnection
        {
            get { return EntityRes.GetString(EntityRes.DeleteDatabaseWithOpenConnection); }
        }

        /// <summary>
        ///     A string like "{0} column with MaxLength greater than {1} is not supported."
        /// </summary>
        internal static string ColumnGreaterThanMaxLengthNotSupported(object p0, object p1)
        {
            return EntityRes.GetString(EntityRes.ColumnGreaterThanMaxLengthNotSupported, p0, p1);
        }
    }

    /// <summary>
    ///     Strongly-typed and parameterized exception factory.
    /// </summary>
    [GeneratedCode("Resources.SqlServerCompact.tt", "1.0.0.0")]
    internal static class Error
    {
        /// <summary>
        ///     ArgumentException with message like "The argument '{0}' cannot be null, empty or contain only white space."
        /// </summary>
        internal static Exception ArgumentIsNullOrWhitespace(object p0)
        {
            return new ArgumentException(Strings.ArgumentIsNullOrWhitespace(p0));
        }

        /// <summary>
        ///     ArgumentException with message like "The precondition '{0}' failed. {1}"
        /// </summary>
        internal static Exception PreconditionFailed(object p0, object p1)
        {
            return new ArgumentException(Strings.PreconditionFailed(p0, p1));
        }

        /// <summary>
        ///     The exception that is thrown when a null reference (Nothing in Visual Basic) is passed to a method that does not accept it as a valid argument.
        /// </summary>
        internal static Exception ArgumentNull(string paramName)
        {
            return new ArgumentNullException(paramName);
        }

        /// <summary>
        ///     The exception that is thrown when the value of an argument is outside the allowable range of values as defined by the invoked method.
        /// </summary>
        internal static Exception ArgumentOutOfRange(string paramName)
        {
            return new ArgumentOutOfRangeException(paramName);
        }

        /// <summary>
        ///     The exception that is thrown when the author has yet to implement the logic at this point in the program. This can act as an exception based TODO tag.
        /// </summary>
        internal static Exception NotImplemented()
        {
            return new NotImplementedException();
        }

        /// <summary>
        ///     The exception that is thrown when an invoked method is not supported, or when there is an attempt to read, seek, or write to a stream that does not support the invoked functionality.
        /// </summary>
        internal static Exception NotSupported()
        {
            return new NotSupportedException();
        }
    }

    /// <summary>
    ///     AutoGenerated resource class. Usage:
    ///     string s = EntityRes.GetString(EntityRes.MyIdenfitier);
    /// </summary>
    [GeneratedCode("Resources.SqlServerCompact.tt", "1.0.0.0")]
    internal sealed class EntityRes
    {
        internal const string ArgumentIsNullOrWhitespace = "ArgumentIsNullOrWhitespace";
        internal const string PreconditionFailed = "PreconditionFailed";
        internal const string ADP_CanNotRetrieveServerGeneratedKey = "ADP_CanNotRetrieveServerGeneratedKey";
        internal const string ADP_InvalidCommandTimeOut = "ADP_InvalidCommandTimeOut";
        internal const string ADP_InvalidCommandType = "ADP_InvalidCommandType";
        internal const string InvalidStringArgument = "InvalidStringArgument";
        internal const string DatabaseDoesNotExist = "DatabaseDoesNotExist";
        internal const string Mapping_Provider_WrongConnectionType = "Mapping_Provider_WrongConnectionType";
        internal const string Update_SqlEntitySetWithoutDmlFunctions = "Update_SqlEntitySetWithoutDmlFunctions";
        internal const string Mapping_Provider_WrongManifestType = "Mapping_Provider_WrongManifestType";
        internal const string ProviderReturnedNullForGetDbInformation = "ProviderReturnedNullForGetDbInformation";
        internal const string UnableToDetermineStoreVersion = "UnableToDetermineStoreVersion";
        internal const string InvalidDatePartArgumentExpression = "InvalidDatePartArgumentExpression";
        internal const string InvalidDatePartArgumentValue = "InvalidDatePartArgumentValue";
        internal const string NiladicFunctionsCannotHaveParameters = "NiladicFunctionsCannotHaveParameters";
        internal const string UnknownExpressionType = "UnknownExpressionType";
        internal const string FullOuterJoinNotSupported = "FullOuterJoinNotSupported";
        internal const string CollateInOrderByNotSupported = "CollateInOrderByNotSupported";
        internal const string DMLQueryCannotReturnResults = "DMLQueryCannotReturnResults";
        internal const string SkipNotSupportedException = "SkipNotSupportedException";
        internal const string WithTiesNotSupportedException = "WithTiesNotSupportedException";
        internal const string UserDefinedFunctionsNotSupported = "UserDefinedFunctionsNotSupported";
        internal const string TVFsNotSupported = "TVFsNotSupported";
        internal const string DistinctAggregatesNotSupported = "DistinctAggregatesNotSupported";
        internal const string ProviderDoesNotSupportType = "ProviderDoesNotSupportType";
        internal const string NoStoreTypeForEdmType = "NoStoreTypeForEdmType";
        internal const string StoredProceduresNotSupported = "StoredProceduresNotSupported";
        internal const string FunctionNotSupported = "FunctionNotSupported";
        internal const string UpdateStatementCannotBeGeneratedForAcquiringLock = "UpdateStatementCannotBeGeneratedForAcquiringLock";
        internal const string Update_NotSupportedServerGenKey = "Update_NotSupportedServerGenKey";
        internal const string Update_NotSupportedIdentityType = "Update_NotSupportedIdentityType";
        internal const string ADP_InternalProviderError = "ADP_InternalProviderError";
        internal const string ADP_InvalidEnumerationValue = "ADP_InvalidEnumerationValue";
        internal const string ADP_NotSupportedEnumerationValue = "ADP_NotSupportedEnumerationValue";
        internal const string ADP_ConnectionStringSyntax = "ADP_ConnectionStringSyntax";
        internal const string ADP_InvalidConnectionOptionValue = "ADP_InvalidConnectionOptionValue";
        internal const string InvalidConnectionTypeException = "InvalidConnectionTypeException";
        internal const string ComputedColumnsNotSupported = "ComputedColumnsNotSupported";
        internal const string IncompleteDatabaseCreation = "IncompleteDatabaseCreation";
        internal const string ServerGeneratedGuidKeyNotSupported = "ServerGeneratedGuidKeyNotSupported";
        internal const string DeleteDatabaseNotAllowedWithinTransaction = "DeleteDatabaseNotAllowedWithinTransaction";
        internal const string CreateDatabaseNotAllowedWithinTransaction = "CreateDatabaseNotAllowedWithinTransaction";
        internal const string DeleteDatabaseWithOpenConnection = "DeleteDatabaseWithOpenConnection";
        internal const string ColumnGreaterThanMaxLengthNotSupported = "ColumnGreaterThanMaxLengthNotSupported";

        private static EntityRes loader;
        private readonly ResourceManager resources;

        private EntityRes()
        {
            resources = new ResourceManager(
                "System.Data.Entity.SqlServer.Properties.Resources.SqlServerCompact", typeof(Check).Assembly);
        }

        private static EntityRes GetLoader()
        {
            if (loader == null)
            {
                var sr = new EntityRes();
                Interlocked.CompareExchange(ref loader, sr, null);
            }
            return loader;
        }

        private static CultureInfo Culture
        {
            get { return null /*use ResourceManager default, CultureInfo.CurrentUICulture*/; }
        }

        public static ResourceManager Resources
        {
            get { return GetLoader().resources; }
        }

        public static string GetString(string name, params object[] args)
        {
            var sys = GetLoader();
            if (sys == null)
            {
                return null;
            }
            var res = sys.resources.GetString(name, Culture);

            if (args != null
                && args.Length > 0)
            {
                for (var i = 0; i < args.Length; i ++)
                {
                    var value = args[i] as String;
                    if (value != null
                        && value.Length > 1024)
                    {
                        args[i] = value.Substring(0, 1024 - 3) + "...";
                    }
                }
                return String.Format(CultureInfo.CurrentCulture, res, args);
            }
            else
            {
                return res;
            }
        }

        public static string GetString(string name)
        {
            var sys = GetLoader();
            if (sys == null)
            {
                return null;
            }
            return sys.resources.GetString(name, Culture);
        }

        public static string GetString(string name, out bool usedFallback)
        {
            // always false for this version of gensr
            usedFallback = false;
            return GetString(name);
        }

        public static object GetObject(string name)
        {
            var sys = GetLoader();
            if (sys == null)
            {
                return null;
            }
            return sys.resources.GetObject(name, Culture);
        }
    }
}
