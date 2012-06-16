namespace System.Data.Entity.Utilities
{
    using System.Data.Common;
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

            var providerServices = DbProviderServices.GetProviderServices(connection);
            var providerManifestToken = providerServices.GetProviderManifestTokenChecked(connection);
            var providerInfo = new DbProviderInfo(connection.GetProviderInvariantName(), providerManifestToken);

            providerManifest = providerServices.GetProviderManifest(providerManifestToken);

            return providerInfo;
        }
    }
}
