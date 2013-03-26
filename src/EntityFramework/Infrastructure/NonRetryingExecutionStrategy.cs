// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
#if !NET40
    using System.Threading;
    using System.Threading.Tasks;
#endif

    /// <summary>
    ///     An <see cref="IExecutionStrategy"/> that doesn't retry operations if they fail.
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

        /// <summary>
        ///     Executes the specified asynchronous task once, without retrying on failure.
        /// </summary>
        /// <param name="func">A function that returns a started task.</param>
        /// <param name="cancellationToken">
        ///     A cancellation token used to cancel the retry operation, but not operations that are already in flight
        ///     or that already completed successfully.
        /// </param>
        /// <returns>
        ///     A task that will run to completion if the original task completes successfully.
        /// </returns>
        public Task ExecuteAsync(Func<Task> func, CancellationToken cancellationToken)
        {
            return func();
        }

        /// <summary>
        ///     Executes the specified asynchronous task once, without retrying on failure.
        /// </summary>
        /// <typeparam name="TResult">
        ///     The result type of the <see cref="Task{T}"/> returned by <paramref name="func"/>.
        /// </typeparam>
        /// <param name="func">A function that returns a started task.</param>
        /// <param name="cancellationToken">
        ///     A cancellation token used to cancel the retry operation, but not operations that are already in flight
        ///     or that already completed successfully.
        /// </param>
        /// <returns>
        ///     A task that will run to completion if the original task completes successfully.
        /// </returns>
        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> func, CancellationToken cancellationToken)
        {
            return func();
        }

#endif
    }
}
