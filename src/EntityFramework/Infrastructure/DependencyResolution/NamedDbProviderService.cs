// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Utilities;

    internal class NamedDbProviderService
    {
        private readonly string _invariantName;
        private readonly DbProviderServices _providerServices;

        public NamedDbProviderService(string invariantName, DbProviderServices providerServices)
        {
            DebugCheck.NotEmpty(invariantName);
            DebugCheck.NotNull(providerServices);

            _invariantName = invariantName;
            _providerServices = providerServices;
        }

        public string InvariantName
        {
            get { return _invariantName; }
        }

        public DbProviderServices ProviderServices
        {
            get { return _providerServices; }
        }
    }
}
