// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
#if !NET40
    using System.Threading;
    using System.Threading.Tasks;
#endif
    using Xunit;

    public class DefaultExecutionStrategyTests
    {
        [Fact]
        public void RetriesOnFailure_returns_false()
        {
            Assert.False(new DefaultExecutionStrategy().RetriesOnFailure);
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

        private void Execute_doesnt_retry(Action<IDbExecutionStrategy, Func<int>> execute)
        {
            var executionStrategy = new DefaultExecutionStrategy();
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
            ExecuteAsync_doesnt_retry((e, f) => e.ExecuteAsync(() => (Task)f(), CancellationToken.None));
        }

        [Fact]
        public void ExecuteAsync_Func_doesnt_retry()
        {
            ExecuteAsync_doesnt_retry((e, f) => e.ExecuteAsync(f, CancellationToken.None));
        }

        private void ExecuteAsync_doesnt_retry(Func<IDbExecutionStrategy, Func<Task<int>>, Task> executeAsync)
        {
            var executionStrategy = new DefaultExecutionStrategy();
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

        [Fact]
        public void Non_generic_ExecuteAsync_throws_OperationCanceledException_if_task_is_cancelled()
        {
            Assert.Throws<OperationCanceledException>(
                () => new DefaultExecutionStrategy().ExecuteAsync(() => null, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void Generic_ExecuteAsync_throws_OperationCanceledException_if_task_is_cancelled()
        {
            Assert.Throws<OperationCanceledException>(
                () => new DefaultExecutionStrategy().ExecuteAsync<object>(() => null, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

#endif
    }
}
