namespace System.Data.Entity.Infrastructure
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IExecutionStrategy
    {
        /// <summary>
        ///     Indicates whether this <see cref="IExecutionStrategy"/> might retry the execution after a failure.
        /// </summary>
        bool RetriesOnFailure { get; }

        /// <summary>
        ///     Executes the specified action.
        /// </summary>
        /// <param name="action">A delegate representing an executable action that doesn't return any results.</param>
        void Execute(Action action);

        /// <summary>
        ///     Executes the specified function and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The return type of <paramref name="func"/>.</typeparam>
        /// <param name="func">A delegate representing an executable action that returns the result of type <typeparamref name="TResult"/>.</param>
        /// <returns>The result from the action.</returns>
        TResult Execute<TResult>(Func<TResult> func);
        
#if !NET40

        /// <summary>
        ///     Executes the specified asynchronous action.
        /// </summary>
        /// <param name="taskAction">A function that returns a started task.</param>
        /// <returns>
        ///     A task that will run to completion if the original task completes successfully (either the
        ///     first time or after retrying transient failures). If the task fails with a non-transient error or
        ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
        /// </returns>
        Task ExecuteAsync(Func<Task> taskFunc);

        /// <summary>
        ///     Executes the specified asynchronous action.
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
        Task ExecuteAsync(Func<Task> taskFunc, CancellationToken cancellationToken);

        /// <summary>
        ///     Executes the specified asynchronous function and returns the result.
        /// </summary>
        /// <typeparam name="TResult">
        ///     The type parameter of the <see cref="Task{T}"/> returned by <paramref name="taskFunc"/>.
        /// </typeparam>
        /// <param name="taskFunc">A function that returns a started task of type <typeparamref name="TResult"/>.</param>
        /// <returns>
        ///     A task that will run to completion if the original task completes successfully (either the
        ///     first time or after retrying transient failures). If the task fails with a non-transient error or
        ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> taskFunc);

        /// <summary>
        ///     Executes the specified asynchronous function and returns the result.
        /// </summary>
        /// <typeparam name="TResult">
        ///     The type parameter of the <see cref="Task{T}"/> returned by <paramref name="taskFunc"/>.
        /// </typeparam>
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
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> taskFunc, CancellationToken cancellationToken);

#endif

    }
}