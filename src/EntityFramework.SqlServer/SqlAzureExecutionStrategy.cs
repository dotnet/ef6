// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Infrastructure;

    /// <summary>
    ///     An <see cref="IExecutionStrategy"/> that retries actions that throw exceptions caused by SQL Azure transient failures.
    /// </summary>
    [DbProviderName("System.Data.SqlClient")]
    public class SqlAzureExecutionStrategy : ExecutionStrategyBase
    {
        /// <inheritdoc/>
        protected override bool ShouldRetryOn(Exception exception)
        {
            return SqlAzureRetriableExceptionDetector.ShouldRetryOn(exception);
        }
    }
}
