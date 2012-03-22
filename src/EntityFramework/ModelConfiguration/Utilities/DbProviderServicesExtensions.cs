namespace System.Data.Entity.ModelConfiguration.Utilities
{
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;

    internal static class DbProviderServicesExtensions
    {
        public static string GetProviderManifestTokenChecked(
            this DbProviderServices providerServices, DbConnection connection)
        {
            Contract.Assert(providerServices != null);
            Contract.Assert(connection != null);

            try
            {
                return providerServices.GetProviderManifestToken(connection);
            }
            catch (ProviderIncompatibleException ex)
            {
                if ("(localdb)\v11.0".Equals(connection.DataSource, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ProviderIncompatibleException(Strings.BadLocalDBDatabaseName, ex);
                }

                throw new ProviderIncompatibleException(Strings.FailedToGetProviderInformation, ex);
            }
        }
    }
}
