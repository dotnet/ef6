// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
#if !NET40
    using System.Threading.Tasks;
#endif
    using Xunit;

    public class NonRetryingExecutionStrategyTests
    {
        [Fact]
        public void SupportsExistingTransactions_returns_true()
        {
            Assert.True(new NonRetryingExecutionStrategy().SupportsExistingTransactions);
        }

        [Fact]
        public void Execute_Action_doesnt_retry()
        {
            Execute_doesnt_retry((e, a) => e.Execute(() => { a(); }));
        }
        [Fact]
        public void Execute_Func_doesnt_retry()
        {
            Execute_doesnt_retry((e, a) => e.Execute(a));
        }

        private void Execute_doesnt_retry(Action<IExecutionStrategy, Func<int>> execute)
        {
            var executionStrategy = new NonRetryingExecutionStrategy();
            var executionCount = 0;
            Assert.Throws<TimeoutException>(
                () =>
                execute(executionStrategy,
                    () =>
                    {
                        executionCount++;
                        throw new TimeoutException();
                    }));

            Assert.Equal(1, executionCount);
        }

#if !NET40

        [Fact]
        public void ExecuteAsync_Action_doesnt_retry()
        {
            ExecuteAsync_doesnt_retry((e, f) => e.ExecuteAsync(() => (Task)f()));
        }

        [Fact]
        public void ExecuteAsync_Func_doesnt_retry()
        {
            ExecuteAsync_doesnt_retry((e, f) => e.ExecuteAsync(f));
        }

        private void ExecuteAsync_doesnt_retry(Func<IExecutionStrategy, Func<Task<int>>, Task> executeAsync)
        {
            var executionStrategy = new NonRetryingExecutionStrategy();
            var executionCount = 0;
            Assert.Throws<TimeoutException>(
                () =>
                ExceptionHelpers.UnwrapAggregateExceptions(
                    () =>
                    executeAsync(executionStrategy,
                        () =>
                        {
                            executionCount++;
                            throw new TimeoutException();
                        }).Wait()));

            Assert.Equal(1, executionCount);
        }

#endif

    }
}
