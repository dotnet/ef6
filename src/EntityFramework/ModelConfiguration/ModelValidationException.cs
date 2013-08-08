// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception thrown by <see cref="DbModelBuilder" /> during model creation when an invalid model is generated.
    /// </summary>
    [Serializable]
    public class ModelValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of ModelValidationException
        /// </summary>
        public ModelValidationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of ModelValidationException
        /// </summary>
        /// <param name="message"> The exception message. </param>
        public ModelValidationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of ModelValidationException
        /// </summary>
        /// <param name="message"> The exception message. </param>
        /// <param name="innerException"> The inner exception. </param>
        public ModelValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        internal ModelValidationException(IEnumerable<DataModelErrorEventArgs> validationErrors)
            : base(validationErrors.ToErrorMessage())
        {
            DebugCheck.NotNull(validationErrors);
            Debug.Assert(validationErrors.Any());
        }

        /// <summary>Initializes a new instance of <see cref="T:System.Data.Entity.ModelConfiguration.ModelValidationException" /> class serialization info and streaming context.</summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected ModelValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
