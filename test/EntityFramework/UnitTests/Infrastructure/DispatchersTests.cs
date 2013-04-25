// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.EntityClient;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;

    public class DispatchersTests : TestBase
    {
        [Fact]
        public void AddInterceptor_adds_interceptor_for_all_interception_interfaces_implemented()
        {
            var mockInterceptor = new Mock<FakeInterceptor>();

            var dispatchers = new Dispatchers();
            dispatchers.AddInterceptor(mockInterceptor.Object);

            dispatchers.Command.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());
            mockInterceptor.Verify(m => m.CallMe(), Times.Once());

            dispatchers.CommandTree.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());
            mockInterceptor.Verify(m => m.CallMe(), Times.Exactly(2));

            dispatchers.CancelableCommand.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());
            mockInterceptor.Verify(m => m.CallMe(), Times.Exactly(3));

            dispatchers.EntityConnection.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());
            mockInterceptor.Verify(m => m.CallMe(), Times.Exactly(4));
        }

        [Fact]
        public void RemoveInterceptor_removes_interceptor_for_all_interception_interfaces_implemented()
        {
            var mockInterceptor = new Mock<FakeInterceptor>();

            var dispatchers = new Dispatchers();
            dispatchers.AddInterceptor(mockInterceptor.Object);
            dispatchers.RemoveInterceptor(mockInterceptor.Object);

            dispatchers.Command.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());
            dispatchers.CommandTree.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());
            dispatchers.CancelableCommand.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());
            dispatchers.EntityConnection.InternalDispatcher.Dispatch(i => ((FakeInterceptor)i).CallMe());

            mockInterceptor.Verify(m => m.CallMe(), Times.Never());
        }

        internal abstract class FakeInterceptor : IDbCommandInterceptor, IDbCommandTreeInterceptor, ICancelableDbCommandInterceptor,
                                                  IEntityConnectionInterceptor
        {
            public abstract void CallMe();

            public abstract void NonQueryExecuting(DbCommand command, DbInterceptionContext interceptionContext);

            public abstract int NonQueryExecuted(DbCommand command, int result, DbInterceptionContext interceptionContext);

            public abstract void ReaderExecuting(DbCommand command, CommandBehavior behavior, DbInterceptionContext interceptionContext);

            public abstract DbDataReader ReaderExecuted(
                DbCommand command, CommandBehavior behavior, DbDataReader result, DbInterceptionContext interceptionContext);

            public abstract void ScalarExecuting(DbCommand command, DbInterceptionContext interceptionContext);

            public abstract object ScalarExecuted(DbCommand command, object result, DbInterceptionContext interceptionContext);

            public abstract void AsyncNonQueryExecuting(DbCommand command, DbInterceptionContext interceptionContext);

            public abstract Task<int> AsyncNonQueryExecuted(DbCommand command, Task<int> result, DbInterceptionContext interceptionContext);

            public abstract void AsyncReaderExecuting(
                DbCommand command, CommandBehavior behavior, DbInterceptionContext interceptionContext);

            public abstract Task<DbDataReader> AsyncReaderExecuted(
                DbCommand command, CommandBehavior behavior, Task<DbDataReader> result, DbInterceptionContext interceptionContext);

            public abstract void AsyncScalarExecuting(DbCommand command, DbInterceptionContext interceptionContext);

            public abstract Task<object> AsyncScalarExecuted(
                DbCommand command, Task<object> result, DbInterceptionContext interceptionContext);

            public abstract DbCommandTree TreeCreated(DbCommandTree commandTree, DbInterceptionContext interceptionContext);

            public abstract bool CommandExecuting(DbCommand command, DbInterceptionContext interceptionContext);

            public abstract bool ConnectionOpening(EntityConnection connection, DbInterceptionContext interceptionContext);
        }
    }
}
