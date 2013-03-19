// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.SqlClient;
    using Moq;
    using Xunit;

    public class InterceptionTests
    {
        [Fact]
        public void Can_intercept_globally()
        {
            var mockInterceptor = new Mock<IDbInterceptor>();

            var interception = new Interception();

            interception.Add(mockInterceptor.Object);

            var command = new SqlCommand();
            var commandTree = new Mock<DbCommandTree>().Object;
            var connection = new EntityConnection();

            mockInterceptor.Setup(m => m.CommandExecuting(command)).Returns(true);
            mockInterceptor.Setup(m => m.CommandTreeCreated(commandTree)).Returns(commandTree);
            mockInterceptor.Setup(m => m.ConnectionOpening(connection)).Returns(true);

            Assert.True(interception.Dispatch(command));
            Assert.Same(commandTree, interception.Dispatch(commandTree));
            Assert.True(interception.Dispatch(connection));

            mockInterceptor.Verify(m => m.CommandExecuting(command), Times.Once());
            mockInterceptor.Verify(m => m.CommandTreeCreated(commandTree), Times.Once());
            mockInterceptor.Verify(m => m.ConnectionOpening(connection), Times.Once());
        }

        [Fact]
        public void Can_remove_global_interceptors()
        {
            var mockInterceptor = new Mock<IDbInterceptor>();

            var interception = new Interception();

            var command = new SqlCommand();
            var commandTree = new Mock<DbCommandTree>().Object;
            var connection = new EntityConnection();

            interception.Add(mockInterceptor.Object);
            interception.Remove(mockInterceptor.Object);

            interception.Dispatch(command);
            interception.Dispatch(commandTree);
            interception.Dispatch(connection);

            mockInterceptor.Verify(m => m.CommandExecuting(command), Times.Never());
            mockInterceptor.Verify(m => m.CommandTreeCreated(commandTree), Times.Never());
            mockInterceptor.Verify(m => m.ConnectionOpening(connection), Times.Never());
        }

        [Fact]
        public void Can_intercept_by_context()
        {
            var mockInterceptor = new Mock<IDbInterceptor>();
            var context = new DbContext("Foo");

            var interception = new Interception();

            interception.Add(context, mockInterceptor.Object);

            var command = new SqlCommand();
            var commandTree = new Mock<DbCommandTree>().Object;
            var connection = new EntityConnection();

            mockInterceptor.Setup(m => m.CommandExecuting(command)).Returns(true);
            mockInterceptor.Setup(m => m.CommandTreeCreated(commandTree)).Returns(commandTree);
            mockInterceptor.Setup(m => m.ConnectionOpening(connection)).Returns(true);

            interception.Dispatch(command);
            interception.Dispatch(commandTree);
            interception.Dispatch(connection);

            mockInterceptor.Verify(m => m.CommandExecuting(command), Times.Never());
            mockInterceptor.Verify(m => m.CommandTreeCreated(commandTree), Times.Never());
            mockInterceptor.Verify(m => m.ConnectionOpening(connection), Times.Never());

            interception.SetActiveContext(context);

            Assert.True(interception.Dispatch(command));
            Assert.Same(commandTree, interception.Dispatch(commandTree));
            Assert.True(interception.Dispatch(connection));

            mockInterceptor.Verify(m => m.CommandExecuting(command), Times.Once());
            mockInterceptor.Verify(m => m.CommandTreeCreated(commandTree), Times.Once());
            mockInterceptor.Verify(m => m.ConnectionOpening(connection), Times.Once());
        }

        [Fact]
        public void Can_remove_per_context_interceptors()
        {
            var mockInterceptor = new Mock<IDbInterceptor>();
            var context = new DbContext("Foo");

            var interception = new Interception();

            var command = new SqlCommand();
            var commandTree = new Mock<DbCommandTree>().Object;
            var connection = new EntityConnection();

            interception.SetActiveContext(context);
            interception.Add(context, mockInterceptor.Object);
            interception.Remove(context, mockInterceptor.Object);

            interception.Dispatch(command);
            interception.Dispatch(commandTree);
            interception.Dispatch(connection);

            mockInterceptor.Verify(m => m.CommandExecuting(command), Times.Never());
            mockInterceptor.Verify(m => m.CommandTreeCreated(commandTree), Times.Never());
            mockInterceptor.Verify(m => m.ConnectionOpening(connection), Times.Never());
        }

        [Fact]
        public void Dispatching_command_invokes_all_interceptors_and_aggregates_result()
        {
            var command = new SqlCommand();

            var mockInterceptor1 = new Mock<IDbInterceptor>();
            mockInterceptor1.Setup(m => m.CommandExecuting(command)).Returns(false);

            var mockInterceptor2 = new Mock<IDbInterceptor>();
            mockInterceptor2.Setup(m => m.CommandExecuting(command)).Returns(true);

            var context = new DbContext("Foo");

            var interception = new Interception();

            interception.SetActiveContext(context);
            interception.Add(mockInterceptor1.Object);
            interception.Add(context, mockInterceptor2.Object);

            var result = interception.Dispatch(command);

            mockInterceptor1.Verify(m => m.CommandExecuting(command), Times.Once());
            mockInterceptor2.Verify(m => m.CommandExecuting(command), Times.Once());

            Assert.False(result);
        }

        [Fact]
        public void Dispatching_connection_open_invokes_all_interceptors_and_aggregates_result()
        {
            var connection = new EntityConnection();

            var mockInterceptor1 = new Mock<IDbInterceptor>();
            mockInterceptor1.Setup(m => m.ConnectionOpening(connection)).Returns(false);

            var mockInterceptor2 = new Mock<IDbInterceptor>();
            mockInterceptor2.Setup(m => m.ConnectionOpening(connection)).Returns(true);

            var context = new DbContext("Foo");

            var interception = new Interception();

            interception.SetActiveContext(context);
            interception.Add(mockInterceptor1.Object);
            interception.Add(context, mockInterceptor2.Object);

            var result = interception.Dispatch(connection);

            mockInterceptor1.Verify(m => m.ConnectionOpening(connection), Times.Once());
            mockInterceptor2.Verify(m => m.ConnectionOpening(connection), Times.Once());

            Assert.False(result);
        }
    }
}
