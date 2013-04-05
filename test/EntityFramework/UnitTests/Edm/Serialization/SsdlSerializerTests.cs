// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Serialization
{
    using Moq;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.SqlServer;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using Xunit;

    public class SsdlSerializerTests
    {
        private static readonly XNamespace Ssdl3Ns = XmlConstants.TargetNamespace_3;

        [Fact]
        public void SsdlSerializer_uses_entitycontainer_to_create_schema_namespace_name_if_no_provided()
        {
            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb))
            {
                new SsdlSerializer()
                    .Serialize(
                    new EdmModel(DataSpace.SSpace),
                    ProviderRegistry.Sql2008_ProviderInfo.ProviderInvariantName,
                    ProviderRegistry.Sql2008_ProviderInfo.ProviderManifestToken, 
                    writer);
            }

            Assert.Equal(
                "CodeFirstDatabaseSchema",
                (string)XDocument.Parse(sb.ToString()).Root.Attribute("Namespace"));
        }

        [Fact]
        public void SsdlSerializer_uses_schema_namespace_name_if_provided()
        {
            Assert.Equal(
                "MyNamespace",
                (string)(GetSerializedModel(new EdmModel(DataSpace.SSpace), "MyNamespace").Root.Attribute("Namespace")));
        }

        [Fact]
        public void SsdlSerializer_writes_default_nullability_when_serializeDefaultNullability_true()
        {
            var ssdl = GetSerializedModel(CreateTestModel(), "MyNamespace", serializeDefaultNullability: true);

            Assert.Equal(
                "true",
                (string)GetProperty(ssdl, "NullableProperty").Attribute("Nullable"));

            Assert.Equal(
                "false",
                (string)GetProperty(ssdl, "NonNullableProperty").Attribute("Nullable"));
        }

        [Fact]
        public void SsdlSerializer_does_not_write_default_nullability_when_serializeDefaultNullability_false()
        {
            var ssdl = GetSerializedModel(CreateTestModel(), "MyNamespace", serializeDefaultNullability: false);

            Assert.Null(GetProperty(ssdl, "NullableProperty").Attribute("Nullable"));

            Assert.Equal(
                "false",
                (string)GetProperty(ssdl, "NonNullableProperty").Attribute("Nullable"));
        }

        [Fact]
        public void Serialize_without_schemaNamespace_returns_false_if_multiple_NamespaceNames()
        {
            var mockModel = new Mock<EdmModel>(DataSpace.SSpace, XmlConstants.SchemaVersionLatest);
            mockModel.Setup(m => m.NamespaceNames).Returns(new string[2]);
            var mockWriter = new Mock<XmlWriter>();

            var validationErrors = new List<Validation.DataModelErrorEventArgs>();
            var serializer = new SsdlSerializer();
            serializer.OnError += (_, e) => validationErrors.Add(e);
            Assert.False(serializer.Serialize(
                mockModel.Object,
                ProviderRegistry.Sql2008_ProviderInfo.ProviderInvariantName,
                ProviderRegistry.Sql2008_ProviderInfo.ProviderManifestToken,
                mockWriter.Object,
                false));
            Assert.Equal(1, validationErrors.Count());
            Assert.Equal(Strings.Serializer_OneNamespaceAndOneContainer, validationErrors[0].ErrorMessage);
            mockWriter.Verify(m => m.WriteStartDocument(), Times.Never());
        }

        [Fact]
        public void Serialize_with_schemaNamespace_returns_false_if_multiple_NamespaceNames()
        {
            var mockModel = new Mock<EdmModel>(DataSpace.SSpace, XmlConstants.SchemaVersionLatest);
            mockModel.Setup(m => m.NamespaceNames).Returns(new string[2]);
            var mockWriter = new Mock<XmlWriter>();

            var validationErrors = new List<Validation.DataModelErrorEventArgs>(); 
            var serializer = new SsdlSerializer();
            serializer.OnError += (_, e) => validationErrors.Add(e);
            Assert.False(serializer.Serialize(
                mockModel.Object,
                "MyNamespace",
                ProviderRegistry.Sql2008_ProviderInfo.ProviderInvariantName,
                ProviderRegistry.Sql2008_ProviderInfo.ProviderManifestToken,
                mockWriter.Object,
                false));
            Assert.Equal(1, validationErrors.Count());
            Assert.Equal(Strings.Serializer_OneNamespaceAndOneContainer, validationErrors[0].ErrorMessage);
            mockWriter.Verify(m => m.WriteStartDocument(), Times.Never());
        }

        [Fact]
        public void Serialize_without_schemaNamespace_returns_false_if_multiple_Containers()
        {
            var mockModel = new Mock<EdmModel>(DataSpace.SSpace, XmlConstants.SchemaVersionLatest);
            mockModel.Setup(m => m.Containers).Returns(
                new EntityContainer[] {
                    new EntityContainer("Container1", DataSpace.SSpace), 
                    new EntityContainer("Container2", DataSpace.SSpace) });
            var mockWriter = new Mock<XmlWriter>();

            var validationErrors = new List<Validation.DataModelErrorEventArgs>();
            var serializer = new SsdlSerializer();
            serializer.OnError += (_, e) => validationErrors.Add(e);
            Assert.False(serializer.Serialize(
                mockModel.Object,
                ProviderRegistry.Sql2008_ProviderInfo.ProviderInvariantName,
                ProviderRegistry.Sql2008_ProviderInfo.ProviderManifestToken,
                mockWriter.Object,
                false));
            Assert.Equal(1, validationErrors.Count());
            Assert.Equal(Strings.Serializer_OneNamespaceAndOneContainer, validationErrors[0].ErrorMessage);
            mockWriter.Verify(m => m.WriteStartDocument(), Times.Never());
        }

        [Fact]
        public void Serialize_with_schemaNamespace_returns_false_if_multiple_Containers()
        {
            var mockModel = new Mock<EdmModel>(DataSpace.SSpace, XmlConstants.SchemaVersionLatest);
            mockModel.Setup(m => m.Containers).Returns(new EntityContainer[] { new EntityContainer("Container1", DataSpace.SSpace), new EntityContainer("Container2", DataSpace.SSpace) });
            var mockWriter = new Mock<XmlWriter>();

            var validationErrors = new List<Validation.DataModelErrorEventArgs>();
            var serializer = new SsdlSerializer();
            serializer.OnError += (_, e) => validationErrors.Add(e);
            Assert.False(serializer.Serialize(
                mockModel.Object,
                "MyNamespace",
                ProviderRegistry.Sql2008_ProviderInfo.ProviderInvariantName,
                ProviderRegistry.Sql2008_ProviderInfo.ProviderManifestToken,
                mockWriter.Object,
                false));
            Assert.Equal(1, validationErrors.Count());
            Assert.Equal(Strings.Serializer_OneNamespaceAndOneContainer, validationErrors[0].ErrorMessage);
            mockWriter.Verify(m => m.WriteStartDocument(), Times.Never());
        }

        [Fact]
        public void Serialize_without_schemaNamespace_returns_false_if_error_in_model()
        {
            var model = new EdmModel(DataSpace.SSpace);
            var mockWriter = new Mock<XmlWriter>();

            // add EntityType with no properties which will cause error
            var et = new EntityType("TestEntity", "TestNamespace", DataSpace.SSpace);
            model.AddItem(et);

            var validationErrors = new List<Validation.DataModelErrorEventArgs>();
            var serializer = new SsdlSerializer();
            serializer.OnError += (_, e) => validationErrors.Add(e);
            Assert.False(serializer.Serialize(
                model,
                ProviderRegistry.Sql2008_ProviderInfo.ProviderInvariantName,
                ProviderRegistry.Sql2008_ProviderInfo.ProviderManifestToken,
                mockWriter.Object,
                false));
            Assert.Equal(1, validationErrors.Count());
            Assert.Equal(
                Strings.EdmModel_Validator_Semantic_KeyMissingOnEntityType("TestEntity"),
                validationErrors[0].ErrorMessage);
            mockWriter.Verify(m => m.WriteStartDocument(), Times.Never());
        }

        [Fact]
        public void Serialize_with_schemaNamespace_returns_false_if_error_in_model()
        {
            var model = new EdmModel(DataSpace.SSpace);
            var mockWriter = new Mock<XmlWriter>();

            // add EntityType with no properties which will cause error
            var et = new EntityType("TestEntity", "TestNamespace", DataSpace.SSpace);
            model.AddItem(et);

            var validationErrors = new List<Validation.DataModelErrorEventArgs>();
            var serializer = new SsdlSerializer();
            serializer.OnError += (_, e) => validationErrors.Add(e);
            Assert.False(serializer.Serialize(
                model,
                "MyNamespace",
                ProviderRegistry.Sql2008_ProviderInfo.ProviderInvariantName,
                ProviderRegistry.Sql2008_ProviderInfo.ProviderManifestToken,
                mockWriter.Object,
                false));
            Assert.Equal(1, validationErrors.Count());
            Assert.Equal(
                Strings.EdmModel_Validator_Semantic_KeyMissingOnEntityType("TestEntity"),
                validationErrors[0].ErrorMessage);
            mockWriter.Verify(m => m.WriteStartDocument(), Times.Never());
        }

        private EdmModel CreateTestModel()
        {
            var model = new EdmModel(DataSpace.SSpace);

            var sqlManifest = new SqlProviderManifest("2008");
            var stringTypeUsage = sqlManifest.GetStoreType(
                TypeUsage.CreateDefaultTypeUsage(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)));

            var complexType = new ComplexType("Entity", "Unicorns420", DataSpace.SSpace);
            complexType.AddMember(new EdmProperty("NullableProperty", stringTypeUsage) { Nullable = true });
            complexType.AddMember(new EdmProperty("NonNullableProperty", stringTypeUsage) { Nullable = false });
            model.AddItem(complexType);

            return model;
        }

        private static XDocument GetSerializedModel(EdmModel model, string schemaNamespace, bool serializeDefaultNullability = true)
        {
            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb))
            {
                new SsdlSerializer()
                    .Serialize(
                        model,
                        schemaNamespace,
                        ProviderRegistry.Sql2008_ProviderInfo.ProviderInvariantName,
                        ProviderRegistry.Sql2008_ProviderInfo.ProviderManifestToken,
                        writer,
                        serializeDefaultNullability);
            }

            return XDocument.Parse(sb.ToString());
        }

        private static XElement GetProperty(XDocument ssdl, string propertyName)
        {
            return 
                ssdl
                    .Descendants(Ssdl3Ns + "Property").
                    Single(p => (string)p.Attribute("Name") == propertyName);
        }
    }
}
