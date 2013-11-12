// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;
    using System.Linq.Expressions;
    using FunctionalTests.Model;
    using Xunit;

    public class AdvancedMappingScenarioTests : TestBase
    {
        [Fact]
        public void Sql_ce_should_get_explicit_max_lengths_for_string_and_binary_properties_by_convention()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MaxLengthProperties>();

            var databaseMapping = BuildCeMapping(modelBuilder);

            databaseMapping.AssertValid();

            databaseMapping.Assert<MaxLengthProperties>(e => e.Id).DbEqual(4000, f => f.MaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Id).DbEqual(false, f => f.IsMaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Id).DbEqual("nvarchar", c => c.TypeName);

            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop1).DbEqual(4000, f => f.MaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop1).DbEqual(false, f => f.IsMaxLengthConstant);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop1).DbEqual(false, f => f.IsMaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop1).DbEqual("nvarchar", c => c.TypeName);

            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop2).DbEqual(4000, f => f.MaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop2).DbEqual(false, f => f.IsMaxLengthConstant);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop2).DbEqual(false, f => f.IsMaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop2).DbEqual("varbinary", c => c.TypeName);
        }

        [Fact]
        public void Sql_ce_should_get_explicit_max_lengths_for_fixed_length_string_and_fixed_length_binary_properties_by_convention()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MaxLengthProperties>().Property(e => e.Id).IsFixedLength();
            modelBuilder.Entity<MaxLengthProperties>().Property(e => e.Prop1).IsFixedLength();
            modelBuilder.Entity<MaxLengthProperties>().Property(e => e.Prop2).IsFixedLength();

            var databaseMapping = BuildCeMapping(modelBuilder);

            databaseMapping.AssertValid();

            databaseMapping.Assert<MaxLengthProperties>(e => e.Id).DbEqual(4000, f => f.MaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Id).DbEqual(false, f => f.IsMaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Id).DbEqual("nchar", c => c.TypeName);

            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop1).DbEqual(4000, f => f.MaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop1).DbEqual(false, f => f.IsMaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop1).DbEqual("nchar", c => c.TypeName);

            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop2).DbEqual(4000, f => f.MaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop2).DbEqual(false, f => f.IsMaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop2).DbEqual("binary", c => c.TypeName);
        }

        [Fact]
        public void Sql_should_get_implicit_max_lengths_for_string_and_binary_properties_by_convention()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MaxLengthProperties>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            databaseMapping.Assert<MaxLengthProperties>(e => e.Id).DbEqual(128, f => f.MaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Id).DbEqual(false, f => f.IsMaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Id).DbEqual("nvarchar", c => c.TypeName);

            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop1).DbEqual(true, f => f.IsMaxLengthConstant);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop1).DbEqual(false, f => f.IsMaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop1).DbEqual("nvarchar(max)", c => c.TypeName);

            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop2).DbEqual(true, f => f.IsMaxLengthConstant);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop2).DbEqual(false, f => f.IsMaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop2).DbEqual("varbinary(max)", c => c.TypeName);
        }

        [Fact]
        public void Sql_should_get_explicit_max_lengths_for_fixed_length_string_and_fixed_length_binary_properties_by_convention()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MaxLengthProperties>().Property(e => e.Id).IsFixedLength();
            modelBuilder.Entity<MaxLengthProperties>().Property(e => e.Prop1).IsFixedLength();
            modelBuilder.Entity<MaxLengthProperties>().Property(e => e.Prop2).IsFixedLength();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            databaseMapping.Assert<MaxLengthProperties>(e => e.Id).DbEqual(128, f => f.MaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Id).DbEqual(false, f => f.IsMaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Id).DbEqual("nchar", c => c.TypeName);

            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop1).DbEqual(128, f => f.MaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop1).DbEqual(false, f => f.IsMaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop1).DbEqual("nchar", c => c.TypeName);

            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop2).DbEqual(128, f => f.MaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop2).DbEqual(false, f => f.IsMaxLength);
            databaseMapping.Assert<MaxLengthProperties>(e => e.Prop2).DbEqual("binary", c => c.TypeName);
        }

        public class MaxLengthProperties
        {
            public string Id { get; set; }
            public string Prop1 { get; set; }
            public byte[] Prop2 { get; set; }
        }

        [Fact]
        public void Can_have_configured_duplicate_column_and_by_convention_column_is_uniquified()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<EntityWithConfiguredDuplicateColumn>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<EntityWithConfiguredDuplicateColumn>(e => e.Description).DbEqual(
                "Description1",
                c => c.Name);
            databaseMapping.Assert<EntityWithConfiguredDuplicateColumn>(e => e.Details).DbEqual(
                "Description",
                c => c.Name);
        }

        public class EntityWithConfiguredDuplicateColumn
        {
            public int Id { get; set; }
            public string Description { get; set; }

            [Column("Description")]
            public string Details { get; set; }
        }

        [Fact]
        public void Can_have_configured_duplicate_column_and_by_convention_columns_are_uniquified_first()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<EntityWithDescBase>();
            modelBuilder.Entity<EntityWithDescA>().Property(e => e.Description).HasColumnName("Description");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<EntityWithDescA>(e => e.Description).DbEqual("Description", c => c.Name);
            databaseMapping.Assert<EntityWithDescB>(e => e.Description).DbEqual("Description1", c => c.Name);
            databaseMapping.Assert<EntityWithDescC>(e => e.Description).DbEqual("Description2", c => c.Name);
        }

        [Fact]
        public void Can_have_configured_duplicate_column_and_by_convention_columns_are_uniquified_second()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<EntityWithDescBase>();
            modelBuilder.Entity<EntityWithDescB>().Property(e => e.Description).HasColumnName("Description");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<EntityWithDescA>(e => e.Description).DbEqual("Description1", c => c.Name);
            databaseMapping.Assert<EntityWithDescB>(e => e.Description).DbEqual("Description", c => c.Name);
            databaseMapping.Assert<EntityWithDescC>(e => e.Description).DbEqual("Description2", c => c.Name);
        }

        [Fact]
        public void Can_have_configured_duplicate_column_and_by_convention_columns_are_uniquified_third()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<EntityWithDescBase>();
            modelBuilder.Entity<EntityWithDescC>().Property(e => e.Description).HasColumnName("Description");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<EntityWithDescA>(e => e.Description).DbEqual("Description1", c => c.Name);
            databaseMapping.Assert<EntityWithDescB>(e => e.Description).DbEqual("Description2", c => c.Name);
            databaseMapping.Assert<EntityWithDescC>(e => e.Description).DbEqual("Description", c => c.Name);
        }

        [Fact]
        public void Can_have_configured_duplicate_column_and_by_convention_columns_are_uniquified_complex()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<EntityWithDescA>().Property(e => e.Complex.Description).HasColumnName("Description");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<ComplexWithDesc>(c => c.Description).DbEqual("Description", c => c.Name);
            databaseMapping.Assert<ComplexWithDesc>(c => c.Description).DbEqual(false, c => c.Nullable);
            databaseMapping.Assert<EntityWithDescA>(e => e.Description).DbEqual("Description1", c => c.Name);
        }

        [Fact]
        public void Can_have_configured_complex_column_override_column_name_clash()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<EntityWithDescA>().Property(e => e.Complex.Description).HasColumnName("Description");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<ComplexWithDesc>(c => c.Description).DbEqual(false, c => c.Nullable);
            databaseMapping.Assert<ComplexWithDesc>(c => c.Description).DbEqual("Description", c => c.Name);
            databaseMapping.Assert<EntityWithDescA>(e => e.Description).DbEqual("Description1", c => c.Name);
        }

        [Fact]
        public void Can_have_configured_duplicate_column_and_by_convention_columns_are_uniquified_conflict()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<EntityWithDescBase>();
            modelBuilder.Entity<EntityWithDescB>().Property(e => e.Description).HasColumnName("Description");
            modelBuilder.Entity<EntityWithDescB>().Property(e => e.NotDescription).HasColumnName("Description");

            Assert.Throws<ModelValidationException>(() => BuildMapping(modelBuilder));
        }

        public class EntityWithDescBase
        {
            public int Id { get; set; }
        }

        public class EntityWithDescA : EntityWithDescBase
        {
            public string Description { get; set; }
            public ComplexWithDesc Complex { get; set; }
        }

        public class EntityWithDescB : EntityWithDescBase
        {
            public string Description { get; set; }
            public string NotDescription { get; set; }
            public ComplexWithDesc Complex { get; set; }
        }

        public class EntityWithDescC : EntityWithDescBase
        {
            public string Description { get; set; }
            public ComplexWithDesc Complex { get; set; }
        }

        public class ComplexWithDesc
        {
            [Required]
            public string Description { get; set; }
        }

        [Fact]
        public void Can_table_split_and_conflicting_columns_are_uniquified()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SplitProduct>().ToTable("Product");
            modelBuilder.Entity<SplitProductDetail>().ToTable("Product");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
        }

        [Fact]
        public void Can_table_split_and_conflicting_columns_can_be_configured()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SplitProduct>().ToTable("Product");

            modelBuilder.Entity<SplitProductDetail>()
                .ToTable("Product")
                .Property(s => s.Name)
                .HasColumnName("Unique")
                .HasAnnotation("Fish", "Blub");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<SplitProduct>(s => s.Name).DbEqual("Name", c => c.Name);
            databaseMapping.Assert<SplitProductDetail>(s => s.Name).DbEqual("Unique", c => c.Name);

            databaseMapping.Assert<SplitProduct>("Product")
                .Column("Name")
                .HasNoAnnotation("Fish");

            databaseMapping.Assert<SplitProductDetail>("Product")
                .Column("Unique")
                .HasAnnotation("Fish", "Blub");
        }

        public class SplitProduct
        {
            public int Id { get; set; }
            public string Name { get; set; }

            [Required]
            public SplitProductDetail Detail { get; set; }
        }

        public class SplitProductDetail
        {
            [ForeignKey("Product")]
            public int Id { get; set; }

            public string Name { get; set; }

            [Required]
            public SplitProduct Product { get; set; }
        }

        [Fact]
        public void Single_abstract_type_with_associations_throws_not_mappable_exception()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<SingleAbstract>();

            var exception = Assert.Throws<InvalidOperationException>(() => BuildMapping(modelBuilder));
            exception.ValidateMessage("UnmappedAbstractType", typeof(SingleAbstract));
        }

        [Fact]
        public void Configured_decimal_key_gets_correct_facet_defaults()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<DecimalKey>().HasKey(d => d.Id);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<DecimalKey>(d => d.Id).FacetEqual((byte)18, f => f.Precision);
            databaseMapping.Assert<DecimalKey>(d => d.Id).FacetEqual((byte)2, f => f.Scale);
        }

        [Fact]
        public void Decimal_key_with_custom_store_type_should_propagate_facets()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<DecimalKey>().Property(p => p.Id).HasColumnType("money");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
        }

        public class DecimalKey
        {
            public decimal Id { get; set; }
            public ICollection<DecimalDependent> DecimalDependents { get; set; }
        }

        public class DecimalDependent
        {
            public int Id { get; set; }
            public decimal DecimalKeyId { get; set; }
        }

        public abstract class SingleAbstract
        {
            public int Id { get; set; }
            public DecimalDependent Nav { get; set; }
        }

        [Fact]
        public void Throw_when_mapping_properties_expression_contains_assignments()
        {
            var modelBuilder = new DbModelBuilder();
            Expression<Func<StockOrder, object>> propertiesExpression = so => new { Foo = so.LocationId };

            var exception = Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Entity<StockOrder>().Map(emc => emc.Properties(propertiesExpression)));

            exception.ValidateMessage("InvalidComplexPropertiesExpression", propertiesExpression);
        }

        [Fact]
        public void Circular_delete_cascade_path_can_be_generated()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<StockOrder>();
            var databaseMapping = BuildMapping(modelBuilder);

            Assert.Equal(
                3,
                databaseMapping.Model
                    .AssociationTypes
                    .SelectMany(a => a.Members)
                    .Cast<AssociationEndMember>()
                    .Count(e => e.DeleteBehavior == OperationAction.Cascade));
        }

        public class StockOrder
        {
            public int Id { get; set; }
            public int LocationId { get; set; }
            public Location Location { get; set; }
            public ICollection<Organization> Organizations { get; set; }
        }

        public class Organization
        {
            public int Id { get; set; }
            public int StockOrderId { get; set; }
            public StockOrder StockOrder { get; set; }
            public ICollection<Location> Locations { get; set; }
        }

        public class Location
        {
            public int Id { get; set; }
            public ICollection<StockOrder> StockOrders { get; set; }
            public int OrganizationId { get; set; }
            public Organization Organization { get; set; }
        }

        [Fact]
        public void Build_model_for_entity_splitting_difference_schemas()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Vendor>()
                .Map(
                    m =>
                    {
                        m.Properties(
                            v1 => new
                                      {
                                          v1.VendorID,
                                          v1.Name,
                                          v1.PreferredVendorStatus,
                                          v1.AccountNumber,
                                          v1.ActiveFlag,
                                          v1.CreditRating
                                      });
                        m.ToTable("Vendor", "vendors");
                    })
                .Map(
                    m =>
                    {
                        m.Properties(
                            v2 => new
                                      {
                                          v2.VendorID,
                                          v2.ModifiedDate,
                                          v2.PurchasingWebServiceURL
                                      });
                        m.ToTable("VendorDetails", "details");
                    });

            var databaseMapping = BuildMapping(modelBuilder);

            Assert.True(databaseMapping.Database.GetEntitySets().Any(s => s.Schema == "vendors"));
            Assert.True(databaseMapping.Database.GetEntitySets().Any(s => s.Schema == "details"));
        }

        [Fact]
        public void Build_model_for_mapping_to_duplicate_tables_different_schemas()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>().ToTable("tbl");
            modelBuilder.Entity<Product>().ToTable("tbl", "other");

            var databaseMapping = BuildMapping(modelBuilder);

            Assert.True(databaseMapping.Database.GetEntitySets().Any(s => s.Schema == "dbo"));
            Assert.True(databaseMapping.Database.GetEntitySets().Any(s => s.Schema == "other"));

            databaseMapping.Assert<Customer>().DbEqual("tbl", t => t.Table);
            databaseMapping.Assert<Product>().DbEqual("tbl", t => t.Table);
        }

        [Fact]
        public void Build_model_after_configuring_entity_set_name()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<TransactionHistoryArchive>().HasEntitySetName("Foos");

            var databaseMapping = BuildMapping(modelBuilder);

            Assert.True(databaseMapping.Model.Containers.Single().EntitySets.Any(es => es.Name == "Foos"));
        }
    }
}
