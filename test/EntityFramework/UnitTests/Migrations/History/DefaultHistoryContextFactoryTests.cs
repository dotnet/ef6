// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.History
{
    using System.Data.SqlClient;
    using Xunit;

    public class DefaultHistoryContextFactoryTests : DbTestCase
    {
        [Fact]
        public void Create_should_return_history_context()
        {
            var historyContextFactory = new DefaultHistoryContextFactory();

            Assert.NotNull(historyContextFactory.Create(new SqlConnection(), true, null));
        }
    }
}
