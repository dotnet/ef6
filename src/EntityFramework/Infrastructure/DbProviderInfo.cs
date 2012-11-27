// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Utilities;

    public sealed class DbProviderInfo
    {
        private readonly string _providerInvariantName;
        private readonly string _providerManifestToken;

        public DbProviderInfo(string providerInvariantName, string providerManifestToken)
        {
            Check.NotEmpty(providerInvariantName, "providerInvariantName");
            Check.NotEmpty(providerManifestToken, "providerManifestToken");

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
