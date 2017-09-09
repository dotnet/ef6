// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.Data.Entity;
    using FunctionalTests.Fixtures;
    using Xunit;

    public sealed class HierarchyIdScenarioTests : TestBase
    {
        [Fact]
        public void Simple_hierarchyid_mapping_scenario()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<HierarchyId_Customer>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<HierarchyId_Customer>(c => c.Path).DbEqual("hierarchyid", c => c.TypeName);
        }

        [Fact]
        public void Complex_type_hierarchyid_mapping_scenario()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<HierarchyId_ComplexClass>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<HierarchyId_ComplexType>(c => c.Path).DbEqual("hierarchyid", c => c.TypeName);
        }

        [Fact]
        public void Configure_hierarchyid_members_on_type()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<HierarchyId_Customer>().Property(c => c.Path).HasColumnName("the_column");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<HierarchyId_Customer>(c => c.Path).DbEqual("the_column", c => c.Name);
        }

        [Fact]
        public void Should_be_able_to_use_v4_1_model_builder_without_hierarchyids()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<HierarchyId_Customer>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            Assert.Equal(2.0, databaseMapping.Model.SchemaVersion);
            Assert.Equal(2.0, databaseMapping.Database.SchemaVersion);
        }

        [Fact]
        public void Explicitly_using_a_HierarchyId_property_as_a_key_with_4_1_throws()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<HierarchyId_Customer>().HasKey(e => e.Path);

            Assert.Throws<NotSupportedException>(
                () =>
                modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("UnsupportedUseOfV3Type", "HierarchyId_Customer", "Path");
        }

        [Fact]
        public void Explicitly_using_a_HierarchyId_property_as_part_of_a_composite_key_with_4_1_throws()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<HierarchyId_Customer>().HasKey(
                e => new
                {
                    e.Id,
                    e.Path
                });

            Assert.Throws<NotSupportedException>(
                () =>
                modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("UnsupportedUseOfV3Type", "HierarchyId_Customer", "Path");
        }

        [Fact]
        public void Explicitly_mapping_a_HierarchyId_property_with_Map_using_4_1_throws()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<HierarchyId_Customer>().Map(
                mapping =>
                {
                    mapping.ToTable("Table1");
                    mapping.Properties(e => e.Path);
                });

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(
                    ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("EntityMappingConfiguration_CannotMapIgnoredProperty", "HierarchyId_Customer", "Path");
        }

        [Fact]
        public void Explicitly_mapping_a_HierarchyId_property_using_4_1_throws()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<HierarchyId_Customer>().Property(e => e.Path);

            Assert.Throws<NotSupportedException>(
                () =>
                modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("UnsupportedUseOfV3Type", "HierarchyId_Customer", "Path");
        }

        [Fact]
        public void Explicitly_mapping_a_HierarchyId_property_on_a_complex_type_using_4_1_throws()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.ComplexType<HierarchyId_ComplexType>().Property(c => c.Path);

            Assert.Throws<NotSupportedException>(
                () =>
                modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("UnsupportedUseOfV3Type", "HierarchyId_ComplexType", "Path");
        }
    }

    namespace Fixtures
    {
        using System.ComponentModel.DataAnnotations;
        using System.ComponentModel.DataAnnotations.Schema;
        using System.Data.Entity.Hierarchy;

        public class HierarchyId_Customer
        {
            public int Id { get; set; }

            [Required]
            public HierarchyId Path { get; set; }
        }

        public class HierarchyId_ComplexClass
        {
            public int Id { get; set; }
            public HierarchyId_ComplexType Complex { get; set; }
        }

        [ComplexType]
        public class HierarchyId_ComplexType
        {
            [Column("c")]
            public HierarchyId Path { get; set; }
        }
    }
}
