// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Microsoft.VisualStudio.Shell.Interop;
    using Moq;
    using UnitTests.TestHelpers;
    using VSLangProj;
    using Xunit;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    public class EdmUtilsTests
    {
        [Fact]
        public void IsDataServicesEdmx_returns_false_for_invalid_or_non_existing_path()
        {
            Assert.False(EdmUtils.IsDataServicesEdmx((string)null));
            Assert.False(EdmUtils.IsDataServicesEdmx(string.Empty));
            Assert.False(EdmUtils.IsDataServicesEdmx(Guid.NewGuid().ToString()));
        }

        [Fact]
        public void IsDataServicesEdmx_returns_false_for_invalid_Xml_file()
        {
            Assert.False(EdmUtils.IsDataServicesEdmx(GetType().Assembly.Location));
        }

        [Fact]
        public void IsDataServicesEdmx_returns_true_for_known_data_services_edmx()
        {
            const string edmxTemplate = "<Edmx xmlns=\"{0}\"><DataServices /></Edmx>";

            foreach (var edmxNs in SchemaManager.GetEDMXNamespaceNames())
            {
                Assert.True(
                    EdmUtils.IsDataServicesEdmx(
                        XDocument.Parse(
                            string.Format(edmxTemplate, edmxNs))));
            }
        }

        [Fact]
        public void IsDataServicesEdmx_returns_false_for_no_data_services_edmx()
        {
            Assert.False(
                EdmUtils.IsDataServicesEdmx(XDocument.Parse("<Edmx xmlns=\"abc\"><DataServices /></Edmx>")));

            Assert.False(
                EdmUtils.IsDataServicesEdmx(
                    XDocument.Parse("<Edmx xmlns=\"http://schemas.microsoft.com/ado/2009/11/edmx\" />")));
        }

        [Fact]
        public void SafeLoadXmlFromString_throws_if_xml_contains_entities()
        {
            var message = Assert.Throws<XmlException>(
                () => EdmUtils.SafeLoadXmlFromString(
                    "<!ENTITY network \"network\">\n<entity-framework>&network;</entity-framework>")).Message;

            Assert.Contains("DTD", message);
            Assert.Contains("DtdProcessing", message);
            Assert.Contains("Parse", message);
        }

        [Fact]
        public void SafeLoadXmlFromString_can_load_xml_without_entities()
        {
            var xmlDoc = EdmUtils.SafeLoadXmlFromString("<entity-framework />");
            Assert.NotNull(xmlDoc);
            Assert.Equal("entity-framework", xmlDoc.DocumentElement.Name);
        }

        [Fact]
        public void SafeLoadXmlFromPath_throws_if_xml_contains_entities()
        {
            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, "<!ENTITY network \"network\">\n<entity-framework>&network;</entity-framework>");

            var message = Assert.Throws<XmlException>(
                () => EdmUtils.SafeLoadXmlFromPath(filePath)).Message;

            Assert.Contains("DTD", message);
            Assert.Contains("DtdProcessing", message);
            Assert.Contains("Parse", message);
        }

        [Fact]
        public void SafeLoadXmlFromPath_can_load_xml_without_entities()
        {
            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, "<entity-framework />");

            var xmlDoc = EdmUtils.SafeLoadXmlFromPath(filePath);
            Assert.NotNull(xmlDoc);
            Assert.Equal("entity-framework", xmlDoc.DocumentElement.Name);
        }

        [Fact]
        public void IsValidModelNamespace_returns_false_for_invalid_namespaces()
        {
            // the version does not matter since the definition 
            // of allowed strings for namespaces have not changed since v1
            Assert.False(EdmUtils.IsValidModelNamespace(null));
            Assert.False(EdmUtils.IsValidModelNamespace(string.Empty));
            Assert.False(EdmUtils.IsValidModelNamespace("\u0001\u0002"));
            Assert.False(EdmUtils.IsValidModelNamespace("<>"));
        }

        [Fact]
        public void IsValidModelNamespace_returns_true_for_valid_namespaces()
        {
            // the version does not matter since the definition 
            // of allowed strings for namespaces have not changed since v1
            Assert.True(EdmUtils.IsValidModelNamespace("abc"));
        }

        [Fact]
        public void ConstructUniqueNamespaces_returns_proposed_namespace_if_existing_namespaces_null()
        {
            Assert.Equal("testNamespace", EdmUtils.ConstructUniqueNamespace("testNamespace", null));
        }

        [Fact]
        public void ConstructUniqueNamespaces_returns_uniquified_namespace_()
        {
            Assert.Equal("Model1", EdmUtils.ConstructUniqueNamespace("Model", new HashSet<string> { "Model" }));
        }

        [Fact]
        public void ConstructValidModelNamespace_returns_proposed_namespace_if_valid()
        {
            Assert.Equal(
                "proposed",
                EdmUtils.ConstructValidModelNamespace("proposed", "default"));
        }

        [Fact]
        public void ConstructValidModelNamespace_returns_default_namespace_if_proposed_namespace_null_or_empty_string()
        {
            Assert.Equal(
                "default",
                EdmUtils.ConstructValidModelNamespace(null, "default"));

            Assert.Equal(
                "default",
                EdmUtils.ConstructValidModelNamespace(string.Empty, "default"));
        }

        [Fact]
        public void ConstructValidModelNamespace_returns_default_namespace_if_sanitized_proposed_namespace_invalid_or_empty_string()
        {
            Assert.Equal(
                "default",
                EdmUtils.ConstructValidModelNamespace("&", "default"));

            Assert.Equal(
                "default",
                EdmUtils.ConstructValidModelNamespace("&\u0001", "default"));

            Assert.Equal(
                "default",
                EdmUtils.ConstructValidModelNamespace("&123", "default"));

            Assert.Equal(
                "default",
                EdmUtils.ConstructValidModelNamespace("_a a", "default"));
        }

        [Fact]
        public void ConstructValidModelNamespace_returns_sanitized_proposed_namespace_if_sanitized_proposed_namespace_valid()
        {
            Assert.Equal(
                "proposed",
                EdmUtils.ConstructValidModelNamespace("<proposed>", "default"));

            Assert.Equal(
                "proposed",
                EdmUtils.ConstructValidModelNamespace("<123_proposed>", "default"));
        }

        [Fact]
        public void GetEntityFrameworkVersion_returns_null_when_misc_files()
        {
            var project = MockDTE.CreateMiscFilesProject();
            var serviceProvider = new Mock<IServiceProvider>();

            var schemaVersion = EdmUtils.GetEntityFrameworkVersion(project, serviceProvider.Object);

            Assert.Null(schemaVersion);
        }

        [Fact]
        public void GetEntityFrameworkVersion_returns_version_when_ef_installed()
        {
            var helper = new MockDTE(
                ".NETFramework,Version=v4.5", references: new[] { MockDTE.CreateReference("EntityFramework", "6.0.0.0") });

            var schemaVersion = EdmUtils.GetEntityFrameworkVersion(helper.Project, helper.ServiceProvider);

            Assert.Equal(EntityFrameworkVersion.Version3, schemaVersion);
        }

        [Fact]
        public void GetEntityFrameworkVersion_returns_latest_version_when_EF_dll_in_the_project_and_useLatestIfNoEf_true()
        {
            var netFxToSchemaVersionMapping =
                new[]
                    {
                        new KeyValuePair<string, Version>(".NETFramework,Version=v4.5", new Version(3, 0, 0, 0)),
                        new KeyValuePair<string, Version>(".NETFramework,Version=v4.0", new Version(3, 0, 0, 0)),
                        new KeyValuePair<string, Version>(".NETFramework,Version=v3.5", new Version(1, 0, 0, 0))
                    };

            foreach (var mapping in netFxToSchemaVersionMapping)
            {
                var helper = new MockDTE( /* .NET Framework Moniker */ mapping.Key, references: new Reference[0]);
                var schemaVersion = EdmUtils.GetEntityFrameworkVersion(helper.Project, helper.ServiceProvider, useLatestIfNoEF: true);
                Assert.Equal( /*expected schema version */ mapping.Value, schemaVersion);
            }
        }

        [Fact]
        public void
            GetEntityFrameworkVersion_returns_version_corresponding_to_net_framework_version_when_no_EF_dll_in_the_project_and_useLatestIfNoEf_false
            ()
        {
            var netFxToSchemaVersionMapping =
                new[]
                    {
                        new KeyValuePair<string, Version>(".NETFramework,Version=v4.5", new Version(3, 0, 0, 0)),
                        new KeyValuePair<string, Version>(".NETFramework,Version=v4.0", new Version(2, 0, 0, 0)),
                        new KeyValuePair<string, Version>(".NETFramework,Version=v3.5", new Version(1, 0, 0, 0))
                    };

            foreach (var mapping in netFxToSchemaVersionMapping)
            {
                var helper = new MockDTE( /* .NET Framework Moniker */ mapping.Key, references: new Reference[0]);
                var schemaVersion = EdmUtils.GetEntityFrameworkVersion(helper.Project, helper.ServiceProvider, useLatestIfNoEF: false);
                Assert.Equal( /*expected schema version */ mapping.Value, schemaVersion);
            }
        }

        [Fact]
        public void
            CreateUpdateCodeGenStrategyCommand_returns_UpdateDefaultableValueCommand_when_updating_CodeGenStrategy_value_from_empty_string()
        {
            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var entityDesignArtifactMock =
                new Mock<EntityDesignArtifact>(modelManager, new Uri("urn:dummy"), modelProvider);

            using (var designerInfoRoot = new EFDesignerInfoRoot(entityDesignArtifactMock.Object, new XElement("_")))
            {
                const string designerPropertyName = "CodeGenerationStrategy";
                designerInfoRoot
                    .AddDesignerInfo(
                        "Options",
                        SetupOptionsDesignerInfo(designerPropertyName, string.Empty));

                entityDesignArtifactMock
                    .Setup(a => a.DesignerInfo)
                    .Returns(designerInfoRoot);

                Assert.IsType<UpdateDefaultableValueCommand<string>>(
                    EdmUtils.SetCodeGenStrategyToNoneCommand(entityDesignArtifactMock.Object));
            }
        }

        [Fact]
        public void
            CreateUpdateCodeGenStrategyCommand_returns_UpdateDefaultableValueCommand_when_updating_CodeGenStrategy_value_from_non_empty_string
            ()
        {
            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var entityDesignArtifactMock =
                new Mock<EntityDesignArtifact>(modelManager, new Uri("urn:dummy"), modelProvider);

            using (var designerInfoRoot = new EFDesignerInfoRoot(entityDesignArtifactMock.Object, new XElement("_")))
            {
                const string designerPropertyName = "CodeGenerationStrategy";
                designerInfoRoot
                    .AddDesignerInfo(
                        "Options",
                        SetupOptionsDesignerInfo(designerPropertyName, "Default"));

                entityDesignArtifactMock
                    .Setup(a => a.DesignerInfo)
                    .Returns(designerInfoRoot);

                Assert.IsType<UpdateDefaultableValueCommand<string>>(
                    EdmUtils.SetCodeGenStrategyToNoneCommand(entityDesignArtifactMock.Object));
            }
        }

        [Fact]
        public void CreateUpdateCodeGenStrategyCommand_returns_null_when_attempting_to_update_CodeGenStrategy_to_same_value()
        {
            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var entityDesignArtifactMock =
                new Mock<EntityDesignArtifact>(modelManager, new Uri("urn:dummy"), modelProvider);

            using (var designerInfoRoot = new EFDesignerInfoRoot(entityDesignArtifactMock.Object, new XElement("_")))
            {
                const string designerPropertyName = "CodeGenerationStrategy";
                designerInfoRoot
                    .AddDesignerInfo(
                        "Options",
                        SetupOptionsDesignerInfo(designerPropertyName, "None"));

                entityDesignArtifactMock
                    .Setup(a => a.DesignerInfo)
                    .Returns(designerInfoRoot);

                Assert.Null(EdmUtils.SetCodeGenStrategyToNoneCommand(entityDesignArtifactMock.Object));
            }
        }

        private DesignerInfo SetupOptionsDesignerInfo(string designerPropertyName, string designerPropertyValue)
        {
            var designerInfo =
                new OptionsDesignerInfo(
                    null,
                    XElement.Parse(
                        "<Options xmlns='http://schemas.microsoft.com/ado/2009/11/edmx' />"));
            var designerInfoPropertySet =
                new DesignerInfoPropertySet(
                    designerInfo,
                    XElement.Parse(
                        "<DesignerInfoPropertySet xmlns='http://schemas.microsoft.com/ado/2009/11/edmx' />"));
            if (designerPropertyName != null)
            {
                var designerProperty =
                    new DesignerProperty(
                        designerInfoPropertySet,
                        XElement.Parse(
                            "<DesignerProperty Name='" + designerPropertyName + "' Value='" +
                            designerPropertyValue +
                            "' xmlns='http://schemas.microsoft.com/ado/2009/11/edmx' />"));
                designerInfoPropertySet.AddDesignerProperty(designerPropertyName, designerProperty);
            }

            designerInfo.PropertySet = designerInfoPropertySet;
            return designerInfo;
        }

        [Fact]
        public void UpdateConfigForSqlDbFileUpgrade_updates_and_saves_config()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(
                "<configuration>" +
                "  <connectionStrings>" +
                "    <add connectionString=\"Data source=.\\SQLExpress;AttachDbFilename=C:\\MyFolder\\MyDataFile.mdf;Database=dbname;Trusted_Connection=Yes;\" />" +
                "  </connectionStrings>" +
                "</configuration>");

            var mockConfigFileUtils = 
                new Mock<ConfigFileUtils>(Mock.Of<Project>(), Mock.Of<IServiceProvider>(), null, Mock.Of<IVsUtils>(), null);
            mockConfigFileUtils
                .Setup(u => u.LoadConfig())
                .Returns(xmlDoc);

            EdmUtils.UpdateConfigForSqlDbFileUpgrade(
                mockConfigFileUtils.Object, 
                Mock.Of<Project>(), 
                Mock.Of<IVsUpgradeLogger>());

            mockConfigFileUtils.Verify(u => u.SaveConfig(It.IsAny<XmlDocument>()), Times.Once());
        }

        [Fact]
        public void UpdateConfigForSqlDbFileUpgrade_does_not_save_config_if_content_not_loaded()
        {
            var mockConfigFileUtils = new Mock<ConfigFileUtils>(
                Mock.Of<Project>(), Mock.Of<IServiceProvider>(), null, Mock.Of<IVsUtils>(), null);

            EdmUtils.UpdateConfigForSqlDbFileUpgrade(
                mockConfigFileUtils.Object,
                Mock.Of<Project>(),
                Mock.Of<IVsUpgradeLogger>());

            mockConfigFileUtils.Verify(u => u.SaveConfig(It.IsAny<XmlDocument>()), Times.Never());
        }

        [Fact]
        public void UpdateConfigForSqlDbFileUpgrade_logs_exceptions()
        {
            var mockConfigFileUtils = new Mock<ConfigFileUtils>(
                Mock.Of<Project>(), Mock.Of<IServiceProvider>(), null, Mock.Of<IVsUtils>(), null);

            mockConfigFileUtils
                .Setup(u => u.LoadConfig())
                .Throws(new InvalidOperationException("Loading Failed"));

            var mockLogger = new Mock<IVsUpgradeLogger>();

            EdmUtils.UpdateConfigForSqlDbFileUpgrade(
                mockConfigFileUtils.Object,
                Mock.Of<Project>(),
                mockLogger.Object);

            var expectedErrorMessage =
                string.Format(Resources.ErrorDuringSqlDatabaseFileUpgrade, null, "Loading Failed");

            mockLogger
                .Verify(l => l.LogMessage(2, It.IsAny<string>(), It.IsAny<string>(), expectedErrorMessage), Times.Once());
        }
    }
}
