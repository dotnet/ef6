// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace SimpleModel
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;

    public class SimpleModelForLinqInitializer : DropCreateDatabaseIfModelChanges<SimpleModelForLinq>
    {
        protected override void Seed(SimpleModelForLinq context)
        {
            var numbers = new List<NumberForLinq>
                {
                    new NumberForLinq(5, "Five"),
                    new NumberForLinq(4, "Four"),
                    new NumberForLinq(1, "One"),
                    new NumberForLinq(3, "Three"),
                    new NumberForLinq(9, "Nine"),
                    new NumberForLinq(8, "Eight"),
                    new NumberForLinq(6, "Six"),
                    new NumberForLinq(7, "Seven"),
                    new NumberForLinq(2, "Two"),
                    new NumberForLinq(0, "Zero"),
                };

            foreach (var number in numbers)
            {
                context.Numbers.Add(number);
            }

            var products = new List<ProductForLinq>
                {
                    new ProductForLinq
                        {
                            ProductName = "Chai",
                            Category = "Beverages",
                            UnitPrice = 18.0000M,
                            UnitsInStock = 39
                        },
                    new ProductForLinq
                        {
                            ProductName = "Chang",
                            Category = "Beverages",
                            UnitPrice = 19.0000M,
                            UnitsInStock = 17
                        },
                    new ProductForLinq
                        {
                            ProductName = "Aniseed Syrup",
                            Category = "Condiments",
                            UnitPrice = 10.0000M,
                            UnitsInStock = 13
                        },
                    new ProductForLinq
                        {
                            ProductName = "Chef Anton's Cajun Seasoning",
                            Category = "Condiments",
                            UnitPrice = 22.0000M,
                            UnitsInStock = 53
                        },
                    new ProductForLinq
                        {
                            ProductName = "Chef Anton's Gumbo Mix",
                            Category = "Condiments",
                            UnitPrice = 21.3500M,
                            UnitsInStock = 0
                        },
                    new ProductForLinq
                        {
                            ProductName = "Grandma's Boysenberry Spread",
                            Category = "Condiments",
                            UnitPrice = 25.0000M,
                            UnitsInStock = 120
                        },
                    new ProductForLinq
                        {
                            ProductName = "Uncle Bob's Organic Dried Pears",
                            Category = "Produce",
                            UnitPrice = 30.0000M,
                            UnitsInStock = 15
                        },
                    new FeaturedProductForLinq
                        {
                            ProductName = "Northwoods Cranberry Sauce",
                            Category = "Condiments",
                            UnitPrice = 40.0000M,
                            UnitsInStock = 6
                        },
                    new ProductForLinq
                        {
                            ProductName = "Mishi Kobe Niku",
                            Category = "Meat/Poultry",
                            UnitPrice = 97.0000M,
                            UnitsInStock = 29
                        },
                    new ProductForLinq
                        {
                            ProductName = "Ikura",
                            Category = "Seafood",
                            UnitPrice = 31.0000M,
                            UnitsInStock = 31
                        },
                    new ProductForLinq
                        {
                            ProductName = "Queso Cabrales",
                            Category = "Dairy Products",
                            UnitPrice = 21.0000M,
                            UnitsInStock = 22
                        },
                    new FeaturedProductForLinq
                        {
                            ProductName = "Queso Manchego La Pastora",
                            Category = "Dairy Products",
                            UnitPrice = 38.0000M,
                            UnitsInStock = 86
                        },
                    new ProductForLinq
                        {
                            ProductName = "Konbu",
                            Category = "Seafood",
                            UnitPrice = 6.0000M,
                            UnitsInStock = 24
                        },
                    new ProductForLinq
                        {
                            ProductName = "Tofu",
                            Category = "Produce",
                            UnitPrice = 23.2500M,
                            UnitsInStock = 35
                        },
                    new ProductForLinq
                        {
                            ProductName = "Genen Shouyu",
                            Category = "Condiments",
                            UnitPrice = 15.5000M,
                            UnitsInStock = 39
                        },
                    new ProductForLinq
                        {
                            ProductName = "Pavlova",
                            Category = "Confections",
                            UnitPrice = 17.4500M,
                            UnitsInStock = 29
                        },
                    new FeaturedProductForLinq
                        {
                            ProductName = "Alice Mutton",
                            Category = "Meat/Poultry",
                            UnitPrice = 39.0000M,
                            UnitsInStock = 0
                        },
                    new FeaturedProductForLinq
                        {
                            ProductName = "Carnarvon Tigers",
                            Category = "Seafood",
                            UnitPrice = 62.5000M,
                            UnitsInStock = 42
                        },
                    new ProductForLinq
                        {
                            ProductName = "Teatime Chocolate Biscuits",
                            Category = "Confections",
                            UnitPrice = 9.2000M,
                            UnitsInStock = 25
                        },
                    new ProductForLinq
                        {
                            ProductName = "Sir Rodney's Marmalade",
                            Category = "Confections",
                            UnitPrice = 81.0000M,
                            UnitsInStock = 40
                        },
                    new ProductForLinq
                        {
                            ProductName = "Sir Rodney's Scones",
                            Category = "Confections",
                            UnitPrice = 10.0000M,
                            UnitsInStock = 3
                        },
                };

            foreach (var product in products)
            {
                context.Products.Add(product);
            }

            var customers = new List<CustomerForLinq>
                                {
                                    new CustomerForLinq
                                        {
                                            Region = "WA",
                                            CompanyName = "Microsoft"
                                        },
                                    new CustomerForLinq
                                        {
                                            Region = "WA",
                                            CompanyName = "NewMonics"
                                        },
                                    new CustomerForLinq
                                        {
                                            Region = "OR",
                                            CompanyName = "NewMonics"
                                        },
                                    new CustomerForLinq
                                        {
                                            Region = "CA",
                                            CompanyName = "Microsoft"
                                        },
                                };

            foreach (var customer in customers)
            {
                context.Customers.Add(customer);
            }

            var orders = new List<OrderForLinq>
                {
                    new OrderForLinq
                        {
                            Total = 111M,
                            OrderDate = new DateTime(1997, 9, 3),
                            Customer = customers[0]
                        },
                    new OrderForLinq
                        {
                            Total = 222M,
                            OrderDate = new DateTime(2006, 9, 3),
                            Customer = customers[1]
                        },
                    new OrderForLinq
                        {
                            Total = 333M,
                            OrderDate = new DateTime(1999, 9, 3),
                            Customer = customers[0]
                        },
                    new OrderForLinq
                        {
                            Total = 444M,
                            OrderDate = new DateTime(2010, 9, 3),
                            Customer = customers[1]
                        },
                    new OrderForLinq
                        {
                            Total = 2555M,
                            OrderDate = new DateTime(2009, 9, 3),
                            Customer = customers[2]
                        },
                    new OrderForLinq
                        {
                            Total = 6555M,
                            OrderDate = new DateTime(1976, 9, 3),
                            Customer = customers[3]
                        },
                    new OrderForLinq
                        {
                            Total = 555M,
                            OrderDate = new DateTime(1985, 9, 3),
                            Customer = customers[2]
                        },
                };

            foreach (var order in orders)
            {
                context.Orders.Add(order);
            }
        }
    }
}
