// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;
    using FunctionalTests.Fixtures;
    using Xunit;

    public class EnumsScenarioTests : TestBase
    {
        [Fact]
        public void Simple_enum_mapping_scenario()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Enum_Product>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<Enum_Product>(p => p.CategoryId).IsTrue(t => t.IsEnumType);
            databaseMapping.Assert<Enum_Product>().HasColumn("CategoryId");

            var enumType = databaseMapping.Model.GetEnumType("CategoryId");

            Assert.NotNull(enumType);
            Assert.Equal(7, enumType.Members.Count);
            Assert.False(enumType.IsFlags);
            Assert.Equal(PrimitiveTypeKind.Int32, enumType.UnderlyingType.PrimitiveTypeKind);
        }

        [Fact]
        public void Short_enum_mapping_with_flags_attribute()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Enum_Flags>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            var enumType = databaseMapping.Model.GetEnumType("WithFlags");

            Assert.NotNull(enumType);
            Assert.Equal(4, enumType.Members.Count);
            Assert.True(enumType.IsFlags);
            Assert.Equal(PrimitiveTypeKind.Int16, enumType.UnderlyingType.PrimitiveTypeKind);
        }

        [Fact]
        public void Build_model_for_a_single_type_with_a_enum_key()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Enum_Product_PK>();

            var databaseMapping = BuildMapping(modelBuilder);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());
        }

        [Fact]
        public void Enum_mapped_as_by_convention_fk_in_association()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Enum_Fk_Product>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<Enum_Fk_Product>().HasForeignKeyColumn("CategoryId");
        }

        [Fact]
        public void Enum_mapped_as_by_convention_IA_in_association()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Enum_IA_Principal>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<Enum_IA_Dependent>().HasForeignKeyColumn("Principal_Id");
        }

        [Fact]
        public void Annotated_nullable_enum_inside_complex_type_is_mapped()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Enum_ComplexEntity>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<Enum_ComplexType>(c => c.Enum).IsTrue(t => t.IsEnumType);
            databaseMapping.Assert<Enum_ComplexType>(c => c.Enum).IsTrue(t => t.Nullable);
            databaseMapping.Assert<Enum_ComplexType>(c => c.Enum).DbEqual("col_enum", c => c.Name);
        }

        [Fact]
        public void Fluent_api_configured_enum_property_gets_correct_facets()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Enum_Product>().Property(p => p.CategoryId).IsOptional();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<Enum_Product>(p => p.CategoryId).IsTrue(p => p.Nullable);
        }

        [Fact]
        public void Should_be_able_to_use_v4_1_model_builder_without_enums()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<Enum_Product>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            Assert.Equal(2.0, databaseMapping.Model.Version);
            Assert.Equal(2.0, databaseMapping.Database.Version);
        }

        [Fact]
        public void Empty_enum_in_model_does_not_fail_validation()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Enum_Empty>();

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();
            databaseMapping.Assert<Enum_Empty>(p => p.Empty).IsFalse(e => e.EnumType.Members.Any());
        }

        [Fact]
        public void Unsigned_enum_in_model_should_fail_validation()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Enum_Unsigned>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
        }

        [Fact]
        public void Explicitly_mapping_an_enum_property_using_4_1_throws()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<Enum_Product>().Property(e => e.CategoryId);

            Assert.Throws<NotSupportedException>(
                () =>
                BuildMapping(modelBuilder))
                .ValidateMessage("UnsupportedUseOfV3Type", "Enum_Product", "CategoryId");
        }

        [Fact]
        public void Explicitly_mapping_a_nullable_enum_property_using_4_1_throws()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<Enum_Nullable>().Property(e => e.CategoryId);

            Assert.Throws<NotSupportedException>(
                () =>
                BuildMapping(modelBuilder))
                .ValidateMessage("UnsupportedUseOfV3Type", "Enum_Nullable", "CategoryId");
        }

        [Fact]
        public void Explicitly_mapping_an_enum_property_on_a_complex_type_using_4_1_throws()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.ComplexType<Enum_ComplexType>().Property(c => c.Enum);

            Assert.Throws<NotSupportedException>(
                () =>
                BuildMapping(modelBuilder))
                .ValidateMessage("UnsupportedUseOfV3Type", "Enum_ComplexType", "Enum");
        }

        [Fact]
        public void Explicitly_using_an_enum_property_as_a_key_with_4_1_throws()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<Enum_Product>().HasKey(e => e.CategoryId);

            Assert.Throws<NotSupportedException>(
                () =>
                BuildMapping(modelBuilder))
                .ValidateMessage("UnsupportedUseOfV3Type", "Enum_Product", "CategoryId");
        }

        [Fact]
        public void Explicitly_using_an_enum_property_as_part_of_a_composite_key_with_4_1_throws()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<Enum_Product>().HasKey(
                e => new
                         {
                             e.Id,
                             e.CategoryId
                         });

            Assert.Throws<NotSupportedException>(
                () =>
                BuildMapping(modelBuilder))
                .ValidateMessage("UnsupportedUseOfV3Type", "Enum_Product", "CategoryId");
        }

        [Fact]
        public void Explicitly_mapping_an_enum_property_with_Map_using_4_1_throws()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<Enum_Product>().Map(
                mapping =>
                    {
                        mapping.ToTable("Table1");
                        mapping.Properties(e => e.CategoryId);
                    });

            Assert.Throws<InvalidOperationException>(
                () =>
                BuildMapping(modelBuilder))
                .ValidateMessage("EntityMappingConfiguration_CannotMapIgnoredProperty", "Enum_Product", "CategoryId");
        }
    }

    namespace Fixtures
    {
        using System.Collections.Generic;
        using System.ComponentModel.DataAnnotations.Schema;

        public enum CategoryId
        {
            Beverages = 1,
            Condiments,
            Confections,
            Dairy_Products,
            Grains_Cereals,
            Meat_Poultry,
            Produce
        }

        public enum Empty
        {
        }

        public class Enum_Empty
        {
            public int Id { get; set; }
            public Empty Empty { get; set; }
        }

        public enum Unsigned : uint
        {
            Member
        }

        public class Enum_Unsigned
        {
            public int Id { get; set; }
            public Unsigned Unsigned { get; set; }
        }

        public class Enum_Product
        {
            public int Id { get; set; }
            public string ProductName { get; set; }
            public decimal UnitPrice { get; set; }
            public short UnitsInStock { get; set; }
            public CategoryId CategoryId { get; set; }
        }

        public class Enum_Nullable
        {
            public int Id { get; set; }
            public CategoryId? CategoryId { get; set; }
        }

        [Flags]
        public enum WithFlags : short
        {
            Beverages = 0,
            Condiments = 1,
            Confections = 2,
            Dairy_Products = 4
        }

        public class Enum_Flags
        {
            public int Id { get; set; }
            public WithFlags FlagsEnum { get; set; }
        }

        public class Enum_Product_PK
        {
            public CategoryId Id { get; set; }
            public string ProductName { get; set; }
            public decimal UnitPrice { get; set; }
            public short UnitsInStock { get; set; }
        }

        public class Enum_Fk_Product
        {
            public int Id { get; set; }
            public CategoryId CategoryId { get; set; }
            public Enum_Fk_Category Category { get; set; }
        }

        public class Enum_Fk_Category
        {
            public CategoryId Id { get; set; }
            public ICollection<Enum_Fk_Product> Products { get; set; }
        }

        public class Enum_IA_Principal
        {
            public CategoryId Id { get; set; }
            public ICollection<Enum_IA_Dependent> Dependents { get; set; }
        }

        public class Enum_IA_Dependent
        {
            public int Id { get; set; }
            public Enum_IA_Principal Principal { get; set; }
        }

        public class Enum_ComplexEntity
        {
            public int Id { get; set; }
            public Enum_ComplexType ComplexType { get; set; }
        }

        [ComplexType]
        public class Enum_ComplexType
        {
            [Column("col_enum")]
            public WithFlags? Enum { get; set; }
        }
    }
}
