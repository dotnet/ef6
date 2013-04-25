// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Edm;
    using System.Linq;
    using System.Xml.Linq;

    internal static class XDocumentExtensions
    {
        public static StorageMappingItemCollection GetStorageMappingItemCollection(
            this XDocument model, out DbProviderInfo providerInfo)
        {
            DebugCheck.NotNull(model);

            var edmItemCollection
                = new EdmItemCollection(
                    new[]
                        {
                            model.Descendants(EdmXNames.Csdl.SchemaNames).Single().CreateReader()
                        });

            var ssdlSchemaElement = model.Descendants(EdmXNames.Ssdl.SchemaNames).Single();

            providerInfo = new DbProviderInfo(
                ssdlSchemaElement.ProviderAttribute(),
                ssdlSchemaElement.ProviderManifestTokenAttribute());

            var storeItemCollection
                = new StoreItemCollection(
                    new[]
                        {
                            ssdlSchemaElement.CreateReader()
                        });

            return new StorageMappingItemCollection(
                edmItemCollection,
                storeItemCollection,
                new[] { new XElement(model.Descendants(EdmXNames.Msl.MappingNames).Single()).CreateReader() });
        }
    }
}
