namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Core.Common;
    using System.Data.Common;
    using System.Diagnostics.Contracts;

    internal abstract class RepositoryBase
    {
        private readonly string _connectionString;
        private readonly DbProviderFactory _providerFactory;

        protected RepositoryBase(string connectionString, DbProviderFactory providerFactory)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(connectionString));
            Contract.Requires(providerFactory != null);

            _connectionString = connectionString;
            _providerFactory = providerFactory;
        }

        protected DbConnection CreateConnection()
        {
            var connection = _providerFactory.CreateConnection();
            connection.ConnectionString = _connectionString;

            return connection;
        }
    }
}
