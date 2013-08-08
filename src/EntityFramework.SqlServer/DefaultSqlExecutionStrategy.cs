// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Core;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.SqlServer.Resources;
    using System.Data.Entity.SqlServer.Utilities;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An <see cref="IDbExecutionStrategy"/> that doesn't affect the execution but will throw a more helpful exception if a transient failure is detected.
    /// </summary>
    internal sealed class DefaultSqlExecutionStrategy : IDbExecutionStrategy
    {
        public bool RetriesOnFailure
        {
            get { return false; }
        }
        
        public void Execute(Action operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }

            Execute(
                () =>
                {
                    operation();
                    return (object)null;
                });
        }

        public TResult Execute<TResult>(Func<TResult> operation)
        {
            Check.NotNull(operation, "operation");
            
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                if (DbExecutionStrategy.UnwrapAndHandleException(ex, SqlAzureRetriableExceptionDetector.ShouldRetryOn))
                {
                    throw new EntityException(Strings.TransientExceptionDetected, ex);
                }

                throw;
            }
        }

#if !NET40

        public Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken)
        {
            Check.NotNull(operation, "operation");

            return ExecuteAsyncImplementation(
                async () =>
                          {
                              await operation().ConfigureAwait(continueOnCapturedContext: false);
                              return true;
                          });
        }

        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken)
        {
            Check.NotNull(operation, "operation");

            return ExecuteAsyncImplementation(operation);
        }

        private async Task<TResult> ExecuteAsyncImplementation<TResult>(Func<Task<TResult>> func)
        {
            try
            {
                return await func().ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception ex)
            {
                if (DbExecutionStrategy.UnwrapAndHandleException(ex, SqlAzureRetriableExceptionDetector.ShouldRetryOn))
                {
                    throw new EntityException(Strings.TransientExceptionDetected, ex);
                }

                throw;
            }
        }
#endif
    }
}
