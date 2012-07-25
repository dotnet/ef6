// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Extensions
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Xunit;

    public class CompilerErrorExceptionTests
    {
        [Fact]
        public void Ctor_validates_preconditions()
        {
            IEnumerable<CompilerError> errors = null;

            var ex = Assert.Throws<ArgumentNullException>(
                () => new CompilerErrorException("Not used", errors));

            Assert.Equal("errors", ex.ParamName);
        }

        [Fact]
        public void Ctor_sets_properties()
        {
            var message = "Some message";
            var errors = new CompilerError[0];

            var ex = new CompilerErrorException(message, errors);

            Assert.Equal(message, ex.Message);
            Assert.Same(errors, ex.Errors);
        }

        [Fact]
        public void Is_serializable()
        {
            var message = "Some message";
            var errors = new CompilerError[0];
            var formatter = new BinaryFormatter();
            CompilerErrorException ex;

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, new CompilerErrorException(message, errors));
                stream.Position = 0;
                ex = (CompilerErrorException)formatter.Deserialize(stream);
            }

            Assert.Equal(message, ex.Message);
            Assert.Equal(errors, ex.Errors);
        }
    }
}
