namespace System.Data.Entity.Internal
{
    using System.ComponentModel;
    using System.Data.Entity.Resources;
    using Xunit;

    public class InternalSqlNonSetQueryTests
    {
        [Fact]
        public void ToString_returns_the_query()
        {
            var query = MockHelper.CreateInternalSqlNonSetQuery("select * from products");

            Assert.Equal("select * from products", query.ToString());
        }

        [Fact]
        public void ToString_returns_the_query_but_not_the_parameters()
        {
            var query = MockHelper.CreateInternalSqlNonSetQuery("select * from Products where Id < {0} and CategoryId = {1}", 4, "Beverages");

            Assert.Equal("select * from Products where Id < {0} and CategoryId = {1}", query.ToString());
        }

        [Fact]
        public void Non_generic_non_entity_SQL_query_ContainsListCollection_returns_false()
        {
            var query = MockHelper.CreateInternalSqlNonSetQuery("query");

            Assert.False(((IListSource)query).ContainsListCollection);
        }

        [Fact]
        public void Non_generic_non_entity_SQL_query_GetList_throws_indicating_that_binding_to_queries_is_not_allowed()
        {
            var query = MockHelper.CreateInternalSqlNonSetQuery("query");

            Assert.Equal(Strings.DbQuery_BindingToDbQueryNotSupported, Assert.Throws<NotSupportedException>(() => ((IListSource)query).GetList()).Message);
        }
    }
}
