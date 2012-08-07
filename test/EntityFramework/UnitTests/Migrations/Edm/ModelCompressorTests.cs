// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity;
    using System.Data.Entity.Utilities;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Edm;
    using System.IO;
    using System.Text;
    using System.Xml;
    using Xunit;

    public class ModelCompressorTests
    {
        [Fact]
        public void Should_be_able_to_roundtrip_model_through_gzip()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();

            var model = modelBuilder.Build(new DbProviderInfo(DbProviders.Sql, "2008"));

            var edmxString = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(
                edmxString, new XmlWriterSettings
                                {
                                    Indent = true
                                }))
            {
                EdmxWriter.WriteEdmx(model, xmlWriter);
            }

            var modelCompressor = new ModelCompressor();

            var bytes = modelCompressor.Compress(model.GetModel());

            Assert.True(bytes.Length > 2000);

            var edmxXDocument = modelCompressor.Decompress(bytes);

            using (var stringWriter = new StringWriter())
            {
                edmxXDocument.Save(stringWriter);

                Assert.Equal(edmxString.ToString(), stringWriter.ToString());
            }
        }
    }
}
