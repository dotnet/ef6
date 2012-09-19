// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.ELinq
{
    using System;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Linq.Expressions;
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
        public void ExecuteAsync_nongeneric_throws_for_null_argument()
        {
            Assert.Throws<ArgumentNullException>(
                () => ((IDbAsyncQueryProvider)CreateObjectQueryProviderMock().Object).ExecuteAsync(null));
        }

        [Fact]
        public void ExecuteAsync_generic_throws_for_null_argument()
        {
            Assert.Throws<ArgumentNullException>(
                () => ((IDbAsyncQueryProvider)CreateObjectQueryProviderMock().Object).ExecuteAsync<object>(null));
        }

        [Fact]
        public void ExecuteAsync_nongeneric_calls_Single_by_default()
        {
            var createObjectQueryProviderMock = CreateObjectQueryProviderMock();

            var expectedResult = new object();

            createObjectQueryProviderMock.Setup(m => m.CreateQuery(It.IsAny<Expression>(), It.IsAny<Type>()))
                .Returns(MockHelper.CreateMockObjectQuery(expectedResult).Object);

            var result = createObjectQueryProviderMock.Object.ExecuteAsync(new Mock<Expression>().Object).Result;

            Assert.Same(expectedResult, result);
        }

        [Fact]
        public void ExecuteAsync_generic_calls_Single_by_default()
        {
            var createObjectQueryProviderMock = CreateObjectQueryProviderMock();

            var expectedResult = new object();

            createObjectQueryProviderMock.Setup(m => m.CreateQuery<object>(It.IsAny<Expression>()))
                .Returns(MockHelper.CreateMockObjectQuery(expectedResult).Object);

            var result = createObjectQueryProviderMock.Object.ExecuteAsync<object>(new Mock<Expression>().Object).Result;

            Assert.Same(expectedResult, result);
        }

#endif

        private Mock<ObjectQueryProvider> CreateObjectQueryProviderMock()
        {
            return new Mock<ObjectQueryProvider>(new ObjectContext()) { CallBase = true };
        }
    }
}
