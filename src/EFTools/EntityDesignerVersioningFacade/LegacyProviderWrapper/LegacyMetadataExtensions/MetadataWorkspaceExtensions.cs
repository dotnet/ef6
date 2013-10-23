// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using LegacyMapping = System.Data.Mapping;
using LegacyMetadata = System.Data.Metadata.Edm;

namespace Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper.LegacyMetadataExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml;

    internal static class MetadataWorkspaceExtensions
    {
        private static readonly IDictionary<Version, LegacyMetadata.EdmItemCollection> LegacyEdmItemCollections =
            new Dictionary<Version, LegacyMetadata.EdmItemCollection>();

        private const string MslTemplate =
            @"<Mapping Space=""C-S"" xmlns=""{0}"">" +
            @"  <EntityContainerMapping StorageEntityContainer=""{1}"" CdmEntityContainer=""DummyContainer"" />" +
            @"</Mapping>";

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static LegacyMetadata.MetadataWorkspace ToLegacyMetadataWorkspace(this MetadataWorkspace metadataWorkspace)
        {
            Debug.Assert(metadataWorkspace != null, "metadataWorkspace != null");

            // The cloned workspace is supposed to be used only by provider. Therefore we only care about SSpace.
            // For CSpace and C-S mapping we register dummy item collections just to make the workspace checks pass.
            var legacyStoreItemCollection =
                ((StoreItemCollection)metadataWorkspace.GetItemCollection(DataSpace.SSpace)).ToLegacyStoreItemCollection();

            var version = EntityFrameworkVersion.DoubleToVersion(legacyStoreItemCollection.StoreSchemaVersion);

            var legacyEdmItemCollection = GetLegacyEdmItemCollection(version);

            var legacyWorkspace = new LegacyMetadata.MetadataWorkspace();
            legacyWorkspace.RegisterItemCollection(legacyEdmItemCollection);
            legacyWorkspace.RegisterItemCollection(legacyStoreItemCollection);

            var msl = string.Format(
                CultureInfo.InvariantCulture,
                MslTemplate,
                SchemaManager.GetMSLNamespaceName(version),
                legacyStoreItemCollection.GetItems<LegacyMetadata.EntityContainer>().Single().Name);

            using (var stringReader = new StringReader(msl))
            {
                using (var reader = XmlReader.Create(stringReader))
                {
                    var legacyMappingItemCollection =
                        new LegacyMapping.StorageMappingItemCollection(
                            legacyEdmItemCollection,
                            legacyStoreItemCollection,
                            new[] { reader });

                    legacyWorkspace.RegisterItemCollection(legacyMappingItemCollection);
                }
            }

            return legacyWorkspace;
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private static LegacyMetadata.EdmItemCollection GetLegacyEdmItemCollection(Version version)
        {
            const string csdlTemplate =
                @"<Schema xmlns=""{0}"" Namespace=""dummy"">" +
                @"    <EntityContainer Name=""DummyContainer""/>" +
                @"</Schema>";

            LegacyMetadata.EdmItemCollection itemCollection;

            if (!LegacyEdmItemCollections.TryGetValue(version, out itemCollection))
            {
                var csdl = string.Format(CultureInfo.InvariantCulture,
                    csdlTemplate, SchemaManager.GetCSDLNamespaceName(version));

                using (var stringReader = new StringReader(csdl))
                {
                    using (var reader = XmlReader.Create(stringReader))
                    {
                        itemCollection = new LegacyMetadata.EdmItemCollection(new[] { reader });
                        LegacyEdmItemCollections[version] = itemCollection;
                    }
                }
            }

            return itemCollection;
        }
    }
}
