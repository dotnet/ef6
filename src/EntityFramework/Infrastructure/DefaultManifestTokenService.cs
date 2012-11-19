// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     A default implementation of <see cref="IManifestTokenService" /> that uses the
    ///     underlying provider to get the manifest token.
    /// </summary>
    public class DefaultManifestTokenService : IManifestTokenService
    {
        /// <inheritdoc />
        public string GetProviderManifestToken(DbConnection connection)
        {
            Check.NotNull(connection, "connection");

            return DbProviderServices.GetProviderServices(connection).GetProviderManifestTokenChecked(connection);
        }
    }
}
