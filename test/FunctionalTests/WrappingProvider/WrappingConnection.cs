// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.WrappingProvider
{
    using System.Data.Common;

    public class WrappingConnection<TBase> : DbConnection, IDisposable where TBase : DbProviderFactory
    {
        private DbConnection _baseConnection;

        public WrappingConnection(DbConnection baseConnection)
        {
            _baseConnection = baseConnection;
            _baseConnection.StateChange += OnBaseStateChange;
        }

        private void OnBaseStateChange(object sender, StateChangeEventArgs stateChangeEventArgs)
        {
            OnStateChange(stateChangeEventArgs);
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            WrappingAdoNetProvider<TBase>.Instance.Log.Add(new LogItem("BeginTransaction", this, isolationLevel));

            return new WrappingTransaction<TBase>(_baseConnection.BeginTransaction(isolationLevel));
        }

        public override void Close()
        {
            WrappingAdoNetProvider<TBase>.Instance.Log.Add(new LogItem("Close", this, ""));

            _baseConnection.Close();
        }

        public override void ChangeDatabase(string databaseName)
        {
            _baseConnection.ChangeDatabase(databaseName);
        }

        public override void Open()
        {
            WrappingAdoNetProvider<TBase>.Instance.Log.Add(new LogItem("Open", this, ""));

            _baseConnection.Open();
        }

        public override string ConnectionString
        {
            get { return _baseConnection.ConnectionString; }
            set { _baseConnection.ConnectionString = value; }
        }

        public override string Database
        {
            get { return _baseConnection.Database; }
        }

        public override ConnectionState State
        {
            get { return _baseConnection.State; }
        }

        public override string DataSource
        {
            get { return _baseConnection.DataSource; }
        }

        public override string ServerVersion
        {
            get { return _baseConnection.ServerVersion; }
        }

        protected override DbCommand CreateDbCommand()
        {
            return new WrappingCommand<TBase>(_baseConnection.CreateCommand());
        }

        protected override DbProviderFactory DbProviderFactory
        {
            get { return WrappingAdoNetProvider<TBase>.Instance; }
        }

        public DbConnection BaseConnection
        {
            get { return _baseConnection; }
        }

        public new void Dispose()
        {
            _baseConnection.Dispose();
            _baseConnection = null;
        }
    }
}
