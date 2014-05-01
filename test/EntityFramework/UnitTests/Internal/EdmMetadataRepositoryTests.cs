// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Core;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.SqlClient;
    using System.Linq;
    using Moq;
    using Xunit;

    public class EdmMetadataRepositoryTests : TestBase
    {
        [Fact]
        public void QueryForModelHash_uses_interception()
        {
            var dbConnectionInterceptorMock = new Mock<IDbConnectionInterceptor>();
            DbInterception.Add(dbConnectionInterceptorMock.Object);
            try
            {
                var repository = new EdmMetadataRepository("Database=Foo", SqlClientFactory.Instance);
                var mockContext = CreateMockContext("Hash");

                repository.QueryForModelHash(() => mockContext.Object);
            }
            finally
            {
                DbInterception.Remove(dbConnectionInterceptorMock.Object);
            }

            dbConnectionInterceptorMock.Verify(
                m => m.ConnectionStringGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                Times.Once());
            dbConnectionInterceptorMock.Verify(
                m => m.ConnectionStringGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                Times.Once());
        }

        [Fact]
        public void QueryForModelHash_returns_the_model_hash_if_it_exists()
        {
            var repository = new EdmMetadataRepository("Database=Foo", SqlClientFactory.Instance);
            var mockContext = CreateMockContext("Hash");

            Assert.Equal("Hash", repository.QueryForModelHash(() => mockContext.Object));
        }

        [Fact]
        public void QueryForModelHash_returns_the_last_model_hash_if_more_than_one_exists()
        {
            var repository = new EdmMetadataRepository("Database=Foo", SqlClientFactory.Instance);
            var mockContext = CreateMockContext("Hash1", "Hash2", "Hash3");

            Assert.Equal("Hash3", repository.QueryForModelHash(() => mockContext.Object));
        }

        [Fact]
        public void QueryForModelHash_returns_null_if_the_EdmMetadata_table_is_missing()
        {
            var repository = new EdmMetadataRepository("Database=Foo", SqlClientFactory.Instance);
            var mockContext = CreateMockContext("Hash");
            mockContext.Setup(m => m.Metadata).Throws(new EntityCommandExecutionException());

            Assert.Null(repository.QueryForModelHash(() => mockContext.Object));
        }

        [Fact]
        public void QueryForModelHash_returns_null_if_the_EdmMetadata_has_no_rows()
        {
            var repository = new EdmMetadataRepository("Database=Foo", SqlClientFactory.Instance);
            var mockContext = CreateMockContext();

            Assert.Null(repository.QueryForModelHash(() => mockContext.Object));
        }

        [Fact]
        public void QueryForModelHash_returns_null_if_the_EdmMetadata_has_row_with_null_model_hash()
        {
            var repository = new EdmMetadataRepository("Database=Foo", SqlClientFactory.Instance);
            var mockContext = CreateMockContext((string)null);

            Assert.Null(repository.QueryForModelHash(() => mockContext.Object));
        }

        private Mock<EdmMetadataContext> CreateMockContext(params string[] hashValues)
        {
            var mockContext = new Mock<EdmMetadataContext>(new Mock<DbConnection>().Object, true)
                                  {
                                      CallBase = true
                                  };
            mockContext.Setup(m => m.Metadata).Returns(CreateMockEdmMetadataSet(hashValues).Object);
            return mockContext;
        }

#pragma warning disable 612,618
        private Mock<IDbSet<EdmMetadata>> CreateMockEdmMetadataSet(params string[] hashValues)
        {
            var edmMetadata = hashValues.Select(
                (h, i) => new EdmMetadata
                              {
                                  Id = i,
                                  ModelHash = h
                              }).AsQueryable();
            var mockSet = new Mock<IDbSet<EdmMetadata>>();
            mockSet.Setup(m => m.ElementType).Returns(edmMetadata.ElementType);
            mockSet.Setup(m => m.Expression).Returns(edmMetadata.Expression);
            mockSet.Setup(m => m.Provider).Returns(edmMetadata.Provider);
            mockSet.Setup(m => m.GetEnumerator()).Returns(edmMetadata.GetEnumerator());
            return mockSet;
        }
#pragma warning restore 612,618
    }
}
