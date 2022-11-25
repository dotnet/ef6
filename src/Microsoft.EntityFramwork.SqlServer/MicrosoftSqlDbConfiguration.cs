namespace System.Data.Entity.SqlServer
{
    /// <summary>
    /// Default configuration.
    /// </summary>
    public class MicrosoftSqlDbConfiguration : DbConfiguration
    {
        /// <summary>
        /// Default configuration, used for code based configuration of this provider.
        /// </summary>
        public MicrosoftSqlDbConfiguration()
        {
            SetProviderFactory(MicrosoftSqlProviderServices.ProviderInvariantName, Microsoft.Data.SqlClient.SqlClientFactory.Instance);
            SetProviderServices(MicrosoftSqlProviderServices.ProviderInvariantName, MicrosoftSqlProviderServices.Instance);
        }
    }
}
