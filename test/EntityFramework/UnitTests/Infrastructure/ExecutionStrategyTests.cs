// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Resources;
    using System.Diagnostics.Eventing.Reader;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using Moq;
    using Xunit;

    public class ExecutionStrategyTests
    {
        [Fact]
        public void Constructor_throws_on_null_parameters()
        {
            Assert.Equal(
                "retryDelayStrategy",
                Assert.Throws<ArgumentNullException>(() => new ExecutionStrategy(null, new Mock<IRetriableExceptionDetector>().Object)).ParamName);
            Assert.Equal(
                "retriableExceptionDetector",
                Assert.Throws<ArgumentNullException>(() => new ExecutionStrategy(new Mock<IRetryDelayStrategy>().Object, null)).ParamName);
        }

        [Fact]
        public void RetriesOnFailure_returns_true()
        {
            var mockExecutionStrategy = new ExecutionStrategy(
                new Mock<IRetryDelayStrategy>().Object, new Mock<IRetriableExceptionDetector>().Object);

            Assert.True(mockExecutionStrategy.RetriesOnFailure);
        }

        public class Execute
        {
            [Fact]
            public void Execute_Action_throws_for_an_existing_transaction()
            {
                Execute_throws_for_an_existing_transaction(e => e.Execute(() => { }));
            }

            [Fact]
            public void Execute_Func_throws_for_an_existing_transaction()
            {
                Execute_throws_for_an_existing_transaction(e => e.Execute(() => 1));
            }

            private void Execute_throws_for_an_existing_transaction(Action<ExecutionStrategy> executeAsync)
            {
                var mockExecutionStrategy =
                    new Mock<ExecutionStrategy>(new Mock<IRetryDelayStrategy>().Object, new Mock<IRetriableExceptionDetector>().Object)
                        {
                            CallBase = true
                        }.Object;
                using (new TransactionScope())
                {
                    Assert.Equal(
                        Strings.ExecutionStrategy_ExistingTransaction,
                        Assert.Throws<InvalidOperationException>(
                            () =>
                            executeAsync(mockExecutionStrategy)).Message);
                }
            }

            [Fact]
            public void Execute_Action_throws_when_invoked_twice()
            {
                Execute_throws_when_invoked_twice(e => e.Execute(() => { }));
            }

            [Fact]
            public void Execute_Func_throws_when_invoked_twice()
            {
                Execute_throws_when_invoked_twice(e => e.Execute(() => 1));
            }

            private void Execute_throws_when_invoked_twice(Action<ExecutionStrategy> Execute)
            {
                var mockExecutionStrategy =
                    new Mock<ExecutionStrategy>(new Mock<IRetryDelayStrategy>().Object, new Mock<IRetriableExceptionDetector>().Object)
                        {
                            CallBase = true
                        }.Object;
                Execute(mockExecutionStrategy);

                Assert.Equal(
                    Strings.ExecutionStrategy_AlreadyExecuted,
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        Execute(mockExecutionStrategy)).Message);
            }

            [Fact]
            public void Execute_Action_throws_on_null_parameters()
            {
                var mockExecutionStrategy =
                    new Mock<ExecutionStrategy>(new Mock<IRetryDelayStrategy>().Object, new Mock<IRetriableExceptionDetector>().Object)
                        {
                            CallBase = true
                        }.Object;

                Assert.Equal(
                    "action",
                    Assert.Throws<ArgumentNullException>(() => mockExecutionStrategy.Execute(null)).ParamName);
            }

            [Fact]
            public void Execute_Func_throws_on_null_parameters()
            {
                var mockExecutionStrategy =
                    new Mock<ExecutionStrategy>(new Mock<IRetryDelayStrategy>().Object, new Mock<IRetriableExceptionDetector>().Object)
                        {
                            CallBase = true
                        }.Object;

                Assert.Equal(
                    "func",
                    Assert.Throws<ArgumentNullException>(() => mockExecutionStrategy.Execute((Func<int>)null)).ParamName);
            }

            [Fact]
            public void Execute_Action_throws_on_invalid_delay()
            {
                Execute_throws_on_invalid_delay((e, f) => e.Execute(() => { f(); }));
            }

            [Fact]
            public void Execute_Func_throws_on_invalid_delay()
            {
                Execute_throws_on_invalid_delay((e, f) => e.Execute(f));
            }

            private void Execute_throws_on_invalid_delay(Action<ExecutionStrategy, Func<int>> execute)
            {
                var mockRetryDelayStrategy = new Mock<IRetryDelayStrategy>();
                mockRetryDelayStrategy.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                    e => TimeSpan.FromTicks(-1));
                var mockRetriableExceptionDetector = new Mock<IRetriableExceptionDetector>();
                mockRetriableExceptionDetector.Setup(m => m.ShouldRetryOn(It.IsAny<Exception>())).Returns<Exception>(
                    e => e is ExternalException);

                var mockExecutionStrategy =
                    new Mock<ExecutionStrategy>(mockRetryDelayStrategy.Object, mockRetriableExceptionDetector.Object)
                        {
                            CallBase = true
                        }.Object;

                var executionCount = 0;

                Assert.Equal(
                    Strings.ExecutionStrategy_NegativeDelay,
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        execute(
                            mockExecutionStrategy, () =>
                                                       {
                                                           if (executionCount++ < 3)
                                                           {
                                                               throw new ExternalException();
                                                           }
                                                           else
                                                           {
                                                               Assert.True(false);
                                                               return 0;
                                                           }
                                                       })).Message);

                Assert.Equal(1, executionCount);
            }

            [Fact]
            public void Execute_Action_doesnt_retry_if_succesful()
            {
                Execute_doesnt_retry_if_succesful((e, f) => e.Execute(() => { f(); }));
            }

            [Fact]
            public void Execute_Func_doesnt_retry_if_succesful()
            {
                Execute_doesnt_retry_if_succesful((e, f) => e.Execute(f));
            }

            private void Execute_doesnt_retry_if_succesful(Action<ExecutionStrategy, Func<int>> execute)
            {
                var mockRetryDelayStrategy = new Mock<IRetryDelayStrategy>();
                mockRetryDelayStrategy.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                    e =>
                    {
                        Assert.True(false);
                        return null;
                    });
                var mockRetriableExceptionDetector = new Mock<IRetriableExceptionDetector>();
                mockRetriableExceptionDetector.Setup(m => m.ShouldRetryOn(It.IsAny<Exception>())).Returns<Exception>(
                    e =>
                    {
                        Assert.True(false);
                        return false;
                    });

                var mockExecutionStrategy =
                    new Mock<ExecutionStrategy>(mockRetryDelayStrategy.Object, mockRetriableExceptionDetector.Object)
                        {
                            CallBase = true
                        }.Object;

                var executionCount = 0;
                execute(mockExecutionStrategy, () => executionCount++);

                Assert.Equal(1, executionCount);
            }

            [Fact]
            public void Execute_Action_retries_until_succesful()
            {
                Execute_retries_until_succesful((e, f) => e.Execute(() => { f(); }));
            }

            [Fact]
            public void Execute_Func_retries_until_succesful()
            {
                Execute_retries_until_succesful((e, f) => e.Execute(f));
            }

            private void Execute_retries_until_succesful(Action<ExecutionStrategy, Func<int>> execute)
            {
                var mockRetryDelayStrategy = new Mock<IRetryDelayStrategy>();
                mockRetryDelayStrategy.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                    e => TimeSpan.FromTicks(0));
                var mockRetriableExceptionDetector = new Mock<IRetriableExceptionDetector>();
                mockRetriableExceptionDetector.Setup(m => m.ShouldRetryOn(It.IsAny<Exception>())).Returns<Exception>(
                    e => e is ExternalException);

                var mockExecutionStrategy =
                    new Mock<ExecutionStrategy>(mockRetryDelayStrategy.Object, mockRetriableExceptionDetector.Object)
                        {
                            CallBase = true
                        }.Object;

                var executionCount = 0;

                execute(
                    mockExecutionStrategy, () =>
                                               {
                                                   if (executionCount++ < 3)
                                                   {
                                                       throw new ExternalException();
                                                   }

                                                   return executionCount;
                                               });

                Assert.Equal(4, executionCount);
            }

            [Fact]
            public void Execute_Action_retries_until_not_retrieable_exception_is_thrown()
            {
                Execute_retries_until_not_retrieable_exception_is_thrown((e, f) => e.Execute(() => { f(); }));
            }

            [Fact]
            public void Execute_Func_retries_until_not_retrieable_exception_is_thrown()
            {
                Execute_retries_until_not_retrieable_exception_is_thrown((e, f) => e.Execute(f));
            }

            private void Execute_retries_until_not_retrieable_exception_is_thrown(Action<ExecutionStrategy, Func<int>> execute)
            {
                var mockRetryDelayStrategy = new Mock<IRetryDelayStrategy>();
                mockRetryDelayStrategy.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                    e => TimeSpan.FromTicks(0));
                var mockRetriableExceptionDetector = new Mock<IRetriableExceptionDetector>();
                mockRetriableExceptionDetector.Setup(m => m.ShouldRetryOn(It.IsAny<Exception>())).Returns<Exception>(
                    e => e is ExternalException);

                var mockExecutionStrategy =
                    new Mock<ExecutionStrategy>(mockRetryDelayStrategy.Object, mockRetriableExceptionDetector.Object)
                        {
                            CallBase = true
                        }.Object;

                var executionCount = 0;

                Assert.Throws<EventLogException>(
                    () =>
                    execute(
                        mockExecutionStrategy, () =>
                                                   {
                                                       if (executionCount++ < 3)
                                                       {
                                                           throw new ExternalException();
                                                       }
                                                       else
                                                       {
                                                           throw new EventLogException();
                                                       }
                                                   }));

                Assert.Equal(4, executionCount);
            }

            [Fact]
            public void Execute_Action_retries_until_limit_is_reached()
            {
                Execute_retries_until_limit_is_reached((e, f) => e.Execute(() => { f(); }));
            }

            [Fact]
            public void Execute_Func_retries_until_limit_is_reached()
            {
                Execute_retries_until_limit_is_reached((e, f) => e.Execute(f));
            }

            private void Execute_retries_until_limit_is_reached(Action<ExecutionStrategy, Func<int>> execute)
            {
                var executionCount = 0;
                var mockRetryDelayStrategy = new Mock<IRetryDelayStrategy>();
                mockRetryDelayStrategy.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                    e => executionCount < 3 ? (TimeSpan?)TimeSpan.FromTicks(0) : null);
                var mockRetriableExceptionDetector = new Mock<IRetriableExceptionDetector>();
                mockRetriableExceptionDetector.Setup(m => m.ShouldRetryOn(It.IsAny<Exception>())).Returns<Exception>(
                    e => e is ExternalException);

                var mockExecutionStrategy =
                    new Mock<ExecutionStrategy>(mockRetryDelayStrategy.Object, mockRetriableExceptionDetector.Object)
                        {
                            CallBase = true
                        }.Object;

                Assert.IsType<ExternalException>(
                    Assert.Throws<RetryLimitExceededException>(
                        () =>
                        execute(
                            mockExecutionStrategy, () =>
                                                       {
                                                           if (executionCount++ < 3)
                                                           {
                                                               throw new ExternalException();
                                                           }
                                                           else
                                                           {
                                                               Assert.True(false);
                                                               return 0;
                                                           }
                                                       })).InnerException);

                Assert.Equal(3, executionCount);
            }
        }

#if !NET40

        public class ExecuteAsync
        {
            [Fact]
            public void ExecuteAsync_Action_throws_for_an_existing_transaction()
            {
                ExecuteAsync_throws_for_an_existing_transaction(e => e.ExecuteAsync(() => (Task)Task.FromResult(1), CancellationToken.None));
            }

            [Fact]
            public void ExecuteAsync_Func_throws_for_an_existing_transaction()
            {
                ExecuteAsync_throws_for_an_existing_transaction(e => e.ExecuteAsync(() => Task.FromResult(1), CancellationToken.None));
            }

            private void ExecuteAsync_throws_for_an_existing_transaction(Func<ExecutionStrategy, Task> executeAsync)
            {
                var mockExecutionStrategy =
                    new Mock<ExecutionStrategy>(new Mock<IRetryDelayStrategy>().Object, new Mock<IRetriableExceptionDetector>().Object)
                        {
                            CallBase = true
                        }.Object;

                using (new TransactionScope())
                {
                    Assert.Equal(
                        Strings.ExecutionStrategy_ExistingTransaction,
                        Assert.Throws<InvalidOperationException>(
                            () =>
                            executeAsync(mockExecutionStrategy)).Message);
                }
            }

            [Fact]
            public void ExecuteAsync_Action_throws_when_invoked_twice()
            {
                ExecuteAsync_throws_when_invoked_twice(e => e.ExecuteAsync(() => (Task)Task.FromResult(1), CancellationToken.None));
            }

            [Fact]
            public void ExecuteAsync_Func_throws_when_invoked_twice()
            {
                ExecuteAsync_throws_when_invoked_twice(e => e.ExecuteAsync(() => Task.FromResult(1), CancellationToken.None));
            }

            private void ExecuteAsync_throws_when_invoked_twice(Func<ExecutionStrategy, Task> executeAsync)
            {
                var mockExecutionStrategy =
                    new Mock<ExecutionStrategy>(new Mock<IRetryDelayStrategy>().Object, new Mock<IRetriableExceptionDetector>().Object)
                        {
                            CallBase = true
                        }.Object;
                executeAsync(mockExecutionStrategy).Wait();

                Assert.Equal(
                    Strings.ExecutionStrategy_AlreadyExecuted,
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        ExceptionHelpers.UnwrapAggregateExceptions(
                            () =>
                            executeAsync(mockExecutionStrategy).Wait())).Message);
            }

            [Fact]
            public void ExecuteAsync_Action_throws_on_null_parameters()
            {
                var mockExecutionStrategy =
                    new Mock<ExecutionStrategy>(new Mock<IRetryDelayStrategy>().Object, new Mock<IRetriableExceptionDetector>().Object)
                        {
                            CallBase = true
                        }.Object;

                Assert.Equal(
                    "taskFunc",
                    Assert.Throws<ArgumentNullException>(() => mockExecutionStrategy.ExecuteAsync(null, CancellationToken.None).Wait()).ParamName);
            }

            [Fact]
            public void ExecuteAsync_Func_throws_on_null_parameters()
            {
                var mockExecutionStrategy =
                    new Mock<ExecutionStrategy>(new Mock<IRetryDelayStrategy>().Object, new Mock<IRetriableExceptionDetector>().Object)
                        {
                            CallBase = true
                        }.Object;

                Assert.Equal(
                    "taskFunc",
                    Assert.Throws<ArgumentNullException>(() => mockExecutionStrategy.ExecuteAsync((Func<Task<int>>)null, CancellationToken.None).Wait()).ParamName);
            }

            [Fact]
            public void ExecuteAsync_Action_throws_on_invalid_delay()
            {
                ExecuteAsync_throws_on_invalid_delay((e, f) => e.ExecuteAsync(() => (Task)f(), CancellationToken.None));
            }

            [Fact]
            public void ExecuteAsync_Func_throws_on_invalid_delay()
            {
                ExecuteAsync_throws_on_invalid_delay((e, f) => e.ExecuteAsync(f, CancellationToken.None));
            }

            private void ExecuteAsync_throws_on_invalid_delay(Func<ExecutionStrategy, Func<Task<int>>, Task> executeAsync)
            {
                var mockRetryDelayStrategy = new Mock<IRetryDelayStrategy>();
                mockRetryDelayStrategy.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                    e => TimeSpan.FromTicks(-1));
                var mockRetriableExceptionDetector = new Mock<IRetriableExceptionDetector>();
                mockRetriableExceptionDetector.Setup(m => m.ShouldRetryOn(It.IsAny<Exception>())).Returns<Exception>(
                    e => e is ExternalException);

                var mockExecutionStrategy =
                    new Mock<ExecutionStrategy>(mockRetryDelayStrategy.Object, mockRetriableExceptionDetector.Object)
                        {
                            CallBase = true
                        }.Object;

                var executionCount = 0;
                Assert.Equal(
                    Strings.ExecutionStrategy_NegativeDelay,
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        ExceptionHelpers.UnwrapAggregateExceptions(
                            () =>
                            executeAsync(
                                mockExecutionStrategy, () =>
                                                           {
                                                               if (executionCount++ < 3)
                                                               {
                                                                   throw new ExternalException();
                                                               }
                                                               else
                                                               {
                                                                   Assert.True(false);
                                                                   return Task.FromResult(0);
                                                               }
                                                           }).Wait())).Message);

                Assert.Equal(1, executionCount);
            }

            [Fact]
            public void ExecuteAsync_Action_doesnt_retry_if_succesful()
            {
                ExecuteAsync_doesnt_retry_if_succesful((e, f) => e.ExecuteAsync(() => (Task)f(), CancellationToken.None));
            }

            [Fact]
            public void ExecuteAsync_Func_doesnt_retry_if_succesful()
            {
                ExecuteAsync_doesnt_retry_if_succesful((e, f) => e.ExecuteAsync(f, CancellationToken.None));
            }

            private void ExecuteAsync_doesnt_retry_if_succesful(Func<ExecutionStrategy, Func<Task<int>>, Task> executeAsync)
            {
                var mockRetryDelayStrategy = new Mock<IRetryDelayStrategy>();
                mockRetryDelayStrategy.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                    e =>
                        {
                            Assert.True(false);
                            return null;
                        });
                var mockRetriableExceptionDetector = new Mock<IRetriableExceptionDetector>();
                mockRetriableExceptionDetector.Setup(m => m.ShouldRetryOn(It.IsAny<Exception>())).Returns<Exception>(
                    e =>
                        {
                            Assert.True(false);
                            return false;
                        });

                var mockExecutionStrategy =
                    new Mock<ExecutionStrategy>(mockRetryDelayStrategy.Object, mockRetriableExceptionDetector.Object)
                        {
                            CallBase = true
                        }.Object;

                var executionCount = 0;
                executeAsync(mockExecutionStrategy, () => Task.FromResult(executionCount++)).Wait();

                Assert.Equal(1, executionCount);
            }

            [Fact]
            public void ExecuteAsync_Action_retries_until_succesful()
            {
                ExecuteAsync_retries_until_succesful((e, f) => e.ExecuteAsync(() => (Task)f(), CancellationToken.None));
            }

            [Fact]
            public void ExecuteAsync_Func_retries_until_succesful()
            {
                ExecuteAsync_retries_until_succesful((e, f) => e.ExecuteAsync(f, CancellationToken.None));
            }

            private void ExecuteAsync_retries_until_succesful(Func<ExecutionStrategy, Func<Task<int>>, Task> executeAsync)
            {
                var mockRetryDelayStrategy = new Mock<IRetryDelayStrategy>();
                mockRetryDelayStrategy.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                    e => TimeSpan.FromTicks(0));
                var mockRetriableExceptionDetector = new Mock<IRetriableExceptionDetector>();
                mockRetriableExceptionDetector.Setup(m => m.ShouldRetryOn(It.IsAny<Exception>())).Returns<Exception>(
                    e => e is ExternalException);

                var mockExecutionStrategy =
                    new Mock<ExecutionStrategy>(mockRetryDelayStrategy.Object, mockRetriableExceptionDetector.Object)
                        {
                            CallBase = true
                        }.Object;

                var executionCount = 0;

                executeAsync(
                    mockExecutionStrategy, () =>
                                               {
                                                   if (executionCount++ < 3)
                                                   {
                                                       throw new ExternalException();
                                                   }

                                                   return Task.FromResult(executionCount);
                                               }).Wait();

                Assert.Equal(4, executionCount);
            }

            [Fact]
            public void ExecuteAsync_Action_retries_until_not_retrieable_exception_is_thrown()
            {
                ExecuteAsync_retries_until_not_retrieable_exception_is_thrown((e, f) => e.ExecuteAsync(() => (Task)f(), CancellationToken.None));
            }

            [Fact]
            public void ExecuteAsync_Func_retries_until_not_retrieable_exception_is_thrown()
            {
                ExecuteAsync_retries_until_not_retrieable_exception_is_thrown((e, f) => e.ExecuteAsync(f, CancellationToken.None));
            }

            private void ExecuteAsync_retries_until_not_retrieable_exception_is_thrown(
                Func<ExecutionStrategy, Func<Task<int>>, Task> executeAsync)
            {
                var mockRetryDelayStrategy = new Mock<IRetryDelayStrategy>();
                mockRetryDelayStrategy.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                    e => TimeSpan.FromTicks(0));
                var mockRetriableExceptionDetector = new Mock<IRetriableExceptionDetector>();
                mockRetriableExceptionDetector.Setup(m => m.ShouldRetryOn(It.IsAny<Exception>())).Returns<Exception>(
                    e => e is ExternalException);

                var mockExecutionStrategy =
                    new Mock<ExecutionStrategy>(mockRetryDelayStrategy.Object, mockRetriableExceptionDetector.Object)
                        {
                            CallBase = true
                        }.Object;

                var executionCount = 0;

                Assert.Throws<EventLogException>(
                    () =>
                    ExceptionHelpers.UnwrapAggregateExceptions(
                        () =>
                        executeAsync(
                            mockExecutionStrategy, () =>
                                                       {
                                                           if (executionCount++ < 3)
                                                           {
                                                               throw new ExternalException();
                                                           }
                                                           else
                                                           {
                                                               throw new EventLogException();
                                                           }
                                                       }).Wait()));

                Assert.Equal(4, executionCount);
            }

            [Fact]
            public void ExecuteAsync_Action_retries_until_limit_is_reached()
            {
                ExecuteAsync_retries_until_limit_is_reached((e, f) => e.ExecuteAsync(() => (Task)f(), CancellationToken.None));
            }

            [Fact]
            public void ExecuteAsync_Func_retries_until_limit_is_reached()
            {
                ExecuteAsync_retries_until_limit_is_reached((e, f) => e.ExecuteAsync(f, CancellationToken.None));
            }

            private void ExecuteAsync_retries_until_limit_is_reached(Func<ExecutionStrategy, Func<Task<int>>, Task> executeAsync)
            {
                var executionCount = 0;
                var mockRetryDelayStrategy = new Mock<IRetryDelayStrategy>();
                mockRetryDelayStrategy.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                    e => executionCount < 3 ? (TimeSpan?)TimeSpan.FromTicks(0) : null);
                var mockRetriableExceptionDetector = new Mock<IRetriableExceptionDetector>();
                mockRetriableExceptionDetector.Setup(m => m.ShouldRetryOn(It.IsAny<Exception>())).Returns<Exception>(
                    e => e is ExternalException);

                var mockExecutionStrategy =
                    new Mock<ExecutionStrategy>(mockRetryDelayStrategy.Object, mockRetriableExceptionDetector.Object)
                        {
                            CallBase = true
                        }.Object;

                Assert.IsType<ExternalException>(
                    Assert.Throws<RetryLimitExceededException>(
                        () =>
                        ExceptionHelpers.UnwrapAggregateExceptions(
                            () =>
                            executeAsync(
                                mockExecutionStrategy, () =>
                                                           {
                                                               if (executionCount++ < 3)
                                                               {
                                                                   throw new ExternalException();
                                                               }
                                                               else
                                                               {
                                                                   Assert.True(false);
                                                                   return Task.FromResult(0);
                                                               }
                                                           }).Wait())).InnerException);

                Assert.Equal(3, executionCount);
            }
        }

#endif

    }
}
