// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace SimpleModel
{
    using System.Collections.Generic;
    using System.Data.Entity;

    public class SimpleLocalDbModelInitializer : DropCreateDatabaseAlways<SimpleLocalDbModelContext>
    {
        protected override void Seed(SimpleLocalDbModelContext context)
        {
            var products = new List<Product>
                {
                    new Product
                        {
                            Name = "Marmite",
                            CategoryId = "Foods"
                        },
                    new Product
                        {
                            Name = "Bovril",
                            CategoryId = "Beverages"
                        },
                    new Product
                        {
                            Name = "iSnack 2.0",
                            CategoryId = "Foods"
                        },
                    new Product
                        {
                            Name = "Irn-Bru",
                            CategoryId = "Beverages"
                        },
                    new Product
                        {
                            Name = "Ibuprofen",
                            CategoryId = "Medications"
                        },
                    new Product
                        {
                            Name = "Strongbow",
                            CategoryId = "Medications"
                        },
                    new FeaturedProduct
                        {
                            Name = "Cadillac",
                            CategoryId = "Cars",
                            PromotionalCode = "Ed Wood"
                        },
                };

            foreach (var product in products)
            {
                context.Products.Add(product);
            }

            var categories = new List<Category>
                {
                    new Category
                        {
                            Id = "Beverages"
                        },
                    new Category
                        {
                            Id = "Foods"
                        },
                    new Category
                        {
                            Id = "Medications"
                        },
                    new Category
                        {
                            Id = "Cars"
                        },
                };

            foreach (var category in categories)
            {
                context.Categories.Add(category);
            }
        }
    }
}
