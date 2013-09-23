// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Data.Entity.Resources;
    using System.Runtime.Serialization;

    /// <summary>
    /// metadata exception class
    /// </summary>
    [Serializable]
    public sealed class MetadataException : EntityException
    {
        private const int HResultMetadata = -2146232007;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.MetadataException" /> class with a default message.
        /// </summary>
        public MetadataException() // required ctor
            : base(Strings.Metadata_General_Error)
        {
            HResult = HResultMetadata;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.MetadataException" /> class with the specified message.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public MetadataException(string message) // required ctor
            : base(message)
        {
            HResult = HResultMetadata;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.MetadataException" /> class with the specified message and inner exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">
        /// The exception that is the cause of this <see cref="T:System.Data.Entity.Core.MetadataException" />.
        /// </param>
        public MetadataException(string message, Exception innerException) // required ctor
            : base(message, innerException)
        {
            HResult = HResultMetadata;
        }

        // <summary>
        // constructor for deserialization
        // </summary>
        private MetadataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}
