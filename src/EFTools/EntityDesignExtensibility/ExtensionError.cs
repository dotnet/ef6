// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    using System;

    /// <summary>
    ///     An enumeration that describes the severity of an
    ///     <see
    ///         cref="T:Microsoft.Data.Entity.Design.Extensibility.ExtensionError" />
    ///     .
    /// </summary>
    public enum ExtensionErrorSeverity
    {
        /// <summary>
        ///     Indicates that the severity of the <see cref="T:Microsoft.Data.Entity.Design.Extensibility.ExtensionError" /> is Warning. An
        ///     <see
        ///         cref="T:Microsoft.Data.Entity.Design.Extensibility.ExtensionError" />
        ///     with this severity will appear in the Visual Studio Error List as a warning.
        /// </summary>
        Warning = 0,

        /// <summary>
        ///     Indicates that the severity of the <see cref="T:Microsoft.Data.Entity.Design.Extensibility.ExtensionError" /> is Error. An
        ///     <see
        ///         cref="T:Microsoft.Data.Entity.Design.Extensibility.ExtensionError" />
        ///     with this severity will appear in the Visual Studio Error List as an error.
        /// </summary>
        Error = 1,

        /// <summary>
        ///     Indicates that the severity of the <see cref="T:Microsoft.Data.Entity.Design.Extensibility.ExtensionError" /> is Message. An
        ///     <see
        ///         cref="T:Microsoft.Data.Entity.Design.Extensibility.ExtensionError" />
        ///     with this severity will appear in the Visual Studio Error List as a message.
        /// </summary>
        Message = 2,
    }

    /// <summary>Encapsulates custom error information for Visual Studio extensions that extend the functionality of the Entity Data Model Designer.</summary>
    [Serializable]
    public sealed class ExtensionError
    {
        private readonly string _message;
        private int _errorCode;
        private ExtensionErrorSeverity _severity = ExtensionErrorSeverity.Warning;
        private int _line = -1;
        private int _column = -1;

        /// <summary>
        ///     Instantiates a new instance of <see cref="T:Microsoft.Data.Entity.Design.Extensibility.ExtensionError" />.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="errorCode">The error code associated with the error.</param>
        /// <param name="severity">The severity of the error.</param>
        public ExtensionError(string message, int errorCode, ExtensionErrorSeverity severity)
        {
            _message = message;

            Initialize(errorCode, severity, -1, -1);
        }

        /// <summary>
        ///     Instantiates a new instance of <see cref="T:Microsoft.Data.Entity.Design.Extensibility.ExtensionError" />.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="errorCode">The error code associated with the error.</param>
        /// <param name="severity">The severity of the error.</param>
        /// <param name="line">The line of the input or output document in which the error occurred.</param>
        /// <param name="column">The column of the input or output document in which the error occurred.</param>
        public ExtensionError(string message, int errorCode, ExtensionErrorSeverity severity, int line, int column)
        {
            _message = message;

            if (line < 0)
            {
                throw new ArgumentOutOfRangeException("line");
            }

            if (column < 0)
            {
                throw new ArgumentOutOfRangeException("column");
            }

            Initialize(errorCode, severity, line, column);
        }

        private void Initialize(int errorCode, ExtensionErrorSeverity severity, int line, int column)
        {
            if (errorCode < 0)
            {
                throw new ArgumentOutOfRangeException("errorCode");
            }

            _errorCode = errorCode;
            _severity = severity;
            _line = line;
            _column = column;
        }

        /// <summary>The message that describes the error.</summary>
        /// <returns>The message that describes the error.</returns>
        public string Message
        {
            get { return _message; }
        }

        /// <summary>The error code associated with the error.</summary>
        /// <returns>The error code associated with the error.</returns>
        public int ErrorCode
        {
            get { return _errorCode; }
        }

        /// <summary>The severity of the error.</summary>
        /// <returns>The severity of the error.</returns>
        public ExtensionErrorSeverity Severity
        {
            get { return _severity; }
            set { _severity = value; }
        }

        /// <summary>The line of the input or output document in which the error occurred.</summary>
        /// <returns>The line of the input or output document in which the error occurred.</returns>
        public int Line
        {
            get { return _line; }
        }

        /// <summary>The column of the input or output document in which the error occurred.</summary>
        /// <returns>The column of the input or output document in which the error occurred.</returns>
        public int Column
        {
            get { return _column; }
        }
    }
}
