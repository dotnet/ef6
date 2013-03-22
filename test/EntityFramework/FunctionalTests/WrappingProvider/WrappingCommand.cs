// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.WrappingProvider
{
    using System.Data.Common;

    public class WrappingCommand<TBase> : DbCommand, IDisposable
        where TBase : DbProviderFactory
    {
        private readonly DbCommand _baseCommand;

        public WrappingCommand(DbCommand baseCommand)
        {
            _baseCommand = baseCommand;
        }

        public override void Prepare()
        {
            _baseCommand.Prepare();
        }

        public override string CommandText
        {
            get { return _baseCommand.CommandText; }
            set { _baseCommand.CommandText = value; }
        }

        public override int CommandTimeout
        {
            get { return _baseCommand.CommandTimeout; }
            set
            {
                WrappingAdoNetProvider<TBase>.Instance.Log.Add(new LogItem("Set CommandTimeout", Connection, value));

                _baseCommand.CommandTimeout = value;
            }
        }

        public override CommandType CommandType
        {
            get { return _baseCommand.CommandType; }
            set { _baseCommand.CommandType = value; }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get { return _baseCommand.UpdatedRowSource; }
            set { _baseCommand.UpdatedRowSource = value; }
        }

        protected override DbConnection DbConnection
        {
            get { return _baseCommand.Connection; }
            set { _baseCommand.Connection = ((WrappingConnection<TBase>)value).BaseConnection; }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return _baseCommand.Parameters; }
        }

        protected override DbTransaction DbTransaction
        {
            get { return _baseCommand.Transaction == null ? null : new WrappingTransaction<TBase>(_baseCommand.Transaction); }
            set
            {
                WrappingAdoNetProvider<TBase>.Instance.Log.Add(new LogItem("Set DbTransaction", Connection, value));

                _baseCommand.Transaction = value == null ? null : ((WrappingTransaction<TBase>)value).BaseTransaction;
            }
        }

        public override bool DesignTimeVisible
        {
            get { return _baseCommand.DesignTimeVisible; }
            set { _baseCommand.DesignTimeVisible = value; }
        }

        public override void Cancel()
        {
            _baseCommand.Cancel();
        }

        protected override DbParameter CreateDbParameter()
        {
            return _baseCommand.CreateParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            WrappingAdoNetProvider<TBase>.Instance.Log.Add(new LogItem("ExecuteReader", Connection, CommandText));

            return _baseCommand.ExecuteReader(behavior);
        }

        public override int ExecuteNonQuery()
        {
            WrappingAdoNetProvider<TBase>.Instance.Log.Add(new LogItem("ExecuteNonQuery", Connection, CommandText));

            return _baseCommand.ExecuteNonQuery();
        }

        public override object ExecuteScalar()
        {
            WrappingAdoNetProvider<TBase>.Instance.Log.Add(new LogItem("ExecuteScalar", Connection, CommandText));

            return _baseCommand.ExecuteScalar();
        }

        public new void Dispose()
        {
            _baseCommand.Dispose();
        }
    }
}
