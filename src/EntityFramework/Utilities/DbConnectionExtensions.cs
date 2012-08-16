// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Common;
    using System.Data.Entity.Config;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    internal static class DbConnectionExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static string GetProviderInvariantName(this DbConnection connection)
        {
            Contract.Requires(connection != null);

            return DbProviderServices.GetProviderFactory(connection).GetProviderInvariantName();
        }

        public static DbProviderInfo GetProviderInfo(
            this DbConnection connection, out DbProviderManifest providerManifest)
        {
            Contract.Requires(connection != null);

            var providerManifestToken = DbConfiguration
                .GetService<IManifestTokenService>()
                .GetProviderManifestToken(connection);
            
            var providerInfo = new DbProviderInfo(connection.GetProviderInvariantName(), providerManifestToken);

            providerManifest = DbProviderServices.GetProviderServices(connection).GetProviderManifest(providerManifestToken);

            return providerInfo;
        }
    }
}
