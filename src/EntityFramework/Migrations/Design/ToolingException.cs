// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET45 || NET40

namespace System.Data.Entity.Migrations.Design
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Represents an exception that occurred while running an operation in another AppDomain in the
    ///     <see cref="ToolingFacade" />.
    /// </summary>
    [Obsolete("Use System.Data.Entity.Infrastructure.Design.IErrorHandler instead.")]
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors",
        Justification = "SerializeObjectState used instead")]
    [Serializable]
    public class ToolingException : Exception
    {
        [NonSerialized]
        private ToolingExceptionState _state;

        /// <summary>
        ///     Initializes a new instance of the ToolingException class.
        /// </summary>
        public ToolingException()
        {
            SubscribeToSerializeObjectState();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Exception" /> class with a specified error message.
        /// </summary>
        /// <param name="message"> The message that describes the error. </param>
        public ToolingException(string message)
            : base(message)
        {
            SubscribeToSerializeObjectState();
        }

        /// <summary>
        ///     Initializes a new instance of the ToolingException class.
        /// </summary>
        /// <param name="message"> Error that explains the reason for the exception. </param>
        /// <param name="innerType"> The type of the exception that was thrown. </param>
        /// <param name="innerStackTrace"> The stack trace of the exception that was thrown. </param>
        public ToolingException(string message, string innerType, string innerStackTrace)
            : base(message)
        {
            _state.InnerType = innerType;
            _state.InnerStackTrace = innerStackTrace;

            SubscribeToSerializeObjectState();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Exception" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message"> The error message that explains the reason for the exception. </param>
        /// <param name="innerException"> The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified. </param>
        public ToolingException(string message, Exception innerException)
            : base(message, innerException)
        {
            SubscribeToSerializeObjectState();
        }

        /// <summary>
        ///     Gets the type of the exception that was thrown.
        /// </summary>
        public string InnerType
        {
            get { return _state.InnerType; }
        }

        /// <summary>
        ///     Gets the stack trace of the exception that was thrown.
        /// </summary>
        public string InnerStackTrace
        {
            get { return _state.InnerStackTrace; }
        }

        private void SubscribeToSerializeObjectState()
        {
            SerializeObjectState += (_, a) => a.AddSerializedState(_state);
        }

        [Serializable]
        private struct ToolingExceptionState : ISafeSerializationData
        {
            public string InnerType { get; set; }
            public string InnerStackTrace { get; set; }

            public void CompleteDeserialization(object deserialized)
            {
                ((ToolingException)deserialized)._state = this;
            }
        }
    }
}

#endif
