// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Data.Entity.Core;
    using System.Data;
    using System.Data.Entity;
    using System.Linq;
    using System.Transactions;
    using SimpleModel;
    using Xunit;

    /// <summary>
    ///     These are not really tests, but rather simple scenarios with minimal final state validation.
    ///     They are here as a good starting point for understanding the functionality and to give some level
    ///     of confidence that the simple scenarios keep on working as the code evolves.
    /// </summary>
    public class SimpleScenarios : FunctionalTestBase
    {
        #region Scenarios for SQL Server

        [Fact]
        public void
            SqlServer_Database_can_be_created_with_columns_that_explicitly_total_more_that_8060_bytes_and_data_longer_than_8060_can_be_inserted
            ()
        {
            EnsureDatabaseInitialized(() => new ModelWithWideProperties());

            using (new TransactionScope())
            {
                using (var context = new ModelWithWideProperties())
                {
                    var entity = new EntityWithExplicitWideProperties
                                     {
                                         Property1 = new String('1', 1000),
                                         Property2 = new String('2', 1000),
                                         Property3 = new String('3', 1000),
                                         Property4 = new String('4', 1000),
                                     };

                    context.ExplicitlyWide.Add(entity);

                    context.SaveChanges();

                    entity.Property1 = new String('A', 4000);
                    entity.Property2 = new String('B', 4000);

                    context.SaveChanges();
                }
            }
        }

        [Fact]
        public void
            SqlServer_Database_can_be_created_with_columns_that_implicitly_total_more_that_8060_bytes_and_data_longer_than_8060_can_be_inserted
            ()
        {
            EnsureDatabaseInitialized(() => new ModelWithWideProperties());

            using (new TransactionScope())
            {
                using (var context = new ModelWithWideProperties())
                {
                    var entity = new EntityWithImplicitWideProperties
                                     {
                                         Property1 = new String('1', 1000),
                                         Property2 = new String('2', 1000),
                                         Property3 = new String('3', 1000),
                                         Property4 = new String('4', 1000),
                                     };

                    context.ImplicitlyWide.Add(entity);

                    context.SaveChanges();

                    entity.Property1 = new String('A', 4000);
                    entity.Property2 = new String('B', 4000);

                    context.SaveChanges();
                }
            }
        }

        [Fact]
        public void Scenario_Find()
        {
            using (var context = new SimpleModelContext())
            {
                var product = context.Products.Find(1);
                var category = context.Categories.Find("Foods");

                // Scenario ends; simple validation of final state follows
                Assert.NotNull(product);
                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, product).State);

                Assert.NotNull(category);
                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, category).State);

                Assert.Equal("Foods", product.CategoryId);
                Assert.Same(category, product.Category);
                Assert.True(category.Products.Contains(product));
            }
        }

        [Fact]
        public void Scenario_Insert()
        {
            EnsureDatabaseInitialized(() => new SimpleModelContext());

            using (new TransactionScope())
            {
                using (var context = new SimpleModelContext())
                {
                    var product = new Product
                                      {
                                          Name = "Vegemite"
                                      };
                    context.Products.Add(product);
                    context.SaveChanges();

                    // Scenario ends; simple validation of final state follows
                    Assert.NotEqual(0, product.Id);
                    Assert.Equal(EntityState.Unchanged, GetStateEntry(context, product).State);
                }
            }
        }

        [Fact]
        public void Scenario_Update()
        {
            EnsureDatabaseInitialized(() => new SimpleModelContext());

            using (new TransactionScope())
            {
                using (var context = new SimpleModelContext())
                {
                    var product = context.Products.Find(1);
                    product.Name = "iSnack 2.0";
                    context.SaveChanges();

                    // Scenario ends; simple validation of final state follows
                    Assert.Equal("iSnack 2.0", product.Name);
                    Assert.Equal(EntityState.Unchanged, GetStateEntry(context, product).State);
                }
            }
        }

        [Fact]
        public void Scenario_Query()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.ToList();

                // Scenario ends; simple validation of final state follows
                Assert.Equal(7, products.Count);
                Assert.True(products.TrueForAll(p => GetStateEntry(context, p).State == EntityState.Unchanged));
            }
        }

        [Fact]
        public void Scenario_Relate_using_query()
        {
            EnsureDatabaseInitialized(() => new SimpleModelContext());

            using (new TransactionScope())
            {
                using (var context = new SimpleModelContext())
                {
                    var category = context.Categories.Find("Foods");
                    var product = new Product
                                      {
                                          Name = "Bovril",
                                          Category = category
                                      };
                    context.Products.Add(product);
                    context.SaveChanges();

                    // Scenario ends; simple validation of final state follows
                    Assert.NotNull(product);
                    Assert.Equal(EntityState.Unchanged, GetStateEntry(context, product).State);

                    Assert.NotNull(category);
                    Assert.Equal(EntityState.Unchanged, GetStateEntry(context, category).State);

                    Assert.Equal("Foods", product.CategoryId);
                    Assert.Same(category, product.Category);
                    Assert.True(category.Products.Contains(product));
                }
            }
        }

        [Fact]
        public void Scenario_Relate_using_FK()
        {
            EnsureDatabaseInitialized(() => new SimpleModelContext());

            using (new TransactionScope())
            {
                using (var context = new SimpleModelContext())
                {
                    var product = new Product
                                      {
                                          Name = "Bovril",
                                          CategoryId = "Foods"
                                      };
                    context.Products.Add(product);
                    context.SaveChanges();

                    // Scenario ends; simple validation of final state follows
                    Assert.NotNull(product);
                    Assert.Equal(EntityState.Unchanged, GetStateEntry(context, product).State);
                    Assert.Equal("Foods", product.CategoryId);
                }
            }
        }

        [Fact]
        public void Scenario_CodeFirst_with_ModelBuilder()
        {
            Database.Delete("Scenario_CodeFirstWithModelBuilder");

            var builder = new DbModelBuilder();

            builder.Entity<Product>();
            builder.Entity<Category>();

            var model = builder.Build(ProviderRegistry.Sql2008_ProviderInfo).Compile();

            using (var context = new SimpleModelContextWithNoData("Scenario_CodeFirstWithModelBuilder", model))
            {
                InsertIntoCleanContext(context);
            }

            using (var context = new SimpleModelContextWithNoData("Scenario_CodeFirstWithModelBuilder", model))
            {
                ValidateFromCleanContext(context);
            }
        }

        private void ValidateFromCleanContext(SimpleModelContextWithNoData context)
        {
            var product = context.Products.Find(1);
            var category = context.Categories.Find("Large Hadron Collider");

            // Scenario ends; simple validation of final state follows
            Assert.NotNull(product);
            Assert.Equal(EntityState.Unchanged, GetStateEntry(context, product).State);

            Assert.NotNull(category);
            Assert.Equal(EntityState.Unchanged, GetStateEntry(context, category).State);

            Assert.Equal("Large Hadron Collider", product.CategoryId);
            Assert.Same(category, product.Category);
            Assert.True(category.Products.Contains(product));
        }

        private void InsertIntoCleanContext(SimpleModelContextWithNoData context)
        {
            context.Categories.Add(
                new Category
                    {
                        Id = "Large Hadron Collider"
                    });
            context.Products.Add(
                new Product
                    {
                        Name = "Higgs Boson",
                        CategoryId = "Large Hadron Collider"
                    });
            context.SaveChanges();
        }

        [Fact]
        public void Scenario_Using_two_databases()
        {
            EnsureDatabaseInitialized(() => new LoginsContext());
            EnsureDatabaseInitialized(() => new SimpleModelContext());

            using (new TransactionScope())
            {
                using (var context = new LoginsContext())
                {
                    var login = new Login
                                    {
                                        Id = Guid.NewGuid(),
                                        Username = "elmo"
                                    };
                    context.Logins.Add(login);
                    context.SaveChanges();

                    // Scenario ends; simple validation of final state follows
                    Assert.Same(login, context.Logins.Find(login.Id));
                    Assert.Equal(EntityState.Unchanged, GetStateEntry(context, login).State);
                }
            }

            using (new TransactionScope())
            {
                using (var context = new SimpleModelContext())
                {
                    var category = new Category
                                       {
                                           Id = "Books"
                                       };
                    var product = new Product
                                      {
                                          Name = "The Unbearable Lightness of Being",
                                          Category = category
                                      };
                    context.Products.Add(product);
                    context.SaveChanges();

                    // Scenario ends; simple validation of final state follows
                    Assert.Equal(EntityState.Unchanged, GetStateEntry(context, product).State);
                    Assert.Equal(EntityState.Unchanged, GetStateEntry(context, category).State);
                    Assert.Equal("Books", product.CategoryId);
                    Assert.Same(category, product.Category);
                    Assert.True(category.Products.Contains(product));
                }
            }
        }

        [Fact]
        public void Scenario_Use_AppConfig_connection_string()
        {
            Database.Delete("Scenario_Use_AppConfig_connection_string");

            using (var context = new SimpleModelContextWithNoData("Scenario_Use_AppConfig_connection_string"))
            {
                Assert.Equal("Scenario_Use_AppConfig", context.Database.Connection.Database);
                InsertIntoCleanContext(context);
            }

            using (var context = new SimpleModelContextWithNoData("Scenario_Use_AppConfig_connection_string"))
            {
                ValidateFromCleanContext(context);
            }
        }

        [Fact]
        public void Scenario_Include()
        {
            using (var context = new SimpleModelContext())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var products = context.Products.Where(p => p != null).Include("Category").ToList();

                foreach (var product in products)
                {
                    Assert.NotNull(product.Category);
                }
            }
        }

        [Fact]
        public void Scenario_IncludeWithLambda()
        {
            using (var context = new SimpleModelContext())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var products = context.Products.Where(p => p != null).Include(p => p.Category).ToList();

                foreach (var product in products)
                {
                    Assert.NotNull(product.Category);
                }
            }
        }

        #endregion Scenarios for SQL Server
    }
}
