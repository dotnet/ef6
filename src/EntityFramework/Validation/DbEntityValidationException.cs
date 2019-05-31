// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception thrown from <see cref="DbContext.SaveChanges()" /> when validating entities fails.
    /// </summary>
    [Serializable]
    public class DbEntityValidationException : DataException
    {
        // <summary>
        // Validation results.
        // </summary>
        private IList<DbEntityValidationResult> _entityValidationResults;

        /// <summary>
        /// Initializes a new instance of DbEntityValidationException.
        /// </summary>
        public DbEntityValidationException()
            : this(Strings.DbEntityValidationException_ValidationFailed)
        {
        }

        /// <summary>
        /// Initializes a new instance of DbEntityValidationException.
        /// </summary>
        /// <param name="message"> The exception message. </param>
        public DbEntityValidationException(string message)
            : this(message, Enumerable.Empty<DbEntityValidationResult>())
        {
        }

        /// <summary>
        /// Initializes a new instance of DbEntityValidationException.
        /// </summary>
        /// <param name="message"> The exception message. </param>
        /// <param name="entityValidationResults"> Validation results. </param>
        public DbEntityValidationException(
            string message, IEnumerable<DbEntityValidationResult> entityValidationResults)
            : base(message)
        {
            // Users should be able to set the errors to null but we should not
            Check.NotNull(entityValidationResults, "entityValidationResults");

            InititializeValidationResults(entityValidationResults);
        }

        /// <summary>
        /// Initializes a new instance of DbEntityValidationException.
        /// </summary>
        /// <param name="message"> The exception message. </param>
        /// <param name="innerException"> The inner exception. </param>
        public DbEntityValidationException(string message, Exception innerException)
            : this(message, Enumerable.Empty<DbEntityValidationResult>(), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of DbEntityValidationException.
        /// </summary>
        /// <param name="message"> The exception message. </param>
        /// <param name="entityValidationResults"> Validation results. </param>
        /// <param name="innerException"> The inner exception. </param>
        public DbEntityValidationException(
            string message, IEnumerable<DbEntityValidationResult> entityValidationResults, Exception innerException)
            : base(message, innerException)
        {
            // Users should be able to set the errors to null but we should not.
            Check.NotNull(entityValidationResults, "entityValidationResults");

            InititializeValidationResults(entityValidationResults);
        }

        /// <summary>
        /// Initializes a new instance of the DbEntityValidationException class with the specified serialization information and context.
        /// </summary>
        /// <param name="info"> The data necessary to serialize or deserialize an object. </param>
        /// <param name="context"> Description of the source and destination of the specified serialized stream. </param>
        protected DbEntityValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _entityValidationResults = (List<DbEntityValidationResult>)info.GetValue("EntityValidationErrors", typeof(List<DbEntityValidationResult>));
        }

        /// <summary>
        /// Validation results.
        /// </summary>
        public IEnumerable<DbEntityValidationResult> EntityValidationErrors
        {
            get { return _entityValidationResults; }
        }

        /// <summary>
        /// Sets the <see cref="SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info"> The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
        /// <param name="context"> The <see cref="StreamingContext" /> that contains contextual information about the source or destination. </param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("EntityValidationErrors", _entityValidationResults);
        }

        private void InititializeValidationResults(IEnumerable<DbEntityValidationResult> entityValidationResults)
        {
            _entityValidationResults = entityValidationResults == null
                                           ? new List<DbEntityValidationResult>()
                                           : entityValidationResults.ToList();
        }
    }
}
