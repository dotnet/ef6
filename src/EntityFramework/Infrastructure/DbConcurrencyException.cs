// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Resources;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Indicates that concurrent access to a critical section was detected.
    ///     This exception indicates potential state corruption and should not be caught.
    /// </summary>
    [Serializable]
    public class DbConcurrencyException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DbConcurrencyException" /> class.
        /// </summary>
        public DbConcurrencyException()
            : base(Strings.ConcurrentMethodInvocation)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbConcurrencyException" /> class.
        /// </summary>
        /// <param name="message"> The message. </param>
        public DbConcurrencyException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbConcurrencyException" /> class.
        /// </summary>
        /// <param name="message"> The message. </param>
        /// <param name="innerException"> The inner exception. </param>
        public DbConcurrencyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbConcurrencyException" /> class.
        /// </summary>
        /// <param name="info"> The object that holds the serialized object data. </param>
        /// <param name="context"> The contextual information about the source or destination. </param>
        protected DbConcurrencyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
