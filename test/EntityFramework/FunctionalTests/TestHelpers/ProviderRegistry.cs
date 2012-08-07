// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;

    public static class ProviderRegistry
    {
        public static DbProviderInfo Sql2008_ProviderInfo
        {
            get { return new DbProviderInfo("System.Data.SqlClient", "2008"); }
        }

        public static DbProviderManifest Sql2008_ProviderManifest
        {
            get { return DbProviderServices.GetProviderServices(new SqlConnection()).GetProviderManifest("2008"); }
        }

        public static DbProviderInfo SqlCe4_ProviderInfo
        {
            get { return new DbProviderInfo("System.Data.SqlServerCe.4.0", "4.0"); }
        }

        public static DbProviderInfo SqlCe35_ProviderInfo
        {
            get { return new DbProviderInfo("System.Data.SqlServerCe.3.5", "3.5"); }
        }
    }
}
