// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Runtime.Serialization;

    /// <summary>
    ///     Represents a failure while trying to prepare or execute a CommandCompilation
    ///     This exception is intended to provide a common exception that people can catch to
    ///     hold provider exceptions (SqlException, OracleException) when using the EntityCommand
    ///     to execute statements.
    /// </summary>
    [Serializable]
    public sealed class EntityCommandCompilationException : EntityException
    {
        private const int HResultCommandCompilation = -2146232005;

        #region Constructors

        /// <summary>
        ///     initializes a new instance of EntityCommandCompilationException, no message, no inner exception.  Probably shouldn't
        ///     exist, but it makes FxCop happy.
        /// </summary>
        public EntityCommandCompilationException()
        {
            HResult = HResultCommandCompilation;
        }

        /// <summary>
        ///     initializes a new instance of EntityCommandCompilationException, with message, no inner exception.  Probably shouldn't
        ///     exist, but it makes FxCop happy.
        /// </summary>
        public EntityCommandCompilationException(string message)
            : base(message)
        {
            HResult = HResultCommandCompilation;
        }

        /// <summary>
        ///     initializes a new instance of EntityCommandCompilationException with message and an inner exception instance
        /// </summary>
        /// <param name="message"> </param>
        /// <param name="innerException"> </param>
        public EntityCommandCompilationException(string message, Exception innerException)
            : base(message, innerException)
        {
            HResult = HResultCommandCompilation;
        }

        /// <summary>
        ///     initializes a new instance EntityCommandCompilationException with a given SerializationInfo and StreamingContext
        /// </summary>
        /// <param name="serializationInfo"> </param>
        /// <param name="streamingContext"> </param>
        private EntityCommandCompilationException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            HResult = HResultCommandCompilation;
        }

        #endregion
    }
}
