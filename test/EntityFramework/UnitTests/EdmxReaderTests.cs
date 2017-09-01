// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System;
    using System.Data.Entity;
    using System.IO;
    using System.Xml;
    using SimpleModel;
    using Xunit;

    /// <summary>
    /// Unit tests for EdmxReader methods.
    /// </summary>
    public class EdmxReaderTests : TestBase
    {
        [Fact]
        public void Read_throws_when_given_null_reader()
        {
            Assert.Equal(
                "reader",
                Assert.Throws<ArgumentNullException>(() => EdmxReader.Read(null, "defaultSchema")).ParamName);
        }

        [Fact]
        public void Read_loads_edmx_from_EdmxWriter_into_DbCompiledModel()
        {
            var stream = new MemoryStream();
            var xmlWriter = XmlWriter.Create(stream);

            using (var context = new EmptyContext())
            {
                EdmxWriter.WriteEdmx(context, xmlWriter);
            }

            stream.Seek(0, SeekOrigin.Begin);

            var defaultSchema = "default";
            var xmlReader = XmlReader.Create(stream);
            var readModel = EdmxReader.Read(xmlReader, defaultSchema);

            Assert.IsType<DbCompiledModel>(readModel);

            Assert.Equal(defaultSchema, readModel.DefaultSchema);
            Assert.NotNull(readModel.ProviderInfo);
        }
    }
}
