// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Eventing.Reader;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class ExecutionStrategyBaseTests
    {
        private class TestExecutionStrategy : ExecutionStrategyBase
        {
            public TestExecutionStrategy(int maxRetryCount, TimeSpan maxDelay)
                : base(maxRetryCount, maxDelay)
            {
            }

            protected override bool ShouldRetryOn(Exception exception)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void Constructor_throws_on_null_parameters()
        {
            Assert.Equal(
                "maxRetryCount",
                Assert.Throws<ArgumentOutOfRangeException>(
                    () =>
                    new TestExecutionStrategy(
                        /*maxRetryCount:*/ -1, /*maxDelay:*/ TimeSpan.FromTicks(0))).ParamName);
            Assert.Equal(
                "maxDelay",
                Assert.Throws<ArgumentOutOfRangeException>(
                    () =>
                    new TestExecutionStrategy(
                        /*maxRetryCount:*/ 0, /*maxDelay:*/ TimeSpan.FromTicks(-1))).ParamName);
        }

        [Fact]
        public void GetNextDelay_returns_the_expected_default_sequence()
        {
            var strategy = new Mock<ExecutionStrategyBase>
                               {
                                   CallBase = true
                               }.Object;
            var delays = new List<TimeSpan>();
            TimeSpan? nextDelay;
            while ((nextDelay = strategy.GetNextDelay(null)) != null)
            {
                delays.Add(nextDelay.Value);
            }

            var expectedDelays = new List<TimeSpan>
                                     {
                                         TimeSpan.FromSeconds(0),
                                         TimeSpan.FromSeconds(1),
                                         TimeSpan.FromSeconds(3),
                                         TimeSpan.FromSeconds(7),
                                         TimeSpan.FromSeconds(15)
                                     };

            Assert.Equal(expectedDelays.Count, delays.Count);
            for (var i = 0; i < expectedDelays.Count; i++)
            {
                Assert.True(
                    (delays[i] - expectedDelays[i]).TotalMilliseconds <=
                    expectedDelays[i].TotalMilliseconds * 0.1 + 1,
                    string.Format("Expected: {0}; Actual: {1}", expectedDelays[i], delays[i]));
            }
        }

        [Fact]
        public void RetriesOnFailure_returns_true()
        {
            var mockExecutionStrategy = new Mock<ExecutionStrategyBase>
                                            {
                                                CallBase = true
                                            }.Object;

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

            private void Execute_throws_for_an_existing_transaction(Action<ExecutionStrategyBase> executeAsync)
            {
                var mockExecutionStrategy =
                    new Mock<ExecutionStrategyBase>
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

            private void Execute_throws_when_invoked_twice(Action<ExecutionStrategyBase> Execute)
            {
                var mockExecutionStrategy =
                    new Mock<ExecutionStrategyBase>
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
                    new Mock<ExecutionStrategyBase>
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
                    new Mock<ExecutionStrategyBase>
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

            private void Execute_throws_on_invalid_delay(Action<ExecutionStrategyBase, Func<int>> execute)
            {
                var executionStrategyMock =
                    new Mock<ExecutionStrategyBase>
                        {
                            CallBase = true
                        };

                executionStrategyMock.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                    e => TimeSpan.FromTicks(-1));
                executionStrategyMock.Protected().Setup<bool>("ShouldRetryOn", ItExpr.IsAny<Exception>()).Returns<Exception>(
                    e => e is ExternalException);

                var executionCount = 0;

                Assert.Equal(
                    Strings.ExecutionStrategy_NegativeDelay,
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        execute(
                            executionStrategyMock.Object, () =>
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

            private void Execute_doesnt_retry_if_succesful(Action<ExecutionStrategyBase, Func<int>> execute)
            {
                var executionStrategyMock =
                    new Mock<ExecutionStrategyBase>
                        {
                            CallBase = true
                        };

                executionStrategyMock.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                    e =>
                        {
                            Assert.True(false);
                            return null;
                        });
                executionStrategyMock.Protected().Setup<bool>("ShouldRetryOn", ItExpr.IsAny<Exception>()).Returns<Exception>(
                    e =>
                        {
                            Assert.True(false);
                            return false;
                        });

                var executionCount = 0;
                execute(executionStrategyMock.Object, () => executionCount++);

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

            private void Execute_retries_until_succesful(Action<ExecutionStrategyBase, Func<int>> execute)
            {
                var executionStrategyMock =
                    new Mock<ExecutionStrategyBase>
                        {
                            CallBase = true
                        };

                executionStrategyMock.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                    e => TimeSpan.FromTicks(0));
                executionStrategyMock.Protected().Setup<bool>("ShouldRetryOn", ItExpr.IsAny<Exception>()).Returns<Exception>(
                    e => e is ExternalException);

                var executionCount = 0;

                execute(
                    executionStrategyMock.Object, () =>
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

            private void Execute_retries_until_not_retrieable_exception_is_thrown(Action<ExecutionStrategyBase, Func<int>> execute)
            {
                var executionStrategyMock =
                    new Mock<ExecutionStrategyBase>
                        {
                            CallBase = true
                        };

                executionStrategyMock.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                    e => TimeSpan.FromTicks(0));
                executionStrategyMock.Protected().Setup<bool>("ShouldRetryOn", ItExpr.IsAny<Exception>()).Returns<Exception>(
                    e => e is ExternalException);

                var executionCount = 0;

                Assert.Throws<EventLogException>(
                    () =>
                    execute(
                        executionStrategyMock.Object, () =>
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

            private void Execute_retries_until_limit_is_reached(Action<ExecutionStrategyBase, Func<int>> execute)
            {
                var executionCount = 0;

                var executionStrategyMock =
                    new Mock<ExecutionStrategyBase>
                        {
                            CallBase = true
                        };

                executionStrategyMock.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                    e => executionCount < 3 ? (TimeSpan?)TimeSpan.FromTicks(0) : null);
                executionStrategyMock.Protected().Setup<bool>("ShouldRetryOn", ItExpr.IsAny<Exception>()).Returns<Exception>(
                    e => e is ExternalException);

                Assert.IsType<ExternalException>(
                    Assert.Throws<RetryLimitExceededException>(
                        () =>
                        execute(
                            executionStrategyMock.Object, () =>
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

            private void ExecuteAsync_throws_for_an_existing_transaction(Func<ExecutionStrategyBase, Task> executeAsync)
            {
                var mockExecutionStrategy =
                    new Mock<ExecutionStrategyBase>
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

            private void ExecuteAsync_throws_when_invoked_twice(Func<ExecutionStrategyBase, Task> executeAsync)
            {
                var mockExecutionStrategy =
                    new Mock<ExecutionStrategyBase>
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
                    new Mock<ExecutionStrategyBase>
                        {
                            CallBase = true
                        }.Object;

                Assert.Equal(
                    "func",
                    Assert.Throws<ArgumentNullException>(() => mockExecutionStrategy.ExecuteAsync(null, CancellationToken.None).Wait())
                          .ParamName);
            }

            [Fact]
            public void ExecuteAsync_Func_throws_on_null_parameters()
            {
                var mockExecutionStrategy =
                    new Mock<ExecutionStrategyBase>
                        {
                            CallBase = true
                        }.Object;

                Assert.Equal(
                    "func",
                    Assert.Throws<ArgumentNullException>(
                        () => mockExecutionStrategy.ExecuteAsync((Func<Task<int>>)null, CancellationToken.None).Wait()).ParamName);
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

            private void ExecuteAsync_throws_on_invalid_delay(Func<ExecutionStrategyBase, Func<Task<int>>, Task> executeAsync)
            {
                var executionStrategyMock =
                    new Mock<ExecutionStrategyBase>
                        {
                            CallBase = true
                        };

                executionStrategyMock.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                    e => TimeSpan.FromTicks(-1));
                executionStrategyMock.Protected().Setup<bool>("ShouldRetryOn", ItExpr.IsAny<Exception>()).Returns<Exception>(
                    e => e is ExternalException);

                var executionCount = 0;
                Assert.Equal(
                    Strings.ExecutionStrategy_NegativeDelay,
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        ExceptionHelpers.UnwrapAggregateExceptions(
                            () =>
                            executeAsync(
                                executionStrategyMock.Object, () =>
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

            private void ExecuteAsync_doesnt_retry_if_succesful(Func<ExecutionStrategyBase, Func<Task<int>>, Task> executeAsync)
            {
                var executionStrategyMock =
                    new Mock<ExecutionStrategyBase>
                        {
                            CallBase = true
                        };

                executionStrategyMock.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                    e =>
                        {
                            Assert.True(false);
                            return null;
                        });
                executionStrategyMock.Protected().Setup<bool>("ShouldRetryOn", ItExpr.IsAny<Exception>()).Returns<Exception>(
                    e =>
                        {
                            Assert.True(false);
                            return false;
                        });

                var executionCount = 0;
                executeAsync(executionStrategyMock.Object, () => Task.FromResult(executionCount++)).Wait();

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

            private void ExecuteAsync_retries_until_succesful(Func<ExecutionStrategyBase, Func<Task<int>>, Task> executeAsync)
            {
                var executionStrategyMock =
                    new Mock<ExecutionStrategyBase>
                        {
                            CallBase = true
                        };

                executionStrategyMock.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                    e => TimeSpan.FromTicks(0));
                executionStrategyMock.Protected().Setup<bool>("ShouldRetryOn", ItExpr.IsAny<Exception>()).Returns<Exception>(
                    e => e is ExternalException);

                var executionCount = 0;

                executeAsync(
                    executionStrategyMock.Object, () =>
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
                ExecuteAsync_retries_until_not_retrieable_exception_is_thrown(
                    (e, f) => e.ExecuteAsync(() => (Task)f(), CancellationToken.None));
            }

            [Fact]
            public void ExecuteAsync_Func_retries_until_not_retrieable_exception_is_thrown()
            {
                ExecuteAsync_retries_until_not_retrieable_exception_is_thrown((e, f) => e.ExecuteAsync(f, CancellationToken.None));
            }

            private void ExecuteAsync_retries_until_not_retrieable_exception_is_thrown(
                Func<ExecutionStrategyBase, Func<Task<int>>, Task> executeAsync)
            {
                var executionStrategyMock =
                    new Mock<ExecutionStrategyBase>
                        {
                            CallBase = true
                        };

                executionStrategyMock.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                    e => TimeSpan.FromTicks(0));
                executionStrategyMock.Protected().Setup<bool>("ShouldRetryOn", ItExpr.IsAny<Exception>()).Returns<Exception>(
                    e => e is ExternalException);

                var executionCount = 0;

                Assert.Throws<EventLogException>(
                    () =>
                    ExceptionHelpers.UnwrapAggregateExceptions(
                        () =>
                        executeAsync(
                            executionStrategyMock.Object, () =>
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

            private void ExecuteAsync_retries_until_limit_is_reached(Func<ExecutionStrategyBase, Func<Task<int>>, Task> executeAsync)
            {
                var executionCount = 0;

                var executionStrategyMock =
                    new Mock<ExecutionStrategyBase>
                        {
                            CallBase = true
                        };

                executionStrategyMock.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                    e => executionCount < 3 ? (TimeSpan?)TimeSpan.FromTicks(0) : null);
                executionStrategyMock.Protected().Setup<bool>("ShouldRetryOn", ItExpr.IsAny<Exception>()).Returns<Exception>(
                    e => e is ExternalException);

                Assert.IsType<ExternalException>(
                    Assert.Throws<RetryLimitExceededException>(
                        () =>
                        ExceptionHelpers.UnwrapAggregateExceptions(
                            () =>
                            executeAsync(
                                executionStrategyMock.Object, () =>
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

        public class UnwrapAndHandleException
        {
            [Fact]
            public void Unwraps_EntityException()
            {
                var innerException = new TimeoutException();
                Assert.True(
                    ExecutionStrategyBase.UnwrapAndHandleException(
                        new EntityException("", innerException),
                        ex =>
                            {
                                Assert.Same(innerException, ex);
                                return true;
                            }));
            }

            [Fact]
            public void Unwraps_DbUpdateException()
            {
                var innerException = new TimeoutException();
                Assert.True(
                    ExecutionStrategyBase.UnwrapAndHandleException(
                        new DbUpdateException("", innerException),
                        ex =>
                            {
                                Assert.Same(innerException, ex);
                                return true;
                            }));
            }

            [Fact]
            public void Unwraps_UpdateException()
            {
                var innerException = new TimeoutException();
                Assert.True(
                    ExecutionStrategyBase.UnwrapAndHandleException(
                        new UpdateException("", innerException),
                        ex =>
                            {
                                Assert.Same(innerException, ex);
                                return true;
                            }));
            }

            [Fact]
            public void Unwraps_wrapped_null_exception()
            {
                Exception innerException = null;
                Assert.True(
                    ExecutionStrategyBase.UnwrapAndHandleException(
                        new UpdateException("", innerException),
                        ex =>
                            {
                                Assert.Same(innerException, ex);
                                return true;
                            }));
            }

            [Fact]
            public void Unwraps_Nested_exceptions()
            {
                var innerException = new TimeoutException("", new EntityException("", new DbUpdateException("", new UpdateException(""))));
                Assert.True(
                    ExecutionStrategyBase.UnwrapAndHandleException(
                        new EntityException("", new DbUpdateException("", new UpdateException("", innerException))),
                        ex =>
                            {
                                Assert.Same(innerException, ex);
                                return true;
                            }));
            }
        }
    }
}
