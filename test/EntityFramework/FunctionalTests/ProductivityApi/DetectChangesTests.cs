// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.TestHelpers;
    using System.Linq;
    using AdvancedPatternsModel;
    using Xunit;

    public class DetectChangesTests : FunctionalTestBase
    {
        #region DetectChanges is O(1) when there is nothing to do (Dev11 113588)

        [Fact]
        public void Change_tracking_of_mutated_complex_types_happens_correctly_for_proxies()
        {
            using (var context = new ProxiesContext())
            {
                var product = context.Products.Create();
                product.Id = 1;
                product.Name = "Marmite";
                product.Properties = new ProductProperties
                                         {
                                             SpecialOfferCode = "YEAST",
                                             StockCount = 77,
                                             UnitCost = 1.99m
                                         };

                context.Products.Attach(product);

                product.Properties.StockCount = 75;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(EntityState.Modified, context.Entry(product).State);
                Assert.True(context.Entry(product).Property(p => p.Properties).IsModified);
            }
        }

        [Fact]
        public void DetectChanges_does_not_consider_change_tracking_proxies_with_no_complex_types()
        {
            using (var context = new ProxiesContext())
            {
                for (var i = 0; i < 2; i++)
                {
                    var product = context.Products.Create();
                    product.Id = i;
                    product.Name = "Marmite";
                    product.Properties = new ProductProperties
                                             {
                                                 SpecialOfferCode = "YEAST",
                                                 StockCount = 77,
                                                 UnitCost = 1.99m
                                             };

                    var category = context.Categories.Create();
                    category.Id = i;
                    category.Name = "Foods";
                    category.Products.Add(product);

                    context.Categories.Attach(category);
                }

                ProxyCategory.ReadCount = 0;
                ProxyProduct.ReadCount = 0;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(0, ProxyCategory.ReadCount);
                Assert.Equal(2, ProxyProduct.ReadCount);
            }
        }

        public class ProxiesContext : DbContext
        {
            public ProxiesContext()
            {
                Database.SetInitializer<ProxiesContext>(null);
            }

            public DbSet<ProxyProduct> Products { get; set; }
            public DbSet<ProxyCategory> Categories { get; set; }
        }

        public class ProxyProduct
        {
            private string _name;

            public virtual int Id { get; set; }

            public virtual string Name
            {
                get
                {
                    ReadCount++;
                    return _name;
                }

                set { _name = value; }
            }

            public virtual ProductProperties Properties { get; set; }
            public virtual ProxyCategory Category { get; set; }

            public static int ReadCount { get; set; }
        }

        public class ProxyCategory
        {
            private string _name;

            public virtual int Id { get; set; }

            public virtual string Name
            {
                get
                {
                    ReadCount++;
                    return _name;
                }

                set { _name = value; }
            }

            public virtual ICollection<ProxyProduct> Products { get; set; }

            public static int ReadCount { get; set; }
        }

        public class ProductProperties
        {
            public virtual decimal UnitCost { get; set; }
            public virtual int StockCount { get; set; }
            public virtual string SpecialOfferCode { get; set; }
        }

        #endregion

        #region Detect changes for complex types with null original values (Dev11 36323)

        [Fact]
        public void Calling_DetectChanges_with_nested_complex_property_null_after_Attach_with_change_to_another_property_should_not_throw()
        {
            using (var context = new Context36323())
            {
                var entry = context.Entry(
                    new Model36323
                        {
                            Id = 14,
                            Contact = new ContactInfo36323
                                          {
                                              First = "Name",
                                              HomePhone = new Phone36323
                                                              {
                                                                  Number = "12345"
                                                              }
                                          }
                        });
                entry.State = EntityState.Unchanged;

                entry.Entity.Contact.WorkPhone = new Phone36323
                                                     {
                                                         Number = "1234"
                                                     };

                context.ChangeTracker.DetectChanges();

                Assert.NotNull(entry.Property(m => m.Contact).OriginalValue);
                Assert.NotNull(entry.Property(m => m.Contact).CurrentValue);

                Assert.NotNull(entry.ComplexProperty(m => m.Contact).Property(c => c.HomePhone).OriginalValue);
                Assert.NotNull(entry.ComplexProperty(m => m.Contact).Property(c => c.HomePhone).CurrentValue);
                Assert.Equal(
                    "12345",
                    entry.ComplexProperty(m => m.Contact).Property(c => c.HomePhone).OriginalValue.Number);
                Assert.Equal(
                    "12345",
                    entry.ComplexProperty(m => m.Contact).Property(c => c.HomePhone).CurrentValue.Number);

                Assert.Null(entry.ComplexProperty(m => m.Contact).Property(c => c.WorkPhone).OriginalValue);
                Assert.NotNull(entry.ComplexProperty(m => m.Contact).Property(c => c.WorkPhone).CurrentValue);
                Assert.Equal(
                    "1234",
                    entry.ComplexProperty(m => m.Contact).Property(c => c.WorkPhone).CurrentValue.Number);
            }
        }

        [Fact]
        public void
            Setting_a_nested_complex_property_original_value_onto_a_complex_property_that_was_originally_null_throws()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var entry = context.Entry(CreateBuilding(nullAddress: true));
                entry.State = EntityState.Unchanged;

                entry.Entity.Address = CreateAddress(nullSiteInfo: true);
                context.ChangeTracker.DetectChanges();

                Assert.Null(entry.Property(b => b.Address).OriginalValue);

                Assert.Throws<InvalidOperationException>(
                    () =>
                    entry.ComplexProperty(b => b.Address).Property(a => a.SiteInfo).OriginalValue = CreateSiteInfo()).
                    ValidateMessage(
                        "DbPropertyValues_CannotSetPropertyOnNullOriginalValue", typeof(SiteInfo).Name,
                        typeof(Address).Name);
            }
        }

        [Fact]
        public void Setting_a_nested_complex_property_current_value_onto_a_complex_property_that_is_null_throws()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var entry = context.Entry(CreateBuilding(nullAddress: true));
                entry.State = EntityState.Unchanged;

                Assert.Throws<InvalidOperationException>(
                    () =>
                    entry.ComplexProperty(b => b.Address).Property(a => a.SiteInfo).CurrentValue = CreateSiteInfo()).
                    ValidateMessage(
                        "DbPropertyValues_CannotSetPropertyOnNullCurrentValue", typeof(SiteInfo).Name,
                        typeof(Address).Name);
            }
        }

        [Fact]
        public void Setting_a_nested_complex_property_original_value_onto_a_complex_property_that_is_null_throws()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var entry = context.Entry(CreateBuilding(nullAddress: true));
                entry.State = EntityState.Unchanged;

                Assert.Throws<InvalidOperationException>(
                    () =>
                    entry.ComplexProperty(b => b.Address).Property(a => a.SiteInfo).OriginalValue = CreateSiteInfo()).
                    ValidateMessage(
                        "DbPropertyValues_CannotSetPropertyOnNullOriginalValue", typeof(SiteInfo).Name,
                        typeof(Address).Name);
            }
        }

        [Fact]
        public void Setting_a_nested_complex_property_current_value_onto_a_complex_property_that_is_null_for_a_Deleted_entity_throws()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var entry = context.Entry(CreateBuilding(nullAddress: true));
                entry.State = EntityState.Deleted;

                Assert.Throws<InvalidOperationException>(
                    () =>
                    entry.ComplexProperty(b => b.Address).Property(a => a.SiteInfo).CurrentValue = CreateSiteInfo()).
                    ValidateMessage(
                        "DbPropertyValues_CannotSetPropertyOnNullCurrentValue", typeof(SiteInfo).Name,
                        typeof(Address).Name);
            }
        }

        [Fact]
        public void Setting_a_nested_complex_property_current_value_onto_a_complex_property_that_is_null_for_a_Detached_entity_throws()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var entry = context.Entry(CreateBuilding(nullAddress: true));

                Assert.Throws<InvalidOperationException>(
                    () =>
                    entry.ComplexProperty(b => b.Address).Property(a => a.SiteInfo).CurrentValue = CreateSiteInfo()).
                    ValidateMessage(
                        "DbPropertyValues_CannotSetPropertyOnNullCurrentValue", typeof(SiteInfo).Name,
                        typeof(Address).Name);
            }
        }

        [Fact]
        public void
            Calling_DetectChanges_twice_for_complex_type_that_is_null_should_work_and_original_value_should_be_null()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var entry = context.Entry(CreateBuilding(nullAddress: true));
                entry.State = EntityState.Unchanged;

                context.ChangeTracker.DetectChanges();
                context.ChangeTracker.DetectChanges();

                Assert.Null(entry.Property(b => b.Address).OriginalValue);
                Assert.Null(entry.Property(b => b.Address).CurrentValue);

                Assert.Null(entry.ComplexProperty(b => b.Address).Property(a => a.SiteInfo).OriginalValue);
                Assert.Null(entry.ComplexProperty(b => b.Address).Property(a => a.SiteInfo).CurrentValue);
            }
        }

        [Fact]
        public void Calling_DetectChanges_twice_for_nested_complex_type_that_is_null_should_work_and_original_value_should_be_null()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var entry = context.Entry(CreateBuilding(nullAddress: false, nullSiteInfo: true));
                entry.State = EntityState.Unchanged;

                context.ChangeTracker.DetectChanges();
                context.ChangeTracker.DetectChanges();

                Assert.NotNull(entry.Property(b => b.Address).OriginalValue);
                Assert.NotNull(entry.Property(b => b.Address).CurrentValue);
                Assert.Equal("Donkey Boulevard", entry.Property(b => b.Address).OriginalValue.Street);
                Assert.Equal("Donkey Boulevard", entry.Property(b => b.Address).CurrentValue.Street);

                Assert.Null(entry.ComplexProperty(b => b.Address).Property(a => a.SiteInfo).OriginalValue);
                Assert.Null(entry.ComplexProperty(b => b.Address).Property(a => a.SiteInfo).CurrentValue);
            }
        }

        [Fact]
        public void
            Calling_DetectChanges_twice_for_complex_type_that_was_null_but_is_no_longer_null_should_work_and_original_value_should_be_null()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var entry = context.Entry(CreateBuilding(nullAddress: true));
                entry.State = EntityState.Unchanged;

                context.ChangeTracker.DetectChanges();

                entry.Entity.Address = CreateAddress(nullSiteInfo: true);

                context.ChangeTracker.DetectChanges();
                context.ChangeTracker.DetectChanges();

                Assert.Null(entry.Property(b => b.Address).OriginalValue);
                Assert.NotNull(entry.Property(b => b.Address).CurrentValue);
                Assert.Equal("Donkey Boulevard", entry.Property(b => b.Address).CurrentValue.Street);

                Assert.Null(entry.ComplexProperty(b => b.Address).Property(a => a.SiteInfo).OriginalValue);
                Assert.Null(entry.ComplexProperty(b => b.Address).Property(a => a.SiteInfo).CurrentValue);
            }
        }

        [Fact]
        public void
            Calling_DetectChanges_twice_for_nested_complex_type_that_was_null_but_is_no_longer_null_should_work_and_original_value_should_be_null
            ()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var entry = context.Entry(CreateBuilding(nullAddress: false, nullSiteInfo: true));
                entry.State = EntityState.Unchanged;

                context.ChangeTracker.DetectChanges();

                entry.Entity.Address.SiteInfo = CreateSiteInfo();

                context.ChangeTracker.DetectChanges();
                context.ChangeTracker.DetectChanges();

                Assert.NotNull(entry.Property(b => b.Address).OriginalValue);
                Assert.NotNull(entry.Property(b => b.Address).CurrentValue);
                Assert.Equal("Donkey Boulevard", entry.Property(b => b.Address).OriginalValue.Street);
                Assert.Equal("Donkey Boulevard", entry.Property(b => b.Address).CurrentValue.Street);

                Assert.Null(entry.ComplexProperty(b => b.Address).Property(a => a.SiteInfo).OriginalValue);
                Assert.NotNull(entry.ComplexProperty(b => b.Address).Property(a => a.SiteInfo).CurrentValue);
                Assert.Equal(18, entry.ComplexProperty(b => b.Address).Property(a => a.SiteInfo).CurrentValue.Zone);
            }
        }

        public class Context36323 : DbContext
        {
            public Context36323()
            {
                Database.SetInitializer(new DropCreateDatabaseIfModelChanges<Context36323>());
            }

            public DbSet<Model36323> Models { get; set; }
        }

        public class Model36323
        {
            public int Id { get; set; }
            public string Foo { get; set; }
            public ContactInfo36323 Contact { get; set; }
        }

        [ComplexType]
        public class ContactInfo36323
        {
            public string First { get; set; }
            public string Last { get; set; }

            public Phone36323 HomePhone { get; set; }
            public Phone36323 WorkPhone { get; set; }
        }

        [ComplexType]
        public class Phone36323
        {
            public string Number { get; set; }
        }

        private Building CreateBuilding(bool nullAddress, bool nullSiteInfo = true)
        {
            return new Building
                       {
                           Name = "Unicorn Mannor",
                           Address = nullAddress ? null : CreateAddress(nullSiteInfo),
                       };
        }

        private Address CreateAddress(bool nullSiteInfo)
        {
            return new Address
                       {
                           Street = "Donkey Boulevard",
                           City = "Working Horse City",
                           State = "WA",
                           ZipCode = "98052",
                           County = "KING",
                           SiteInfo = nullSiteInfo ? null : CreateSiteInfo(),
                       };
        }

        private SiteInfo CreateSiteInfo()
        {
            return new SiteInfo
                       {
                           Zone = 18,
                           Environment = "Dungy"
                       };
        }

        #endregion

        [Fact] // CodePlex 1278
        public void DetectChanges_correctly_detects_change_of_complex_object_instance_on_change_tracking_proxy()
        {
            using (var context = new Context1278())
            {
                context.Configuration.AutoDetectChangesEnabled = false;

                var entity = context.Entities.Create();
                entity.ComplexProperty = new ComplexType1278();

                context.Entities.Attach(entity);

                entity.ComplexProperty = new ComplexType1278();

                Assert.True(context.Entry(entity).Property(e => e.ComplexProperty).IsModified);

                context.ChangeTracker.DetectChanges();

                Assert.True(context.Entry(entity).Property(e => e.ComplexProperty).IsModified);
            }
        }

        [Fact]
        public void DetectChanges_correctly_detects_change_of_complex_object_instance_on_non_proxy()
        {
            using (var context = new Context1278())
            {
                context.Configuration.AutoDetectChangesEnabled = false;

                var entity = new Entity1278 { ComplexProperty = new ComplexType1278() };

                context.Entities.Attach(entity);

                entity.ComplexProperty = new ComplexType1278();

                Assert.False(context.Entry(entity).Property(e => e.ComplexProperty).IsModified);

                context.ChangeTracker.DetectChanges();

                Assert.True(context.Entry(entity).Property(e => e.ComplexProperty).IsModified);
            }
        }

        [ComplexType]
        public class ComplexType1278
        {
            public int Value1 { get; set; }
            public int? Value2 { get; set; }
        }

        public class Entity1278
        {
            public virtual int Id { get; set; }
            public virtual ComplexType1278 ComplexProperty { get; set; }
        }

        public class Context1278 : DbContext
        {
            static Context1278()
            {
                Database.SetInitializer<Context1278>(null);
            }

            public virtual DbSet<Entity1278> Entities { get; set; }
        }

        [Fact] // CodePlex 663
        [UseDefaultExecutionStrategy]
        public void DetectChanges_can_be_called_twice_for_nullable_key_with_related_entities()
        {
            ExtendedSqlAzureExecutionStrategy.ExecuteNew(
                () =>
                {
                    using (var context = new BlogContext663())
                    {
                        context.Configuration.AutoDetectChangesEnabled = false;
                        context.Database.Initialize(force: false);

                        using (context.Database.BeginTransaction())
                        {
                            var blog1 = context.Blogs.Add(new Blog663());
                            var blog2 = context.Blogs.Add(new Blog663());

                            blog1.Posts = new List<Post663> { new Post663() };
                            blog2.Posts = new List<Post663> { new Post663() };

                            context.ChangeTracker.DetectChanges();
                            context.ChangeTracker.DetectChanges();

                            Assert.Equal(EntityState.Added, context.Entry(blog1).State);
                            Assert.Equal(EntityState.Added, context.Entry(blog2).State);
                            Assert.Equal(EntityState.Added, context.Entry(blog1.Posts.First()).State);
                            Assert.Equal(EntityState.Added, context.Entry(blog2.Posts.First()).State);

                            context.SaveChanges();

                            Assert.Equal(EntityState.Unchanged, context.Entry(blog1).State);
                            Assert.Equal(EntityState.Unchanged, context.Entry(blog2).State);
                            Assert.Equal(EntityState.Unchanged, context.Entry(blog1.Posts.First()).State);
                            Assert.Equal(EntityState.Unchanged, context.Entry(blog2.Posts.First()).State);

                            Assert.NotNull(blog1.Id);
                            Assert.NotNull(blog2.Id);
                            Assert.Equal(blog1.Id, blog1.Posts.First().BlogId);
                            Assert.Equal(blog2.Id, blog2.Posts.First().BlogId);
                        }
                    }
                });
        }

        [Fact] // CodePlex 663
        [UseDefaultExecutionStrategy]
        public void DetectChanges_can_be_called_twice_for_nullable_composite_key_with_related_entities()
        {
            ExtendedSqlAzureExecutionStrategy.ExecuteNew(
                () =>
                {
                    using (var context = new BlogContext663())
                    {
                        context.Configuration.AutoDetectChangesEnabled = false;
                        context.Database.Initialize(force: false);

                        using (context.Database.BeginTransaction())
                        {
                            var blog1 = context.CompositeBlogs.Add(new CompositeBlog663());
                            var blog2 = context.CompositeBlogs.Add(new CompositeBlog663());

                            blog1.Posts = new List<CompositePost663> { new CompositePost663() };
                            blog2.Posts = new List<CompositePost663> { new CompositePost663() };

                            context.ChangeTracker.DetectChanges();
                            context.ChangeTracker.DetectChanges();

                            Assert.Equal(EntityState.Added, context.Entry(blog1).State);
                            Assert.Equal(EntityState.Added, context.Entry(blog2).State);
                            Assert.Equal(EntityState.Added, context.Entry(blog1.Posts.First()).State);
                            Assert.Equal(EntityState.Added, context.Entry(blog2.Posts.First()).State);

                            blog1.Id1 = 1;
                            blog1.Id2 = 2;
                            blog2.Id1 = 1;
                            blog2.Id2 = 3;

                            context.ChangeTracker.DetectChanges();
                            context.SaveChanges();

                            Assert.Equal(EntityState.Unchanged, context.Entry(blog1).State);
                            Assert.Equal(EntityState.Unchanged, context.Entry(blog2).State);
                            Assert.Equal(EntityState.Unchanged, context.Entry(blog1.Posts.First()).State);
                            Assert.Equal(EntityState.Unchanged, context.Entry(blog2.Posts.First()).State);

                            Assert.NotNull(blog1.Id1);
                            Assert.NotNull(blog1.Id2);
                            Assert.NotNull(blog2.Id1);
                            Assert.NotNull(blog2.Id2);
                            Assert.Equal(blog1.Id1, blog1.Posts.First().BlogId1);
                            Assert.Equal(blog1.Id2, blog1.Posts.First().BlogId2);
                            Assert.Equal(blog2.Id1, blog2.Posts.First().BlogId1);
                            Assert.Equal(blog2.Id2, blog2.Posts.First().BlogId2);
                        }
                    }
                });
        }

        public class BlogContext663 : DbContext
        {
            static BlogContext663()
            {
                Database.SetInitializer(new DropCreateDatabaseIfModelChanges<BlogContext663>());
            }

            public DbSet<Blog663> Blogs { get; set; }
            public DbSet<Post663> Posts { get; set; }
            public DbSet<CompositeBlog663> CompositeBlogs { get; set; }
            public DbSet<CompositePost663> CompositePosts { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<CompositeBlog663>()
                    .HasKey(e => new { e.Id1, e.Id2 })
                    .HasMany(e => e.Posts)
                    .WithOptional(e => e.Blog)
                    .HasForeignKey(e => new { e.BlogId1, e.BlogId2 });
            }
        }

        public class Blog663
        {
            public int? Id { get; set; }
            public ICollection<Post663> Posts { get; set; }
        }

        public class Post663
        {
            public int? Id { get; set; }

            public int? BlogId { get; set; }
            public Blog663 Blog { get; set; }
        }

        public class CompositeBlog663
        {
            public int? Id1 { get; set; }
            public int? Id2 { get; set; }
            
            public ICollection<CompositePost663> Posts { get; set; }
        }

        public class CompositePost663
        {
            public int? Id { get; set; }

            public int? BlogId1 { get; set; }
            public int? BlogId2 { get; set; }
            public CompositeBlog663 Blog { get; set; }
        }

        [Fact] // CodePlex 2606
        public void DetectChanges_sets_FKs_if_entity_added_to_two_navigations_on_both_ends()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Transaction>();
            modelBuilder.Entity<TransactionDetail>();
            modelBuilder.Entity<Note>();

            using (var context = new DbContext("Context2606", modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo).Compile()))
            {
                new DropCreateDatabaseIfModelChanges<DbContext>().InitializeDatabase(context);

                var note = new Note();
                context.Set<Note>().Add(note);
                context.Entry(note).State = EntityState.Unchanged;

                var transaction = new Transaction();
                context.Set<Transaction>().Add(transaction);

                var transactionDetail = new TransactionDetail();
                transactionDetail.Transaction = transaction;
                transactionDetail.Notes.Add(note);
                transaction.TransactionDetails.Add(transactionDetail);
                note.TransactionDetail = transactionDetail;

                Assert.Null(transactionDetail.TransactionId);
                Assert.Null(note.TransactionDetailId);

                context.ChangeTracker.DetectChanges();

                Assert.NotNull(transactionDetail.TransactionId);
                Assert.NotNull(note.TransactionDetailId);
                Assert.Equal(EntityState.Modified, context.Entry(note).State);
            }
        }

        public class Transaction
        {
            public Transaction()
            {
                TransactionDetails = new HashSet<TransactionDetail>();
            }
            public int TransactionId { get; set; }
            public virtual ICollection<TransactionDetail> TransactionDetails { get; set; }
        }

        public class TransactionDetail
        {
            public TransactionDetail()
            {
                Notes = new HashSet<Note>();
            }

            public int TransactionDetailId { get; set; }
            public int? TransactionId { get; set; }
            public virtual ICollection<Note> Notes { get; set; }
            public virtual Transaction Transaction { get; set; }
        }

        public class Note
        {
            public int NoteId { get; set; }
            public string Description { get; set; }
            public int? TransactionDetailId { get; set; }
            public virtual TransactionDetail TransactionDetail { get; set; }
        }
    }
}
