// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Configuration
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;

    public class SqlClientConfiguration : DbConfiguration
    {
        public SqlClientConfiguration()
        {
            SetDefaultConnectionFactory(new SqlConnectionFactory());
        }
    }
}
