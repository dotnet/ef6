// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;

    public class FunctionalTestsManifestTokenResolver : IManifestTokenResolver
    {
        private static readonly DefaultManifestTokenResolver _defaultManifestTokenResolver = new DefaultManifestTokenResolver();

        public string ResolveManifestToken(DbConnection connection)
        {
            return (!string.IsNullOrWhiteSpace(connection.Database) // Some negative cases require the provider to fail
                   && connection is SqlConnection)
                   || connection.GetType().FullName.StartsWith("Castle.Proxies.")
                       ? "2008"
                       : _defaultManifestTokenResolver.ResolveManifestToken(connection);
        }
    }
}
