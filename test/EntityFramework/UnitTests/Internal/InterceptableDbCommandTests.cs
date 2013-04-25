// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class InterceptableDbCommandTests
    {
        [Fact]
        public void ExecuteNonQuery_should_dispatch_to_interceptor_and_optionally_execute()
        {
            var mockCommand = new Mock<DbCommand>();
            mockCommand.Setup(m => m.ExecuteNonQuery()).Returns(11);

            var mockCancelable = new Mock<ICancelableDbCommandInterceptor>();
            var mockPublicInterceptor = new Mock<DbInterceptor> { CallBase = true };

            var dispatchers = new Dispatchers();
            dispatchers.AddInterceptor(mockCancelable.Object);
            dispatchers.AddInterceptor(mockPublicInterceptor.Object);

            var interceptionContext = new DbInterceptionContext();
            var interceptableDbCommand = new InterceptableDbCommand(mockCommand.Object, interceptionContext, dispatchers);

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, interceptionContext)).Returns(false);

            interceptableDbCommand.ExecuteNonQuery();

            mockCommand.Verify(m => m.ExecuteNonQuery(), Times.Never());

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.NonQueryExecuting(It.IsAny<DbCommand>(), It.IsAny<DbInterceptionContext>()), Times.Never());

            mockPublicInterceptor.Verify(
                m => m.NonQueryExecuted(It.IsAny<DbCommand>(), It.IsAny<int>(), It.IsAny<DbInterceptionContext>()), Times.Never());

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, interceptionContext)).Returns(true);

            Assert.Equal(11, interceptableDbCommand.ExecuteNonQuery());

            mockCommand.Verify(m => m.ExecuteNonQuery(), Times.Once());

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Exactly(2));

            mockPublicInterceptor.Verify(
                m => m.NonQueryExecuting(mockCommand.Object, interceptionContext), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.NonQueryExecuted(mockCommand.Object, 11, interceptionContext), Times.Once());
        }

        [Fact]
        public void ExecuteScalar_should_dispatch_to_interceptor_and_optionally_execute()
        {
            var mockCommand = new Mock<DbCommand>();
            mockCommand.Setup(m => m.ExecuteScalar()).Returns(11);

            var mockCancelable = new Mock<ICancelableDbCommandInterceptor>();
            var mockPublicInterceptor = new Mock<DbInterceptor> { CallBase = true };

            var dispatchers = new Dispatchers();
            dispatchers.AddInterceptor(mockCancelable.Object);
            dispatchers.AddInterceptor(mockPublicInterceptor.Object);

            var interceptionContext = new DbInterceptionContext();
            var interceptableDbCommand = new InterceptableDbCommand(mockCommand.Object, interceptionContext, dispatchers);

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, interceptionContext)).Returns(false);

            interceptableDbCommand.ExecuteScalar();

            mockCommand.Verify(m => m.ExecuteScalar(), Times.Never());

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.ScalarExecuting(It.IsAny<DbCommand>(), It.IsAny<DbInterceptionContext>()), Times.Never());

            mockPublicInterceptor.Verify(
                m => m.ScalarExecuted(It.IsAny<DbCommand>(), It.IsAny<int>(), It.IsAny<DbInterceptionContext>()), Times.Never());

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, interceptionContext)).Returns(true);

            Assert.Equal(11, interceptableDbCommand.ExecuteScalar());

            mockCommand.Verify(m => m.ExecuteScalar(), Times.Once());

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Exactly(2));

            mockPublicInterceptor.Verify(
                m => m.ScalarExecuting(mockCommand.Object, interceptionContext), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.ScalarExecuted(mockCommand.Object, 11, interceptionContext), Times.Once());
        }

        [Fact]
        public void ExecuteReader_should_dispatch_to_interceptor_and_optionally_execute()
        {
            var mockReader = new Mock<DbDataReader>();

            var mockCommand = new Mock<DbCommand>();
            mockCommand.Protected()
                       .Setup<DbDataReader>("ExecuteDbDataReader", CommandBehavior.SingleRow)
                       .Returns(mockReader.Object);

            var mockCancelable = new Mock<ICancelableDbCommandInterceptor>();
            var mockPublicInterceptor = new Mock<DbInterceptor> { CallBase = true };

            var dispatchers = new Dispatchers();
            dispatchers.AddInterceptor(mockCancelable.Object);
            dispatchers.AddInterceptor(mockPublicInterceptor.Object);

            var interceptionContext = new DbInterceptionContext();
            var interceptableDbCommand = new InterceptableDbCommand(mockCommand.Object, interceptionContext, dispatchers);

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, interceptionContext)).Returns(false);

            var reader = interceptableDbCommand.ExecuteReader(CommandBehavior.SingleRow);

            Assert.NotSame(mockReader.Object, reader);
            Assert.True(reader.NextResult());
            Assert.False(reader.NextResult());

            mockCommand.Protected().Verify("ExecuteDbDataReader", Times.Never(), CommandBehavior.SingleRow);

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.ReaderExecuting(
                    It.IsAny<DbCommand>(),
                    It.IsAny<CommandBehavior>(),
                    It.IsAny<DbInterceptionContext>()), Times.Never());

            mockPublicInterceptor.Verify(
                m => m.ReaderExecuted(
                    It.IsAny<DbCommand>(),
                    It.IsAny<CommandBehavior>(),
                    It.IsAny<DbDataReader>(),
                    It.IsAny<DbInterceptionContext>()), Times.Never());

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, interceptionContext)).Returns(true);

            reader = interceptableDbCommand.ExecuteReader(CommandBehavior.SingleRow);

            Assert.Same(mockReader.Object, reader);

            mockCommand.Protected().Verify("ExecuteDbDataReader", Times.Once(), CommandBehavior.SingleRow);

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Exactly(2));

            mockPublicInterceptor.Verify(
                m => m.ReaderExecuting(mockCommand.Object, CommandBehavior.SingleRow, interceptionContext), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.ReaderExecuted(mockCommand.Object, CommandBehavior.SingleRow, mockReader.Object, interceptionContext), Times.Once());
        }

#if !NET40
        [Fact]
        public void ExecuteNonQueryAsync_should_dispatch_to_interceptor_and_optionally_execute()
        {
            var mockCommand = new Mock<DbCommand>();
            var result = new Task<int>(() => 11);
            mockCommand.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Returns(result);

            var mockCancelable = new Mock<ICancelableDbCommandInterceptor>();
            var mockPublicInterceptor = new Mock<DbInterceptor> { CallBase = true };

            var dispatchers = new Dispatchers();
            dispatchers.AddInterceptor(mockCancelable.Object);
            dispatchers.AddInterceptor(mockPublicInterceptor.Object);

            var interceptionContext = new DbInterceptionContext();
            var interceptableDbCommand = new InterceptableDbCommand(mockCommand.Object, interceptionContext, dispatchers);

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, interceptionContext)).Returns(false);

            interceptableDbCommand.ExecuteNonQueryAsync();

            mockCommand.Verify(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()), Times.Never());

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.AsyncNonQueryExecuting(It.IsAny<DbCommand>(), It.IsAny<DbInterceptionContext>()), Times.Never());

            mockPublicInterceptor.Verify(
                m => m.AsyncNonQueryExecuted(It.IsAny<DbCommand>(), It.IsAny<Task<int>>(), It.IsAny<DbInterceptionContext>()), Times.Never());

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, interceptionContext)).Returns(true);

            Assert.Same(result, interceptableDbCommand.ExecuteNonQueryAsync());

            mockCommand.Verify(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()), Times.Once());

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Exactly(2));

            mockPublicInterceptor.Verify(
                m => m.AsyncNonQueryExecuting(mockCommand.Object, interceptionContext), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.AsyncNonQueryExecuted(mockCommand.Object, result, interceptionContext), Times.Once());
        }

        [Fact]
        public void ExecuteScalarAsync_should_dispatch_to_interceptor_and_optionally_execute()
        {
            var mockCommand = new Mock<DbCommand>();
            var result = new Task<object>(() => 11);
            mockCommand.Setup(m => m.ExecuteScalarAsync(It.IsAny<CancellationToken>())).Returns(result);

            var mockCancelable = new Mock<ICancelableDbCommandInterceptor>();
            var mockPublicInterceptor = new Mock<DbInterceptor> { CallBase = true };

            var dispatchers = new Dispatchers();
            dispatchers.AddInterceptor(mockCancelable.Object);
            dispatchers.AddInterceptor(mockPublicInterceptor.Object);

            var interceptionContext = new DbInterceptionContext();
            var interceptableDbCommand = new InterceptableDbCommand(mockCommand.Object, interceptionContext, dispatchers);

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, interceptionContext)).Returns(false);

            interceptableDbCommand.ExecuteScalarAsync();

            mockCommand.Verify(m => m.ExecuteScalarAsync(It.IsAny<CancellationToken>()), Times.Never());

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.AsyncScalarExecuting(It.IsAny<DbCommand>(), It.IsAny<DbInterceptionContext>()), Times.Never());

            mockPublicInterceptor.Verify(
                m => m.AsyncScalarExecuted(It.IsAny<DbCommand>(), It.IsAny<Task<object>>(), It.IsAny<DbInterceptionContext>()),
                Times.Never());

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, interceptionContext)).Returns(true);

            Assert.Same(result, interceptableDbCommand.ExecuteScalarAsync());

            mockCommand.Verify(m => m.ExecuteScalarAsync(It.IsAny<CancellationToken>()), Times.Once());

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Exactly(2));

            mockPublicInterceptor.Verify(
                m => m.AsyncScalarExecuting(mockCommand.Object, interceptionContext), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.AsyncScalarExecuted(mockCommand.Object, result, interceptionContext), Times.Once());
        }

        [Fact]
        public void ExecuteReaderAsync_should_dispatch_to_interceptor_and_optionally_execute()
        {
            var result = new Task<DbDataReader>(() => new Mock<DbDataReader>().Object);

            var mockCommand = new Mock<DbCommand>();
            mockCommand.Protected()
                       .Setup<Task<DbDataReader>>(
                           "ExecuteDbDataReaderAsync", CommandBehavior.SingleRow, ItExpr.IsAny<CancellationToken>())
                       .Returns(result);

            var mockCancelable = new Mock<ICancelableDbCommandInterceptor>();
            var mockPublicInterceptor = new Mock<DbInterceptor> { CallBase = true };

            var dispatchers = new Dispatchers();
            dispatchers.AddInterceptor(mockCancelable.Object);
            dispatchers.AddInterceptor(mockPublicInterceptor.Object);

            var interceptionContext = new DbInterceptionContext();
            var interceptableDbCommand = new InterceptableDbCommand(mockCommand.Object, interceptionContext, dispatchers);

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, interceptionContext)).Returns(false);

            Assert.NotSame(result, interceptableDbCommand.ExecuteReaderAsync(CommandBehavior.SingleRow));

            mockCommand.Protected().Verify(
                "ExecuteDbDataReaderAsync", Times.Never(), CommandBehavior.SingleRow, ItExpr.IsAny<CancellationToken>());

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.AsyncReaderExecuting(
                    It.IsAny<DbCommand>(),
                    It.IsAny<CommandBehavior>(),
                    It.IsAny<DbInterceptionContext>()), Times.Never());

            mockPublicInterceptor.Verify(
                m => m.AsyncReaderExecuted(
                    It.IsAny<DbCommand>(),
                    It.IsAny<CommandBehavior>(),
                    It.IsAny<Task<DbDataReader>>(),
                    It.IsAny<DbInterceptionContext>()), Times.Never());

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, interceptionContext)).Returns(true);

            Assert.Same(result, interceptableDbCommand.ExecuteReaderAsync(CommandBehavior.SingleRow));

            mockCommand.Protected().Verify(
                "ExecuteDbDataReaderAsync", Times.Once(), CommandBehavior.SingleRow, ItExpr.IsAny<CancellationToken>());

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Exactly(2));

            mockPublicInterceptor.Verify(
                m => m.AsyncReaderExecuting(mockCommand.Object, CommandBehavior.SingleRow, interceptionContext), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.AsyncReaderExecuted(mockCommand.Object, CommandBehavior.SingleRow, result, interceptionContext), Times.Once());
        }
#endif
    }
}
