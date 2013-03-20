// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Core;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.SqlServer.Resources;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     An <see cref="IExecutionStrategy"/> that doesn't affect the execution but will throw a more helpful exception if a transient failure is detected.
    /// </summary>
    internal sealed class DefaultSqlExecutionStrategy : IExecutionStrategy
    {
        public bool RetriesOnFailure
        {
            get { return false; }
        }
        
        public void Execute(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            Execute(
                () =>
                {
                    action();
                    return (object)null;
                });
        }

        public TResult Execute<TResult>(Func<TResult> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }
            
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                if (ExecutionStrategyBase.UnwrapAndHandleException(ex, SqlAzureRetriableExceptionDetector.ShouldRetryOn))
                {
                    throw new EntityException(Strings.TransientExceptionDetected, ex);
                }

                throw;
            }
        }

#if !NET40

        public Task ExecuteAsync(Func<Task> func, CancellationToken cancellationToken)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            return ExecuteAsyncImplementation(
                async () =>
                          {
                              await func().ConfigureAwait(continueOnCapturedContext: false);
                              return true;
                          });
        }

        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> func, CancellationToken cancellationToken)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            return ExecuteAsyncImplementation(func);
        }

        private async Task<TResult> ExecuteAsyncImplementation<TResult>(Func<Task<TResult>> func)
        {
            try
            {
                return await func().ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception ex)
            {
                if (ExecutionStrategyBase.UnwrapAndHandleException(ex, SqlAzureRetriableExceptionDetector.ShouldRetryOn))
                {
                    throw new EntityException(Strings.TransientExceptionDetected, ex);
                }

                throw;
            }
        }
#endif
    }
}
