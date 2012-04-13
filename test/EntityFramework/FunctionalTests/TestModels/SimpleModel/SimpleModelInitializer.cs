namespace SimpleModel
{
    using System.Collections.Generic;
    using System.Data.Entity;

    public class SimpleModelInitializer : DropCreateDatabaseIfModelChanges<SimpleModelContext>
    {
        protected override void Seed(SimpleModelContext context)
        {
            new List<Product>
            {
                new Product { Name = "Marmite", CategoryId = "Foods" },
                new Product { Name = "Bovril", CategoryId = "Beverages" },
                new Product { Name = "iSnack 2.0", CategoryId = "Foods" },
                new Product { Name = "Irn-Bru", CategoryId = "Beverages" },
                new Product { Name = "Ibuprofen", CategoryId = "Medications" },
                new Product { Name = "Strongbow", CategoryId = "Medications" },
                new FeaturedProduct { Name = "Cadillac", CategoryId = "Cars", PromotionalCode = "Ed Wood" },
            }.ForEach(p => context.Products.Add(p));

            new List<Category>
            {
                new Category { Id = "Beverages" },
                new Category { Id = "Foods" },
                new Category { Id = "Medications" },
                new Category { Id = "Cars" },
            }.ForEach(c => context.Categories.Add(c));
        }
    }
}