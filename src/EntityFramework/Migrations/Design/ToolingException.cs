namespace System.Data.Entity.Migrations.Design
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Security;

    /// <summary>
    ///     Represents an exception that occurred while running an operation in another AppDomain in the <see cref = "ToolingFacade" />.
    /// </summary>
    [Serializable]
    public class ToolingException : Exception
    {
        private readonly string _innerType;
        private readonly string _innerStackTrace;

        /// <summary>
        ///     Initializes a new instance of the ToolingException class.
        /// </summary>
        public ToolingException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        public ToolingException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the ToolingException class.
        /// </summary>
        /// <param name = "message">Error that explains the reason for the exception.</param>
        /// <param name = "innerType">The type of the exception that was thrown.</param>
        /// <param name = "innerStackTrace">The stack trace of the exception that was thrown.</param>
        public ToolingException(string message, string innerType, string innerStackTrace)
            : base(message)
        {
            _innerType = innerType;
            _innerStackTrace = innerStackTrace;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception. </param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified. </param>
        public ToolingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <inheritdoc />
        protected ToolingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _innerType = info.GetString("InnerType");
            _innerStackTrace = info.GetString("InnerStackTrace");
        }

        /// <summary>
        ///     Gets the type of the exception that was thrown.
        /// </summary>
        public string InnerType
        {
            get { return _innerType; }
        }

        /// <summary>
        ///     Gets the stack trace of the exception that was thrown.
        /// </summary>
        public string InnerStackTrace
        {
            get { return _innerStackTrace; }
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule")]
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("InnerType", _innerType);
            info.AddValue("InnerStackTrace", _innerStackTrace);
        }
    }
}
