// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using Moq;
    using Xunit;

    public class DispatchersTests : TestBase
    {
        [Fact]
        public void AddInterceptor_adds_interceptor_for_all_interception_interfaces_implemented()
        {
            var mockInterceptor = new Mock<FakeInterceptor>();

            var dispatchers = new DbDispatchers();
            dispatchers.AddInterceptor(mockInterceptor.Object);

            dispatchers.Command.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());
            mockInterceptor.Verify(m => m.CallMe(), Times.Once());

            dispatchers.CommandTree.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());
            mockInterceptor.Verify(m => m.CallMe(), Times.Exactly(2));

            dispatchers.CancelableCommand.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());
            mockInterceptor.Verify(m => m.CallMe(), Times.Exactly(3));

            dispatchers.CancelableEntityConnection.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());
            mockInterceptor.Verify(m => m.CallMe(), Times.Exactly(4));

            dispatchers.Configuration.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());
            mockInterceptor.Verify(m => m.CallMe(), Times.Exactly(5));

            dispatchers.Connection.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());
            mockInterceptor.Verify(m => m.CallMe(), Times.Exactly(6));

            dispatchers.Transaction.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());
            mockInterceptor.Verify(m => m.CallMe(), Times.Exactly(7));
        }

        [Fact]
        public void RemoveInterceptor_removes_interceptor_for_all_interception_interfaces_implemented()
        {
            var mockInterceptor = new Mock<FakeInterceptor>();

            var dispatchers = new DbDispatchers();
            dispatchers.AddInterceptor(mockInterceptor.Object);
            dispatchers.RemoveInterceptor(mockInterceptor.Object);

            dispatchers.Command.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());
            dispatchers.CommandTree.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());
            dispatchers.CancelableCommand.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());
            dispatchers.CancelableEntityConnection.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());
            dispatchers.Configuration.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());
            dispatchers.Connection.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());
            dispatchers.Transaction.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());

            mockInterceptor.Verify(m => m.CallMe(), Times.Never());
        }

        internal abstract class FakeInterceptor : IDbCommandInterceptor, IDbCommandTreeInterceptor, ICancelableDbCommandInterceptor,
            ICancelableEntityConnectionInterceptor, IDbConfigurationInterceptor, IDbConnectionInterceptor,
            IDbTransactionInterceptor
        {
            public abstract void CallMe();

            public abstract void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext);

            public abstract void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext);

            public abstract void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext);

            public abstract void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext);

            public abstract void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext);

            public abstract void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext);

            public abstract void TreeCreated(DbCommandTreeInterceptionContext interceptionContext);

            public abstract bool CommandExecuting(DbCommand command, DbInterceptionContext interceptionContext);

            public abstract bool ConnectionOpening(EntityConnection connection, DbInterceptionContext interceptionContext);

            public abstract void Loaded(
                DbConfigurationLoadedEventArgs loadedEventArgs, DbConfigurationInterceptionContext interceptionContext);

            public abstract void BeginningTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext);

            public abstract void BeganTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext);

            public abstract void Closing(DbConnection connection, DbConnectionInterceptionContext interceptionContext);

            public abstract void Closed(DbConnection connection, DbConnectionInterceptionContext interceptionContext);

            public abstract void ConnectionStringGetting(
                DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

            public abstract void ConnectionStringGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

            public abstract void ConnectionStringSetting(
                DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext);

            public abstract void ConnectionStringSet(
                DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext);

            public abstract void ConnectionTimeoutGetting(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext);

            public abstract void ConnectionTimeoutGot(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext);

            public abstract void DatabaseGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

            public abstract void DatabaseGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

            public abstract void DataSourceGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

            public abstract void DataSourceGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

            public abstract void Disposing(DbConnection connection, DbConnectionInterceptionContext interceptionContext);

            public abstract void Disposed(DbConnection connection, DbConnectionInterceptionContext interceptionContext);

            public abstract void EnlistingTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext);

            public abstract void EnlistedTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext);

            public abstract void Opening(DbConnection connection, DbConnectionInterceptionContext interceptionContext);

            public abstract void Opened(DbConnection connection, DbConnectionInterceptionContext interceptionContext);

            public abstract void ServerVersionGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

            public abstract void ServerVersionGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

            public abstract void StateGetting(DbConnection connection, DbConnectionInterceptionContext<ConnectionState> interceptionContext);

            public abstract void StateGot(DbConnection connection, DbConnectionInterceptionContext<ConnectionState> interceptionContext);

            public abstract void ConnectionGetting(
                DbTransaction transaction, DbTransactionInterceptionContext<DbConnection> interceptionContext);

            public abstract void ConnectionGot(
                DbTransaction transaction, DbTransactionInterceptionContext<DbConnection> interceptionContext);

            public abstract void IsolationLevelGetting(
                DbTransaction transaction, DbTransactionInterceptionContext<IsolationLevel> interceptionContext);

            public abstract void IsolationLevelGot(
                DbTransaction transaction, DbTransactionInterceptionContext<IsolationLevel> interceptionContext);

            public abstract void Committing(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext);

            public abstract void Committed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext);

            public abstract void Disposing(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext);

            public abstract void Disposed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext);

            public abstract void RollingBack(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext);

            public abstract void RolledBack(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext);
        }
    }
}
