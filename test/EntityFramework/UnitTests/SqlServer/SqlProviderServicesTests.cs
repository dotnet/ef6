// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using Xunit;

    public class SqlProviderServicesTests
    {
        [Fact]
        public void GetExecutionStrategy_returns_DefaultSqlExecutionStrategy()
        {
            Assert.IsType<DefaultSqlExecutionStrategy>(SqlProviderServices.Instance.GetExecutionStrategy());
        }
    }
}
