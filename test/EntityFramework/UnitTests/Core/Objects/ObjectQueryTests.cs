// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections.Generic;
    using System.Data.Entity.TestHelpers;
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

            var enumerator = objectQuery.GetEnumeratorInternal();

            shaperMock.Verify(m => m.GetEnumerator(), Times.Never());

            enumerator.MoveNext();

            shaperMock.Verify(m => m.GetEnumerator(), Times.Once());
        }
    }
}
