// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Internal;
    using Xunit;

    /// <summary>
    /// Unit tests for <see cref="DbSqlQuery" /> and <see cref="DbSqlQuery{TEntity}" />.
    /// </summary>
    public class DbSqlQueryTests : TestBase
    {
        [Fact]
        public void Generic_DbSqlQuery_AsNoTracking_returns_new_object_with_no_tracking_flag_set()
        {
            var query = new DbSqlQuery<FakeEntity>(MockHelper.CreateInternalSqlSetQuery("query", false, 1, 2));
            DynamicNoTrackingTest(query);
        }

        [Fact]
        public void Non_generic_DbSqlQuery_AsNoTracking_returns_new_object_with_no_tracking_flag_set()
        {
            var query = new DbSqlQuery(MockHelper.CreateInternalSqlSetQuery("query", false,  1, 2));
            DynamicNoTrackingTest(query);
        }

        private void DynamicNoTrackingTest(dynamic query)
        {
            Assert.False(((InternalSqlSetQuery)query.InternalQuery).IsNoTracking);

            var noTrackingQuery = query.AsNoTracking();

            Assert.NotSame(query, noTrackingQuery);

            var internalQuery = (InternalSqlSetQuery)noTrackingQuery.InternalQuery;
            Assert.True(internalQuery.IsNoTracking);

            Assert.Equal("query", internalQuery.Sql);
            Assert.Equal(2, internalQuery.Parameters.Length);
            Assert.Equal(1, internalQuery.Parameters[0]);
            Assert.Equal(2, internalQuery.Parameters[1]);
        }
    }
}
