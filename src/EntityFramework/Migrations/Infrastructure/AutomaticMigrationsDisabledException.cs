namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Runtime.Serialization;

    /// <summary>
    ///     Represents an error that occurs when there are pending model changes after applying the last migration and automatic migration is disabled.
    /// </summary>
    [Serializable]
    public sealed class AutomaticMigrationsDisabledException : MigrationsException
    {
        /// <summary>
        ///     Initializes a new instance of the AutomaticMigrationsDisabledException class.
        /// </summary>
        public AutomaticMigrationsDisabledException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the AutomaticMigrationsDisabledException class.
        /// </summary>
        /// <param name = "message">The message that describes the error.</param>
        public AutomaticMigrationsDisabledException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MigrationsException class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified. </param>
        public AutomaticMigrationsDisabledException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        private AutomaticMigrationsDisabledException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
