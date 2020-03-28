// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace SimpleModel
{
    using System.Data.Entity;
    using System.Linq;

    /// <summary>
    /// A simple context for use in DbQuery LINQ testing.
    /// </summary>
    public class SimpleModelForLinq : DbContext
    {
        static SimpleModelForLinq()
        {
            Database.SetInitializer(new SimpleModelForLinqInitializer());
        }

        public DbSet<NumberForLinq> Numbers { get; set; }
        public DbSet<ProductForLinq> Products { get; set; }
        public DbSet<CustomerForLinq> Customers { get; set; }
        public DbSet<OrderForLinq> Orders { get; set; }

        public IQueryable<NumberForLinq> NumbersGreaterThanTen()
        {
            return Numbers.Where(n => n.Value > 10);
        }

        public IQueryable<ProductForLinq> ProductsStartingWithP
        {
            get { return Products.Where(p => p.ProductName.StartsWith("P")); }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NumberForLinq>().HasKey(e => e.Id);
            modelBuilder.Entity<ProductForLinq>().HasKey(e => e.Id);
            modelBuilder.Entity<CustomerForLinq>().HasKey(e => e.Id);
            modelBuilder.Entity<OrderForLinq>().HasKey(e => e.Id);
        }
    }
}
