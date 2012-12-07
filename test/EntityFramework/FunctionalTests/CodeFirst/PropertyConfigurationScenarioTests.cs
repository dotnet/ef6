// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration;
    using System.Linq;
    using FunctionalTests.Model;
    using Xunit;

    public sealed class PropertyConfigurationScenarioTests : TestBase
    {
        [Fact]
        public void Binary_fixed_length_properties_get_correct_length_in_store()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<AllBinaryDataTypes>().Property(p => p.Prop_binary_10).IsFixedLength();
            modelBuilder.Entity<AllBinaryDataTypes>().Property(p => p.NProp_binary_10).IsFixedLength();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<AllBinaryDataTypes>(p => p.Prop_binary_10).DbEqual(128, f => f.MaxLength);
            databaseMapping.Assert<AllBinaryDataTypes>(p => p.Prop_binary_10).DbEqual(false, f => f.IsMaxLength);
            databaseMapping.Assert<AllBinaryDataTypes>(p => p.NProp_binary_10).DbEqual(128, f => f.MaxLength);
            databaseMapping.Assert<AllBinaryDataTypes>(p => p.NProp_binary_10).DbEqual(false, f => f.IsMaxLength);
        }

        [Fact]
        public void Decimal_property_gets_default_precision_by_convention()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<BillOfMaterials>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            databaseMapping.Assert<BillOfMaterials>(b => b.PerAssemblyQty).FacetEqual((byte)18, f => f.Precision);
            databaseMapping.Assert<BillOfMaterials>(b => b.PerAssemblyQty).FacetEqual((byte)2, f => f.Scale);
        }

        [Fact]
        public void Duplicate_property_names_differing_by_case_are_uniquified()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<DuplicatePropNames>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            databaseMapping.Assert<DuplicatePropNames>().HasColumns("Id", "name", "NAME");
        }

        [Fact]
        public void Configure_is_max_length_on_property()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>().Property(c => c.CustomerType).IsMaxLength();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            databaseMapping.Assert<Customer>(c => c.CustomerType).FacetEqual(true, f => f.IsMaxLength);
        }

        private void Configure_is_max_length_on_complex_property(Action<DbModelBuilder> configure)
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            configure(modelBuilder);

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            databaseMapping.Assert<UnitMeasure>(u => u.Name).FacetEqual(true, c => c.IsMaxLength);
            // Should be null for nvarchar(max)
            databaseMapping.Assert<BillOfMaterials>("BillOfMaterials").Column("UnitMeasure_Name")
                .DbEqual(false, c => c.IsMaxLength);
            databaseMapping.Assert<UnitMeasure>(u => u.Name).FacetEqual(null, c => c.MaxLength);
            databaseMapping.Assert<BillOfMaterials>("BillOfMaterials").Column("UnitMeasure_Name")
                .DbEqual(null, c => c.MaxLength);
            databaseMapping.Assert<UnitMeasure>(u => u.Name).FacetEqual(false, c => c.IsFixedLength);
            databaseMapping.Assert<BillOfMaterials>("BillOfMaterials").Column("UnitMeasure_Name")
                .DbEqual(null, c => c.IsFixedLength);
        }

        [Fact]
        public void Configure_is_max_length_on_complex_property_using_api()
        {
            Configure_is_max_length_on_complex_property(
                modelBuilder =>
                    {
                        modelBuilder.Entity<BillOfMaterials>().Ignore(b => b.Product);
                        modelBuilder.Entity<BillOfMaterials>().Ignore(
                            b => b.Product1);
                        modelBuilder.ComplexType<UnitMeasure>().Property(u => u.Name)
                            .IsMaxLength();
                    });
        }

        [Fact]
        public void Configure_is_max_length_on_complex_property_using_configuration()
        {
            Configure_is_max_length_on_complex_property(
                modelBuilder =>
                    {
                        var configuration =
                            new ComplexTypeConfiguration<UnitMeasure>();

                        configuration.Property(u => u.Name).IsMaxLength();

                        modelBuilder.Configurations.Add(configuration);

                        modelBuilder.Entity<BillOfMaterials>().Ignore(b => b.Product);
                        modelBuilder.Entity<BillOfMaterials>().Ignore(
                            b => b.Product1);
                    });
        }

        [Fact]
        public void Configure_has_max_length_on_property()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>().Property(c => c.CustomerType).HasMaxLength(40);

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);
            databaseMapping.AssertValid();

            databaseMapping.Assert<Customer>(c => c.CustomerType).FacetEqual(40, f => f.MaxLength);
        }

        [Fact]
        public void Configure_store_type_on_property()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>().Property(c => c.CustomerType).HasColumnType("ntext");

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);
            databaseMapping.AssertValid();

            databaseMapping.Assert<Customer>(c => c.CustomerType).DbEqual("ntext", f => f.TypeName);
        }

        [Fact]
        public void Configure_nullable_scalar_key()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Location>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);
            databaseMapping.AssertValid();

            databaseMapping.Assert<Location>(l => l.LocationID).IsFalse(t => t.Nullable);
        }

        [Fact]
        public void Overridden_nullable_scalar_key_becomes_nullable_scalar_property()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder
                .Entity<Location>()
                .HasKey(l => l.Name);

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);
            databaseMapping.AssertValid();

            databaseMapping.Assert<Location>(l => l.LocationID).IsTrue(t => t.Nullable);
        }

        [Fact]
        public void Configure_identity_on_nullable_scalar_property()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder
                .Entity<SpecialOffer>()
                .Property(s => s.MaxQty)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);
            databaseMapping.AssertValid();

            databaseMapping.Assert<SpecialOffer>(s => s.MaxQty)
                .AnnotationEqual(StoreGeneratedPattern.Identity, "StoreGeneratedPattern");
            databaseMapping.Assert<SpecialOffer>(s => s.MaxQty).DbIsFalse(t => t.Nullable);
        }

        [Fact]
        public void Configure_IsConcurrencyToken_using_api()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<BillOfMaterials>().Ignore(b => b.Product);
            modelBuilder.Entity<BillOfMaterials>().Ignore(b => b.Product1);
            modelBuilder.ComplexType<UnitMeasure>().Property(u => u.UnitMeasureCode).IsConcurrencyToken();

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            Assert.Equal(
                ConcurrencyMode.Fixed,
                databaseMapping.Model.ComplexTypes.Single()
                    .Properties.Single(p => p.Name == "UnitMeasureCode").ConcurrencyMode);
        }

        [Fact]
        public void Configure_IsConcurrencyToken_using_configuration()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            var configuration = new ComplexTypeConfiguration<UnitMeasure>();

            configuration.Property(u => u.UnitMeasureCode).IsConcurrencyToken();

            modelBuilder.Configurations.Add(configuration);

            modelBuilder.Entity<BillOfMaterials>().Ignore(b => b.Product);
            modelBuilder.Entity<BillOfMaterials>().Ignore(b => b.Product1);

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            Assert.Equal(
                ConcurrencyMode.Fixed,
                databaseMapping.Model.ComplexTypes.Single()
                    .Properties.Single(p => p.Name == "UnitMeasureCode").ConcurrencyMode);
        }

        [Fact]
        public void Configure_nullable_scalar_property_as_required_using_annotation()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Product>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            databaseMapping.Assert<Product>(p => p.SellEndDate).IsFalse(t => t.Nullable);
        }

        [Fact]
        public void Configure_nullable_scalar_property_as_required_using_api()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Product>().Property(p => p.ProductSubcategoryID).IsRequired();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            databaseMapping.Assert<Product>(p => p.ProductSubcategoryID).IsFalse(t => t.Nullable);
        }

        [Fact]
        public void Configure_identity_on_non_key_property()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder
                .Entity<WorkOrder>()
                .Property(w => w.OrderQty)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            databaseMapping.Assert<WorkOrder>(w => w.OrderQty)
                .AnnotationEqual(StoreGeneratedPattern.Identity, "StoreGeneratedPattern");
            databaseMapping.Assert<WorkOrder>(w => w.WorkOrderID)
                .AnnotationNull("StoreGeneratedPattern");
        }

        [Fact]
        public void Configure_identity_on_complex_property()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder
                .ComplexType<WorkOrder>()
                .Property(w => w.OrderQty)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(
                StoreGeneratedPattern.Identity,
                databaseMapping.Model
                    .ComplexTypes.Single()
                    .Properties.Single(p => p.Name == "OrderQty")
                    .Annotations.Single(a => a.Name == "StoreGeneratedPattern").Value);
        }

        [Fact]
        public void Configure_IsRequired_on_a_complex_child_property()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<CTEmployee>()
                .Property(a => a.HomeAddress.Line1)
                .IsOptional();
            modelBuilder.Entity<CTEmployee>()
                .Property(a => a.HomeAddress.Line1)
                .IsRequired();

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            databaseMapping.Assert<CTEmployee>("CTEmployees")
                .Column("HomeAddress_Line1")
                .DbEqual(false, l => l.Nullable);
        }

        [Fact]
        public void Configure_IsRequired_on_a_complex_child_property_conflicting_values()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<CTEmployee>()
                .Property(a => a.HomeAddress.Line1)
                .IsRequired();
            modelBuilder.Entity<Building>()
                .Property(b => b.Address.Line1)
                .IsOptional();

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo));
        }

        [Fact]
        public void Configure_IsOptional_on_a_complex_child_property_after_IsRequired_on_complex_type()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.ComplexType<Address>().Property(a => a.Line1).IsRequired();

            modelBuilder.Entity<Building>()
                .Property(b => b.Address.Line1)
                .IsOptional();

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo));
        }

        [Fact]
        public void Configure_IsOptional_and_HasMaxLength_on_a_complex_child_property_after_HasMaxLength_on_complex_type()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.ComplexType<Address>().Property(a => a.Line1).HasMaxLength(10);

            modelBuilder.Entity<CTEmployee>()
                .Property(a => a.HomeAddress.Line1)
                .HasMaxLength(20);
            modelBuilder.Entity<Building>()
                .Property(b => b.Address.Line1)
                .IsOptional();

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo));
        }

        [Fact]
        public void Configure_HasColumnName_using_api()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<UnitMeasure>().HasKey(u => u.UnitMeasureCode);
            modelBuilder.Entity<UnitMeasure>().Property(u => u.UnitMeasureCode).HasColumnName("Code");

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);
            databaseMapping.AssertValid();

            databaseMapping.Assert<UnitMeasure>("UnitMeasures").HasColumns("Code", "Name");
        }

        [Fact]
        public void Configure_HasColumnName_using_configuration()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            var configuration = new EntityTypeConfiguration<UnitMeasure>();

            configuration.HasKey(u => u.UnitMeasureCode);
            configuration.Property(u => u.UnitMeasureCode).HasColumnName("Code");

            modelBuilder.Configurations.Add(configuration);

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            databaseMapping.Assert<UnitMeasure>("UnitMeasures").HasColumns("Code", "Name");
        }

        [Fact]
        public void Configure_HasColumnName_using_configuration_can_be_overriden_using_api()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            var configuration = new EntityTypeConfiguration<UnitMeasure>();

            configuration.HasKey(u => u.UnitMeasureCode);
            configuration.Property(u => u.UnitMeasureCode).HasColumnName("Code");

            modelBuilder.Configurations.Add(configuration);

            modelBuilder.Entity<UnitMeasure>().Property(u => u.UnitMeasureCode).HasColumnName("UnitCode");

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            databaseMapping.Assert<UnitMeasure>("UnitMeasures").HasColumns("UnitCode", "Name");
        }

        [Fact]
        public void Configure_HasColumnName_on_a_complex_child_property_different_entities()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<CTEmployee>()
                .Property(a => a.HomeAddress.Line1)
                .HasColumnName("HomeAddress");
            modelBuilder.Entity<Building>()
                .Property(b => b.Address.Line1)
                .HasColumnName("StreetAddress");

            modelBuilder.ComplexType<Address>().Property(a => a.Line1).HasColumnName("FirstLine");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            databaseMapping.Assert<CTEmployee>("CTEmployees").HasColumns(
                "CTEmployeeId", "HomeAddress",
                "HomeAddress_Line2", "FirstLine",
                "WorkAddress_Line2", "Discriminator");

            databaseMapping.Assert<Building>("Buildings").HasColumns("Id", "StreetAddress", "Address_Line2");
        }

        [Fact]
        public void Configure_HasColumnName_on_a_complex_child_property_after_HasColumnName_on_complex_type()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.ComplexType<Address>().Property(a => a.Line1).HasColumnName("FirstLine");

            modelBuilder.Entity<Building>();
            modelBuilder.Entity<CTEmployee>()
                .Property(a => a.HomeAddress.Line1)
                .HasColumnName("HomeAddress");

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            databaseMapping.Assert<CTEmployee>("CTEmployees").HasColumns(
                "CTEmployeeId", "HomeAddress",
                "HomeAddress_Line2", "FirstLine",
                "WorkAddress_Line2", "Discriminator");

            databaseMapping.Assert<Building>("Buildings").HasColumns("Id", "FirstLine", "Address_Line2");
        }

        // TODO: METADATA [Fact]
        public void Configure_HasColumnName_on_complex_type_appearing_several_times_on_an_entity_throws()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.ComplexType<Address>().Property(a => a.Line1).HasColumnName("FirstLine");

            modelBuilder.Entity<OffSiteEmployee>()
                .HasKey(e => e.CTEmployeeId);

            var databaseMapping = BuildMapping(modelBuilder);

            Assert.Throws<MetadataException>(() => databaseMapping.AssertValid());
        }

        [Fact]
        public void Two_properties_that_both_match_the_primary_key_convention_can_be_disambiguated_using_the_fluent_API()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TwoManyKeys>()
                .HasKey(e => e.ID);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<TwoManyKeys>(e => e.ID)
                .DbEqual(true, c => c.IsPrimaryKeyColumn);
        }

        [Fact]
        public void
            Two_properties_that_both_match_the_primary_key_convention_can_be_disambiguated_using_a_data_annotation()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TwoManyKeysWithAnnotation>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<TwoManyKeysWithAnnotation>(e => e.ID)
                .DbEqual(true, c => c.IsPrimaryKeyColumn);
        }
    }

    #region Fixtures

    public class DuplicatePropNames
    {
        public int Id { get; set; }
        public string name { get; set; }
        public string NAME { get; set; }
    }

    public class AllBinaryDataTypes
    {
        public int ID { get; set; }
        public byte[] Prop_binary_10 { get; set; }
        public byte[] NProp_binary_10 { get; set; }
    }

    #endregion

    public class TwoManyKeys
    {
        public int Id { get; set; }
        public int ID { get; set; }
    }

    public class TwoManyKeysWithAnnotation
    {
        public int Id { get; set; }

        [Key]
        public int ID { get; set; }
    }
}
