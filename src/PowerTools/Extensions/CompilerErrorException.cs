// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Extensions
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.DbContextPackage.Utilities;

    [Serializable]
    public class CompilerErrorException : Exception
    {
        private readonly IEnumerable<CompilerError> _errors;

        public CompilerErrorException()
        {
        }

        public CompilerErrorException(string message)
            : base(message)
        {
        }

        public CompilerErrorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public CompilerErrorException(string message, IEnumerable<CompilerError> errors)
            : base(message)
        {
            Check.NotNull(errors, "errors");

            _errors = errors;
        }

        protected CompilerErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Check.NotNull(info, "info");

            _errors = (IEnumerable<CompilerError>)info.GetValue("Errors", typeof(IEnumerable<CompilerError>));
        }

        public IEnumerable<CompilerError> Errors
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
