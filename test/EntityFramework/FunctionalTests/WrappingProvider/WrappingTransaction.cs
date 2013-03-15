// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.WrappingProvider
{
    using System.Data.Common;

    public class WrappingTransaction<TBase> : DbTransaction where TBase : DbProviderFactory
    {
        private readonly DbTransaction _baseTransaction;

        public WrappingTransaction(DbTransaction baseTransaction)
        {
            _baseTransaction = baseTransaction;
        }

        public override void Commit()
        {
            WrappingAdoNetProvider<TBase>.Instance.Log.Add(new LogItem("Commit", Connection, this));

            _baseTransaction.Commit();
        }

        public override void Rollback()
        {
            WrappingAdoNetProvider<TBase>.Instance.Log.Add(new LogItem("Rollback", Connection, this));

            _baseTransaction.Rollback();
        }

        protected override DbConnection DbConnection
        {
            get { return _baseTransaction.Connection == null ? null : new WrappingConnection<TBase>(_baseTransaction.Connection); }
        }

        public override IsolationLevel IsolationLevel
        {
            get { return _baseTransaction.IsolationLevel; }
        }

        public DbTransaction BaseTransaction
        {
            get { return _baseTransaction; }
        }
    }
}
