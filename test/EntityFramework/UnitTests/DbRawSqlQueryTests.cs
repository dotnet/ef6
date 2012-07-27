// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity
{
    using System;
    using System.ComponentModel;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Resources;
    using Xunit;

    /// <summary>
    /// Unit tests for <see cref="DbRawSqlQuery"/> and <see cref="DbRawSqlQuery{TElement}"/>.
    /// </summary> 
    public class DbRawSqlQueryTests : TestBase
    {
        #region ToString tests

        [Fact]
        public void Generic_non_entity_SQL_query_ToString_returns_the_query()
        {
            var query = new DbRawSqlQuery<FakeEntity>(MockHelper.CreateInternalSqlNonSetQuery("select * from products"));

            Assert.Equal("select * from products", query.ToString());
        }

        [Fact]
        public void Generic_non_entity_SQL_query_ToString_returns_the_query_but_not_the_parameters()
        {
            var query = new DbRawSqlQuery<FakeEntity>(MockHelper.CreateInternalSqlNonSetQuery("select * from Products where Id < {0} and CategoryId = {1}", 4, "Beverages"));

            Assert.Equal("select * from Products where Id < {0} and CategoryId = {1}", query.ToString());
        }

        [Fact]
        public void Non_generic_DbSqlQuery_ToString_returns_the_query()
        {
            var query = new DbRawSqlQuery(MockHelper.CreateInternalSqlSetQuery("select * from products"));

            Assert.Equal("select * from products", query.ToString());
        }

        [Fact]
        public void Non_generic_DbSqlQuery_ToString_returns_the_query_but_not_the_parameters()
        {
            var query = new DbRawSqlQuery(MockHelper.CreateInternalSqlSetQuery("select * from Products where Id < {0} and CategoryId = {1}", 4, "Beverages"));

            Assert.Equal("select * from Products where Id < {0} and CategoryId = {1}", query.ToString());
        }

        [Fact]
        public void Generic_DbSqlQuery_ToString_returns_the_query()
        {
            var query = new DbRawSqlQuery<FakeEntity>(MockHelper.CreateInternalSqlSetQuery("select * from products"));

            Assert.Equal("select * from products", query.ToString());
        }

        [Fact]
        public void Generic_DbSqlQuery_ToString_returns_the_query_but_not_the_parameters()
        {
            var query = new DbRawSqlQuery<FakeEntity>(MockHelper.CreateInternalSqlSetQuery("select * from Products where Id < {0} and CategoryId = {1}", 4, "Beverages"));

            Assert.Equal("select * from Products where Id < {0} and CategoryId = {1}", query.ToString());
        }

        #endregion

        #region DbRawSqlQuery as IListSource tests

        [Fact]
        public void Non_entity_SQL_query_ContainsListCollection_returns_false()
        {
            var query = new DbRawSqlQuery<FakeEntity>(MockHelper.CreateInternalSqlNonSetQuery("query"));

            Assert.False(((IListSource)query).ContainsListCollection);
        }

        [Fact]
        public void Non_entity_SQL_query_GetList_throws_indicating_that_binding_to_queries_is_not_allowed()
        {
            var query = new DbRawSqlQuery<Random>(MockHelper.CreateInternalSqlNonSetQuery("query"));

            Assert.Equal(Strings.DbQuery_BindingToDbQueryNotSupported, Assert.Throws<NotSupportedException>(() => ((IListSource)query).GetList()).Message);
        }

        [Fact]
        public void DbSqlQuery_ContainsListCollection_returns_false()
        {
            var query = new DbRawSqlQuery<FakeEntity>(MockHelper.CreateInternalSqlSetQuery("query"));

            Assert.False(((IListSource)query).ContainsListCollection);
        }

        [Fact]
        public void Non_generic_DbSqlQuery_ContainsListCollection_returns_false()
        {
            var query = new DbRawSqlQuery(MockHelper.CreateInternalSqlSetQuery("query"));

            Assert.False(((IListSource)query).ContainsListCollection);
        }

        [Fact]
        public void DbSqlQuery_GetList_throws_indicating_that_binding_to_queries_is_not_allowed()
        {
            var query = new DbRawSqlQuery<FakeEntity>(MockHelper.CreateInternalSqlSetQuery("query"));

            Assert.Equal(Strings.DbQuery_BindingToDbQueryNotSupported, Assert.Throws<NotSupportedException>(() => ((IListSource)query).GetList()).Message);
        }

        [Fact]
        public void Non_generic_DbSqlQuery_GetList_throws_indicating_that_binding_to_queries_is_not_allowed()
        {
            var query = new DbRawSqlQuery(MockHelper.CreateInternalSqlSetQuery("query"));

            Assert.Equal(Strings.DbQuery_BindingToDbQueryNotSupported, Assert.Throws<NotSupportedException>(() => ((IListSource)query).GetList()).Message);
        }

        #endregion
    }
}
