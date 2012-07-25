// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is thrown when a update operation violates the concurrency constraint.
    /// </summary>
    [Serializable]
    public sealed class OptimisticConcurrencyException : UpdateException
    {
        /// <summary>
        /// new OptimisticConcurrencyException object
        /// </summary> 
        public OptimisticConcurrencyException()
        {
        }

        /// <summary>
        /// Initializes a new instance of OptimisticConcurrencyException
        /// </summary>
        /// <param name="message"></param>
        public OptimisticConcurrencyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of OptimisticConcurrencyException
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public OptimisticConcurrencyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of OptimisticConcurrencyException
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <param name="stateEntries"></param>
        public OptimisticConcurrencyException(string message, Exception innerException, IEnumerable<ObjectStateEntry> stateEntries)
            : base(message, innerException, stateEntries)
        {
        }

        /// <summary>
        /// Initializes a new instance of OptimisticConcurrencyException
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        private OptimisticConcurrencyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
