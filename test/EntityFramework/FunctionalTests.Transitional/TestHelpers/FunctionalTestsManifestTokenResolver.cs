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
            if (!string.IsNullOrWhiteSpace(connection.Database) // Some negative cases require the provider to fail
                   && connection is SqlConnection || connection.GetType().FullName.StartsWith("Castle.Proxies."))
            {
                if (connection.Database.EndsWith("_2012"))
                    return "2012";

                return (DatabaseTestHelpers.IsSqlAzure(connection.ConnectionString)) ? "2012.Azure" : "2008";
            }
            return _defaultManifestTokenResolver.ResolveManifestToken(connection);
        }
    }
}
