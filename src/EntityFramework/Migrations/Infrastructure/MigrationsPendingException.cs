// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Thrown when an operation can't be performed because there are existing migrations that have not been applied to the database.
    /// </summary>
    [Serializable]
    public sealed class MigrationsPendingException : MigrationsException
    {
        /// <summary>
        /// Initializes a new instance of the MigrationsPendingException class.
        /// </summary>
        public MigrationsPendingException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the MigrationsPendingException class.
        /// </summary>
        /// <param name="message"> The message that describes the error. </param>
        public MigrationsPendingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MigrationsPendingException class.
        /// </summary>
        /// <param name="message"> The message that describes the error. </param>
        /// <param name="innerException"> The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified. </param>
        public MigrationsPendingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MigrationsPendingException class with serialized data.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext" /> that contains contextual information about the source or destination.
        /// </param>
        private MigrationsPendingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
