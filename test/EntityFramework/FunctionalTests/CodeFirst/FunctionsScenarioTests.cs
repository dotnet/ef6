// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.CodeFirst
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using FunctionalTests;
    using Xunit;
    using Xunit.Extensions;

    public class FunctionsScenarioTests
    {
        public class EndToEnd : EndToEndFunctionsTest
        {
            [Fact]
            [AutoRollback]
            public void Can_insert_update_and_delete_when_generated_property()
            {
                using (var context = CreateContext())
                {
                    var order = new Order
                    {
                        Type = "Foo"
                    };

                    Assert.Equal(0, context.Set<Order>().Count());

                    // Insert
                    context.Set<Order>().Add(order);
                    context.SaveChanges();

                    Assert.Equal(1, context.Set<Order>().Count());
                    Assert.NotNull(context.Set<Order>().Select(ol => ol.Version).First());

                    // Update
                    order.Type = "Bar";
                    context.SaveChanges();

                    // Delete
                    context.Set<Order>().Remove(order);
                    context.SaveChanges();

                    Assert.Equal(0, context.Set<OrderLine>().Count());
                }
            }

            [Fact]
            [AutoRollback]
            public void Can_insert_update_and_delete_when_tph_inheritance()
            {
                using (var context = CreateContext())
                {
                    var customer = new SpecialCustomer();

                    Assert.Equal(0, context.Set<SpecialCustomer>().Count());

                    // Insert
                    context.Set<SpecialCustomer>().Add(customer);
                    context.SaveChanges();

                    Assert.Equal(1, context.Set<SpecialCustomer>().Count());

                    // Update
                    customer.Points = 1;
                    context.SaveChanges();

                    Assert.Equal(1, context.Set<SpecialCustomer>().Select(c => c.Points).First());

                    // Delete
                    context.Set<SpecialCustomer>().Remove(customer);
                    context.SaveChanges();

                    Assert.Equal(0, context.Set<SpecialCustomer>().Count());
                }
            }

            [Fact]
            [AutoRollback]
            public void Can_insert_and_delete_when_many_to_many()
            {
                using (var context = CreateContext())
                {
                    var tag = new FunctionalTests.FunctionsScenarioTests.ModificationFunctions.Tag
                    {
                        Products = new List<FunctionalTests.FunctionsScenarioTests.ModificationFunctions.ProductA>
                        {
                            new FunctionalTests.FunctionsScenarioTests.ModificationFunctions.ProductA()
                        }
                    };

                    Assert.Equal(0, context.Set<FunctionalTests.FunctionsScenarioTests.ModificationFunctions.Tag>().SelectMany(t => t.Products).Count());

                    // Insert
                    context.Set<FunctionalTests.FunctionsScenarioTests.ModificationFunctions.Tag>().Add(tag);
                    context.SaveChanges();

                    Assert.Equal(1, context.Set<FunctionalTests.FunctionsScenarioTests.ModificationFunctions.Tag>().SelectMany(t => t.Products).Count());

                    // Delete
                    tag.Products.Clear();
                    context.SaveChanges();

                    Assert.Equal(0, context.Set<FunctionalTests.FunctionsScenarioTests.ModificationFunctions.Tag>().SelectMany(t => t.Products).Count());
                }
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Order>();
                modelBuilder.Entity<MigrationsCustomer>();
                modelBuilder.Entity<FunctionalTests.FunctionsScenarioTests.ModificationFunctions.Tag>()
                    .HasMany(t => t.Products)
                    .WithMany(p => p.Tags)
                    .Map(m => m.ToTable("TagProductAs"))
                    .MapToStoredProcedures();
            }
        }

        public class EndToEntWithTPT : EndToEndFunctionsTest
        {
            [Fact]
            [AutoRollback]
            public void Can_insert_update_and_delete_when_tpt_inheritance()
            {
                using (var context = CreateContext())
                {
                    var customer = new SpecialCustomer();

                    Assert.Equal(0, context.Set<SpecialCustomer>().Count());

                    // Insert
                    context.Set<SpecialCustomer>().Add(customer);
                    context.SaveChanges();

                    Assert.Equal(1, context.Set<SpecialCustomer>().Count());

                    // Update
                    customer.Points = 1;
                    context.SaveChanges();

                    Assert.Equal(1, context.Set<SpecialCustomer>().Select(c => c.Points).First());

                    // Delete
                    context.Set<SpecialCustomer>().Remove(customer);
                    context.SaveChanges();

                    Assert.Equal(0, context.Set<SpecialCustomer>().Count());
                }
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Ignore<Order>();
                modelBuilder.Entity<MigrationsCustomer>()
                    .Map(m => m.ToTable("MigrationsCustomers"))
                    .Map<SpecialCustomer>(m => m.ToTable("SpecialCustomers"))
                    .Map<GoldCustomer>(m => m.ToTable("GoldCustomers"));
            }
        }

        public class EndToEntWithTPC : EndToEndFunctionsTest
        {
            [Fact]
            [AutoRollback]
            public void Can_insert_update_and_delete_when_tpt_inheritance()
            {
                using (var context = CreateContext())
                {
                    var customer = new SpecialCustomer();

                    Assert.Equal(0, context.Set<SpecialCustomer>().Count());

                    // Insert
                    context.Set<SpecialCustomer>().Add(customer);
                    context.SaveChanges();

                    Assert.Equal(1, context.Set<SpecialCustomer>().Count());

                    // Update
                    customer.Points = 1;
                    context.SaveChanges();

                    Assert.Equal(1, context.Set<SpecialCustomer>().Select(c => c.Points).First());

                    // Delete
                    context.Set<SpecialCustomer>().Remove(customer);
                    context.SaveChanges();

                    Assert.Equal(0, context.Set<SpecialCustomer>().Count());
                }
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Ignore<Order>();
                modelBuilder.Entity<MigrationsCustomer>()
                    .Map(
                        m =>
                        {
                            m.MapInheritedProperties();
                            m.ToTable("MigrationsCustomers");
                        })
                    .Map<SpecialCustomer>(
                        m =>
                        {
                            m.MapInheritedProperties();
                            m.ToTable("SpecialCustomers");
                        })
                    .Map<GoldCustomer>(
                        m =>
                        {
                            m.MapInheritedProperties();
                            m.ToTable("GoldCustomers");
                        });
            }
        }
    }
}
