// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Data.Metadata.Edm;
    using System.Runtime.Serialization;
    using Microsoft.DbContextPackage.Utilities;

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
            Check.NotNull(errors, "errors");

            _errors = errors;
        }

        public EdmSchemaErrorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected EdmSchemaErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Check.NotNull(info, "info");

            _errors = (IEnumerable<EdmSchemaError>)info.GetValue("Errors", typeof(IEnumerable<EdmSchemaError>));
        }

        public IEnumerable<EdmSchemaError> Errors
        {
            get { return _errors; }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Check.NotNull(info, "info");

            info.AddValue("Errors", _errors);

            base.GetObjectData(info, context);
        }
    }
}
