// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Common;
    using System.Data.Entity.Config;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    internal static class DbConnectionExtensions
    {
#if NET40

        private static readonly MethodInfo _getFactoryMethod = typeof(DbProviderFactories)
            .GetMethod(
                "GetFactory",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(DbConnection) },
                null);

#endif

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

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static DbProviderFactory GetProviderFactory(this DbConnection connection)
        {
            Contract.Requires(connection != null);

#if NET40
            // TODO: Use non-reflective mechanism here.
            return (DbProviderFactory)_getFactoryMethod.Invoke(null, new object[] { connection });
#else
            return DbProviderFactories.GetFactory(connection);
#endif
        }
    }
}
