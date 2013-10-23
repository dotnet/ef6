// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Moq;
    using Xunit;

    public class EdmxHelperTests
    {
        private const string EdmxTemplate =
            @"<edmx:Edmx Version=""3.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2009/11/edmx"">" +
            @"  <!-- EF Runtime content -->" +
            @"  <edmx:Runtime>" +
            @"    <!-- SSDL content -->" +
            @"    <edmx:StorageModels>" +
            @"      <dummy />" +
            @"    </edmx:StorageModels>" +
            @"    <!-- CSDL content -->" +
            @"    <edmx:ConceptualModels>" +
            @"      <dummy />" +
            @"    </edmx:ConceptualModels>" +
            @"    <!-- C-S mapping content -->" +
            @"    <edmx:Mappings>" +
            @"      <dummy />" +
            @"    </edmx:Mappings>" +
            @"  </edmx:Runtime>" +
            @"  <!-- EF Designer content (DO NOT EDIT MANUALLY BELOW HERE) -->" +
            @"  <Designer xmlns=""http://schemas.microsoft.com/ado/2009/11/edmx"" />" +
            @"</edmx:Edmx>";

        private static readonly XNamespace EdmxV3Namespace =
            SchemaManager.GetEDMXNamespaceName(EntityFrameworkVersion.Version3);

        [Fact]
        public void UpdateEdmxFromModel_updates_csdl_ssdl_msl()
        {
            var inputXml = XDocument.Parse("<root />");
            var databaseMapping =
                new DbDatabaseMapping
                    {
                        Model = new EdmModel(DataSpace.CSpace),
                        Database = new EdmModel(DataSpace.SSpace)
                    };
            var model = new DbModel(databaseMapping, new DbModelBuilder());

            var mockEdmxHelper = new Mock<EdmxHelper>(inputXml);
            mockEdmxHelper.Setup(
                h => h.UpdateStorageModels(
                    It.Is<EdmModel>(m => m == databaseMapping.Database),
                    It.Is<string>(n => n == "storeNamespace"),
                    It.Is<DbProviderInfo>(i => i == model.ProviderInfo),
                    It.IsAny<List<EdmSchemaError>>()))
                .Returns(true);
            mockEdmxHelper.Setup(
                h => h.UpdateConceptualModels(
                    It.Is<EdmModel>(m => m == databaseMapping.Model),
                    It.Is<string>(n => n == "entityModelNamespace")))
                .Returns(true);
            mockEdmxHelper.Setup(
                h => h.UpdateMapping(
                    It.Is<DbModel>(m => m == model)))
                .Returns(true);
            mockEdmxHelper.Object.UpdateEdmxFromModel(
                model, "storeNamespace", "entityModelNamespace", new List<EdmSchemaError>());

            mockEdmxHelper
                .Verify(
                    h => h.UpdateStorageModels(
                        It.Is<EdmModel>(m => m == databaseMapping.Database),
                        It.Is<string>(n => n == "storeNamespace"),
                        It.Is<DbProviderInfo>(i => i == model.ProviderInfo),
                        It.IsAny<List<EdmSchemaError>>()), Times.Once());

            mockEdmxHelper
                .Verify(
                    h => h.UpdateConceptualModels(
                        It.Is<EdmModel>(m => m == databaseMapping.Model),
                        It.Is<string>(n => n == "entityModelNamespace")), Times.Once());

            mockEdmxHelper
                .Verify(
                    h => h.UpdateMapping(
                        It.Is<DbModel>(m => m == model)), Times.Once());
        }

        [Fact]
        public void UpdateEdmxFromModel_does_not_update_csdl_or_msl_if_ssdl_update_fails()
        {
            var inputXml = XDocument.Parse("<root />");
            var databaseMapping =
                new DbDatabaseMapping
                    {
                        Model = new EdmModel(DataSpace.CSpace),
                        Database = new EdmModel(DataSpace.SSpace)
                    };
            var model = new DbModel(databaseMapping, new DbModelBuilder());

            var mockEdmxHelper = new Mock<EdmxHelper>(inputXml);
            mockEdmxHelper.Setup(
                h => h.UpdateStorageModels(
                    It.Is<EdmModel>(m => m == databaseMapping.Database),
                    It.Is<string>(n => n == "storeNamespace"),
                    It.Is<DbProviderInfo>(i => i == model.ProviderInfo),
                    It.IsAny<List<EdmSchemaError>>()))
                .Returns(false);
            mockEdmxHelper.Setup(
                h => h.UpdateConceptualModels(
                    It.Is<EdmModel>(m => m == databaseMapping.Model),
                    It.Is<string>(n => n == "entityModelNamespace")))
                .Returns(true);
            mockEdmxHelper.Setup(
                h => h.UpdateMapping(
                    It.Is<DbModel>(m => m == model)))
                .Returns(true);
            mockEdmxHelper.Object.UpdateEdmxFromModel(
                model, "storeNamespace", "entityModelNamespace", new List<EdmSchemaError>());

            mockEdmxHelper
                .Verify(
                    h => h.UpdateStorageModels(
                        It.Is<EdmModel>(m => m == databaseMapping.Database),
                        It.Is<string>(n => n == "storeNamespace"),
                        It.Is<DbProviderInfo>(i => i == model.ProviderInfo),
                        It.IsAny<List<EdmSchemaError>>()), Times.Once());

            mockEdmxHelper
                .Verify(
                    h => h.UpdateConceptualModels(
                        It.Is<EdmModel>(m => m == databaseMapping.Model),
                        It.Is<string>(n => n == "entityModelNamespace")), Times.Never());

            mockEdmxHelper
                .Verify(
                    h => h.UpdateMapping(
                        It.Is<DbModel>(m => m == model)), Times.Never());
        }

        [Fact]
        public void UpdateEdmxFromModel_does_not_update_msl_if_ssdl_update_works_but_csdl_update_fails()
        {
            var inputXml = XDocument.Parse("<root />");
            var databaseMapping =
                new DbDatabaseMapping
                    {
                        Model = new EdmModel(DataSpace.CSpace),
                        Database = new EdmModel(DataSpace.SSpace)
                    };
            var model = new DbModel(databaseMapping, new DbModelBuilder());

            var mockEdmxHelper = new Mock<EdmxHelper>(inputXml);
            mockEdmxHelper.Setup(
                h => h.UpdateStorageModels(
                    It.Is<EdmModel>(m => m == databaseMapping.Database),
                    It.Is<string>(n => n == "storeNamespace"),
                    It.Is<DbProviderInfo>(i => i == model.ProviderInfo),
                    It.IsAny<List<EdmSchemaError>>()))
                .Returns(true);
            mockEdmxHelper.Setup(
                h => h.UpdateConceptualModels(
                    It.Is<EdmModel>(m => m == databaseMapping.Model),
                    It.Is<string>(n => n == "entityModelNamespace")))
                .Returns(false);
            mockEdmxHelper.Setup(
                h => h.UpdateMapping(
                    It.Is<DbModel>(m => m == model)))
                .Returns(true);
            mockEdmxHelper.Object.UpdateEdmxFromModel(
                model, "storeNamespace", "entityModelNamespace", new List<EdmSchemaError>());

            mockEdmxHelper
                .Verify(
                    h => h.UpdateStorageModels(
                        It.Is<EdmModel>(m => m == databaseMapping.Database),
                        It.Is<string>(n => n == "storeNamespace"),
                        It.Is<DbProviderInfo>(i => i == model.ProviderInfo),
                        It.IsAny<List<EdmSchemaError>>()), Times.Once());

            mockEdmxHelper
                .Verify(
                    h => h.UpdateConceptualModels(
                        It.Is<EdmModel>(m => m == databaseMapping.Model),
                        It.Is<string>(n => n == "entityModelNamespace")), Times.Once());

            mockEdmxHelper
                .Verify(
                    h => h.UpdateMapping(
                        It.Is<DbModel>(m => m == model)), Times.Never());
        }

        [Fact]
        public void UpdateConceptualModel_updates_csdl()
        {
            var edmx = XDocument.Parse(EdmxTemplate);

            new EdmxHelper(edmx)
                .UpdateConceptualModels(new EdmModel(DataSpace.CSpace), "modelNamespace");

            var conceptualModelsElements =
                edmx.Descendants(EdmxV3Namespace + "ConceptualModels").Single();

            Assert.Equal(1, conceptualModelsElements.Elements().Count());
            Assert.Equal(
                XName.Get("Schema", SchemaManager.GetCSDLNamespaceName(EntityFrameworkVersion.Version3)),
                conceptualModelsElements.Elements().Single().Name);

            Assert.Equal("modelNamespace", (string)conceptualModelsElements.Elements().Single().Attribute("Namespace"));
        }

        [Fact]
        public void UpdateConceptualModel_does_not_update_csdl_when_ReplaceEdmxSection_returns_false()
        {
            // This test is really only to test that EdmxHelper.ReplaceEdmxSection() returns false
            // when the underlying XmlWriter returns null (because of errors in the model it is trying
            // to write) - but that method is private so need to test through one of the other methods

            var edmx = XDocument.Parse(EdmxTemplate);

            // create EntityType with no members (and hence no keys) which will cause error
            var edmModel = new EdmModel(DataSpace.CSpace);
            edmModel.AddItem(
                EntityType.Create(
                    "TestEntity", "TestNamespace", DataSpace.CSpace,
                    new string[0], new EdmMember[0], new MetadataProperty[0]));
            new EdmxHelper(edmx).UpdateConceptualModels(edmModel, "modelNamespace");

            var conceptualModelsElements =
                edmx.Descendants(EdmxV3Namespace + "ConceptualModels").Single();

            Assert.Equal(1, conceptualModelsElements.Elements().Count());
            Assert.Equal(
                XNamespace.None + "dummy",
                conceptualModelsElements.Elements().Single().Name);
        }

        [Fact]
        public void UpdateStorageModel_updates_ssdl()
        {
            var edmx = XDocument.Parse(EdmxTemplate);

            var storeModel = new EdmModel(DataSpace.SSpace);
            var providerInfo = new DbProviderInfo("ProviderInvariantName", "20081");

            new EdmxHelper(edmx)
                .UpdateStorageModels(storeModel, "myNamespace", providerInfo, new List<EdmSchemaError>());

            var storageModelsElements =
                edmx.Descendants(EdmxV3Namespace + "StorageModels").Single();

            Assert.Equal(1, storageModelsElements.Elements().Count());
            Assert.Equal(
                XName.Get("Schema", SchemaManager.GetSSDLNamespaceName(EntityFrameworkVersion.Version3)),
                storageModelsElements.Elements().Single().Name);
            var schemaElement = storageModelsElements.Elements().Single();
            Assert.Equal("myNamespace", (string)schemaElement.Attribute("Namespace"));
            Assert.Equal("ProviderInvariantName", (string)schemaElement.Attribute("Provider"));
            Assert.Equal("20081", (string)schemaElement.Attribute("ProviderManifestToken"));
        }

        [Fact]
        public void UpdateStorageModel_add_errors_if_validation_fails()
        {
            var edmx = XDocument.Parse(EdmxTemplate);

            var storeModel = new EdmModel(DataSpace.SSpace);
            var providerInfo = new DbProviderInfo("ProviderInvariantName", "20081");

            storeModel.AddItem(
                EntityType.Create("entities", "ns", DataSpace.SSpace, new string[0], new EdmMember[0], null));

            var errors = new List<EdmSchemaError>();
            new EdmxHelper(edmx)
                .UpdateStorageModels(storeModel, "myNamespace", providerInfo, errors);

            Assert.NotEmpty(errors);
        }

        [Fact]
        public void UpdateMapping_updates_msl()
        {
            var edmx = XDocument.Parse(EdmxTemplate);
            var databaseMapping =
                new DbDatabaseMapping
                    {
                        Database = new EdmModel(DataSpace.SSpace),
                        Model = new EdmModel(DataSpace.SSpace),
                    };
            var model = new DbModel(databaseMapping, new DbModelBuilder());

            databaseMapping.AddEntityContainerMapping(
                new EntityContainerMapping(
                    databaseMapping.Model.Containers.Single(),
                    databaseMapping.Database.Containers.Single(),
                    null, false, false));

            new EdmxHelper(edmx).UpdateMapping(model);

            var storageModelsElements =
                edmx.Descendants(EdmxV3Namespace + "Mappings").Single();

            Assert.Equal(1, storageModelsElements.Elements().Count());
            Assert.Equal(
                XName.Get("Mapping", SchemaManager.GetMSLNamespaceName(EntityFrameworkVersion.Version3)),
                storageModelsElements.Elements().Single().Name);
        }

        [Fact]
        public void UpdateDesignerOptionProperty_updates_existing_property()
        {
            var edmx = XDocument.Parse(EdmxTemplate);
            var edmxNs = edmx.Root.Name.Namespace;

            var designerElement = edmx.Root.Element(edmxNs + "Designer");
            designerElement.Add(
                new XElement(
                    edmxNs + "Options",
                    new XElement(
                        edmxNs + "DesignerInfoPropertySet",
                        new XElement(
                            edmxNs + "DesignerProperty",
                            new XAttribute("Name", "TestProperty"),
                            new XAttribute("Value", Guid.NewGuid().ToString())))));

            var newValue = Guid.NewGuid();

            new EdmxHelper(edmx).UpdateDesignerOptionProperty("TestProperty", newValue);

            Assert.Equal(
                newValue,
                Guid.Parse((string)edmx.Descendants(edmxNs + "DesignerProperty").Single().Attribute("Value")));
        }

        [Fact]
        public void UpdateDesignerOptionProperty_creates_property_if_not_exists()
        {
            var edmx = XDocument.Parse(EdmxTemplate);
            var edmxNs = edmx.Root.Name.Namespace;

            var designerElement = edmx.Root.Element(edmxNs + "Designer");
            designerElement.Add(
                new XElement(edmxNs + "Options"));

            var newValue = Guid.NewGuid();

            new EdmxHelper(edmx).UpdateDesignerOptionProperty("TestProperty", newValue);

            Assert.Equal(
                newValue,
                Guid.Parse((string)edmx.Descendants(edmxNs + "DesignerProperty").Single().Attribute("Value")));
        }
    }
}
