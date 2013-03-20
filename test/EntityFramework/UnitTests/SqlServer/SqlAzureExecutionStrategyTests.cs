// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class SqlAzureExecutionStrategyTests
    {
        [Fact]
        public void Has_ProviderInvariantNameAttribute()
        {
            Assert.Equal(
                "System.Data.SqlClient",
                DbProviderNameAttribute.GetFromType(typeof(SqlAzureExecutionStrategy)).Single().Name);
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
            var executionStrategy = new SqlAzureExecutionStrategy();
            var executionCount = 0;

            execute(
                executionStrategy, () =>
                                       {
                                           if (executionCount++ < 3)
                                           {
                                               throw new TimeoutException();
                                           }

                                           return executionCount;
                                       });

            Assert.Equal(4, executionCount);
        }

#if !NET40

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
            var executionStrategy = new SqlAzureExecutionStrategy();

            var executionCount = 0;

            executeAsync(
                executionStrategy, () =>
                                       {
                                           if (executionCount++ < 3)
                                           {
                                               throw new TimeoutException();
                                           }

                                           return Task.FromResult(executionCount);
                                       }).Wait();

            Assert.Equal(4, executionCount);
        }

#endif
    }
}
