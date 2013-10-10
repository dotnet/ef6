// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ProductivityApi
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using Xunit;

    public class GetHashCodeTests
    {
        #region Test model

        public class GetHashCodeContext : DbContext
        {
            public GetHashCodeContext()
                : base("GetHashCodeContext")
            {
            }

            static GetHashCodeContext()
            {
                Database.SetInitializer(new GetHashCodeInitializer());
            }

            public DbSet<GetHashCodeProduct> Products { get; set; }
            public DbSet<GetHashCodeSku> Skus { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<GetHashCodeSku>().HasRequired(o => o.Product).WithMany(o => o.Skus)
                            .HasForeignKey(o => o.ProductId);
            }
        }

        public class GetHashCodeInitializer : DropCreateDatabaseAlways<GetHashCodeContext>
        {
            protected override void Seed(GetHashCodeContext context)
            {
                var product = context.Products.Add(
                    new GetHashCodeProduct
                        {
                            Id = "ALFKI",
                            SupplierId = 14,
                            Details = new GetHashCodeProductDetails
                                {
                                    Name = "Squiggle",
                                    Price = 124
                                },
                        });

                context.Skus.Add(
                    new GetHashCodeSku
                        {
                            Name = "Standard Edition",
                            Product = product,
                            ProductId = product.Id
                        });

                context.Skus.Add(
                    new GetHashCodeSku
                        {
                            Name = "Ultimate Edition",
                            Product = product,
                            ProductId = product.Id
                        });
            }
        }

        [Serializable]
        public class GetHashCodeProduct
        {
            public virtual string Id { get; set; }
            public virtual int SupplierId { get; set; }
            public virtual GetHashCodeProductDetails Details { get; set; }
            public virtual HashSet<GetHashCodeSku> Skus { get; set; }
            public virtual ICollection<GetHashCodeSku> DeprecatedSkus { get; set; }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public override bool Equals(object obj)
            {
                throw new NotImplementedException();
            }
        }

        [Serializable]
        public class GetHashCodeProductDetails
        {
            public string Name { get; set; }
            public int Price { get; set; }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public override bool Equals(object obj)
            {
                throw new NotImplementedException();
            }
        }

        [Serializable]
        public class GetHashCodeSku
        {
            public virtual int Id { get; set; }
            public virtual string Name { get; set; }
            public virtual string ProductId { get; set; }
            public virtual GetHashCodeProduct Product { get; set; }
            public virtual GetHashCodeSku Parent { get; set; }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public override bool Equals(object obj)
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        [Fact]
        public void Local_on_an_entitySet_of_entities_that_override_Equals_shouldnt_throw()
        {
            using (var context = new GetHashCodeContext())
            {
                context.Skus.Add(
                    new GetHashCodeSku
                        {
                            Name = "Student"
                        });
                context.Skus.Add(
                    new GetHashCodeSku
                        {
                            Name = "Home"
                        });

                Assert.Equal(2, context.Skus.Local.Count());
            }
        }

        [Fact]
        public void Removing_from_Local_entities_that_override_Equals_shouldnt_throw()
        {
            using (var context = new GetHashCodeContext())
            {
                context.Skus.Load();
                Assert.Equal(2, context.Skus.Local.Count());

                foreach (var sku in context.Skus.Local.ToList())
                {
                    context.Skus.Remove(sku);
                }

                Assert.Equal(0, context.Skus.Local.Count());
            }
        }

        [Fact]
        public void Join_query_on_a_collection_of_entities_that_override_GetHashCode_shouldnt_throw()
        {
            using (var db = new GetHashCodeContext())
            {
                var query = from p in db.Products
                            join s in db.Skus on p equals s.Product into ps
                            select new
                                {
                                    Product = p,
                                    Foo = ps
                                };

                Assert.Equal(1, query.Count());
            }
        }

        [Fact]
        public void Changing_the_value_of_a_property_on_an_entity_that_overrides_GetHashCode_shouldnt_throw()
        {
            using (var context = new GetHashCodeContext())
            {
                var product = context.Products.Single(p => p.Id == "ALFKI");
                product.SupplierId = 23;

                ((IObjectContextAdapter)context).ObjectContext.DetectChanges();

                Assert.Equal(EntityState.Modified, context.Entry(product).State);
            }
        }

        [Fact]
        public void Changing_the_value_of_a_complex_property_on_an_entity_that_overrides_GetHashCode_shouldnt_throw()
        {
            using (var context = new GetHashCodeContext())
            {
                var product = context.Products.Single(p => p.Id == "ALFKI");
                product.Details.Name = "New Name";

                ((IObjectContextAdapter)context).ObjectContext.DetectChanges();

                Assert.Equal(EntityState.Modified, context.Entry(product).State);
            }
        }

        public void Changing_the_complex_property_on_an_entity_that_overrides_Equals_shouldnt_throw()
        {
            using (var context = new GetHashCodeContext())
            {
                var product = context.Products.Single(p => p.Id == "ALFKI");
                var entry = context.Entry(product);
                entry.ComplexProperty(b => b.Details).CurrentValue = new GetHashCodeProductDetails
                    {
                        Name = "New Details"
                    };

                ((IObjectContextAdapter)context).ObjectContext.DetectChanges();

                Assert.Equal(EntityState.Modified, context.Entry(product).State);
                Assert.Equal("New Details", product.Details.Name);
            }
        }

        [Fact]
        public void Loading_related_entity_that_overrides_GetHashCode_shouldnt_throw()
        {
            using (var context = new GetHashCodeContext())
            {
                Assert.NotNull(context.Products.First().Skus.First());
            }
        }

        [Fact]
        public void Attach_and_Remove_on_an_entity_that_overrides_GetHashCode_shouldnt_shouldnt_throw()
        {
            using (var context = new GetHashCodeContext())
            {
                var product = context.Products.First();

                var sku = new GetHashCodeSku
                    {
                        Name = "Sprinkled",
                        ProductId = product.Id
                    };

                product.DeprecatedSkus.Add(sku);
                context.Products.Attach(product);
                context.Products.Remove(product);
            }
        }

        [Fact]
        public void Serializing_an_entity_that_overrides_GetHashCode_shouldnt_shouldnt_throw()
        {
            using (var context = new GetHashCodeContext())
            {
                var skuProxy = context.Skus.Create();
                skuProxy.Parent = context.Skus.Create();

                var stream = new MemoryStream();
                var formatter = new BinaryFormatter();

                formatter.Serialize(stream, skuProxy);
            }
        }
    }
}
