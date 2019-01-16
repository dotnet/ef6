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
        
        public static DbProviderInfo SqlAzure2012_ProviderInfo
        {
            get { return new DbProviderInfo("System.Data.SqlClient", "2012.Azure"); }
        }

        public static DbProviderManifest SqlAzure2012_ProviderManifest
        {
            get { return DbProviderServices.GetProviderServices(new SqlConnection()).GetProviderManifest("2012.Azure"); }
        }      

        public static DbProviderInfo SqlCe4_ProviderInfo
        {
            get { return new DbProviderInfo("System.Data.SqlServerCe.4.0", "4.0"); }
        }

        public static DbProviderManifest SqlCe4_ProviderManifest
        {
            get
            {
                return DbProviderServices.GetProviderServices(
                    new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0")
                        .CreateConnection("foo"))
                                         .GetProviderManifest("4.0");
            }
        }
    }
}
