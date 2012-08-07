// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact
{
    using System.Data.Entity.Core;
    using System.Data.Entity.SqlServerCompact.Resources;
    using System.Globalization;

    internal static class ADP1
    {
        // The class ADP1 defines the exceptions that are specific to the Adapters.
        // The class contains functions that take the proper informational variables and then construct
        // the appropriate exception with an error string obtained from the resource Framework.txt.
        // The exception is then returned to the caller, so that the caller may then throw from its
        // location so that the catcher of the exception will have the appropriate call stack.
        // This class is used so that there will be compile time checking of error messages.
        // The resource Framework.txt will ensure proper string text based on the appropriate
        // locale.

        //
        // COM+ exceptions
        //
        internal static ArgumentException Argument(string error)
        {
            var e = new ArgumentException(error);
            return e;
        }

        internal static ArgumentNullException ArgumentNull(string parameter)
        {
            var e = new ArgumentNullException(parameter);
            return e;
        }

        internal static InvalidOperationException InvalidOperation(string error)
        {
            var e = new InvalidOperationException(error);
            return e;
        }

        internal static NotImplementedException NotImplemented(string error)
        {
            var e = new NotImplementedException(error);

            return e;
        }

        internal static NotSupportedException NotSupported()
        {
            var e = new NotSupportedException();

            return e;
        }

        internal static NotSupportedException NotSupported(string error)
        {
            var e = new NotSupportedException(error);

            return e;
        }

        #region Metadata Exceptions

        internal static MetadataException Metadata(string message)
        {
            var e = new MetadataException(message);
            return e;
        }

        #endregion

        #region Internal Errors

        /// <summary>
        ///     Internal error code to use with the InternalError exception.
        /// </summary>
        /// <remarks>
        ///     You must never renumber these, because we rely upon them when
        ///     we get an exception report once we release the bits.
        /// </remarks>
        internal enum InternalErrorCode
        {
            /// <summary>
            ///     Thrown when SQL gen produces parameters for anything other than a 
            ///     modification command tree.
            /// </summary>
            SqlGenParametersNotPermitted = 1017,
        }

        internal static Exception InternalError(InternalErrorCode internalError)
        {
            return InvalidOperation(EntityRes.GetString(EntityRes.ADP_InternalProviderError, (int)internalError));
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////
        //
        // Provider specific sqlgen errors
        //
        internal static NotSupportedException FullOuterJoinNotSupportedException()
        {
            return NotSupported(EntityRes.GetString(EntityRes.FullOuterJoinNotSupported));
        }

        internal static NotSupportedException CollateInOrderByNotSupportedException()
        {
            return NotSupported(EntityRes.GetString(EntityRes.CollateInOrderByNotSupported));
        }

        internal static NotSupportedException DMLQueryCannotReturnResultsException()
        {
            return NotSupported(EntityRes.GetString(EntityRes.DMLQueryCannotReturnResults));
        }

        internal static NotSupportedException SkipNotSupportedException()
        {
            return NotSupported(EntityRes.GetString(EntityRes.SkipNotSupportedException));
        }

        internal static NotSupportedException WithTiesNotSupportedException()
        {
            return NotSupported(EntityRes.GetString(EntityRes.WithTiesNotSupportedException));
        }

        ////////////////////////////////////////////////////////////////////////
        //
        // EntityUtil.cs
        //
        internal static UpdateException Update(string message, Exception innerException)
        {
            var e = new UpdateException(message, innerException);
            return e;
        }

        internal static ProviderIncompatibleException ProviderIncompatible(string message)
        {
            var e = new ProviderIncompatibleException(message);
            return e;
        }

        internal static string Update_NotSupportedServerGenKey(object p0)
        {
            return EntityRes.GetString(EntityRes.Update_NotSupportedServerGenKey, new[] { p0 });
        }

        internal static string Update_NotSupportedIdentityType(object p0, object p1)
        {
            return EntityRes.GetString(EntityRes.Update_NotSupportedIdentityType, new[] { p0, p1 });
        }

        ////////////////////////////////////////////////////////////////////////
        //
        // DDl errors
        //
        internal static ArgumentException InvalidConnectionType()
        {
            return Argument(EntityRes.GetString(EntityRes.InvalidConnectionTypeException));
        }

        internal static NotSupportedException ComputedColumnsNotSupportedException()
        {
            return NotSupported(EntityRes.GetString(EntityRes.ComputedColumnsNotSupported));
        }

        internal static NotSupportedException ServerGeneratedGuidKeyNotSupportedException(object p0)
        {
            return NotSupported(EntityRes.GetString(EntityRes.ServerGeneratedGuidKeyNotSupported, new[] { p0 }));
        }

        internal static InvalidOperationException CreateDatabaseNotAllowedWithinTransaction()
        {
            return InvalidOperation(EntityRes.GetString(EntityRes.CreateDatabaseNotAllowedWithinTransaction));
        }

        internal static InvalidOperationException DeleteDatabaseNotAllowedWithinTransaction()
        {
            return InvalidOperation(EntityRes.GetString(EntityRes.DeleteDatabaseNotAllowedWithinTransaction));
        }

        internal static ArgumentException DeleteDatabaseWithOpenConnection()
        {
            return Argument(EntityRes.GetString(EntityRes.DeleteDatabaseWithOpenConnection));
        }

        internal static NotSupportedException ColumnGreaterThanMaxLengthNotSupported(object p0, object p1)
        {
            return NotSupported(EntityRes.GetString(EntityRes.ColumnGreaterThanMaxLengthNotSupported, new[] { p0, p1 }));
        }

        ////////////////////////////////////////////////////////////////////////
        //
        // Helper Functions
        //
        internal static T CheckArgumentNull<T>(T value, string parameterName) where T : class
        {
            if (null == value)
            {
                throw ArgumentNull(parameterName);
            }
            return value;
        }

        // global constant strings
        internal const string Parameter = "Parameter";
        internal const string ParameterName = "ParameterName";

        internal const CompareOptions compareOptions =
            CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase;
    }
}
