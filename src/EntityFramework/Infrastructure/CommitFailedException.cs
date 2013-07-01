// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Runtime.Serialization;

    [Serializable]
    public class CommitFailedException : DataException
    {
        /// <summary>
        ///     Initializes a new instance of <see cref="CommitFailedException"/>
        /// </summary>
        public CommitFailedException()
        {
        }
        
        /// <summary>
        ///     Initializes a new instance of <see cref="CommitFailedException"/>
        /// </summary>
        /// <param name="message"> The exception message. </param>
        public CommitFailedException(string message)
            : base(message)
        {
        }
        
        /// <summary>
        ///     Initializes a new instance of <see cref="CommitFailedException"/>
        /// </summary>
        /// <param name="message"> The exception message. </param>
        /// <param name="innerException"> The inner exception. </param>
        public CommitFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected CommitFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
