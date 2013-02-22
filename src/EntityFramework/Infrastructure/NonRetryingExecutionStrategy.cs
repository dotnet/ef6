// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
#if !NET40
    using System.Threading;
    using System.Threading.Tasks;
#endif

    /// <summary>
    ///     An <see cref="ExecutionStrategy"/> that doesn't retry operations if they fail and supports existing transactions.
    /// </summary>
    public class NonRetryingExecutionStrategy : IExecutionStrategy
    {
        public bool RetriesOnFailure
        {
            get { return false; }
        }

        public void Execute(Action action)
        {
            action();
        }

        public TResult Execute<TResult>(Func<TResult> func)
        {
            return func();
        }
        
#if !NET40

        public Task ExecuteAsync(Func<Task> taskFunc)
        {
            return taskFunc();
        }

        public Task ExecuteAsync(Func<Task> taskFunc, CancellationToken cancellationToken)
        {
            return taskFunc();
        }

        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> taskFunc)
        {
            return taskFunc();
        }

        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> taskFunc, CancellationToken cancellationToken)
        {
            return taskFunc();
        }

#endif
    }
}
