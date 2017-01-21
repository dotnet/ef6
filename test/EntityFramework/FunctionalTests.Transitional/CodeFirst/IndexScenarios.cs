// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Core;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.TestHelpers;
    using Xunit;

    public class IndexScenarios : TestBase
    {
        [Fact]
        public void IndexAttribute_can_be_used_to_annotate_columns()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<EntityWithIndexes>();
            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<EntityWithIndexes>("EntityWithIndexes")
                .Column("SimpleProp")
                .HasAnnotation(
                    "Index",
                    new IndexAnnotation(new IndexAttribute()),
                    new IndexAnnotationEqualityComparer());
        }

        [Fact]
        public void IndexAttribute_on_properties_of_complex_type_can_be_used_to_annotate_columns()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<EntityWithIndexes>();
            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<EntityWithIndexes>("EntityWithIndexes")
                .Column("Carbs1_Mmmm")
                .HasAnnotation(
                    "Index",
                    new IndexAnnotation(new IndexAttribute("C1")),
                    new IndexAnnotationEqualityComparer());

            databaseMapping.Assert<EntityWithIndexes>("EntityWithIndexes")
                .Column("Carbs1_MoreCarbs_Ahhh")
                .HasAnnotation(
                    "Index",
                    new IndexAnnotation(new IndexAttribute("C2")),
                    new IndexAnnotationEqualityComparer());

            databaseMapping.Assert<EntityWithIndexes>("EntityWithIndexes")
                .Column("Carbs2_Mmmm")
                .HasAnnotation(
                    "Index",
                    new IndexAnnotation(new IndexAttribute("C1")),
                    new IndexAnnotationEqualityComparer());

            databaseMapping.Assert<EntityWithIndexes>("EntityWithIndexes")
                .Column("Carbs2_MoreCarbs_Ahhh")
                .HasAnnotation(
                    "Index",
                    new IndexAnnotation(new IndexAttribute("C2")),
                    new IndexAnnotationEqualityComparer());
        }

        [Fact]
        public void Multiple_IndexAttribute_can_be_used_on_a_single_column()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<EntityWithIndexes>();
            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<EntityWithIndexes>("EntityWithIndexes")
                .Column("WithMultiple")
                .HasAnnotation(
                    "Index",
                    new IndexAnnotation(new[] { new IndexAttribute("I1"), new IndexAttribute("I2") }),
                    new IndexAnnotationEqualityComparer());
        }

        [Fact]
        public void IndexAttribute_does_not_overwrite_index_annotation_added_by_fluent_API()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<EntityWithIndexes>()
                .Property(p => p.SimpleProp)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("Fluent")));

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<EntityWithIndexes>("EntityWithIndexes")
                .Column("SimpleProp")
                .HasAnnotation(
                    "Index",
                    new IndexAnnotation(new IndexAttribute("Fluent")),
                    new IndexAnnotationEqualityComparer());
        }

        [Fact]
        public void Options_specified_in_IndexAttribute_make_it_into_the_annotation()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<EntityWithIndexes>();
            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<EntityWithIndexes>("EntityWithIndexes")
                .Column("WithOptions")
                .HasAnnotation(
                    "Index",
                    new IndexAnnotation(
                        new[]
                        {
                            new IndexAttribute("I1"),
                            new IndexAttribute("I2", 6),
                            new IndexAttribute("I3", 7),
                            new IndexAttribute("I4") { IsClustered = true },
                            new IndexAttribute("I5") { IsUnique = true },
                            new IndexAttribute("I6", 8) { IsClustered = true },
                            new IndexAttribute("I7", 9) { IsClustered = true },
                            new IndexAttribute("I8", 10) { IsUnique = true },
                            new IndexAttribute("I9", 11) { IsUnique = true },
                            new IndexAttribute("I10", 12) { IsClustered = true, IsUnique = true },
                            new IndexAttribute("I11", 13) { IsClustered = true, IsUnique = true }
                        }),
                    new IndexAnnotationEqualityComparer());
        }

        [Fact]
        public void Non_conflicting_duplicate_definitions_of_the_same_index_on_the_same_property_are_merged()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<EntityWithIndexes>();
            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<EntityWithIndexes>("EntityWithIndexes")
                .Column("WithDupes")
                .HasAnnotation(
                    "Index",
                    new IndexAnnotation(
                        new[]
                        {
                            new IndexAttribute("I1", 6) { IsClustered = true, IsUnique = true }
                        }),
                    new IndexAnnotationEqualityComparer());
        }

        public class EntityWithIndexes
        {
            public int Id { get; set; }
            
            [Index]
            public string SimpleProp { get; set; }

            [Index("I1")]
            [Index("I2")]
            public string WithMultiple { get; set; }

            [Index("I1")]
            [Index("I2", 6)]
            [Index("I3", Order = 7)]
            [Index("I4", IsClustered = true)]
            [Index("I5", IsUnique = true)]
            [Index("I6", 8, IsClustered = true)]
            [Index("I7", Order = 9, IsClustered = true)]
            [Index("I8", 10, IsUnique = true)]
            [Index("I9", Order = 11, IsUnique = true)]
            [Index("I10", 12, IsClustered = true, IsUnique = true)]
            [Index("I11", Order = 13, IsClustered = true, IsUnique = true)]
            public string WithOptions { get; set; }

            [Index("I1")]
            [Index("I1", 6)]
            [Index("I1", Order = 6)]
            [Index("I1", IsClustered = true)]
            [Index("I1", IsUnique = true)]
            [Index("I1", 6, IsClustered = true)]
            [Index("I1", Order = 6, IsClustered = true)]
            [Index("I1", 6, IsUnique = true)]
            [Index("I1", Order = 6, IsUnique = true)]
            [Index("I1", 6, IsClustered = true, IsUnique = true)]
            [Index("I1", Order = 6, IsClustered = true, IsUnique = true)]
            public string WithDupes { get; set; }

            public AllCarbs Carbs1 { get; set; }
            public AllCarbs Carbs2 { get; set; }
        }

        [ComplexType]
        public class AllCarbs
        {
            [Index("C1")]
            public int Mmmm { get; set; }
            public ComplexCarbs MoreCarbs { get; set; }
        }

        [ComplexType]
        public class ComplexCarbs
        {
            [Index("C2")]
            public int Ahhh { get; set; }
        }

        [Fact]
        public void Indexes_configured_on_multiple_properties_can_unify_to_same_column()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<TeaPeaHBase>();
            modelBuilder.Entity<TeaPeaH1>().Property(e => e.Prop1A).HasColumnName("MyCatHasPaws");
            modelBuilder.Entity<TeaPeaH2>().Property(e => e.Prop2A).HasColumnName("MyCatHasPaws");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<TeaPeaHBase>("TeaPeaHBases")
                .Column("MyCatHasPaws")
                .HasAnnotation(
                    "Index",
                    new IndexAnnotation(
                        new[]
                        {
                            new IndexAttribute("I1", 6) { IsClustered = false, IsUnique = false },
                            new IndexAttribute("I2", 7) { IsClustered = true, IsUnique = true },
                            new IndexAttribute("I3", 8) { IsClustered = false, IsUnique = true },
                            new IndexAttribute("I4", 9) { IsClustered = true, IsUnique = false }
                        }),
                    new IndexAnnotationEqualityComparer());
        }

        [Fact]
        public void Non_conflicting_duplicate_index_definitions_configured_on_multiple_properties_are_merged()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<TeaPeaHBase>();
            modelBuilder.Entity<TeaPeaH1>().Property(e => e.Prop1B).HasColumnName("MyCatHasPaws");
            modelBuilder.Entity<TeaPeaH2>().Property(e => e.Prop2B).HasColumnName("MyCatHasPaws");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<TeaPeaHBase>("TeaPeaHBases")
                .Column("MyCatHasPaws")
                .HasAnnotation(
                    "Index",
                    new IndexAnnotation(
                        new[]
                        {
                            new IndexAttribute("I0", 6) { IsClustered = true, IsUnique = true },
                            new IndexAttribute("I1", 6) { IsClustered = false, IsUnique = false },
                            new IndexAttribute("I2", 7) { IsClustered = true, IsUnique = true },
                            new IndexAttribute("I3", 8) { IsClustered = false, IsUnique = true },
                            new IndexAttribute("I4", 9) { IsClustered = true, IsUnique = false }
                        }),
                    new IndexAnnotationEqualityComparer());
        }

        public class TeaPeaHBase
        {
            public int Id { get; set; }
        }

        public class TeaPeaH1 : TeaPeaHBase
        {
            [Index("I1", 6, IsClustered = false, IsUnique = false)]
            [Index("I2", 7, IsClustered = true, IsUnique = true)]
            public int Prop1A { get; set; }

            [Index("I1", 6, IsClustered = false, IsUnique = false)]
            [Index("I0")]
            [Index("I0", Order = 6)]
            [Index("I0", IsUnique = true)]
            [Index("I0", Order = 6, IsClustered = true)]
            [Index("I0", Order = 6, IsUnique = true)]
            [Index("I0", Order = 6, IsClustered = true, IsUnique = true)]
            [Index("I2", 7, IsClustered = true, IsUnique = true)]
            public int Prop1B { get; set; }
        }

        public class TeaPeaH2 : TeaPeaHBase
        {
            [Index("I3", 8, IsClustered = false, IsUnique = true)]
            [Index("I4", 9, IsClustered = true, IsUnique = false)]
            public int Prop2A { get; set; }

            [Index("I3", 8, IsClustered = false, IsUnique = true)]
            [Index("I0", 6)]
            [Index("I0", IsClustered = true)]
            [Index("I0", 6, IsClustered = true)]
            [Index("I0", 6, IsUnique = true)]
            [Index("I0", 6, IsClustered = true, IsUnique = true)]
            [Index("I4", 9, IsClustered = true, IsUnique = false)]
            public int Prop2B { get; set; }
        }

        [Fact]
        public void Indexes_configured_on_a_single_property_can_fan_out_to_multiple_columns()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<SplitPeaSoup>()
                .Map(m => m.ToTable("Left").Properties(p => new { p.Id, p.Prop1 }))
                .Map(m => m.ToTable("Right").Properties(p => new { p.Id, p.Prop2 }));

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<SplitPeaSoup>("Left")
                .Column("Id")
                .HasAnnotation(
                    "Index",
                    new IndexAnnotation(
                        new[]
                        {
                            new IndexAttribute("I0", 5) { IsClustered = false, IsUnique = false },
                        }),
                    new IndexAnnotationEqualityComparer());

            databaseMapping.Assert<SplitPeaSoup>("Left")
                .Column("Prop1")
                .HasAnnotation(
                    "Index",
                    new IndexAnnotation(
                        new[]
                        {
                            new IndexAttribute("I1", 6) { IsClustered = false, IsUnique = false },
                            new IndexAttribute("I2", 7) { IsClustered = true, IsUnique = true },
                        }),
                    new IndexAnnotationEqualityComparer());

            databaseMapping.Assert<SplitPeaSoup>("Right")
                .Column("Id")
                .HasAnnotation(
                    "Index",
                    new IndexAnnotation(
                        new[]
                        {
                            new IndexAttribute("I0", 5) { IsClustered = false, IsUnique = false },
                        }),
                    new IndexAnnotationEqualityComparer());

            databaseMapping.Assert<SplitPeaSoup>("Right")
                .Column("Prop2")
                .HasAnnotation(
                    "Index",
                    new IndexAnnotation(
                        new[]
                        {
                            new IndexAttribute("I3", 8) { IsClustered = false, IsUnique = true },
                            new IndexAttribute("I4", 9) { IsClustered = true, IsUnique = false }
                        }),
                    new IndexAnnotationEqualityComparer());
        }

        public class SplitPeaSoup
        {
            [Index("I0", 5, IsClustered = false, IsUnique = false)]
            public int Id { get; set; }

            [Index("I1", 6, IsClustered = false, IsUnique = false)]
            [Index("I2", 7, IsClustered = true, IsUnique = true)]
            public int Prop1 { get; set; }

            [Index("I3", 8, IsClustered = false, IsUnique = true)]
            [Index("I4", 9, IsClustered = true, IsUnique = false)]
            public int Prop2 { get; set; }
        }

        [Fact]
        public void Conflicts_are_detected_in_in_multiple_attributes_on_the_same_property()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<EntityWithIndexConflicts>();

            var exception = Assert.Throws<InvalidOperationException>(() => BuildMapping(modelBuilder));

            exception.ValidateMessage(
                "ConflictingIndexAttributesOnProperty", "WithConflicts", "EntityWithIndexConflicts", "IX",
                BuildConflictDetails("IsClustered", "True", "False"));
        }

        private static string BuildConflictDetails(string propertyName, string value1, string value2)
        {
            return Environment.NewLine + "\t"
                   + string.Format(
                       LookupString(
                           EntityFrameworkAssembly, "System.Data.Entity.Properties.Resources", "ConflictingIndexAttributeProperty"),
                       propertyName, value1, value2);
        }

        public class EntityWithIndexConflicts
        {
            public int Id { get; set; }

            // These are fine: all named differently
            [Index("I1")]
            [Index("I2", 6)]
            [Index("I3", Order = 7)]
            [Index("I4", IsClustered = true)]
            [Index("I5", IsUnique = true)]
            [Index("I6", 8, IsClustered = true)]
            [Index("I7", Order = 9, IsClustered = true)]
            [Index("I8", 10, IsUnique = true)]
            [Index("I9", Order = 11, IsUnique = true)]
            [Index("I10", 12, IsClustered = true, IsUnique = true)]
            [Index("I11", Order = 13, IsClustered = true, IsUnique = true)]

            // These are fine: no options conflicts; can be merged
            [Index("IX")]
            [Index("IX", 6)]
            [Index("IX", Order = 6)]
            [Index("IX", 6, IsUnique = true, IsClustered = true)]

            // This one is bad: IsClustered conflicts
            [Index("IX", IsClustered = false)]
            public string WithConflicts { get; set; }
        }

        [Fact]
        public void Conflicts_are_detected_in_in_attributes_that_converge_on_the_same_column()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<BadTeaPeaHBase>();
            modelBuilder.Entity<BadTeaPeaH1>();
            modelBuilder.Entity<BadTeaPeaH2>();

            modelBuilder.Properties()
                .Where(p => p.Name == "Prop1")
                .Configure(p => p.HasColumnName("MyCatHasPaws"));

            modelBuilder.Properties()
                .Where(p => p.Name == "Prop2")
                .Configure(p => p.HasColumnName("MyCatHasPaws"));

            var exception = Assert.Throws<MappingException>(() => BuildMapping(modelBuilder));

            exception.ValidateMessage(
                "BadTphMappingToSharedColumn", "Prop1", "BadTeaPeaH1", "Prop2", "BadTeaPeaH2", "MyCatHasPaws", "BadTeaPeaHBase",
                BuildConflictDetails("IsUnique", "True", "False"));
        }

        public class BadTeaPeaHBase
        {
            public int Id { get; set; }
        }

        public class BadTeaPeaH1 : BadTeaPeaHBase
        {
            [Index("I1", 6, IsClustered = false, IsUnique = false)]
            [Index("I0", IsUnique = true)]
            [Index("I2", 7, IsClustered = true, IsUnique = true)]
            public int Prop1 { get; set; }
        }

        public class BadTeaPeaH2 : BadTeaPeaHBase
        {
            [Index("I3", 8, IsClustered = false, IsUnique = true)]
            [Index("I0", IsUnique = false)]
            [Index("I4", 9, IsClustered = true, IsUnique = false)]
            public int Prop2 { get; set; }
        }
    }
}
