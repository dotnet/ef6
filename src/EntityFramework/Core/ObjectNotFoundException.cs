// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Runtime.Serialization;

    /// <summary>
    ///     This exception is thrown when a requested object is not found in the store.
    /// </summary>
    [Serializable]
    public sealed class ObjectNotFoundException : DataException
    {
        /// <summary>
        ///     Initializes a new instance of ObjectNotFoundException
        /// </summary>
        public ObjectNotFoundException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of ObjectNotFoundException
        /// </summary>
        /// <param name="message"> </param>
        public ObjectNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Constructor that takes a message and an inner exception
        /// </summary>
        /// <param name="message"> </param>
        /// <param name="innerException"> </param>
        public ObjectNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of ObjectNotFoundException
        /// </summary>
        /// <param name="info"> </param>
        /// <param name="context"> </param>
        private ObjectNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
