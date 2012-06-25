namespace System.Data.Entity.Config
{
    using System.Data.Entity.Internal;

    internal class DbConfigurationLoader
    {
        public virtual DbConfiguration TryLoadFromConfig(AppConfig config)
        {
            // TODO: Implement loading from app.config
            return null;
        }
    }
}