// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Internal;
    using Xunit;
    using Console = System.Console;

    public class CudSqlGeneratorTests
    {
        public class Customer
        {
            public int Id { get; set; }

            [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
            public decimal CustomerNo { get; set; }

            public string Name { get; set; }

            public CustomerType Type { get; set; }
        }

        public class GoldCustomer : Customer
        {
            public string GoldName { get; set; }
        }

        public class CustomerType
        {
            public int Id { get; set; }
        }

        public class MyContext : DbContext
        {
            static MyContext()
            {
                Database.SetInitializer<MyContext>(null);
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>().MapToStoredProcedures();
                modelBuilder.Entity<GoldCustomer>().ToTable("Gold");
            }
        }

        [Fact]
        public void First()
        {
            using (var context = new MyContext())
            {
                var goldCustomer = new GoldCustomer
                                       {
                                           Name = "Foo"
                                       };
                //goldCustomer.Type = new CustomerType();

                context.Set<Customer>().Add(goldCustomer);
                context.Entry(goldCustomer).State = EntityState.Modified;

//                using (var commandTracer = new CommandTracer(context))
//                {
//                    context.SaveChanges();

//                    foreach (var interceptedCommand in commandTracer.DbCommands)
//                    {
//                        //Console.WriteLine(interceptedCommand.CommandText);
//                    }
               // }
            }
        }
    }
}
