// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestDoubles
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Moq;
    using SimpleModel;
    using Xunit;

    public class MockableDbSetTests : FunctionalTestBase
    {
        [Fact]
        public void Moq_DbSet_can_be_used_for_Find()
        {
            var mockSet = new Mock<DbSet<Product>>();
            var product = new Product();
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns(product);

            Assert.Same(product, mockSet.Object.Find(1));
            mockSet.Verify(m => m.Find(1), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_Add()
        {
            var mockSet = new Mock<DbSet<Product>>();
            var product = new Product();
            mockSet.Setup(m => m.Add(product)).Returns(product);

            Assert.Same(product, mockSet.Object.Add(product));
            mockSet.Verify(m => m.Add(product), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_AddRange()
        {
            var mockSet = new Mock<DbSet<Product>>();
            var products = new[] { new Product { Id = 1 }, new Product { Id = 2 } };
            mockSet.Setup(m => m.AddRange(products)).Returns(products);

            Assert.Same(products, mockSet.Object.AddRange(products));
            mockSet.Verify(m => m.AddRange(products), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_Attach()
        {
            var mockSet = new Mock<DbSet<Product>>();
            var product = new Product();
            mockSet.Setup(m => m.Attach(product)).Returns(product);

            Assert.Same(product, mockSet.Object.Attach(product));
            mockSet.Verify(m => m.Attach(product), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_Remove()
        {
            var mockSet = new Mock<DbSet<Product>>();
            var product = new Product();
            mockSet.Setup(m => m.Remove(product)).Returns(product);

            Assert.Same(product, mockSet.Object.Remove(product));
            mockSet.Verify(m => m.Remove(product), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_RemoveRange()
        {
            var mockSet = new Mock<DbSet<Product>>();
            var products = new[] { new Product { Id = 1 }, new Product { Id = 2 } };
            mockSet.Setup(m => m.RemoveRange(products)).Returns(products);

            Assert.Same(products, mockSet.Object.RemoveRange(products));
            mockSet.Verify(m => m.RemoveRange(products), Times.Once());
        }

        public abstract class MockableDbSetWithExtensions<T> : DbSet<T>
            where T : class
        {
            public abstract void AddOrUpdate(params T[] entities);
            public abstract void AddOrUpdate(Expression<Func<T, object>> identifierExpression, params T[] entities);
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_AddOrUpdate()
        {
            var mockSet = new Mock<MockableDbSetWithExtensions<Product>>();
            DbSet<Product> dbSet = mockSet.Object;

            var products = new[] { new Product { Id = 1 }, new Product { Id = 2 } };
            dbSet.AddOrUpdate(products);

            mockSet.Verify(m => m.AddOrUpdate(products), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_AddOrUpdate_with_expression()
        {
            var mockSet = new Mock<MockableDbSetWithExtensions<Product>>();
            DbSet<Product> dbSet = mockSet.Object;

            var products = new[] { new Product { Id = 1 }, new Product { Id = 2 } };
            Expression<Func<Product, object>> expression = e => e.Id;
            dbSet.AddOrUpdate(expression, products);

            mockSet.Verify(m => m.AddOrUpdate(expression, products), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_SqlQuery()
        {
            var mockSqlQuery = new Mock<DbSqlQuery<Product>>();
            var products = new[] { new Product { Id = 1 }, new Product { Id = 2 } };
            mockSqlQuery.Setup(m => m.GetEnumerator()).Returns(((IEnumerable<Product>)products).GetEnumerator());

            var mockSet = new Mock<DbSet<Product>>();
            var query = "not a real query";
            var parameters = new object[] { 1, 2 };
            mockSet.Setup(m => m.SqlQuery(query, parameters)).Returns(mockSqlQuery.Object);

            Assert.Equal(new[] { 1, 2 }, mockSet.Object.SqlQuery(query, parameters).Select(p => p.Id));
            mockSet.Verify(m => m.SqlQuery(query, parameters), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_SqlQuery_with_AsNoTracking()
        {
            var mockSqlQuery = new Mock<DbSqlQuery<Product>>();
            mockSqlQuery.Setup(m => m.AsNoTracking()).Returns(mockSqlQuery.Object);
            var products = new[] { new Product { Id = 1 }, new Product { Id = 2 } };
            mockSqlQuery.Setup(m => m.GetEnumerator()).Returns(((IEnumerable<Product>)products).GetEnumerator());

            var mockSet = new Mock<DbSet<Product>>();
            var query = "not a real query";
            var parameters = new object[] { 1, 2 };
            mockSet.Setup(m => m.SqlQuery(query, parameters)).Returns(mockSqlQuery.Object);

            Assert.Equal(new[] { 1, 2 }, mockSet.Object.SqlQuery(query, parameters).AsNoTracking().Select(p => p.Id));
            mockSet.Verify(m => m.SqlQuery(query, parameters), Times.Once());
            mockSqlQuery.Verify(m => m.AsNoTracking(), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_SqlQuery_with_AsStreaming()
        {
            var mockSqlQuery = new Mock<DbSqlQuery<Product>>();
            mockSqlQuery.Setup(m => m.AsStreaming()).Returns(mockSqlQuery.Object);
            var products = new[] { new Product { Id = 1 }, new Product { Id = 2 } };
            mockSqlQuery.Setup(m => m.GetEnumerator()).Returns(((IEnumerable<Product>)products).GetEnumerator());

            var mockSet = new Mock<DbSet<Product>>();
            var query = "not a real query";
            var parameters = new object[] { 1, 2 };
            mockSet.Setup(m => m.SqlQuery(query, parameters)).Returns(mockSqlQuery.Object);

            Assert.Equal(new[] { 1, 2 }, mockSet.Object.SqlQuery(query, parameters).AsStreaming().Select(p => p.Id));
            mockSet.Verify(m => m.SqlQuery(query, parameters), Times.Once());
            mockSqlQuery.Verify(m => m.AsStreaming(), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_Create()
        {
            var mockSet = new Mock<DbSet<Product>>();
            var product = new Product();
            mockSet.Setup(m => m.Create()).Returns(product);

            Assert.Same(product, mockSet.Object.Create());
            mockSet.Verify(m => m.Create(), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_generic_Create()
        {
            var mockSet = new Mock<DbSet<Product>>();
            var product = new FeaturedProduct();
            mockSet.Setup(m => m.Create<FeaturedProduct>()).Returns(product);

            Assert.Same(product, mockSet.Object.Create<FeaturedProduct>());
            mockSet.Verify(m => m.Create<FeaturedProduct>(), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_Local()
        {
            var mockSet = new Mock<DbSet<Product>>();
            var mockLocal = new Mock<DbLocalView<Product>>();
            mockSet.Setup(m => m.Local).Returns(mockLocal.Object);

            Assert.Same(mockLocal.Object, mockSet.Object.Local);
            mockSet.Verify(m => m.Local, Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_getting_results()
        {
            var products = new[] { new Product { Id = 1 }, new Product { Id = 2 } };
            var mockSet = CreateMockQueryableSet(new InMemoryAsyncQueryable<Product>(products.AsQueryable()));

            DbSet<Product> dbSet = mockSet.Object;
            Assert.Equal(new[] { 1, 2 }, dbSet.ToList().Select(p => p.Id));
            mockSet.Verify(m => m.GetEnumerator(), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_ToString()
        {
            var mockSet = new Mock<DbSet<Product>>();
            mockSet.Setup(m => m.ToString()).Returns("Hello World!");

            Assert.Equal("Hello World!", mockSet.Object.ToString());
            mockSet.Verify(m => m.ToString(), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_ToString_on_SqlQuery()
        {
            var mockSqlQuery = new Mock<DbSqlQuery<Product>>();
            mockSqlQuery.Setup(m => m.ToString()).Returns("Hello World!");

            Assert.Equal("Hello World!", mockSqlQuery.Object.ToString());
            mockSqlQuery.Verify(m => m.ToString(), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_query_with_Include_extension_method()
        {
            var products = new[] { new Product { Id = 1 }, new Product { Id = 2 } };
            var mockSet = CreateMockQueryableSet(new InMemoryAsyncQueryable<Product>(products.AsQueryable()));
            mockSet.Setup(m => m.Include(It.IsAny<string>())).Returns(mockSet.Object);

            IQueryable<Product> query = mockSet.Object;

            var results = query.Include(p => p.Category).ToList();

            Assert.Equal(new[] { 1, 2 }, results.Select(p => p.Id));
            mockSet.Verify(m => m.Include("Category"), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_query_with_base_Include_method()
        {
            var products = new[] { new Product { Id = 1 }, new Product { Id = 2 } };
            var mockSet = CreateMockQueryableSet(new InMemoryAsyncQueryable<Product>(products.AsQueryable()));
            mockSet.Setup(m => m.Include(It.IsAny<string>())).Returns(mockSet.Object);

            DbSet<Product> query = mockSet.Object;

            var results = query.Include("Category").ToList();

            Assert.Equal(new[] { 1, 2 }, results.Select(p => p.Id));
            mockSet.Verify(m => m.Include("Category"), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_query_with_Include_extension_method_that_does_something()
        {
            var products = new[] { new Product { Id = 1 }, new Product { Id = 2 } };
            var mockSet = CreateMockQueryableSet(new InMemoryAsyncQueryable<Product>(products.AsQueryable()));
            mockSet.Setup(m => m.Include("Category")).Callback<string>(
                i =>
                    {
                        foreach (var product in products)
                        {
                            product.Category = new Category { DetailedDescription = "Aardman" };
                        }
                    }).Returns(mockSet.Object);

            IQueryable<Product> query = mockSet.Object;

            var results = query.Include(p => p.Category).ToList();

            Assert.Equal(new[] { 1, 2 }, results.Select(p => p.Id));
            Assert.Equal(new[] { "Aardman", "Aardman" }, results.Select(p => p.Category.DetailedDescription));
            mockSet.Verify(m => m.Include("Category"), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_query_with_base_Include_method_that_does_something()
        {
            var products = new[] { new Product { Id = 1 }, new Product { Id = 2 } };
            var mockSet = CreateMockQueryableSet(new InMemoryAsyncQueryable<Product>(products.AsQueryable()));
            mockSet.Setup(m => m.Include("Category")).Callback<string>(
                i =>
                    {
                        foreach (var product in products)
                        {
                            product.Category = new Category { DetailedDescription = "Aardman" };
                        }
                    }).Returns(mockSet.Object);

            DbSet<Product> query = mockSet.Object;

            var results = query.Include("Category").ToList();

            Assert.Equal(new[] { 1, 2 }, results.Select(p => p.Id));
            Assert.Equal(new[] { "Aardman", "Aardman" }, results.Select(p => p.Category.DetailedDescription));
            mockSet.Verify(m => m.Include("Category"), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_query_with_AsNoTracking_extension_method()
        {
            var products = new[] { new Product { Id = 1 }, new Product { Id = 2 } };
            var mockSet = CreateMockQueryableSet(new InMemoryAsyncQueryable<Product>(products.AsQueryable()));
            mockSet.Setup(m => m.AsNoTracking()).Returns(mockSet.Object);

            IQueryable<Product> query = mockSet.Object;

            var results = query.AsNoTracking().ToList();

            Assert.Equal(new[] { 1, 2 }, results.Select(p => p.Id));
            mockSet.Verify(m => m.AsNoTracking(), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_query_with_base_AsNoTracking_method()
        {
            var products = new[] { new Product { Id = 1 }, new Product { Id = 2 } };
            var mockSet = CreateMockQueryableSet(new InMemoryAsyncQueryable<Product>(products.AsQueryable()));
            mockSet.Setup(m => m.AsNoTracking()).Returns(mockSet.Object);

            DbSet<Product> query = mockSet.Object;

            var results = query.AsNoTracking().ToList();

            Assert.Equal(new[] { 1, 2 }, results.Select(p => p.Id));
            mockSet.Verify(m => m.AsNoTracking(), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_query_with_AsStreaming_extension_method()
        {
            var products = new[] { new Product { Id = 1 }, new Product { Id = 2 } };
            var mockSet = CreateMockQueryableSet(new InMemoryAsyncQueryable<Product>(products.AsQueryable()));
            mockSet.Setup(m => m.AsStreaming()).Returns(mockSet.Object);

            IQueryable<Product> query = mockSet.Object;

            var results = query.AsStreaming().ToList();

            Assert.Equal(new[] { 1, 2 }, results.Select(p => p.Id));
            mockSet.Verify(m => m.AsStreaming(), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_query_with_base_AsStreaming_method()
        {
            var products = new[] { new Product { Id = 1 }, new Product { Id = 2 } };
            var mockSet = CreateMockQueryableSet(new InMemoryAsyncQueryable<Product>(products.AsQueryable()));
            mockSet.Setup(m => m.AsStreaming()).Returns(mockSet.Object);

            DbSet<Product> query = mockSet.Object;

            var results = query.AsStreaming().ToList();

            Assert.Equal(new[] { 1, 2 }, results.Select(p => p.Id));
            mockSet.Verify(m => m.AsStreaming(), Times.Once());
        }

#if !NET40
        [Fact]
        public void Moq_DbSet_can_be_used_for_FindAsync()
        {
            var mockSet = new Mock<DbSet<Product>>();
            var product = new Product();
            mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).Returns(Task.FromResult(product));

            Assert.Same(product, mockSet.Object.FindAsync(1).Result);
            mockSet.Verify(m => m.FindAsync(1), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_getting_async_results()
        {
            var products = new[] { new Product { Id = 1 }, new Product { Id = 2 } };
            var mockSet = CreateMockQueryableSet(new InMemoryAsyncQueryable<Product>(products.AsQueryable()));

            DbSet<Product> dbSet = mockSet.Object;
            Assert.Equal(new[] { 1, 2 }, dbSet.ToListAsync().Result.Select(p => p.Id));
            mockSet.Verify(m => m.GetAsyncEnumerator(), Times.Once());
        }

        [Fact]
        public void Moq_DbSet_can_be_used_for_async_SqlQuery()
        {
            var mockSqlQuery = new Mock<DbSqlQuery<Product>>();
            var products = new[] { new Product { Id = 1 }, new Product { Id = 2 } };
            mockSqlQuery.As<IDbAsyncEnumerable<Product>>()
                .Setup(m => m.GetAsyncEnumerator())
                .Returns(new InMemoryDbAsyncEnumerator<Product>(((IEnumerable<Product>)products).GetEnumerator()));

            var mockSet = new Mock<DbSet<Product>>();
            var query = "not a real query";
            var parameters = new object[] { 1, 2 };
            mockSet.Setup(m => m.SqlQuery(query, parameters)).Returns(mockSqlQuery.Object);

            Assert.Equal(new[] { 1, 2 }, mockSet.Object.SqlQuery(query, parameters).ToListAsync().Result.Select(p => p.Id));
            mockSet.Verify(m => m.SqlQuery(query, parameters), Times.Once());
        }
#endif

        // Works around an issue with Moq that won't let the explicit implementation of IQueryable<T> on DbSet
        // be configured even when using As().
        public abstract class MockableDbSetWithIQueryable<T> : DbSet<T>, IQueryable<T>
#if !NET40
            , IDbAsyncEnumerable<T>
#endif
            where T : class
        {
            public abstract IEnumerator<T> GetEnumerator();
            public abstract Expression Expression { get; }
            public abstract Type ElementType { get; }
            public abstract IQueryProvider Provider { get; }
#if !NET40
            public abstract IDbAsyncEnumerator<T> GetAsyncEnumerator();
#endif
        }

        private static Mock<MockableDbSetWithIQueryable<Product>> CreateMockQueryableSet(IQueryable<Product> queryable)
        {
            var mockSet = new Mock<MockableDbSetWithIQueryable<Product>>();
            mockSet.Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
#if !NET40
            mockSet.Setup(m => m.GetAsyncEnumerator()).Returns(((IDbAsyncEnumerable<Product>)queryable).GetAsyncEnumerator());
#endif

            return mockSet;
        }
    }
}
