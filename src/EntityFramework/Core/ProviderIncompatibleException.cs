// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Runtime.Serialization;

    /// <summary>
    ///     This exception is thrown when the store provider exhibits a behavior incompatible with the entity client provider
    /// </summary>
    [Serializable]
    public sealed class ProviderIncompatibleException : EntityException
    {
        /// <summary>
        ///     Initializes a new instance of ProviderIncompatibleException
        /// </summary>
        public ProviderIncompatibleException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of ProviderIncompatibleException
        /// </summary>
        /// <param name="message"> </param>
        public ProviderIncompatibleException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Constructor that takes a message and an inner exception
        /// </summary>
        /// <param name="message"> </param>
        /// <param name="innerException"> </param>
        public ProviderIncompatibleException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of ProviderIncompatibleException
        /// </summary>
        /// <param name="info"> </param>
        /// <param name="context"> </param>
        private ProviderIncompatibleException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
