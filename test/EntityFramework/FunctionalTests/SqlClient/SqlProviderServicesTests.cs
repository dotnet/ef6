// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;
    using Moq;
    using Xunit;

    public class SqlProviderServicesTests : TestBase
    {
        [Fact]
        public void DatabaseExists_uses_ExecutionStrategy()
        {
            var executionStrategyMock = new Mock<IExecutionStrategy>();
            executionStrategyMock.Setup(m => m.Execute(It.IsAny<Action>())).Callback<Action>(a => a());

            MutableResolver.AddResolver<IExecutionStrategy>(key => executionStrategyMock.Object);
            try
            {
                var connection = new SqlConnection(SimpleConnectionString(("master")));
                Assert.True(
                    DbProviderServices.GetProviderServices(connection).DatabaseExists(
                        connection, null,
                        new Mock<StoreItemCollection>().Object));
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }

            executionStrategyMock.Verify(m => m.Execute(It.IsAny<Action>()), Times.Once());
        }
    }
}
