// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.UnitTests
{
    using System.CodeDom.Compiler;
    using System.Data.Entity.Edm.Common;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using Xunit;

    public sealed class ModelValidationExceptionTests
    {
        [Fact]
        public void Can_create_exception_with_errors()
        {
            var modelValidationException
                = new ModelValidationException(new[] { (new DataModelErrorEventArgs { ErrorMessage = "Foo" }) });

            Assert.True(modelValidationException.Message.Contains("Foo"));
        }

        [Fact]
        public void String_Ctor_creates_empty_exception()
        {
            var e = new ModelValidationException("A");

            Assert.Equal("A", e.Message);
        }

        [Fact]
        public void String_and_inner_Ctor_creates_empty_exception()
        {
            CodeGenerator.IsValidLanguageIndependentIdentifier("d");

            var inner = new InvalidOperationException();
            var e = new ModelValidationException("A", inner);

            Assert.Equal("A", e.Message);
            Assert.Equal(inner, e.InnerException);
        }

        internal static T BinarySerialization<T>(T obj)
        {
            IFormatter formatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();

            formatter.Serialize(memoryStream, obj);
            memoryStream.Position = 0;

            var newObject = (T)formatter.Deserialize(memoryStream);
            return newObject;
        }
    }
}