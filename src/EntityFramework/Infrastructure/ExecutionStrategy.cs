// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;

    /// <summary>
    ///     Provides the base implementation of the retry mechanism for unreliable actions and transient conditions.
    ///     A new instance will be created each time an action is executed.
    /// </summary>
    public class ExecutionStrategy : IExecutionStrategy
    {
        private bool _hasExecuted;

        /// <summary>
        ///     Creates a new instance of <see cref="ExecutionStrategy"/> that use the supplied retry delay strategy and
        ///     retriable exception detector to handle transient failures during action execution.
        /// </summary>
        /// <param name="retryDelayStrategy">The strategy used to determine the delay between execution attempts.</param>
        /// <param name="retriableExceptionDetector">The detector used to detect retriable exceptions.</param>
        public ExecutionStrategy(IRetryDelayStrategy retryDelayStrategy, IRetriableExceptionDetector retriableExceptionDetector)
        {
            Check.NotNull(retryDelayStrategy, "retryDelayStrategy");
            Check.NotNull(retriableExceptionDetector, "retriableExceptionDetector");

            RetryDelayStrategy = retryDelayStrategy;
            RetriableExceptionDetector = retriableExceptionDetector;
        }

        protected IRetryDelayStrategy RetryDelayStrategy { get; private set; }
        protected IRetriableExceptionDetector RetriableExceptionDetector { get; private set; }

        /// <inheritdoc/>
        public bool RetriesOnFailure
        {
            get { return true; }
        }

        /// <summary>
        ///     Repetitively executes the specified action while it satisfies the current retry policy.
        /// </summary>
        /// <param name="action">A delegate representing an executable action that doesn't return any results.</param>
        /// <exception cref="RetryLimitExceededException">if the retry delay strategy determines the action shouldn't be retried anymore</exception>
        /// <exception cref="InvalidOperationException">if an existing transaction is detected and the execution strategy doesn't support it</exception>
        /// <exception cref="InvalidOperationException">if this instance was already used to execute an action</exception>
        public void Execute(Action action)
        {
            Check.NotNull(action, "action");

            Execute(
                () =>
                    {
                        action();
                        return (object)null;
                    });
        }

        /// <summary>
        ///     Repetitively executes the specified action while it satisfies the current retry policy.
        /// </summary>
        /// <typeparam name="TResult">The type of result expected from the executable action.</typeparam>
        /// <param name="func">A delegate representing an executable action that returns the result of type <typeparamref name="TResult"/>.</param>
        /// <returns>The result from the action.</returns>
        /// <exception cref="RetryLimitExceededException">if the retry delay strategy determines the action shouldn't be retried anymore</exception>
        /// <exception cref="InvalidOperationException">if an existing transaction is detected and the execution strategy doesn't support it</exception>
        /// <exception cref="InvalidOperationException">if this instance was already used to execute an action</exception>
        public TResult Execute<TResult>(Func<TResult> func)
        {
            Check.NotNull(func, "func");
            EnsurePreexecutionState();

            return ProtectedExecute(func);
        }

        protected virtual TResult ProtectedExecute<TResult>(Func<TResult> func)
        {
            while (true)
            {
                TimeSpan? delay;

                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    if (!RetriableExceptionDetector.ShouldRetryOn(ex))
                    {
                        throw;
                    }

                    delay = RetryDelayStrategy.GetNextDelay(ex);
                    if (delay == null)
                    {
                        throw new RetryLimitExceededException(ex);
                    }
                }

                if (delay < TimeSpan.Zero)
                {
                    throw Error.ExecutionStrategy_NegativeDelay();
                }

                Thread.Sleep(delay.Value);
            }
        }

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
        /// <exception cref="RetryLimitExceededException">if the retry delay strategy determines the action shouldn't be retried anymore</exception>
        /// <exception cref="InvalidOperationException">if an existing transaction is detected and the execution strategy doesn't support it</exception>
        /// <exception cref="InvalidOperationException">if this instance was already used to execute an action</exception>
        public Task ExecuteAsync(Func<Task> taskFunc)
        {
            Check.NotNull(taskFunc, "taskFunc");

            return ExecuteAsync(taskFunc, CancellationToken.None);
        }

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
        /// <exception cref="RetryLimitExceededException">if the retry delay strategy determines the action shouldn't be retried anymore</exception>
        /// <exception cref="InvalidOperationException">if an existing transaction is detected and the execution strategy doesn't support it</exception>
        /// <exception cref="InvalidOperationException">if this instance was already used to execute an action</exception>
        public Task ExecuteAsync(Func<Task> taskFunc, CancellationToken cancellationToken)
        {
            Check.NotNull(taskFunc, "taskFunc");
            EnsurePreexecutionState();

            return ProtectedExecuteAsync(
                async () =>
                          {
                              await taskFunc().ConfigureAwait(continueOnCapturedContext: false);
                              return true;
                          }, cancellationToken);
        }

        /// <summary>
        ///     Repeatedly executes the specified asynchronous task while it satisfies the current retry policy.
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
        /// <exception cref="RetryLimitExceededException">if the retry delay strategy determines the action shouldn't be retried anymore</exception>
        /// <exception cref="InvalidOperationException">if an existing transaction is detected and the execution strategy doesn't support it</exception>
        /// <exception cref="InvalidOperationException">if this instance was already used to execute an action</exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> taskFunc)
        {
            Check.NotNull(taskFunc, "taskFunc");

            return ExecuteAsync(taskFunc, CancellationToken.None);
        }

        /// <summary>
        ///     Repeatedly executes the specified asynchronous task while it satisfies the current retry policy.
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
        /// <exception cref="RetryLimitExceededException">if the retry delay strategy determines the action shouldn't be retried anymore</exception>
        /// <exception cref="InvalidOperationException">if an existing transaction is detected and the execution strategy doesn't support it</exception>
        /// <exception cref="InvalidOperationException">if this instance was already used to execute an action</exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> taskFunc, CancellationToken cancellationToken)
        {
            Check.NotNull(taskFunc, "taskFunc");
            EnsurePreexecutionState();

            return ProtectedExecuteAsync(taskFunc, cancellationToken);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        protected virtual async Task<TResult> ProtectedExecuteAsync<TResult>(
            Func<Task<TResult>> taskFunc, CancellationToken cancellationToken)
        {
            while (true)
            {
                TimeSpan? delay;

                try
                {
                    return await taskFunc().ConfigureAwait(continueOnCapturedContext: false);
                }
                catch (Exception ex)
                {
                    if (!RetriableExceptionDetector.ShouldRetryOn(ex))
                    {
                        throw;
                    }

                    delay = RetryDelayStrategy.GetNextDelay(ex);
                    if (delay == null)
                    {
                        throw new RetryLimitExceededException(ex);
                    }
                }

                if (delay < TimeSpan.Zero)
                {
                    throw Error.ExecutionStrategy_NegativeDelay();
                }

                await Task.Delay(delay.Value, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }
        }

#endif

        private void EnsurePreexecutionState()
        {
            if (Transaction.Current != null)
            {
                throw Error.ExecutionStrategy_ExistingTransaction();
            }

            if (_hasExecuted)
            {
                throw Error.ExecutionStrategy_AlreadyExecuted();
            }

            _hasExecuted = true;
        }
    }
}
