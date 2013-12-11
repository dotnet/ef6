// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


#if !NET40

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
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
        public void Current_runs_initializer()
        {
            var initialized = false;
            var enumerator = new LazyAsyncEnumerator<object>(
                ct =>
                    {
                        initialized = true;
                        return Task.FromResult(new Mock<IDbAsyncEnumerator<object>>().Object);
                    });

            var _ = enumerator.Current;

            Assert.True(initialized);
        }

        [Fact]
        public void MoveNextAsync_runs_initializer()
        {
            var initialized = false;
            var enumerator = new LazyAsyncEnumerator<int>(
                ct =>
                    {
                        initialized = true;
                        return Task.FromResult(
                            (IDbAsyncEnumerator<int>)new DbEnumeratorShim<int>(
                                                         ((IEnumerable<int>)new[] { 1 }).GetEnumerator()));
                    });

            Assert.True(enumerator.MoveNextAsync(CancellationToken.None).Result);

            Assert.True(initialized);
            Assert.Equal(1, enumerator.Current);
        }

        [Fact]
        public void MoveNextAsync_passes_users_cancellationToken()
        {
            var cancellationToken = new CancellationTokenSource().Token;

            var mockAsyncEnumerator = new Mock<IDbAsyncEnumerator<int>>();
            mockAsyncEnumerator
                .Setup(e => e.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    (CancellationToken token) =>
                    {
                        Assert.Equal(cancellationToken, token);
                        return Task.FromResult(false);
                    });

            var lazyEnumerator = 
                new LazyAsyncEnumerator<int>(
                    token =>
                    {
                        Assert.Equal(cancellationToken, token);
                        return Task.FromResult(mockAsyncEnumerator.Object);
                    });

            lazyEnumerator
                .MoveNextAsync(cancellationToken)
                .GetAwaiter()
                .GetResult();
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
