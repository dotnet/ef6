// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public class SqlProviderServicesTests
    {
        [Fact]
        public void GetExecutionStrategyFactory_returns_DefaultSqlExecutionStrategy()
        {
            Assert.IsType<DefaultSqlExecutionStrategy>(SqlProviderServices.Instance.GetExecutionStrategyFactory()());
        }

        [Fact]
        public void Has_ProviderInvariantNameAttribute()
        {
            Assert.Equal(
                "System.Data.SqlClient",
                DbProviderNameAttribute.GetFromType(typeof(SqlProviderServices)).Single().Name);
        }
    }
}
