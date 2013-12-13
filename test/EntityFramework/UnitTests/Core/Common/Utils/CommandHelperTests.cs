// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils
{
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;

    public class CommandHelperTests
    {
        [Fact]
        public void ConsumeReader()
        {
            var mockDbDataReader = new Mock<DbDataReader>();
            var expectedNumberOfResults = 3;
            var currentResult = 0;
            mockDbDataReader.Setup(m => m.NextResult()).Returns(
                () => currentResult++ < expectedNumberOfResults);

            CommandHelper.ConsumeReader(mockDbDataReader.Object);

            Assert.Equal(expectedNumberOfResults, currentResult - 1);
        }

#if !NET40

        [Fact]
        public void ConsumeReaderAsync()
        {
            var mockDbDataReader = new Mock<DbDataReader>();
            var expectedNumberOfResults = 3;
            var currentResult = 0;
            mockDbDataReader.Setup(m => m.NextResultAsync(It.IsAny<CancellationToken>())).Returns(
                () => Task.FromResult(currentResult++ < expectedNumberOfResults));

            CommandHelper.ConsumeReaderAsync(mockDbDataReader.Object, CancellationToken.None).Wait();

            Assert.Equal(expectedNumberOfResults, currentResult - 1);
        }

        [Fact]
        public void ConsumeReaderAsync_does_not_throw_OperationCanceledException_if_reader_null_or_closed_even_if_task_is_cancelled()
        {
            Assert.False(CommandHelper.ConsumeReaderAsync(null, new CancellationToken(canceled: true)).IsCanceled);

            var mockReader = new Mock<DbDataReader>();
            mockReader.Setup(r => r.IsClosed).Returns(true);

            Assert.False(CommandHelper.ConsumeReaderAsync(mockReader.Object, new CancellationToken(canceled: true)).IsCanceled);
        }

        [Fact]
        public void ConsumeReaderAsync_throws_OperationCanceledException_before_executing_command_if_task_is_cancelled()
        {
            var mockDbDataReader = new Mock<DbDataReader>();
            mockDbDataReader
                .Setup(m => m.IsClosed)
                .Returns(false);

            mockDbDataReader
                .Setup(m => m.NextResultAsync(It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            Assert.Throws<OperationCanceledException>(
                () => CommandHelper.ConsumeReaderAsync(mockDbDataReader.Object, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void ConsumeReaderAsync_checks_cancellation_token_when_enumerating_results()
        {
            var tokenSource = new CancellationTokenSource();
            var taskCancelled = false;

            var mockDbDataReader = new Mock<DbDataReader>();
            mockDbDataReader
                .Setup(e => e.NextResultAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    (CancellationToken token) =>
                    {
                        Assert.False(taskCancelled);
                        tokenSource.Cancel();
                        taskCancelled = true;
                        return Task.FromResult(true);
                    });

            Assert.Throws<OperationCanceledException>(
                () => CommandHelper.ConsumeReaderAsync(mockDbDataReader.Object, tokenSource.Token)
                    .GetAwaiter().GetResult());
        }

#endif
    }
}
