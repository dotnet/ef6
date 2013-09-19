// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Data.Metadata.Edm;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Xunit;

    public class EdmSchemaErrorExceptionTests
    {
        [Fact]
        public void Ctor_validates_preconditions()
        {
            IEnumerable<EdmSchemaError> errors = null;

            var ex = Assert.Throws<ArgumentNullException>(
                () => new EdmSchemaErrorException("Not used", errors));

            Assert.Equal("errors", ex.ParamName);
        }

        [Fact]
        public void Ctor_sets_properties()
        {
            var message = "Some message";
            var errors = new EdmSchemaError[0];

            var ex = new EdmSchemaErrorException(message, errors);

            Assert.Equal(message, ex.Message);
            Assert.Same(errors, ex.Errors);
        }

        [Fact]
        public void Is_serializable()
        {
            var message = "Some message";
            var errors = new EdmSchemaError[0];
            var formatter = new BinaryFormatter();
            EdmSchemaErrorException ex;

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, new EdmSchemaErrorException(message, errors));
                stream.Position = 0;
                ex = (EdmSchemaErrorException)formatter.Deserialize(stream);
            }

            Assert.Equal(message, ex.Message);
            Assert.Equal(errors, ex.Errors);
        }
    }
}
