namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Internal.ConfigFile;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal class ProviderConfig
    {
        private readonly EntityFrameworkSection _entityFrameworkSettings;

        public ProviderConfig(EntityFrameworkSection entityFrameworkSettings)
        {
            _entityFrameworkSettings = entityFrameworkSettings;
        }

        public string TryGetDbProviderServicesTypeName(string providerInvariantName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(providerInvariantName));

            var providerElement =
                _entityFrameworkSettings.Providers.OfType<ProviderElement>().FirstOrDefault(
                    e => providerInvariantName.Equals(e.InvariantName, StringComparison.OrdinalIgnoreCase));
         
            return providerElement == null ? null : providerElement.ProviderTypeName;
        }
    }
}
