// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Core;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.SqlServer.Resources;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     An execution strategy that doesn't affect the execution but will throw a more helpful exception if a transient failure is detected.
    /// </summary>
    internal sealed class DefaultSqlExecutionStrategy : ExecutionStrategy
    {
        public DefaultSqlExecutionStrategy()
            : base(new ExponentialRetryDelayStrategy(), new SqlAzureRetriableExceptionDetector())
        {
        }

        public override bool SupportsExistingTransactions
        {
            get { return true; }
        }

        protected override TResult ProtectedExecute<TResult>(Func<TResult> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                if (RetriableExceptionDetector.ShouldRetryOn(ex))
                {
                    throw new EntityException(Strings.TransientExceptionDetected, ex);
                }

                throw;
            }
        }

#if !NET40

        protected override async Task<TResult> ProtectedExecuteAsync<TResult>(
            Func<Task<TResult>> taskFunc, CancellationToken cancellationToken)
        {
            try
            {
                return await taskFunc().ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception ex)
            {
                if (RetriableExceptionDetector.ShouldRetryOn(ex))
                {
                    throw new EntityException(Strings.TransientExceptionDetected, ex);
                }

                throw;
            }
        }

#endif

    }
}
