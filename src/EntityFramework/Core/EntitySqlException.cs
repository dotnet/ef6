// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core
{
    using System.Data.Entity.Core.Common.EntitySql;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    /// <summary>
    /// Represents an eSQL Query compilation exception;
    /// The class of exceptional conditions that may cause this exception to be raised are mainly:
    /// 1) Syntax Errors: raised during query text parsing and when a given query does not conform to eSQL formal grammar;
    /// 2) Semantic Errors: raised when semantic rules of eSQL language are not met such as metadata or schema information
    ///    not accurate or not present, type validation errors, scoping rule violations, user of undefined variables, etc.
    /// For more information, see eSQL Language Spec.
    /// </summary>
    [Serializable]
    public sealed class EntitySqlException : EntityException
    {
        #region Private Fields

        private const int HResultInvalidQuery = -2146232006;

        /// <summary>
        /// error message description. 
        /// </summary>
        private readonly string _errorDescription;

        /// <summary>
        /// information about the context where the error occurred 
        /// </summary>
        private readonly string _errorContext;

        /// <summary>
        /// error line number
        /// </summary>
        private readonly int _line;

        /// <summary>
        /// error column number
        /// </summary>
        private readonly int _column;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="EntitySqlException"/> with the generic error message.
        /// </summary>
        public EntitySqlException()
            : this(Strings.GeneralQueryError)
        {
            HResult = HResultInvalidQuery;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="EntitySqlException"/> with the given message.
        /// </summary>
        public EntitySqlException(string message)
            : base(message)
        {
            HResult = HResultInvalidQuery;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="EntitySqlException"/> with the given message and innerException instance.
        /// </summary>
        public EntitySqlException(string message, Exception innerException)
            : base(message, innerException)
        {
            HResult = HResultInvalidQuery;
        }

        /// <summary>
        /// Initializes a new instance <see cref="EntitySqlException"/> with the given serializationInfo and streamingContext.
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        private EntitySqlException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            HResult = HResultInvalidQuery;
            _errorDescription = serializationInfo.GetString("ErrorDescription");
            _errorContext = serializationInfo.GetString("ErrorContext");
            _line = serializationInfo.GetInt32("Line");
            _column = serializationInfo.GetInt32("Column");
        }

        #endregion

        #region Internal Constructors

        /// <summary>
        /// Initializes a new instance EntityException with an ErrorContext instance and a given error message.
        /// </summary>
        internal static EntitySqlException Create(ErrorContext errCtx, string errorMessage, Exception innerException)
        {
            return Create(
                errCtx.CommandText, errorMessage, errCtx.InputPosition, errCtx.ErrorContextInfo, errCtx.UseContextInfoAsResourceIdentifier,
                innerException);
        }

        /// <summary>
        /// Initializes a new instance EntityException with contextual information to allow detailed error feedback.
        /// </summary>
        internal static EntitySqlException Create(
            string commandText,
            string errorDescription,
            int errorPosition,
            string errorContextInfo,
            bool loadErrorContextInfoFromResource,
            Exception innerException)
        {
            int line;
            int column;
            var errorContext = FormatErrorContext(
                commandText, errorPosition, errorContextInfo, loadErrorContextInfoFromResource, out line, out column);

            var errorMessage = FormatQueryError(errorDescription, errorContext);

            return new EntitySqlException(errorMessage, errorDescription, errorContext, line, column, innerException);
        }

        /// <summary>
        /// core constructor
        /// </summary>
        private EntitySqlException(
            string message, string errorDescription, string errorContext, int line, int column, Exception innerException)
            : base(message, innerException)
        {
            _errorDescription = errorDescription;
            _errorContext = errorContext;
            _line = line;
            _column = column;

            HResult = HResultInvalidQuery;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the error description explaining the reason why the query was not accepted or an empty String.Empty
        /// </summary>
        public string ErrorDescription
        {
            get { return _errorDescription ?? String.Empty; }
        }

        /// <summary>
        /// Gets the aproximate context where the error occurred if available.
        /// </summary>
        public string ErrorContext
        {
            get { return _errorContext ?? String.Empty; }
        }

        /// <summary>
        /// Returns the the aproximate line number where the error occurred
        /// </summary>
        public int Line
        {
            get { return _line; }
        }

        /// <summary>
        /// Returns the the aproximate column number where the error occurred
        /// </summary>
        public int Column
        {
            get { return _column; }
        }

        #endregion

        #region Helpers

        internal static string GetGenericErrorMessage(string commandText, int position)
        {
            var lineNumber = 0;
            var colNumber = 0;
            return FormatErrorContext(commandText, position, EntityRes.GenericSyntaxError, true, out lineNumber, out colNumber);
        }

        /// <summary>
        /// Returns error context in the format [[errorContextInfo, ]line ddd, column ddd].
        /// Returns empty string if errorPosition is less than 0 and errorContextInfo is not specified.
        /// </summary>
        internal static string FormatErrorContext(
            string commandText,
            int errorPosition,
            string errorContextInfo,
            bool loadErrorContextInfoFromResource,
            out int lineNumber,
            out int columnNumber)
        {
            Debug.Assert(errorPosition > -1, "position in input stream cannot be < 0");
            Debug.Assert(errorPosition <= commandText.Length, "position in input stream cannot be greater than query text size");

            if (loadErrorContextInfoFromResource)
            {
                errorContextInfo = !String.IsNullOrEmpty(errorContextInfo) ? EntityRes.GetString(errorContextInfo) : String.Empty;
            }

            //
            // Replace control chars and newLines for single representation characters
            //
            var sb = new StringBuilder(commandText.Length);
            for (var i = 0; i < commandText.Length; i++)
            {
                var c = commandText[i];
                if (CqlLexer.IsNewLine(c))
                {
                    c = '\n';
                }
                else if ((Char.IsControl(c) || Char.IsWhiteSpace(c))
                         && ('\r' != c))
                {
                    c = ' ';
                }
                sb.Append(c);
            }
            commandText = sb.ToString().TrimEnd(new[] { '\n' });

            //
            // Compute line and column
            //
            var queryLines = commandText.Split(new[] { '\n' }, StringSplitOptions.None);
            for (lineNumber = 0, columnNumber = errorPosition;
                 lineNumber < queryLines.Length && columnNumber > queryLines[lineNumber].Length;
                 columnNumber -= (queryLines[lineNumber].Length + 1), ++lineNumber)
            {
                ;
            }

            ++lineNumber; // switch lineNum and colNum to 1-based indexes
            ++columnNumber;

            //
            // Error context format: "[errorContextInfo,] line ddd, column ddd"
            //
            sb = new StringBuilder();
            if (!String.IsNullOrEmpty(errorContextInfo))
            {
                sb.AppendFormat(CultureInfo.CurrentCulture, "{0}, ", errorContextInfo);
            }

            if (errorPosition >= 0)
            {
                sb.AppendFormat(
                    CultureInfo.CurrentCulture,
                    "{0} {1}, {2} {3}",
                    Strings.LocalizedLine,
                    lineNumber,
                    Strings.LocalizedColumn,
                    columnNumber);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns error message in the format: "error such and such[, near errorContext]."
        /// </summary>
        private static string FormatQueryError(string errorMessage, string errorContext)
        {
            //
            // Message format: error such and such[, near errorContextInfo].
            //
            var sb = new StringBuilder();
            sb.Append(errorMessage);
            if (!String.IsNullOrEmpty(errorContext))
            {
                sb.AppendFormat(CultureInfo.CurrentCulture, " {0} {1}", Strings.LocalizedNear, errorContext);
            }

            return sb.Append(".").ToString();
        }

        #endregion

        #region ISerializable implementation

        /// <summary>
        /// sets the System.Runtime.Serialization.SerializationInfo
        /// with information about the exception.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized
        /// object data about the exception being thrown.
        /// </param>
        /// <param name="context"></param>
        [SecurityCritical]
        [PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ErrorDescription", _errorDescription);
            info.AddValue("ErrorContext", _errorContext);
            info.AddValue("Line", _line);
            info.AddValue("Column", _column);
        }

        #endregion
    }
}
