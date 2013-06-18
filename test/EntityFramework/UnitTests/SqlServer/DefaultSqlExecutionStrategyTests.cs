// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Core;
    using System.Data.Entity.Infrastructure;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class DefaultSqlExecutionStrategyTests
    {
        [Fact]
        public void RetriesOnFailure_returns_false()
        {
            Assert.False(new DefaultSqlExecutionStrategy().RetriesOnFailure);
        }

        [Fact]
        public void Execute_throws_on_null_parameters()
        {
            var executionStrategy = new DefaultSqlExecutionStrategy();

            Assert.Equal(
                "operation",
                Assert.Throws<ArgumentNullException>(() => executionStrategy.Execute(null)).ParamName);

            Assert.Equal(
                "operation",
                Assert.Throws<ArgumentNullException>(() => executionStrategy.Execute((Func<object>)null)).ParamName);
        }

        [Fact]
        public void Execute_Action_doesnt_retry_on_transient_exceptions()
        {
            Execute_doesnt_retry_on_transient_exceptions((e, a) => e.Execute(() => { a(); }));
        }

        [Fact]
        public void Execute_Func_doesnt_retry_on_transient_exceptions()
        {
            Execute_doesnt_retry_on_transient_exceptions((e, a) => e.Execute(a));
        }

        private void Execute_doesnt_retry_on_transient_exceptions(Action<IDbExecutionStrategy, Func<int>> execute)
        {
            var executionStrategy = new DefaultSqlExecutionStrategy();
            var executionCount = 0;
            var exception = Assert.Throws<EntityException>(
                () =>
                execute(
                    executionStrategy,
                    () =>
                        {
                            executionCount++;
                            throw new TimeoutException();
                        }));
            Assert.IsType<TimeoutException>(exception.InnerException);
            exception.ValidateMessage(
                TestBase.EntityFrameworkSqlServerAssembly,
                "TransientExceptionDetected", "System.Data.Entity.SqlServer.Properties.Resources.SqlServer");

            Assert.Equal(1, executionCount);
        }

        [Fact]
        public void Execute_Action_doesnt_retry_on_nontransient_exceptions()
        {
            Execute_doesnt_retry_on_nontransient_exceptions((e, a) => e.Execute(() => { a(); }));
        }

        [Fact]
        public void Execute_Func_doesnt_retry_on_nontransient_exceptions()
        {
            Execute_doesnt_retry_on_nontransient_exceptions((e, a) => e.Execute(a));
        }

        private void Execute_doesnt_retry_on_nontransient_exceptions(Action<IDbExecutionStrategy, Func<int>> execute)
        {
            var executionStrategy = new DefaultSqlExecutionStrategy();
            var executionCount = 0;
            Assert.Throws<ArgumentException>(
                () =>
                execute(
                    executionStrategy,
                    () =>
                        {
                            executionCount++;
                            throw new ArgumentException();
                        }));

            Assert.Equal(1, executionCount);
        }

#if !NET40

        [Fact]
        public void ExecuteAsync_throws_on_null_parameter()
        {
            var executionStrategy = new DefaultSqlExecutionStrategy();

            Assert.Equal(
                "operation",
                Assert.Throws<ArgumentNullException>(() => executionStrategy.ExecuteAsync((Func<Task<object>>)null, CancellationToken.None)).ParamName);

            Assert.Equal(
                "operation",
                Assert.Throws<ArgumentNullException>(() => executionStrategy.ExecuteAsync((Func<Task<object>>)null, CancellationToken.None)).ParamName);
        }
        [Fact]
        public void ExecuteAsync_Action_doesnt_retry_on_transient_exceptions()
        {
            ExecuteAsync_doesnt_retry_on_transient_exceptions((e, f) => e.ExecuteAsync(() => (Task)f(), CancellationToken.None));
        }

        [Fact]
        public void ExecuteAsync_Func_doesnt_retry_on_transient_exceptions()
        {
            ExecuteAsync_doesnt_retry_on_transient_exceptions((e, f) => e.ExecuteAsync(f, CancellationToken.None));
        }

        private void ExecuteAsync_doesnt_retry_on_transient_exceptions(Func<IDbExecutionStrategy, Func<Task<int>>, Task> executeAsync)
        {
            var executionStrategy = new DefaultSqlExecutionStrategy();
            var executionCount = 0;
            var exception = Assert.Throws<EntityException>(
                () =>
                ExceptionHelpers.UnwrapAggregateExceptions(
                    () =>
                    executeAsync(
                        executionStrategy,
                        () =>
                            {
                                executionCount++;
                                throw new TimeoutException();
                            }).Wait()));

            Assert.IsType<TimeoutException>(exception.InnerException);
            exception.ValidateMessage(
                TestBase.EntityFrameworkSqlServerAssembly,
                "TransientExceptionDetected", "System.Data.Entity.SqlServer.Properties.Resources.SqlServer");

            Assert.Equal(1, executionCount);
        }

        [Fact]
        public void ExecuteAsync_Action_doesnt_retry_on_nontransient_exceptions()
        {
            ExecuteAsync_doesnt_retry_on_nontransient_exceptions((e, f) => e.ExecuteAsync(() => (Task)f(), CancellationToken.None));
        }

        [Fact]
        public void ExecuteAsync_Func_doesnt_retry_on_nontransient_exceptions()
        {
            ExecuteAsync_doesnt_retry_on_nontransient_exceptions((e, f) => e.ExecuteAsync(f, CancellationToken.None));
        }

        private void ExecuteAsync_doesnt_retry_on_nontransient_exceptions(Func<IDbExecutionStrategy, Func<Task<int>>, Task> executeAsync)
        {
            var executionStrategy = new DefaultSqlExecutionStrategy();
            var executionCount = 0;
            Assert.Throws<ArgumentException>(
                () =>
                ExceptionHelpers.UnwrapAggregateExceptions(
                    () =>
                    executeAsync(
                        executionStrategy,
                        () =>
                            {
                                executionCount++;
                                throw new ArgumentException();
                            }).Wait()));

            Assert.Equal(1, executionCount);
        }

#endif
    }
}
