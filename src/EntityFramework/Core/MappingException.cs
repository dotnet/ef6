// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core
{
    using System.Data.Entity.Resources;
    using System.Runtime.Serialization;

    /// <summary>
    /// Mapping exception class. Note that this class has state - so if you change even
    /// its internals, it can be a breaking change
    /// </summary>
    /// 
    [Serializable]
    public sealed class MappingException : EntityException
    {
        /// <summary>
        /// constructor with default message
        /// </summary>
        public MappingException() // required ctor
            : base(Strings.Mapping_General_Error)
        {
        }

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="message">localized error message</param>
        public MappingException(string message) // required ctor
            : base(message)
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="message">localized error message</param>
        /// <param name="innerException">inner exception</param>
        public MappingException(string message, Exception innerException) // required ctor
            : base(message, innerException)
        {
        }

        /// <summary>
        /// constructor for deserialization
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        private MappingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
