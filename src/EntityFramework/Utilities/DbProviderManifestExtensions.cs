// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static class DbProviderManifestExtensions
    {
        public static PrimitiveType GetStoreTypeFromName(this DbProviderManifest providerManifest, string name)
        {
            Contract.Requires(providerManifest != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            return providerManifest.GetStoreTypes()
                .Single(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
