// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Diagnostics;
    using System.Xml;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal sealed class SsdlProviderAttributesHandler : MetadataConverterHandler
    {
        private const string SsdlSchemaXPath = "/edmx:Edmx/edmx:Runtime/edmx:StorageModels/ssdl:Schema";
        private const string ProviderValueToBeConverted = "System.Data.SqlServerCe.3.5";
        private const string ProviderNewValue = "System.Data.SqlServerCe.4.0";
        private const string ProviderManifestTokenValueToBeConverted = "3.5";
        private const string ProviderManifestTokenNewValue = "4.0";

        private readonly Version _sourceSchemaVersion;

        internal SsdlProviderAttributesHandler(Version sourceSchemaVersion)
        {
            _sourceSchemaVersion = sourceSchemaVersion;
        }

        /// <summary>
        ///     Update the Provider and ProviderManifestToken attributes in the file from the 3.5 versions to the 4.0 ones
        /// </summary>
        protected override XmlDocument DoHandleConversion(XmlDocument doc)
        {
            Debug.Assert(doc != null, "doc != null");

            var nsMgr = SchemaManager.GetEdmxNamespaceManager(doc.NameTable, _sourceSchemaVersion);
            foreach (XmlElement element in doc.SelectNodes(SsdlSchemaXPath, nsMgr))
            {
                // update Provider attribute
                var providerValue = element.GetAttribute("Provider");
                if (ProviderValueToBeConverted.Equals(providerValue, StringComparison.OrdinalIgnoreCase))
                {
                    element.SetAttribute("Provider", ProviderNewValue);

                    // update ProviderManifestToken attribute
                    var providerManifestTokenValue = element.GetAttribute("ProviderManifestToken");

                    if (ProviderManifestTokenValueToBeConverted.Equals(
                        providerManifestTokenValue, StringComparison.OrdinalIgnoreCase))
                    {
                        element.SetAttribute("ProviderManifestToken", ProviderManifestTokenNewValue);
                    }
                }
            }

            return doc;
        }
    }
}
