// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.WrappingProvider
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Infrastructure;

    public class WrappingProviderFactoryService<TBase> : IDbProviderFactoryService
        where TBase : DbProviderFactory
    {
        public DbProviderFactory GetProviderFactory(DbConnection connection)
        {
            return connection is EntityConnection
                       ? (DbProviderFactory)EntityProviderFactory.Instance
                       : WrappingAdoNetProvider<TBase>.Instance;
        }
    }
}
