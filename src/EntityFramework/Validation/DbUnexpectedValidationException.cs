// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Exception thrown from <see cref="DbContext.GetValidationErrors()" /> when an exception is thrown from the validation
    ///     code.
    /// </summary>
    [Serializable]
    public class DbUnexpectedValidationException : DataException
    {
        /// <summary>
        ///     Initializes a new instance of DbUnexpectedValidationException.
        /// </summary>
        /// <param name="message"> The exception message. </param>
        public DbUnexpectedValidationException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of DbUnexpectedValidationException.
        /// </summary>
        /// <param name="message"> The exception message. </param>
        public DbUnexpectedValidationException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of DbUnexpectedValidationException.
        /// </summary>
        /// <param name="message"> The exception message. </param>
        /// <param name="innerException"> The inner exception. </param>
        public DbUnexpectedValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of DbUnexpectedValidationException with the specified serialization info and
        ///     context.
        /// </summary>
        /// <param name="info"> The serialization info. </param>
        /// <param name="context"> The streaming context. </param>
        [ExcludeFromCodeCoverage]
        protected DbUnexpectedValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
