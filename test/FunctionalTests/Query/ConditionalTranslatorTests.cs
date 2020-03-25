// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using Xunit;

    public class ConditionalTranslatorTests : FunctionalTestBase
    {
        private class Context : DbContext
        {
            public Context()
            {
                Database.SetInitializer(new ContextInitializer());
            }

            public DbSet<Customer> Customers { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Address>().HasRequired(a => a.Customer).WithOptional(c => c.Address);
            }
        }

        public class Customer
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }
            public string FirstName { get; set; }
            public Address Address { get; set; }
        }

        public class Address
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }            
            public string Street { get; set; }
            public Customer Customer { get; set; }
        }

        public class CustomerDto
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public AddressDto Address { get; set; }
        }

        public class AddressDto
        {
            public string Street { get; set; }
        }

        private class ContextInitializer : DropCreateDatabaseIfModelChanges<Context>
        {
            protected override void Seed(Context context)
            {
                var c1 = new Customer { Id = 1, FirstName = "Bill", Address = new Address { Id = 1, Street = "21st" } };
                var c2 = new Customer { Id = 2, FirstName = "John", Address = null };

                context.Customers.AddRange(new[] { c1, c2 });
                context.SaveChanges();
            }
        }

        [Fact]
        public void CodePlex826_Dto_member_can_be_assigned_to_result_of_conditional_operator_with_null_then()
        {
            using (var context = new Context())
            {
                var customers = context.Customers.Select(
                    customer =>
                        new CustomerDto
                        {
                            Id = customer.Id,
                            FirstName = customer.FirstName,
                            Address =
                                (customer.Address == null)
                                    ? null
                                    : new AddressDto { Street = customer.Address.Street }
                        }).AsEnumerable();

                Assert.Equal(2, customers.Count());

                var c1 = customers.ElementAt(0);
                var c2 = customers.ElementAt(1);

                Assert.Equal(c1.FirstName, "Bill");
                Assert.Equal(c1.Address.Street, "21st");
                Assert.Equal(c2.FirstName, "John");
                Assert.Null(c2.Address);
            }
        }

        [Fact]
        public void CodePlex826_Dto_member_can_be_assigned_to_result_of_conditional_operator_with_null_else()
        {
            using (var context = new Context())
            {
                var customers = context.Customers.Select(
                    customer =>
                        new CustomerDto
                        {
                            Id = customer.Id,
                            FirstName = customer.FirstName,
                            Address =
                                (customer.Address != null)
                                    ? new AddressDto { Street = customer.Address.Street }
                                    : null
                        }).AsEnumerable();

                Assert.Equal(2, customers.Count());

                var c1 = customers.ElementAt(0);
                var c2 = customers.ElementAt(1);

                Assert.Equal(c1.FirstName, "Bill");
                Assert.Equal(c1.Address.Street, "21st");
                Assert.Equal(c2.FirstName, "John");
                Assert.Null(c2.Address);
            }
        }
    }
}
