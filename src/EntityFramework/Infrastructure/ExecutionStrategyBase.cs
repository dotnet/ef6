// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;

    /// <summary>
    ///     Provides the base implementation of the retry mechanism for unreliable actions and transient conditions that uses
    ///     exponentially increasing delays between retries.
    /// </summary>
    /// <remarks>
    ///     A new instance will be created each time an action is executed.
    /// 
    ///     The following formula is used to calculate the delay after <c>retryCount</c> number of attempts:
    ///     <code>min(random(1, 1.1) * (2 ^ retryCount - 1), maxDelay)</code>
    ///     The <c>retryCount</c> starts at 0.
    ///     The random factor distributes uniformly the retry attempts from multiple parallel actions failing simultaneously.
    /// </remarks>
    public abstract class ExecutionStrategyBase : IExecutionStrategy
    {
        private bool _hasExecuted;
        private readonly List<Exception> _exceptionsEncountered = new List<Exception>();
        private readonly Random _random = new Random();

        private readonly int _maxRetryCount;
        private readonly TimeSpan _maxDelay;

        /// <summary>
        ///     The default number of retry attempts, must be nonnegative.
        /// </summary>
        private const int DefaultMaxRetryCount = 5;

        /// <summary>
        ///     The default maximum random factor, must not be lesser than 1.
        /// </summary>
        private const double DefaultRandomFactor = 1.1;

        /// <summary>
        ///     The default base for the exponential function used to compute the delay between retries, must be positive.
        /// </summary>
        private const double DefaultExponentialBase = 2;

        /// <summary>
        ///     The default coefficient for the exponential function used to compute the delay between retries, must be nonnegative.
        /// </summary>
        private static readonly TimeSpan DefaultCoefficient = TimeSpan.FromSeconds(1);

        /// <summary>
        ///     The default maximum time delay between retries, must be nonnegative.
        /// </summary>
        private static readonly TimeSpan DefaultMaxDelay = TimeSpan.FromSeconds(30);

        /// <summary>
        ///     Creates a new instance of <see cref="ExecutionStrategyBase"/>.
        /// </summary>
        /// <remarks>
        ///     The default retry limit is 5, which means that the total amout of time spent between retries is 26 seconds plus the random factor.
        /// </remarks>
        protected ExecutionStrategyBase()
            : this(DefaultMaxRetryCount, DefaultMaxDelay)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="ExecutionStrategyBase"/> with the specified limits for number of retries and the delay between retries.
        /// </summary>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        /// <param name="maxDelay"> The maximum delay in milliseconds between retries. </param>
        protected ExecutionStrategyBase(int maxRetryCount, TimeSpan maxDelay)
        {
            if (maxRetryCount < 0)
            {
                throw new ArgumentOutOfRangeException("maxRetryCount");
            }
            if (maxDelay.TotalMilliseconds < 0.0)
            {
                throw new ArgumentOutOfRangeException("maxDelay");
            }

            _maxRetryCount = maxRetryCount;
            _maxDelay = maxDelay;
        }

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
                    if (!UnwrapAndHandleException(ex, ShouldRetryOn))
                    {
                        throw;
                    }

                    delay = GetNextDelay(ex);
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
        /// <param name="func">A function that returns a started task.</param>
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
        public Task ExecuteAsync(Func<Task> func, CancellationToken cancellationToken)
        {
            Check.NotNull(func, "func");
            EnsurePreexecutionState();

            return ProtectedExecuteAsync(
                async () =>
                          {
                              await func().ConfigureAwait(continueOnCapturedContext: false);
                              return true;
                          }, cancellationToken);
        }

        /// <summary>
        ///     Repeatedly executes the specified asynchronous task while it satisfies the current retry policy.
        /// </summary>
        /// <typeparam name="TResult">
        ///     The result type of the <see cref="Task{T}"/> returned by <paramref name="func"/>.
        /// </typeparam>
        /// <param name="func">A function that returns a started task of type <typeparamref name="TResult"/>.</param>
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
        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> func, CancellationToken cancellationToken)
        {
            Check.NotNull(func, "func");
            EnsurePreexecutionState();

            return ProtectedExecuteAsync(func, cancellationToken);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        protected virtual async Task<TResult> ProtectedExecuteAsync<TResult>(
            Func<Task<TResult>> func, CancellationToken cancellationToken)
        {
            while (true)
            {
                TimeSpan? delay;

                try
                {
                    return await func().ConfigureAwait(continueOnCapturedContext: false);
                }
                catch (Exception ex)
                {
                    if (!ShouldRetryOn(ex))
                    {
                        throw;
                    }

                    delay = GetNextDelay(ex);
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

        /// <summary>
        ///     Determines whether the action should be retried and the delay before the next attempt.
        /// </summary>
        /// <param name="lastException">The exception thrown during the last execution attempt.</param>
        /// <returns>
        ///     Returns the delay indicating how long to wait for before the next execution attempt if the action should be retried;
        ///     <c>null</c> otherwise
        /// </returns>
        protected internal virtual TimeSpan? GetNextDelay(Exception lastException)
        {
            _exceptionsEncountered.Add(lastException);

            var currentRetryCount = _exceptionsEncountered.Count - 1;
            if (currentRetryCount < _maxRetryCount)
            {
                var delta = (Math.Pow(DefaultExponentialBase, currentRetryCount) - 1.0)
                            * (1.0 + _random.NextDouble() * (DefaultRandomFactor - 1.0));

                var delay = Math.Min(
                    DefaultCoefficient.TotalMilliseconds * delta,
                    _maxDelay.TotalMilliseconds);

                return TimeSpan.FromMilliseconds(delay);
            }

            return null;
        }

        /// <summary>
        ///     Recursively gets InnerException from <paramref name="exception"/> as long as it's an 
        ///     <see cref="EntityException"/>, <see cref="DbUpdateException"/> or <see cref="UpdateException"/>
        ///     and passes it to <paramref name="exceptionHandler"/>
        /// </summary>
        /// <param name="exception"> The exception to be unwrapped. </param>
        /// <param name="exceptionHandler"> A delegate that will be called with the unwrapped exception. </param>
        /// <returns> The result from <paramref name="exceptionHandler"/>. </returns>
        public static T UnwrapAndHandleException<T>(Exception exception, Func<Exception, T> exceptionHandler)
        {
            var entityException = exception as EntityException;
            if (entityException != null)
            {
                return UnwrapAndHandleException(entityException.InnerException, exceptionHandler);
            }

            var dbUpdateException = exception as DbUpdateException;
            if (dbUpdateException != null)
            {
                return UnwrapAndHandleException(dbUpdateException.InnerException, exceptionHandler);
            }

            var updateException = exception as UpdateException;
            if (updateException != null)
            {
                return UnwrapAndHandleException(updateException.InnerException, exceptionHandler);
            }

            return exceptionHandler(exception);
        }

        /// <summary>
        ///     Determines whether the specified exception represents a transient failure that can be compensated by a retry.
        /// </summary>
        /// <param name="ex">The exception object to be verified.</param>
        /// <returns><c>true</c> if the specified exception is considered as transient, otherwise <c>false</c>.</returns>
        protected abstract bool ShouldRetryOn(Exception exception);
    }
}
