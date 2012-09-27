// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Edm;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Xml.Linq;

    internal static class XDocumentExtensions
    {
        public static StoreItemCollection GetStoreItemCollection(this XDocument model, out DbProviderInfo providerInfo)
        {
            Contract.Requires(model != null);

            var schemaElement = model.Descendants(EdmXNames.Ssdl.SchemaNames).Single();

            providerInfo = new DbProviderInfo(
                schemaElement.ProviderAttribute(),
                schemaElement.ProviderManifestTokenAttribute());

            return new StoreItemCollection(new[] { schemaElement.CreateReader() });
        }

        public static bool HasSystemOperations(this XDocument model)
        {
            Contract.Requires(model != null);

            return model.Descendants().Attributes(EdmXNames.IsSystemName).Any();
        }
    }
}
