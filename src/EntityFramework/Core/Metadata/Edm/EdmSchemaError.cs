// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Resources;
    using System.Globalization;

    /// <summary>
    ///     This class encapsulates the error information for a schema error that was encountered.
    /// </summary>
    [Serializable]
    public sealed class EdmSchemaError : EdmError
    {
        private int _errorCode;
        private EdmSchemaErrorSeverity _severity = EdmSchemaErrorSeverity.Warning;
        private string _schemaLocation;
        private int _line = -1;
        private int _column = -1;
        private string _stackTrace = string.Empty;

        /// <summary>
        ///     Constructs a EdmSchemaError object.
        /// </summary>
        /// <param name="message"> The explanation of the error. </param>
        /// <param name="errorCode"> The code associated with this error. </param>
        /// <param name="severity"> The severity of the error. </param>
        public EdmSchemaError(string message, int errorCode, EdmSchemaErrorSeverity severity)
            :
                this(message, errorCode, severity, null)
        {
        }

        /// <summary>
        ///     Constructs a EdmSchemaError object.
        /// </summary>
        /// <param name="message"> The explanation of the error. </param>
        /// <param name="errorCode"> The code associated with this error. </param>
        /// <param name="severity"> The severity of the error. </param>
        /// <param name="exception"> The exception that caused the error to be filed. </param>
        internal EdmSchemaError(string message, int errorCode, EdmSchemaErrorSeverity severity, Exception exception)
            : base(message)
        {
            Initialize(errorCode, severity, null, -1, -1, exception);
        }

        /// <summary>
        ///     Constructs a EdmSchemaError object.
        /// </summary>
        /// <param name="message"> The explanation of the error. </param>
        /// <param name="errorCode"> The code associated with this error. </param>
        /// <param name="severity"> The severity of the error. </param>
        /// <param name="sourceUri"> </param>
        /// <param name="lineNumber"> </param>
        /// <param name="sourceColumn"> </param>
        internal EdmSchemaError(string message, int errorCode, EdmSchemaErrorSeverity severity, string schemaLocation, int line, int column)
            : this(message, errorCode, severity, schemaLocation, line, column, null)
        {
        }

        /// <summary>
        ///     Constructs a EdmSchemaError object.
        /// </summary>
        /// <param name="message"> The explanation of the error. </param>
        /// <param name="errorCode"> The code associated with this error. </param>
        /// <param name="severity"> The severity of the error. </param>
        /// <param name="sourceUri"> </param>
        /// <param name="lineNumber"> </param>
        /// <param name="sourceColumn"> </param>
        /// <param name="exception"> The exception that caused the error to be filed. </param>
        internal EdmSchemaError(
            string message, int errorCode, EdmSchemaErrorSeverity severity, string schemaLocation, int line, int column, Exception exception)
            : base(message)
        {
            if (severity < EdmSchemaErrorSeverity.Warning
                || severity > EdmSchemaErrorSeverity.Error)
            {
                throw new ArgumentOutOfRangeException("severity", severity, Strings.ArgumentOutOfRange(severity));
            }

            Initialize(errorCode, severity, schemaLocation, line, column, exception);
        }

        private void Initialize(
            int errorCode, EdmSchemaErrorSeverity severity, string schemaLocation, int line, int column, Exception exception)
        {
            if (errorCode < 0)
            {
                throw new ArgumentOutOfRangeException("errorCode", errorCode, Strings.ArgumentOutOfRangeExpectedPostiveNumber(errorCode));
            }

            _errorCode = errorCode;
            _severity = severity;
            _schemaLocation = schemaLocation;
            _line = line;
            _column = column;
            if (exception != null)
            {
                _stackTrace = exception.StackTrace;
            }
        }

        /// <summary>Returns the error message.</summary>
        /// <returns>The error message.</returns>
        public override string ToString()
        {
            string text;
            string severity;

            switch (Severity)
            {
                case EdmSchemaErrorSeverity.Error:
                    severity = Strings.GeneratorErrorSeverityError;
                    break;
                case EdmSchemaErrorSeverity.Warning:
                    severity = Strings.GeneratorErrorSeverityWarning;
                    break;
                default:
                    severity = Strings.GeneratorErrorSeverityUnknown;
                    break;
            }

            if (String.IsNullOrEmpty(SchemaName)
                && Line < 0
                && Column < 0)
            {
                text = String.Format(
                    CultureInfo.CurrentCulture, "{0} {1:0000}: {2}",
                    severity,
                    ErrorCode,
                    Message);
            }
            else
            {
                text = String.Format(
                    CultureInfo.CurrentCulture, "{0}({1},{2}) : {3} {4:0000}: {5}",
                    (SchemaName == null) ? Strings.SourceUriUnknown : SchemaName,
                    Line,
                    Column,
                    severity,
                    ErrorCode,
                    Message);
            }

            return text;
        }

        /// <summary>Gets the error code.</summary>
        /// <returns>The error code.</returns>
        public int ErrorCode
        {
            get { return _errorCode; }
        }

        /// <summary>Gets the severity level of the error.</summary>
        /// <returns>
        ///     One of the <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmSchemaErrorSeverity" /> values. The default is
        ///     <see
        ///         cref="F:System.Data.Entity.Core.Metadata.Edm.EdmSchemaErrorSeverity.Warning" />
        ///     .
        /// </returns>
        public EdmSchemaErrorSeverity Severity
        {
            get { return _severity; }
            set { _severity = value; }
        }

        /// <summary>Gets the line number where the error occurred.</summary>
        /// <returns>The line number where the error occurred.</returns>
        public int Line
        {
            get { return _line; }
        }

        /// <summary>Gets the column where the error occurred.</summary>
        /// <returns>The column where the error occurred.</returns>
        public int Column
        {
            get { return _column; }
        }

        /// <summary>Gets the location of the schema that contains the error. This string also includes the name of the schema at the end.</summary>
        /// <returns>The location of the schema that contains the error.</returns>
        public string SchemaLocation
        {
            get { return _schemaLocation; }
        }

        /// <summary>Gets the name of the schema that contains the error.</summary>
        /// <returns>The name of the schema that contains the error.</returns>
        public string SchemaName
        {
            get { return GetNameFromSchemaLocation(SchemaLocation); }
        }

        /// <summary>Gets a string representation of the stack trace at the time the error occurred.</summary>
        /// <returns>A string representation of the stack trace at the time the error occurred.</returns>
        public string StackTrace
        {
            get { return _stackTrace; }
        }

        private static string GetNameFromSchemaLocation(string schemaLocation)
        {
            if (string.IsNullOrEmpty(schemaLocation))
            {
                return schemaLocation;
            }

            var pos = Math.Max(schemaLocation.LastIndexOf('/'), schemaLocation.LastIndexOf('\\'));
            var start = pos + 1;
            if (pos < 0)
            {
                return schemaLocation;
            }
            else if (start >= schemaLocation.Length)
            {
                return string.Empty;
            }

            return schemaLocation.Substring(start);
        }
    }
}
