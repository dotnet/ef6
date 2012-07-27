// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace ProductivityApiUnitTests
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using Xunit;

    public class DbSqlQueryTests : DbRawSqlQueryTests
    {
        #region AsNoTracking tests

        [Fact]
        public void Generic_DbSqlQuery_AsNoTracking_returns_new_object_with_no_tracking_flag_set()
        {
            var query = new DbSqlQuery<FakeEntity>(CreateInternalSetQuery("query", 1, 2));
            DynamicNoTrackingTest(query);
        }

        [Fact]
        public void Non_generic_DbSqlQuery_AsNoTracking_returns_new_object_with_no_tracking_flag_set()
        {
            var query = new DbSqlQuery(CreateInternalSetQuery("query", 1, 2));
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

        #endregion
    }
}
