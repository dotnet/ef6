// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40

namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;

    public class IDbAsyncEnumerableExtensionsTests
    {
        [Fact]
        public void Non_generic_ForEachAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
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
        public void Non_generic_ForEachAsync_checks_cancellation_token_when_enumerating_results()
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
        public void Generic_ForEachAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
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
        public void Generic_ForEachAsync_checks_cancellation_token_when_enumerating_results()
        {
            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.ForEachAsync(o => { }, cancellationToken));
        }

        [Fact]
        public void Non_generic_ToListAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerable = new Mock<IDbAsyncEnumerable>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .ToListAsync<int>(new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void Generic_ToListAsync_throws_TaskCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerator = new Mock<IDbAsyncEnumerator<int>>();
            mockAsyncEnumerator
                .Setup(e => e.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            var mockAsyncEnumerable = new Mock<IDbAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Returns(mockAsyncEnumerator.Object);

            Assert.Throws<TaskCanceledException>(
                () => mockAsyncEnumerable.Object
                    .ToListAsync(new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void ToArrayAsync_throws_TaskCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerator = new Mock<IDbAsyncEnumerator<int>>();
            mockAsyncEnumerator
                .Setup(e => e.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            var mockAsyncEnumerable = new Mock<IDbAsyncEnumerable<int>>();

            Assert.Throws<TaskCanceledException>(
                () => mockAsyncEnumerable.Object
                    .ToArrayAsync(new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void ToDictionaryAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
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
                    .ToDictionaryAsync(n => n, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .ToDictionaryAsync(n => n, n => n, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());

            var equalityComparer = new Mock<IEqualityComparer<int>>().Object;

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .ToDictionaryAsync(n => n, equalityComparer, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .ToDictionaryAsync(n => n, n => n, equalityComparer, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void FirstAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerable = new Mock<IDbAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .FirstAsync(new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .FirstAsync(n => true, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void FirstOrDefaultAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerable = new Mock<IDbAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .FirstOrDefaultAsync(new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .FirstOrDefaultAsync(n => true, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void SingleAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerable = new Mock<IDbAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .SingleAsync(new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .SingleAsync(n => true, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void SingleAsync_checks_cancellation_token_when_enumerating_results()
        {
            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.SingleAsync(cancellationToken));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.SingleAsync(n => true, cancellationToken));
        }

        [Fact]
        public void SingleOrDefaultAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerable = new Mock<IDbAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .SingleOrDefaultAsync(new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .SingleOrDefaultAsync(n => true, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void SingleOrDefaultAsync_checks_cancellation_token_when_enumerating_results()
        {
            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.SingleOrDefaultAsync(cancellationToken));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.SingleOrDefaultAsync(n => true, cancellationToken));
        }

        [Fact]
        public void ContainsAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerable = new Mock<IDbAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .ContainsAsync(42, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void ContainsAsync_checks_cancellation_token_when_enumerating_results()
        {
            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.ContainsAsync(42, cancellationToken));
        }

        [Fact]
        public void AnyAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerable = new Mock<IDbAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .AnyAsync(new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .AnyAsync(n => true, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void AnyAsync_checks_cancellation_token_when_enumerating_results()
        {
            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.AnyAsync(n => false, cancellationToken));
        }

        [Fact]
        public void AllAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerable = new Mock<IDbAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .AnyAsync(n => true, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }


        [Fact]
        public void AllAsync_checks_cancellation_token_when_enumerating_results()
        {
            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.AllAsync(n => false, cancellationToken));
        }

        [Fact]
        public void CountAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerable = new Mock<IDbAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .CountAsync(new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .CountAsync(n => true, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void CountAsync_checks_cancellation_token_when_enumerating_results()
        {
            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.CountAsync(cancellationToken));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.CountAsync(n => true, cancellationToken));

        }

        [Fact]
        public void LongCountAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerable = new Mock<IDbAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .LongCountAsync(new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .LongCountAsync(n => true, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void LongCountAsync_checks_cancellation_token_when_enumerating_results()
        {
            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.LongCountAsync(cancellationToken));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.LongCountAsync(n => true, cancellationToken));

        }

        [Fact]
        public void MinAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerable = new Mock<IDbAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .MinAsync(new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void MinAsync_checks_cancellation_token_when_enumerating_results()
        {
            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.MinAsync(cancellationToken));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<string>(
                (source, cancellationToken) => source.MinAsync(cancellationToken));
        }

        [Fact]
        public void MaxAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerable = new Mock<IDbAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .MaxAsync(new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void MaxAsync_checks_cancellation_token_when_enumerating_results()
        {
            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.MaxAsync(cancellationToken));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<string>(
                (source, cancellationToken) => source.MaxAsync(cancellationToken));
        }

        [Fact]
        public void SumAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            ArithmeticAsyncMethod_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled<int>("SumAsync");
            ArithmeticAsyncMethod_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled<int?>("SumAsync");
            ArithmeticAsyncMethod_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled<long>("SumAsync");
            ArithmeticAsyncMethod_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled<long?>("SumAsync");
            ArithmeticAsyncMethod_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled<float>("SumAsync");
            ArithmeticAsyncMethod_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled<float?>("SumAsync");
            ArithmeticAsyncMethod_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled<double>("SumAsync");
            ArithmeticAsyncMethod_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled<double?>("SumAsync");
            ArithmeticAsyncMethod_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled<decimal>("SumAsync");
            ArithmeticAsyncMethod_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled<decimal?>("SumAsync");
        }

        [Fact]
        public void SumAsync_checks_cancellation_token_when_enumerating_results()
        {
            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => (Task)GetAsyncMethod<int>("SumAsync")
                    .Invoke(null, new object[] { source, cancellationToken }));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<int?>(
                (source, cancellationToken) => (Task)GetAsyncMethod<int?>("SumAsync")
                    .Invoke(null, new object[] { source, cancellationToken }));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<long>(
                (source, cancellationToken) => (Task)GetAsyncMethod<long>("SumAsync")
                    .Invoke(null, new object[] { source, cancellationToken }));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<long?>(
                (source, cancellationToken) => (Task)GetAsyncMethod<long?>("SumAsync")
                    .Invoke(null, new object[] { source, cancellationToken }));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<float>(
                (source, cancellationToken) => (Task)GetAsyncMethod<float>("SumAsync")
                    .Invoke(null, new object[] { source, cancellationToken }));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<float?>(
                (source, cancellationToken) => (Task)GetAsyncMethod<float?>("SumAsync")
                    .Invoke(null, new object[] { source, cancellationToken }));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<double>(
                (source, cancellationToken) => (Task)GetAsyncMethod<double>("SumAsync")
                    .Invoke(null, new object[] { source, cancellationToken }));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<double?>(
                (source, cancellationToken) => (Task)GetAsyncMethod<double?>("SumAsync")
                    .Invoke(null, new object[] { source, cancellationToken }));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<decimal>(
                (source, cancellationToken) => (Task)GetAsyncMethod<decimal>("SumAsync")
                    .Invoke(null, new object[] { source, cancellationToken }));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<decimal?>(
                (source, cancellationToken) => (Task)GetAsyncMethod<decimal?>("SumAsync")
                    .Invoke(null, new object[] { source, cancellationToken }));
        }

        [Fact]
        public void AverageAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            ArithmeticAsyncMethod_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled<int>("AverageAsync");
            ArithmeticAsyncMethod_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled<int?>("AverageAsync");
            ArithmeticAsyncMethod_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled<long>("AverageAsync");
            ArithmeticAsyncMethod_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled<long?>("AverageAsync");
            ArithmeticAsyncMethod_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled<float>("AverageAsync");
            ArithmeticAsyncMethod_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled<float?>("AverageAsync");
            ArithmeticAsyncMethod_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled<double>("AverageAsync");
            ArithmeticAsyncMethod_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled<double?>("AverageAsync");
            ArithmeticAsyncMethod_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled<decimal>("AverageAsync");
            ArithmeticAsyncMethod_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled<decimal?>("AverageAsync");
        }

        [Fact]
        public void AverageAsync_checks_cancellation_token_when_enumerating_results()
        {
            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => (Task)GetAsyncMethod<int>("AverageAsync")
                    .Invoke(null, new object[] { source, cancellationToken }));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<int?>(
                (source, cancellationToken) => (Task)GetAsyncMethod<int?>("AverageAsync")
                    .Invoke(null, new object[] { source, cancellationToken }));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<long>(
                (source, cancellationToken) => (Task)GetAsyncMethod<long>("AverageAsync")
                    .Invoke(null, new object[] { source, cancellationToken }));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<long?>(
                (source, cancellationToken) => (Task)GetAsyncMethod<long?>("AverageAsync")
                    .Invoke(null, new object[] { source, cancellationToken }));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<float>(
                (source, cancellationToken) => (Task)GetAsyncMethod<float>("AverageAsync")
                    .Invoke(null, new object[] { source, cancellationToken }));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<float?>(
                (source, cancellationToken) => (Task)GetAsyncMethod<float?>("AverageAsync")
                    .Invoke(null, new object[] { source, cancellationToken }));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<double>(
                (source, cancellationToken) => (Task)GetAsyncMethod<double>("AverageAsync")
                    .Invoke(null, new object[] { source, cancellationToken }));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<double?>(
                (source, cancellationToken) => (Task)GetAsyncMethod<double?>("AverageAsync")
                    .Invoke(null, new object[] { source, cancellationToken }));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<decimal>(
                (source, cancellationToken) => (Task)GetAsyncMethod<decimal>("AverageAsync")
                    .Invoke(null, new object[] { source, cancellationToken }));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<decimal?>(
                (source, cancellationToken) => (Task)GetAsyncMethod<decimal?>("AverageAsync")
                    .Invoke(null, new object[] { source, cancellationToken }));
        }

        private static void AsyncMethod_checks_for_cancellation_when_enumerating_results<T>(Func<IDbAsyncEnumerable<T>, CancellationToken, Task> asyncMethod)
        {
            var tokenSource = new CancellationTokenSource();
            var taskCancelled = false;

            var mockAsyncEnumerator = new Mock<IDbAsyncEnumerator<T>>();
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

            var mockAsyncEnumerable = new Mock<IDbAsyncEnumerable<T>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Returns(mockAsyncEnumerator.Object);

            Assert.Throws<OperationCanceledException>(
                () => asyncMethod(mockAsyncEnumerable.Object, tokenSource.Token)
                .GetAwaiter().GetResult());
        }

        private static void ArithmeticAsyncMethod_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled<T>(string methodName)
        {
            var mockAsyncEnumerable = new Mock<IDbAsyncEnumerable<T>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            var methodInfo = GetAsyncMethod<T>(methodName);

            Debug.Assert(methodName != null, "Invalid method name");

            Assert.Throws<OperationCanceledException>(
                () => ((Task)methodInfo.Invoke(null, new object[] { mockAsyncEnumerable.Object, new CancellationToken(canceled: true) }))
                    .GetAwaiter().GetResult());
        }

        private static MethodInfo GetAsyncMethod<T>(string methodName)
        {
            return typeof(IDbAsyncEnumerableExtensions)
                .GetMethod(
                    methodName, BindingFlags.Static | BindingFlags.NonPublic, null,
                    new[] { typeof(IDbAsyncEnumerable<T>), typeof(CancellationToken) }, null);
            
        }
    }
}

#endif