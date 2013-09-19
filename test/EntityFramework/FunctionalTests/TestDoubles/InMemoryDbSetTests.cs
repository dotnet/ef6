// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestDoubles
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using SimpleModel;
    using Xunit;

    public class InMemoryDbSetTests : FunctionalTestBase
    {
        [Fact]
        public void In_memory_DbSet_can_be_used_for_Find()
        {
            var product = new Product { Id = 1 };
            var set = new InMemoryDbSet<Product>(
                new[] { product, new Product { Id = 2 } },
                (d, k) => d.Single(p => p.Id == (int)k[0]));

            Assert.Same(product, set.Find(1));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_Add()
        {
            var set = new InMemoryDbSet<Product>();
            var product = new Product();

            Assert.Same(product, set.Add(product));
            Assert.Same(product, set.Single());
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_AddRange()
        {
            var set = new InMemoryDbSet<Product>();
            var products = new[] { new Product { Id = 1 }, new Product { Id = 2 } };

            Assert.Same(products, set.AddRange(products));
            Assert.Equal(products.OrderBy(p => p.Id), set.OrderBy(p => p.Id));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_Attach()
        {
            var set = new InMemoryDbSet<Product>();
            var product = new Product();

            Assert.Same(product, set.Attach(product));
            Assert.Same(product, set.Single());
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_Remove()
        {
            var set = new InMemoryDbSet<Product>();
            var product = new Product();

            set.Add(product);

            Assert.Same(product, set.Remove(product));
            Assert.Empty(set);
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_RemoveRange()
        {
            var products = new[] { new Product(), new Product() };
            var set = new InMemoryDbSet<Product>(products);

            Assert.Same(products, set.RemoveRange(products));
            Assert.Empty(set);
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_AddOrUpdate()
        {
            DbSet<Product> set = new InMemoryDbSet<Product>();

            set.AddOrUpdate(new[] { new Product { Id = 1 }, new Product { Id = 2 } });

            Assert.Equal(new[] { 1, 2 }, set.Select(p => p.Id).OrderBy(i => i));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_AddOrUpdate_with_expression()
        {
            DbSet<Product> set = new InMemoryDbSet<Product>();

            set.AddOrUpdate(e => e.Id, new Product { Id = 1 }, new Product { Id = 2 });

            Assert.Equal(new[] { 1, 2 }, set.Select(p => p.Id).OrderBy(i => i));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_SqlQuery()
        {
            Assert.Equal(
                new[] { "Cheese", "Gromit", "Wallace" },
                CreateSetWithRawData().SqlQuery("not a real query", 4, 5).Select(p => p.Name).OrderBy(n => n));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_SqlQuery_with_AsNoTracking()
        {
            Assert.Equal(
                new[] { "Cheese", "Gromit", "Wallace" },
                CreateSetWithRawData().SqlQuery("not a real query", 4, 5).AsNoTracking().Select(p => p.Name).OrderBy(n => n));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_SqlQuery_with_AsStreaming()
        {
            Assert.Equal(
                new[] { "Cheese", "Gromit", "Wallace" },
                CreateSetWithRawData().SqlQuery("not a real query", 4, 5).AsStreaming().Select(p => p.Name).OrderBy(n => n));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_Create()
        {
            Assert.IsType<Product>(new InMemoryDbSet<Product>().Create());
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_generic_Create()
        {
            Assert.IsType<FeaturedProduct>(new InMemoryDbSet<Product>().Create<FeaturedProduct>());
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_Local()
        {
            Assert.Equal(new[] { "Cheese", "Gromit", "Wallace" }, CreateSetWithData().Local.Select(p => p.Name).OrderBy(n => n));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_getting_results()
        {
            Assert.Equal(new[] { "Cheese", "Gromit", "Wallace" }, CreateSetWithData().Select(p => p.Name).OrderBy(n => n));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_ToString()
        {
            Assert.Equal("An in-memory DbSet", new InMemoryDbSet<Product>().ToString());
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_ToString_on_SqlQuery()
        {
            Assert.Equal(
                "An in-memory SqlQuery",
                new InMemoryDbSet<Product>(null, null, null, (p, d) => new InMemorySqlQuery<Product>(new Product[0])).SqlQuery("Foo").ToString());
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_query_with_Include_extension_method()
        {
            var results = CreateSetWithData().Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .Include(p => p.Category)
                .ToList();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_query_with_base_Include_method()
        {
            var results = CreateSetWithData().Include(p => p.Category)
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .ToList();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_query_with_Include_extension_method_that_does_something()
        {
            var results = CreateSetWithData(
                (p, d) =>
                    {
                        foreach (var product in d)
                        {
                            product.Category = new Category { DetailedDescription = "Aardman" };
                        }
                    })
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .Include(p => p.Category)
                .ToList();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
            Assert.Equal(new[] { "Aardman", "Aardman" }, results.Select(p => p.Category.DetailedDescription));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_query_with_base_Include_method_that_does_something()
        {
            var results = CreateSetWithData(
                (p, d) =>
                    {
                        foreach (var product in d)
                        {
                            product.Category = new Category { DetailedDescription = "Aardman" };
                        }
                    })
                .Include(p => p.Category)
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .ToList();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
            Assert.Equal(new[] { "Aardman", "Aardman" }, results.Select(p => p.Category.DetailedDescription));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_query_with_AsNoTracking_extension_method()
        {
            var results = CreateSetWithData().Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .AsNoTracking()
                .ToList();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_query_with_base_AsNoTracking_method()
        {
            var results = CreateSetWithData().AsNoTracking()
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .ToList();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_query_with_AsStreaming_extension_method()
        {
            var results = CreateSetWithData().Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .AsStreaming()
                .ToList();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_query_with_base_AsStreaming_method()
        {
            var results = CreateSetWithData().AsStreaming()
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .ToList();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
        }

        [Fact]
        public void In_memory_DbSet_can_be_converted_to_non_generic_in_memory_DbDet()
        {
            Assert.Equal(
                new[] { "Cheese", "Gromit", "Wallace" }, 
                ((DbSet)CreateSetWithData()).ToList<Product>().Select(p => p.Name).OrderBy(n => n));
        }

#if !NET40
        [Fact]
        public void In_memory_DbSet_can_be_used_for_FindAsync()
        {
            var product = new Product { Id = 1 };
            var set = new InMemoryDbSet<Product>(
                new[] { product, new Product { Id = 2 } },
                (d, k) => d.Single(p => p.Id == (int)k[0]));

            Assert.Same(product, set.FindAsync(1).Result);
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_getting_async_results()
        {
            var results = CreateSetWithData().ToListAsync();

            Assert.Equal(new[] { "Cheese", "Gromit", "Wallace" }, results.Result.Select(p => p.Name).OrderBy(n => n));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_async_query_with_Include_extension_method()
        {
            var results = CreateSetWithData().Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .Include(p => p.Category)
                .ToListAsync();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Result.Select(p => p.Name));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_async_query_with_base_Include_method()
        {
            var results = CreateSetWithData().Include(p => p.Category)
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .ToListAsync();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Result.Select(p => p.Name));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_async_query_with_Include_extension_method_that_does_something()
        {
            var results = CreateSetWithData(
                (p, d) =>
                    {
                        foreach (var product in d)
                        {
                            product.Category = new Category { DetailedDescription = "Aardman" };
                        }
                    })
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .Include(p => p.Category)
                .ToListAsync();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Result.Select(p => p.Name));
            Assert.Equal(new[] { "Aardman", "Aardman" }, results.Result.Select(p => p.Category.DetailedDescription));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_async_query_with_base_Include_method_that_does_something()
        {
            var results = CreateSetWithData(
                (p, d) =>
                    {
                        foreach (var product in d)
                        {
                            product.Category = new Category { DetailedDescription = "Aardman" };
                        }
                    })
                .Include(p => p.Category)
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .ToListAsync();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Result.Select(p => p.Name));
            Assert.Equal(new[] { "Aardman", "Aardman" }, results.Result.Select(p => p.Category.DetailedDescription));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_async_query_with_AsNoTracking_extension_method()
        {
            var results = CreateSetWithData().Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .AsNoTracking()
                .ToListAsync();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Result.Select(p => p.Name));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_async_query_with_base_AsNoTracking_method()
        {
            var results = CreateSetWithData().AsNoTracking()
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .ToListAsync();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Result.Select(p => p.Name));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_async_query_with_AsStreaming_extension_method()
        {
            var results = CreateSetWithData().Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .AsStreaming()
                .ToListAsync();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Result.Select(p => p.Name));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_async_query_with_base_AsStreaming_method()
        {
            var results = CreateSetWithData().AsStreaming()
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .ToListAsync();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Result.Select(p => p.Name));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_async_SqlQuery()
        {
            Assert.Equal(
                new[] { "Cheese", "Gromit", "Wallace" },
                CreateSetWithRawData().SqlQuery("not a real query", 4, 5).ToArrayAsync().Result.Select(p => p.Name).OrderBy(n => n));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_async_SqlQuery_with_AsNoTracking()
        {
            Assert.Equal(
                new[] { "Cheese", "Gromit", "Wallace" },
                CreateSetWithRawData()
                    .SqlQuery("not a real query", 4, 5)
                    .AsNoTracking()
                    .ToArrayAsync()
                    .Result.Select(p => p.Name)
                    .OrderBy(n => n));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_async_SqlQuery_with_AsStreaming()
        {
            Assert.Equal(
                new[] { "Cheese", "Gromit", "Wallace" },
                CreateSetWithRawData()
                    .SqlQuery("not a real query", 4, 5)
                    .AsStreaming()
                    .ToArrayAsync()
                    .Result.Select(p => p.Name)
                    .OrderBy(n => n));
        }
#endif

        private static InMemoryDbSet<Product> CreateSetWithRawData()
        {
            return new InMemoryDbSet<Product>(
                null, null, null, (q, p) => new InMemorySqlQuery<Product>(
                                          new[]
                                              {
                                                  new Product { Name = "Wallace" },
                                                  new Product { Name = "Gromit" },
                                                  new Product { Name = "Cheese" }
                                              }));
        }

        private static InMemoryDbSet<Product> CreateSetWithData(Action<string, IEnumerable<Product>> include = null)
        {
            return new InMemoryDbSet<Product>(
                new[]
                    {
                        new Product { Name = "Wallace" },
                        new Product { Name = "Gromit" },
                        new Product { Name = "Cheese" }
                    }, null, include);
        }
    }
}
