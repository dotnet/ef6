// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.Linq
{
    using System.Collections;
    using System.Linq.Expressions;
    using System.Threading;
    using Moq;
    using Xunit;

    public class InternalDbSetTests
    {
        [Fact]
        public void InternalDbSet_delegates_to_InternalSet_correctly()
        {
            VerifyMethod<string>(e => e.Add("foo"), m => m.Add("foo"));
            VerifyMethod<string>(e => e.AsNoTracking(), m => m.AsNoTracking());
            VerifyMethod<string>(e => e.Attach("foo"), m => m.Attach("foo"));
            VerifyMethod<string>(e => e.Create(), m => m.Create());
            VerifyGetter<string, Type>(e => e.ElementType, m => m.ElementType);
            var key = 1;
            VerifyMethod<string>(e => e.Find(key), m => m.Find(key));
            VerifyMethod<string>(e => e.FindAsync(key), m => m.FindAsync(CancellationToken.None, key));
            var cancellationTokenSource = new CancellationTokenSource();
            VerifyMethod<string>(e => e.FindAsync(cancellationTokenSource.Token, key), m => m.FindAsync(cancellationTokenSource.Token, key));
            VerifyMethod<string>(e => e.GetAsyncEnumerator(), m => m.GetAsyncEnumerator());
            VerifyMethod<string>(e => e.GetEnumerator(), m => m.GetEnumerator());
            VerifyMethod<string>(e => e.Include("foo"), m => m.Include("foo"));
            VerifyGetter<string, IList>(e => e.Local, m => m.Local);
            VerifyMethod<string>(e => e.Remove("foo"), m => m.Remove("foo"));
        }

        [Fact]
        public void Cast_calls_InternalContext_Set()
        {
            var internalSetMock = new Mock<IInternalSet<string>>();
            var internalContextMock = new Mock<InternalContextForMock>();
            internalSetMock.Setup(m => m.InternalContext).Returns(internalContextMock.Object);
            internalSetMock.Setup(m => m.ElementType).Returns(typeof(object));

            var dbSet = new InternalDbSet<string>(internalSetMock.Object);

            dbSet.Cast<object>();
            
            internalContextMock.Verify(m => m.Set<object>(), Times.Once());
        }

        [Fact]
        public void SqlQuery_passes_through_the_sql_command()
        {
            var internalSetMock = new Mock<IInternalSet<string>>();
            var dbSet = new InternalDbSet<string>(internalSetMock.Object);
            
            var sql = "foo";

            var query = dbSet.SqlQuery(sql);

            Assert.Equal(sql, query.InternalQuery.Sql);
        }

        #region Helpers

        internal void VerifyGetter<T, TProperty>(
            Func<InternalDbSet<T>, TProperty> getterFunc,
            Expression<Func<IInternalSet<T>, TProperty>> mockGetterFunc)
            where T : class
        {
            Assert.NotNull(getterFunc);
            Assert.NotNull(mockGetterFunc);

            var internalSetMock = new Mock<IInternalSet<T>>();
            var dbSet = new InternalDbSet<T>(internalSetMock.Object);

            try
            {
                getterFunc(dbSet);
            }
            catch (Exception)
            {
            }

            internalSetMock.Verify(mockGetterFunc, Times.Once());
        }

        internal void VerifyMethod<T>(Action<InternalDbSet<T>> methodInvoke, Expression<Action<IInternalSet<T>>> mockMethodInvoke)
            where T : class
        {
            Assert.NotNull(methodInvoke);
            Assert.NotNull(mockMethodInvoke);

            var internalSetMock = new Mock<IInternalSet<T>>();
            var dbSet = new InternalDbSet<T>(internalSetMock.Object);

            try
            {
                methodInvoke(dbSet);
            }
            catch (Exception)
            {
            }

            internalSetMock.Verify(mockMethodInvoke, Times.Once());
        }

        #endregion
    }
}
