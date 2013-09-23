// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a failure while trying to prepare or execute a CommandCompilation
    /// This exception is intended to provide a common exception that people can catch to
    /// hold provider exceptions (SqlException, OracleException) when using the EntityCommand
    /// to execute statements.
    /// </summary>
    [Serializable]
    public sealed class EntityCommandCompilationException : EntityException
    {
        private const int HResultCommandCompilation = -2146232005;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="T:System.Data.Entity.Core.EntityCommandCompilationException" />.
        /// </summary>
        public EntityCommandCompilationException()
        {
            HResult = HResultCommandCompilation;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="T:System.Data.Entity.Core.EntityCommandCompilationException" />.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public EntityCommandCompilationException(string message)
            : base(message)
        {
            HResult = HResultCommandCompilation;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="T:System.Data.Entity.Core.EntityCommandCompilationException" />.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that caused the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public EntityCommandCompilationException(string message, Exception innerException)
            : base(message, innerException)
        {
            HResult = HResultCommandCompilation;
        }

        // <summary>
        // initializes a new instance EntityCommandCompilationException with a given SerializationInfo and StreamingContext
        // </summary>
        private EntityCommandCompilationException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            HResult = HResultCommandCompilation;
        }

        #endregion
    }
}
