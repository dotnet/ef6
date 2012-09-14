// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.Data.Entity;
    using FunctionalTests.Fixtures;
    using Xunit;

    namespace Fixtures
    {
        using System.ComponentModel.DataAnnotations;
        using System.ComponentModel.DataAnnotations.Schema;
        using System.Data.Entity.Spatial;

        public class Spatial_Customer
        {
            public int Id { get; set; }
            public DbGeometry Geometry { get; set; }

            [Required]
            public DbGeography Geography { get; set; }
        }

        public class Spatial_ComplexClass
        {
            public int Id { get; set; }
            public Spatial_ComplexType Complex { get; set; }
        }

        [ComplexType]
        public class Spatial_ComplexType
        {
            [Column("c")]
            public DbGeometry Geometry { get; set; }

            public DbGeography Geography { get; set; }
        }
    }

    public sealed class SpatialScenarioTests : TestBase
    {
        [Fact]
        public void Simple_spatial_mapping_scenario()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Spatial_Customer>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<Spatial_Customer>(c => c.Geography).DbEqual("geography", c => c.TypeName);
            databaseMapping.Assert<Spatial_Customer>(c => c.Geometry).DbEqual("geometry", c => c.TypeName);
        }

        [Fact]
        public void Complex_type_spatial_mapping_scenario()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Spatial_ComplexClass>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<Spatial_ComplexType>(c => c.Geography).DbEqual("geography", c => c.TypeName);
            databaseMapping.Assert<Spatial_ComplexType>(c => c.Geometry).DbEqual("geometry", c => c.TypeName);
        }

        [Fact]
        public void Configure_spatial_members_on_type()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Spatial_Customer>().Property(c => c.Geography).HasColumnName("the_column");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<Spatial_Customer>(c => c.Geography).DbEqual("the_column", c => c.Name);
        }

        [Fact]
        public void Should_be_able_to_use_v4_1_model_builder_without_spatials()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<Spatial_Customer>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            Assert.Equal(2.0, databaseMapping.Model.Version);
            Assert.Equal(2.0, databaseMapping.Database.Version);
        }

        [Fact]
        public void Explicitly_mapping_a_DbGeography_property_using_4_1_throws()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<Spatial_Customer>().Property(e => e.Geography);

            Assert.Throws<NotSupportedException>(
                () =>
                modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("UnsupportedUseOfV3Type", "Spatial_Customer", "Geography");
        }

        [Fact]
        public void Explicitly_mapping_a_DbGeography_property_on_a_complex_type_using_4_1_throws()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.ComplexType<Spatial_ComplexType>().Property(c => c.Geography);

            Assert.Throws<NotSupportedException>(
                () =>
                modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("UnsupportedUseOfV3Type", "Spatial_ComplexType", "Geography");
        }

        [Fact]
        public void Explicitly_using_a_DbGeography_property_as_a_key_with_4_1_throws()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<Spatial_Customer>().HasKey(e => e.Geography);

            Assert.Throws<NotSupportedException>(
                () =>
                modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("UnsupportedUseOfV3Type", "Spatial_Customer", "Geography");
        }

        [Fact]
        public void Explicitly_using_a_DbGeography_property_as_part_of_a_composite_key_with_4_1_throws()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<Spatial_Customer>().HasKey(
                e => new
                         {
                             e.Id,
                             e.Geography
                         });

            Assert.Throws<NotSupportedException>(
                () =>
                modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("UnsupportedUseOfV3Type", "Spatial_Customer", "Geography");
        }

        [Fact]
        public void Explicitly_mapping_a_DbGeography_property_with_Map_using_4_1_throws()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<Spatial_Customer>().Map(
                mapping =>
                {
                    mapping.ToTable("Table1");
                    mapping.Properties(e => e.Geography);
                });

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(
                    ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("EntityMappingConfiguration_CannotMapIgnoredProperty", "Spatial_Customer", "Geography");
        }

        [Fact]
        public void Explicitly_mapping_a_DbGeometry_property_using_4_1_throws()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<Spatial_Customer>().Property(e => e.Geometry);

            Assert.Throws<NotSupportedException>(
                () =>
                modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("UnsupportedUseOfV3Type", "Spatial_Customer", "Geometry");
        }

        [Fact]
        public void Explicitly_mapping_a_DbGeometry_property_on_a_complex_type_using_4_1_throws()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.ComplexType<Spatial_ComplexType>().Property(c => c.Geometry);

            Assert.Throws<NotSupportedException>(
                () =>
                modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("UnsupportedUseOfV3Type", "Spatial_ComplexType", "Geometry");
        }

        [Fact]
        public void Explicitly_using_a_DbGeometry_property_as_a_key_with_4_1_throws()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<Spatial_Customer>().HasKey(e => e.Geometry);

            Assert.Throws<NotSupportedException>(
                () =>
                modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("UnsupportedUseOfV3Type", "Spatial_Customer", "Geometry");
        }

        [Fact]
        public void Explicitly_using_a_DbGeometry_property_as_part_of_a_composite_key_with_4_1_throws()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<Spatial_Customer>().HasKey(
                e => new
                         {
                             e.Id,
                             e.Geometry
                         });

            Assert.Throws<NotSupportedException>(
                () =>
                modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("UnsupportedUseOfV3Type", "Spatial_Customer", "Geometry");
        }

        [Fact]
        public void Explicitly_mapping_a_DbGeometry_property_with_Map_using_4_1_throws()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<Spatial_Customer>().Map(
                mapping =>
                {
                    mapping.ToTable("Table1");
                    mapping.Properties(e => e.Geometry);
                });

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(
                    ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("EntityMappingConfiguration_CannotMapIgnoredProperty", "Spatial_Customer", "Geometry");
        }
    }
}
