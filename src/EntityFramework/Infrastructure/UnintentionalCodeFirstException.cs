// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Resources;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Thrown when a context is generated from the <see cref = "DbContext" /> templates in Database First or Model
    ///     First mode and is then used in Code First mode.
    /// </summary>
    /// <remarks>
    ///     Code generated using the T4 templates provided for Database First and Model First use may not work
    ///     correctly if used in Code First mode. To use these classes with Code First please add any additional
    ///     configuration using attributes or the DbModelBuilder API and then remove the code that throws this
    ///     exception.
    /// </remarks>
    [Serializable]
    public class UnintentionalCodeFirstException : InvalidOperationException
    {
        #region Constructors and fields

        /// <summary>
        ///     Initializes a new instance of the <see cref = "UnintentionalCodeFirstException" /> class.
        /// </summary>
        public UnintentionalCodeFirstException()
            : base(Strings.UnintentionalCodeFirstException_Message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref = "UnintentionalCodeFirstException" /> class.
        /// </summary>
        /// <param name = "info">The object that holds the serialized object data.</param>
        /// <param name = "context">The contextual information about the source or destination.</param>
        protected UnintentionalCodeFirstException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref = "UnintentionalCodeFirstException" /> class.
        /// </summary>
        /// <param name = "message">The message.</param>
        public UnintentionalCodeFirstException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref = "UnintentionalCodeFirstException" /> class.
        /// </summary>
        /// <param name = "message">The message.</param>
        /// <param name = "innerException">The inner exception.</param>
        public UnintentionalCodeFirstException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #endregion
    }
}
