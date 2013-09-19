// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace SampleEntityFrameworkProvider
{
    public partial class SampleCommand : DbCommand
    {
        internal DbCommand _WrappedCommand = new SqlCommand();

        public SampleCommand()
        {
        }

        public SampleCommand(string commandText)
        {
            this.InitializeMe(commandText, null, null);
        }

        public SampleCommand(string commandText, SampleConnection connection)
        {
            this.InitializeMe(commandText, connection, null);
        }

        public SampleCommand(string commandText, SampleConnection connection, DbTransaction transaction)
        {
            this.InitializeMe(commandText, connection, transaction);
        }

        private void InitializeMe(string commandText, SampleConnection connection, DbTransaction transaction)
        {
            this.CommandText = commandText;
            this.Connection = connection;
            this.Transaction = transaction;
        }

        public override void Cancel()
        {
            this._WrappedCommand.Cancel();
        }

        public override string CommandText
        {
            get
            {
                return this._WrappedCommand.CommandText;
            }
            set
            {
                this._WrappedCommand.CommandText = value;
            }
        }

        public override int CommandTimeout
        {
            get
            {
                return this._WrappedCommand.CommandTimeout;
            }
            set
            {
                this._WrappedCommand.CommandTimeout = value;
            }
        }

        public override CommandType CommandType
        {
            get
            {
                return this._WrappedCommand.CommandType;
            }
            set
            {
                this._WrappedCommand.CommandType = value;
            }
        }

        protected override DbParameter CreateDbParameter()
        {
            return this._WrappedCommand.CreateParameter();
        }

        private SampleConnection _Connection = null;
        protected override DbConnection DbConnection
        {
            get
            {
                return this._Connection;
            }
            set
            {
                this._Connection = (SampleConnection) value;
                this._WrappedCommand.Connection = this._Connection._WrappedConnection;
            }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return this._WrappedCommand.Parameters; }
        }

        private DbTransaction _Transaction = null;
        protected override DbTransaction DbTransaction
        {
            get
            {
                return this._Transaction;
            }
            set
            {
                this._Transaction = value;
                this._WrappedCommand.Transaction = this._Transaction;
            }
        }

        private bool _DesignTimeVisible = true;
        public override bool DesignTimeVisible
        {
            get
            {
                return this._DesignTimeVisible;
            }
            set
            {
                this._DesignTimeVisible = value;
            }
        }
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return this._WrappedCommand.ExecuteReader(behavior);
        }

        public override int ExecuteNonQuery()
        {
            return this._WrappedCommand.ExecuteNonQuery();
        }

        public override object ExecuteScalar()
        {
            return this._WrappedCommand.ExecuteScalar();
        }

        public override void Prepare()
        {
            this._WrappedCommand.Prepare();
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get
            {
                return this._WrappedCommand.UpdatedRowSource;
            }
            set
            {
                this._WrappedCommand.UpdatedRowSource = value;
            }
        }
    }
}
