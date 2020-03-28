// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Utilities;
    using System.Threading;
    using System.Threading.Tasks;

    public class SuspendableSqlAzureExecutionStrategy : IDbExecutionStrategy
    {
        private readonly IDbExecutionStrategy _azureExecutionStrategy;

        public SuspendableSqlAzureExecutionStrategy()
        {
            _azureExecutionStrategy = new ExtendedSqlAzureExecutionStrategy();
        }

        public bool RetriesOnFailure
        {
            get
            {
                return !FunctionalTestsConfiguration.SuspendExecutionStrategy;
            }
        }

        public virtual void Execute(Action operation)
        {
            Check.NotNull(operation, "operation");

            Execute(
                () =>
                {
                    operation();
                    return (object)null;
                });
        }

        public virtual TResult Execute<TResult>(Func<TResult> operation)
        {
            if (!RetriesOnFailure)
            {
                return operation();
            }
            return _azureExecutionStrategy.Execute(operation);
        }

#if !NET40

        public virtual Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken)
        {
            if (!RetriesOnFailure)
            {
                return operation();
            }
            return _azureExecutionStrategy.ExecuteAsync(operation, cancellationToken);
        }

        public virtual Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken)
        {
            if (!RetriesOnFailure)
            {
                return operation();
            }
            return _azureExecutionStrategy.ExecuteAsync(operation, cancellationToken);
        }

#endif
    }
}
