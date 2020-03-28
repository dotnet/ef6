// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.Linq
{
    using System.Data.Entity.Infrastructure;
    using System.Linq.Expressions;
    using System.Threading;
    using Moq;
    using Xunit;

    public class DbQueryProviderTests
    {
#if !NET40

        [Fact]
        public void ExecuteAsync_throws_OperationCanceledException_if_task_is_cancelled()
        {
            var internalContext = new Mock<InternalContext>().Object;
            var queryProvider = new DbQueryProvider(internalContext, new Mock<InternalQuery<object>>(internalContext).Object);

            Assert.Throws<OperationCanceledException>(
                () => ((IDbAsyncQueryProvider)queryProvider)
                    .ExecuteAsync(new Mock<Expression>().Object, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());

            Assert.Throws<OperationCanceledException>(
                () => ((IDbAsyncQueryProvider)queryProvider)
                    .ExecuteAsync<object>(new Mock<Expression>().Object, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

#endif
    }
}
