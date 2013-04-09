// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Serialization
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Metadata.Edm.Provider;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using Moq;
    using Xunit;

    public class EdmSerializationVisitorTests
    {
        [Fact]
        public void EdmSerializationVisitor_writes_extended_annotations_for_entity_set()
        {
            var entityType = new EntityType("MyEntity", "Model", DataSpace.SSpace);
            var entitySet = new EntitySet("Entities", null, "Entities", null, entityType);
            entitySet.AddMetadataProperties(
                CreateMetadataProperties(new[] { "http://tempuri.org:extended-property" }));

            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb))
            {
                new EdmSerializationVisitor(writer, 3.0).VisitEdmEntitySet(entitySet);
            }

            var xml = XDocument.Parse(sb.ToString());
            Assert.Equal(
                "ytreporp-dednetxe:gro.irupmet//:ptth",
                (string)xml.Root.Attribute("{http://tempuri.org}extended-property"));
        }

        [Fact]
        public void EdmSerializationVisitor_does_not_write_incorrectly_named_extended_properties()
        {
            var incorrectNames =
                new[]
                    {
                        "extended-property-without-namespace",
                        ":extended-property-starts-with-colon",
                        "extended-property-ends-with-colon:"
                    };

            var entityType = new EntityType("MyEntity", "Model", DataSpace.SSpace);
            var entitySet = new EntitySet("Entities", null, "Entities", null, entityType);

            entitySet.AddMetadataProperties(CreateMetadataProperties(incorrectNames));

            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb))
            {
                new EdmSerializationVisitor(writer, 3.0).VisitEdmEntitySet(entitySet);
            }

            var xml = XDocument.Parse(sb.ToString());
            Assert.False(xml.Root.Attributes().Any(a => incorrectNames.Contains(a.Name.LocalName)));
            Assert.False(xml.Root.Attributes().Any(a => a.Name.Namespace != XNamespace.None));
        }

        private static List<MetadataProperty> CreateMetadataProperties(IEnumerable<string> names)
        {
            var edmString = EdmProviderManifest.Instance.GetPrimitiveType(PrimitiveTypeKind.String);
            var metadataProperties = new List<MetadataProperty>();
            foreach (var name in names)
            {
                metadataProperties.Add(
                    new MetadataProperty(
                        name,
                        TypeUsage.CreateDefaultTypeUsage(edmString),
                        new string(name.Reverse().ToArray())));
            }
            return metadataProperties;
        }

        [Fact]
        public void EdmSerializationVisitor_writes_defining_query_for_entity_set()
        {
            var entityType = new EntityType("MyEntity", "Model", DataSpace.SSpace);
            var entitySet = new EntitySet("Entities", null, "Entities", "Defining Query", entityType);

            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb))
            {
                new EdmSerializationVisitor(writer, 3.0).VisitEdmEntitySet(entitySet);
            }

            var xml = XDocument.Parse(sb.ToString());
            Assert.Equal(
                "Defining Query",
                (string)xml.Root.Element("DefiningQuery"));
        }

        [Fact]
        public void VisitEdmFunction_should_write_start_and_end_elements()
        {
            var schemaWriterMock = new Mock<EdmXmlSchemaWriter>();
            var function = new EdmFunction("F", "N", DataSpace.SSpace);

            new EdmSerializationVisitor(schemaWriterMock.Object).VisitEdmFunction(function);

            schemaWriterMock.Verify(sw => sw.WriteFunctionElementHeader(function), Times.Once());
            schemaWriterMock.Verify(sw => sw.WriteEndElement(), Times.Once());
        }

        [Fact]
        public void VisitFunctionParameter_should_write_start_and_end_elements()
        {
            var schemaWriterMock = new Mock<EdmXmlSchemaWriter>();
            var functionParameter = new FunctionParameter();

            new EdmSerializationVisitor(schemaWriterMock.Object).VisitFunctionParameter(functionParameter);

            schemaWriterMock.Verify(sw => sw.WriteFunctionParameterHeader(functionParameter), Times.Once());
            schemaWriterMock.Verify(sw => sw.WriteEndElement(), Times.Once());
        }

        [Fact]
        public void VisitFunctionImport_writes_start_and_end_elements()
        {
            var schemaWriterMock = new Mock<EdmXmlSchemaWriter>();
            var functionImport = new EdmFunction("foo", "bar", DataSpace.CSpace);

            new EdmSerializationVisitor(schemaWriterMock.Object).VisitFunctionImport(functionImport);

            schemaWriterMock.Verify(sw => sw.WriteFunctionImportElementHeader(functionImport), Times.Once());
            schemaWriterMock.Verify(sw => sw.WriteEndElement(), Times.Once());
        }

        [Fact]
        public void VisitFunctionImportParameter_writes_function_parameter()
        {
            var schemaWriterMock = new Mock<EdmXmlSchemaWriter>();
            var functionImport = new FunctionParameter();

            new EdmSerializationVisitor(schemaWriterMock.Object).VisitFunctionImportParameter(functionImport);

            schemaWriterMock.Verify(sw => sw.WriteFunctionImportParameterElementHeader(functionImport), Times.Once());
            schemaWriterMock.Verify(sw => sw.WriteEndElement(), Times.Once());
        }

        [Fact]
        public void VisitFunctionReturnParameter_writes_return_and_collection_header_for_rowtype()
        {
            var schemaWriterMock = new Mock<EdmXmlSchemaWriter>();
            var returnParameter =
                new FunctionParameter("r", TypeUsage.CreateDefaultTypeUsage(new RowType()), ParameterMode.ReturnValue);

            new EdmSerializationVisitor(schemaWriterMock.Object).VisitFunctionReturnParameter(returnParameter);

            schemaWriterMock.Verify(sw => sw.WriteFunctionReturnTypeElementHeader(), Times.Once());
            schemaWriterMock.Verify(sw => sw.WriteCollectionTypeElementHeader(), Times.Once());
            schemaWriterMock.Verify(sw => sw.WriteRowTypeElementHeader(), Times.Once());
            schemaWriterMock.Verify(sw => sw.WriteEndElement(), Times.Exactly(3));
        }

        [Fact]
        public void VisitFunctionReturnParameter_writes_return_and_collection_header_for_primitive_return_type()
        {
            var schemaWriterMock = new Mock<EdmXmlSchemaWriter>();
            var returnParameter =
                new FunctionParameter(
                    "r", 
                    TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)), 
                    ParameterMode.ReturnValue);

            new EdmSerializationVisitor(schemaWriterMock.Object).VisitFunctionReturnParameter(returnParameter);

            schemaWriterMock.Verify(sw => sw.WriteFunctionReturnTypeElementHeader(), Times.Never());
            schemaWriterMock.Verify(sw => sw.WriteCollectionTypeElementHeader(), Times.Never());
            schemaWriterMock.Verify(sw => sw.WriteRowTypeElementHeader(), Times.Never());
            schemaWriterMock.Verify(sw => sw.WriteEndElement(), Times.Never());
        }

        [Fact]
        public void VisitRowType_writes_rowtype()
        {
            var schemaWriterMock = new Mock<EdmXmlSchemaWriter>();
            new EdmSerializationVisitor(schemaWriterMock.Object).VisitRowType(new RowType());

            schemaWriterMock.Verify(sw => sw.WriteRowTypeElementHeader(), Times.Once());
            schemaWriterMock.Verify(sw => sw.WriteEndElement(), Times.Once());
        }
    }
}