// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections;
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Linq.Expressions;
    using Moq;
    using Xunit;

    public class ObjectResultTests
    {
        [Fact]
        public void GetEnumerator_methods_call_Shaper_GetEnumerator()
        {
            VerifyMethod(
                r => r.GetEnumerator(),
                m => m.GetEnumerator());
            VerifyMethod(
                r => ((IEnumerable)r).GetEnumerator(),
                m => m.GetEnumerator());
#if !NET40
            VerifyMethod(
                r => ((IDbAsyncEnumerable<object>)r).GetAsyncEnumerator(),
                m => m.GetEnumerator());
            VerifyMethod(
                r => ((IDbAsyncEnumerable)r).GetAsyncEnumerator(),
                m => m.GetEnumerator());
#endif
        }

        [Fact]
        public void GetEnumerator_throws_when_called_twice()
        {
            var objectResult = VerifyMethod(
                r => r.GetEnumerator(),
                m => m.GetEnumerator());

            Assert.Equal(
                Strings.Materializer_CannotReEnumerateQueryResults,
                Assert.Throws<InvalidOperationException>(() => ((IEnumerable)objectResult).GetEnumerator()).Message);
        }

        private ObjectResult VerifyMethod(Action<ObjectResult<object>> methodInvoke, Expression<Action<Shaper<object>>> mockMethodInvoke)
        {
            Assert.NotNull(methodInvoke);
            Assert.NotNull(mockMethodInvoke);

            var shaperMock = MockHelper.CreateShaperMock<object>();
            shaperMock.Setup(m => m.GetEnumerator()).Returns(new Mock<IDbEnumerator<object>>().Object);
            var objectResult = new Mock<ObjectResult<object>>(shaperMock.Object, null, null)
            {
                CallBase = true
            }.Object;

            methodInvoke(objectResult);

            shaperMock.Verify(mockMethodInvoke, Times.Once());

            return objectResult;
        }
    }
}
