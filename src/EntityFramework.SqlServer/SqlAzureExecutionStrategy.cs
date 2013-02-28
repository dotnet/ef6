// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Infrastructure;

    /// <summary>
    ///     An <see cref="ExecutionStrategy"/> that uses the <see cref="ExponentialRetryDelayStrategy"/> and
    ///     <see cref="SqlAzureRetriableExceptionDetector"/>.
    /// </summary>
    [DbProviderName("System.Data.SqlClient")]
    public class SqlAzureExecutionStrategy : ExecutionStrategy
    {
        public SqlAzureExecutionStrategy()
            : base(new ExponentialRetryDelayStrategy(), new SqlAzureRetriableExceptionDetector())
        {
        }
    }
}
