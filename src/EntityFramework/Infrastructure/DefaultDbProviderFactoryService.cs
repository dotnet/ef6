// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Utilities;

    internal class DefaultDbProviderFactoryService : IDbProviderFactoryService
    {
        public DbProviderFactory GetProviderFactory(DbConnection connection)
        {
            Check.NotNull(connection, "connection");

            return DbProviderFactories.GetFactory(connection);
        }
    }
}

#endif
