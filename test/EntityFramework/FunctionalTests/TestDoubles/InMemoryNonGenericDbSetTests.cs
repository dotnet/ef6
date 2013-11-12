// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestDoubles
{
    using System.Collections.Generic;
    using System.Linq;
    using SimpleModel;
    using Xunit;

    public class InMemoryNonGenericDbSetTests : FunctionalTestBase
    {
        [Fact]
        public void In_memory_DbSet_can_be_used_for_Find()
        {
            var product = new Product { Id = 1 };
            var set = new InMemoryNonGenericDbSet<Product>(
                new[] { product, new Product { Id = 2 } }, (d, k) => d.Single(p => p.Id == (int)k[0]));

            Assert.Same(product, set.Find(1));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_Add()
        {
            var set = new InMemoryNonGenericDbSet<Product>();
            var product = new Product();

            Assert.Same(product, set.Add(product));
            Assert.Same(product, set.ToList<Product>().Single());
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_AddRange()
        {
            var set = new InMemoryNonGenericDbSet<Product>();
            var products = new[] { new Product { Id = 1 }, new Product { Id = 2 } };

            Assert.Same(products, set.AddRange(products));
            Assert.Equal(products.OrderBy(p => p.Id), set.ToList<Product>().OrderBy(p => p.Id));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_Attach()
        {
            var set = new InMemoryNonGenericDbSet<Product>();
            var product = new Product();

            Assert.Same(product, set.Attach(product));
            Assert.Same(product, set.ToList<Product>().Single());
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_Remove()
        {
            var set = new InMemoryNonGenericDbSet<Product>();
            var product = new Product();

            set.Add(product);

            Assert.Same(product, set.Remove(product));
            Assert.Empty(set);
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_RemoveRange()
        {
            var products = new[] { new Product(), new Product() };
            var set = new InMemoryNonGenericDbSet<Product>(products);

            Assert.Same(products, set.RemoveRange(products));
            Assert.Empty(set);
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_SqlQuery()
        {
            Assert.Equal(
                new[] { "Cheese", "Gromit", "Wallace" },
                CreateSetWithRawData().SqlQuery("not a real query", 4, 5).ToList<Product>().Select(p => p.Name).OrderBy(n => n));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_SqlQuery_with_AsNoTracking()
        {
            Assert.Equal(
                new[] { "Cheese", "Gromit", "Wallace" },
                CreateSetWithRawData().SqlQuery("not a real query", 4, 5).AsNoTracking().ToList<Product>().Select(p => p.Name).OrderBy(n => n));
        }

#pragma warning disable 612, 618
        [Fact]
        public void In_memory_DbSet_can_be_used_for_SqlQuery_with_AsStreaming()
        {
            Assert.Equal(
                new[] { "Cheese", "Gromit", "Wallace" },
                CreateSetWithRawData().SqlQuery("not a real query", 4, 5).AsStreaming().ToList<Product>().Select(p => p.Name).OrderBy(n => n));
        }
#pragma warning restore 612, 618

        [Fact]
        public void In_memory_DbSet_can_be_used_for_Create()
        {
            Assert.IsType<Product>(new InMemoryNonGenericDbSet<Product>().Create());
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_generic_Create()
        {
            Assert.IsType<FeaturedProduct>(new InMemoryNonGenericDbSet<Product>().Create(typeof(FeaturedProduct)));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_Local()
        {
            Assert.Equal(
                new[] { "Cheese", "Gromit", "Wallace" }, CreateSetWithData().Local.ToList<Product>().Select(p => p.Name).OrderBy(n => n));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_getting_results()
        {
            Assert.Equal(new[] { "Cheese", "Gromit", "Wallace" }, CreateSetWithData().ToList<Product>().Select(p => p.Name).OrderBy(n => n));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_ToString()
        {
            Assert.Equal("An in-memory DbSet", new InMemoryNonGenericDbSet<Product>().ToString());
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_ToString_on_SqlQuery()
        {
            Assert.Equal(
                "An in-memory SqlQuery",
                new InMemoryNonGenericDbSet<Product>(
                    null, null, null, (p, d) => new InMemoryNonGenericSqlQuery<Product>(new Product[0])).SqlQuery("Foo").ToString());
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_query_with_Include_extension_method()
        {
            IQueryable query = CreateSetWithData();
            var results = query.Include("Category")
                .ToList<Product>()
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name);

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_query_with_base_Include_method()
        {
            var results = CreateSetWithData().Include("Category")
                .ToList<Product>()
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .ToList();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_query_with_Include_extension_method_that_does_something()
        {
            IQueryable query = CreateSetWithData(
                (p, d) =>
                    {
                        foreach (var product in d)
                        {
                            product.Category = new Category { DetailedDescription = "Aardman" };
                        }
                    });

            var results = query.Include("Category")
                .ToList<Product>()
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
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
                .Include("Category")
                .ToList<Product>()
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .ToList();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
            Assert.Equal(new[] { "Aardman", "Aardman" }, results.Select(p => p.Category.DetailedDescription));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_query_with_AsNoTracking_extension_method()
        {
            IQueryable query = CreateSetWithData();
            var results = query.AsNoTracking()
                .ToList<Product>()
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .ToList();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_query_with_base_AsNoTracking_method()
        {
            var results = CreateSetWithData().AsNoTracking()
                .ToList<Product>()
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .ToList();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
        }

#pragma warning disable 612, 618
        [Fact]
        public void In_memory_DbSet_can_be_used_for_query_with_AsStreaming_extension_method()
        {
            IQueryable query = CreateSetWithData();
            var results = query.AsStreaming()
                .ToList<Product>()
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .ToList();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
        }
#pragma warning restore 612, 618

#pragma warning disable 612, 618
        [Fact]
        public void In_memory_DbSet_can_be_used_for_query_with_base_AsStreaming_method()
        {
            var results = CreateSetWithData().AsStreaming()
                .ToList<Product>()
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name)
                .ToList();

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
        }
#pragma warning restore 612, 618

        [Fact]
        public void In_memory_DbSet_can_be_converted_to_generic_in_memory_DbDet()
        {
            Assert.Equal(
                new[] { "Cheese", "Gromit", "Wallace" },
                CreateSetWithData().Cast().Select(p => p.Name).OrderBy(n => n));
        }

#if !NET40
        [Fact]
        public void In_memory_DbSet_can_be_used_for_FindAsync()
        {
            var product = new Product { Id = 1 };
            var set = new InMemoryNonGenericDbSet<Product>(
                new[] { product, new Product { Id = 2 } },
                (d, k) => d.Single(p => p.Id == (int)k[0]));

            Assert.Same(product, set.FindAsync(1).Result);
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_getting_async_results()
        {
            var results = CreateSetWithData().ToListAsync();

            Assert.Equal(new[] { "Cheese", "Gromit", "Wallace" }, results.Result.ToList<Product>().Select(p => p.Name).OrderBy(n => n));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_async_query_with_Include_extension_method()
        {
            IQueryable query = CreateSetWithData();
            var results = query.Include("Category")
                .ToListAsync().Result.OfType<Product>()
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name);

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_async_query_with_base_Include_method()
        {
            var results = CreateSetWithData().Include("Category")
                .ToListAsync().Result.OfType<Product>()
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name);

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_async_query_with_Include_extension_method_that_does_something()
        {
            IQueryable query = CreateSetWithData(
                (p, d) =>
                    {
                        foreach (var product in d)
                        {
                            product.Category = new Category { DetailedDescription = "Aardman" };
                        }
                    });
            var results = query
                .Include("Category")
                .ToListAsync().Result.OfType<Product>()
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name);

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
            Assert.Equal(new[] { "Aardman", "Aardman" }, results.Select(p => p.Category.DetailedDescription));
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
                .Include("Category")
                .ToListAsync().Result.OfType<Product>()
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name);

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
            Assert.Equal(new[] { "Aardman", "Aardman" }, results.Select(p => p.Category.DetailedDescription));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_async_query_with_AsNoTracking_extension_method()
        {
            IQueryable query = CreateSetWithData();
            var results = query.AsNoTracking()
                .ToListAsync().Result.OfType<Product>()
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name);

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_async_query_with_base_AsNoTracking_method()
        {
            var results = CreateSetWithData().AsNoTracking()
                .ToListAsync().Result.OfType<Product>()
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name);

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
        }

#pragma warning disable 612, 618
        [Fact]
        public void In_memory_DbSet_can_be_used_for_async_query_with_AsStreaming_extension_method()
        {
            IQueryable query = CreateSetWithData();
            var results = query.AsStreaming()
                .ToListAsync().Result.OfType<Product>()
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name);

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_async_query_with_base_AsStreaming_method()
        {
            var results = CreateSetWithData().AsStreaming()
                .ToListAsync().Result.OfType<Product>()
                .Where(p => p.Name.Contains("e"))
                .OrderBy(p => p.Name);

            Assert.Equal(new[] { "Cheese", "Wallace" }, results.Select(p => p.Name));
        }
#pragma warning restore 612, 618

        [Fact]
        public void In_memory_DbSet_can_be_used_for_async_SqlQuery()
        {
            Assert.Equal(
                new[] { "Cheese", "Gromit", "Wallace" },
                CreateSetWithRawData()
                    .SqlQuery("not a real query", 4, 5)
                    .ToListAsync()
                    .Result.OfType<Product>()
                    .Select(p => p.Name)
                    .OrderBy(n => n));
        }

        [Fact]
        public void In_memory_DbSet_can_be_used_for_async_SqlQuery_with_AsNoTracking()
        {
            Assert.Equal(
                new[] { "Cheese", "Gromit", "Wallace" },
                CreateSetWithRawData()
                    .SqlQuery("not a real query", 4, 5)
                    .AsNoTracking()
                    .ToListAsync()
                    .Result.OfType<Product>()
                    .Select(p => p.Name)
                    .OrderBy(n => n));
        }

#pragma warning disable 612, 618
        [Fact]
        public void In_memory_DbSet_can_be_used_for_async_SqlQuery_with_AsStreaming()
        {
            Assert.Equal(
                new[] { "Cheese", "Gromit", "Wallace" },
                CreateSetWithRawData()
                    .SqlQuery("not a real query", 4, 5)
                    .AsStreaming()
                    .ToListAsync()
                    .Result.OfType<Product>()
                    .Select(p => p.Name)
                    .OrderBy(n => n));
        }
#pragma warning restore 612, 618
#endif

        private static InMemoryNonGenericDbSet<Product> CreateSetWithRawData()
        {
            return new InMemoryNonGenericDbSet<Product>(
                null, null, null, (q, p) => new InMemoryNonGenericSqlQuery<Product>(
                                          new[]
                                              {
                                                  new Product { Name = "Wallace" },
                                                  new Product { Name = "Gromit" },
                                                  new Product { Name = "Cheese" }
                                              }));
        }

        private static InMemoryNonGenericDbSet<Product> CreateSetWithData(Action<string, IEnumerable<Product>> include = null)
        {
            return new InMemoryNonGenericDbSet<Product>(
                new[]
                    {
                        new Product { Name = "Wallace" },
                        new Product { Name = "Gromit" },
                        new Product { Name = "Cheese" }
                    },
                null, include);
        }
    }
}
