// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Data.Entity.Resources;
    using System.Runtime.Serialization;

    /// <summary>
    ///     metadata exception class
    /// </summary>
    [Serializable]
    public sealed class MetadataException : EntityException
    {
        private const int HResultMetadata = -2146232007;

        #region Constructors

        /// <summary>
        ///     constructor with default message
        /// </summary>
        public MetadataException() // required ctor
            : base(Strings.Metadata_General_Error)
        {
            HResult = HResultMetadata;
        }

        /// <summary>
        ///     default constructor
        /// </summary>
        /// <param name="message"> localized error message </param>
        public MetadataException(string message) // required ctor
            : base(message)
        {
            HResult = HResultMetadata;
        }

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="message"> localized error message </param>
        /// <param name="innerException"> inner exception </param>
        public MetadataException(string message, Exception innerException) // required ctor
            : base(message, innerException)
        {
            HResult = HResultMetadata;
        }

        /// <summary>
        ///     constructor for deserialization
        /// </summary>
        /// <param name="info"> </param>
        /// <param name="context"> </param>
        private MetadataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}
