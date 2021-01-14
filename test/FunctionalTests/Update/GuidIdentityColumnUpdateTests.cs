// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Update
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.TestHelpers;
    using System.Linq;
    using System.Transactions;
    using Xunit;

    public class GuidIdentityColumnUpdateTests : FunctionalTestBase
    {
        public GuidIdentityColumnUpdateTests()
        {
            using (var context = new GuidIdentityColumnContext())
            {
                context.Customers.ToList();
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Verify_insert_update_delete_for_guid_identity_column()
        {
            ExtendedSqlAzureExecutionStrategy.ExecuteNew(
                () =>
                {
                    using (new TransactionScope())
                    {
                        using (var context = new GuidIdentityColumnContext())
                        {
                            var customer = context.Customers.Single();
                            var orders = customer.Orders.ToList();
                            orders[0].Name = "Changed Name";
                            context.Orders.Remove(orders[1]);
                            context.SaveChanges();
                        }

                        using (var context = new GuidIdentityColumnContext())
                        {
                            var customer = context.Customers.Single();
                            var orders = customer.Orders;

                            Assert.Equal(1, orders.Count());
                            Assert.Equal("Changed Name", orders.Single().Name);
                        }
                    }
                });
        }

        public class Customer
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            virtual public ICollection<Order> Orders { get; set; }
        }

        public class Order
        {
            public int Id { get; set; }
            public string Name { get; set; }
            virtual public Customer Customer { get; set; }
        }

        public class  GuidIdentityColumnContext : DbContext
        {
            static GuidIdentityColumnContext()
            {
                Database.SetInitializer(new DatabaseGeneratedGuidInitializer());
            }

            public DbSet<Customer> Customers { get; set; }
            public DbSet<Order> Orders { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>().Property(c => c.Id)
                    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            }
        }

        public class DatabaseGeneratedGuidInitializer : DropCreateDatabaseIfModelChanges<GuidIdentityColumnContext>
        {
            protected override void Seed(GuidIdentityColumnContext context)
            {
                var order1 = new Order { Name = "Order1" };
                var order2 = new Order { Name = "Order2" };
                
                var customer = new Customer
                    {
                        Name = "Customer1",
                        Orders = new [] { order1, order2 },
                    };

                context.Customers.Add(customer);
                context.SaveChanges();
            }
        }
    }
}
