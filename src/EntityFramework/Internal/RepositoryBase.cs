// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Utilities;

    internal abstract class RepositoryBase
    {
        private readonly string _connectionString;
        private readonly DbProviderFactory _providerFactory;

        protected RepositoryBase(string connectionString, DbProviderFactory providerFactory)
        {
            DebugCheck.NotEmpty(connectionString);
            DebugCheck.NotNull(providerFactory);

            _connectionString = connectionString;
            _providerFactory = providerFactory;
        }

        protected DbConnection CreateConnection()
        {
            var connection = _providerFactory.CreateConnection();
            DbInterception.Dispatch.Connection.SetConnectionString(connection,
                new DbConnectionPropertyInterceptionContext<string>().WithValue(_connectionString));

            return connection;
        }
    }
}
