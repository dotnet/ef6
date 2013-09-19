// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure.Interception;
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
                var interceptionContext = new DbCommandInterceptionContext();
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteNonQuery()).Returns(11);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<int>>((c, i) =>
                        {
                            Assert.Equal(11, i.Result);
                            Assert.Equal(11, i.OriginalResult);
                            Assert.False(i.IsExecutionSuppressed);
                            i.Result = 13;
                            Assert.Equal(13, i.Result);
                            Assert.Equal(11, i.OriginalResult);
                            Assert.False(i.IsExecutionSuppressed);
                        });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                Assert.Equal(13, dispatcher.NonQuery(mockCommand.Object, interceptionContext));

                mockCommand.Verify(m => m.ExecuteNonQuery());
                mockInterceptor.Verify(m => m.NonQueryExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()));
                mockInterceptor.Verify(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()));
            }

            [Fact]
            public void Dispatch_method_does_not_execute_command_if_result_is_already_set()
            {
                var interceptionContext = new DbCommandInterceptionContext();
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteNonQuery()).Throws(new Exception("Bang!"));

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.NonQueryExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<int>>((c, i) =>
                    {
                        Assert.False(i.IsExecutionSuppressed);
                        i.Result = 13;
                        Assert.True(i.IsExecutionSuppressed);
                    });
                mockInterceptor.Setup(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<int>>((c, i) =>
                    {
                        Assert.True(i.IsExecutionSuppressed);
                        Assert.Equal(13, i.Result);
                        Assert.Equal(0, i.OriginalResult);
                        i.Result = 15;
                        Assert.True(i.IsExecutionSuppressed);
                        Assert.Equal(15, i.Result);
                        Assert.Equal(0, i.OriginalResult);
                    });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                Assert.Equal(15, dispatcher.NonQuery(mockCommand.Object, interceptionContext));

                mockCommand.Verify(m => m.ExecuteNonQuery(), Times.Never());
                mockInterceptor.Verify(m => m.NonQueryExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()));
                mockInterceptor.Verify(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()));
            }

            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_even_if_exception_thrown()
            {
                var interceptionContext = new DbCommandInterceptionContext();
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteNonQuery()).Throws(new Exception("Bang!"));

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<int>>((c, i) =>
                    {
                        Assert.Equal("Bang!", i.Exception.Message);
                        Assert.Equal("Bang!", i.OriginalException.Message);
                        Assert.False(i.IsExecutionSuppressed);
                        i.Exception = new Exception("Bing!");
                        Assert.Equal("Bing!", i.Exception.Message);
                        Assert.Equal("Bang!", i.OriginalException.Message);
                        Assert.False(i.IsExecutionSuppressed);
                    });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var exception = Assert.Throws<Exception>(() => dispatcher.NonQuery(mockCommand.Object, interceptionContext));

                Assert.Equal("Bing!", exception.Message);

                mockCommand.Verify(m => m.ExecuteNonQuery());
                mockInterceptor.Verify(m => m.NonQueryExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()));
                mockInterceptor.Verify(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()));
            }

            [Fact]
            public void Dispatch_method_does_not_execute_command_if_exception_is_already_set()
            {
                var interceptionContext = new DbCommandInterceptionContext();
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteNonQuery()).Throws(new Exception("Bang!"));

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.NonQueryExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<int>>((c, i) =>
                    {
                        Assert.False(i.IsExecutionSuppressed);
                        i.Exception = new Exception("Bing!");
                        Assert.True(i.IsExecutionSuppressed);
                    });
                mockInterceptor.Setup(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<int>>((c, i) =>
                    {
                        Assert.Equal("Bing!", i.Exception.Message);
                        Assert.Null(i.OriginalException);
                        Assert.True(i.IsExecutionSuppressed);
                        i.Exception = new Exception("Bung!");
                        Assert.Equal("Bung!", i.Exception.Message);
                        Assert.Null(i.OriginalException);
                        Assert.True(i.IsExecutionSuppressed);
                    });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var exception = Assert.Throws<Exception>(() => dispatcher.NonQuery(mockCommand.Object, interceptionContext));

                Assert.Equal("Bung!", exception.Message);

                mockCommand.Verify(m => m.ExecuteNonQuery(), Times.Never());
                mockInterceptor.Verify(m => m.NonQueryExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()));
                mockInterceptor.Verify(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()));
            }

            [Fact]
            public void Dispatch_method_does_not_execute_command_if_explicitly_suppressed_and_can_still_be_made_to_throw()
            {
                var interceptionContext = new DbCommandInterceptionContext();
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteNonQuery()).Throws(new Exception("Bang!"));

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.NonQueryExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<int>>((c, i) =>
                    {
                        Assert.False(i.IsExecutionSuppressed);
                        i.SuppressExecution();
                        Assert.True(i.IsExecutionSuppressed);
                    });
                mockInterceptor.Setup(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<int>>((c, i) =>
                    {
                        Assert.Null(i.Exception);
                        Assert.Null(i.OriginalException);
                        Assert.Equal(0, i.Result);
                        Assert.Equal(0, i.OriginalResult);
                        Assert.True(i.IsExecutionSuppressed);
                        i.Exception = new Exception("Bung!");
                        Assert.Equal("Bung!", i.Exception.Message);
                        Assert.Null(i.OriginalException);
                        Assert.Equal(0, i.Result);
                        Assert.Equal(0, i.OriginalResult);
                        Assert.True(i.IsExecutionSuppressed);
                    });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var exception = Assert.Throws<Exception>(() => dispatcher.NonQuery(mockCommand.Object, interceptionContext));

                Assert.Equal("Bung!", exception.Message);

                mockCommand.Verify(m => m.ExecuteNonQuery(), Times.Never());
                mockInterceptor.Verify(m => m.NonQueryExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()));
                mockInterceptor.Verify(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()));
            }

            [Fact]
            public void Dispatch_method_swallows_exception_if_interceptor_sets_Exception_to_null()
            {
                var interceptionContext = new DbCommandInterceptionContext();
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteNonQuery()).Throws(new Exception("Bang!"));

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<int>>((c, i) =>
                    {
                        Assert.Equal("Bang!", i.Exception.Message);
                        Assert.Equal("Bang!", i.OriginalException.Message);
                        Assert.Equal(0, i.Result);
                        Assert.Equal(0, i.OriginalResult);
                        Assert.False(i.IsExecutionSuppressed);
                        i.Exception = null;
                        i.Result = 13;
                        Assert.Null(i.Exception);
                        Assert.Equal("Bang!", i.OriginalException.Message);
                        Assert.Equal(13, i.Result);
                        Assert.Equal(0, i.OriginalResult);
                        Assert.False(i.IsExecutionSuppressed);
                    });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                Assert.Equal(13, dispatcher.NonQuery(mockCommand.Object, interceptionContext));

                mockCommand.Verify(m => m.ExecuteNonQuery());
                mockInterceptor.Verify(m => m.NonQueryExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()));
                mockInterceptor.Verify(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()));
            }

            [Fact]
            public void Dispatch_method_executes_command_even_if_no_interceptors_are_registered()
            {
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteNonQuery()).Returns(11);

                Assert.Equal(11, new DbCommandDispatcher().NonQuery(mockCommand.Object, new DbCommandInterceptionContext()));

                mockCommand.Verify(m => m.ExecuteNonQuery());
            }

            [Fact]
            public void Dispatch_method_checks_for_nulls()
            {
                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().NonQuery(null, new DbCommandInterceptionContext())).ParamName);

                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().NonQuery(new Mock<DbCommand>().Object, null)).ParamName);
            }

            [Fact] // CodePlex 1140
            public void Dispatch_method_does_not_populate_given_interception_context_with_execution_results()
            {
                var interceptionContext = new DbCommandInterceptionContext<int> { Exception = new Exception("Bang!") };

                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteNonQuery()).Returns(11);

                var dispatcher = new DbCommandDispatcher();
                dispatcher.InternalDispatcher.Add(new Mock<IDbCommandInterceptor>().Object);

                Assert.Equal(11, dispatcher.NonQuery(mockCommand.Object, interceptionContext));

                mockCommand.Verify(m => m.ExecuteNonQuery());

                Assert.Equal(0, interceptionContext.Result);
                Assert.Equal("Bang!", interceptionContext.Exception.Message);
            }
        }

        public class Scalar : TestBase
        {
            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_which_can_change_result()
            {
                var interceptionContext = new DbCommandInterceptionContext();
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteScalar()).Returns(11);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.ScalarExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<object>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<object>>((c, i) =>
                    {
                        Assert.Equal(11, i.Result);
                        Assert.Equal(11, i.OriginalResult);
                        Assert.False(i.IsExecutionSuppressed);
                        i.Result = 13;
                        Assert.Equal(13, i.Result);
                        Assert.Equal(11, i.OriginalResult);
                        Assert.False(i.IsExecutionSuppressed);
                    });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                Assert.Equal(13, dispatcher.Scalar(mockCommand.Object, interceptionContext));

                mockCommand.Verify(m => m.ExecuteScalar());
                mockInterceptor.Verify(m => m.ScalarExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<object>>()));
                mockInterceptor.Verify(m => m.ScalarExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<object>>()));
            }

            [Fact]
            public void Dispatch_method_does_not_execute_command_if_result_is_already_set()
            {
                var interceptionContext = new DbCommandInterceptionContext();
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteScalar()).Throws(new Exception("Bang!"));

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.ScalarExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<object>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<object>>((c, i) =>
                    {
                        Assert.False(i.IsExecutionSuppressed);
                        i.Result = 13;
                        Assert.True(i.IsExecutionSuppressed);
                    });
                mockInterceptor.Setup(m => m.ScalarExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<object>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<object>>((c, i) =>
                    {
                        Assert.True(i.IsExecutionSuppressed);
                        Assert.Equal(13, i.Result);
                        Assert.Null(i.OriginalResult);
                        i.Result = 15;
                        Assert.True(i.IsExecutionSuppressed);
                        Assert.Equal(15, i.Result);
                        Assert.Null(i.OriginalResult);
                    });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                Assert.Equal(15, dispatcher.Scalar(mockCommand.Object, interceptionContext));

                mockCommand.Verify(m => m.ExecuteScalar(), Times.Never());
                mockInterceptor.Verify(m => m.ScalarExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<object>>()));
                mockInterceptor.Verify(m => m.ScalarExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<object>>()));
            }

            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_even_if_exception_thrown()
            {
                var interceptionContext = new DbCommandInterceptionContext();
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteScalar()).Throws(new Exception("Bang!"));

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.ScalarExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<object>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<object>>((c, i) =>
                    {
                        Assert.Equal("Bang!", i.Exception.Message);
                        Assert.Equal("Bang!", i.OriginalException.Message);
                        Assert.False(i.IsExecutionSuppressed);
                        i.Exception = new Exception("Bing!");
                        Assert.Equal("Bing!", i.Exception.Message);
                        Assert.Equal("Bang!", i.OriginalException.Message);
                        Assert.False(i.IsExecutionSuppressed);
                    });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var exception = Assert.Throws<Exception>(() => dispatcher.Scalar(mockCommand.Object, interceptionContext));

                Assert.Equal("Bing!", exception.Message);

                mockCommand.Verify(m => m.ExecuteScalar());
                mockInterceptor.Verify(m => m.ScalarExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<object>>()));
                mockInterceptor.Verify(m => m.ScalarExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<object>>()));
            }

            [Fact]
            public void Dispatch_method_executes_command_even_if_no_interceptors_are_registered()
            {
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteScalar()).Returns(11);

                Assert.Equal(11, new DbCommandDispatcher().Scalar(mockCommand.Object, new DbCommandInterceptionContext()));

                mockCommand.Verify(m => m.ExecuteScalar());
            }

            [Fact]
            public void Dispatch_method_checks_for_nulls()
            {
                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().Scalar(null, new DbCommandInterceptionContext())).ParamName);

                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().Scalar(new Mock<DbCommand>().Object, null)).ParamName);
            }

            [Fact] // CodePlex 1140
            public void Dispatch_method_does_not_populate_given_interception_context_with_execution_results()
            {
                var interceptionContext = new DbCommandInterceptionContext<object> { Exception = new Exception("Bang!") };

                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteScalar()).Returns(11);

                var dispatcher = new DbCommandDispatcher();
                dispatcher.InternalDispatcher.Add(new Mock<IDbCommandInterceptor>().Object);

                Assert.Equal(11, dispatcher.Scalar(mockCommand.Object, interceptionContext));

                mockCommand.Verify(m => m.ExecuteScalar());

                Assert.Null(interceptionContext.Result);
                Assert.Equal("Bang!", interceptionContext.Exception.Message);
            }
        }

        public class Reader : TestBase
        {
            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_which_can_change_result()
            {
                var interceptionContext = new DbCommandInterceptionContext().WithCommandBehavior(CommandBehavior.SequentialAccess);

                var reader = new Mock<DbDataReader>().Object;
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Protected()
                           .Setup<DbDataReader>("ExecuteDbDataReader", CommandBehavior.SequentialAccess)
                           .Returns(reader);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                var interceptReader = new Mock<DbDataReader>().Object;
                mockInterceptor.Setup(
                    m => m.ReaderExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<DbDataReader>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<DbDataReader>>((c, i) =>
                    {
                        Assert.Same(reader, i.Result);
                        Assert.Same(reader, i.OriginalResult);
                        Assert.False(i.IsExecutionSuppressed);
                        i.Result = interceptReader;
                        Assert.Same(interceptReader, i.Result);
                        Assert.Same(reader, i.OriginalResult);
                        Assert.False(i.IsExecutionSuppressed);
                    });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                Assert.Same(interceptReader, dispatcher.Reader(mockCommand.Object, interceptionContext));

                mockCommand.Protected().Verify("ExecuteDbDataReader", Times.Once(), CommandBehavior.SequentialAccess);
                mockInterceptor.Verify(m => m.ReaderExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<DbDataReader>>()));
                mockInterceptor.Verify(m => m.ReaderExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<DbDataReader>>()));
            }

            [Fact]
            public void Dispatch_method_does_not_execute_command_if_result_is_already_set()
            {
                var interceptionContext = new DbCommandInterceptionContext()
                    .WithCommandBehavior(CommandBehavior.SequentialAccess);

                var interceptReader1 = new Mock<DbDataReader>().Object;
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Protected()
                           .Setup<DbDataReader>("ExecuteDbDataReader", CommandBehavior.SequentialAccess)
                           .Throws(new Exception("Bang!"));

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                var interceptReader2 = new Mock<DbDataReader>().Object;
                mockInterceptor.Setup(
                    m => m.ReaderExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<DbDataReader>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<DbDataReader>>((c, i) =>
                    {
                        Assert.False(i.IsExecutionSuppressed);
                        i.Result = interceptReader1;
                        Assert.True(i.IsExecutionSuppressed);
                    });
                mockInterceptor.Setup(
                    m => m.ReaderExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<DbDataReader>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<DbDataReader>>((c, i) =>
                    {
                        Assert.True(i.IsExecutionSuppressed);
                        Assert.Same(interceptReader1, i.Result);
                        Assert.Null(i.OriginalResult);
                        i.Result = interceptReader2;
                        Assert.True(i.IsExecutionSuppressed);
                        Assert.Same(interceptReader2, i.Result);
                        Assert.Null(i.OriginalResult);
                    });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                Assert.Same(interceptReader2, dispatcher.Reader(mockCommand.Object, interceptionContext));

                mockCommand.Protected().Verify("ExecuteDbDataReader", Times.Never(), CommandBehavior.SequentialAccess);
                mockInterceptor.Verify(m => m.ReaderExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<DbDataReader>>()));
                mockInterceptor.Verify(m => m.ReaderExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<DbDataReader>>()));
            }

            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_even_if_exception_thrown()
            {
                var interceptionContext = new DbCommandInterceptionContext().WithCommandBehavior(CommandBehavior.SequentialAccess);

                var mockCommand = new Mock<DbCommand>();
                mockCommand.Protected()
                           .Setup<DbDataReader>("ExecuteDbDataReader", CommandBehavior.SequentialAccess)
                           .Throws(new Exception("Bang!"));

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(
                    m => m.ReaderExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<DbDataReader>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<DbDataReader>>((c, i) =>
                    {
                        Assert.Equal("Bang!", i.Exception.Message);
                        Assert.Equal("Bang!", i.OriginalException.Message);
                        Assert.False(i.IsExecutionSuppressed);
                        i.Exception = new Exception("Bing!");
                        Assert.Equal("Bing!", i.Exception.Message);
                        Assert.Equal("Bang!", i.OriginalException.Message);
                        Assert.False(i.IsExecutionSuppressed);
                    });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var exception = Assert.Throws<Exception>(() => dispatcher.Reader(mockCommand.Object, interceptionContext));

                Assert.Equal("Bing!", exception.Message);

                mockCommand.Protected().Verify("ExecuteDbDataReader", Times.Once(), CommandBehavior.SequentialAccess);
                mockInterceptor.Verify(m => m.ReaderExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<DbDataReader>>()));
                mockInterceptor.Verify(m => m.ReaderExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<DbDataReader>>()));
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
                    new DbCommandDispatcher().Reader(
                        mockCommand.Object,
                        new DbCommandInterceptionContext().WithCommandBehavior(CommandBehavior.SequentialAccess)));

                mockCommand.Protected().Verify("ExecuteDbDataReader", Times.Once(), CommandBehavior.SequentialAccess);
            }

            [Fact]
            public void Dispatch_method_checks_for_nulls()
            {
                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().Reader(null, new DbCommandInterceptionContext())).ParamName);

                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().Reader(new Mock<DbCommand>().Object, null)).ParamName);
            }

            [Fact] // CodePlex 1140
            public void Dispatch_method_does_not_populate_given_interception_context_with_execution_results()
            {
                var interceptionContext = new DbCommandInterceptionContext<DbDataReader> { Exception = new Exception("Bang!") };

                var mockCommand = new Mock<DbCommand>();
                mockCommand.Protected()
                           .Setup<DbDataReader>("ExecuteDbDataReader", It.IsAny<CommandBehavior>())
                           .Returns(new Mock<DbDataReader>().Object);

                var dispatcher = new DbCommandDispatcher();
                dispatcher.InternalDispatcher.Add(new Mock<IDbCommandInterceptor>().Object);

                Assert.NotNull(dispatcher.Reader(mockCommand.Object, interceptionContext));

                mockCommand.Protected().Verify("ExecuteDbDataReader", Times.Once(), It.IsAny<CommandBehavior>());

                Assert.Null(interceptionContext.Result);
                Assert.Equal("Bang!", interceptionContext.Exception.Message);
            }
        }

#if !NET40
        public class AsyncNonQuery : TestBase
        {
            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_which_can_change_result()
            {
                var cancellationToken = new CancellationToken();

                var mockCommand = new Mock<DbCommand>();
                var result = new Task<int>(() => 11);

                mockCommand.Setup(m => m.ExecuteNonQueryAsync(cancellationToken)).Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<int>>((c, i) =>
                    {
                        Assert.Equal(11, i.Result);
                        Assert.Equal(11, i.OriginalResult);
                        Assert.False(i.IsExecutionSuppressed);
                        i.Result = 13;
                        Assert.Equal(13, i.Result);
                        Assert.Equal(11, i.OriginalResult);
                        Assert.False(i.IsExecutionSuppressed);
                    });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult = dispatcher.NonQueryAsync(mockCommand.Object, new DbCommandInterceptionContext(), cancellationToken);
                Assert.NotSame(result, interceptResult);

                mockCommand.Verify(m => m.ExecuteNonQueryAsync(cancellationToken));
                mockInterceptor.Verify(
                    m => m.NonQueryExecuting(mockCommand.Object, It.Is<DbCommandInterceptionContext<int>>(c => c.IsAsync)));
                mockInterceptor.Verify(
                    m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()), Times.Never());

                result.Start();
                var awaited = AwaitMe(interceptResult);
                awaited.Wait();

                Assert.False(interceptResult.IsCanceled);
                Assert.False(awaited.IsCanceled);
                Assert.False(interceptResult.IsFaulted);
                Assert.False(awaited.IsFaulted);

                Assert.Equal(13, interceptResult.Result);
                Assert.Equal(13, awaited.Result);

                mockInterceptor.Verify(
                    m => m.NonQueryExecuted(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext<int>>(
                            c => c.IsAsync && c.TaskStatus.HasFlag(TaskStatus.RanToCompletion) && c.Exception == null)));
            }

            [Fact]
            public void Dispatch_method_does_not_execute_command_if_result_is_already_set()
            {
                var cancellationToken = new CancellationToken();

                var mockCommand = new Mock<DbCommand>();
                var result = new Task<int>(() => { throw new Exception("Bang!"); });

                mockCommand.Setup(m => m.ExecuteNonQueryAsync(cancellationToken)).Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.NonQueryExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<int>>((c, i) =>
                    {
                        Assert.False(i.IsExecutionSuppressed);
                        i.Result = 11;
                        Assert.True(i.IsExecutionSuppressed);
                    });
                mockInterceptor.Setup(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<int>>((c, i) =>
                    {
                        Assert.True(i.IsExecutionSuppressed);
                        Assert.Equal(11, i.Result);
                        Assert.Equal(0, i.OriginalResult);
                        i.Result = 13;
                        Assert.True(i.IsExecutionSuppressed);
                        Assert.Equal(13, i.Result);
                        Assert.Equal(0, i.OriginalResult);                    
                    });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult = dispatcher.NonQueryAsync(mockCommand.Object, new DbCommandInterceptionContext(), cancellationToken);
                Assert.NotSame(result, interceptResult);

                mockCommand.Verify(m => m.ExecuteNonQueryAsync(cancellationToken), Times.Never());
                mockInterceptor.Verify(
                    m => m.NonQueryExecuting(mockCommand.Object, It.Is<DbCommandInterceptionContext<int>>(c => c.IsAsync)));

                // Note that if the command is not executed then there is no async operation and "after" interceptors are
                // executed immediately and synchronously
                mockInterceptor.Verify(
                    m => m.NonQueryExecuted(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext<int>>(
                            c => c.IsAsync && c.TaskStatus.HasFlag(TaskStatus.RanToCompletion) && c.Exception == null)));

                var awaited = AwaitMe(interceptResult);
                awaited.Wait();

                Assert.False(interceptResult.IsCanceled);
                Assert.False(awaited.IsCanceled);
                Assert.False(interceptResult.IsFaulted);
                Assert.False(awaited.IsFaulted);

                Assert.Equal(13, interceptResult.Result);
                Assert.Equal(13, awaited.Result);
            }

            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_even_if_operation_throws()
            {
                var mockCommand = new Mock<DbCommand>();
                var result = new Task<int>(() => { throw new Exception("Bang!"); });

                mockCommand.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<int>>((c, i) =>
                    {
                        Assert.Equal("Bang!", i.Exception.Message);
                        Assert.Equal("Bang!", i.OriginalException.Message);
                        Assert.False(i.IsExecutionSuppressed);
                        i.Exception = new Exception("Bing!");
                        Assert.Equal("Bing!", i.Exception.Message);
                        Assert.Equal("Bang!", i.OriginalException.Message);
                        Assert.False(i.IsExecutionSuppressed);
                    });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult = dispatcher.NonQueryAsync(mockCommand.Object, new DbCommandInterceptionContext(), CancellationToken.None);
                Assert.NotSame(result, interceptResult);

                mockCommand.Verify(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()));
                mockInterceptor.Verify(m => m.NonQueryExecuting(mockCommand.Object, It.Is<DbCommandInterceptionContext<int>>(c => c.IsAsync)));
                mockInterceptor.Verify(
                    m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()), Times.Never());

                result.Start();
                var awaited = AwaitMe(interceptResult);

                var exception = Assert.Throws<AggregateException>(() => awaited.Wait()).InnerException;

                Assert.Equal("Bing!", exception.Message);

                Assert.False(interceptResult.IsCanceled);
                Assert.False(awaited.IsCanceled);
                Assert.True(interceptResult.IsFaulted);
                Assert.True(awaited.IsFaulted);

                Assert.Throws<AggregateException>(() => interceptResult.Result);
                Assert.Throws<AggregateException>(() => awaited.Result);

                mockInterceptor.Verify(
                    m => m.NonQueryExecuted(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext<int>>(
                            c => c.IsAsync && c.TaskStatus.HasFlag(TaskStatus.Faulted) && c.Exception.Message == "Bing!" && c.Result == 0)));
            }

            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_even_if_operation_is_canceled()
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;

                var mockCommand = new Mock<DbCommand>();
                var result = new Task<int>(() => 11, cancellationToken);

                mockCommand.Setup(m => m.ExecuteNonQueryAsync(cancellationToken)).Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()));

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult = dispatcher.NonQueryAsync(
                    mockCommand.Object, new DbCommandInterceptionContext(), cancellationToken);
                Assert.NotSame(result, interceptResult);

                mockCommand.Verify(m => m.ExecuteNonQueryAsync(cancellationToken));
                mockInterceptor.Verify(
                    m => m.NonQueryExecuting(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext<int>>(c => c.IsAsync)));
                mockInterceptor.Verify(
                    m => m.NonQueryExecuted(
                        mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()), Times.Never());

                var awaited = AwaitMe(interceptResult);

                cancellationTokenSource.Cancel();

                Assert.Throws<AggregateException>(() => awaited.Wait());

                Assert.True(interceptResult.IsCanceled);
                Assert.True(awaited.IsCanceled);
                Assert.False(interceptResult.IsFaulted);
                Assert.False(awaited.IsFaulted);

                Assert.Throws<AggregateException>(() => interceptResult.Result);
                Assert.Throws<AggregateException>(() => awaited.Result);

                mockInterceptor.Verify(
                    m => m.NonQueryExecuted(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext<int>>(
                            c => c.IsAsync && c.TaskStatus.HasFlag(TaskStatus.Canceled) && c.Exception == null && c.Result == 0)));
            }

            [Fact]
            public void Dispatch_method_does_not_execute_command_if_exception_is_already_set()
            {
                var cancellationToken = new CancellationToken();

                var mockCommand = new Mock<DbCommand>();
                var result = new Task<int>(() => { throw new Exception("Bang!"); });

                mockCommand.Setup(m => m.ExecuteNonQueryAsync(cancellationToken)).Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.NonQueryExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<int>>((c, i) =>
                    {
                        Assert.True(i.IsAsync);
                        Assert.False(i.IsExecutionSuppressed);
                        i.Exception = new Exception("Bing!");
                        Assert.True(i.IsExecutionSuppressed);
                    });
                mockInterceptor.Setup(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<int>>((c, i) =>
                    {
                        Assert.True(i.IsAsync);
                        Assert.Equal("Bing!", i.Exception.Message);
                        Assert.Null(i.OriginalException);
                        Assert.True(i.IsExecutionSuppressed);
                        i.Exception = new Exception("Bung!");
                        Assert.Equal("Bung!", i.Exception.Message);
                        Assert.Null(i.OriginalException);
                        Assert.True(i.IsExecutionSuppressed);
                    });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult = dispatcher.NonQueryAsync(mockCommand.Object, new DbCommandInterceptionContext(), cancellationToken);
                Assert.NotSame(result, interceptResult);

                mockCommand.Verify(m => m.ExecuteNonQueryAsync(cancellationToken), Times.Never());
                mockInterceptor.Verify(
                    m => m.NonQueryExecuting(mockCommand.Object, It.Is<DbCommandInterceptionContext<int>>(c => c.IsAsync)));

                // Note that if the command is not executed then there is no async operation and "after" interceptors are
                // executed immediately and synchronously
                mockInterceptor.Verify(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()));

                var awaited = AwaitMe(interceptResult);

                var exception = Assert.Throws<AggregateException>(() => awaited.Wait()).InnerException;

                Assert.Equal("Bung!", exception.Message);

                Assert.False(interceptResult.IsCanceled);
                Assert.False(awaited.IsCanceled);
                Assert.True(interceptResult.IsFaulted);
                Assert.True(awaited.IsFaulted);
            }

            [Fact]
            public void Dispatch_method_does_not_execute_command_if_explicitly_suppressed_and_can_still_be_made_to_throw()
            {
                var cancellationToken = new CancellationToken();

                var mockCommand = new Mock<DbCommand>();
                var result = new Task<int>(() => { throw new Exception("Bang!"); });

                mockCommand.Setup(m => m.ExecuteNonQueryAsync(cancellationToken)).Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.NonQueryExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<int>>((c, i) =>
                    {
                        Assert.True(i.IsAsync);
                        Assert.False(i.IsExecutionSuppressed);
                        i.SuppressExecution();
                        Assert.True(i.IsExecutionSuppressed);
                    });
                mockInterceptor.Setup(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<int>>((c, i) =>
                    {
                        Assert.True(i.IsAsync);
                        Assert.Null(i.Exception);
                        Assert.Null(i.OriginalException);
                        Assert.Equal(0, i.Result);
                        Assert.Equal(0, i.OriginalResult);
                        Assert.True(i.IsExecutionSuppressed);
                        i.Exception = new Exception("Bung!");
                        Assert.Equal("Bung!", i.Exception.Message);
                        Assert.Null(i.OriginalException);
                        Assert.Equal(0, i.Result);
                        Assert.Equal(0, i.OriginalResult);
                        Assert.True(i.IsExecutionSuppressed);
                    });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult = dispatcher.NonQueryAsync(mockCommand.Object, new DbCommandInterceptionContext(), cancellationToken);
                Assert.NotSame(result, interceptResult);

                mockCommand.Verify(m => m.ExecuteNonQueryAsync(cancellationToken), Times.Never());
                mockInterceptor.Verify(
                    m => m.NonQueryExecuting(mockCommand.Object, It.Is<DbCommandInterceptionContext<int>>(c => c.IsAsync)));

                // Note that if the command is not executed then there is no async operation and "after" interceptors are
                // executed immediately and synchronously
                mockInterceptor.Verify(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()));

                var awaited = AwaitMe(interceptResult);

                var exception = Assert.Throws<AggregateException>(() => awaited.Wait()).InnerException;

                Assert.Equal("Bung!", exception.Message);

                Assert.False(interceptResult.IsCanceled);
                Assert.False(awaited.IsCanceled);
                Assert.True(interceptResult.IsFaulted);
                Assert.True(awaited.IsFaulted);
            }

            [Fact]
            public void Dispatch_method_swallows_exception_if_interceptor_sets_Exception_to_null()
            {
                var mockCommand = new Mock<DbCommand>();
                var result = new Task<int>(() => { throw new Exception("Bang!"); });

                mockCommand.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<int>>((c, i) =>
                    {
                        Assert.Equal("Bang!", i.Exception.Message);
                        Assert.Equal("Bang!", i.OriginalException.Message);
                        Assert.Equal(0, i.Result);
                        Assert.Equal(0, i.OriginalResult);
                        Assert.False(i.IsExecutionSuppressed);
                        i.Exception = null;
                        i.Result = 17;
                        Assert.Null(i.Exception);
                        Assert.Equal("Bang!", i.OriginalException.Message);
                        Assert.Equal(17, i.Result);
                        Assert.Equal(0, i.OriginalResult);
                        Assert.False(i.IsExecutionSuppressed);
                    });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult = dispatcher.NonQueryAsync(mockCommand.Object, new DbCommandInterceptionContext(), CancellationToken.None);
                Assert.NotSame(result, interceptResult);

                mockCommand.Verify(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()));
                mockInterceptor.Verify(m => m.NonQueryExecuting(mockCommand.Object, It.Is<DbCommandInterceptionContext<int>>(c => c.IsAsync)));
                mockInterceptor.Verify(
                    m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()), Times.Never());

                result.Start();
                var awaited = AwaitMe(interceptResult);
                awaited.Wait();

                Assert.False(interceptResult.IsCanceled);
                Assert.False(awaited.IsCanceled);
                Assert.False(interceptResult.IsFaulted);
                Assert.False(awaited.IsFaulted);

                Assert.Equal(17, interceptResult.Result);
                Assert.Equal(17, awaited.Result);

                mockInterceptor.Verify(
                    m => m.NonQueryExecuted(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext<int>>(
                            c => c.IsAsync && c.TaskStatus.HasFlag(TaskStatus.RanToCompletion) && c.Exception == null)));
            }

            [Fact]
            public void Dispatch_method_executes_command_even_if_no_interceptors_are_registered()
            {
                var mockCommand = new Mock<DbCommand>();
                var result = Task.FromResult(11);
                mockCommand.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Returns(result);

                Assert.Same(
                    result,
                    new DbCommandDispatcher().NonQueryAsync(mockCommand.Object, new DbCommandInterceptionContext(), CancellationToken.None));

                mockCommand.Verify(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()));
            }

            [Fact]
            public void Dispatch_method_checks_for_nulls()
            {
                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().NonQueryAsync(null, new DbCommandInterceptionContext(), CancellationToken.None))
                          .ParamName);

                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().NonQueryAsync(new Mock<DbCommand>().Object, null, CancellationToken.None)).ParamName);
            }

            [Fact] // CodePlex 1140
            public void Dispatch_method_does_not_populate_given_interception_context_with_execution_results()
            {
                var interceptionContext = new DbCommandInterceptionContext<int> { Exception = new Exception("Bang!") };

                var mockCommand = new Mock<DbCommand>();
                var result = new Task<int>(() => 11);

                mockCommand.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Returns(result);

                var dispatcher = new DbCommandDispatcher();
                dispatcher.InternalDispatcher.Add(new Mock<IDbCommandInterceptor>().Object);

                var interceptResult = dispatcher.NonQueryAsync(mockCommand.Object, interceptionContext, new CancellationToken());

                mockCommand.Verify(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()));

                result.Start();
                interceptResult.Wait();

                Assert.Equal(11, interceptResult.Result);

                Assert.Equal(0, interceptionContext.Result);
                Assert.Equal("Bang!", interceptionContext.Exception.Message);
            }
        }

        public class AsyncScalar : TestBase
        {
            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_which_can_change_result()
            {
                var interceptionContext = new DbCommandInterceptionContext();
                var cancellationToken = new CancellationToken();

                var mockCommand = new Mock<DbCommand>();
                var result = new Task<object>(() => 11);

                mockCommand.Setup(m => m.ExecuteScalarAsync(cancellationToken)).Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(
                    m => m.ScalarExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<object>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<object>>((c, i) =>
                    {
                        Assert.Equal(11, i.Result);
                        Assert.Equal(11, i.OriginalResult);
                        Assert.False(i.IsExecutionSuppressed);
                        i.Result = 13;
                        Assert.Equal(13, i.Result);
                        Assert.Equal(11, i.OriginalResult);
                        Assert.False(i.IsExecutionSuppressed);
                    });


                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult = dispatcher.ScalarAsync(mockCommand.Object, interceptionContext, cancellationToken);
                Assert.NotSame(result, interceptResult);

                mockCommand.Verify(m => m.ExecuteScalarAsync(cancellationToken));
                mockInterceptor.Verify(
                    m => m.ScalarExecuting(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext<object>>(c => c.IsAsync)));
                mockInterceptor.Verify(
                    m => m.ScalarExecuted(
                        mockCommand.Object, It.IsAny<DbCommandInterceptionContext<object>>()), Times.Never());

                result.Start();
                var awaited = AwaitMe(interceptResult);
                awaited.Wait();

                Assert.False(interceptResult.IsCanceled);
                Assert.False(awaited.IsCanceled);
                Assert.False(interceptResult.IsFaulted);
                Assert.False(awaited.IsFaulted);

                Assert.Equal(13, interceptResult.Result);
                Assert.Equal(13, awaited.Result);

                mockInterceptor.Verify(
                    m => m.ScalarExecuted(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext<object>>(
                            c => c.IsAsync && c.TaskStatus.HasFlag(TaskStatus.RanToCompletion) && c.Exception == null)));
            }

            [Fact]
            public void Dispatch_method_does_not_execute_command_if_result_is_already_set()
            {
                var interceptionContext = new DbCommandInterceptionContext();
                var cancellationToken = new CancellationToken();

                var mockCommand = new Mock<DbCommand>();
                var result = new Task<object>(() => { throw new Exception("Bang!"); });

                mockCommand.Setup(m => m.ExecuteScalarAsync(cancellationToken)).Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(
                    m => m.ScalarExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<object>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<object>>((c, i) =>
                    {
                        Assert.False(i.IsExecutionSuppressed);
                        i.Result = 11;
                        Assert.True(i.IsExecutionSuppressed);
                    });
                mockInterceptor.Setup(
                    m => m.ScalarExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<object>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<object>>((c, i) =>
                    {
                        Assert.True(i.IsExecutionSuppressed);
                        Assert.Equal(11, i.Result);
                        Assert.Null(i.OriginalResult);
                        i.Result = 13;
                        Assert.True(i.IsExecutionSuppressed);
                        Assert.Equal(13, i.Result);
                        Assert.Null(i.OriginalResult);
                    });


                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult = dispatcher.ScalarAsync(mockCommand.Object, interceptionContext, cancellationToken);
                Assert.NotSame(result, interceptResult);

                mockCommand.Verify(m => m.ExecuteScalarAsync(cancellationToken), Times.Never());
                mockInterceptor.Verify(
                    m => m.ScalarExecuting(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext<object>>(c => c.IsAsync)));

                // Note that if the command is not executed then there is no async operation and "after" interceptors are
                // executed immediately and synchronously
                mockInterceptor.Verify(
                    m => m.ScalarExecuted(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext<object>>(
                            c => c.IsAsync && c.TaskStatus.HasFlag(TaskStatus.RanToCompletion) && c.Exception == null)));

                var awaited = AwaitMe(interceptResult);
                awaited.Wait();

                Assert.False(interceptResult.IsCanceled);
                Assert.False(awaited.IsCanceled);
                Assert.False(interceptResult.IsFaulted);
                Assert.False(awaited.IsFaulted);

                Assert.Equal(13, interceptResult.Result);
                Assert.Equal(13, awaited.Result);
            }

            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_even_if_operation_throws()
            {
                var mockCommand = new Mock<DbCommand>();
                var result = new Task<object>(() => { throw new Exception("Bang!"); });

                mockCommand.Setup(m => m.ExecuteScalarAsync(It.IsAny<CancellationToken>())).Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.ScalarExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<object>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<object>>((c, i) =>
                    {
                        Assert.Equal("Bang!", i.Exception.Message);
                        Assert.Equal("Bang!", i.OriginalException.Message);
                        Assert.False(i.IsExecutionSuppressed);
                        i.Exception = new Exception("Bing!");
                        Assert.Equal("Bing!", i.Exception.Message);
                        Assert.Equal("Bang!", i.OriginalException.Message);
                        Assert.False(i.IsExecutionSuppressed);
                    });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult = dispatcher.ScalarAsync(mockCommand.Object, new DbCommandInterceptionContext(), CancellationToken.None);
                Assert.NotSame(result, interceptResult);

                mockCommand.Verify(m => m.ExecuteScalarAsync(It.IsAny<CancellationToken>()));
                mockInterceptor.Verify(
                    m => m.ScalarExecuting(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext<object>>(c => c.IsAsync)));
                mockInterceptor.Verify(
                    m => m.ScalarExecuted(
                        mockCommand.Object, It.IsAny<DbCommandInterceptionContext<object>>()), Times.Never());

                result.Start();
                var awaited = AwaitMe(interceptResult);

                var exception = Assert.Throws<AggregateException>(() => awaited.Wait()).InnerException;
                Assert.Equal("Bing!", exception.Message);

                Assert.False(interceptResult.IsCanceled);
                Assert.False(awaited.IsCanceled);
                Assert.True(interceptResult.IsFaulted);
                Assert.True(awaited.IsFaulted);

                Assert.Throws<AggregateException>(() => interceptResult.Result);
                Assert.Throws<AggregateException>(() => awaited.Result);

                mockInterceptor.Verify(
                    m => m.ScalarExecuted(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext<object>>(
                            c => c.IsAsync && c.TaskStatus.HasFlag(TaskStatus.Faulted) && c.Exception.Message == "Bing!" && c.Result == null)));
            }

            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_even_if_operation_canceled()
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;

                var mockCommand = new Mock<DbCommand>();
                var result = new Task<object>(() => 11, cancellationToken);

                mockCommand.Setup(m => m.ExecuteScalarAsync(cancellationToken)).Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(
                    m => m.ScalarExecuted(
                        mockCommand.Object, It.IsAny<DbCommandInterceptionContext<object>>()));

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult = dispatcher.ScalarAsync(mockCommand.Object, new DbCommandInterceptionContext(), cancellationToken);
                Assert.NotSame(result, interceptResult);

                mockCommand.Verify(m => m.ExecuteScalarAsync(cancellationToken));
                mockInterceptor.Verify(
                    m => m.ScalarExecuting(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext<object>>(c => c.IsAsync)));
                mockInterceptor.Verify(
                    m => m.ScalarExecuted(
                        mockCommand.Object, It.IsAny<DbCommandInterceptionContext<object>>()), Times.Never());

                var awaited = AwaitMe(interceptResult);

                cancellationTokenSource.Cancel();

                Assert.Throws<AggregateException>(() => awaited.Wait());

                Assert.True(interceptResult.IsCanceled);
                Assert.True(awaited.IsCanceled);
                Assert.False(interceptResult.IsFaulted);
                Assert.False(awaited.IsFaulted);

                Assert.Throws<AggregateException>(() => interceptResult.Result);
                Assert.Throws<AggregateException>(() => awaited.Result);

                mockInterceptor.Verify(
                    m => m.ScalarExecuted(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext<object>>(
                            c => c.IsAsync && c.TaskStatus.HasFlag(TaskStatus.Canceled) && c.Exception == null && c.Result == null)));
            }

            [Fact]
            public void Dispatch_method_executes_command_even_if_no_interceptors_are_registered()
            {
                var mockCommand = new Mock<DbCommand>();
                var result = Task.FromResult<object>(11);
                mockCommand.Setup(m => m.ExecuteScalarAsync(It.IsAny<CancellationToken>())).Returns(result);

                Assert.Same(
                    result,
                    new DbCommandDispatcher().ScalarAsync(mockCommand.Object, new DbCommandInterceptionContext(), CancellationToken.None));

                mockCommand.Verify(m => m.ExecuteScalarAsync(It.IsAny<CancellationToken>()));
            }

            [Fact]
            public void Dispatch_method_checks_for_nulls()
            {
                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().ScalarAsync(null, new DbCommandInterceptionContext(), CancellationToken.None))
                          .ParamName);

                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().ScalarAsync(new Mock<DbCommand>().Object, null, CancellationToken.None)).ParamName);
            }

            [Fact] // CodePlex 1140
            public void Dispatch_method_does_not_populate_given_interception_context_with_execution_results()
            {
                var interceptionContext = new DbCommandInterceptionContext<object> { Exception = new Exception("Bang!") };

                var mockCommand = new Mock<DbCommand>();
                var result = new Task<object>(() => 11);

                mockCommand.Setup(m => m.ExecuteScalarAsync(It.IsAny<CancellationToken>())).Returns(result);

                var dispatcher = new DbCommandDispatcher();
                dispatcher.InternalDispatcher.Add(new Mock<IDbCommandInterceptor>().Object);

                var interceptResult = dispatcher.ScalarAsync(mockCommand.Object, interceptionContext, new CancellationToken());

                mockCommand.Verify(m => m.ExecuteScalarAsync(It.IsAny<CancellationToken>()));

                result.Start();
                interceptResult.Wait();

                Assert.Equal(11, interceptResult.Result);

                Assert.Null(interceptionContext.Result);
                Assert.Equal("Bang!", interceptionContext.Exception.Message);
            }
        }

        public class AsyncReader : TestBase
        {
            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_which_can_change_result()
            {
                var interceptionContext = new DbCommandInterceptionContext().WithCommandBehavior(CommandBehavior.SequentialAccess);
                var cancellationToken = new CancellationToken();

                var originalReader = new Mock<DbDataReader>().Object;
                var result = new Task<DbDataReader>(() => originalReader);

                var mockCommand = new Mock<DbCommand>();
                mockCommand.Protected()
                           .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", CommandBehavior.SequentialAccess, cancellationToken)
                           .Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                var interceptReader = new Mock<DbDataReader>().Object;
                mockInterceptor.Setup(
                    m => m.ReaderExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<DbDataReader>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<DbDataReader>>((c, i) =>
                    {
                        Assert.Same(originalReader, i.Result);
                        Assert.Same(originalReader, i.OriginalResult);
                        Assert.False(i.IsExecutionSuppressed);
                        i.Result = interceptReader;
                        Assert.Same(interceptReader, i.Result);
                        Assert.Same(originalReader, i.OriginalResult);
                        Assert.False(i.IsExecutionSuppressed);
                    });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult =
                    dispatcher.ReaderAsync(mockCommand.Object, interceptionContext, cancellationToken);
                Assert.NotSame(result, interceptResult);

                mockCommand.Protected()
                           .Verify("ExecuteDbDataReaderAsync", Times.Once(), CommandBehavior.SequentialAccess, cancellationToken);

                mockInterceptor.Verify(
                    m => m.ReaderExecuting(
                        mockCommand.Object,
                        It.Is<DbCommandInterceptionContext<DbDataReader>>(c => c.IsAsync && c.CommandBehavior == CommandBehavior.SequentialAccess)));
                mockInterceptor.Verify(
                    m => m.ReaderExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<DbDataReader>>()),
                    Times.Never());

                result.Start();
                var awaited = AwaitMe(interceptResult);
                awaited.Wait();

                Assert.False(interceptResult.IsCanceled);
                Assert.False(awaited.IsCanceled);
                Assert.False(interceptResult.IsFaulted);
                Assert.False(awaited.IsFaulted);

                Assert.Same(interceptReader, interceptResult.Result);
                Assert.Same(interceptReader, awaited.Result);

                mockInterceptor.Verify(
                    m => m.ReaderExecuted(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext<DbDataReader>>(
                            c => c.IsAsync
                                 && c.TaskStatus.HasFlag(TaskStatus.RanToCompletion)
                                 && c.Exception == null
                                 && c.CommandBehavior == CommandBehavior.SequentialAccess)));
            }

            [Fact]
            public void Dispatch_method_does_not_execute_command_if_result_is_already_set()
            {
                var interceptionContext = new DbCommandInterceptionContext().WithCommandBehavior(CommandBehavior.SequentialAccess);
                var cancellationToken = new CancellationToken();

                var originalReader = new Mock<DbDataReader>().Object;
                var result = new Task<DbDataReader>(() => { throw new Exception("Bang!"); });

                var mockCommand = new Mock<DbCommand>();
                mockCommand.Protected()
                           .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", CommandBehavior.SequentialAccess, cancellationToken)
                           .Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                var interceptReader = new Mock<DbDataReader>().Object;
                mockInterceptor.Setup(
                    m => m.ReaderExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<DbDataReader>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<DbDataReader>>((c, i) =>
                    {
                        Assert.False(i.IsExecutionSuppressed);
                        i.Result = originalReader;
                        Assert.True(i.IsExecutionSuppressed);
                    });
                mockInterceptor.Setup(
                    m => m.ReaderExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<DbDataReader>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<DbDataReader>>((c, i) =>
                    {
                        Assert.True(i.IsExecutionSuppressed);
                        Assert.Same(originalReader, i.Result);
                        Assert.Null(i.OriginalResult);
                        i.Result = interceptReader;
                        Assert.True(i.IsExecutionSuppressed);
                        Assert.Same(interceptReader, i.Result);
                        Assert.Null(i.OriginalResult);
                    });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult =
                    dispatcher.ReaderAsync(mockCommand.Object, interceptionContext, cancellationToken);
                Assert.NotSame(result, interceptResult);

                mockCommand.Protected()
                           .Verify("ExecuteDbDataReaderAsync", Times.Never(), CommandBehavior.SequentialAccess, cancellationToken);

                mockInterceptor.Verify(
                    m => m.ReaderExecuting(
                        mockCommand.Object,
                        It.Is<DbCommandInterceptionContext<DbDataReader>>(c => c.IsAsync && c.CommandBehavior == CommandBehavior.SequentialAccess)));

                // Note that if the command is not executed then there is no async operation and "after" interceptors are
                // executed immediately and synchronously
                mockInterceptor.Verify(
                    m => m.ReaderExecuted(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext<DbDataReader>>(
                            c => c.IsAsync
                                 && c.TaskStatus.HasFlag(TaskStatus.RanToCompletion)
                                 && c.Exception == null
                                 && c.CommandBehavior == CommandBehavior.SequentialAccess)));

                var awaited = AwaitMe(interceptResult);
                awaited.Wait();

                Assert.False(interceptResult.IsCanceled);
                Assert.False(awaited.IsCanceled);
                Assert.False(interceptResult.IsFaulted);
                Assert.False(awaited.IsFaulted);

                Assert.Same(interceptReader, interceptResult.Result);
                Assert.Same(interceptReader, awaited.Result);
            }

            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_even_if_operation_throws()
            {
                var interceptionContext = new DbCommandInterceptionContext().WithCommandBehavior(CommandBehavior.SequentialAccess);

                var result = new Task<DbDataReader>(() => { throw new Exception("Bang!"); });

                var mockCommand = new Mock<DbCommand>();
                mockCommand.Protected()
                           .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", CommandBehavior.SequentialAccess, It.IsAny<CancellationToken>())
                           .Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(
                    m => m.ReaderExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<DbDataReader>>()))
                    .Callback<DbCommand, DbCommandInterceptionContext<DbDataReader>>((c, i) =>
                    {
                        Assert.Equal("Bang!", i.Exception.Message);
                        Assert.Equal("Bang!", i.OriginalException.Message);
                        Assert.False(i.IsExecutionSuppressed);
                        i.Exception = new Exception("Bing!");
                        Assert.Equal("Bing!", i.Exception.Message);
                        Assert.Equal("Bang!", i.OriginalException.Message);
                        Assert.False(i.IsExecutionSuppressed);
                    });

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult = dispatcher.ReaderAsync(mockCommand.Object, interceptionContext, CancellationToken.None);
                Assert.NotSame(result, interceptResult);

                mockCommand.Protected()
                           .Verify("ExecuteDbDataReaderAsync", Times.Once(), CommandBehavior.SequentialAccess, It.IsAny<CancellationToken>());

                mockInterceptor.Verify(
                    m => m.ReaderExecuting(
                        mockCommand.Object,
                        It.Is<DbCommandInterceptionContext<DbDataReader>>(c => c.IsAsync && c.CommandBehavior == CommandBehavior.SequentialAccess)));
                mockInterceptor.Verify(
                    m => m.ReaderExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<DbDataReader>>()),
                    Times.Never());

                result.Start();
                var awaited = AwaitMe(interceptResult);

                var exception = Assert.Throws<AggregateException>(() => awaited.Wait()).InnerException;

                Assert.Equal("Bing!", exception.Message);

                Assert.False(interceptResult.IsCanceled);
                Assert.False(awaited.IsCanceled);
                Assert.True(interceptResult.IsFaulted);
                Assert.True(awaited.IsFaulted);

                Assert.Throws<AggregateException>(() => interceptResult.Result);
                Assert.Throws<AggregateException>(() => awaited.Result);

                mockInterceptor.Verify(
                    m => m.ReaderExecuted(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext<DbDataReader>>(
                            c => c.IsAsync
                                 && c.TaskStatus.HasFlag(TaskStatus.Faulted)
                                 && c.Exception.Message == "Bing!"
                                 && c.CommandBehavior == CommandBehavior.SequentialAccess
                                 && c.Result == null)));
            }

            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_even_if_operation_is_canceled()
            {
                var interceptionContext = new DbCommandInterceptionContext().WithCommandBehavior(CommandBehavior.SequentialAccess);
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;

                var result = new Task<DbDataReader>(() => new Mock<DbDataReader>().Object, cancellationToken);

                var mockCommand = new Mock<DbCommand>();
                mockCommand.Protected()
                           .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", CommandBehavior.SequentialAccess, cancellationToken)
                           .Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(
                    m => m.ReaderExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<DbDataReader>>()));

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult = dispatcher.ReaderAsync(mockCommand.Object, interceptionContext, cancellationToken);
                Assert.NotSame(result, interceptResult);

                mockCommand.Protected()
                           .Verify("ExecuteDbDataReaderAsync", Times.Once(), CommandBehavior.SequentialAccess, cancellationToken);

                mockInterceptor.Verify(
                    m => m.ReaderExecuting(
                        mockCommand.Object,
                        It.Is<DbCommandInterceptionContext<DbDataReader>>(c => c.IsAsync && c.CommandBehavior == CommandBehavior.SequentialAccess)));
                mockInterceptor.Verify(
                    m => m.ReaderExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<DbDataReader>>()),
                    Times.Never());

                var awaited = AwaitMe(interceptResult);

                cancellationTokenSource.Cancel();

                Assert.Throws<AggregateException>(() => awaited.Wait());

                Assert.True(interceptResult.IsCanceled);
                Assert.True(awaited.IsCanceled);
                Assert.False(interceptResult.IsFaulted);
                Assert.False(awaited.IsFaulted);

                Assert.Throws<AggregateException>(() => interceptResult.Result);
                Assert.Throws<AggregateException>(() => awaited.Result);

                mockInterceptor.Verify(
                    m => m.ReaderExecuted(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext<DbDataReader>>(
                            c => c.IsAsync
                                 && c.TaskStatus.HasFlag(TaskStatus.Canceled)
                                 && c.Exception == null
                                 && c.CommandBehavior == CommandBehavior.SequentialAccess
                                 && c.Result == null)));
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
                    new DbCommandDispatcher().ReaderAsync(
                        mockCommand.Object, new DbCommandInterceptionContext(), CancellationToken.None));

                mockCommand.Protected()
                           .Verify(
                               "ExecuteDbDataReaderAsync", Times.Once(), ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>());
            }

            [Fact]
            public void Dispatch_method_checks_for_nulls()
            {
                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(
                        () =>
                        new DbCommandDispatcher().ReaderAsync(
                            null, new DbCommandInterceptionContext(), CancellationToken.None)).ParamName);

                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(
                        () =>
                        new DbCommandDispatcher().ReaderAsync(
                            new Mock<DbCommand>().Object, null, CancellationToken.None)).ParamName);
            }

            [Fact] // CodePlex 1140
            public void Dispatch_method_does_not_populate_given_interception_context_with_execution_results()
            {
                var interceptionContext = new DbCommandInterceptionContext<DbDataReader> { Exception = new Exception("Bang!") };

                var mockCommand = new Mock<DbCommand>();
                var result = new Task<DbDataReader>(() => new Mock<DbDataReader>().Object);

                mockCommand.Protected()
                    .Setup<Task<DbDataReader>>(
                        "ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                    .Returns(result);

                var dispatcher = new DbCommandDispatcher();
                dispatcher.InternalDispatcher.Add(new Mock<IDbCommandInterceptor>().Object);

                var interceptResult = dispatcher.ReaderAsync(mockCommand.Object, interceptionContext, new CancellationToken());

                mockCommand.Protected().Verify(
                    "ExecuteDbDataReaderAsync", Times.Once(), ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>());

                result.Start();
                interceptResult.Wait();

                Assert.NotNull(interceptResult.Result);

                Assert.Null(interceptionContext.Result);
                Assert.Equal("Bang!", interceptionContext.Exception.Message);
            }
        }

        private static async Task<T> AwaitMe<T>(Task<T> task)
        {
            return await task;
        }
#endif
    }
}
