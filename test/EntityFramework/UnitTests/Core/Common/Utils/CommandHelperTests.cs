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

#endif
    }
}
