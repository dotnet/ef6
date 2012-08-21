// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Internal.Linq;
    using System.Linq.Expressions;
    using Moq;
    using Xunit;

    public class InternalSqlSetQueryTests
    {
        [Fact]
        public void InternalSqlSetQuery_delegates_to_InternalSet_correctly()
        {
            var isNoTracking = true;
            var parameters = new[] { "bar" };
            VerifyMethod<string>(e => e.GetAsyncEnumerator(), m => m.ExecuteSqlQueryAsync("foo", isNoTracking, parameters),
                "foo", isNoTracking, parameters);
            VerifyMethod<string>(e => e.GetEnumerator(), m => m.ExecuteSqlQuery("foo", isNoTracking, parameters),
                "foo", isNoTracking, parameters);
        }

        internal void VerifyMethod<T>(Action<InternalSqlSetQuery> methodInvoke, Expression<Action<IInternalSet<T>>> mockMethodInvoke,
            string sql, bool isNoTracking, object[] parameters)
            where T : class
        {
            Assert.NotNull(methodInvoke);
            Assert.NotNull(mockMethodInvoke);

            var internalSetMock = new Mock<IInternalSet<T>>();
            var sqlSetQuery = new InternalSqlSetQuery(internalSetMock.Object, sql, isNoTracking, parameters);

            try
            {
                methodInvoke(sqlSetQuery);
            }
            catch (Exception)
            {
            }

            internalSetMock.Verify(mockMethodInvoke, Times.Once());
        }
    }
}
