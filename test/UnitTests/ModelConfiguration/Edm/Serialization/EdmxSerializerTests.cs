// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Serialization
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Services;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using Xunit;

    public sealed class EdmxSerializerTests
    {
        [Fact]
        public void Serialize_should_return_valid_edmx_xml_v1()
        {
            var databaseMapping = CreateSimpleModel(XmlConstants.StoreVersionForV1);
            var edmx = new XDocument();

            using (var xmlWriter = edmx.CreateWriter())
            {
                new EdmxSerializer().Serialize(databaseMapping, xmlWriter);
            }

            edmx.Validate(LoadEdmxSchemaSet(1), (_, e) => { throw e.Exception; });

            Assert.Equal(
                "False",
                (string)edmx.Descendants("{http://schemas.microsoft.com/ado/2007/06/edmx}DesignerProperty")
                    .Single(e => (string)e.Attribute("Name") == "UseLegacyProvider")
                    .Attribute("Value"));
        }

        [Fact]
        public void Serialize_should_return_valid_edmx_xml_v2()
        {
            var databaseMapping = CreateSimpleModel(XmlConstants.StoreVersionForV2);
            var edmx = new XDocument();

            using (var xmlWriter = edmx.CreateWriter())
            {
                new EdmxSerializer().Serialize(databaseMapping, xmlWriter);
            }

            edmx.Validate(LoadEdmxSchemaSet(2), (_, e) => { throw e.Exception; });

            Assert.Equal(
                "False",
                (string)edmx.Descendants("{http://schemas.microsoft.com/ado/2008/10/edmx}DesignerProperty")
                    .Single(e => (string)e.Attribute("Name") == "UseLegacyProvider")
                    .Attribute("Value"));
        }

        [Fact]
        public void Serialize_should_return_valid_edmx_xml_v3()
        {
            var databaseMapping = CreateSimpleModel(XmlConstants.StoreVersionForV3);
            var edmx = new XDocument();

            using (var xmlWriter = edmx.CreateWriter())
            {
                new EdmxSerializer().Serialize(databaseMapping, xmlWriter);
            }

            edmx.Validate(LoadEdmxSchemaSet(3), (_, e) => { throw e.Exception; });

            Assert.Equal(
                "False",
                (string)edmx.Descendants("{http://schemas.microsoft.com/ado/2009/11/edmx}DesignerProperty")
                    .Single(e => (string)e.Attribute("Name") == "UseLegacyProvider")
                    .Attribute("Value"));
        }

        private static DbDatabaseMapping CreateSimpleModel(double version)
        {
            var model = new EdmModel(DataSpace.CSpace, version);

            var entityType = model.AddEntityType("E");
            var type = typeof(object);

            entityType.GetMetadataProperties().SetClrType(type);
            model.AddEntitySet("ESet", entityType);

            var property1 = EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);
            var property = property1;
            property.Nullable = false;
            entityType.AddKeyMember(property);

            return new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderInfo, ProviderRegistry.Sql2008_ProviderManifest).Generate(model);
        }

        private static XmlSchemaSet LoadEdmxSchemaSet(int version)
        {
            const string resourcePath
                = "System.Data.Entity.ModelConfiguration.Edm.Serialization.Xsd.";

            var schemaSet = new XmlSchemaSet();
            var assembly = typeof(EdmxSerializerTests).Assembly();

            foreach (var schema in new[]
                                       {
                                           "Microsoft.Data.Entity.Design.Edmx_" + version + ".xsd",
                                           "System.Data.Resources.AnnotationSchema.xsd",
                                           "System.Data.Resources.CodeGenerationSchema.xsd",
                                           "System.Data.Resources.CSDLSchema_" + version + ".xsd",
                                           "System.Data.Resources.CSMSL_" + version + ".xsd",
                                           "System.Data.Resources.EntityStoreSchemaGenerator.xsd",
                                           "System.Data.Resources.SSDLSchema_" + version + ".xsd"
                                       })
            {
                schemaSet.Add(null, XmlReader.Create(assembly.GetManifestResourceStream(resourcePath + schema)));
            }

            return schemaSet;
        }
    }
}
