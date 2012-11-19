// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Runtime.Serialization;

    /// <summary>
    ///     Represents a failure while trying to prepare or execute a CommandExecution
    ///     This exception is intended to provide a common exception that people can catch to
    ///     hold provider exceptions (SqlException, OracleException) when using the EntityCommand
    ///     to execute statements.
    /// </summary>
    [Serializable]
    public sealed class EntityCommandExecutionException : EntityException
    {
        private const int HResultCommandExecution = -2146232004;

        #region Constructors

        /// <summary>
        ///     initializes a new instance of EntityCommandExecutionException, no message, no inner exception.  Probably shouldn't
        ///     exist, but it makes FxCop happy.
        /// </summary>
        public EntityCommandExecutionException()
        {
            HResult = HResultCommandExecution;
        }

        /// <summary>
        ///     initializes a new instance of EntityCommandExecutionException, with message, no inner exception.  Probably shouldn't
        ///     exist, but it makes FxCop happy.
        /// </summary>
        public EntityCommandExecutionException(string message)
            : base(message)
        {
            HResult = HResultCommandExecution;
        }

        /// <summary>
        ///     initializes a new instance of EntityCommandExecutionException with message and an inner exception instance
        /// </summary>
        /// <param name="message"> </param>
        /// <param name="innerException"> </param>
        public EntityCommandExecutionException(string message, Exception innerException)
            : base(message, innerException)
        {
            HResult = HResultCommandExecution;
        }

        /// <summary>
        ///     initializes a new instance EntityCommandExecutionException with a given SerializationInfo and StreamingContext
        /// </summary>
        /// <param name="serializationInfo"> </param>
        /// <param name="streamingContext"> </param>
        private EntityCommandExecutionException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            HResult = HResultCommandExecution;
        }

        #endregion
    }
}
