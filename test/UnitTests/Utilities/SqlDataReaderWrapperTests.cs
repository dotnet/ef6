// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.Utilities
{
    using Moq;
    using System.Threading;
    using Xunit;

    public class SqlDataReaderWrapperTests
    {
#if !NET40

        [Fact]
        public void NextResultAsync_throws_OperationCanceledException_if_task_is_cancelled()
        {
            var sqlDataReaderWrapper = new Mock<SqlDataReaderWrapper>{ CallBase = true }.Object;

            Assert.Throws<OperationCanceledException>(
                () => sqlDataReaderWrapper.NextResultAsync(new CancellationToken(canceled: true))
                        .GetAwaiter().GetResult());
        }

        [Fact]
        public void ReadAsync_throws_OperationCanceledException_if_task_is_cancelled()
        {
            var sqlDataReaderWrapper = new Mock<SqlDataReaderWrapper> { CallBase = true }.Object;

            Assert.Throws<OperationCanceledException>(
                () => sqlDataReaderWrapper.ReadAsync(new CancellationToken(canceled: true))
                        .GetAwaiter().GetResult());
        }

#endif
    }
}
