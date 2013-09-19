// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is thrown when a requested object is not found in the store.
    /// </summary>
    [Serializable]
    public sealed class ObjectNotFoundException : DataException
    {
        /// <summary>
        /// Initializes a new instance of <see cref="T:System.Data.Entity.Core.ObjectNotFoundException" />.
        /// </summary>
        public ObjectNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="T:System.Data.Entity.Core.ObjectNotFoundException" /> with a specialized error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ObjectNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="T:System.Data.Entity.Core.ObjectNotFoundException" /> class that uses a specified error message and a reference to the inner exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public ObjectNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of ObjectNotFoundException
        /// </summary>
        private ObjectNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
