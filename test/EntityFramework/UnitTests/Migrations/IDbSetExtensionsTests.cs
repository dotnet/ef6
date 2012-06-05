namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Linq.Expressions;
    using Moq;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    public class DbSetExtensionsTests : DbTestCase
    {
        public class FakeDbSet<TEntity>
            where TEntity : class
        {
            public virtual void AddOrUpdate(params TEntity[] entities)
            {
            }

            public virtual void AddOrUpdate(Expression<Func<TEntity, object>> identifierExpression, params TEntity[] entities)
            {
            }
        }

        [MigrationsTheory]
        public void Can_dispatch_to_custom_implementation_one_arg()
        {
            var fakeSetMock = new Mock<FakeDbSet<MigrationsCustomer>>();
            var dbSetMock = fakeSetMock.As<IDbSet<MigrationsCustomer>>();

            var customer = new MigrationsCustomer();

            dbSetMock.Object.AddOrUpdate(new[] { customer });

            fakeSetMock.Verify(f => f.AddOrUpdate(customer), Times.AtMostOnce());
        }

        [MigrationsTheory]
        public void Can_dispatch_to_custom_implementation_two_args()
        {
            var fakeSetMock = new Mock<FakeDbSet<MigrationsCustomer>>();
            var dbSetMock = fakeSetMock.As<IDbSet<MigrationsCustomer>>();

            var customer = new MigrationsCustomer();

            Expression<Func<MigrationsCustomer, object>> identifierExpression = c => c.CustomerNumber;

            dbSetMock.Object.AddOrUpdate(identifierExpression, new[] { customer });

            fakeSetMock.Verify(f => f.AddOrUpdate(identifierExpression, customer), Times.AtMostOnce());
        }

        [MigrationsTheory]
        public void Dispatch_when_no_target_method_should_throw_one_arg()
        {
            var mock = new Mock<IDbSet<MigrationsCustomer>>();

            Assert.Equal(Strings.UnableToDispatchAddOrUpdate(mock.Object.GetType()), Assert.Throws<InvalidOperationException>(() => mock.Object.AddOrUpdate(new[] { new MigrationsCustomer() })).Message);
        }

        [MigrationsTheory]
        public void Dispatch_when_no_target_method_should_throw_two_args()
        {
            var mock = new Mock<IDbSet<MigrationsCustomer>>();

            Assert.Equal(Strings.UnableToDispatchAddOrUpdate(mock.Object.GetType()), Assert.Throws<InvalidOperationException>(() => mock.Object.AddOrUpdate(c => c.CustomerNumber, new[] { new MigrationsCustomer() })).Message);
        }

        [MigrationsTheory]
        public void AddOrUpdate_should_validate_preconditions()
        {
            using (var context = CreateContext<ShopContext_v1>())
            {
                Assert.Equal("identifierExpression", Assert.Throws<ArgumentNullException>(() => context.Customers.AddOrUpdate((Expression<Func<MigrationsCustomer, object>>)null, new MigrationsCustomer())).ParamName);

                Assert.Equal("entities", Assert.Throws<ArgumentNullException>(() => context.Customers.AddOrUpdate(c => c.Name, (MigrationsCustomer[])null)).ParamName);
            }
        }

        private class AddOrUpdateInitContext : DbContext
        {
            public DbSet<MigrationsProduct> Products { get; set; }
        }

        [MigrationsTheory]
        public void AddOrUpdate_when_first_call_to_context_should_trigger_correct_initialization()
        {
            using (var context = new AddOrUpdateInitContext())
            {
                context.Products.AddOrUpdate(new MigrationsProduct { ProductId = 1, Name = "Foo", Sku = "Sku1" });

                context.SaveChanges();
            }

            using (var context = new AddOrUpdateInitContext())
            {
                Assert.Equal(1, context.Products.Count());
            }
        }

        [MigrationsTheory]
        public void AddOrUpdate_should_be_able_to_add_new_data_by_key()
        {
            ResetDatabase();

            CreateMigrator<ShopContext_v1>().Update();

            using (var context = CreateContext<ShopContext_v1>())
            {
                context.Customers.AddOrUpdate
                    (
                        new MigrationsCustomer { FullName = "Andrew Peters", CustomerNumber = 123 },
                        new MigrationsCustomer { FullName = "Brice Lambson", CustomerNumber = 456 },
                        new MigrationsCustomer { FullName = "Rowan Miller", CustomerNumber = 789 }
                    );

                context.SaveChanges();
            }

            using (var context = CreateContext<ShopContext_v1>())
            {
                Assert.Equal(3, context.Customers.Count());
            }
        }

        [MigrationsTheory]
        public void AddOrUpdate_should_be_able_to_update_existing_data_by_key()
        {
            ResetDatabase();

            CreateMigrator<ShopContext_v1>().Update();

            using (var context = CreateContext<ShopContext_v1>())
            {
                context.Products.AddOrUpdate
                    (
                        new MigrationsProduct { ProductId = 1, Name = "Foo", Sku = "Sku1" },
                        new MigrationsProduct { ProductId = 2, Name = "Bar", Sku = "Sku2" }
                    );

                context.SaveChanges();
            }

            using (var context = CreateContext<ShopContext_v1>())
            {
                Assert.Equal(2, context.Products.Count());

                context.Products.AddOrUpdate
                    (
                        new MigrationsProduct { ProductId = 1, Name = "Foo Updated", Sku = "Sku1" },
                        new MigrationsProduct { ProductId = 2, Name = "Bar", Sku = "Sku2" }
                    );

                context.SaveChanges();
            }

            using (var context = CreateContext<ShopContext_v1>())
            {
                Assert.Equal(2, context.Products.Count());
                Assert.Equal("Foo Updated", context.Products.Single(p => p.ProductId == 1).Name);
                Assert.Equal("Bar", context.Products.Single(p => p.ProductId == 2).Name);
            }
        }

        [MigrationsTheory]
        public void AddOrUpdate_should_be_able_to_add_new_data_by_custom_identifier()
        {
            ResetDatabase();

            CreateMigrator<ShopContext_v1>().Update();

            using (var context = CreateContext<ShopContext_v1>())
            {
                context.Customers.AddOrUpdate
                    (
                        c => new { c.FullName, c.Name },
                        new MigrationsCustomer { FullName = "Andrew Peters", CustomerNumber = 123 },
                        new MigrationsCustomer { FullName = "Brice Lambson", CustomerNumber = 456 },
                        new MigrationsCustomer { FullName = "Rowan Miller", CustomerNumber = 789 }
                    );

                context.SaveChanges();
            }

            using (var context = CreateContext<ShopContext_v1>())
            {
                Assert.Equal(3, context.Customers.Count());
            }
        }

        [MigrationsTheory]
        public void AddOrUpdate_should_be_able_to_update_existing_data_by_custom_identifier()
        {
            ResetDatabase();

            CreateMigrator<ShopContext_v1>().Update();

            using (var context = CreateContext<ShopContext_v1>())
            {
                context.Customers.AddOrUpdate
                    (
                        new MigrationsCustomer { FullName = "Andrew Peters", Name = "Andrew", CustomerNumber = 123 },
                        new MigrationsCustomer { FullName = "Andrew Peters", Name = "Andrew2", CustomerNumber = 123 }
                    );

                context.SaveChanges();
            }

            using (var context = CreateContext<ShopContext_v1>())
            {
                Assert.Equal(2, context.Customers.Count());

                context.Customers.AddOrUpdate
                    (
                        c => new { c.FullName, c.Name },
                        new MigrationsCustomer { FullName = "Andrew Peters", Name = "Andrew", CustomerNumber = 456 }
                    );

                context.SaveChanges();
            }

            using (var context = CreateContext<ShopContext_v1>())
            {
                Assert.Equal(2, context.Customers.Count());
                Assert.Equal(
                    456L,
                    context.Customers
                        .Single(c => c.FullName == "Andrew Peters" && c.Name == "Andrew").CustomerNumber);
                Assert.Equal(
                    123,
                    context.Customers
                        .Single(c => c.FullName == "Andrew Peters" && c.Name == "Andrew2").CustomerNumber);
            }
        }
    }
}
