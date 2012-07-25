// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace ProductivityApiTests
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Transactions;
    using FunctionalTests.TestHelpers;
    using SimpleModel;
    using Xunit;

    /// <summary>
    /// These are not really tests, but rather simple scenarios with minimal final state validation.
    /// They are here as a good starting point for understanding the functionality and to give some level
    /// of confidence that the simple scenarios keep on working as the code evolves.
    /// </summary>
    public class SimpleScenariosForSqlCe : FunctionalTestBase, IDisposable
    {
        #region Infrastructure/setup

        private readonly IDbConnectionFactory _previousConnectionFactory;

        public SimpleScenariosForSqlCe()
        {
            _previousConnectionFactory = DefaultConnectionFactoryResolver.Instance.ConnectionFactory;

            var sqlCeConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0",
                                                                    AppDomain.CurrentDomain.BaseDirectory, "");
            
            DefaultConnectionFactoryResolver.Instance.ConnectionFactory = sqlCeConnectionFactory;
        }

        public void Dispose()
        {
            DefaultConnectionFactoryResolver.Instance.ConnectionFactory = _previousConnectionFactory;
        }

        #endregion

        #region Scenarios for SQL Server Compact Edition

        [Fact]
        public void SqlCe_Database_can_be_created_with_columns_that_explicitly_total_more_that_8060_bytes_but_data_longer_than_8060_cannot_be_inserted()
        {
            RunInSqlCeTransaction<ModelWithWidePropertiesForSqlCe>(context =>
                                                                   {
                                                                       var entity = new EntityWithExplicitWideProperties
                                                                                    {
                                                                                        Property1 =
                                                                                            new String('1', 1000),
                                                                                        Property2 =
                                                                                            new String('2', 1000),
                                                                                        Property3 =
                                                                                            new String('3', 1000),
                                                                                        Property4 =
                                                                                            new String('4', 1000),
                                                                                    };

                                                                       context.ExplicitlyWide.Add(entity);

                                                                       context.SaveChanges();

                                                                       entity.Property1 = new String('A', 4000);
                                                                       entity.Property2 = new String('B', 4000);

                                                                       Assert.Throws<DbUpdateException>(
                                                                           () => context.SaveChanges());
                                                                   });
        }

        [Fact]
        public void SqlCe_Database_can_be_created_with_columns_that_implicitly_total_more_that_8060_bytes_but_data_longer_than_8060_cannot_be_inserted()
        {
            RunInSqlCeTransaction<ModelWithWidePropertiesForSqlCe>(context =>
                                                                   {
                                                                       var entity = new EntityWithImplicitWideProperties
                                                                                    {
                                                                                        Property1 =
                                                                                            new String('1', 1000),
                                                                                        Property2 =
                                                                                            new String('2', 1000),
                                                                                        Property3 =
                                                                                            new String('3', 1000),
                                                                                        Property4 =
                                                                                            new String('4', 1000),
                                                                                    };

                                                                       context.ImplicitlyWide.Add(entity);

                                                                       context.SaveChanges();

                                                                       entity.Property1 = new String('A', 4000);
                                                                       entity.Property2 = new String('B', 4000);

                                                                       Assert.Throws<DbUpdateException>(
                                                                           () => context.SaveChanges());
                                                                   });
        }

        [Fact]
        public void Scenario_Find_OnSqlCe()
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
        public void Scenario_Insert_OnSqlCe()
        {
            RunInSqlCeTransaction<SimpleModelContext>(context =>
                                                      {
                                                          var product = new Product() { Name = "Vegemite" };
                                                          context.Products.Add(product);
                                                          context.SaveChanges();

                                                          // Scenario ends; simple validation of final state follows
                                                          Assert.NotEqual(0, product.Id);
                                                          Assert.Equal(EntityState.Unchanged,
                                                                       GetStateEntry(context, product).State);
                                                      });
        }

        [Fact]
        public void Scenario_Update_OnSqlCe()
        {
            RunInSqlCeTransaction<SimpleModelContext>(context =>
                                                      {
                                                          var product = context.Products.Find(1);
                                                          product.Name = "iSnack 2.0";
                                                          context.SaveChanges();

                                                          // Scenario ends; simple validation of final state follows
                                                          Assert.Equal("iSnack 2.0", product.Name);
                                                          Assert.Equal(EntityState.Unchanged,
                                                                       GetStateEntry(context, product).State);
                                                      });
        }

        [Fact]
        public void Scenario_Query_OnSqlCe()
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
        public void Scenario_Relate_using_query_OnSqlCe()
        {
            RunInSqlCeTransaction<SimpleModelContext>(context =>
                                                      {
                                                          var category = context.Categories.Find("Foods");
                                                          var product = new Product()
                                                                        { Name = "Bovril", Category = category };
                                                          context.Products.Add(product);
                                                          context.SaveChanges();

                                                          // Scenario ends; simple validation of final state follows
                                                          Assert.NotNull(product);
                                                          Assert.Equal(EntityState.Unchanged,
                                                                       GetStateEntry(context, product).State);

                                                          Assert.NotNull(category);
                                                          Assert.Equal(EntityState.Unchanged,
                                                                       GetStateEntry(context, category).State);

                                                          Assert.Equal("Foods", product.CategoryId);
                                                          Assert.Same(category, product.Category);
                                                          Assert.True(category.Products.Contains(product));
                                                      });
        }

        [Fact]
        public void Scenario_Relate_using_FK_OnSqlCe()
        {
            RunInSqlCeTransaction<SimpleModelContext>(context =>
                                                      {
                                                          var product = new Product()
                                                                        { Name = "Bovril", CategoryId = "Foods" };
                                                          context.Products.Add(product);
                                                          context.SaveChanges();

                                                          // Scenario ends; simple validation of final state follows
                                                          Assert.NotNull(product);
                                                          Assert.Equal(EntityState.Unchanged,
                                                                       GetStateEntry(context, product).State);
                                                          Assert.Equal("Foods", product.CategoryId);
                                                      });
        }

        [Fact]
        public void Scenario_CodeFirst_with_ModelBuilder_OnSqlCe()
        {
            Database.Delete("Scenario_CodeFirstWithModelBuilder");

            var builder = new DbModelBuilder();

            builder.Entity<Product>();
            builder.Entity<Category>();

            var model = builder.Build(ProviderRegistry.SqlCe4_ProviderInfo).Compile();

            using (var context = new SimpleModelContextWithNoData("Scenario_CodeFirstWithModelBuilder", model))
            {
                InsertIntoCleanContext(context);
            }

            using (var context = new SimpleModelContextWithNoData("Scenario_CodeFirstWithModelBuilder", model))
            {
                ValidateFromCleanContext(context);
            }
        }

        [Fact]
        public void Scenario_Using_two_databases_OnSqlCe()
        {
            using (var context = new CeLoginsContext())
            {
                context.Database.CreateIfNotExists();
            }

            RunInSqlCeTransaction<CeLoginsContext>(context =>
                                                   {
                                                       var login = new Login()
                                                                   { Id = Guid.NewGuid(), Username = "elmo" };
                                                       context.Logins.Add(login);
                                                       context.SaveChanges();

                                                       // Scenario ends; simple validation of final state follows
                                                       Assert.Same(login, context.Logins.Find(login.Id));
                                                       Assert.Equal(EntityState.Unchanged,
                                                                    GetStateEntry(context, login).State);
                                                   });

            RunInSqlCeTransaction<SimpleModelContext>(context =>
                                                      {
                                                          var category = new Category() { Id = "Books" };
                                                          var product = new Product()
                                                                        {
                                                                            Name = "The Unbearable Lightness of Being",
                                                                            Category = category
                                                                        };
                                                          context.Products.Add(product);
                                                          context.SaveChanges();

                                                          // Scenario ends; simple validation of final state follows
                                                          Assert.Equal(EntityState.Unchanged,
                                                                       GetStateEntry(context, product).State);
                                                          Assert.Equal(EntityState.Unchanged,
                                                                       GetStateEntry(context, category).State);
                                                          Assert.Equal("Books", product.CategoryId);
                                                          Assert.Same(category, product.Category);
                                                          Assert.True(category.Products.Contains(product));
                                                      });
        }

        [Fact]
        public void Scenario_Use_AppConfig_connection_string_OnSqlCe()
        {
            Database.Delete("Scenario_Use_SqlCe_AppConfig_connection_string");

            using (var context = new SimpleModelContextWithNoData("Scenario_Use_SqlCe_AppConfig_connection_string"))
            {
                Assert.Equal("Scenario_Use_AppConfig.sdf", context.Database.Connection.Database);
                InsertIntoCleanContext(context);
            }

            using (var context = new SimpleModelContextWithNoData("Scenario_Use_SqlCe_AppConfig_connection_string"))
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
            context.Categories.Add(new Category() { Id = "Large Hadron Collider" });
            context.Products.Add(new Product() { Name = "Higgs Boson", CategoryId = "Large Hadron Collider" });
            context.SaveChanges();
        }

        [Fact]
        public void Scenario_Include_OnSqlCe()
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
        public void Scenario_IncludeWithLambda_OnSqlCe()
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

        #endregion

        #region Helpers

        /// <summary>
        /// SQL Compact doesn't allow the same connection to be opened more than once inside
        /// a transaction scope.  Therefore, we have to do some funky stuff to make sure that
        /// EF doesn't open and close the connection inside the transaction.
        /// </summary>
        private void RunInSqlCeTransaction<TContext>(Action<TContext> test) where TContext : DbContext, new()
        {
            EnsureDatabaseInitialized(() => new TContext());

            using (new TransactionScope())
            {
                using (var context = new TContext())
                {
                    ((IObjectContextAdapter)context).ObjectContext.Connection.Open();
                    test(context);
                    ((IObjectContextAdapter)context).ObjectContext.Connection.Close();
                }
            }
        }

        #endregion
    }
}