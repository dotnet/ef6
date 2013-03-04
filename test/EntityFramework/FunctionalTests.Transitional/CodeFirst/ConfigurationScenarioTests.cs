// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration;
    using System.Linq;
    using FunctionalTests.Model;
    using Xunit;

    public class ConfigurationScenarioTests : TestBase
    {
        [Fact]
        public void Can_set_store_type_with_column_annotation_on_base_property()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<BaseEntity_155894>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<BaseEntity_155894>(e => e.Property1).DbEqual("timestamp", c => c.TypeName);
        }

        public abstract class BaseEntity_155894
        {
            public int Id { get; set; }

            [Column(TypeName = "timestamp")]
            public byte[] Property1 { get; set; }
        }

        public class DerivedEntity_155894 : BaseEntity_155894
        {
            public string Property2 { get; set; }
        }

        public void TestCompositeKeyOrder(
            Action<DbModelBuilder> configure, string[] expectedPropertyOrder,
            string[] expectedColumnOrder)
        {
            var modelBuilder = new DbModelBuilder();

            configure(modelBuilder);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.True(
                expectedPropertyOrder.SequenceEqual(
                    databaseMapping.Model.EntityTypes.Single().DeclaredKeyProperties.Select(
                        p => p.Name)));

            databaseMapping.Assert<CompositeKeyNoOrder>("CompositeKeyNoOrders").HasColumns(expectedColumnOrder);
        }

        [Fact]
        public void Composite_key_should_result_in_correct_order_when_key_and_order_configured_using_api()
        {
            TestCompositeKeyOrder(
                modelBuilder =>
                    {
                        modelBuilder.Entity<CompositeKeyNoOrder>().Property(c => c.Id2).HasColumnOrder(1);
                        modelBuilder.Entity<CompositeKeyNoOrder>().Property(c => c.Id1).HasColumnOrder(2);
                        modelBuilder.Entity<CompositeKeyNoOrder>().HasKey(
                            c => new
                                     {
                                         c.Id1,
                                         c.Id2
                                     });
                    },
                new[] { "Id1", "Id2" },
                new[] { "Id2", "Id1" });
        }

        [Fact]
        public void Composite_key_should_result_in_correct_order_when_key_configured_using_api()
        {
            TestCompositeKeyOrder(
                modelBuilder => modelBuilder.Entity<CompositeKeyNoOrder>().HasKey(
                    c => new
                             {
                                 c.Id2,
                                 c.Id1
                             }),
                new[] { "Id2", "Id1" },
                new[] { "Id2", "Id1" });
        }

        [Fact]
        public void Composite_key_should_result_in_correct_order_when_order_configured_using_api()
        {
            TestCompositeKeyOrder(
                modelBuilder =>
                    {
                        modelBuilder.Entity<CompositeKeyNoOrder>().Property(c => c.Id2).HasColumnOrder(1);
                        modelBuilder.Entity<CompositeKeyNoOrder>().Property(c => c.Id1).HasColumnOrder(2);
                    },
                new[] { "Id2", "Id1" },
                new[] { "Id2", "Id1" });
        }

        [Fact]
        public void Composite_key_should_result_in_correct_order_when_key_and_order_configured_using_configuration()
        {
            TestCompositeKeyOrder(
                modelBuilder =>
                    {
                        var configuration = new EntityTypeConfiguration<CompositeKeyNoOrder>();

                        configuration.Property(c => c.Id2).HasColumnOrder(1);
                        configuration.Property(c => c.Id1).HasColumnOrder(2);
                        configuration.HasKey(
                            c => new
                                     {
                                         c.Id1,
                                         c.Id2
                                     });

                        modelBuilder.Configurations.Add(configuration);
                    },
                new[] { "Id1", "Id2" },
                new[] { "Id2", "Id1" });
        }

        [Fact]
        public void Composite_key_should_result_in_correct_order_when_key_configured_using_configuration()
        {
            TestCompositeKeyOrder(
                modelBuilder =>
                    {
                        var configuration = new EntityTypeConfiguration<CompositeKeyNoOrder>();

                        configuration.HasKey(
                            c => new
                                     {
                                         c.Id2,
                                         c.Id1
                                     });

                        modelBuilder.Configurations.Add(configuration);
                    },
                new[] { "Id2", "Id1" },
                new[] { "Id2", "Id1" });
        }

        [Fact]
        public void Composite_key_should_result_in_correct_order_when_order_configured_using_configuration()
        {
            TestCompositeKeyOrder(
                modelBuilder =>
                    {
                        var configuration = new EntityTypeConfiguration<CompositeKeyNoOrder>();

                        configuration.Property(c => c.Id2).HasColumnOrder(1);
                        configuration.Property(c => c.Id1).HasColumnOrder(2);

                        modelBuilder.Configurations.Add(configuration);
                    },
                new[] { "Id2", "Id1" },
                new[] { "Id2", "Id1" });
        }

        public class CompositeKeyNoOrder
        {
            [Key]
            public int Id1 { get; set; }

            [Key]
            public int Id2 { get; set; }
        }

        [Fact]
        public void Composite_key_should_throw_when_no_order_configured()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<CompositeKeyNoOrder>();

            Assert.Throws<InvalidOperationException>(
                () => BuildMapping(modelBuilder))
                .ValidateMessage("ModelGeneration_UnableToDetermineKeyOrder", typeof(CompositeKeyNoOrder));
        }

        [Fact]
        public void HasEntitySetName_configured_using_api()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<UnitMeasure>().HasKey(u => u.UnitMeasureCode);
            modelBuilder.Entity<UnitMeasure>().HasEntitySetName("Units");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal("Units", databaseMapping.Model.Containers.Single().EntitySets.Single().Name);
        }

        [Fact]
        public void HasEntitySetName_configured_using_configuration()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            var configuration = new EntityTypeConfiguration<UnitMeasure>();

            configuration.HasKey(u => u.UnitMeasureCode);
            configuration.HasEntitySetName("Units");

            modelBuilder.Configurations.Add(configuration);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal("Units", databaseMapping.Model.Containers.Single().EntitySets.Single().Name);
        }

        [Fact]
        public void External_duplicate_association_configuration()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Configurations
                .Add(new ProductCategoryConfiguration())
                .Add(new ProductSubcategoryConfiguration());

            BuildMapping(modelBuilder);
        }

        private class ProductCategoryConfiguration : EntityTypeConfiguration<ProductCategory>
        {
            public ProductCategoryConfiguration()
            {
                HasMany(p => p.ProductSubcategories)
                    .WithRequired(s => s.ProductCategory)
                    .HasForeignKey(s => s.ProductCategoryID);
            }
        }

        private class ProductSubcategoryConfiguration : EntityTypeConfiguration<ProductSubcategory>
        {
            public ProductSubcategoryConfiguration()
            {
                HasRequired(s => s.ProductCategory)
                    .WithMany(p => p.ProductSubcategories)
                    .HasForeignKey(s => s.ProductCategoryID);
            }
        }

        [Fact]
        public void Can_call_Entity_after_adding_custom_configuration_class_during_OnModelCreating()
        {
            Database.SetInitializer<BasicTypeContext>(null);
            using (var ctx = new BasicTypeContext())
            {
                var oc = ((IObjectContextAdapter)ctx).ObjectContext;
            }
        }

        [Fact]
        public void HasKey_throws_on_collection_nav_prop()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Entity<Customer>().HasKey(c => c.CustomerAddresses))
                .ValidateMessage("ModelBuilder_KeyPropertiesMustBePrimitive", "CustomerAddresses", typeof(Customer));
        }

        [Fact]
        public void HasKey_throws_on_reference_nav_prop()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Entity<Customer>().HasKey(c => c.CustomerDiscount))
                .ValidateMessage("ModelBuilder_KeyPropertiesMustBePrimitive", "CustomerDiscount", typeof(Customer));
        }

        [Fact]
        public void HasKey_throws_on_complex_property()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.ComplexType<CustomerDiscount>();

            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Entity<Customer>().HasKey(c => c.CustomerDiscount))
                .ValidateMessage("ModelBuilder_KeyPropertiesMustBePrimitive", "CustomerDiscount", typeof(Customer));
        }

        [Fact]
        public void Nested_config_class_with_private_property()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Configurations.Add(new CreditCard.CreditCardConfiguration());

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<CreditCard>().HasColumn("CardNumber");
        }

        public class BasicType
        {
            public int Id { get; set; }
        }

        public class BasicTypeConfiguration : EntityTypeConfiguration<BasicType>
        {
            public BasicTypeConfiguration()
            {
                ToTable("Blah");
            }
        }

        public class BasicTypeContext : DbContext
        {
            public DbSet<BasicType> BasicTypes { get; set; }

            private readonly BasicTypeConfiguration _basicTypeConfiguration = new BasicTypeConfiguration();

            internal BasicTypeConfiguration BasicTypeConfiguration
            {
                get { return _basicTypeConfiguration; }
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Configurations.Add(BasicTypeConfiguration);
                modelBuilder.Entity<BasicType>();
                Assert.Equal("BasicTypes", modelBuilder.ModelConfiguration.Entity(typeof(BasicType)).EntitySetName);
                Assert.Equal("Blah", modelBuilder.ModelConfiguration.Entity(typeof(BasicType)).GetTableName().Name);
            }
        }
    }
}
