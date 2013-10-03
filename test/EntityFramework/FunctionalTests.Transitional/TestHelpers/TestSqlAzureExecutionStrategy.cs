// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Utilities;
    using System.Threading;
    using System.Threading.Tasks;

    internal class TestSqlAzureExecutionStrategy : IDbExecutionStrategy
    {
        private IDbExecutionStrategy azureExecutionStrategy;

        public TestSqlAzureExecutionStrategy()
        {
            azureExecutionStrategy = new ExtendedSqlAzureExecutionStrategy();
        }

        public bool RetriesOnFailure
        {
            get
            {
                return !FunctionalTestsConfiguration.SuspendExecutionStrategy;
            }
        }

        public void Execute(Action operation)
        {
            Check.NotNull(operation, "operation");

            Execute(
                () =>
                {
                    operation();
                    return (object)null;
                });
        }

        public TResult Execute<TResult>(Func<TResult> operation)
        {
            if (!RetriesOnFailure)
            {
                return operation();
            }
            return azureExecutionStrategy.Execute(operation);
        }

#if !NET40

        public Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken)
        {
            if (!RetriesOnFailure)
            {
                return operation();
            }
            return azureExecutionStrategy.ExecuteAsync(operation, cancellationToken);
        }

        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken)
        {
            if (!RetriesOnFailure)
            {
                return operation();
            }
            return azureExecutionStrategy.ExecuteAsync(operation, cancellationToken);
        }

#endif
    }
}
