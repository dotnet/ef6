// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Design
{
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Xunit;

    public class ToolingExceptionTests
    {
        [Fact]
        public void Can_set_properties()
        {
            var ex = new ToolingException("message", "innerType", "innerStackTrace");

            Assert.Equal("message", ex.Message);
            Assert.Equal("innerType", ex.InnerType);
            Assert.Equal("innerStackTrace", ex.InnerStackTrace);
        }

        [Fact]
        public void Can_serialize()
        {
            var formatter = new BinaryFormatter();
            var originalException = new ToolingException("message", "innerType", "innerStackTrace");

            ToolingException exception;

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, originalException);

                stream.Seek(0, SeekOrigin.Begin);

                exception = (ToolingException)formatter.Deserialize(stream);
            }

            Assert.Equal(originalException.Message, exception.Message);
            Assert.Equal(originalException.InnerType, exception.InnerType);
            Assert.Equal(originalException.InnerStackTrace, exception.InnerStackTrace);
        }
    }
}
