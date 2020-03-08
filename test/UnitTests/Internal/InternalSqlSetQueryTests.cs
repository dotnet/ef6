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
        public void InternalSqlSetQuery_delegates_to_InternalSet_correctly_with_noTracking_and_buffering()
        {
            InternalSqlSetQuery_delegates_to_InternalSet_correctly(true, false);
        }

        [Fact]
        public void InternalSqlSetQuery_delegates_to_InternalSet_correctly_with_tracking_and_streaming()
        {
            InternalSqlSetQuery_delegates_to_InternalSet_correctly(false, true);
        }

        private void InternalSqlSetQuery_delegates_to_InternalSet_correctly(bool isNoTracking, bool streaming)
        {
            var parameters = new[] { "bar" };

#if !NET40
            VerifyMethod<string>(
                e => e.GetAsyncEnumerator(), m => m.ExecuteSqlQueryAsync("foo", isNoTracking, streaming ? true: (bool?)null, parameters),
                "foo", isNoTracking, streaming, parameters);
#endif

            VerifyMethod<string>(
                e => e.GetEnumerator(), m => m.ExecuteSqlQuery("foo", isNoTracking, streaming ? true : (bool?)null, parameters),
                "foo", isNoTracking, streaming, parameters);
        }

        internal void VerifyMethod<T>(
            Action<InternalSqlSetQuery> methodInvoke, Expression<Action<IInternalSet<T>>> mockMethodInvoke,
            string sql, bool isNoTracking, bool streaming, object[] parameters)
            where T : class
        {
            Assert.NotNull(methodInvoke);
            Assert.NotNull(mockMethodInvoke);

            var internalSetMock = new Mock<IInternalSet<T>>();
            var sqlSetQuery = new InternalSqlSetQuery(internalSetMock.Object, sql, isNoTracking, parameters);
            if (streaming)
            {
                sqlSetQuery = (InternalSqlSetQuery)sqlSetQuery.AsStreaming();
            }

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
