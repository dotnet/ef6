﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Infrastructure;
#if USES_MICROSOFT_DATA_SQLCLIENT
    using Microsoft.Data.SqlClient;
#else
    using System.Data.SqlClient;
#endif

    /// <summary>
    /// An <see cref="IDbExecutionStrategy"/> that retries actions that throw exceptions caused by SQL Azure transient failures.
    /// </summary>
    /// <remarks>
    /// This execution strategy will retry the operation on <see cref="TimeoutException"/> and <see cref="SqlException"/>
    /// if the <see cref="SqlException.Errors"/> contains any of the following error numbers:
    /// 40613, 40501, 40197, 10929, 10928, 10060, 10054, 10053, 233, 64 and 20
    /// </remarks>
#if USES_MICROSOFT_DATA_SQLCLIENT
    public class MicrosoftSqlAzureExecutionStrategy : DbExecutionStrategy
#else
    public class SqlAzureExecutionStrategy : DbExecutionStrategy
#endif
    {
#if USES_MICROSOFT_DATA_SQLCLIENT
        /// <summary>
        /// Creates a new instance of <see cref="MicrosoftSqlAzureExecutionStrategy" />.
        /// </summary>
        /// <remarks>
        /// The default retry limit is 5, which means that the total amount of time spent between retries is 26 seconds plus the random factor.
        /// </remarks>
        public MicrosoftSqlAzureExecutionStrategy()
#else
        /// <summary>
        /// Creates a new instance of <see cref="SqlAzureExecutionStrategy" />.
        /// </summary>
        /// <remarks>
        /// The default retry limit is 5, which means that the total amount of time spent between retries is 26 seconds plus the random factor.
        /// </remarks>
        public SqlAzureExecutionStrategy()
#endif
        {
        }

#if USES_MICROSOFT_DATA_SQLCLIENT
        /// <summary>
        /// Creates a new instance of <see cref="MicrosoftSqlAzureExecutionStrategy" /> with the specified limits for
        /// number of retries and the delay between retries.
        /// </summary>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        /// <param name="maxDelay"> The maximum delay in milliseconds between retries. </param>
        public MicrosoftSqlAzureExecutionStrategy(int maxRetryCount, TimeSpan maxDelay)
#else
        /// <summary>
        /// Creates a new instance of <see cref="SqlAzureExecutionStrategy" /> with the specified limits for
        /// number of retries and the delay between retries.
        /// </summary>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        /// <param name="maxDelay"> The maximum delay in milliseconds between retries. </param>
        public SqlAzureExecutionStrategy(int maxRetryCount, TimeSpan maxDelay)
#endif
            : base(maxRetryCount, maxDelay)
        {
        }

        /// <inheritdoc/>
        protected override bool ShouldRetryOn(Exception exception)
        {
            return SqlAzureRetriableExceptionDetector.ShouldRetryOn(exception);
        }
    }
}
