// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Common;
    using System.Data.Entity.Config;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.CodeAnalysis;

    internal static class DbConnectionExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static string GetProviderInvariantName(this DbConnection connection)
        {
            DebugCheck.NotNull(connection);

            return DbProviderServices.GetProviderFactory(connection).GetProviderInvariantName();
        }

        public static DbProviderInfo GetProviderInfo(
            this DbConnection connection, out DbProviderManifest providerManifest)
        {
            DebugCheck.NotNull(connection);

            var providerManifestToken = DbConfiguration
                .GetService<IManifestTokenService>()
                .GetProviderManifestToken(connection);

            var providerInfo = new DbProviderInfo(connection.GetProviderInvariantName(), providerManifestToken);

            providerManifest = DbProviderServices.GetProviderServices(connection).GetProviderManifest(providerManifestToken);

            return providerInfo;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static DbProviderFactory GetProviderFactory(this DbConnection connection)
        {
            DebugCheck.NotNull(connection);

            return DbConfiguration.GetService<IDbProviderFactoryService>().GetProviderFactory(connection);
        }
    }
}
