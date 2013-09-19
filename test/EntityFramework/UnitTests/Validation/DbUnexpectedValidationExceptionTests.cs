// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using Xunit;

    public class DbUnexpectedValidationExceptionTests
    {
        [Fact]
        public void DbUnexpectedValidationException_parameterless_constructor()
        {
            var exception = new DbUnexpectedValidationException();

            Assert.Equal("Data Exception.", exception.Message);
        }

        [Fact]
        public void DbUnexpectedValidationException_string_constructor()
        {
            var exception = new DbUnexpectedValidationException("Exception");

            Assert.Equal("Exception", exception.Message);
        }

        [Fact]
        public void DbUnexpectedValidationException_string_exception_constructor()
        {
            var innerException = new Exception();
            var exception = new DbUnexpectedValidationException("Exception", innerException);

            Assert.Equal("Exception", exception.Message);
            Assert.Same(innerException, exception.InnerException);
        }
    }
}
