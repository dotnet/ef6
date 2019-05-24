// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Data.Entity.Utilities;
    using System.Runtime.Serialization;

    /// <summary>
    /// Property constraint exception class. Note that this class has state - so if you change even
    /// its internals, it can be a breaking change
    /// </summary>
    [Serializable]
    public sealed class PropertyConstraintException : ConstraintException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.PropertyConstraintException" /> class with default message.
        /// </summary>
        public PropertyConstraintException() // required ctor
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.PropertyConstraintException" /> class with supplied message.
        /// </summary>
        /// <param name="message">A localized error message.</param>
        public PropertyConstraintException(string message) // required ctor
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.PropertyConstraintException" /> class with supplied message and inner exception.
        /// </summary>
        /// <param name="message">A localized error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PropertyConstraintException(string message, Exception innerException) // required ctor
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.PropertyConstraintException" /> class.
        /// </summary>
        /// <param name="message">A localized error message.</param>
        /// <param name="propertyName">The name of the property.</param>
        public PropertyConstraintException(string message, string propertyName) // required ctor
            : base(message)
        {
            Check.NotEmpty(propertyName, "propertyName");
            PropertyName = propertyName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.PropertyConstraintException" /> class.
        /// </summary>
        /// <param name="message">A localized error message.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="innerException">The inner exception.</param>
        public PropertyConstraintException(string message, string propertyName, Exception innerException) // required ctor
            : base(message, innerException)
        {
            Check.NotEmpty(propertyName, "propertyName");
            PropertyName = propertyName;
        }

        private PropertyConstraintException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PropertyName = info.GetString("PropertyName");
        }

        /// <summary>Gets the name of the property that violated the constraint.</summary>
        /// <returns>The name of the property that violated the constraint.</returns>
        public string PropertyName { get; }

        /// <summary>
        /// Sets the <see cref="SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info"> The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
        /// <param name="context"> The <see cref="StreamingContext" /> that contains contextual information about the source or destination. </param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("PropertyName", PropertyName);
        }
    }
}
