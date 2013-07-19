// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.EntityClient;
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

            public abstract void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext);

            public abstract void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext);

            public abstract void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext);

            public abstract void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext);

            public abstract void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext);

            public abstract void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext);

            public abstract void TreeCreated(DbCommandTree commandTree, DbCommandTreeInterceptionContext interceptionContext);

            public abstract bool CommandExecuting(DbCommand command, DbInterceptionContext interceptionContext);

            public abstract bool ConnectionOpening(EntityConnection connection, DbInterceptionContext interceptionContext);
        }
    }
}
