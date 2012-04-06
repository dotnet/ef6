namespace ProductivityApiTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core;
    using System.Data;
    using System.Data.Entity;
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
                                     { SpecialOfferCode = "YEAST", StockCount = 77, UnitCost = 1.99m };

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
                for (int i = 0; i < 2; i++)
                {
                    var product = context.Products.Create();
                    product.Id = i;
                    product.Name = "Marmite";
                    product.Properties = new ProductProperties
                                         { SpecialOfferCode = "YEAST", StockCount = 77, UnitCost = 1.99m };

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

        #endregion

        #region Detect changes for complex types with null original values (Dev11 36323)

        [Fact]
        public void Calling_DetectChanges_with_nested_complex_property_null_after_Attach_with_change_to_another_property_should_not_throw()
        {
            using (var context = new Context36323())
            {
                var entry = context.Entry(new Model36323
                                          {
                                              Id = 14,
                                              Contact = new ContactInfo36323
                                                        {
                                                            First = "Name",
                                                            HomePhone = new Phone36323 { Number = "12345" }
                                                        }
                                          });
                entry.State = EntityState.Unchanged;

                entry.Entity.Contact.WorkPhone = new Phone36323 { Number = "1234" };

                context.ChangeTracker.DetectChanges();

                Assert.NotNull(entry.Property(m => m.Contact).OriginalValue);
                Assert.NotNull(entry.Property(m => m.Contact).CurrentValue);

                Assert.NotNull(entry.ComplexProperty(m => m.Contact).Property(c => c.HomePhone).OriginalValue);
                Assert.NotNull(entry.ComplexProperty(m => m.Contact).Property(c => c.HomePhone).CurrentValue);
                Assert.Equal("12345",
                             entry.ComplexProperty(m => m.Contact).Property(c => c.HomePhone).OriginalValue.Number);
                Assert.Equal("12345",
                             entry.ComplexProperty(m => m.Contact).Property(c => c.HomePhone).CurrentValue.Number);

                Assert.Null(entry.ComplexProperty(m => m.Contact).Property(c => c.WorkPhone).OriginalValue);
                Assert.NotNull(entry.ComplexProperty(m => m.Contact).Property(c => c.WorkPhone).CurrentValue);
                Assert.Equal("1234",
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
                    ValidateMessage("DbPropertyValues_CannotSetPropertyOnNullOriginalValue", typeof(SiteInfo).Name,
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
                    ValidateMessage("DbPropertyValues_CannotSetPropertyOnNullCurrentValue", typeof(SiteInfo).Name,
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
                    ValidateMessage("DbPropertyValues_CannotSetPropertyOnNullOriginalValue", typeof(SiteInfo).Name,
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
                    ValidateMessage("DbPropertyValues_CannotSetPropertyOnNullCurrentValue", typeof(SiteInfo).Name,
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
                    ValidateMessage("DbPropertyValues_CannotSetPropertyOnNullCurrentValue", typeof(SiteInfo).Name,
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
        public void Calling_DetectChanges_twice_for_complex_type_that_was_null_but_is_no_longer_null_should_work_and_original_value_should_be_null()
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
        public void Calling_DetectChanges_twice_for_nested_complex_type_that_was_null_but_is_no_longer_null_should_work_and_original_value_should_be_null()
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
            return new SiteInfo { Zone = 18, Environment = "Dungy" };
        }

        #endregion
    }

    #region Change tracking proxy model with some complex types

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

    #region Model for Dev11 36323

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

    #endregion
}