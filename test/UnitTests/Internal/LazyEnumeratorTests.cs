// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    using Moq;
    using Xunit;

    public class LazyEnumeratorTests
    {
        [Fact]
        public void Constructor_doesnt_run_initializer()
        {
            var initialized = false;
            var _ = new LazyEnumerator<object>(
                () =>
                {
                    initialized = true;
                    return null;
                });

            Assert.False(initialized);
        }

        [Fact]
        public void Current_doesnt_run_initializer()
        {
            var initialized = false;
            var enumerator = new LazyEnumerator<object>(
                () =>
                {
                    initialized = true;
                    return new Mock<ObjectResult<object>>().Object;
                });

            var _ = enumerator.Current;

            Assert.False(initialized);
        }

        [Fact]
        public void Reset_doesnt_run_initializer()
        {
            var initialized = false;
            var enumerator = new LazyEnumerator<object>(
                () =>
                {
                    initialized = true;
                    return new Mock<ObjectResult<object>>().Object;
                });

            enumerator.Reset();

            Assert.False(initialized);
        }

        [Fact]
        public void MoveNext_runs_initializer()
        {
            var mockShaper = Core.Objects.MockHelper.CreateShaperMock<int>();
            mockShaper.Setup(m => m.GetEnumerator()).Returns(
                () => new DbEnumeratorShim<int>(Enumerable.Range(1, 1).GetEnumerator()));
            var mockObjectResult = new Mock<ObjectResult<int>>(mockShaper.Object, null, null) 
                { CallBase = true };

            var initialized = false;
            var enumerator = new LazyEnumerator<int>(
                () =>
                {
                    initialized = true;
                    return mockObjectResult.Object;
                });

            Assert.True(enumerator.MoveNext());

            Assert.True(initialized);
            Assert.Equal(1, enumerator.Current);
        }
    }
}
