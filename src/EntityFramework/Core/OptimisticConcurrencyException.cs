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
        /// Initializes a new instance of <see cref="T:System.Data.Entity.Core.OptimisticConcurrencyException" />.
        /// </summary>
        public OptimisticConcurrencyException()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="T:System.Data.Entity.Core.OptimisticConcurrencyException" /> with a specialized error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public OptimisticConcurrencyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="T:System.Data.Entity.Core.OptimisticConcurrencyException" /> that uses a specified error message and a reference to the inner exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public OptimisticConcurrencyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="T:System.Data.Entity.Core.OptimisticConcurrencyException" /> that uses a specified error message, a reference to the inner exception, and an enumerable collection of
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" />
        /// objects.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        /// <param name="stateEntries">
        /// The enumerable collection of <see cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" /> objects.
        /// </param>
        public OptimisticConcurrencyException(string message, Exception innerException, IEnumerable<ObjectStateEntry> stateEntries)
            : base(message, innerException, stateEntries)
        {
        }

        // <summary>
        // Initializes a new instance of OptimisticConcurrencyException
        // </summary>
        private OptimisticConcurrencyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
