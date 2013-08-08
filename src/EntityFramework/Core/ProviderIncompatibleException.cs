// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is thrown when the store provider exhibits a behavior incompatible with the entity client provider
    /// </summary>
    [Serializable]
    public sealed class ProviderIncompatibleException : EntityException
    {
        /// <summary>
        /// Initializes a new instance of <see cref="T:System.Data.Entity.Core.ProviderIncompatibleException" />.
        /// </summary>
        public ProviderIncompatibleException()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="T:System.Data.Entity.Core.ProviderIncompatibleException" /> with a specialized error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ProviderIncompatibleException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="T:System.Data.Entity.Core.ProviderIncompatibleException" /> that uses a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public ProviderIncompatibleException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of ProviderIncompatibleException
        /// </summary>
        private ProviderIncompatibleException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
