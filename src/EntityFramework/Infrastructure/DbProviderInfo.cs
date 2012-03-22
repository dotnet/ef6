namespace System.Data.Entity.Infrastructure
{
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbProviderInfo
    {
        private readonly string _providerInvariantName;
        private readonly string _providerManifestToken;

        public DbProviderInfo(string providerInvariantName, string providerManifestToken)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(providerInvariantName));
            Contract.Requires(!string.IsNullOrWhiteSpace(providerManifestToken));

            _providerInvariantName = providerInvariantName;
            _providerManifestToken = providerManifestToken;
        }

        public string ProviderInvariantName
        {
            get { return _providerInvariantName; }
        }

        public string ProviderManifestToken
        {
            get { return _providerManifestToken; }
        }
    }
}
