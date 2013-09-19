// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.Linq
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Linq.Expressions;
    using Moq;
    using Xunit;

    public class InternalDbQueryTests
    {
        [Fact]
        public void Methods_delegate_to_underlying_InternalQuery_correctly()
        {
#if !NET40
            VerifyMethod(
                q => q.GetAsyncEnumerator(),
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
            var dbQuery = new InternalDbQuery<string>(internalQueryMock.Object);
#if !NET40
            ((IDbAsyncEnumerable)dbQuery).GetAsyncEnumerator();

            nonGenericInternalQueryMock.Verify(m => m.GetAsyncEnumerator(), Times.Once());
#endif

            ((IEnumerable)dbQuery).GetEnumerator();

            nonGenericInternalQueryMock.Verify(m => m.GetEnumerator(), Times.Once());
        }

        private void VerifyGetter<TProperty>(
            Func<InternalDbQuery<string>, TProperty> getterFunc,
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
            var dbQuery = new InternalDbQuery<string>(internalQueryMock.Object);

            getterFunc(dbQuery);
            internalQueryMock.VerifyGet(mockGetterFunc, Times.Once());
        }

        private void VerifyMethod(Action<InternalDbQuery<string>> methodInvoke, Expression<Action<IInternalQuery<string>>> mockMethodInvoke)
        {
            Assert.NotNull(methodInvoke);
            Assert.NotNull(mockMethodInvoke);

            var internalQueryMock = new Mock<IInternalQuery<string>>();
            internalQueryMock.Setup(m => m.GetEnumerator()).Returns(new Mock<IEnumerator<string>>().Object);
            internalQueryMock.Setup(m => m.AsNoTracking()).Returns(internalQueryMock.Object);
            internalQueryMock.Setup(m => m.Include(It.IsAny<string>())).Returns(internalQueryMock.Object);
            var dbQuery = new InternalDbQuery<string>(internalQueryMock.Object);

            methodInvoke(dbQuery);

            internalQueryMock.Verify(mockMethodInvoke, Times.Once());
        }
    }
}
