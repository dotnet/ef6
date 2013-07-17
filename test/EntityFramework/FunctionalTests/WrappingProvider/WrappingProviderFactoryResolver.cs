// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.WrappingProvider
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Infrastructure;

    public class WrappingProviderFactoryResolver<TBase> : IDbProviderFactoryResolver
        where TBase : DbProviderFactory
    {
        public DbProviderFactory ResolveProviderFactory(DbConnection connection)
        {
            return connection is EntityConnection
                       ? (DbProviderFactory)EntityProviderFactory.Instance
                       : WrappingAdoNetProvider<TBase>.Instance;
        }
    }
}
