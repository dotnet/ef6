namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents errors that occur inside the Code First Migrations pipeline.
    /// </summary>
    [Serializable]
    public class MigrationsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the MigrationsException class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MigrationsException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MigrationsException class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected MigrationsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}