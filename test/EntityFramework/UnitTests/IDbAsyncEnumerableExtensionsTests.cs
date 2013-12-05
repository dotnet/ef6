// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40

namespace System.Data.Entity
{
    using System.Data.Entity.Infrastructure;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;

    public class IDbAsyncEnumerableExtensionsTests
    {
        [Fact]
        public void ForEachAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerable = new Mock<IDbAsyncEnumerable>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));
            
            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .ForEachAsync(o => { }, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void ForEachAsync_checks_cancellation_token_when_enumerating_results()
        {
            var tokenSource = new CancellationTokenSource();
            var taskCancelled = false;

            var mockAsyncEnumerator = new Mock<IDbAsyncEnumerator>();
            mockAsyncEnumerator
                .Setup(e => e.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    (CancellationToken token) =>
                    {
                        Assert.False(taskCancelled);
                        tokenSource.Cancel();
                        taskCancelled = true;
                        return Task.FromResult(true);
                    });

            var mockAsyncEnumerable = new Mock<IDbAsyncEnumerable>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Returns(mockAsyncEnumerator.Object);

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                .ForEachAsync(o => { }, tokenSource.Token)
                .GetAwaiter().GetResult());
        }

        [Fact]
        public void ForEachAsync_T_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerator = new Mock<IDbAsyncEnumerator<int>>();
            mockAsyncEnumerator
                .Setup(e => e.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            var mockAsyncEnumerable = new Mock<IDbAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Returns(mockAsyncEnumerator.Object);

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .ForEachAsync(o => { }, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void ForEachAsync_T_checks_cancellation_token_when_enumerating_results()
        {
            var tokenSource = new CancellationTokenSource();
            var taskCancelled = false;

            var mockAsyncEnumerator = new Mock<IDbAsyncEnumerator<int>>();
            mockAsyncEnumerator
                .Setup(e => e.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    (CancellationToken token) =>
                    {
                        Assert.False(taskCancelled);
                        tokenSource.Cancel();
                        taskCancelled = true;
                        return Task.FromResult(true);
                    });               

            var mockAsyncEnumerable = new Mock<IDbAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Returns(mockAsyncEnumerator.Object);

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                .ForEachAsync(o => { }, tokenSource.Token)
                .GetAwaiter().GetResult());
        }
    }
}

#endif