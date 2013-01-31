// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Infrastructure;
    using System.Reflection;
    using Xunit;

    public class SqlAzureExecutionStrategyTests
    {
        [Fact]
        public void Default_constructor_uses_ExponentialRetryDelayStrategy_and_SqlAzureRetriableExceptionDetector()
        {
            var executionStrategy = new SqlAzureExecutionStrategy();

            var retryDelayStrategyProperty = executionStrategy.GetType().GetProperty(
                "RetryDelayStrategy",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsType<ExponentialRetryDelayStrategy>(retryDelayStrategyProperty.GetValue(executionStrategy, null));

            var retriableExceptionDetectorProperty = executionStrategy.GetType().GetProperty(
                "RetriableExceptionDetector",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsType<SqlAzureRetriableExceptionDetector>(retriableExceptionDetectorProperty.GetValue(executionStrategy, null));
        }
    }
}
