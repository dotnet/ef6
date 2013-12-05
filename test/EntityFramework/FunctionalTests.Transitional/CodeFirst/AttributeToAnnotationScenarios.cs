// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Xunit;

    public class AttributeToAnnotationScenarios : TestBase
    {
        private static readonly AttributeToColumnAnnotationConvention<AnAttribute, string> _columnConvention =
            new AttributeToColumnAnnotationConvention<AnAttribute, string>(
                "Eeky", (p, a) => a.OrderBy(oa => oa.Data).Aggregate(p.Name + ": ", (s, na) => s + na.Data + " "));

        private static readonly AttributeToTableAnnotationConvention<AnAttribute, string> _tableConvention =
            new AttributeToTableAnnotationConvention<AnAttribute, string>(
                "Pandy", (t, a) => a.OrderBy(oa => oa.Data).Aggregate(t.Name + ": ", (s, na) => s + na.Data + " "));

        [Fact]
        public void AttributeToColumnAnnotationConvention_can_be_used_to_annotate_columns()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Conventions.Add(_columnConvention);
            modelBuilder.Entity<EntityWithAttributes>();
            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<EntityWithAttributes>("EntityWithAttributes")
                .Column("SimpleProp")
                .HasAnnotation("Eeky", "SimpleProp: A1 ");
        }

        [Fact]
        public void AttributeToColumnAnnotationConvention_on_properties_of_complex_type_can_be_used_to_annotate_columns()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Conventions.Add(_columnConvention);
            modelBuilder.Entity<EntityWithAttributes>();
            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<EntityWithAttributes>("EntityWithAttributes")
                .Column("Carbs1_Mmmm")
                .HasAnnotation("Eeky", "Mmmm: C1 ");

            databaseMapping.Assert<EntityWithAttributes>("EntityWithAttributes")
                .Column("Carbs1_MoreCarbs_Ahhh")
                .HasAnnotation("Eeky", "Ahhh: C2 ");

            databaseMapping.Assert<EntityWithAttributes>("EntityWithAttributes")
                .Column("Carbs2_Mmmm")
                .HasAnnotation("Eeky", "Mmmm: C1 ");

            databaseMapping.Assert<EntityWithAttributes>("EntityWithAttributes")
                .Column("Carbs2_MoreCarbs_Ahhh")
                .HasAnnotation("Eeky", "Ahhh: C2 ");
        }

        [Fact]
        public void Multiple_AttributeToColumnAnnotationConvention_can_be_used_on_a_single_column()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Conventions.Add(_columnConvention);
            modelBuilder.Entity<EntityWithAttributes>();
            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<EntityWithAttributes>("EntityWithAttributes")
                .Column("WithMultiple")
                .HasAnnotation("Eeky", "WithMultiple: A1 A2 ");
        }

        [Fact]
        public void AttributeToColumnAnnotationConvention_does_not_overwrite_index_annotation_added_by_fluent_API()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Conventions.Add(_columnConvention);
            modelBuilder.Entity<EntityWithAttributes>()
                .Property(p => p.SimpleProp)
                .HasAnnotation("Eeky", "Loves Pandy");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<EntityWithAttributes>("EntityWithAttributes")
                .Column("SimpleProp")
                .HasAnnotation("Eeky", "Loves Pandy");
        }

        [Fact]
        public void AttributeToTableAnnotationConvention_can_be_used_to_annotate_tables()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Conventions.Add(_tableConvention);
            modelBuilder.Entity<EntityWithAttributes>();
            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<EntityWithAttributes>("EntityWithAttributes")
                .HasAnnotation("Pandy", "EntityWithAttributes: T1 ");
        }

        [Fact]
        public void Multiple_AttributeToTableAnnotationConvention_can_be_used_on_a_single_table()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Conventions.Add(_tableConvention);
            modelBuilder.Entity<EntityWithTwoAttributes>();
            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<EntityWithTwoAttributes>("EntityWithTwoAttributes")
                .HasAnnotation("Pandy", "EntityWithTwoAttributes: T1 T2 ");
        }

        [Fact]
        public void AttributeToTableAnnotationConvention_does_not_overwrite_index_annotation_added_by_fluent_API()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Conventions.Add(_tableConvention);
            modelBuilder.Entity<EntityWithAttributes>()
                .HasAnnotation("Pandy", "Loves Eeky");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<EntityWithAttributes>("EntityWithAttributes")
                .HasAnnotation("Pandy", "Loves Eeky");
        }

        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
        public class AnAttribute : Attribute
        {
            public AnAttribute(string data)
            {
                Data = data;
            }

            public string Data { get; set; }

            public override object TypeId
            {
                get { return RuntimeHelpers.GetHashCode(this); }
            }
        }

        [AnAttribute("T1")]
        public class EntityWithAttributes
        {
            public int Id { get; set; }

            [AnAttribute("A1")]
            public string SimpleProp { get; set; }

            [AnAttribute("A1")]
            [AnAttribute("A2")]
            public string WithMultiple { get; set; }

            public AllCarbs Carbs1 { get; set; }
            public AllCarbs Carbs2 { get; set; }
        }

        [AnAttribute("T1")]
        [AnAttribute("T2")]
        public class EntityWithTwoAttributes
        {
            public int Id { get; set; }
        }

        [ComplexType]
        public class AllCarbs
        {
            [AnAttribute("C1")]
            public int Mmmm { get; set; }

            public ComplexCarbs MoreCarbs { get; set; }
        }

        [ComplexType]
        public class ComplexCarbs
        {
            [AnAttribute("C2")]
            public int Ahhh { get; set; }
        }
    }
}
