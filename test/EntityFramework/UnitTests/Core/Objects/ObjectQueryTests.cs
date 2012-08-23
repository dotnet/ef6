// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using Moq;
    using Xunit;

    public class ObjectQueryTests
    {
        [Fact]
        public void GetEnumerator_calls_Shaper_GetEnumerator_lazily()
        {
            var shaperMock = MockHelper.CreateShaperMock<object>();
            shaperMock.Setup(m => m.GetEnumerator()).Returns(
                () =>
                new DbEnumeratorShim<object>(((IEnumerable<object>)new[] { new object() }).GetEnumerator()));
            var objectQuery = MockHelper.CreateMockObjectQuery(null, shaperMock.Object).Object;

            var enumerator = ((IEnumerable<object>)objectQuery).GetEnumerator();

            shaperMock.Verify(m => m.GetEnumerator(), Times.Never());

            enumerator.MoveNext();

            shaperMock.Verify(m => m.GetEnumerator(), Times.Once());
        }

#if !NET40

        [Fact]
        public void GetEnumeratorAsync_calls_Shaper_GetEnumerator_lazily()
        {
            var shaperMock = MockHelper.CreateShaperMock<object>();
            shaperMock.Setup(m => m.GetEnumerator()).Returns(
                () =>
                new DbEnumeratorShim<object>(((IEnumerable<object>)new[] { new object() }).GetEnumerator()));
            var objectQuery = MockHelper.CreateMockObjectQuery(null, shaperMock.Object).Object;

            var enumerator = ((IDbAsyncEnumerable<object>)objectQuery).GetAsyncEnumerator();

            shaperMock.Verify(m => m.GetEnumerator(), Times.Never());

            enumerator.MoveNextAsync().Wait();

            shaperMock.Verify(m => m.GetEnumerator(), Times.Once());
        }

#endif

        [Fact]
        public void Foreach_calls_generic_GetEnumerator()
        {
            var shaperMock = MockHelper.CreateShaperMock<string>();
            shaperMock.Setup(m => m.GetEnumerator()).Returns(
                () =>
                new DbEnumeratorShim<string>(((IEnumerable<string>)new[] { "foo" }).GetEnumerator()));
            var objectQuery = MockHelper.CreateMockObjectQuery(null, shaperMock.Object).Object;

            foreach(var element in objectQuery)
            {
                Assert.True(element.StartsWith("foo")); 
            }
        }
    }
}
