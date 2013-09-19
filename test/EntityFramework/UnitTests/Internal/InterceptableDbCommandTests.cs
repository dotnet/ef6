// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Utilities;
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
            var mockPublicInterceptor = new Mock<DbCommandInterceptor> { CallBase = true };

            var dispatchers = new DbDispatchers();
            dispatchers.AddInterceptor(mockCancelable.Object);
            dispatchers.AddInterceptor(mockPublicInterceptor.Object);

            var context = new Mock<InternalContextForMock>().Object.Owner;
            var interceptionContext = new DbInterceptionContext().WithDbContext(context);
            var interceptableDbCommand = new InterceptableDbCommand(mockCommand.Object, interceptionContext, dispatchers);

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>())).Returns(false);

            interceptableDbCommand.ExecuteNonQuery();

            mockCommand.Verify(m => m.ExecuteNonQuery(), Times.Never());

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.NonQueryExecuting(It.IsAny<DbCommand>(), It.IsAny<DbCommandInterceptionContext<int>>()), Times.Never());

            mockPublicInterceptor.Verify(
                m => m.NonQueryExecuted(It.IsAny<DbCommand>(), It.IsAny<DbCommandInterceptionContext<int>>()), Times.Never());

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, It.IsAny<DbInterceptionContext>())).Returns(true);

            Assert.Equal(11, interceptableDbCommand.ExecuteNonQuery());

            mockCommand.Verify(m => m.ExecuteNonQuery(), Times.Once());

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Exactly(2));

            mockPublicInterceptor.Verify(
                m => m.NonQueryExecuting(
                    mockCommand.Object,
                    It.Is<DbCommandInterceptionContext<int>>(c => c.DbContexts.Contains(context, ReferenceEquals))), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.NonQueryExecuted(
                    mockCommand.Object,
                    It.Is<DbCommandInterceptionContext<int>>(
                        c => c.DbContexts.Contains(context, ReferenceEquals) && c.Result == 11)), Times.Once());
        }

        [Fact]
        public void ExecuteScalar_should_dispatch_to_interceptor_and_optionally_execute()
        {
            var mockCommand = new Mock<DbCommand>();
            mockCommand.Setup(m => m.ExecuteScalar()).Returns(11);

            var mockCancelable = new Mock<ICancelableDbCommandInterceptor>();
            var mockPublicInterceptor = new Mock<DbCommandInterceptor> { CallBase = true };

            var dispatchers = new DbDispatchers();
            dispatchers.AddInterceptor(mockCancelable.Object);
            dispatchers.AddInterceptor(mockPublicInterceptor.Object);

            var context = new Mock<InternalContextForMock>().Object.Owner;
            var interceptionContext = new DbInterceptionContext().WithDbContext(context);
            var interceptableDbCommand = new InterceptableDbCommand(mockCommand.Object, interceptionContext, dispatchers);

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, It.IsAny<DbInterceptionContext>())).Returns(false);

            interceptableDbCommand.ExecuteScalar();

            mockCommand.Verify(m => m.ExecuteScalar(), Times.Never());

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.ScalarExecuting(It.IsAny<DbCommand>(), It.IsAny<DbCommandInterceptionContext<object>>()), Times.Never());

            mockPublicInterceptor.Verify(
                m => m.ScalarExecuted(It.IsAny<DbCommand>(), It.IsAny<DbCommandInterceptionContext<object>>()), Times.Never());

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, It.IsAny<DbInterceptionContext>())).Returns(true);

            Assert.Equal(11, interceptableDbCommand.ExecuteScalar());

            mockCommand.Verify(m => m.ExecuteScalar(), Times.Once());

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Exactly(2));

            mockPublicInterceptor.Verify(
                m => m.ScalarExecuting(
                    mockCommand.Object,
                    It.Is<DbCommandInterceptionContext<object>>(c => c.DbContexts.Contains(context, ReferenceEquals))), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.ScalarExecuted(
                    mockCommand.Object,
                    It.Is<DbCommandInterceptionContext<object>>(
                        c => c.DbContexts.Contains(context, ReferenceEquals) && (int)c.Result == 11)), Times.Once());
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
            var mockPublicInterceptor = new Mock<DbCommandInterceptor> { CallBase = true };

            var dispatchers = new DbDispatchers();
            dispatchers.AddInterceptor(mockCancelable.Object);
            dispatchers.AddInterceptor(mockPublicInterceptor.Object);

            var interceptionContext = new DbInterceptionContext();
            var interceptableDbCommand = new InterceptableDbCommand(mockCommand.Object, interceptionContext, dispatchers);

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, It.IsAny<DbInterceptionContext>())).Returns(false);

            var reader = interceptableDbCommand.ExecuteReader(CommandBehavior.SingleRow);

            Assert.NotSame(mockReader.Object, reader);
            Assert.True(reader.NextResult());
            Assert.False(reader.NextResult());

            mockCommand.Protected().Verify("ExecuteDbDataReader", Times.Never(), CommandBehavior.SingleRow);

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.ReaderExecuting(
                    It.IsAny<DbCommand>(),
                    It.IsAny<DbCommandInterceptionContext<DbDataReader>>()), Times.Never());

            mockPublicInterceptor.Verify(
                m => m.ReaderExecuted(
                    It.IsAny<DbCommand>(),
                    It.IsAny<DbCommandInterceptionContext<DbDataReader>>()), Times.Never());

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, It.IsAny<DbInterceptionContext>())).Returns(true);

            reader = interceptableDbCommand.ExecuteReader(CommandBehavior.SingleRow);

            Assert.Same(mockReader.Object, reader);

            mockCommand.Protected().Verify("ExecuteDbDataReader", Times.Once(), CommandBehavior.SingleRow);

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Exactly(2));

            mockPublicInterceptor.Verify(
                m => m.ReaderExecuting(
                    mockCommand.Object,
                    It.Is<DbCommandInterceptionContext<DbDataReader>>(c => c.CommandBehavior == CommandBehavior.SingleRow)), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.ReaderExecuted(
                    mockCommand.Object,
                    It.Is<DbCommandInterceptionContext<DbDataReader>>(
                    c => c.CommandBehavior == CommandBehavior.SingleRow && c.Result == mockReader.Object)), Times.Once());
        }

#if !NET40
        [Fact]
        public void ExecuteNonQueryAsync_should_dispatch_to_interceptor_and_optionally_execute()
        {
            var mockCommand = new Mock<DbCommand>();
            var result = Task.FromResult(11);
            mockCommand.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Returns(result);

            var mockCancelable = new Mock<ICancelableDbCommandInterceptor>();
            var mockPublicInterceptor = new Mock<DbCommandInterceptor> { CallBase = true };

            var dispatchers = new DbDispatchers();
            dispatchers.AddInterceptor(mockCancelable.Object);
            dispatchers.AddInterceptor(mockPublicInterceptor.Object);

            var interceptionContext = new DbInterceptionContext();
            var interceptableDbCommand = new InterceptableDbCommand(mockCommand.Object, interceptionContext, dispatchers);

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, It.IsAny<DbInterceptionContext>())).Returns(false);

            interceptableDbCommand.ExecuteNonQueryAsync();

            mockCommand.Verify(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()), Times.Never());

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.NonQueryExecuting(It.IsAny<DbCommand>(), It.IsAny<DbCommandInterceptionContext<int>>()), Times.Never());

            mockPublicInterceptor.Verify(
                m => m.NonQueryExecuted(It.IsAny<DbCommand>(), It.IsAny<DbCommandInterceptionContext<int>>()), Times.Never());

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, It.IsAny<DbInterceptionContext>())).Returns(true);

            var interceptResult = interceptableDbCommand.ExecuteNonQueryAsync();
            interceptResult.Wait();
            Assert.Equal(11, interceptResult.Result);

            mockCommand.Verify(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()), Times.Once());

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Exactly(2));

            mockPublicInterceptor.Verify(
                m => m.NonQueryExecuting(
                    mockCommand.Object, It.Is<DbCommandInterceptionContext<int>>(c => c.IsAsync)), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.NonQueryExecuted(
                    mockCommand.Object, It.Is<DbCommandInterceptionContext<int>>(c => c.IsAsync && c.Result == 11)), Times.Once());
        }

        [Fact]
        public void ExecuteScalarAsync_should_dispatch_to_interceptor_and_optionally_execute()
        {
            var mockCommand = new Mock<DbCommand>();
            var result = Task.FromResult<object>(11);
            mockCommand.Setup(m => m.ExecuteScalarAsync(It.IsAny<CancellationToken>())).Returns(result);

            var mockCancelable = new Mock<ICancelableDbCommandInterceptor>();
            var mockPublicInterceptor = new Mock<DbCommandInterceptor> { CallBase = true };

            var dispatchers = new DbDispatchers();
            dispatchers.AddInterceptor(mockCancelable.Object);
            dispatchers.AddInterceptor(mockPublicInterceptor.Object);

            var interceptionContext = new DbInterceptionContext();
            var interceptableDbCommand = new InterceptableDbCommand(mockCommand.Object, interceptionContext, dispatchers);

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, It.IsAny<DbInterceptionContext>())).Returns(false);

            interceptableDbCommand.ExecuteScalarAsync();

            mockCommand.Verify(m => m.ExecuteScalarAsync(It.IsAny<CancellationToken>()), Times.Never());

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.ScalarExecuting(It.IsAny<DbCommand>(), It.IsAny<DbCommandInterceptionContext<object>>()), Times.Never());

            mockPublicInterceptor.Verify(
                m => m.ScalarExecuted(It.IsAny<DbCommand>(), It.IsAny<DbCommandInterceptionContext<object>>()),
                Times.Never());

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, It.IsAny<DbInterceptionContext>())).Returns(true);

            var interceptResult = interceptableDbCommand.ExecuteScalarAsync();
            interceptResult.Wait();
            Assert.Equal(11, interceptResult.Result);

            mockCommand.Verify(m => m.ExecuteScalarAsync(It.IsAny<CancellationToken>()), Times.Once());

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Exactly(2));

            mockPublicInterceptor.Verify(
                m => m.ScalarExecuting(mockCommand.Object, It.Is<DbCommandInterceptionContext<object>>(c => c.IsAsync)), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.ScalarExecuted(
                    mockCommand.Object, It.Is<DbCommandInterceptionContext<object>>(c => c.IsAsync && (int)c.Result == 11)), Times.Once());
        }

        [Fact]
        public void ExecuteReaderAsync_should_dispatch_to_interceptor_and_optionally_execute()
        {
            var result = Task.FromResult(new Mock<DbDataReader>().Object);

            var mockCommand = new Mock<DbCommand>();
            mockCommand.Protected()
                .Setup<Task<DbDataReader>>(
                    "ExecuteDbDataReaderAsync", CommandBehavior.SingleRow, ItExpr.IsAny<CancellationToken>())
                .Returns(result);

            var mockCancelable = new Mock<ICancelableDbCommandInterceptor>();
            var mockPublicInterceptor = new Mock<DbCommandInterceptor> { CallBase = true };

            var dispatchers = new DbDispatchers();
            dispatchers.AddInterceptor(mockCancelable.Object);
            dispatchers.AddInterceptor(mockPublicInterceptor.Object);

            var interceptionContext = new DbInterceptionContext();
            var interceptableDbCommand = new InterceptableDbCommand(mockCommand.Object, interceptionContext, dispatchers);

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, It.IsAny<DbInterceptionContext>())).Returns(false);

            Assert.NotSame(result, interceptableDbCommand.ExecuteReaderAsync(CommandBehavior.SingleRow));

            mockCommand.Protected().Verify(
                "ExecuteDbDataReaderAsync", Times.Never(), CommandBehavior.SingleRow, ItExpr.IsAny<CancellationToken>());

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Once());

            mockPublicInterceptor.Verify(
                m => m.ReaderExecuting(
                    It.IsAny<DbCommand>(),
                    It.IsAny<DbCommandInterceptionContext<DbDataReader>>()), Times.Never());

            mockPublicInterceptor.Verify(
                m => m.ReaderExecuted(
                    It.IsAny<DbCommand>(),
                    It.IsAny<DbCommandInterceptionContext<DbDataReader>>()), Times.Never());

            mockCancelable.Setup(m => m.CommandExecuting(mockCommand.Object, It.IsAny<DbInterceptionContext>())).Returns(true);

            var interceptResult = interceptableDbCommand.ExecuteReaderAsync(CommandBehavior.SingleRow);
            interceptResult.Wait();
            Assert.Same(result.Result, interceptResult.Result);

            mockCommand.Protected().Verify(
                "ExecuteDbDataReaderAsync", Times.Once(), CommandBehavior.SingleRow, ItExpr.IsAny<CancellationToken>());

            mockCancelable.Verify(m => m.CommandExecuting(mockCommand.Object, interceptionContext), Times.Exactly(2));

            mockPublicInterceptor.Verify(
                m => m.ReaderExecuting(
                    mockCommand.Object,
                    It.Is<DbCommandInterceptionContext<DbDataReader>>(c => c.IsAsync && c.CommandBehavior == CommandBehavior.SingleRow)),
                Times.Once());

            mockPublicInterceptor.Verify(
                m => m.ReaderExecuted(
                    mockCommand.Object,
                    It.Is<DbCommandInterceptionContext<DbDataReader>>(
                        c => c.IsAsync && c.CommandBehavior == CommandBehavior.SingleRow && c.Result == result.Result)), Times.Once());
        }
#endif
    }
}
