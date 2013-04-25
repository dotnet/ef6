// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class DbCommandDispatcherTests
    {
        public class NonQuery : TestBase
        {
            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_which_can_change_result()
            {
                var interceptionContext = new DbInterceptionContext();
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteNonQuery()).Returns(11);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.NonQueryExecuted(mockCommand.Object, 11, interceptionContext)).Returns(13);

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                Assert.Equal(13, dispatcher.NonQuery(mockCommand.Object, interceptionContext));

                mockCommand.Verify(m => m.ExecuteNonQuery());
                mockInterceptor.Verify(m => m.NonQueryExecuting(mockCommand.Object, interceptionContext));
                mockInterceptor.Verify(m => m.NonQueryExecuted(mockCommand.Object, 11, interceptionContext));
            }

            [Fact]
            public void Dispatch_method_executes_command_even_if_no_interceptors_are_registered()
            {
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteNonQuery()).Returns(11);

                Assert.Equal(11, new DbCommandDispatcher().NonQuery(mockCommand.Object, new DbInterceptionContext()));

                mockCommand.Verify(m => m.ExecuteNonQuery());
            }

            [Fact]
            public void Dispatch_method_checks_for_nulls()
            {
                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().NonQuery(null, new DbInterceptionContext())).ParamName);

                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().NonQuery(new Mock<DbCommand>().Object, null)).ParamName);
            }
        }

        public class Scalar : TestBase
        {
            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_which_can_change_result()
            {
                var interceptionContext = new DbInterceptionContext();
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteScalar()).Returns(11);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.ScalarExecuted(mockCommand.Object, 11, interceptionContext)).Returns(13);

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                Assert.Equal(13, dispatcher.Scalar(mockCommand.Object, interceptionContext));

                mockCommand.Verify(m => m.ExecuteScalar());
                mockInterceptor.Verify(m => m.ScalarExecuting(mockCommand.Object, interceptionContext));
                mockInterceptor.Verify(m => m.ScalarExecuted(mockCommand.Object, 11, interceptionContext));
            }

            [Fact]
            public void Dispatch_method_executes_command_even_if_no_interceptors_are_registered()
            {
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteScalar()).Returns(11);

                Assert.Equal(11, new DbCommandDispatcher().Scalar(mockCommand.Object, new DbInterceptionContext()));

                mockCommand.Verify(m => m.ExecuteScalar());
            }

            [Fact]
            public void Dispatch_method_checks_for_nulls()
            {
                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().Scalar(null, new DbInterceptionContext())).ParamName);

                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().Scalar(new Mock<DbCommand>().Object, null)).ParamName);
            }
        }

        public class Reader : TestBase
        {
            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_which_can_change_result()
            {
                var interceptionContext = new DbInterceptionContext();

                var reader = new Mock<DbDataReader>().Object;
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Protected()
                           .Setup<DbDataReader>("ExecuteDbDataReader", CommandBehavior.SequentialAccess)
                           .Returns(reader);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                var interceptReader = new Mock<DbDataReader>().Object;
                mockInterceptor.Setup(
                    m => m.ReaderExecuted(mockCommand.Object, CommandBehavior.SequentialAccess, reader, interceptionContext))
                               .Returns(interceptReader);

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                Assert.Same(interceptReader, dispatcher.Reader(mockCommand.Object, CommandBehavior.SequentialAccess, interceptionContext));

                mockCommand.Protected().Verify("ExecuteDbDataReader", Times.Once(), CommandBehavior.SequentialAccess);
                mockInterceptor.Verify(m => m.ReaderExecuting(mockCommand.Object, CommandBehavior.SequentialAccess, interceptionContext));
                mockInterceptor.Verify(
                    m => m.ReaderExecuted(mockCommand.Object, CommandBehavior.SequentialAccess, reader, interceptionContext));
            }

            [Fact]
            public void Dispatch_method_executes_command_even_if_no_interceptors_are_registered()
            {
                var reader = new Mock<DbDataReader>().Object;
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Protected()
                           .Setup<DbDataReader>("ExecuteDbDataReader", CommandBehavior.SequentialAccess)
                           .Returns(reader);

                Assert.Same(
                    reader,
                    new DbCommandDispatcher().Reader(mockCommand.Object, CommandBehavior.SequentialAccess, new DbInterceptionContext()));

                mockCommand.Protected().Verify("ExecuteDbDataReader", Times.Once(), CommandBehavior.SequentialAccess);
            }

            [Fact]
            public void Dispatch_method_checks_for_nulls()
            {
                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().Reader(null, CommandBehavior.Default, new DbInterceptionContext())).ParamName);

                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().Reader(new Mock<DbCommand>().Object, CommandBehavior.Default, null)).ParamName);
            }
        }

#if !NET40
        public class AsyncNonQuery : TestBase
        {
            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_which_can_change_result()
            {
                var interceptionContext = new DbInterceptionContext();
                var cancellationToken = new CancellationToken();

                var mockCommand = new Mock<DbCommand>();
                var result = new Task<int>(() => 11);
                mockCommand.Setup(m => m.ExecuteNonQueryAsync(cancellationToken)).Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                var interceptResult = new Task<int>(() => 13);
                mockInterceptor.Setup(m => m.AsyncNonQueryExecuted(mockCommand.Object, result, interceptionContext))
                               .Returns(interceptResult);

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                Assert.Same(interceptResult, dispatcher.AsyncNonQuery(mockCommand.Object, cancellationToken, interceptionContext));

                mockCommand.Verify(m => m.ExecuteNonQueryAsync(cancellationToken));
                mockInterceptor.Verify(m => m.AsyncNonQueryExecuting(mockCommand.Object, interceptionContext));
                mockInterceptor.Verify(m => m.AsyncNonQueryExecuted(mockCommand.Object, result, interceptionContext));
            }

            [Fact]
            public void Dispatch_method_executes_command_even_if_no_interceptors_are_registered()
            {
                var mockCommand = new Mock<DbCommand>();
                var result = new Task<int>(() => 11);
                mockCommand.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Returns(result);

                Assert.Same(
                    result,
                    new DbCommandDispatcher().AsyncNonQuery(mockCommand.Object, CancellationToken.None, new DbInterceptionContext()));

                mockCommand.Verify(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()));
            }

            [Fact]
            public void Dispatch_method_checks_for_nulls()
            {
                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().AsyncNonQuery(null, CancellationToken.None, new DbInterceptionContext())).ParamName);

                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().AsyncNonQuery(new Mock<DbCommand>().Object, CancellationToken.None, null)).ParamName);
            }
        }

        public class AsyncScalar : TestBase
        {
            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_which_can_change_result()
            {
                var interceptionContext = new DbInterceptionContext();
                var cancellationToken = new CancellationToken();

                var mockCommand = new Mock<DbCommand>();
                var result = new Task<object>(() => 11);
                mockCommand.Setup(m => m.ExecuteScalarAsync(cancellationToken)).Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                var interceptResult = new Task<object>(() => 13);
                mockInterceptor.Setup(m => m.AsyncScalarExecuted(mockCommand.Object, result, interceptionContext)).Returns(interceptResult);

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                Assert.Same(interceptResult, dispatcher.AsyncScalar(mockCommand.Object, cancellationToken, interceptionContext));

                mockCommand.Verify(m => m.ExecuteScalarAsync(cancellationToken));
                mockInterceptor.Verify(m => m.AsyncScalarExecuting(mockCommand.Object, interceptionContext));
                mockInterceptor.Verify(m => m.AsyncScalarExecuted(mockCommand.Object, result, interceptionContext));
            }

            [Fact]
            public void Dispatch_method_executes_command_even_if_no_interceptors_are_registered()
            {
                var mockCommand = new Mock<DbCommand>();
                var result = new Task<object>(() => 11);
                mockCommand.Setup(m => m.ExecuteScalarAsync(It.IsAny<CancellationToken>())).Returns(result);

                Assert.Same(
                    result,
                    new DbCommandDispatcher().AsyncScalar(mockCommand.Object, CancellationToken.None, new DbInterceptionContext()));

                mockCommand.Verify(m => m.ExecuteScalarAsync(It.IsAny<CancellationToken>()));
            }

            [Fact]
            public void Dispatch_method_checks_for_nulls()
            {
                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().AsyncScalar(null, CancellationToken.None, new DbInterceptionContext())).ParamName);

                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().AsyncScalar(new Mock<DbCommand>().Object, CancellationToken.None, null)).ParamName);
            }
        }

        public class AsyncReader : TestBase
        {
            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_which_can_change_result()
            {
                var interceptionContext = new DbInterceptionContext();
                var cancellationToken = new CancellationToken();

                var result = new Task<DbDataReader>(() => new Mock<DbDataReader>().Object);
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Protected()
                           .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", CommandBehavior.SequentialAccess, cancellationToken)
                           .Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                var interceptResult = new Task<DbDataReader>(() => new Mock<DbDataReader>().Object);
                mockInterceptor.Setup(
                    m => m.AsyncReaderExecuted(mockCommand.Object, CommandBehavior.SequentialAccess, result, interceptionContext))
                               .Returns(interceptResult);

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                Assert.Same(
                    interceptResult,
                    dispatcher.AsyncReader(mockCommand.Object, CommandBehavior.SequentialAccess, cancellationToken, interceptionContext));

                mockCommand.Protected()
                           .Verify("ExecuteDbDataReaderAsync", Times.Once(), CommandBehavior.SequentialAccess, cancellationToken);

                mockInterceptor.Verify(
                    m => m.AsyncReaderExecuting(mockCommand.Object, CommandBehavior.SequentialAccess, interceptionContext));

                mockInterceptor.Verify(
                    m => m.AsyncReaderExecuted(mockCommand.Object, CommandBehavior.SequentialAccess, result, interceptionContext));
            }

            [Fact]
            public void Dispatch_method_executes_command_even_if_no_interceptors_are_registered()
            {
                var result = new Task<DbDataReader>(() => new Mock<DbDataReader>().Object);
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Protected()
                           .Setup<Task<DbDataReader>>(
                               "ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                           .Returns(result);

                Assert.Same(
                    result,
                    new DbCommandDispatcher().AsyncReader(
                        mockCommand.Object, CommandBehavior.SequentialAccess, CancellationToken.None, new DbInterceptionContext()));

                mockCommand.Protected()
                           .Verify("ExecuteDbDataReaderAsync", Times.Once(), ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>());
            }

            [Fact]
            public void Dispatch_method_checks_for_nulls()
            {
                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(
                        () =>
                        new DbCommandDispatcher().AsyncReader(
                            null, CommandBehavior.Default, CancellationToken.None, new DbInterceptionContext())).ParamName);

                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(
                        () =>
                        new DbCommandDispatcher().AsyncReader(
                            new Mock<DbCommand>().Object, CommandBehavior.Default, CancellationToken.None, null)).ParamName);
            }
        }
#endif
    }
}
