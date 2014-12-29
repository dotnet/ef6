// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;

    internal static class DbProviderManifestExtensions
    {
        public static PrimitiveType GetStoreTypeFromName(this DbProviderManifest providerManifest, string name)
        {
            DebugCheck.NotNull(providerManifest);
            DebugCheck.NotEmpty(name);

            var primitiveType = providerManifest.GetStoreTypes()
                .SingleOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

            if (primitiveType == null)
            {
	            throw Error.StoreTypeNotFound(name, providerManifest.NamespaceName);
            }
            return primitiveType;
        }
    }
}
