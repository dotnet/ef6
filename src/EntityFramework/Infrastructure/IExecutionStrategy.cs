namespace System.Data.Entity.Infrastructure
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IExecutionStrategy
    {
        /// <summary>
        ///     Indicates whether this <see cref="ExecutionStrategy"/> supports transactions started before the action is executed.
        ///     Most strategies will retry the action execution after a failure and thus cannot support existing transactions.
        /// </summary>
        bool SupportsExistingTransactions { get; }

        /// <summary>
        ///     Repetitively executes the specified action while it satisfies the current retry policy.
        /// </summary>
        /// <param name="action">A delegate representing an executable action that doesn't return any results.</param>
        /// <exception cref="RetryLimitExceededException">if the retry delay strategy determines the action shouldn't be retried anymore</exception>
        /// <exception cref="InvalidOperationException">if an existing transaction is detected and the execution strategy doesn't support it</exception>
        /// <exception cref="InvalidOperationException">if this instance was already used to execute an action</exception>
        void Execute(Action action);

        /// <summary>
        ///     Repetitively executes the specified action while it satisfies the current retry policy.
        /// </summary>
        /// <typeparam name="TResult">The type of result expected from the executable action.</typeparam>
        /// <param name="func">A delegate representing an executable action that returns the result of type <typeparamref name="TResult"/>.</param>
        /// <returns>The result from the action.</returns>
        /// <exception cref="RetryLimitExceededException">if the retry delay strategy determines the action shouldn't be retried anymore</exception>
        /// <exception cref="InvalidOperationException">if an existing transaction is detected and the execution strategy doesn't support it</exception>
        /// <exception cref="InvalidOperationException">if this instance was already used to execute an action</exception>
        TResult Execute<TResult>(Func<TResult> func);
        
#if !NET40

        /// <summary>
        ///     Repetitively executes the specified asynchronous task while it satisfies the current retry policy.
        /// </summary>
        /// <param name="taskAction">A function that returns a started task.</param>
        /// <returns>
        ///     A task that will run to completion if the original task completes successfully (either the
        ///     first time or after retrying transient failures). If the task fails with a non-transient error or
        ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
        /// </returns>
        /// <exception cref="InvalidOperationException">if an existing transaction is detected and the execution strategy doesn't support it</exception>
        /// <exception cref="InvalidOperationException">if this instance was already used to execute an action</exception>
        Task ExecuteAsync(Func<Task> taskFunc);

        /// <summary>
        ///     Repetitively executes the specified asynchronous task while it satisfies the current retry policy.
        /// </summary>
        /// <param name="taskAction">A function that returns a started task.</param>
        /// <param name="cancellationToken">
        ///     A cancellation token used to cancel the retry operation, but not operations that are already in flight
        ///     or that already completed successfully.
        /// </param>
        /// <returns>
        ///     A task that will run to completion if the original task completes successfully (either the
        ///     first time or after retrying transient failures). If the task fails with a non-transient error or
        ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
        /// </returns>
        /// <exception cref="InvalidOperationException">if an existing transaction is detected and the execution strategy doesn't support it</exception>
        /// <exception cref="InvalidOperationException">if this instance was already used to execute an action</exception>
        Task ExecuteAsync(Func<Task> taskFunc, CancellationToken cancellationToken);

        /// <summary>
        ///     Repeatedly executes the specified asynchronous task while it satisfies the current retry policy.
        /// </summary>
        /// <param name="taskFunc">A function that returns a started task of type <typeparamref name="TResult"/>.</param>
        /// <returns>
        ///     A task that will run to completion if the original task completes successfully (either the
        ///     first time or after retrying transient failures). If the task fails with a non-transient error or
        ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
        /// </returns>
        /// <exception cref="InvalidOperationException">if an existing transaction is detected and the execution strategy doesn't support it</exception>
        /// <exception cref="InvalidOperationException">if this instance was already used to execute an action</exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> taskFunc);

        /// <summary>
        ///     Repeatedly executes the specified asynchronous task while it satisfies the current retry policy.
        /// </summary>
        /// <param name="taskFunc">A function that returns a started task of type <typeparamref name="TResult"/>.</param>
        /// <param name="cancellationToken">
        ///     A cancellation token used to cancel the retry operation, but not operations that are already in flight
        ///     or that already completed successfully.
        /// </param>
        /// <returns>
        ///     A task that will run to completion if the original task completes successfully (either the
        ///     first time or after retrying transient failures). If the task fails with a non-transient error or
        ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
        /// </returns>
        /// <exception cref="InvalidOperationException">if an existing transaction is detected and the execution strategy doesn't support it</exception>
        /// <exception cref="InvalidOperationException">if this instance was already used to execute an action</exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> taskFunc, CancellationToken cancellationToken);

#endif

    }
}