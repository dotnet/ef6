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
        ///     constructor with default message
        /// </summary>
        public PropertyConstraintException() // required ctor
        {
            SubscribeToSerializeObjectState();
        }

        /// <summary>
        ///     costructor with supplied message
        /// </summary>
        /// <param name="message"> localized error message </param>
        public PropertyConstraintException(string message) // required ctor
            : base(message)
        {
            SubscribeToSerializeObjectState();
        }

        /// <summary>
        ///     costructor with supplied message and inner exception
        /// </summary>
        /// <param name="message"> localized error message </param>
        /// <param name="innerException"> inner exception </param>
        public PropertyConstraintException(string message, Exception innerException) // required ctor
            : base(message, innerException)
        {
            SubscribeToSerializeObjectState();
        }

        /// <summary>
        ///     default constructor
        /// </summary>
        /// <param name="message"> localized error message </param>
        public PropertyConstraintException(string message, string propertyName) // required ctor
            : base(message)
        {
            Check.NotEmpty(propertyName, "propertyName");
            _state.PropertyName = propertyName;

            SubscribeToSerializeObjectState();
        }

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="message"> localized error message </param>
        /// <param name="innerException"> inner exception </param>
        public PropertyConstraintException(string message, string propertyName, Exception innerException) // required ctor
            : base(message, innerException)
        {
            Check.NotEmpty(propertyName, "propertyName");
            _state.PropertyName = propertyName;

            SubscribeToSerializeObjectState();
        }

        /// <summary>
        ///     Gets the name of the property that violated the constraint.
        /// </summary>
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
