// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Xml;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Xunit;

    public class NamespaceConverterHandlerTests
    {
        [Fact]
        public void NamespaceConverterHandlerTests_rewrites_document_to_use_namespaces_for_target_schema_version()
        {
            for (var sourceMajorVersion = 1; sourceMajorVersion <= 3; sourceMajorVersion++)
            {
                for (var targetMajorVersion = 1; targetMajorVersion <= 3; targetMajorVersion++)
                {
                    var sourceSchemaVersion = new Version(sourceMajorVersion, 0, 0, 0);
                    var targetSchemaVersion = new Version(targetMajorVersion, 0, 0, 0);

                    var sourceEdmx = CreateSourceEdmx(sourceSchemaVersion);

                    Assert.True(
                        UsesNamespacesForTargetSchemaVersion(
                            new NamespaceConverterHandler(sourceSchemaVersion, targetSchemaVersion).HandleConversion(sourceEdmx),
                            targetSchemaVersion));
                }
            }
        }

        private static XmlDocument CreateSourceEdmx(Version schemaVersion)
        {
            const string template =
                "<Edmx xmlns=\"{0}\">" +
                "  <Runtime>" +
                "    <StorageModels>" +
                "      <Schema xmlns=\"{1}\" />" +
                "    </StorageModels>" +
                "    <ConceptualModels>" +
                "      <Schema xmlns=\"{2}\" />" +
                "    </ConceptualModels>" +
                "    <Mappings>" +
                "      <Mapping xmlns=\"{3}\" />" +
                "    </Mappings>" +
                "  </Runtime>" +
                "  <Designer/>" +
                "</Edmx>";

            var edmx = new XmlDocument();
            edmx.LoadXml(
                string.Format(
                    template,
                    SchemaManager.GetEDMXNamespaceName(schemaVersion),
                    SchemaManager.GetSSDLNamespaceName(schemaVersion),
                    SchemaManager.GetCSDLNamespaceName(schemaVersion),
                    SchemaManager.GetMSLNamespaceName(schemaVersion)));

            return edmx;
        }

        private static bool UsesNamespacesForTargetSchemaVersion(XmlDocument edmx, Version targetSchemaVersion)
        {
            var nsMgr = SchemaManager.GetEdmxNamespaceManager(edmx.NameTable, targetSchemaVersion);

            return
                edmx.SelectSingleNode("/edmx:Edmx/edmx:Runtime/edmx:StorageModels/ssdl:Schema", nsMgr) != null &&
                edmx.SelectSingleNode("/edmx:Edmx/edmx:Runtime/edmx:ConceptualModels/csdl:Schema", nsMgr) != null &&
                edmx.SelectSingleNode("/edmx:Edmx/edmx:Runtime/edmx:Mappings/msl:Mapping", nsMgr) != null &&
                edmx.SelectSingleNode("/edmx:Edmx/edmx:Designer", nsMgr) != null;
        }
    }
}
