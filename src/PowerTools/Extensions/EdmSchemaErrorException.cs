// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Data.Metadata.Edm;
    using System.Runtime.Serialization;
    using Utilities;

    [Serializable]
    public class EdmSchemaErrorException : Exception
    {
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

            Errors = errors;
        }

        public EdmSchemaErrorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected EdmSchemaErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Check.NotNull(info, "info");

            Errors = (IEnumerable<EdmSchemaError>)info.GetValue("Errors", typeof(IEnumerable<EdmSchemaError>));
        }

        public IEnumerable<EdmSchemaError> Errors { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Check.NotNull(info, "info");

            info.AddValue("Errors", Errors);

            base.GetObjectData(info, context);
        }
    }
}
