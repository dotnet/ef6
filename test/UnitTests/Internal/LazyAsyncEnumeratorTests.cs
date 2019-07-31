// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


#if !NET40

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;

    public class LazyAsyncEnumeratorTests
    {
        [Fact]
        public void Constructor_doesnt_run_initializer()
        {
            var initialized = false;
            var _ = new LazyAsyncEnumerator<object>(
                ct =>
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
            var enumerator = new LazyAsyncEnumerator<object>(
                ct =>
                {
                    initialized = true;
                    return Task.FromResult(new Mock<ObjectResult<object>>().Object);
                });

            var _ = enumerator.Current;

            Assert.False(initialized);
        }

        [Fact]
        public void MoveNextAsync_runs_initializer()
        {
            var mockShaper = Core.Objects.MockHelper.CreateShaperMock<int>();
            mockShaper
                .Setup(m => m.GetEnumerator())
                .Returns(() => new DbEnumeratorShim<int>(Enumerable.Range(1, 1).GetEnumerator()));

            var mockObjectResult = 
                new Mock<ObjectResult<int>>(mockShaper.Object, null, null)
                {
                    CallBase = true
                };

            var initialized = false;
            var enumerator = new LazyAsyncEnumerator<int>(
                ct =>
                {
                    initialized = true;
                    return Task.FromResult(mockObjectResult.Object);
                });

            Assert.True(enumerator.MoveNextAsync(CancellationToken.None).Result);

            Assert.True(initialized);
            Assert.Equal(1, enumerator.Current);
        }

        [Fact]
        public void MoveNextAsync_passes_users_cancellationToken()
        {
            var cancellationToken = new CancellationTokenSource().Token;

            var mockEnumerator = 
                new Mock<DbEnumeratorShim<int>>(Enumerable.Empty<int>().GetEnumerator());

            mockEnumerator
                .As<IDbAsyncEnumerator<int>>()
                .Setup(e => e.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns((CancellationToken token) => Task.FromResult(false));

            var mockShaper = Core.Objects.MockHelper.CreateShaperMock<int>();

            mockShaper
                .Setup(m => m.GetEnumerator())
                .Returns(() => mockEnumerator.Object);

            var mockObjectResult = 
                new Mock<ObjectResult<int>>(mockShaper.Object, null, null)
                {
                    CallBase = true
                };

            var lazyEnumerator =
                new LazyAsyncEnumerator<int>(
                    token =>
                    {
                        Assert.Equal(cancellationToken, token);
                        return Task.FromResult(mockObjectResult.Object);
                    });

            Assert.False(
                lazyEnumerator
                    .MoveNextAsync(cancellationToken)
                    .GetAwaiter()
                    .GetResult());
            
            mockEnumerator
                .As<IDbAsyncEnumerator<int>>()
                .Verify(e => e.MoveNextAsync(cancellationToken), Times.Once());

        }

        [Fact]
        public void MoveNextAsync_throws_OperationCanceledException_if_task_is_cancelled()
        {
            Assert.Throws<OperationCanceledException>(
                () => new LazyAsyncEnumerator<int>(t => null).MoveNextAsync(new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }
    }
}

#endif
