// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Common;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Resources;

    internal static class DbProviderServicesExtensions
    {
        public static string GetProviderManifestTokenChecked(
            this DbProviderServices providerServices, DbConnection connection)
        {
            DebugCheck.NotNull(providerServices);
            DebugCheck.NotNull(connection);

            try
            {
                return providerServices.GetProviderManifestToken(connection);
            }
            catch (ProviderIncompatibleException ex)
            {
                throw new ProviderIncompatibleException(Strings.FailedToGetProviderInformation, ex);
            }
        }
    }
}
