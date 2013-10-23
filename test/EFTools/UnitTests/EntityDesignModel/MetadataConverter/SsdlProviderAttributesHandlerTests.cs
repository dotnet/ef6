// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Xml;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Xunit;

    public class SsdlProviderAttributesHandlerTests
    {
        private const string EdmxTemplate =
            "<Edmx xmlns=\"{0}\">" +
            "  <Runtime>" +
            "    <StorageModels>" +
            "      <Schema xmlns=\"{1}\" Provider=\"{2}\" ProviderManifestToken=\"{3}\" />" +
            "    </StorageModels>" +
            "  </Runtime>" +
            "</Edmx>";

        [Fact]
        public void SsdlProviderAttributesHandler_updates_provider_invariant_name_and_manifest_token_for_SqlCE()
        {
            for (var i = 1; i <= 3; i++)
            {
                var schemaVersion = new Version(i, 0, 0, 0);
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(
                    string.Format(
                        EdmxTemplate,
                        SchemaManager.GetEDMXNamespaceName(schemaVersion),
                        SchemaManager.GetSSDLNamespaceName(schemaVersion),
                        "System.Data.SqlServerCe.3.5",
                        "3.5"));

                new SsdlProviderAttributesHandler(schemaVersion).HandleConversion(xmlDoc);

                Assert.Equal(
                    "System.Data.SqlServerCe.4.0",
                    xmlDoc.SelectSingleNode("//*[local-name() = 'Schema']/@Provider").Value);

                Assert.Equal(
                    "4.0",
                    xmlDoc.SelectSingleNode("//*[local-name() = 'Schema']/@ProviderManifestToken").Value);
            }
        }

        [Fact]
        public void SsdlProviderAttributesHandler_does_not_modify_manifest_token_for_non_SqlCE()
        {
            var schemaVersion = new Version(1, 0, 0, 0);
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(
                string.Format(
                    EdmxTemplate,
                    SchemaManager.GetEDMXNamespaceName(schemaVersion),
                    SchemaManager.GetSSDLNamespaceName(schemaVersion),
                    "MyProvider",
                    "3.5"));

            new SsdlProviderAttributesHandler(schemaVersion).HandleConversion(xmlDoc);

            Assert.Equal(
                "MyProvider",
                xmlDoc.SelectSingleNode("//*[local-name() = 'Schema']/@Provider").Value);

            Assert.Equal(
                "3.5",
                xmlDoc.SelectSingleNode("//*[local-name() = 'Schema']/@ProviderManifestToken").Value);
        }

        [Fact]
        public void SsdlProviderAttributesHandler_does_not_modify_manifest_token_for_SqlCE_if_not_3_5()
        {
            var schemaVersion = new Version(1, 0, 0, 0);
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(
                string.Format(
                    EdmxTemplate,
                    SchemaManager.GetEDMXNamespaceName(schemaVersion),
                    SchemaManager.GetSSDLNamespaceName(schemaVersion),
                    "System.Data.SqlServerCe.3.5",
                    "17.0"));

            new SsdlProviderAttributesHandler(schemaVersion).HandleConversion(xmlDoc);

            Assert.Equal(
                "System.Data.SqlServerCe.4.0",
                xmlDoc.SelectSingleNode("//*[local-name() = 'Schema']/@Provider").Value);

            Assert.Equal(
                "17.0",
                xmlDoc.SelectSingleNode("//*[local-name() = 'Schema']/@ProviderManifestToken").Value);
        }
    }
}
