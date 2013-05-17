// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Linq.Expressions;
    using Moq;
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
            var query = MockHelper.CreateInternalSqlNonSetQuery(
                "select * from Products where Id < {0} and CategoryId = {1}", false, 4, "Beverages");

            Assert.Equal("select * from Products where Id < {0} and CategoryId = {1}", query.ToString());
        }

        [Fact]
        public void InternalSqlNonSetQuery_delegates_to_InternalSet_correctly_with_streaming()
        {
            InternalSqlNonSetQuery_delegates_to_InternalSet_correctly(true);
        }

        [Fact]
        public void InternalSqlNonSetQuery_delegates_to_InternalSet_correctly_with_buffering()
        {
            InternalSqlNonSetQuery_delegates_to_InternalSet_correctly(false);
        }

        private void InternalSqlNonSetQuery_delegates_to_InternalSet_correctly(bool streaming)
        {
            var parameters = new[] { "bar" };

#if !NET40
            VerifyMethod<string>(
                e => e.GetAsyncEnumerator(), m => m.ExecuteSqlQueryAsync(typeof(string), "foo", streaming, parameters),
                "foo", streaming, parameters);
#endif

            VerifyMethod<string>(
                e => e.GetEnumerator(), m => m.ExecuteSqlQuery(typeof(string), "foo", streaming, parameters),
                "foo", streaming, parameters);
        }

        internal void VerifyMethod<T>(
            Action<InternalSqlNonSetQuery> methodInvoke, Expression<Action<InternalContextForMock>> mockMethodInvoke,
            string sql, bool streaming, object[] parameters)
            where T : class
        {
            Assert.NotNull(methodInvoke);
            Assert.NotNull(mockMethodInvoke);

            var internalContextMock = new Mock<InternalContextForMock>();
            var sqlSetQuery = new InternalSqlNonSetQuery(internalContextMock.Object, typeof(T), sql, streaming, parameters);

            try
            {
                methodInvoke(sqlSetQuery);
            }
            catch (Exception)
            {
            }

            internalContextMock.Verify(mockMethodInvoke, Times.Once());
        }
    }
}
