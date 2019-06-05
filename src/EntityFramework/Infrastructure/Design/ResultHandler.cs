// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Design
{
    /// <summary>
    ///     Used with <see cref="Executor" /> to handle operation results.
    /// </summary>
    public class ResultHandler : HandlerBase, IResultHandler2
    {
        private bool _hasResult;
        private object _result;
        private string _errorType;
        private string _errorMessage;
        private string _errorStackTrace;

        /// <summary>
        ///     Gets a value indicating whether a result is available.
        /// </summary>
        /// <value>A value indicating whether a result is available.</value>
        public virtual bool HasResult
            => _hasResult;

        /// <summary>
        ///     Gets the result.
        /// </summary>
        /// <value>The result.</value>
        public virtual object Result
            => _result;

        /// <summary>
        ///     Gets the type of the exception if any.
        /// </summary>
        /// <value>The exception type.</value>
        public virtual string ErrorType
            => _errorType;

        /// <summary>
        ///     Gets the error message if any.
        /// </summary>
        /// <value>The error message.</value>
        public virtual string ErrorMessage
            => _errorMessage;

        /// <summary>
        ///     Get the error stack trace if any.
        /// </summary>
        /// <value> The stack trace. </value>
        public virtual string ErrorStackTrace
            => _errorStackTrace;

        /// <summary>
        ///     Invoked when a result is available.
        /// </summary>
        /// <param name="value"> The result. </param>
        public virtual void SetResult(object value)
        {
            _hasResult = true;
            _result = value;
        }

        /// <summary>
        ///     Invoked when an error occurs.
        /// </summary>
        /// <param name="type"> The exception type. </param>
        /// <param name="message"> The error message. </param>
        /// <param name="stackTrace"> The stack trace. </param>
        public virtual void SetError(string type, string message, string stackTrace)
        {
            _errorType = type;
            _errorMessage = message;
            _errorStackTrace = stackTrace;
        }
    }
}
