namespace ProductivityApiTests
{
    using System;
    using System.Data.Entity.Core;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.IO;
    using System.Linq;
    using System.Transactions;
    using SimpleModel;
    using Xunit;

    public class SimpleScenariosForLocalDb : FunctionalTestBase, IDisposable
    {
        #region Infrastructure/setup

        private readonly IDbConnectionFactory _previousConnectionFactory;
        private readonly object _previousDataDirectory;

        public SimpleScenariosForLocalDb()
        {
            _previousConnectionFactory = Database.DefaultConnectionFactory;
            _previousDataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory");

            AppDomain.CurrentDomain.SetData("DataDirectory", Path.GetTempPath());
            Database.DefaultConnectionFactory = new LocalDbConnectionFactory("v11.0");
        }

        public void Dispose()
        {
            // Ensure LocalDb databases are deleted after use so that LocalDb doesn't throw if
            // the temp location in which they are stored is later cleaned.
            using (var context = new SimpleLocalDbModelContext())
            {
                context.Database.Delete();
            }
            using (var context = new LocalDbLoginsContext())
            {
                context.Database.Delete();
            }
            Database.Delete("Scenario_CodeFirstWithModelBuilder");
            Database.Delete("Scenario_Use_AppConfig_LocalDb_connection_string");

            Database.DefaultConnectionFactory = _previousConnectionFactory;
            AppDomain.CurrentDomain.SetData("DataDirectory", _previousDataDirectory);
        }

        #endregion

        #region Scenarios for SQL Server LocalDb using LocalDbConnectionFactory

        [Fact]
        public void Scenario_Find()
        {
            using (var context = new SimpleLocalDbModelContext())
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

                Assert.Equal(@"(localdb)\v11.0", context.Database.Connection.DataSource);
            }
        }

        [Fact]
        public void Scenario_Insert()
        {
            EnsureDatabaseInitialized(() => new SimpleLocalDbModelContext());

            using (new TransactionScope())
            {
                using (var context = new SimpleLocalDbModelContext())
                {
                    var product = new Product() { Name = "Vegemite" };
                    context.Products.Add(product);
                    context.SaveChanges();

                    // Scenario ends; simple validation of final state follows
                    Assert.NotEqual(0, product.Id);
                    Assert.Equal(EntityState.Unchanged, GetStateEntry(context, product).State);

                    Assert.Equal(@"(localdb)\v11.0", context.Database.Connection.DataSource);
                }
            }
        }

        [Fact]
        public void Scenario_Update()
        {
            EnsureDatabaseInitialized(() => new SimpleLocalDbModelContext());

            using (new TransactionScope())
            {
                using (var context = new SimpleLocalDbModelContext())
                {
                    var product = context.Products.Find(1);
                    product.Name = "iSnack 2.0";
                    context.SaveChanges();

                    // Scenario ends; simple validation of final state follows
                    Assert.Equal("iSnack 2.0", product.Name);
                    Assert.Equal(EntityState.Unchanged, GetStateEntry(context, product).State);

                    Assert.Equal(@"(localdb)\v11.0", context.Database.Connection.DataSource);
                }
            }
        }

        [Fact]
        public void Scenario_Query()
        {
            using (var context = new SimpleLocalDbModelContext())
            {
                var products = context.Products.ToList();

                // Scenario ends; simple validation of final state follows
                Assert.Equal(7, products.Count);
                Assert.True(products.TrueForAll(p => GetStateEntry(context, p).State == EntityState.Unchanged));

                Assert.Equal(@"(localdb)\v11.0", context.Database.Connection.DataSource);
            }
        }

        [Fact]
        public void Scenario_Relate_using_query()
        {
            EnsureDatabaseInitialized(() => new SimpleLocalDbModelContext());

            using (new TransactionScope())
            {
                using (var context = new SimpleLocalDbModelContext())
                {
                    var category = context.Categories.Find("Foods");
                    var product = new Product() { Name = "Bovril", Category = category };
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

                    Assert.Equal(@"(localdb)\v11.0", context.Database.Connection.DataSource);
                }
            }
        }

        [Fact]
        public void Scenario_Relate_using_FK()
        {
            EnsureDatabaseInitialized(() => new SimpleLocalDbModelContext());

            using (new TransactionScope())
            {
                using (var context = new SimpleLocalDbModelContext())
                {
                    var product = new Product() { Name = "Bovril", CategoryId = "Foods" };
                    context.Products.Add(product);
                    context.SaveChanges();

                    // Scenario ends; simple validation of final state follows
                    Assert.NotNull(product);
                    Assert.Equal(EntityState.Unchanged, GetStateEntry(context, product).State);
                    Assert.Equal("Foods", product.CategoryId);

                    Assert.Equal(@"(localdb)\v11.0", context.Database.Connection.DataSource);
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

            using (var context = new SimpleLocalDbModelContextWithNoData("Scenario_CodeFirstWithModelBuilder", model))
            {
                InsertIntoCleanContext(context);
            }

            using (var context = new SimpleLocalDbModelContextWithNoData("Scenario_CodeFirstWithModelBuilder", model))
            {
                ValidateFromCleanContext(context);
            }
        }

        private void ValidateFromCleanContext(SimpleLocalDbModelContextWithNoData context)
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

            Assert.Equal(@"(localdb)\v11.0", context.Database.Connection.DataSource);
        }

        private void InsertIntoCleanContext(SimpleLocalDbModelContextWithNoData context)
        {
            context.Categories.Add(new Category() { Id = "Large Hadron Collider" });
            context.Products.Add(new Product() { Name = "Higgs Boson", CategoryId = "Large Hadron Collider" });
            context.SaveChanges();

            Assert.Equal(@"(localdb)\v11.0", context.Database.Connection.DataSource);
        }

        [Fact]
        public void Scenario_Using_two_databases()
        {
            EnsureDatabaseInitialized(() => new LocalDbLoginsContext());
            EnsureDatabaseInitialized(() => new SimpleLocalDbModelContext());

            using (new TransactionScope())
            {
                using (var context = new LocalDbLoginsContext())
                {
                    var login = new Login() { Id = Guid.NewGuid(), Username = "elmo" };
                    context.Logins.Add(login);
                    context.SaveChanges();

                    // Scenario ends; simple validation of final state follows
                    Assert.Same(login, context.Logins.Find(login.Id));
                    Assert.Equal(EntityState.Unchanged, GetStateEntry(context, login).State);

                    Assert.Equal(@"(localdb)\v11.0", context.Database.Connection.DataSource);
                }
            }

            using (new TransactionScope())
            {
                using (var context = new SimpleLocalDbModelContext())
                {
                    var category = new Category() { Id = "Books" };
                    var product = new Product() { Name = "The Unbearable Lightness of Being", Category = category };
                    context.Products.Add(product);
                    context.SaveChanges();

                    // Scenario ends; simple validation of final state follows
                    Assert.Equal(EntityState.Unchanged, GetStateEntry(context, product).State);
                    Assert.Equal(EntityState.Unchanged, GetStateEntry(context, category).State);
                    Assert.Equal("Books", product.CategoryId);
                    Assert.Same(category, product.Category);
                    Assert.True(category.Products.Contains(product));

                    Assert.Equal(@"(localdb)\v11.0", context.Database.Connection.DataSource);
                }
            }
        }

        [Fact]
        public void Scenario_Use_AppConfig_connection_string()
        {
            Database.Delete("Scenario_Use_AppConfig_LocalDb_connection_string");

            using (var context = new SimpleLocalDbModelContextWithNoData("Scenario_Use_AppConfig_LocalDb_connection_string"))
            {
                Assert.Equal("Scenario_Use_AppConfig_LocalDb", context.Database.Connection.Database);
                InsertIntoCleanContext(context);
            }

            using (var context = new SimpleLocalDbModelContextWithNoData("Scenario_Use_AppConfig_LocalDb_connection_string"))
            {
                ValidateFromCleanContext(context);
            }
        }

        [Fact]
        public void Scenario_Include()
        {
            using (var context = new SimpleLocalDbModelContext())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var products = context.Products.Where(p => p != null).Include("Category").ToList();

                foreach (var product in products)
                {
                    Assert.NotNull(product.Category);
                }

                Assert.Equal(@"(localdb)\v11.0", context.Database.Connection.DataSource);
            }
        }

        [Fact]
        public void Scenario_IncludeWithLambda()
        {
            using (var context = new SimpleLocalDbModelContext())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var products = context.Products.Where(p => p != null).Include(p => p.Category).ToList();

                foreach (var product in products)
                {
                    Assert.NotNull(product.Category);
                }

                Assert.Equal(@"(localdb)\v11.0", context.Database.Connection.DataSource);
            }
        }

        #endregion
    }
}