// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.ELinq
{
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;

    public class ObjectQueryProviderTests
    {
        [Fact]
        public void CreateQuery_nongeneric_throws_for_null_argument()
        {
            Assert.Throws<ArgumentNullException>(
                () => ((IQueryProvider)CreateObjectQueryProviderMock().Object).CreateQuery(null));
        }

        [Fact]
        public void CreateQuery_generic_throws_for_null_argument()
        {
            Assert.Throws<ArgumentNullException>(
                () => ((IQueryProvider)CreateObjectQueryProviderMock().Object).CreateQuery<object>(null));
        }

        [Fact]
        public void Execute_nongeneric_throws_for_null_argument()
        {
            Assert.Throws<ArgumentNullException>(
                () => ((IQueryProvider)CreateObjectQueryProviderMock().Object).Execute(null));
        }

        [Fact]
        public void Execute_generic_throws_for_null_argument()
        {
            Assert.Throws<ArgumentNullException>(
                () => ((IQueryProvider)CreateObjectQueryProviderMock().Object).Execute<object>(null));
        }

        [Fact]
        public void Execute_nongeneric_calls_Single_by_default()
        {
            var createObjectQueryProviderMock = CreateObjectQueryProviderMock();

            var expectedResult = new object();

            createObjectQueryProviderMock.Setup(m => m.CreateQuery(It.IsAny<Expression>(), It.IsAny<Type>()))
                                         .Returns(MockHelper.CreateMockObjectQuery(expectedResult).Object);

            var result = ((IQueryProvider)createObjectQueryProviderMock.Object).Execute(new Mock<Expression>().Object);

            Assert.Same(expectedResult, result);
        }

        [Fact]
        public void Execute_generic_calls_Single_by_default()
        {
            var createObjectQueryProviderMock = CreateObjectQueryProviderMock();

            var expectedResult = new object();

            createObjectQueryProviderMock.Setup(m => m.CreateQuery<object>(It.IsAny<Expression>()))
                                         .Returns(MockHelper.CreateMockObjectQuery(expectedResult).Object);

            var result = ((IQueryProvider)createObjectQueryProviderMock.Object).Execute<object>(new Mock<Expression>().Object);

            Assert.Same(expectedResult, result);
        }

#if !NET40

        [Fact]
        public Task ExecuteAsync_nongeneric_throws_for_null_argument()
        {
            return Assert.ThrowsAsync<ArgumentNullException>(
                () => ((IDbAsyncQueryProvider)CreateObjectQueryProviderMock().Object).ExecuteAsync(null, CancellationToken.None));
        }

        [Fact]
        public Task ExecuteAsync_generic_throws_for_null_argument()
        {
            return Assert.ThrowsAsync<ArgumentNullException>(
                () => ((IDbAsyncQueryProvider)CreateObjectQueryProviderMock().Object).ExecuteAsync<object>(null, CancellationToken.None));
        }

        [Fact]
        public void ExecuteAsync_nongeneric_calls_Single_by_default()
        {
            var createObjectQueryProviderMock = CreateObjectQueryProviderMock();

            var expectedResult = new object();

            createObjectQueryProviderMock.Setup(m => m.CreateQuery(It.IsAny<Expression>(), It.IsAny<Type>()))
                                         .Returns(MockHelper.CreateMockObjectQuery(expectedResult).Object);

            var result = ((IDbAsyncQueryProvider)createObjectQueryProviderMock.Object)
                .ExecuteAsync(new Mock<Expression>().Object, CancellationToken.None).Result;

            Assert.Same(expectedResult, result);
        }

        [Fact]
        public void ExecuteAsync_generic_calls_Single_by_default()
        {
            var createObjectQueryProviderMock = CreateObjectQueryProviderMock();

            var expectedResult = new object();

            createObjectQueryProviderMock.Setup(m => m.CreateQuery<object>(It.IsAny<Expression>()))
                                         .Returns(MockHelper.CreateMockObjectQuery(expectedResult).Object);

            var result = ((IDbAsyncQueryProvider)createObjectQueryProviderMock.Object)
                .ExecuteAsync<object>(new Mock<Expression>().Object, CancellationToken.None).Result;

            Assert.Same(expectedResult, result);
        }

        [Fact]
        public void ExecuteAsync_throws_OperationCanceledException_if_task_is_cancelled()
        {
            var createObjectQueryProviderMock = CreateObjectQueryProviderMock();

            Assert.Throws<OperationCanceledException>(
                () => ((IDbAsyncQueryProvider)createObjectQueryProviderMock.Object)
                    .ExecuteAsync<object>(new Mock<Expression>().Object, new CancellationToken(canceled: true)).Result);

            Assert.Throws<OperationCanceledException>(
                () => ((IDbAsyncQueryProvider)createObjectQueryProviderMock.Object)
                    .ExecuteAsync(new Mock<Expression>().Object, new CancellationToken(canceled: true)).Result);
        }

        [Fact]
        public void ExecuteSingleAsync_throws_OperationCanceledException_if_task_is_cancelled()
        {
            Assert.Throws<OperationCanceledException>(
                () => ObjectQueryProvider.ExecuteSingleAsync<object>(
                    new Mock<IDbAsyncEnumerable<object>>().Object, 
                    new Mock<Expression>().Object, 
                    new CancellationToken(canceled: true)).GetAwaiter().GetResult());
        }


#endif

        private Mock<ObjectQueryProvider> CreateObjectQueryProviderMock()
        {
            return new Mock<ObjectQueryProvider>(new ObjectContext())
                       {
                           CallBase = true
                       };
        }
    }
}
