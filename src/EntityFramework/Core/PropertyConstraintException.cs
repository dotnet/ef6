// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Property constraint exception class. Note that this class has state - so if you change even
    ///     its internals, it can be a breaking change
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors",
        Justification = "SerializeObjectState used instead")]
    [Serializable]
    public sealed class PropertyConstraintException : ConstraintException
    {
        [NonSerialized]
        private PropertyConstraintExceptionState _state;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Data.Entity.Core.PropertyConstraintException" /> class with default message.
        /// </summary>
        public PropertyConstraintException() // required ctor
        {
            SubscribeToSerializeObjectState();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Data.Entity.Core.PropertyConstraintException" /> class with supplied message.
        /// </summary>
        /// <param name="message">A localized error message.</param>
        public PropertyConstraintException(string message) // required ctor
            : base(message)
        {
            SubscribeToSerializeObjectState();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Data.Entity.Core.PropertyConstraintException" /> class with supplied message and inner exception.
        /// </summary>
        /// <param name="message">A localized error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PropertyConstraintException(string message, Exception innerException) // required ctor
            : base(message, innerException)
        {
            SubscribeToSerializeObjectState();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Data.Entity.Core.PropertyConstraintException" /> class.
        /// </summary>
        /// <param name="message">A localized error message.</param>
        /// <param name="propertyName">The name of the property.</param>
        public PropertyConstraintException(string message, string propertyName) // required ctor
            : base(message)
        {
            Check.NotEmpty(propertyName, "propertyName");
            _state.PropertyName = propertyName;

            SubscribeToSerializeObjectState();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Data.Entity.Core.PropertyConstraintException" /> class.
        /// </summary>
        /// <param name="message">A localized error message.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="innerException">The inner exception.</param>
        public PropertyConstraintException(string message, string propertyName, Exception innerException) // required ctor
            : base(message, innerException)
        {
            Check.NotEmpty(propertyName, "propertyName");
            _state.PropertyName = propertyName;

            SubscribeToSerializeObjectState();
        }

        /// <summary>Gets the name of the property that violated the constraint.</summary>
        /// <returns>The name of the property that violated the constraint.</returns>
        public string PropertyName
        {
            get { return _state.PropertyName; }
        }

        private void SubscribeToSerializeObjectState()
        {
            SerializeObjectState += (_, a) => a.AddSerializedState(_state);
        }

        [Serializable]
        private struct PropertyConstraintExceptionState : ISafeSerializationData
        {
            public string PropertyName { get; set; }

            public void CompleteDeserialization(object deserialized)
            {
                ((PropertyConstraintException)deserialized)._state = this;
            }
        }
    }
}
