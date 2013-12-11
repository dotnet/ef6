// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using Moq;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class ShapedBufferedDataRecordTests
    {
#if !NET40

        [Fact]
        public void InitializeAsync_throws_OperationCanceledException_if_task_is_cancelled()
        {
            Assert.Throws<OperationCanceledException>(
                () => ShapedBufferedDataRecord.InitializeAsync(
                    "manifestToken", new Mock<DbProviderServices>().Object,
                    new Mock<DbDataReader>().Object, new Type[0], new bool[0], new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void InitializeAsync_throws_OperationCanceledException_if_task_is_cancelled_during_initialization_when_reading_rows()
        {
            var tokenSource = new CancellationTokenSource();
            var taskCancelled = false;

            var mockDataReader = new Mock<DbDataReader>();
            mockDataReader
                .Setup(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    (CancellationToken token) =>
                    {
                        Assert.False(taskCancelled);
                        tokenSource.Cancel();
                        taskCancelled = true;
                        return Task.FromResult(true);
                    });

            Assert.Throws<OperationCanceledException>(
                () => ShapedBufferedDataRecord.InitializeAsync(
                    "manifestToken", new Mock<DbProviderServices>().Object,
                    mockDataReader.Object, new Type[0], new bool[0], tokenSource.Token)
                    .GetAwaiter().GetResult());            
        }

#endif 
    }
}
