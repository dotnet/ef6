// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Utilities;

    internal abstract class RepositoryBase
    {
        private readonly DbConnection _existingConnection;
        private readonly string _connectionString;
        private readonly DbProviderFactory _providerFactory;

        protected RepositoryBase(InternalContext usersContext, string connectionString, DbProviderFactory providerFactory)
        {
            DebugCheck.NotNull(usersContext);
            DebugCheck.NotEmpty(connectionString);
            DebugCheck.NotNull(providerFactory);

            var existingConnection = usersContext.Connection;
            if (existingConnection != null)
            {
                _existingConnection = existingConnection;
            }

            _connectionString = connectionString;
            _providerFactory = providerFactory;
        }

        protected DbConnection CreateConnection()
        {
            if (_existingConnection != null
                && _existingConnection.State == ConnectionState.Open)
            {
                return _existingConnection;
            }

            var connection = _existingConnection == null
                ? _providerFactory.CreateConnection()
                : DbProviderServices.GetProviderServices(_existingConnection).CloneDbConnection(_existingConnection, _providerFactory);
            DbInterception.Dispatch.Connection.SetConnectionString(connection,
                new DbConnectionPropertyInterceptionContext<string>().WithValue(_connectionString));

            return connection;
        }

        protected void DisposeConnection(DbConnection connection)
        {
            if (connection != null && _existingConnection == null)
            {
                DbInterception.Dispatch.Connection.Dispose(connection, new DbInterceptionContext());
            }
        }
    }
}
