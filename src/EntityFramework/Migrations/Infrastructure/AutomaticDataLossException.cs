namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Diagnostics.Contracts;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Represents an error that occurs when an automatic migration would result in data loss.
    /// </summary>
    [Serializable]
    public sealed class AutomaticDataLossException : MigrationsException
    {
        /// <summary>
        ///     Initializes a new instance of the AutomaticDataLossException class.
        /// </summary>
        /// <param name = "message">The message that describes the error.</param>
        public AutomaticDataLossException(string message)
            : base(message)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(message));
        }

        private AutomaticDataLossException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}