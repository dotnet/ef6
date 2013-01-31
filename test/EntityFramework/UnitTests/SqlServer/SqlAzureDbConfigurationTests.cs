// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Infrastructure;
    using Xunit;

    public class SqlAzureDbConfigurationTests
    {
        [Fact]
        public void Constructor_adds_resolver_for_SqlAzureExecutionStrategy()
        {
            Assert.IsType<SqlAzureExecutionStrategy>(
                new SqlAzureDbConfiguration().InternalConfiguration.GetService<IExecutionStrategy>(
                    new ExecutionStrategyKey("System.Data.SqlClient", "foo")));
        }
    }
}
