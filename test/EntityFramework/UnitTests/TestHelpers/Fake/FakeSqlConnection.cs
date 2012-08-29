// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Internal.UnitTests
{
    using System;
    using System.Data.Entity.Core;
    using System.Data;
    using System.Data.Entity.Core.Common;
    using System.Data.Common;

    /// <summary>
    ///     A DbConnection that doesn't work but providers just enough information that Code First can generate
    ///     an SSDL without having to hit a real database.
    /// </summary>
    public class FakeSqlConnection : DbConnection
    {
        private readonly string _manifestToken;
        private readonly DbProviderFactory _factory;

        public FakeSqlConnection(string manifestToken = "2008", DbProviderFactory factory = null)
        {
            _manifestToken = manifestToken;
            _factory = factory ?? FakeSqlProviderFactory.Instance;
        }

        protected override DbProviderFactory DbProviderFactory
        {
            get { return _factory; }
        }

        public override string ConnectionString { get; set; }

        public override string DataSource
        {
            get { return null; }
        }

        public override string Database
        {
            get { return null; }
        }

        public override string ServerVersion
        {
            get { throw new NotImplementedException(); }
        }

        public override ConnectionState State
        {
            get { return ConnectionState.Closed; }
        }

        public string ManifestToken
        {
            get { return _manifestToken; }
        }

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
