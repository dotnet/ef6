// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Data.Entity.Resources;
    using System.Runtime.Serialization;

    /// <summary>
    /// Mapping exception class. Note that this class has state - so if you change even
    /// its internals, it can be a breaking change
    /// </summary>
    [Serializable]
    public sealed class MappingException : EntityException
    {
        /// <summary>
        /// Initializes a new instance of <see cref="T:System.Data.Entity.Core.MappingException" />.
        /// </summary>
        public MappingException() // required ctor
            : base(Strings.Mapping_General_Error)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="T:System.Data.Entity.Core.MappingException" /> with a specialized error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MappingException(string message) // required ctor
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="T:System.Data.Entity.Core.MappingException" /> that uses a specified error message and a reference to the inner exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public MappingException(string message, Exception innerException) // required ctor
            : base(message, innerException)
        {
        }

        /// <summary>
        /// constructor for deserialization
        /// </summary>
        private MappingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
