// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using FunctionalTests.Model;
    using Xunit;

    public sealed class ComplexTypeScenarioTests : TestBase
    {
        [Fact]
        public void Can_configure_complex_column_name_after_entity_splitting()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.ComplexType<ComplexTypeWithColumnRename>();

            modelBuilder.Entity<EntityWithColumnsRename>()
                .Map(
                    mapping =>
                        {
                            mapping.ToTable("Table1");
                            mapping.Properties(e => e.Property1);
                        });

            modelBuilder.Entity<EntityWithColumnsRename>()
                .Map(
                    mapping =>
                        {
                            mapping.ToTable("Table2");
                            mapping.Properties(e => e.Property2);
                            mapping.Properties(e => e.ComplexProp);
                        });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<EntityWithColumnsRename>("Table2").HasColumn("ColumnFor_Details");
        }

        [Fact]
        public void Complex_types_in_tpt_should_have_configuration_applied()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Person_166889>().ToTable("People");
            modelBuilder.Entity<Employee_166889>().ToTable("Employees");
            modelBuilder.ComplexType<Address_166889>().Property(a => a.Street).HasColumnName("test");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<Address_166889>(c => c.Street).DbEqual("test", c => c.Name);
        }

        [Fact]
        public void Complex_types_discovered_by_convention_should_have_configuration_applied()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<EntityWithByConventionComplexType>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<ChildComplexType>(c => c.Property).DbEqual("Foo", c => c.Name).DbEqual(
                false,
                c =>
                c.Nullable);
        }

        [Fact]
        public void Complex_property_configuration_should_configure_complex_types()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<EntityWithNestedComplexType>().Property(e => e.ChildComplex.Property).HasColumnName(
                "Foo1");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal(2, databaseMapping.Model.ComplexTypes.Count());

            databaseMapping.Assert<EntityWithNestedComplexType>("EntityWithNestedComplexTypes").HasColumns(
                "Id", "Bar",
                "Foo", "Foo1");
        }

        [Fact]
        public void Nested_complex_types_are_discovered()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<EntityWithNestedComplexType>().Ignore(e => e.ChildComplex);
            modelBuilder.ComplexType<ParentComplexType>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal(2, databaseMapping.Model.ComplexTypes.Count());

            databaseMapping.Assert<EntityWithNestedComplexType>("EntityWithNestedComplexTypes").HasColumns(
                "Id", "Bar",
                "Foo");
        }

        [Fact]
        public void Complex_type_and_nested_complex_type_can_have_column_names_configured()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<EntityWithNestedComplexType>()
                .Property(e => e.ChildComplex.Property)
                .HasColumnName("Foo1");

            modelBuilder.Entity<EntityWithNestedComplexType>()
                .Property(e => e.Complex.Nested.Property)
                .HasColumnName("Foo2");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal(2, databaseMapping.Model.ComplexTypes.Count());
            databaseMapping.Assert<EntityWithNestedComplexType>("EntityWithNestedComplexTypes").HasColumns(
                "Id", "Bar",
                "Foo2",
                "Foo1");
        }

        [Fact]
        public void Complex_type_can_have_column_names_configured_whithout_altering_order()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<EntityWithNestedComplexType>()
                .Property(e => e.ChildComplex.Property)
                .HasColumnName("Foo1");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal(2, databaseMapping.Model.ComplexTypes.Count());
            databaseMapping.Assert<EntityWithNestedComplexType>("EntityWithNestedComplexTypes").HasColumns(
                "Id", "Bar",
                "Foo", "Foo1");
        }

        [Fact]
        public void Complex_type_and_nested_complex_type_column_names_configured_using_complex_type_configuration_throws()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<EntityWithNestedComplexType>();
            modelBuilder.ComplexType<ChildComplexType>()
                .Property(c => c.Property)
                .HasColumnName("Foo");

            Assert.Throws<ModelValidationException>(() => BuildMapping(modelBuilder));
        }

        [Fact]
        public void Complex_type_column_names_use_property_path()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ComplexTypeEntity>();
            modelBuilder.ComplexType<ComplexType>().Ignore(c => c.NestedComplexProperty);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping
                .Assert<ComplexTypeEntity>()
                .HasColumns("Id", "ComplexPropertyA_Property", "ComplexPropertyB_Property");
        }

        [Fact]
        public void Self_referencing_complex_type_throws_exception()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ComplexTypeEntity>();
            modelBuilder.ComplexType<ComplexType>();

            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("CircularComplexTypeHierarchy");
        }

        [Fact]
        public void Build_model_for_type_with_a_non_public_complex_type_property()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<TransactionHistory>()
                .HasKey(th => th.TransactionID);
            modelBuilder.ComplexType<RowDetails>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            databaseMapping.Assert<TransactionHistory>(th => th.RowDetails);
        }

        [Fact]
        public void Build_model_for_a_single_type_with_a_complex_type()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<ProductDescription>();
            modelBuilder.ComplexType<RowDetails>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());
            Assert.Equal(1, databaseMapping.Model.ComplexTypes.Count());
        }

        [Fact]
        public void Build_model_containing_a_complex_type_with_annotations()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<ProductDescription>();

            var databaseMapping = modelBuilder.BuildAndValidate(
                ProviderRegistry.Sql2008_ProviderInfo,
                typeof(RowDetails));

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());
            Assert.Equal(1, databaseMapping.Model.ComplexTypes.Count());
        }

        [Fact]
        public void Build_model_containing_a_complex_type_by_convention()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<BillOfMaterials>();

            var databaseMapping = modelBuilder.BuildAndValidate(
                ProviderRegistry.Sql2008_ProviderInfo,
                typeof(UnitMeasure));

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());
            Assert.Equal(1, databaseMapping.Model.ComplexTypes.Count());
        }

        [Fact]
        public void Build_model_containing_a_complex_type_with_configuration()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<ProductDescription>();
            modelBuilder
                .ComplexType<RowDetails>()
                .Property(rd => rd.rowguid)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed)
                .HasColumnName("ROW_GUID");

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());
        }

        [Fact]
        public void Build_model_containing_a_complex_type_with_instance_configuration()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<ProductDescription>()
                .Property(pd => pd.RowDetails.rowguid)
                .HasColumnName("ROW_GUID");
            modelBuilder.ComplexType<RowDetails>()
                .Property(rd => rd.rowguid)
                .HasColumnName("row_guid");
            modelBuilder.Entity<Contact>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(2, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());
        }

        [Fact]
        public void Build_model_containing_a_complex_type_with_instance_cspace_configuration()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<ProductDescription>()
                .Property(pd => pd.RowDetails.rowguid)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            modelBuilder.ComplexType<RowDetails>();
            modelBuilder.Entity<Contact>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(2, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());
        }

        [Fact]
        public void Build_model_containing_a_complex_type_with_has_max_length_configuration()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.ComplexType<LargePhoto>();
            modelBuilder.Entity<ProductPhoto>()
                .Property(p => p.LargePhoto.Photo)
                .HasColumnType("binary")
                .HasMaxLength(42);

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            databaseMapping.Assert<LargePhoto>(l => l.Photo)
                .FacetEqual(42, f => f.MaxLength)
                .DbEqual(42, f => f.MaxLength);
        }

        [Fact]
        public void Build_model_containing_a_complex_type_with_instance_cspace_configuration_override()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<ProductDescription>()
                .Property(pd => pd.RowDetails.rowguid)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            modelBuilder.ComplexType<RowDetails>()
                .Property(rd => rd.rowguid)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Assert.Throws<InvalidOperationException>(() => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo));
        }

        [Fact]
        public void ComplexType_nullable_is_not_propagated_when_using_TPH()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<CTEmployee>();
            modelBuilder.ComplexType<Address>()
                .Property(a => a.Line1)
                .IsRequired();

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            // Not propagated

            Assert.Equal(
                false,
                databaseMapping.Database.EntityTypes.ElementAt(0).Properties.Single(c => c.Name == "HomeAddress_Line1")
                    .Nullable);
            EdmModel tempQualifier1 = databaseMapping.Database;
            DebugCheck.NotNull(tempQualifier1);
            Assert.Equal(
                true,
                tempQualifier1.EntityTypes.ElementAt(0).Properties.Single(c => c.Name == "WorkAddress_Line1")
                    .Nullable);

            EdmModel tempQualifier2 = databaseMapping.Database;
            DebugCheck.NotNull(tempQualifier2);
            Assert.Equal(
                true,
                tempQualifier2.EntityTypes.ElementAt(0).Properties.Single(c => c.Name == "HomeAddress_Line2")
                    .Nullable);
            EdmModel tempQualifier3 = databaseMapping.Database;
            DebugCheck.NotNull(tempQualifier3);
            Assert.Equal(
                true,
                tempQualifier3.EntityTypes.ElementAt(0).Properties.Single(c => c.Name == "WorkAddress_Line2")
                    .Nullable);
        }

        [Fact]
        public void ComplexType_nullable_is_propagated_when_using_TPT()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<CTEmployee>().ToTable("Employees");
            modelBuilder.Entity<OffSiteEmployee>().ToTable("OffSiteEmployees");
            modelBuilder.ComplexType<Address>()
                .Property(a => a.Line1)
                .IsRequired();

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            Assert.Equal(
                false,
                databaseMapping.Database.EntityTypes.ElementAt(0).Properties.Single(c => c.Name == "HomeAddress_Line1")
                    .Nullable);
            EdmModel tempQualifier1 = databaseMapping.Database;
            DebugCheck.NotNull(tempQualifier1);
            Assert.Equal(
                false,
                tempQualifier1.EntityTypes.ElementAt(1).Properties.Single(c => c.Name == "WorkAddress_Line1")
                    .Nullable);

            EdmModel tempQualifier2 = databaseMapping.Database;
            DebugCheck.NotNull(tempQualifier2);
            Assert.Equal(
                true,
                tempQualifier2.EntityTypes.ElementAt(0).Properties.Single(c => c.Name == "HomeAddress_Line2")
                    .Nullable);
            EdmModel tempQualifier3 = databaseMapping.Database;
            DebugCheck.NotNull(tempQualifier3);
            Assert.Equal(
                true,
                tempQualifier3.EntityTypes.ElementAt(1).Properties.Single(c => c.Name == "WorkAddress_Line2")
                    .Nullable);
        }

        [Fact]
        public void Model_with_multiple_complex_types_and_entities_finds_complex_types_by_convention()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<LoCTBuilding>()
                .HasKey(b => b.Number);

            modelBuilder.Entity<LoCTEmployee>()
                .HasKey(b => b.EmployeeNumber);

            modelBuilder.Entity<LoCTOffice>()
                .HasKey(o => o.Number);

            modelBuilder.Entity<LoCTEmployeePhoto>()
                .HasKey(
                    p => new
                             {
                                 p.EmployeeNo,
                                 p.PhotoId
                             });

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            Assert.Equal(2, databaseMapping.Model.ComplexTypes.Count());
        }

        [Fact]
        public void Annotations_on_complex_type_classes_are_not_present_on_properties()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<CTEmployee>();

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            // Not propagated
            var annotations = databaseMapping.Model
                .EntityTypes.Single(x => x.Name == "CTEmployee")
                .Properties.Single(p => p.Name == "HomeAddress")
                .Annotations;
            Assert.Equal(
                0,
                ((ICollection<Attribute>)annotations.SingleOrDefault(a => a.Name == "ClrAttributes").Value).
                    Count);
        }

        [Fact]
        public void Property_max_length_convention_applied_to_complex_types()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<CTEmployee>();
            modelBuilder.Ignore<OffSiteEmployee>();
            modelBuilder.ComplexType<Address>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            databaseMapping.Assert<Address>(a => a.Line1).FacetEqual(true, f => f.IsMaxLength);
        }
    }

    #region Fixtures

    [ComplexType]
    public class Address
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
    }

    public class CTEmployee
    {
        public int CTEmployeeId { get; set; }
        public Address HomeAddress { get; set; }
    }

    public class OffSiteEmployee : CTEmployee
    {
        public Address WorkAddress { get; set; }
    }

    public class Building
    {
        public int Id { get; set; }
        public Address Address { get; set; }
    }

    public class LoCTBuilding
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public LoCTAddress Address { get; set; }

        public ICollection<LoCTOffice> LoCTOffices { get; set; }
    }

    public class LoCTOffice
    {
        public int BuildingNumber { get; set; }
        public string Number { get; set; }

        public LoCTBuilding LoCTBuilding { get; set; }
        public ICollection<LoCTEmployee> Occupants { get; set; }
    }

    public class LoCTEmployee
    {
        public int EmployeeNumber { get; set; }
        public LoCTName Name { get; set; }
        public LoCTAddress HomeAddress { get; set; }

        public LoCTOffice Office { get; set; }
        public ICollection<LoCTEmployeePhoto> Photos { get; set; }
    }

    public class LoCTEmployeePhoto
    {
        public int PhotoId { get; set; }
        public int EmployeeNo { get; set; }

        public byte[] Photo { get; set; }
    }

    public class LoCTName
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
    }

    public class LoCTAddress
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }

    public class ComplexTypeEntity
    {
        public int Id { get; set; }
        public ComplexType ComplexPropertyA { get; set; }
        public ComplexType ComplexPropertyB { get; set; }
    }

    public class ComplexType
    {
        public string Property { get; set; }
        public ComplexType NestedComplexProperty { get; set; }
    }

    public class ParentComplexType
    {
        [Column("Bar")]
        [Required]
        public string Property { get; set; }

        public ChildComplexType Nested { get; set; }
    }

    public class ChildComplexType
    {
        [Column("Foo")]
        [Required]
        public string Property { get; set; }
    }

    public class EntityWithByConventionComplexType
    {
        public int Id { get; set; }
        public ChildComplexType Complex { get; set; }
    }

    public class EntityWithNestedComplexType
    {
        public int Id { get; set; }
        public ParentComplexType Complex { get; set; }
        public ChildComplexType ChildComplex { get; set; }
    }

    public class Person_166889
    {
        [Key]
        public int PersonId { get; set; }

        public string Name { get; set; }
    }

    public class Employee_166889 : Person_166889
    {
        public string EmployeeNo { get; set; }
        public Address_166889 Address { get; set; }
    }

    public class Address_166889
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
    }

    public class ComplexTypeWithColumnRename
    {
        [Column("ColumnFor_Details")]
        public string Details { get; set; }
    }

    public class EntityWithColumnsRename
    {
        public int Id { get; set; }

        [Column("ColumnFor_Property1")]
        public byte[] Property1 { get; set; }

        [Column("ColumnFor_Property2")]
        public string Property2 { get; set; }

        public ComplexTypeWithColumnRename ComplexProp { get; set; }
    }

    #endregion
}
