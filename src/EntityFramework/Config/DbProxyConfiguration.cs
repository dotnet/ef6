namespace System.Data.Entity.Config
{
    public abstract class DbProxyConfiguration : DbConfiguration
    {
        public abstract Type ConfigurationToUse();
    }
}