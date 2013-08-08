// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Internal.UnitTests
{
    using System.Data.Common;

    /// <summary>
    /// A DbConnection that doesn't work but providers just enough information that Code First can generate
    /// an SSDL without having to hit a real database.
    /// </summary>
    public class FakeSqlConnection : DbConnection
    {
        private readonly DbProviderFactory _factory;
        private string _dataSource;
        private string _database;

        public FakeSqlConnection()
            : this("2008")
        {
        }

        public FakeSqlConnection(string manifestToken, DbProviderFactory factory = null)
        {
            ManifestToken = manifestToken;
            _factory = factory ?? FakeSqlProviderFactory.Instance;
        }

        protected override DbProviderFactory DbProviderFactory
        {
            get { return _factory; }
        }

        public override string ConnectionString { get; set; }

        public override string DataSource
        {
            get { return _dataSource; }
        }

        public void SetDataSource(string dataSource)
        {
            _dataSource = dataSource;
        }

        public override string Database
        {
            get { return _database; }
        }

        public void SetDatabase(string database)
        {
            _database = database;
        }

        public override string ServerVersion
        {
            get { throw new NotImplementedException(); }
        }

        public override ConnectionState State
        {
            get { return ConnectionState.Closed; }
        }

        public string ManifestToken { get; set; }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }

        public override void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            throw new NotImplementedException();
        }

        protected override DbCommand CreateDbCommand()
        {
            throw new NotImplementedException();
        }

        public override void Open()
        {
            throw new NotImplementedException();
        }
    }
}
