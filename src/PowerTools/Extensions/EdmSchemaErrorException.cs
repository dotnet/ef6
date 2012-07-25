// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Data.Metadata.Edm;
    using System.Diagnostics.Contracts;
    using System.Runtime.Serialization;

    [Serializable]
    public class EdmSchemaErrorException : Exception
    {
        private readonly IEnumerable<EdmSchemaError> _errors;

        public EdmSchemaErrorException()
        {
        }

        public EdmSchemaErrorException(string message)
            : base(message)
        {
        }

        public EdmSchemaErrorException(string message, IEnumerable<EdmSchemaError> errors)
            : base(message)
        {
            Contract.Requires(errors != null);

            _errors = errors;
        }

        public EdmSchemaErrorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected EdmSchemaErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Contract.Requires(info != null);

            _errors = (IEnumerable<EdmSchemaError>)info.GetValue("Errors", typeof(IEnumerable<EdmSchemaError>));
        }

        public IEnumerable<EdmSchemaError> Errors
        {
            get { return _errors; }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Contract.Requires(info != null);

            info.AddValue("Errors", _errors);

            base.GetObjectData(info, context);
        }
    }
}
