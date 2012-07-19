namespace ProductivityApiUnitTests
{
    using System;
    using System.ComponentModel;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.Linq;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Resources;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit tests for the SQL query classes.
    /// </summary>
    public class DbSqlQueryTests : TestBase
    {
        #region ToString tests

        private InternalSqlSetQuery CreateInternalSetQuery(string sql, params object[] parameters)
        {
            return new InternalSqlSetQuery(new Mock<InternalSetForMock<FakeEntity>>().Object, sql, false, parameters);
        }

        private InternalSqlQuery CreateInternalNonSetQuery(string sql, params object[] parameters)
        {
            return new InternalSqlNonSetQuery(new Mock<InternalContextForMock>().Object, typeof(object), sql, parameters);
        }

        [Fact]
        public void Non_generic_non_entity_SQL_query_ToString_returns_the_query()
        {
            var query = CreateInternalNonSetQuery("select * from products");

            Assert.Equal("select * from products", query.ToString());
        }

        [Fact]
        public void Non_generic_non_entity_SQL_query_ToString_returns_the_query_but_not_the_parameters()
        {
            var query = CreateInternalNonSetQuery("select * from Products where Id < {0} and CategoryId = {1}", 4, "Beverages");

            Assert.Equal("select * from Products where Id < {0} and CategoryId = {1}", query.ToString());
        }

        [Fact]
        public void Generic_non_entity_SQL_query_ToString_returns_the_query()
        {
            var query = new DbSqlQuery<FakeEntity>(CreateInternalNonSetQuery("select * from products"));

            Assert.Equal("select * from products", query.ToString());
        }

        [Fact]
        public void Generic_non_entity_SQL_query_ToString_returns_the_query_but_not_the_parameters()
        {
            var query = new DbSqlQuery<FakeEntity>(CreateInternalNonSetQuery("select * from Products where Id < {0} and CategoryId = {1}", 4, "Beverages"));

            Assert.Equal("select * from Products where Id < {0} and CategoryId = {1}", query.ToString());
        }

        [Fact]
        public void Non_generic_DbSqlQuery_ToString_returns_the_query()
        {
            var query = new DbSqlSetQuery(CreateInternalSetQuery("select * from products"));

            Assert.Equal("select * from products", query.ToString());
        }

        [Fact]
        public void Non_generic_DbSqlvQuery_ToString_returns_the_query_but_not_the_parameters()
        {
            var query = new DbSqlQuery(CreateInternalSetQuery("select * from Products where Id < {0} and CategoryId = {1}", 4, "Beverages"));

            Assert.Equal("select * from Products where Id < {0} and CategoryId = {1}", query.ToString());
        }

        [Fact]
        public void Generic_DbSqlQuery_ToString_returns_the_query()
        {
            var query = new DbSqlSetQuery<FakeEntity>(CreateInternalSetQuery("select * from products"));

            Assert.Equal("select * from products", query.ToString());
        }

        [Fact]
        public void Generic_DbSqlQuery_ToString_returns_the_query_but_not_the_parameters()
        {
            var query = new DbSqlSetQuery<FakeEntity>(CreateInternalSetQuery("select * from Products where Id < {0} and CategoryId = {1}", 4, "Beverages"));

            Assert.Equal("select * from Products where Id < {0} and CategoryId = {1}", query.ToString());
        }

        #endregion

        #region AsNoTracking tests

        [Fact]
        public void Generic_DbSqlQuery_AsNoTracking_returns_new_object_with_no_tracking_flag_set()
        {
            var query = new DbSqlSetQuery<FakeEntity>(CreateInternalSetQuery("query", 1, 2));
            DynamicNoTrackingTest(query);
        }

        [Fact]
        public void Non_generic_DbSqlQuery_AsNoTracking_returns_new_object_with_no_tracking_flag_set()
        {
            var query = new DbSqlSetQuery(CreateInternalSetQuery("query", 1, 2));
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

        #region Query construction tests

        [Fact]
        public void Passing_null_SQL_to_generic_entity_query_method_throws()
        {
            var set = new DbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("sql"), Assert.Throws<ArgumentException>(() => set.SqlQuery(null)).Message);
        }

        [Fact]
        public void Passing_empty_SQL_string_to_generic_entity_query_method_throws()
        {
            var set = new DbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("sql"), Assert.Throws<ArgumentException>(() => set.SqlQuery("")).Message);
        }

        [Fact]
        public void Passing_whitespace_SQL_string_to_generic_entity_query_method_throws()
        {
            var set = new DbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("sql"), Assert.Throws<ArgumentException>(() => set.SqlQuery(" ")).Message);
        }

        [Fact]
        public void Passing_null_parameters_to_generic_entity_query_method_throws()
        {
            var set = new DbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);

            Assert.Equal("parameters", Assert.Throws<ArgumentNullException>(() => set.SqlQuery("query", null)).ParamName);
        }

        [Fact]
        public void Passing_null_SQL_to_non_generic_entity_query_method_throws()
        {
            var set = new InternalDbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("sql"), Assert.Throws<ArgumentException>(() => set.SqlQuery(null)).Message);
        }

        [Fact]
        public void Passing_empty_SQL_string_to_non_generic_entity_query_method_throws()
        {
            var set = new InternalDbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("sql"), Assert.Throws<ArgumentException>(() => set.SqlQuery("")).Message);
        }

        [Fact]
        public void Passing_whitespace_SQL_string_to_non_generic_entity_query_method_throws()
        {
            var set = new InternalDbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("sql"), Assert.Throws<ArgumentException>(() => set.SqlQuery(" ")).Message);
        }

        [Fact]
        public void Passing_null_parameters_to_non_generic_entity_query_method_throws()
        {
            var set = new InternalDbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);

            Assert.Equal("parameters", Assert.Throws<ArgumentNullException>(() => set.SqlQuery("query", null)).ParamName);
        }

        [Fact]
        public void Passing_null_SQL_to_generic_database_query_method_throws()
        {
            var database = new Database(new Mock<InternalContextForMock>().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("sql"), Assert.Throws<ArgumentException>(() => database.SqlQuery<Random>(null)).Message);
        }

        [Fact]
        public void Passing_empty_SQL_string_to_generic_database_query_method_throws()
        {
            var database = new Database(new Mock<InternalContextForMock>().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("sql"), Assert.Throws<ArgumentException>(() => database.SqlQuery<Random>("")).Message);
        }

        [Fact]
        public void Passing_whitespace_SQL_string_to_generic_database_query_method_throws()
        {
            var database = new Database(new Mock<InternalContextForMock>().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("sql"), Assert.Throws<ArgumentException>(() => database.SqlQuery<Random>(" ")).Message);
        }

        [Fact]
        public void Passing_null_parameters_to_generic_database_query_method_throws()
        {
            var database = new Database(new Mock<InternalContextForMock>().Object);

            Assert.Equal("parameters", Assert.Throws<ArgumentNullException>(() => database.SqlQuery<Random>("query", null)).ParamName);
        }

        [Fact]
        public void Passing_null_SQL_to_non_generic_database_query_method_throws()
        {
            var database = new Database(new Mock<InternalContextForMock>().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("sql"), Assert.Throws<ArgumentException>(() => database.SqlQuery(typeof(Random), null)).Message);
        }

        [Fact]
        public void Passing_empty_SQL_string_to_non_generic_database_query_method_throws()
        {
            var database = new Database(new Mock<InternalContextForMock>().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("sql"), Assert.Throws<ArgumentException>(() => database.SqlQuery(typeof(Random), "")).Message);
        }

        [Fact]
        public void Passing_whitespace_SQL_string_to_non_generic_database_query_method_throws()
        {
            var database = new Database(new Mock<InternalContextForMock>().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("sql"), Assert.Throws<ArgumentException>(() => database.SqlQuery(typeof(Random), " ")).Message);
        }

        [Fact]
        public void Passing_null_parameters_to_non_generic_database_query_method_throws()
        {
            var database = new Database(new Mock<InternalContextForMock>().Object);

            Assert.Equal("parameters", Assert.Throws<ArgumentNullException>(() => database.SqlQuery(typeof(Random), "query", null)).ParamName);
        }

        [Fact]
        public void Passing_null_type_to_non_generic_database_query_method_throws()
        {
            var database = new Database(new Mock<InternalContextForMock>().Object);

            Assert.Equal("elementType", Assert.Throws<ArgumentNullException>(() => database.SqlQuery(null, "query")).ParamName);
        }

        [Fact]
        public void Passing_null_SQL_to_command_method_throws()
        {
            var database = new Database(new Mock<InternalContextForMock>().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("sql"), Assert.Throws<ArgumentException>(() => database.ExecuteSqlCommand(null)).Message);
        }

        [Fact]
        public void Passing_empty_SQL_to_command_method_throws()
        {
            var database = new Database(new Mock<InternalContextForMock>().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("sql"), Assert.Throws<ArgumentException>(() => database.ExecuteSqlCommand("")).Message);
        }

        [Fact]
        public void Passing_whitespace_SQL_to_command_method_throws()
        {
            var database = new Database(new Mock<InternalContextForMock>().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("sql"), Assert.Throws<ArgumentException>(() => database.ExecuteSqlCommand(" ")).Message);
        }

        [Fact]
        public void Passing_null_parameters_to_command_method_throws()
        {
            var database = new Database(new Mock<InternalContextForMock>().Object);

            Assert.Equal("parameters", Assert.Throws<ArgumentNullException>(() => database.ExecuteSqlCommand("query", null)).ParamName);
        }

        #endregion

        #region DbSqlQuery as IListSource tests

        [Fact]
        public void Non_entity_SQL_query_ContainsListCollection_returns_false()
        {
            var query = new DbSqlQuery<FakeEntity>(CreateInternalNonSetQuery("query"));

            Assert.False(((IListSource)query).ContainsListCollection);
        }

        [Fact]
        public void Non_generic_non_entity_SQL_query_ContainsListCollection_returns_false()
        {
            var query = CreateInternalNonSetQuery("query");

            Assert.False(((IListSource)query).ContainsListCollection);
        }

        [Fact]
        public void Non_entity_SQL_query_GetList_throws_indicating_that_binding_to_queries_is_not_allowed()
        {
            var query = new DbSqlQuery<Random>(CreateInternalNonSetQuery("query"));

            Assert.Equal(Strings.DbQuery_BindingToDbQueryNotSupported, Assert.Throws<NotSupportedException>(() => ((IListSource)query).GetList()).Message);
        }

        [Fact]
        public void Non_generic_non_entity_SQL_query_GetList_throws_indicating_that_binding_to_queries_is_not_allowed()
        {
            var query = CreateInternalNonSetQuery("query");

            Assert.Equal(Strings.DbQuery_BindingToDbQueryNotSupported, Assert.Throws<NotSupportedException>(() => ((IListSource)query).GetList()).Message);
        }

        [Fact]
        public void DbSqlQuery_ContainsListCollection_returns_false()
        {
            var query = new DbSqlSetQuery<FakeEntity>(CreateInternalSetQuery("query"));

            Assert.False(((IListSource)query).ContainsListCollection);
        }

        [Fact]
        public void Non_generic_DbSqlQuery_ContainsListCollection_returns_false()
        {
            var query = new DbSqlSetQuery(CreateInternalSetQuery("query"));

            Assert.False(((IListSource)query).ContainsListCollection);
        }

        [Fact]
        public void DbSqlQuery_GetList_throws_indicating_that_binding_to_queries_is_not_allowed()
        {
            var query = new DbSqlSetQuery<FakeEntity>(CreateInternalSetQuery("query"));

            Assert.Equal(Strings.DbQuery_BindingToDbQueryNotSupported, Assert.Throws<NotSupportedException>(() => ((IListSource)query).GetList()).Message);
        }

        [Fact]
        public void Non_generic_DbSqlQuery_GetList_throws_indicating_that_binding_to_queries_is_not_allowed()
        {
            var query = new DbSqlSetQuery(CreateInternalSetQuery("query"));

            Assert.Equal(Strings.DbQuery_BindingToDbQueryNotSupported, Assert.Throws<NotSupportedException>(() => ((IListSource)query).GetList()).Message);
        }

        #endregion
    }
}
