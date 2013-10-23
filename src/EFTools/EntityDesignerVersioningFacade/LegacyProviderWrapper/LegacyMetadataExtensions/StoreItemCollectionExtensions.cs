// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using LegacyMetadata = System.Data.Metadata.Edm;

namespace Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper.LegacyMetadataExtensions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.IO;
    using System.Xml;
    using Microsoft.Data.Entity.Design.VersioningFacade.Metadata;

    internal static class StoreItemCollectionExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static LegacyMetadata.StoreItemCollection ToLegacyStoreItemCollection(
            this StoreItemCollection storeItemCollection, string schemaNamespaceName = null)
        {
            Debug.Assert(storeItemCollection != null, "storeItemCollection != null");

            using (var ms = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(ms))
                {
                    storeItemCollection.WriteSsdl(writer, schemaNamespaceName);
                }

                ms.Position = 0;

                using (var reader = XmlReader.Create(ms))
                {
                    return new LegacyMetadata.StoreItemCollection(new[] { reader });
                }
            }
        }
    }
}
