// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Utilities;

    internal abstract class RepositoryBase
    {
        private readonly InternalContext _usersContext;
        private readonly string _connectionString;
        private readonly DbProviderFactory _providerFactory;

        protected RepositoryBase(InternalContext usersContext, string connectionString, DbProviderFactory providerFactory)
        {
            DebugCheck.NotNull(usersContext);
            DebugCheck.NotEmpty(connectionString);
            DebugCheck.NotNull(providerFactory);

            _usersContext = usersContext;
            _connectionString = connectionString;
            _providerFactory = providerFactory;
        }

        protected DbConnection CreateConnection()
        {
            DbConnection connection;
            if (!_usersContext.IsDisposed
                && (connection = _usersContext.Connection) != null)
            {
                if (connection.State == ConnectionState.Open)
                {
                    return connection;
                }

                connection = DbProviderServices.GetProviderServices(connection)
                    .CloneDbConnection(connection, _providerFactory);
            }
            else
            {
                connection = _providerFactory.CreateConnection();
            }

            DbInterception.Dispatch.Connection.SetConnectionString(connection,
                new DbConnectionPropertyInterceptionContext<string>().WithValue(_connectionString));

            return connection;
        }

        protected void DisposeConnection(DbConnection connection)
        {
            if (connection != null
                && (_usersContext.IsDisposed || connection != _usersContext.Connection))
            {
                DbInterception.Dispatch.Connection.Dispose(connection, new DbInterceptionContext());
            }
        }
    }
}
