// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Data.Entity.Utilities;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Represents an error that occurs when an automatic migration would result in data loss.
    /// </summary>
    [Serializable]
    public sealed class AutomaticDataLossException : MigrationsException
    {
        /// <summary>
        ///     Initializes a new instance of the AutomaticDataLossException class.
        /// </summary>
        public AutomaticDataLossException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the AutomaticDataLossException class.
        /// </summary>
        /// <param name="message"> The message that describes the error. </param>
        public AutomaticDataLossException(string message)
            : base(message)
        {
            Check.NotEmpty(message, "message");
        }

        /// <summary>
        ///     Initializes a new instance of the MigrationsException class.
        /// </summary>
        /// <param name="message"> The message that describes the error. </param>
        /// <param name="innerException"> The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified. </param>
        public AutomaticDataLossException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        private AutomaticDataLossException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
