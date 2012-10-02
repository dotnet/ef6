// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.Linq;
    using System.Linq;
    using System.Linq.Expressions;
    using Moq;
    using Xunit;
    using MockHelper = System.Data.Entity.Core.Objects.MockHelper;

    public class DbQueryTests : TestBase
    {
        [Fact]
        public void Methods_delegate_to_underlying_InternalQuery_correctly()
        {
#if !NET40
            VerifyMethod(
                q => ((IDbAsyncEnumerable)q).GetAsyncEnumerator(),
                m => m.GetAsyncEnumerator());
            VerifyMethod(
                q => ((IDbAsyncEnumerable<string>)q).GetAsyncEnumerator(),
                m => m.GetAsyncEnumerator());
#endif
            VerifyMethod(
                q => ((IEnumerable<string>)q).GetEnumerator(),
                m => m.GetEnumerator());
            VerifyMethod(
                q => q.AsNoTracking(),
                m => m.AsNoTracking());
            VerifyMethod(
                q => q.Include("a"),
                m => m.Include("a"));
        }

        [Fact]
        public void Properties_delegate_to_underlying_InternalQuery_correctly()
        {
            VerifyGetter(
                q => ((IQueryable)q).ElementType,
                m => m.ElementType);
            VerifyGetter(
                q => ((IQueryable)q).Expression,
                m => m.Expression);
            VerifyGetter(
                q => ((IQueryable)q).Provider,
                m => m.ObjectQueryProvider);
        }

        [Fact]
        public void NonGeneric_methods_delegate_to_underlying_InternalQuery_correctly()
        {
            var internalQueryMock = new Mock<IInternalQuery<string>>();
            var nonGenericInternalQueryMock = internalQueryMock.As<IInternalQuery>();
            nonGenericInternalQueryMock.Setup(m => m.GetEnumerator()).Returns(new Mock<IEnumerator>().Object);
            var dbQuery = new DbQuery<string>(internalQueryMock.Object);
#if !NET40
            ((IDbAsyncEnumerable)(DbQuery)dbQuery).GetAsyncEnumerator();

            nonGenericInternalQueryMock.Verify(m => m.GetAsyncEnumerator(), Times.Once());
#endif

            ((IEnumerable)(DbQuery)dbQuery).GetEnumerator();

            nonGenericInternalQueryMock.Verify(m => m.GetEnumerator(), Times.Once());
        }

        private void VerifyGetter<TProperty>(
            Func<DbQuery<string>, TProperty> getterFunc,
            Expression<Func<IInternalQuery<string>, TProperty>> mockGetterFunc)
        {
            Assert.NotNull(getterFunc);
            Assert.NotNull(mockGetterFunc);

            var internalQueryMock = new Mock<IInternalQuery<string>>();
            internalQueryMock.Setup(m => m.ElementType).Returns(typeof(string));
            internalQueryMock.Setup(m => m.Expression).Returns(Expression.Constant(new object()));
            internalQueryMock.Setup(m => m.InternalContext).Returns(new Mock<InternalContextForMock<DbContext>>().Object);
            internalQueryMock.Setup(m => m.ObjectQueryProvider).Returns(
                new ObjectQueryProvider(MockHelper.CreateMockObjectContext<string>()));
            var dbQuery = new DbQuery<string>(internalQueryMock.Object);

            getterFunc(dbQuery);
            internalQueryMock.VerifyGet(mockGetterFunc, Times.Once());
        }

        private void VerifyMethod(Action<DbQuery<string>> methodInvoke, Expression<Action<IInternalQuery<string>>> mockMethodInvoke)
        {
            Assert.NotNull(methodInvoke);
            Assert.NotNull(mockMethodInvoke);

            var internalQueryMock = new Mock<IInternalQuery<string>>();
            internalQueryMock.Setup(m => m.GetEnumerator()).Returns(new Mock<IEnumerator<string>>().Object);
            internalQueryMock.Setup(m => m.AsNoTracking()).Returns(internalQueryMock.Object);
            internalQueryMock.Setup(m => m.Include(It.IsAny<string>())).Returns(internalQueryMock.Object);
            var dbQuery = new DbQuery<string>(internalQueryMock.Object);

            methodInvoke(dbQuery);

            internalQueryMock.Verify(mockMethodInvoke, Times.Once());
        }
    }
}
