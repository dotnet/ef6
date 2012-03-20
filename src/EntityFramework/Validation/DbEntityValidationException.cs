namespace System.Data.Entity.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Exception thrown from <see cref = "DbContext.SaveChanges()" /> when validating entities fails.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors",
        Justification = "SerializeObjectState used instead")]
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db",
        Justification = "FxCop rule is wrong; Database is not two words.")]
    [Serializable]
    public class DbEntityValidationException : DataException
    {
        [NonSerialized]
        private DbEntityValidationExceptionState _state = new DbEntityValidationExceptionState();

        /// <summary>
        ///     Initializes a new instance of DbEntityValidationException
        /// </summary>
        public DbEntityValidationException()
            : this(Strings.DbEntityValidationException_ValidationFailed)
        {
        }

        /// <summary>
        ///     Initializes a new instance of DbEntityValidationException
        /// </summary>
        /// <param name = "message">The exception message.</param>
        public DbEntityValidationException(string message)
            : this(message, Enumerable.Empty<DbEntityValidationResult>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of DbEntityValidationException
        /// </summary>
        /// <param name = "message">The exception message.</param>
        /// <param name = "entityValidationResults">Validation results.</param>
        public DbEntityValidationException(string message, IEnumerable<DbEntityValidationResult> entityValidationResults)
            : base(message)
        {
            // Users should be able to set the errors to null but we should not
            Contract.Requires(entityValidationResults != null);

            _state.InititializeValidationResults(entityValidationResults);
            SubscribeToSerializeObjectState();
        }

        /// <summary>
        ///     Initializes a new instance of DbEntityValidationException
        /// </summary>
        /// <param name = "message">The exception message.</param>
        /// <param name = "innerException">The inner exception.</param>
        public DbEntityValidationException(string message, Exception innerException)
            : this(message, Enumerable.Empty<DbEntityValidationResult>(), innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of DbEntityValidationException
        /// </summary>
        /// <param name = "message">The exception message.</param>
        /// <param name = "entityValidationResults">Validation results.</param>
        /// <param name = "innerException">The inner exception.</param>
        public DbEntityValidationException(
            string message, IEnumerable<DbEntityValidationResult> entityValidationResults, Exception innerException)
            : base(message, innerException)
        {
            // Users should be able to set the errors to null but we should not. 
            Contract.Requires(entityValidationResults != null);

            _state.InititializeValidationResults(entityValidationResults);
            SubscribeToSerializeObjectState();
        }

        /// <summary>
        ///     Validation results.
        /// </summary>
        public IEnumerable<DbEntityValidationResult> EntityValidationErrors
        {
            get { return _state.EntityValidationErrors; }
        }

        /// <summary>
        ///     Subscribes the SerializeObjectState event.
        /// </summary>
        private void SubscribeToSerializeObjectState()
        {
            SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
        }

        /// <summary>
        ///     Holds exception state that will be serialized when the exception is serialized.
        /// </summary>
        [Serializable]
        private class DbEntityValidationExceptionState : ISafeSerializationData
        {
            /// <summary>
            ///     Validation results.
            /// </summary>
            private IList<DbEntityValidationResult> _entityValidationResults;

            internal void InititializeValidationResults(IEnumerable<DbEntityValidationResult> entityValidationResults)
            {
                _entityValidationResults = entityValidationResults == null
                                               ? new List<DbEntityValidationResult>()
                                               : entityValidationResults.ToList();
            }

            /// <summary>
            ///     Validation results.
            /// </summary>
            public IEnumerable<DbEntityValidationResult> EntityValidationErrors
            {
                get { return _entityValidationResults; }
            }

            /// <summary>
            ///     Completes the deserialization.
            /// </summary>
            /// <param name = "deserialized">The deserialized object.</param>
            public void CompleteDeserialization(object deserialized)
            {
                ((DbEntityValidationException)deserialized)._state = this;
            }
        }
    }
}