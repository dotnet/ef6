// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Edm.Serialization.UnitTests
{
    using System.Data.Entity;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm.Services;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using Xunit;

    public sealed class EdmxSerializerTests
    {
        [Fact]
        public void Serialize_should_return_valid_edmx_xml_v2()
        {
            var databaseMapping = CreateSimpleModel(2.0);
            var edmx = new XDocument();

            using (var xmlWriter = edmx.CreateWriter())
            {
                new EdmxSerializer().Serialize(databaseMapping, ProviderRegistry.Sql2008_ProviderInfo, xmlWriter);
            }

            edmx.Validate(LoadEdmxSchemaSet(2), (_, e) => { throw e.Exception; });
        }

        [Fact]
        public void Serialize_should_return_valid_edmx_xml_v3()
        {
            var databaseMapping = CreateSimpleModel(3.0);
            var edmx = new XDocument();

            using (var xmlWriter = edmx.CreateWriter())
            {
                new EdmxSerializer().Serialize(databaseMapping, ProviderRegistry.Sql2008_ProviderInfo, xmlWriter);
            }

            edmx.Validate(LoadEdmxSchemaSet(3), (_, e) => { throw e.Exception; });
        }

        private static DbDatabaseMapping CreateSimpleModel(double version)
        {
            var model = new EdmModel().Initialize(version);

            var entityType = model.AddEntityType("E");
            entityType.SetClrType(typeof(object));
            model.AddEntitySet("ESet", entityType);

            var property = entityType.AddPrimitiveProperty("Id");
            property.PropertyType.EdmType = EdmPrimitiveType.Int32;
            property.PropertyType.IsNullable = false;
            entityType.DeclaredKeyProperties.Add(property);

            return new DatabaseMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(model);
        }

        private static XmlSchemaSet LoadEdmxSchemaSet(int version)
        {
            const string resourcePath
                = "System.Data.Entity.ModelConfiguration.Edm.Serialization.Xsd.";

            var schemaSet = new XmlSchemaSet();
            var assembly = Assembly.GetExecutingAssembly();

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