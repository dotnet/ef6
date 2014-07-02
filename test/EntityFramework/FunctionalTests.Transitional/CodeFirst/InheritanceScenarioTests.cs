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
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Linq;
    using FunctionalTests.Model;
    using SimpleModel;
    using Xunit;
    using Product = FunctionalTests.Model.Product;

    public class InheritanceScenarioTests : TestBase
    {
        public class Contract
        {
            public long Id { get; set; }
            public DateTime X { get; set; }
        }

        public class ContractRevision : Contract
        {
        }

        public class InitialContract : Contract
        {
        }

        [Fact]
        public void Hybrid_tpt_tph_should_introduce_discriminator_for_tph()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Contract>();
            modelBuilder.Entity<ContractRevision>().ToTable("TPH");
            modelBuilder.Entity<InitialContract>().ToTable("TPH");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<ContractRevision>().HasColumn("Discriminator");
            databaseMapping.Assert<InitialContract>().HasColumn("Discriminator");
        }

        [Fact]
        public void Hybrid_tpt_tph_should_introduce_discriminator_for_tph_no_base_properties()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Contract>().Ignore(c => c.X);
            modelBuilder.Entity<ContractRevision>().ToTable("TPH");
            modelBuilder.Entity<InitialContract>().ToTable("TPH");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<ContractRevision>().HasColumn("Discriminator");
            databaseMapping.Assert<InitialContract>().HasColumn("Discriminator");
        }

        [Fact]
        public void Hybrid_tpt_tph_should_introduce_discriminator_for_tph_leaf_tpt()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Contract>();
            modelBuilder.Entity<ContractRevision>();
            modelBuilder.Entity<InitialContract>().ToTable("TPT");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<ContractRevision>().HasColumn("Discriminator");
            databaseMapping.Assert<Contract>().HasColumn("Discriminator");
        }

        public class ApplicationUser
        {
            public virtual string Id { get; set; }
        }

        public abstract class Ad
        {
            public int AdID { get; set; }

            public string UserId { get; set; }
            public virtual ApplicationUser User { get; set; }

            public string Name { get; set; }
        }

        public class Boat : Ad
        {
        }

        [Fact]
        public void Tpc_with_association_from_abstract_base_should_not_fail_validation()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers");
            modelBuilder.Entity<Boat>().Map(
                m =>
                {
                    m.MapInheritedProperties();
                    m.ToTable("Boats");
                });
            modelBuilder.Entity<Ad>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
        }

        public abstract class MessageBase
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Contents { get; set; }
        }

        public class Comment : MessageBase
        {
            public MessageBase Parent { get; set; }
            public int? ParentId { get; set; }
        }

        [Fact]
        public void Identity_convention_applied_when_self_ref()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MessageBase>();
            modelBuilder.Entity<Comment>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<MessageBase>("MessageBases").Column("Id").DbEqual(StoreGeneratedPattern.Identity, c => c.StoreGeneratedPattern);
        }

        public class Person1545
        {
            public int Id { get; set; }

            [StringLength(5)]
            public string Name { get; set; }
        }

        public class Employee1545 : Person1545
        {
        }

        [Fact]
        public void Can_override_annotation_when_tph()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Person1545>();
            modelBuilder.Entity<Employee1545>()
                .Property(p => p.Name)
                .HasMaxLength(10);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<Person1545>(p => p.Name).FacetEqual(10, p => p.MaxLength);
        }

        [Fact]
        public void Orphaned_configured_table_should_throw()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<BaseDependent_165027>().HasKey(
                e => new
                    {
                        e.Key1,
                        e.Key2,
                    });
            modelBuilder.Entity<Dependent_165027>()
                        .Map(
                            mapping =>
                                {
                                    mapping.MapInheritedProperties();
                                    mapping.ToTable("Dependent");
                                });
            modelBuilder.Entity<BaseDependent_165027>()
                        .Map(
                            mapping =>
                                {
                                    mapping.MapInheritedProperties();
                                    mapping.ToTable("BaseDependent");
                                });

            Assert.Throws<InvalidOperationException>(
                () => BuildMapping(modelBuilder))
                  .ValidateMessage("OrphanedConfiguredTableDetected", "BaseDependent");
        }

        [Fact]
        public void Orphaned_unconfigured_table_should_be_removed()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<BaseDependent_165027>().HasKey(
                e => new
                    {
                        e.Key1,
                        e.Key2,
                    });
            modelBuilder.Entity<Dependent_165027>()
                        .Map(
                            mapping =>
                                {
                                    mapping.MapInheritedProperties();
                                    mapping.ToTable("Dependent");
                                });
            modelBuilder.Entity<BaseDependent_165027>()
                        .Map(mapping => mapping.MapInheritedProperties());

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal(1, databaseMapping.Database.EntityTypes.Count());
        }

        public abstract class BaseDependent_165027
        {
            public decimal? BaseProperty { get; set; }
            public float? Key1 { get; set; }
            public decimal? Key2 { get; set; }
        }

        public class Dependent_165027 : BaseDependent_165027
        {
        }

        [Fact]
        public void Should_throw_when_configuring_base_properties_via_derived_type_conflicting()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Base_195898>().ToTable("Base");
            modelBuilder.Entity<Base_195898>().Property(b => b.Id).HasColumnName("base_c");
            modelBuilder.Entity<Base_195898>().Property(b => b.Complex.Foo).HasColumnName("base_foo");
            modelBuilder.Entity<Derived_195898>().ToTable("Derived");
            modelBuilder.Entity<Derived_195898>().Property(d => d.Id).HasColumnName("derived_c");
            modelBuilder.Entity<Derived_195898>().Property(d => d.Complex.Foo).HasColumnName("derived_foo");  // conflict as TPT

            Assert.Throws<InvalidOperationException>(() => BuildMapping(modelBuilder));
        }

        [Fact]
        public void Should_throw_when_configuring_base_properties_via_derived_type_reverse_conflicting()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Derived_195898>().ToTable("Derived");
            modelBuilder.Entity<Derived_195898>().Property(d => d.Id).HasColumnName("derived_c");
            modelBuilder.Entity<Derived_195898>().Property(d => d.Complex.Foo).HasColumnName("derived_foo"); // conflict as TPT
            modelBuilder.Entity<Base_195898>().ToTable("Base");
            modelBuilder.Entity<Base_195898>().Property(b => b.Id).HasColumnName("base_c");
            modelBuilder.Entity<Base_195898>().Property(b => b.Complex.Foo).HasColumnName("base_foo");

            Assert.Throws<InvalidOperationException>(() => BuildMapping(modelBuilder));
        }

        [Fact]
        public void Should_be_able_configure_derived_property_and_base_property_is_not_configured()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Base_195898>().ToTable("Base");
            modelBuilder.Entity<Derived_195898>().ToTable("Derived");
            modelBuilder.Entity<Derived_195898>().Property(d => d.Id).HasColumnName("derived_c");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            databaseMapping.Assert<Base_195898>(b => b.Id).DbEqual("Id", c => c.Name);
            databaseMapping.Assert<Complex_195898>(b => b.Foo).DbEqual("Complex_Foo", c => c.Name);
            databaseMapping.Assert<Derived_195898>(b => b.Id).DbEqual("derived_c", c => c.Name);
        }

        [Fact]
        public void Should_be_able_configure_base_property_and_derived_property_inherits_configuration()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Base_195898>().ToTable("Base");
            modelBuilder.Entity<Base_195898>().Property(d => d.Id).HasColumnName("base_c");
            modelBuilder.Entity<Derived_195898>().ToTable("Derived");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            databaseMapping.Assert<Base_195898>(b => b.Id).DbEqual("base_c", c => c.Name);
            databaseMapping.Assert<Complex_195898>(b => b.Foo).DbEqual("Complex_Foo", c => c.Name);
            databaseMapping.Assert<Derived_195898>(d => d.Id).DbEqual("base_c", c => c.Name);
        }

        public class Base_195898
        {
            public int Id { get; set; }
            public Complex_195898 Complex { get; set; }
        }

        public class Derived_195898 : Base_195898
        {
        }

        public class Complex_195898
        {
            public string Foo { get; set; }
        }

        [Fact]
        public void Columns_should_get_preferred_names_when_distinct_in_target_table()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<BaseEntityDuplicateProps>().ToTable("BaseEntities");
            modelBuilder.Entity<Entity1DuplicateProps>().ToTable("Entity1s");
            modelBuilder.Entity<Entity2DuplicateProps>().ToTable("Entity2s");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<Entity1DuplicateProps>(e => e.SomeProperty).DbEqual("SomeProperty", c => c.Name);
            databaseMapping.Assert<Entity2DuplicateProps>(e => e.SomeProperty).DbEqual("SomeProperty", c => c.Name);
        }

        [Fact]
        public void Columns_should_get_configured_names_when_distinct_in_target_table()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<BaseEntityDuplicateProps>().ToTable("BaseEntities");
            modelBuilder.Entity<Entity1DuplicateProps>().ToTable("Entity1s");
            modelBuilder.Entity<Entity2DuplicateProps>().ToTable("Entity2s");
            
            modelBuilder.Entity<Entity1DuplicateProps>()
                .Property(e => e.SomeProperty)
                .HasColumnName("Foo")
                .HasColumnAnnotation("Annotation1", "Entity1")
                .HasColumnAnnotation("Annotation2", "Common");
            
            modelBuilder.Entity<Entity2DuplicateProps>()
                .Property(e => e.SomeProperty)
                .HasColumnName("Foo")
                .HasColumnAnnotation("Annotation1", "Entity2")
                .HasColumnAnnotation("Annotation2", "Common");
            
            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<Entity1DuplicateProps>(e => e.SomeProperty).DbEqual("Foo", c => c.Name);
            databaseMapping.Assert<Entity2DuplicateProps>(e => e.SomeProperty).DbEqual("Foo", c => c.Name);

            databaseMapping.Assert<Entity1DuplicateProps>("Entity1s")
                .Column("Foo")
                .HasAnnotation("Annotation1", "Entity1")
                .HasAnnotation("Annotation2", "Common");

            databaseMapping.Assert<Entity2DuplicateProps>("Entity2s")
                .Column("Foo")
                .HasAnnotation("Annotation1", "Entity2")
                .HasAnnotation("Annotation2", "Common");
        }

        public class BaseEntityDuplicateProps
        {
            public int ID { get; set; }
            public string Title { get; set; }
        }

        public class Entity1DuplicateProps : BaseEntityDuplicateProps
        {
            public string SomeProperty { get; set; }
            public int Entity2ID { get; set; }
            public Entity2DuplicateProps Entity2 { get; set; }
        }

        public class Entity2DuplicateProps : BaseEntityDuplicateProps
        {
            public string SomeProperty { get; set; }
        }

        [Fact]
        public void Column_should_get_only_annotations_configured_for_that_column_when_property_maps_to_multiple_columns()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SplitTeaSoup>()
                .Map(
                    m =>
                    {
                        m.ToTable("Left").Properties(p => new { p.Id, p.Prop1 });
                        m.Property(p => p.Id).HasColumnAnnotation("A0", "V01").HasColumnAnnotation("A1", "V1");
                    })
                .Map(
                    m =>
                    {
                        m.ToTable("Right").Properties(p => new { p.Id, p.Prop2 });
                        m.Property(p => p.Id).HasColumnAnnotation("A0", "V02").HasColumnAnnotation("A2", "V2");
                    });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            databaseMapping.Assert<SplitTeaSoup>("Left")
                .Column("Id")
                .HasAnnotation("A0", "V01")
                .HasAnnotation("A1", "V1")
                .HasNoAnnotation("A2");

            databaseMapping.Assert<SplitTeaSoup>("Right")
                .Column("Id")
                .HasAnnotation("A0", "V02")
                .HasAnnotation("A2", "V2")
                .HasNoAnnotation("A1");
        }

        [Fact]
        public void Column_should_get_only_annotations_configured_for_that_column_and_all_columns()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SplitTeaSoup>()
                .Map(
                    m =>
                    {
                        m.ToTable("Left").Properties(p => new { p.Id, p.Prop1 });
                        m.Property(p => p.Id).HasColumnAnnotation("A0", "V01").HasColumnAnnotation("A1", "V1");
                    })
                .Map(
                    m =>
                    {
                        m.ToTable("Right").Properties(p => new { p.Id, p.Prop2 });
                        m.Property(p => p.Id).HasColumnAnnotation("A0", "V02").HasColumnAnnotation("A2", "V2");
                    });
            modelBuilder.Entity<SplitTeaSoup>().Property(e => e.Id).HasColumnAnnotation("A3", "V3");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            databaseMapping.Assert<SplitTeaSoup>("Left")
                .Column("Id")
                .HasAnnotation("A0", "V01")
                .HasAnnotation("A1", "V1")
                .HasAnnotation("A3", "V3")
                .HasNoAnnotation("A2");

            databaseMapping.Assert<SplitTeaSoup>("Right")
                .Column("Id")
                .HasAnnotation("A0", "V02")
                .HasAnnotation("A2", "V2")
                .HasAnnotation("A3", "V3")
                .HasNoAnnotation("A1");
        }

        public class SplitTeaSoup
        {
            public int Id { get; set; }
            public string Prop1 { get; set; }
            public string Prop2 { get; set; }
        }

        [Fact]
        public void Build_model_for_simple_tpt()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Product>();
            modelBuilder.Entity<ColoredProduct>().ToTable("ColoredProducts");
            modelBuilder.Entity<StyledProduct>().ToTable("StyledProducts");

            var databaseMapping = BuildMapping(modelBuilder);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());
        }

        [Fact]
        public void Build_model_for_tpt_tph()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<DiscontinuedProduct>().ToTable("DiscontinuedProducts");
            modelBuilder.Entity<StyledProduct>()
                        .Map(
                            m =>
                                {
                                    m.Requires("disc").HasValue("S");
                                    m.ToTable("StyledProducts");
                                });
            modelBuilder.Entity<ColoredProduct>()
                        .Map(
                            m =>
                                {
                                    m.Requires("disc").HasValue("C");
                                    m.ToTable("StyledProducts");
                                });

            var databaseMapping = BuildMapping(modelBuilder);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());
        }

        [Fact]
        public void Build_model_for_split_tpt_tph()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Product>();
            modelBuilder.Entity<DiscontinuedProduct>();
            modelBuilder.Entity<StyledProduct>().ToTable("StyledProducts");
            modelBuilder.Entity<ColoredProduct>();

            var databaseMapping = BuildMapping(modelBuilder);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());
        }

        [Fact]
        public void Build_model_for_tpc_with_default_tph_in_part_of_tree()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Product>();
            modelBuilder.Entity<DiscontinuedProduct>();
            modelBuilder.Entity<StyledProduct>()
                        .Map(
                            m =>
                                {
                                    m.MapInheritedProperties();
                                    m.ToTable("StyledProducts");
                                });

            Assert.Throws<NotSupportedException>(
                () => BuildMapping(modelBuilder));
        }

        [Fact]
        public void Build_model_for_three_level_abstract_types_tpt()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<AbstractType1>().ToTable("AbstractType1").HasKey(a => a.Property1_ID);
            modelBuilder.Entity<AbstractType1_1>().ToTable("AbstractType1_1");
            modelBuilder.Entity<ConcreteType1_1_1>().ToTable("ConcreteType1_1_1");
            modelBuilder.Entity<ConcreteType1_2>().ToTable("ConcreteType1_2");

            var databaseMapping = BuildMapping(modelBuilder);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());
        }

        [Fact]
        public void Build_model_for_tree_containing_only_abstract_types()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<AbstractType1>().HasKey(a => a.Property1_ID);
            modelBuilder.Entity<AbstractType1_1>().ToTable("AbstractType1_1");

            Assert.Throws<InvalidOperationException>(() => BuildMapping(modelBuilder));
        }

        [Fact]
        public void Build_model_for_entity_splitting()
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
                                    m.ToTable("Vendor");
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
                                    m.ToTable("VendorDetails");
                                });

            var databaseMapping = BuildMapping(modelBuilder);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());
        }

        [Fact]
        public void Build_model_for_entity_splitting_excluding_key()
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
                                    m.ToTable("Vendor");
                                })
                        .Map(
                            m =>
                                {
                                    m.Properties(
                                        v2 => new
                                            {
                                                v2.ModifiedDate,
                                                v2.PurchasingWebServiceURL
                                            });
                                    m.ToTable("VendorDetails");
                                });

            var databaseMapping = BuildMapping(modelBuilder);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());
        }

        [Fact]
        public void Build_model_for_entity_splitting_with_complex_properties()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<ProductDescription>();
            modelBuilder.ComplexType<RowDetails>();

            modelBuilder.Entity<ProductDescription>()
                        .Map(
                            m =>
                                {
                                    m.Properties(
                                        pd1 => new
                                            {
                                                pd1.ProductDescriptionID,
                                                pd1.RowDetails.rowguid
                                            });
                                    m.ToTable("ProductDescription");
                                })
                        .Map(
                            m =>
                                {
                                    m.Properties(
                                        pd2 => new
                                            {
                                                pd2.ProductDescriptionID,
                                                pd2.Description,
                                                pd2.RowDetails.ModifiedDate
                                            });
                                    m.ToTable("ProductDescriptionExtended");
                                });

            var databaseMapping = BuildMapping(modelBuilder);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());
        }

        [Fact]
        public void Base_type_discovered_by_reachability_is_mapped()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<ITBar>();
            modelBuilder.Entity<ITBaz>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal(2, databaseMapping.Model.Containers.Single().EntitySets.Count);
            Assert.Equal(
                "ITFoo",
                databaseMapping.Model.Containers.Single().EntitySets.Single(es => es.Name == "ITFoos").
                                ElementType.Name);
            Assert.Equal(3, databaseMapping.Model.EntityTypes.Count());

            //Base type with 1 prop
            Assert.Equal(
                1,
                databaseMapping.Model.EntityTypes.Single(et => et.Name == "ITFoo").
                                DeclaredProperties.Count);
            Assert.Equal(
                1,
                databaseMapping.Model.EntityTypes.Single(et => et.Name == "ITFoo").
                                Properties.Count());

            //Derived type with 1 prop, 0 declared
            Assert.Equal(
                0,
                databaseMapping.Model.EntityTypes.Single(et => et.Name == "ITBar").
                                DeclaredProperties.Count);
            Assert.Equal(
                1,
                databaseMapping.Model.EntityTypes.Single(et => et.Name == "ITBar").
                                Properties.Count());
        }

        public abstract class ITFoo
        {
            public int Id { get; set; }
        }

        public class ITBar : ITFoo
        {
        }

        public class ITBaz
        {
            public int Id { get; set; }
            public ICollection<ITFoo> ITFoos { get; set; }
        }

        [Fact]
        public void Abstract_type_at_base_of_TPH_gets_IsTypeOf_mapping()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<A1>().Map<A2>(m => { m.Requires("disc").HasValue("A2"); }).Map<A3>(
                m => { m.Requires("disc").HasValue("A3"); }).Map<A4>(m => { m.Requires("disc").HasValue("A4"); }).
                         ToTable("A1");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());
            Assert.Equal(4, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.ElementAt(0).EntityTypeMappings.Count());
            Assert.True(
                databaseMapping.EntityContainerMappings.Single().EntitySetMappings.ElementAt(0).EntityTypeMappings.Single(
                    x => x.EntityType.Name == "A1").IsHierarchyMapping);
            Assert.False(
                databaseMapping.EntityContainerMappings.Single().EntitySetMappings.ElementAt(0).EntityTypeMappings.Single(
                    x => x.EntityType.Name == "A2").IsHierarchyMapping);
            Assert.False(
                databaseMapping.EntityContainerMappings.Single().EntitySetMappings.ElementAt(0).EntityTypeMappings.Single(
                    x => x.EntityType.Name == "A3").IsHierarchyMapping);
            Assert.False(
                databaseMapping.EntityContainerMappings.Single().EntitySetMappings.ElementAt(0).EntityTypeMappings.Single(
                    x => x.EntityType.Name == "A4").IsHierarchyMapping);
        }

        public abstract class A1
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public int Age1 { get; set; }
            public string Name1 { get; set; }
        }

        public class A2 : A1
        {
            public int Age2 { get; set; }
            public string Name2 { get; set; }
        }

        public class A3 : A2
        {
            public int Age3 { get; set; }
            public string Name3 { get; set; }
        }

        public class A4 : A1
        {
            public int Age4 { get; set; }
            public string Name4 { get; set; }
        }

        //[Fact]
        // Still fails, investigating issue
        public void Abstract_type_in_middle_of_TPH_gets_IsTypeOf_mapping()
        {
            var modelBuilder = new DbModelBuilder();
            //modelBuilder.Entity<B1>().Map<B2>(m =>
            //{
            //    m.Requires("disc").HasValue("B2");
            //}).Map<B3>(m =>
            //{
            //    m.Requires("disc").HasValue("B3");
            //}).ToTable("B1");
            modelBuilder.Entity<B1>();
            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());
            Assert.Equal(3, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.ElementAt(0).EntityTypeMappings.Count());
            Assert.False(
                databaseMapping.EntityContainerMappings.Single().EntitySetMappings.ElementAt(0).EntityTypeMappings.Single(
                    x => x.EntityType.Name == "B1").IsHierarchyMapping);
            Assert.True(
                databaseMapping.EntityContainerMappings.Single().EntitySetMappings.ElementAt(0).EntityTypeMappings.Single(
                    x => x.EntityType.Name == "B2").IsHierarchyMapping);
            Assert.False(
                databaseMapping.EntityContainerMappings.Single().EntitySetMappings.ElementAt(0).EntityTypeMappings.Single(
                    x => x.EntityType.Name == "B3").IsHierarchyMapping);
        }

        public class B1
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public int Age1 { get; set; }
            public string Name1 { get; set; }
        }

        public abstract class B2 : B1
        {
            public int Age2 { get; set; }
            public string Name2 { get; set; }
        }

        public class B3 : B2
        {
            public int Age3 { get; set; }
            public string Name3 { get; set; }
        }

        [Fact]
        public void Mapping_IA_FK_to_derived_type_puts_FK_in_correct_TPT_table()
        {
            var modelBuilder = new DbModelBuilder();

            // Map to TPT
            modelBuilder.Entity<ITOffice>();
            modelBuilder.Entity<ITEmployee>().ToTable("Employees");
            modelBuilder.Entity<ITOffSiteEmployee>().ToTable("OffSiteEmployees");
            modelBuilder.Entity<ITOnSiteEmployee>().ToTable("OnSiteEmployees");
            modelBuilder.Entity<ITOnSiteEmployee>()
                        .HasRequired(e => e.ITOffice);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            databaseMapping.Assert<ITEmployee>().DbEqual("Employees", t => t.Table);
            databaseMapping.Assert<ITOffSiteEmployee>().DbEqual("OffSiteEmployees", t => t.Table);
            databaseMapping.Assert<ITOnSiteEmployee>().DbEqual("OnSiteEmployees", t => t.Table);

            // IA FK was properly moved
            databaseMapping.Assert<ITEmployee>().HasNoForeignKeyColumn("ITOffice_ITOfficeId");
            databaseMapping.Assert<ITOnSiteEmployee>().HasForeignKeyColumn("ITOffice_ITOfficeId");

            // AssociationSet mapping updated properly
            Assert.Equal(
                "OnSiteEmployees",
                databaseMapping.Database.GetEntitySet(
                    databaseMapping.EntityContainerMappings.Single().AssociationSetMappings.ElementAt(0).Table).Table);

            Assert.Equal(
                "ITOffice_ITOfficeId",
                databaseMapping.EntityContainerMappings.Single().AssociationSetMappings.ElementAt(0).SourceEndMapping
                               .PropertyMappings.ElementAt(0).Column.Name);
        }

        [Fact]
        public void Mapping_FK_to_derived_type_puts_FK_in_correct_TPT_table()
        {
            var modelBuilder = new DbModelBuilder();

            // Map to TPT
            modelBuilder.Entity<IT_Office>();
            modelBuilder.Entity<IT_Employee>().ToTable("Employees");
            modelBuilder.Entity<IT_OffSiteEmployee>().ToTable("OffSiteEmployees");
            modelBuilder.Entity<IT_OnSiteEmployee>().ToTable("OnSiteEmployees");
            modelBuilder.Entity<IT_OnSiteEmployee>()
                        .HasRequired(e => e.IT_Office);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            databaseMapping.Assert<IT_Employee>().DbEqual("Employees", t => t.Table);
            databaseMapping.Assert<IT_OffSiteEmployee>().DbEqual("OffSiteEmployees", t => t.Table);
            databaseMapping.Assert<IT_OnSiteEmployee>().DbEqual("OnSiteEmployees", t => t.Table);

            databaseMapping.Assert<IT_Employee>().HasNoForeignKeyColumn("IT_OfficeId");
            databaseMapping.Assert<IT_OnSiteEmployee>().HasForeignKeyColumn("IT_OfficeId");
            Assert.Equal(0, databaseMapping.EntityContainerMappings.Single().AssociationSetMappings.Count());
        }

        public class ITOffice
        {
            public int ITOfficeId { get; set; }
            public string Name { get; set; }
        }

        public class ITEmployee
        {
            public int ITEmployeeId { get; set; }
            public string Name { get; set; }
        }

        public class ITOnSiteEmployee : ITEmployee
        {
            public ITOffice ITOffice { get; set; }
        }

        public class ITOffSiteEmployee : ITEmployee
        {
            public string SiteName { get; set; }
        }

        public class IT_Office
        {
            public int IT_OfficeId { get; set; }
            public string Name { get; set; }
        }

        public class IT_Employee
        {
            public int IT_EmployeeId { get; set; }
            public string Name { get; set; }
        }

        public class IT_OnSiteEmployee : IT_Employee
        {
            public int IT_OfficeId { get; set; }
            public IT_Office IT_Office { get; set; }
        }

        public class IT_OffSiteEmployee : IT_Employee
        {
            public string SiteName { get; set; }
        }

        public class IT_Context : DbContext
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<IT_Office>();
                modelBuilder.Entity<IT_Employee>().ToTable("Employees");
                modelBuilder.Entity<IT_OffSiteEmployee>().ToTable("OffSiteEmployees");
                modelBuilder.Entity<IT_OnSiteEmployee>().ToTable("OnSiteEmployees");
                modelBuilder.Entity<IT_OnSiteEmployee>()
                            .HasRequired(e => e.IT_Office);
            }

            public DbSet<IT_Office> Offices { get; set; }
            public DbSet<IT_Employee> Employees { get; set; }
        }

        [Fact]
        public void Mapping_association_to_subtype_by_convention_and_TPH_uses_correct_entity_sets()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<C1>();
            modelBuilder.Entity<D1>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal("C1", databaseMapping.Model.Containers.Single().AssociationSets[0].SourceSet.Name);
            Assert.Equal("D1", databaseMapping.Model.Containers.Single().AssociationSets[0].TargetSet.Name);
        }

        [Fact]
        public void Mapping_association_to_subtype_by_configuration_and_TPH_uses_correct_entity_sets()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<C1>().HasRequired(g => g.DiscontinueD1).WithOptional();
            modelBuilder.Entity<D1>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal("C1", databaseMapping.Model.Containers.Single().AssociationSets[0].SourceSet.Name);
            Assert.Equal("D1", databaseMapping.Model.Containers.Single().AssociationSets[0].TargetSet.Name);
        }

        public abstract class D1
        {
            public int D1Id { get; set; }
        }

        public class DiscontinueD1 : D1
        {
            public DateTime DiscontinuedOn { get; set; }
        }

        public class C1
        {
            public int Id { get; set; }
            public int DiscontinueD1Id { get; set; }
            public DiscontinueD1 DiscontinueD1 { get; set; }
        }

        [Fact]
        public void TPT_model_can_map_PK_property_to_different_columns_in_different_tables()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Product>();
            modelBuilder.Entity<ColoredProduct>().ToTable("ColoredProducts");
            modelBuilder.Entity<StyledProduct>().ToTable("StyledProducts");

            SetDerivedEntityColumnNames(modelBuilder);

            ValidateTPTOrTPCWithRenamedColumns(modelBuilder);
        }

        [Fact]
        public void TPT_model_can_map_PK_property_to_different_columns_in_different_tables_if_derived_types_configured_first()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<ColoredProduct>().ToTable("ColoredProducts");
            modelBuilder.Entity<StyledProduct>().ToTable("StyledProducts");
            modelBuilder.Entity<Product>();

            SetDerivedEntityColumnNames(modelBuilder);

            ValidateTPTOrTPCWithRenamedColumns(modelBuilder);
        }

        [Fact]
        public void TPT_model_using_Map_can_map_PK_property_to_different_columns_in_different_tables()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Product>();
            modelBuilder.Entity<ColoredProduct>().Map(m => m.ToTable("ColoredProducts"));
            modelBuilder.Entity<StyledProduct>().Map(m => m.ToTable("StyledProducts"));

            SetDerivedEntityColumnNames(modelBuilder);

            ValidateTPTOrTPCWithRenamedColumns(modelBuilder);
        }

        [Fact]
        public void TPT_model_with_HasColumnName_done_before_ToTable_can_map_PK_property_to_different_columns_in_different_tables()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            SetDerivedEntityColumnNames(modelBuilder);

            modelBuilder.Entity<Product>();
            modelBuilder.Entity<ColoredProduct>().ToTable("ColoredProducts");
            modelBuilder.Entity<StyledProduct>().ToTable("StyledProducts");

            ValidateTPTOrTPCWithRenamedColumns(modelBuilder);
        }

        [Fact]
        public void TPC_model_can_map_PK_property_to_different_columns_in_different_tables()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Product>().Map(
                m =>
                    {
                        m.MapInheritedProperties();
                        m.ToTable("Products");
                    });
            modelBuilder.Entity<ColoredProduct>().Map(
                m =>
                    {
                        m.MapInheritedProperties();
                        m.ToTable("ColoredProducts");
                    });
            modelBuilder.Entity<StyledProduct>().Map(
                m =>
                    {
                        m.MapInheritedProperties();
                        m.ToTable("StyledProducts");
                    });

            SetDerivedEntityColumnNames(modelBuilder);

            ValidateTPTOrTPCWithRenamedColumns(modelBuilder);
        }

        [Fact]
        public void TPC_model_with_HasColumnName_done_before_ToTable_can_map_PK_property_to_different_columns_in_different_tables()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            SetDerivedEntityColumnNames(modelBuilder);

            modelBuilder.Entity<Product>().Map(
                m =>
                    {
                        m.MapInheritedProperties();
                        m.ToTable("Products");
                    });
            modelBuilder.Entity<ColoredProduct>().Map(
                m =>
                    {
                        m.MapInheritedProperties();
                        m.ToTable("ColoredProducts");
                    });
            modelBuilder.Entity<StyledProduct>().Map(
                m =>
                    {
                        m.MapInheritedProperties();
                        m.ToTable("StyledProducts");
                    });

            ValidateTPTOrTPCWithRenamedColumns(modelBuilder);
        }

        private void SetDerivedEntityColumnNames(AdventureWorksModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().Property(p => p.ProductID).HasColumnName("base_product_id");
            modelBuilder.Entity<ColoredProduct>().Property(p => p.ProductID).HasColumnName("colored_product_id");
            modelBuilder.Entity<StyledProduct>().Property(p => p.ProductID).HasColumnName("styled_product_id");
        }

        private void ValidateTPTOrTPCWithRenamedColumns(AdventureWorksModelBuilder modelBuilder)
        {
            var databaseMapping = BuildMapping(modelBuilder);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());

            databaseMapping.Assert<Product>(p => p.ProductID).DbEqual("base_product_id", c => c.Name);
            databaseMapping.Assert<ColoredProduct>(p => p.ProductID).DbEqual("colored_product_id", c => c.Name);
            databaseMapping.Assert<StyledProduct>(p => p.ProductID).DbEqual("styled_product_id", c => c.Name);
        }

        [Fact]
        public void TPT_model_using_Table_attributes_can_map_PK_property_to_different_columns_in_different_tables()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<TPTHorse>()
                .Property(e => e.Id)
                .HasColumnAnnotation("WhatDoesItSay", "Mor...or...or...or...orse")
                .HasColumnAnnotation("Common", "Just a pony.")
                .HasColumnName("horse_id");
            
            modelBuilder.Entity<TPTUnicorn>()
                .Property(e => e.Id)
                .HasColumnName("unicorn_id")
                .HasColumnAnnotation("WhatDoesItSay", "Hor...or...or...or...orn")
                .HasColumnAnnotation("Common", "Just a pony.");
            
            modelBuilder.Entity<TPTHornedPegasus>()
                .Property(e => e.Id)
                .HasColumnName("pegasus_id")
                .HasColumnAnnotation("WhatDoesItSay", "Prin...in...in...in...cess");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());

            databaseMapping.Assert<TPTHorse>(p => p.Id).DbEqual("horse_id", c => c.Name);
            databaseMapping.Assert<TPTUnicorn>(p => p.Id).DbEqual("unicorn_id", c => c.Name);
            databaseMapping.Assert<TPTHornedPegasus>(p => p.Id).DbEqual("pegasus_id", c => c.Name);

            databaseMapping.Assert<TPTHorse>("Horses")
                .Column("horse_id")
                .HasAnnotation("WhatDoesItSay", "Mor...or...or...or...orse")
                .HasAnnotation("Common", "Just a pony.");

            databaseMapping.Assert<TPTUnicorn>("Unicorns")
                .Column("unicorn_id")
                .HasAnnotation("WhatDoesItSay", "Hor...or...or...or...orn")
                .HasAnnotation("Common", "Just a pony.");

            databaseMapping.Assert<TPTHornedPegasus>("HornedPegasuses")
                .Column("pegasus_id")
                .HasAnnotation("WhatDoesItSay", "Prin...in...in...in...cess")
                .HasAnnotation("Common", "Just a pony.");
        }

        [Fact] // CodePlex 583
        public void Subclasses_can_map_different_properties_to_same_column_using_TPH()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Person>();
            modelBuilder.Entity<Student>().Property(p => p.Career).HasColumnName("Data");
            modelBuilder.Entity<Officer>().Property(p => p.Department).HasColumnName("Data");
            modelBuilder.Entity<Teacher>();
            modelBuilder.Entity<Lawyer>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<Person>("People").HasColumn("Data");
            databaseMapping.Assert<Student>("People").HasColumn("Data");
            databaseMapping.Assert<Officer>("People").HasColumn("Data");
            databaseMapping.Assert<Teacher>("People").HasColumn("Data");
            databaseMapping.Assert<Lawyer>("People").HasColumn("Data");

            databaseMapping.AssertValid();
        }

        [Fact] // CodePlex 583
        public void Subclasses_can_map_different_parts_of_complex_properties_to_same_column_using_TPH()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Lab>();
            modelBuilder.Entity<MobileLab>().Property(p => p.Vehicle.Registration).HasColumnName("LabId");
            modelBuilder.Entity<StaticLab>().Property(p => p.LabNumber).HasColumnName("LabId");
            modelBuilder.Entity<MobileLab>().Property(p => p.Vehicle.Info.Depth).HasColumnName("InfoDepth");
            modelBuilder.Entity<StaticLab>().Property(p => p.LabInfo.Depth).HasColumnName("InfoDepth");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<Lab>("Labs").HasColumn("LabId");
            databaseMapping.Assert<Lab>("Labs").HasColumn("InfoDepth");
            databaseMapping.Assert<MobileLab>("Labs").HasColumn("LabId");
            databaseMapping.Assert<MobileLab>("Labs").HasColumn("InfoDepth");
            databaseMapping.Assert<StaticLab>("Labs").HasColumn("LabId");
            databaseMapping.Assert<StaticLab>("Labs").HasColumn("InfoDepth");

            databaseMapping.AssertValid();
        }

        [Fact] // CodePlex 583
        public void Subclasses_that_map_properties_to_same_column_with_different_facets_using_TPH_will_throw()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Person>();
            modelBuilder.Entity<Student>().Property(p => p.Career).HasMaxLength(256).HasColumnName("ColumnName");
            modelBuilder.Entity<Officer>().Property(p => p.Department).HasMaxLength(512).HasColumnName("ColumnName");

            var details = Environment.NewLine + "\t" +
                          string.Format(
                              LookupString(
                                  EntityFrameworkAssembly, "System.Data.Entity.Properties.Resources", "ConflictingConfigurationValue"),
                              "MaxLength", 512, "MaxLength", 256);

            Assert.Throws<MappingException>(() => BuildMapping(modelBuilder))
                  .ValidateMessage("BadTphMappingToSharedColumn", "Department", "Officer", "Career", "Student", "ColumnName", "Person", details);
        }

        [Fact] // CodePlex 583
        public void Subclasses_that_map_properties_to_same_column_with_different_annotations_using_TPH_will_throw()
        {
            var modelBuilder = new DbModelBuilder();
            
            modelBuilder.Entity<Person>();
            
            modelBuilder.Entity<Student>()
                .Property(p => p.Career)
                .HasColumnName("ColumnName")
                .HasColumnAnnotation("Annotation1", "Value1")
                .HasColumnAnnotation("Annotation2", "Value2");

            modelBuilder.Entity<Officer>()
                .Property(p => p.Department)
                .HasColumnName("ColumnName")
                .HasColumnAnnotation("Annotation1", "Value1")
                .HasColumnAnnotation("Annotation2", "Different Value");

            var details = Environment.NewLine + "\t" +
                          string.Format(
                              LookupString(
                                  EntityFrameworkAssembly, "System.Data.Entity.Properties.Resources", "ConflictingAnnotationValue"),
                              "Annotation2", "Different Value", "Value2");

            Assert.Throws<MappingException>(() => BuildMapping(modelBuilder))
                  .ValidateMessage("BadTphMappingToSharedColumn", "Department", "Officer", "Career", "Student", "ColumnName", "Person", details);
        }

        [Fact] // CodePlex 583
        public void Column_configuration_can_be_applied_to_only_one_property_when_properties_share_TPH_column()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Person>();
            modelBuilder.Entity<Student>().Property(p => p.Career).HasColumnName("Data");

            modelBuilder.Entity<Officer>()
                .Property(p => p.Department)
                .HasColumnName("Data")
                .HasMaxLength(256)
                .HasColumnType("varchar")
                .HasColumnAnnotation("Annotation1", "Value1")
                .HasColumnAnnotation("Annotation2", "Value2");

            modelBuilder.Entity<Teacher>().Property(p => p.Department).HasColumnName("Data");
            modelBuilder.Entity<Lawyer>().Property(p => p.Specialty).HasColumnName("Data");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<Student>(t => t.Career)
                .DbEqual(256, f => f.MaxLength)
                .DbEqual("varchar", f => f.TypeName);
            databaseMapping.Assert<Officer>(t => t.Department)
                .DbEqual(256, f => f.MaxLength)
                .DbEqual("varchar", f => f.TypeName);
            databaseMapping.Assert<Teacher>(t => t.Department)
                .DbEqual(256, f => f.MaxLength)
                .DbEqual("varchar", f => f.TypeName);
            databaseMapping.Assert<Lawyer>(t => t.Specialty)
                .DbEqual(256, f => f.MaxLength)
                .DbEqual("varchar", f => f.TypeName);

            databaseMapping.AssertValid();

            databaseMapping.Assert("People")
                .Column("Data")
                .HasAnnotation("Annotation1", "Value1")
                .HasAnnotation("Annotation2", "Value2");
        }

        [Fact] // CodePlex 583
        public void Non_conflicting_column_configuration_can_be_spread_across_properties_that_share_TPH_column()
        {
            var modelBuilder = new DbModelBuilder();
            
            modelBuilder.Entity<Person>();
            
            modelBuilder.Entity<Student>()
                .Property(p => p.Career)
                .HasColumnName("Data")
                .HasMaxLength(256)
                .HasColumnAnnotation("Annotation1", "Value1")
                .HasColumnAnnotation("Annotation2", "Value2");
            
            modelBuilder.Entity<Officer>()
                .Property(p => p.Department)
                .HasColumnName("Data")
                .HasColumnType("varchar");
            
            modelBuilder.Entity<Teacher>()
                .Property(p => p.Department)
                .HasColumnName("Data")
                .HasColumnType("varchar")
                .HasColumnAnnotation("Annotation1", "Value1")
                .HasColumnAnnotation("Annotation2", "Value2");
            
            modelBuilder.Entity<Lawyer>()
                .Property(p => p.Specialty)
                .HasColumnName("Data")
                .HasMaxLength(256);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<Student>(t => t.Career)
                           .DbEqual(256, f => f.MaxLength)
                           .DbEqual("varchar", f => f.TypeName);
            databaseMapping.Assert<Officer>(t => t.Department)
                           .DbEqual(256, f => f.MaxLength)
                           .DbEqual("varchar", f => f.TypeName);
            databaseMapping.Assert<Teacher>(t => t.Department)
                           .DbEqual(256, f => f.MaxLength)
                           .DbEqual("varchar", f => f.TypeName);
            databaseMapping.Assert<Lawyer>(t => t.Specialty)
                           .DbEqual(256, f => f.MaxLength)
                           .DbEqual("varchar", f => f.TypeName);

            databaseMapping.AssertValid();

            databaseMapping.Assert("People")
                .Column("Data")
                .HasAnnotation("Annotation1", "Value1")
                .HasAnnotation("Annotation2", "Value2");
        }

        public class TphPersonContext : DbContext
        {
            static TphPersonContext()
            {
                Database.SetInitializer(new TphPersonInitializer());
            }

            public DbSet<Person> People { get; set; }
            public DbSet<Lab> Labs { get; set; }
            public DbSet<CoverBusiness> Covers { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Student>().Property(p => p.Career).HasColumnName("Data").HasMaxLength(200);
                modelBuilder.Entity<Officer>().Property(p => p.Department).HasColumnName("Data");

                modelBuilder.Entity<MobileLab>().Property(p => p.Vehicle.Registration).HasColumnName("LabId");
                modelBuilder.Entity<StaticLab>().Property(p => p.LabNumber).HasColumnName("LabId");

                modelBuilder.Entity<MobileLab>().Property(p => p.Vehicle.Info.Depth).HasColumnName("InfoDepth");
                modelBuilder.Entity<StaticLab>().Property(p => p.LabInfo.Depth).HasColumnName("InfoDepth");
            }
        }

        public class TphPersonInitializer : DropCreateDatabaseIfModelChanges<TphPersonContext>
        {
            protected override void Seed(TphPersonContext context)
            {
                context.People.Add(
                    new Student
                        {
                            Name = "Jesse",
                            Career = "N/A"
                        });

                context.People.Add(
                    new Teacher
                        {
                            Name = "Walter",
                            Department = "Chemistry"
                        });

                context.People.Add(
                    new Lawyer
                        {
                            Name = "Saul",
                            Specialty = "Laundering"
                        });

                context.People.Add(
                    new Officer
                        {
                            Name = "Hank",
                            Department = "DEA"
                        });

                context.People.Add(
                    new Person
                        {
                            Name = "Skyler"
                        });

                context.Labs.Add(
                    new MobileLab
                        {
                            Vehicle = new Vehicle
                                {
                                    Registration = 1,
                                    Info = new LabInfo
                                        {
                                            Depth = 2,
                                            Size = 3
                                        }
                                }
                        });

                context.Labs.Add(
                    new StaticLab
                        {
                            LabNumber = 4,
                            LabInfo = new LabInfo
                                {
                                    Depth = 5,
                                    Size = 6
                                }
                        });

                context.Covers.Add(
                    new CarWash
                        {
                            Name = "Skyler's Car Wash"
                        });

                context.Covers.Add(
                    new FastFoodChain
                    {
                        Name = "Chickin' Lickin'"
                    });

                context.Covers.Add(
                    new LosPollosHermanos
                    {
                        Name = "Chicken Bros"
                    });
            }
        }

        public class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Student : Person
        {
            public string Career { get; set; }
        }

        public class Teacher : Person
        {
            [Column("Data")]
            public string Department { get; set; }
        }

        public class Lawyer : Person
        {
            [Column("Data")]
            public string Specialty { get; set; }
        }

        public class Officer : Person
        {
            public string Department { get; set; }
        }

        public class Lab
        {
            public int Id { get; set; }
        }

        public class MobileLab : Lab
        {
            public Vehicle Vehicle { get; set; }
        }

        public class StaticLab : Lab
        {
            public int LabNumber { get; set; }
            public LabInfo LabInfo { get; set; }
        }

        [ComplexType]
        public class Vehicle
        {
            public int Registration { get; set; }
            public LabInfo Info { get; set; }
        }

        [ComplexType]
        public class LabInfo
        {
            public int Size { get; set; }
            public int Depth { get; set; }
        }

        public abstract class CoverBusiness
        {
            public int Id { get; set; }

            // CodePlex 1063
            // These are needed to push the property count over the limit where
            // the MetadataWorkspace starts treating collections differently.
            public int Something00 { get; set; }
            public int Something01 { get; set; }
            public int Something02 { get; set; }
            public int Something03 { get; set; }
            public int Something04 { get; set; }
            public int Something05 { get; set; }
            public int Something06 { get; set; }
            public int Something07 { get; set; }
            public int Something08 { get; set; }
            public int Something09 { get; set; }
            public int Something10 { get; set; }
            public int Something11 { get; set; }
            public int Something12 { get; set; }
            public int Something13 { get; set; }
            public int Something14 { get; set; }
            public int Something15 { get; set; }
            public int Something16 { get; set; }
            public int Something17 { get; set; }
            public int Something18 { get; set; }
            public int Something19 { get; set; }
            public int Something20 { get; set; }
        }

        public class CarWash : CoverBusiness
        {
            [Column("Name")]
            public string Name { get; set; }
        }

        public class FastFoodChain : CoverBusiness
        {
            [Column("Name")]
            public string Name { get; set; }
        }

        public class HotDogStand : CoverBusiness
        {
            [Column("Name")]
            public string Name { get; set; }
        }

        public class InABun : CoverBusiness
        {
            [Column("Name")]
            public string Name { get; set; }
        }

        public class LosPollosHermanos : FastFoodChain
        {
        }

        [Fact] // CodePlex 1924
        public void Non_shared_columns_with_non_abstract_base_class_are_made_nullable_even_if_required()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Base1924>();
            modelBuilder.Entity<Derived1924A>().Property(e => e.SomeString).IsRequired().HasColumnName("SomeStringA");
            modelBuilder.Entity<Derived1924B>().Property(e => e.SomeString).IsRequired().HasColumnName("SomeStringB");
            modelBuilder.Entity<Derived1924A>().Property(e => e.SomeInt).IsRequired().HasColumnName("SomeIntA");
            modelBuilder.Entity<Derived1924B>().Property(e => e.SomeInt).IsRequired().HasColumnName("SomeIntB");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<Derived1924A>("Base1924").Column("SomeStringA").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<Derived1924B>("Base1924").Column("SomeStringB").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<Derived1924A>("Base1924").Column("SomeIntA").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<Derived1924B>("Base1924").Column("SomeIntB").DbEqual(true, p => p.Nullable);

            databaseMapping.AssertValid();
        }

        [Fact] // CodePlex 1924
        public void Shared_columns_with_non_abstract_base_class_are_made_nullable_even_if_required()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Base1924>();
            modelBuilder.Entity<Derived1924A>().Property(e => e.SomeString).IsRequired().HasColumnName("NullMeString");
            modelBuilder.Entity<Derived1924B>().Property(e => e.SomeString).IsRequired().HasColumnName("NullMeString");
            modelBuilder.Entity<Derived1924A>().Property(e => e.SomeInt).IsRequired().HasColumnName("NullMeInt");
            modelBuilder.Entity<Derived1924B>().Property(e => e.SomeInt).IsRequired().HasColumnName("NullMeInt");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<Derived1924A>("Base1924").Column("NullMeString").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<Derived1924B>("Base1924").Column("NullMeString").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<Derived1924A>("Base1924").Column("NullMeInt").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<Derived1924B>("Base1924").Column("NullMeInt").DbEqual(true, p => p.Nullable);

            databaseMapping.AssertValid();
        }

        public class Base1924
        {
            public int Id { get; set; }
        }

        public class Derived1924A : Base1924
        {
            public string SomeString { get; set; }
            public string SomeInt { get; set; }
        }

        public class Derived1924B : Base1924
        {
            public string SomeString { get; set; }
            public string SomeInt { get; set; }
        }

        [Fact] // CodePlex 1924
        public void Non_shared_columns_with_abstract_base_class_are_made_nullable_even_if_required()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<AbstractBase1924>();
            modelBuilder.Entity<DerivedFromAbstract1924A>().Property(e => e.SomeString).IsRequired().HasColumnName("SomeStringA");
            modelBuilder.Entity<DerivedFromAbstract1924B>().Property(e => e.SomeString).IsRequired().HasColumnName("SomeStringB");
            modelBuilder.Entity<DerivedFromAbstract1924A>().Property(e => e.SomeInt).IsRequired().HasColumnName("SomeIntA");
            modelBuilder.Entity<DerivedFromAbstract1924B>().Property(e => e.SomeInt).IsRequired().HasColumnName("SomeIntB");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<DerivedFromAbstract1924A>("AbstractBase1924").Column("SomeStringA").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<DerivedFromAbstract1924B>("AbstractBase1924").Column("SomeStringB").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<DerivedFromAbstract1924A>("AbstractBase1924").Column("SomeIntA").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<DerivedFromAbstract1924B>("AbstractBase1924").Column("SomeIntB").DbEqual(true, p => p.Nullable);

            databaseMapping.AssertValid();
        }

        [Fact] // CodePlex 1924
        public void Shared_columns_with_abstract_base_class_are_made_nullable_even_if_required()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<AbstractBase1924>();
            modelBuilder.Entity<DerivedFromAbstract1924A>().Property(e => e.SomeString).IsRequired().HasColumnName("NullMeString");
            modelBuilder.Entity<DerivedFromAbstract1924B>().Property(e => e.SomeString).IsRequired().HasColumnName("NullMeString");
            modelBuilder.Entity<DerivedFromAbstract1924A>().Property(e => e.SomeInt).IsRequired().HasColumnName("NullMeInt");
            modelBuilder.Entity<DerivedFromAbstract1924B>().Property(e => e.SomeInt).IsRequired().HasColumnName("NullMeInt");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<DerivedFromAbstract1924A>("AbstractBase1924").Column("NullMeString").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<DerivedFromAbstract1924B>("AbstractBase1924").Column("NullMeString").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<DerivedFromAbstract1924A>("AbstractBase1924").Column("NullMeInt").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<DerivedFromAbstract1924B>("AbstractBase1924").Column("NullMeInt").DbEqual(true, p => p.Nullable);

            databaseMapping.AssertValid();
        }

        public abstract class AbstractBase1924
        {
            public int Id { get; set; }
        }

        public class DerivedFromAbstract1924A : AbstractBase1924
        {
            public string SomeString { get; set; }
            public string SomeInt { get; set; }
        }

        public class DerivedFromAbstract1924B : AbstractBase1924
        {
            public string SomeString { get; set; }
            public string SomeInt { get; set; }
        }

        [Fact] // CodePlex 2254
        public void Non_nullable_columns_in_intermediate_class_with_pure_TPH_mapping_are_made_nullable()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Base2254>().ToTable("Concretes");
            modelBuilder.Entity<Intermediate2254>().ToTable("Concretes");
            modelBuilder.Entity<Concrete2254A>().ToTable("Concretes");
            modelBuilder.Entity<Concrete2254B>().ToTable("Concretes");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<Base2254>("Concretes").Column("Id").DbEqual(false, p => p.Nullable);
            databaseMapping.Assert<Base2254>("Concretes").Column("BaseString").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<Base2254>("Concretes").Column("BaseInteger").DbEqual(false, p => p.Nullable);
            databaseMapping.Assert<Base2254>("Concretes").Column("IntermediateString").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<Base2254>("Concretes").Column("IntermediateInteger").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<Concrete2254A>("Concretes").Column("Concrete1String").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<Concrete2254A>("Concretes").Column("Concrete1Integer").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<Concrete2254B>("Concretes").Column("Concrete2String").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<Concrete2254B>("Concretes").Column("Concrete2Integer").DbEqual(true, p => p.Nullable);

            databaseMapping.AssertValid();
        }

        [Fact] // CodePlex 2254
        public void Non_nullable_columns_in_intermediate_class_with_hybrid_TPT_TPH_mapping_are_made_nullable()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Base2254>();
            modelBuilder.Entity<Intermediate2254>().ToTable("Concretes");
            modelBuilder.Entity<Concrete2254A>().ToTable("Concretes");
            modelBuilder.Entity<Concrete2254B>().ToTable("Concretes");
            
            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<Base2254>("Base2254").Column("Id").DbEqual(false, p => p.Nullable);
            databaseMapping.Assert<Base2254>("Base2254").Column("BaseString").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<Base2254>("Base2254").Column("BaseInteger").DbEqual(false, p => p.Nullable);
            databaseMapping.Assert<Intermediate2254>("Concretes").Column("IntermediateString").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<Intermediate2254>("Concretes").Column("IntermediateInteger").DbEqual(false, p => p.Nullable);
            databaseMapping.Assert<Concrete2254A>("Concretes").Column("Concrete1String").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<Concrete2254A>("Concretes").Column("Concrete1Integer").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<Concrete2254B>("Concretes").Column("Concrete2String").DbEqual(true, p => p.Nullable);
            databaseMapping.Assert<Concrete2254B>("Concretes").Column("Concrete2Integer").DbEqual(true, p => p.Nullable);

            databaseMapping.AssertValid();
        }

        public abstract class Base2254
        {
            public int Id { get; set; }
            public string BaseString { get; set; }
            public int BaseInteger { get; set; }
        }

        public abstract class Intermediate2254 : Base2254
        {
            public string IntermediateString { get; set; }
            public int IntermediateInteger { get; set; }
        }

        public class Concrete2254A : Intermediate2254
        {
            public string Concrete1String { get; set; }
            public int Concrete1Integer { get; set; }
        }

        public class Concrete2254B : Intermediate2254
        {
            public string Concrete2String { get; set; }
            public int Concrete2Integer { get; set; }
        }

        [Fact] // CodePlex 1964
        public void Subclasses_can_map_different_FKs_to_same_column_using_TPH()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<User1964>();
            modelBuilder.Entity<Movie1964>();
            
            modelBuilder.Entity<Tag1964>();

            modelBuilder.Entity<UserTag1964>().Property(m => m.UserId).HasColumnName("ItemId");
            modelBuilder.Entity<UserTag1964>().HasRequired(m => m.User).WithMany().HasForeignKey(m => m.UserId);

            modelBuilder.Entity<MovieTag1964>().Property(m => m.MovieId).HasColumnName("ItemId");
            modelBuilder.Entity<MovieTag1964>().HasRequired(m => m.Movie).WithMany().HasForeignKey(m => m.MovieId);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<UserTag1964>("Tag1964").HasColumn("ItemId");
            databaseMapping.Assert<MovieTag1964>("Tag1964").HasColumn("ItemId");

            Assert.Equal(0, databaseMapping.Database.AssociationTypes.Count());

            databaseMapping.AssertValid();
        }

        [Fact] // CodePlex 1964
        public void Subclasses_can_map_different_FKs_to_same_column_using_TPH_with_explicit_condition()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<User1964>();
            modelBuilder.Entity<Movie1964>();

            modelBuilder.Entity<Tag1964>()
                .Map<UserTag1964>(m => m.Requires("TagType").HasValue("usertag"))
                .Map<MovieTag1964>(m => m.Requires("TagType").HasValue("movietag"));

            modelBuilder.Entity<UserTag1964>().Property(m => m.UserId).HasColumnName("ItemId");
            modelBuilder.Entity<UserTag1964>().HasRequired(m => m.User).WithMany().HasForeignKey(m => m.UserId);

            modelBuilder.Entity<MovieTag1964>().Property(m => m.MovieId).HasColumnName("ItemId");
            modelBuilder.Entity<MovieTag1964>().HasRequired(m => m.Movie).WithMany().HasForeignKey(m => m.MovieId);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<UserTag1964>("Tag1964").HasColumn("ItemId");
            databaseMapping.Assert<MovieTag1964>("Tag1964").HasColumn("ItemId");

            Assert.Equal(0, databaseMapping.Database.AssociationTypes.Count());

            databaseMapping.AssertValid();
        }

        [Fact] // CodePlex 1964
        public void Subclasses_can_map_different_composite_FKs_to_same_column_using_TPH()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<User1964>().HasKey(e => new { e.Id, e.MaybeId1, e.MaybeId2 });
            modelBuilder.Entity<Movie1964>().HasKey(e => new { e.Id, e.MaybeId1, e.MaybeId2 });
            
            modelBuilder.Entity<Tag1964>();

            modelBuilder.Entity<UserTag1964>().Property(m => m.UserId).HasColumnName("ItemId");
            modelBuilder.Entity<UserTag1964>().Property(m => m.UserMaybeFk1).HasColumnName("ItemFk1");
            modelBuilder.Entity<UserTag1964>().Property(m => m.UserMaybeFk2).HasColumnName("ItemFk2");
            modelBuilder.Entity<UserTag1964>()
                .HasRequired(m => m.User)
                .WithMany()
                .HasForeignKey(m => new { m.UserId, m.UserMaybeFk1, m.UserMaybeFk2 });

            modelBuilder.Entity<MovieTag1964>().Property(m => m.MovieId).HasColumnName("ItemId");
            modelBuilder.Entity<MovieTag1964>().Property(m => m.MovieMaybeFk1).HasColumnName("ItemFk1");
            modelBuilder.Entity<MovieTag1964>().Property(m => m.MovieMaybeFk2).HasColumnName("ItemFk2");
            modelBuilder.Entity<MovieTag1964>()
                .HasRequired(m => m.Movie)
                .WithMany()
                .HasForeignKey(m => new { m.MovieId, m.MovieMaybeFk1, m.MovieMaybeFk2 });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<UserTag1964>("Tag1964").HasColumn("ItemId");
            databaseMapping.Assert<UserTag1964>("Tag1964").HasColumn("ItemFk1");
            databaseMapping.Assert<UserTag1964>("Tag1964").HasColumn("ItemFk2");
            
            databaseMapping.Assert<MovieTag1964>("Tag1964").HasColumn("ItemId");
            databaseMapping.Assert<MovieTag1964>("Tag1964").HasColumn("ItemFk1");
            databaseMapping.Assert<MovieTag1964>("Tag1964").HasColumn("ItemFk2");

            Assert.Equal(0, databaseMapping.Database.AssociationTypes.Count());

            databaseMapping.AssertValid();
        }

        [Fact] // CodePlex 1964
        public void Subclasses_can_map_parts_of_different_composite_FKs_to_same_column_using_TPH()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<User1964>().HasKey(e => new { e.Id, e.MaybeId1, e.MaybeId2 });
            modelBuilder.Entity<Movie1964>().HasKey(e => new { e.Id, e.MaybeId1, e.MaybeId2 });

            modelBuilder.Entity<Tag1964>();

            modelBuilder.Entity<UserTag1964>().Property(m => m.UserId).HasColumnName("ItemId");
            modelBuilder.Entity<UserTag1964>().Property(m => m.UserMaybeFk2).HasColumnName("ItemFk2");
            modelBuilder.Entity<UserTag1964>()
                .HasRequired(m => m.User)
                .WithMany()
                .HasForeignKey(m => new { m.UserId, m.UserMaybeFk1, m.UserMaybeFk2 });

            modelBuilder.Entity<MovieTag1964>().Property(m => m.MovieId).HasColumnName("ItemId");
            modelBuilder.Entity<MovieTag1964>().Property(m => m.MovieMaybeFk1).HasColumnName("ItemFk1");
            modelBuilder.Entity<MovieTag1964>()
                .HasRequired(m => m.Movie)
                .WithMany()
                .HasForeignKey(m => new { m.MovieId, m.MovieMaybeFk1, m.MovieMaybeFk2 });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<UserTag1964>("Tag1964").HasColumn("ItemId");
            databaseMapping.Assert<UserTag1964>("Tag1964").HasColumn("UserMaybeFk1");
            databaseMapping.Assert<UserTag1964>("Tag1964").HasColumn("ItemFk2");

            databaseMapping.Assert<MovieTag1964>("Tag1964").HasColumn("ItemId");
            databaseMapping.Assert<MovieTag1964>("Tag1964").HasColumn("ItemFk1");
            databaseMapping.Assert<MovieTag1964>("Tag1964").HasColumn("MovieMaybeFk2");

            Assert.Equal(0, databaseMapping.Database.AssociationTypes.Count());

            databaseMapping.AssertValid();
        }

        public class User1964
        {
            public int Id { get; set; }
            public string MaybeId1 { get; set; }
            public Guid MaybeId2 { get; set; }

            public string SomeData { get; set; }

        }

        public class Movie1964
        {
            public int Id { get; set; }
            public string MaybeId1 { get; set; }
            public Guid MaybeId2 { get; set; }

            public string SomeData { get; set; }
        }

        public abstract class Tag1964
        {
            public int Id { get; set; }

            public string SomeData { get; set; }
        }

        public class UserTag1964 : Tag1964
        {
            public int UserId { get; set; }
            public string UserMaybeFk1 { get; set; }
            public Guid UserMaybeFk2 { get; set; }

            public virtual User1964 User { get; set; }
        }

        public class MovieTag1964 : Tag1964
        {
            public int MovieId { get; set; }
            public string MovieMaybeFk1 { get; set; }
            public Guid MovieMaybeFk2 { get; set; }

            public virtual Movie1964 Movie { get; set; }
        }

        [Fact] // CodePlex 1964
        public void Subclasses_that_map_different_FKs_to_same_column_using_TPH_can_roundtrip()
        {
            using (var context = new Context1964())
            {
                context.Database.Delete();

                context.UserTags.Add(new UserTag1964 { User = new User1964 { SomeData = "UserData" }, SomeData = "UserTagData" });
                context.MovieTags.Add(new MovieTag1964 { Movie = new Movie1964 { SomeData = "MovieData" }, SomeData = "MovieTagData" });

                context.SaveChanges();
            }

            using (var context = new Context1964())
            {
                var userTag = context.UserTags.Single();
                var movieTag = context.MovieTags.Single();

                Assert.Equal("UserTagData", userTag.SomeData);
                Assert.Equal("UserData", userTag.User.SomeData);
                Assert.Equal("MovieTagData", movieTag.SomeData);
                Assert.Equal("MovieData", movieTag.Movie.SomeData);
            }
        }

        public class Context1964 : DbContext
        {
            public DbSet<User1964> Users { get; set; }
            public DbSet<Movie1964> Movies { get; set; }
            public DbSet<UserTag1964> UserTags { get; set; }
            public DbSet<MovieTag1964> MovieTags { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Tag1964>()
                    .Map<UserTag1964>(m => m.Requires("TagType").HasValue("usertag"))
                    .Map<MovieTag1964>(m => m.Requires("TagType").HasValue("movietag"));

                modelBuilder.Entity<UserTag1964>().Property(m => m.UserId).HasColumnName("ItemId");
                modelBuilder.Entity<UserTag1964>().HasRequired(m => m.User).WithMany().HasForeignKey(m => m.UserId);

                modelBuilder.Entity<MovieTag1964>().Property(m => m.MovieId).HasColumnName("ItemId");
                modelBuilder.Entity<MovieTag1964>().HasRequired(m => m.Movie).WithMany().HasForeignKey(m => m.MovieId);
            }
        }

        [Fact] // CodePlex 546
        public void Multiple_one_to_one_associations_using_same_lifted_nav_prop_are_identified_correctly()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<DerivedClassA>()
                .HasOptional(e => e.ClassA1)
                .WithOptionalDependent();

            modelBuilder
                .Entity<DerivedClassA>()
                .HasOptional(e => e.ClassA2)
                .WithOptionalDependent();

            modelBuilder
                .Entity<DerivedClassB>()
                .HasOptional(e => e.ClassA1)
                .WithOptionalDependent();

            modelBuilder
                .Entity<DerivedClassB>()
                .HasOptional(e => e.ClassA2)
                .WithOptionalDependent();

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            var associationTypes = databaseMapping.Model.AssociationTypes.ToList();

            Assert.Equal(4, associationTypes.Count);

            var one = associationTypes.Single(a => a.Name == "DerivedClassA_ClassA1");
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, one.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, one.TargetEnd.RelationshipMultiplicity);
            Assert.Equal("DerivedClassA", one.SourceEnd.GetEntityType().Name);
            Assert.Equal("DerivedClassA", one.TargetEnd.GetEntityType().Name);

            var two = associationTypes.Single(a => a.Name == "DerivedClassA_ClassA2");
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, two.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, two.TargetEnd.RelationshipMultiplicity);
            Assert.Equal("DerivedClassA", two.SourceEnd.GetEntityType().Name);
            Assert.Equal("DerivedClassA", two.TargetEnd.GetEntityType().Name);

            var three = associationTypes.Single(a => a.Name == "DerivedClassB_ClassA1");
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, three.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, three.TargetEnd.RelationshipMultiplicity);
            Assert.Equal("DerivedClassA", three.SourceEnd.GetEntityType().Name);
            Assert.Equal("DerivedClassB", three.TargetEnd.GetEntityType().Name);

            var four = associationTypes.Single(a => a.Name == "DerivedClassB_ClassA2");
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, four.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, four.TargetEnd.RelationshipMultiplicity);
            Assert.Equal("DerivedClassA", four.SourceEnd.GetEntityType().Name);
            Assert.Equal("DerivedClassB", four.TargetEnd.GetEntityType().Name);
        }

        public abstract class TheBaseClass
        {
            public int Id { get; set; }
            public DerivedClassA ClassA1 { get; set; }
            public DerivedClassA ClassA2 { get; set; }
        }

        public class DerivedClassA : TheBaseClass
        {
            public string TitleA { get; set; }
        }

        public class DerivedClassB : TheBaseClass
        {
            public string TitleB { get; set; }
        }

        [Fact] // CodePlex 1747
        public void Abstract_TPH_base_type_not_explicitly_added_to_the_model_is_mapped_correctly()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Message1747>();
            modelBuilder.Entity<Comment1747>();

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            var mappings = databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Single().EntityTypeMappings.ToList();

            Assert.Equal(3, mappings.Count());
            Assert.True(mappings.Single(x => x.EntityType.Name == "MessageBase1747").IsHierarchyMapping);
            Assert.False(mappings.Single(x => x.EntityType.Name == "Message1747").IsHierarchyMapping);
            Assert.False(mappings.Single(x => x.EntityType.Name == "Comment1747").IsHierarchyMapping);
        }

        public abstract class MessageBase1747
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Contents { get; set; }
        }

        public class Message1747 : MessageBase1747
        {
        }

        public class Comment1747 : MessageBase1747
        {
            public MessageBase1747 Parent { get; set; }
            public int ParentId { get; set; }
        }

        [Fact]
        public void Single_concrete_class_has_no_discriminator_column()
        {
            const string databaseName = "MessageContext";
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MessageBase>();
            modelBuilder.Entity<Comment>();

            OnModelCreating(modelBuilder);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            var firstComment = new Comment
            {
                Title = "First comment",
                Contents = "The message"
            };

            var secondComment = new Comment
            {
                Title = "Second comment",
                Contents = "The message",
                Parent = firstComment
            };

            using (var context = new DbContext(databaseName, model.Compile()))
            {
                context.Database.Delete();
                context.Database.Create();

                context.Set<Comment>().Add(firstComment);
                context.Set<Comment>().Add(secondComment);

                context.SaveChanges();
            }

            model.DatabaseMapping.AssertValid();
            model.DatabaseMapping.Assert<Comment>().HasColumns("Id", "Title", "Contents", "ParentId");

            using (var context = new DbContext(databaseName, model.Compile()))
            {
                var returnedComment = context.Set<Comment>().Single(c => c.Title.Contains("Second"));
                Assert.Equal(secondComment.Title, returnedComment.Title);
                Assert.Equal(secondComment.Contents, returnedComment.Contents);
                Assert.Equal(secondComment.ParentId, returnedComment.ParentId);
            }
        }
    }


    public class Issue1776 : TestBase
    {
        [Fact]
        public void FK_name_uniquification_is_not_triggered_on_different_tables()
        {
            Database.SetInitializer(new DomainModelContextInitializer());

            using (var context = new DomainModelContext(
                        @"Data Source=.\Sqlexpress;Initial Catalog=Issue1776;Integrated Security=True;MultipleActiveResultSets=True"))
            {
                context.CustomOnModelCreating = modelBuilder =>
                {
                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    databaseMapping.Assert<COSwap>("COSwap")
                        .ColumnCountEquals(2);
                    databaseMapping.Assert<COSwap>(b => b.ReceiveLegId).DbEqual("ReceiveLegId", c => c.Name);
                    databaseMapping.Assert<COSwap>(b => b.ReceiveLegId).DbEqual("int", c => c.TypeName);
                    
                    databaseMapping.Assert<IRSwap>("LoanDeposit");
                    databaseMapping.Assert<IRSwap>(b => b.ReceiveLegId).DbEqual("ReceiveLegId", c => c.Name);
                    databaseMapping.Assert<IRSwap>(b => b.ReceiveLegId).DbEqual("int", c => c.TypeName);

                    databaseMapping.Assert<Swaption>("LoanDeposit");
                    databaseMapping.Assert<Swaption>(b => b.ReceiveLegId).DbEqual("ReceiveLegId", c => c.Name);
                    databaseMapping.Assert<Swaption>(b => b.ReceiveLegId).DbEqual("int", c => c.TypeName);

                    databaseMapping.Assert<LoanDeposit>("LoanDeposit")
                        .ColumnCountEquals(3);
                };
                context.Set<LoanDeposit>().Where(ld => ld.Id == 0).ToList();
            }
        }

        public class DomainModelContextInitializer : IDatabaseInitializer<DomainModelContext>
        {
            public void InitializeDatabase(DomainModelContext context)
            {
                if (!context.Database.Exists())
                {
                    context.Database.Delete();

                    new EmptyContext(context.Database.Connection.ConnectionString).Database.Create();
                    context.Database.ExecuteSqlCommand(CreateTablesScript);
                }
            }

            private const string CreateTablesScript =
                @"CREATE TABLE [dbo].[Instrument] 
                    (
                        [Id] int CONSTRAINT [DF_Instrument_Id]  NOT NULL,
                        CONSTRAINT [PK_Instrument] PRIMARY KEY CLUSTERED ([Id] ASC)
                    );

                    CREATE TABLE [dbo].[COSwap] 
                    (
                        [Id] int NOT NULL,
                        [ReceiveLegId] int NOT NULL,
                        CONSTRAINT [PK_COSwap] PRIMARY KEY CLUSTERED ([Id] ASC),
                        CONSTRAINT [FK_COSwap_Instrument] FOREIGN KEY ([Id]) REFERENCES [dbo].[Instrument] ([Id]) ON DELETE CASCADE
                    );

                    CREATE TABLE [dbo].[LoanDeposit]
                    (
                        [Id] int NOT NULL,
                        [Discriminator] TINYINT NOT NULL,
                        [ReceiveLegId] int NULL,
                        CONSTRAINT [PK_LoanDeposit] PRIMARY KEY CLUSTERED ([Id] ASC),
                        CONSTRAINT [FK_LoanDeposit_ReceiveLeg] FOREIGN KEY ([ReceiveLegId]) REFERENCES [dbo].[LoanDeposit] ([Id])
                    );";
        }

        public class DomainModelContext : DbContext
        {
            public DomainModelContext(string connectionString)
                : base(connectionString)
            {
            }

            public Action<DbModelBuilder> CustomOnModelCreating { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Instrument>().HasKey(p => p.Id);
                modelBuilder.Entity<Instrument>()
                    .Property(p => p.Id)
                    .IsRequired()
                    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

                modelBuilder.Entity<Instrument>().ToTable(typeof(Instrument).Name);

                modelBuilder.Entity<LoanDeposit>().Map<LoanDeposit>(m => m.Requires("Discriminator").HasValue(1));
                modelBuilder.Entity<LoanDeposit>().Map<IRSwap>(m => m.Requires("Discriminator").HasValue(3));
                modelBuilder.Entity<LoanDeposit>().Map<Swaption>(m => m.Requires("Discriminator").HasValue(4));
                modelBuilder.Entity<LoanDeposit>().ToTable(typeof(LoanDeposit).Name);

                modelBuilder.Entity<COSwap>().ToTable(typeof(COSwap).Name);

                modelBuilder.Ignore<NewLoanDeposit>();

                if (CustomOnModelCreating != null)
                {
                    CustomOnModelCreating(modelBuilder);
                }
            }
        }

        public class Instrument
        {
            public int Id { get; set; }
        }

        public class COSwap : Instrument
        {
            public int ReceiveLegId { get; set; }
            public Instrument ReceiveLeg { get; set; }
        }

        public class LoanDeposit : Instrument
        {
        }

        public class IRSwap : LoanDeposit
        {
            public int? ReceiveLegId { get; set; }

            public LoanDeposit ReceiveLeg { get; set; }
        }

        public class Swaption : IRSwap
        {
        }

        [Fact]
        public void Clashing_properties_in_TPH_are_uniquified()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Instrument>().ToTable(typeof(Instrument).Name);

            modelBuilder.Entity<LoanDeposit>().Map<LoanDeposit>(m => m.Requires("Discriminator").HasValue(0));
            modelBuilder.Entity<LoanDeposit>().Map<NewLoanDeposit>(m => m.Requires("Discriminator").HasValue(1));
            modelBuilder.Entity<LoanDeposit>().Map<IRSwap>(m => m.Requires("Discriminator").HasValue(2));
            modelBuilder.Entity<LoanDeposit>().Map<Swaption>(m => m.Requires("Discriminator").HasValue(3));
            modelBuilder.Entity<LoanDeposit>().ToTable(typeof(LoanDeposit).Name);

            modelBuilder.Entity<COSwap>().ToTable(typeof(COSwap).Name);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            databaseMapping.Assert<COSwap>("COSwap")
                .ColumnCountEquals(2);
            databaseMapping.Assert<COSwap>(b => b.ReceiveLegId).DbEqual("ReceiveLegId", c => c.Name);
            databaseMapping.Assert<COSwap>(b => b.ReceiveLegId).DbEqual("int", c => c.TypeName);

            databaseMapping.Assert<NewLoanDeposit>("LoanDeposit");
            databaseMapping.Assert<NewLoanDeposit>(b => b.ReceiveLegId).DbEqual("ReceiveLegId", c => c.Name);
            databaseMapping.Assert<NewLoanDeposit>(b => b.ReceiveLegId).DbEqual("uniqueidentifier", c => c.TypeName);

            databaseMapping.Assert<IRSwap>("LoanDeposit");
            databaseMapping.Assert<IRSwap>(b => b.ReceiveLegId).DbEqual("ReceiveLegId1", c => c.Name);
            databaseMapping.Assert<IRSwap>(b => b.ReceiveLegId).DbEqual("int", c => c.TypeName);

            databaseMapping.Assert<Swaption>("LoanDeposit");
            databaseMapping.Assert<Swaption>(b => b.ReceiveLegId).DbEqual("ReceiveLegId1", c => c.Name);
            databaseMapping.Assert<Swaption>(b => b.ReceiveLegId).DbEqual("int", c => c.TypeName);
            
            databaseMapping.Assert<LoanDeposit>("LoanDeposit")
                .ColumnCountEquals(4);
        }

        public class NewLoanDeposit : LoanDeposit
        {
            public Guid? ReceiveLegId { get; set; }
        }
    }

    #region Bug DevDiv#223284

    namespace Bug223284A
    {
        public class ITEmployee
        {
            public int ITEmployeeId { get; set; }
            public string Name { get; set; }
        }
    }

    namespace Bug223284B
    {
        public class ITEmployee : Bug223284A.ITEmployee
        {
        }

        public class IT_Context : DbContext
        {
            static IT_Context()
            {
                Database.SetInitializer<IT_Context>(null);
            }

            public DbSet<ITEmployee> Employees { get; set; }
        }

        public sealed class Bug223284Test : FunctionalTestBase
        {
            [Fact]
            public void Duplicate_entity_name_different_namespace_should_throw()
            {
                var context = new IT_Context();

                Assert.Throws<NotSupportedException>(() => context.Employees.Add(new ITEmployee()))
                      .ValidateMessage(
                          "SimpleNameCollision",
                          typeof(ITEmployee).FullName,
                          typeof(Bug223284A.ITEmployee).FullName,
                          typeof(ITEmployee).Name);
            }
        }
    }

    #endregion

    #region Bug DevDiv#175804

    namespace Bug175804
    {
        public class Dependent
        {
            public int principalnavigationkey1 { get; set; }
            public int Id { get; set; }
            public Principal PrincipalNavigation { get; set; }
        }

        public class Principal : BasePrincipal
        {
        }

        public class DerivedDependent : Dependent
        {
            public decimal DependentDerivedProperty1 { get; set; }
        }

        public class BasePrincipal
        {
            public DateTime BaseProperty { get; set; }
            public int Key1 { get; set; }
        }

        public sealed class Bug175804Test : FunctionalTestBase
        {
            [Fact]
            public void TPC_Ordering_Of_Configuration_Between_Related_Types()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<Dependent>().HasRequired(e => e.PrincipalNavigation);
                modelBuilder.Entity<BasePrincipal>().HasKey(e => e.Key1);

                modelBuilder.Entity<DerivedDependent>().Map(
                    mapping =>
                        {
                            mapping.MapInheritedProperties();
                            mapping.ToTable("DerivedDependent");
                        });
                modelBuilder.Entity<Principal>().Map(
                    mapping =>
                        {
                            mapping.MapInheritedProperties();
                            mapping.ToTable("Principal");
                        });

                modelBuilder.Entity<Dependent>().Map(
                    mapping =>
                        {
                            mapping.MapInheritedProperties();
                            mapping.ToTable("Dependent");
                        });
                modelBuilder.Entity<BasePrincipal>().Map(
                    mapping =>
                        {
                            mapping.MapInheritedProperties();
                            mapping.ToTable("BasePrincipal");
                        });

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                var derivedTypeMappings =
                    databaseMapping.EntityContainerMappings.Single().EntitySetMappings
                                   .First(es => es.EntitySet.Name.Contains("Dependent")).EntityTypeMappings;

                Assert.Equal(
                    "Principal",
                    derivedTypeMappings.ElementAt(0).MappingFragments[0].Table.ForeignKeyBuilders.ElementAt(0).
                                                                         PrincipalTable.Name);
                Assert.Equal(
                    "Principal",
                    derivedTypeMappings.ElementAt(1).MappingFragments[0].Table.ForeignKeyBuilders.ElementAt(0).
                                                                         PrincipalTable.Name);
            }
        }
    }

    #endregion

    #region Bug DevDiv#178590

    namespace BugDevDiv_178590
    {
        public abstract class A
        {
            public virtual int Id { get; set; }
            public virtual int? X { get; set; }
        }

        public abstract class B : A
        {
            public virtual int? Y { get; set; }
        }

        public class C : B
        {
            public virtual int? Z { get; set; }
        }

        #region Subclasses have no extra properties

        public abstract class D
        {
            public virtual int Id { get; set; }
            public virtual int? X { get; set; }
        }

        public class E : D
        {
        }

        public class F : E
        {
        }

        #endregion

        public sealed class Bug178590Test : FunctionalTestBase
        {
            [Fact]
            public void AbstractClasses_TPC()
            {
                var modelBuilder = new DbModelBuilder();

                // add .ToTable("B", "dbo") as workaround
                modelBuilder.Entity<A>();
                modelBuilder
                    .Entity<B>()
                    .Map(mapping => mapping.MapInheritedProperties());

                modelBuilder
                    .Entity<C>()
                    .Map(mapping => mapping.MapInheritedProperties())
                    .ToTable("C", "dbo");

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                var typeMappings = databaseMapping.EntityContainerMappings.Single().EntitySetMappings.ElementAt(0).EntityTypeMappings;

                Assert.Equal(1, typeMappings.Count());
                Assert.Equal("C", typeMappings.ElementAt(0).EntityType.Name);
                Assert.Equal(1, typeMappings.ElementAt(0).MappingFragments.Count);
                Assert.Equal(4, typeMappings.ElementAt(0).MappingFragments[0].ColumnMappings.Count());
            }

            [Fact]
            public void AbstractClasses_TPT()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<A>();
                modelBuilder
                    .Entity<B>()
                    .Map(mapping => mapping.MapInheritedProperties());

                modelBuilder
                    .Entity<C>()
                    .ToTable("C", "dbo");

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();
            }

            [Fact]
            public void SubClasses_NoProperties()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<D>();
                modelBuilder.Entity<E>();
                modelBuilder.Entity<F>();

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();
            }
        }
    }

    #endregion

    public sealed class Issue2616Test : FunctionalTestBase
    {
        [Fact]
        public void Can_hide_navigation_property_and_new_property_is_used()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<CustomStaffAccount>();
            modelBuilder.Entity<CustomDomain>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            var association = databaseMapping.GetAssociationSetMappings().Single().AssociationSet.ElementType;
            Assert.Equal(typeof(CustomStaffAccount).Name, association.TargetEnd.GetEntityType().Name);
            Assert.Equal(typeof(CustomDomain).Name, association.SourceEnd.GetEntityType().Name);

            using (var context = CreateContext(modelBuilder))
            {
                context.Database.Delete();
                context.Database.Initialize(force: false);

                var account = new CustomStaffAccount { Domain = new CustomDomain(), Name = "one" };
                ((StaffAccount)account).Name = 1;
                context.Set<CustomStaffAccount>().Add(account);
                context.SaveChanges();
            }

            using (var context = CreateContext(modelBuilder))
            {
                var account = context.Set<CustomStaffAccount>().Single();
                var domain = account.Domain;
                Assert.NotNull(domain);
                Assert.Null(((StaffAccount)account).Domain);
                Assert.Same(account, domain.Accounts.Single());
                Assert.Empty(((Domain)domain).Accounts);
                Assert.Equal("one", account.Name);
                Assert.Null(((StaffAccount)account).Name);

                context.Database.Delete();
            }
        }

        [Fact]
        public void Can_hide_navigation_property_and_if_StaffAccount_mapped_without_proxies_base_property_is_used()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<StaffAccount>();
            modelBuilder.Entity<CustomStaffAccount>();
            modelBuilder.Entity<CustomDomain>();

            VerifyBasePropertiesAreUsed(modelBuilder);
        }

        [Fact]
        public void Can_hide_navigation_property_and_if_Domain_mapped_without_proxies_base_property_is_used()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Domain>();
            modelBuilder.Entity<CustomStaffAccount>();
            modelBuilder.Entity<CustomDomain>();

            VerifyBasePropertiesAreUsed(modelBuilder);
        }

        [Fact]
        public void Can_hide_navigation_property_and_both_bases_mapped_without_proxies_base_property_is_used()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<StaffAccount>();
            modelBuilder.Entity<Domain>();
            modelBuilder.Entity<CustomStaffAccount>();
            modelBuilder.Entity<CustomDomain>();

            VerifyBasePropertiesAreUsed(modelBuilder);
        }

        private void VerifyBasePropertiesAreUsed(DbModelBuilder modelBuilder)
        {
            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            var association = databaseMapping.GetAssociationSetMappings().Single().AssociationSet.ElementType;
            var targetIsDependent = association.TargetEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many;
            var dependentEnd = targetIsDependent ? association.TargetEnd : association.SourceEnd;
            var principalEnd = targetIsDependent ? association.SourceEnd : association.TargetEnd;
            Assert.Equal(typeof(StaffAccount).Name, dependentEnd.GetEntityType().Name);
            Assert.Equal(typeof(Domain).Name, principalEnd.GetEntityType().Name);

            using (var context = CreateContext(modelBuilder))
            {
                context.Database.Delete();
                context.Database.Initialize(force: false);

                var account = new CustomStaffAccount();
                account.Name = "one";
                ((StaffAccount)account).Name = 1;
                ((StaffAccount)account).Domain = new CustomDomain();
                context.Set<CustomStaffAccount>().Add(account);
                context.SaveChanges();
            }

            using (var context = CreateContext(modelBuilder))
            {
                context.Configuration.ProxyCreationEnabled = false;

                var lazyAccount = context.Set<CustomStaffAccount>().AsNoTracking().Single();
                Assert.Null(((StaffAccount)lazyAccount).Domain);
                Assert.Null(lazyAccount.Domain);

                var lazyDomain = context.Set<CustomDomain>().AsNoTracking().Single();
                Assert.Empty(((Domain)lazyDomain).Accounts);
                Assert.Empty(lazyDomain.Accounts);

                var account = context.Set<CustomStaffAccount>().Include(a => ((StaffAccount)a).Domain).Single();
                var domain = ((StaffAccount)account).Domain;
                Assert.NotNull(domain);
                Assert.Null(account.Domain);
                Assert.Same(account, domain.Accounts.Single());
                Assert.Empty(((CustomDomain)domain).Accounts);
                Assert.Null(account.Name);
                Assert.Equal(1, ((StaffAccount)account).Name);

                context.Database.Delete();
            }
        }

        [Fact]
        public void Can_hide_navigation_property_and_both_bases_mapped_with_proxies_base_property_is_used()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<StaffAccount>().HasOptional(a => a.Domain).WithMany();
            modelBuilder.Entity<Domain>().Ignore(d => d.Accounts);
            modelBuilder.Entity<CustomStaffAccountSameField>();
            modelBuilder.Entity<CustomDomainNonVirtual>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            var association = databaseMapping.GetAssociationSetMappings().Single().AssociationSet.ElementType;
            Assert.Equal(typeof(Domain).Name, association.TargetEnd.GetEntityType().Name);
            Assert.Equal(typeof(StaffAccount).Name, association.SourceEnd.GetEntityType().Name);

            using (var context = CreateContext(modelBuilder))
            {
                context.Database.Delete();
                context.Database.Initialize(force: false);

                var account = new CustomStaffAccountSameField();
                account.Name = "1";
                account.Domain = new CustomDomainNonVirtual();
                context.Set<CustomStaffAccountSameField>().Add(account);
                context.SaveChanges();
            }

            using (var context = CreateContext(modelBuilder))
            {
                var account = context.Set<CustomStaffAccountSameField>().Single();

                var domain = account.Domain;
                Assert.NotNull(domain);
                Assert.NotNull(((StaffAccount)account).Domain);
                Assert.Empty(((Domain)domain).Accounts);
                Assert.Empty(domain.Accounts);
                Assert.Equal("1", account.Name);
                Assert.Equal(1, ((StaffAccount)account).Name);

                context.Database.Delete();
            }
        }

        [Fact]
        public void Can_hide_navigation_property_and_ignore_new_property()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<CustomStaffAccount>().Ignore(c => c.Domain);
            modelBuilder.Entity<CustomDomain>().Ignore(d => d.Accounts);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Empty(databaseMapping.GetAssociationSetMappings());
        }

        [Fact]
        public void If_hidden_navigation_property_and_bases_mapped_ignoring_new_property_throws()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<StaffAccount>();
            modelBuilder.Entity<Domain>();
            modelBuilder.Entity<CustomStaffAccount>().Ignore(c => c.Domain);
            modelBuilder.Entity<CustomDomain>().Ignore(d => d.Accounts);

            Assert.Throws<InvalidOperationException>(() => BuildMapping(modelBuilder))
                .ValidateMessage("CannotIgnoreMappedBaseProperty", "Accounts", typeof(CustomDomain).FullName, typeof(Domain).FullName);
        }

        [Fact]
        public void Can_hide_navigation_property_and_ignore_it()
        {
            DbConfiguration.DependencyResolver.GetService<AttributeProvider>().ClearCache();

            using (var staffConfiguration = new DynamicTypeDescriptionConfiguration<StaffAccount>())
            {
                using (var domainConfiguration = new DynamicTypeDescriptionConfiguration<Domain>())
                {
                    staffConfiguration.SetPropertyAttributes(b => b.Domain, new NotMappedAttribute());
                    domainConfiguration.SetPropertyAttributes(b => b.Accounts, new NotMappedAttribute());
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<CustomStaffAccount>();
                    modelBuilder.Entity<CustomDomain>();

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    Assert.Empty(databaseMapping.GetAssociationSetMappings());
                }
            }

            DbConfiguration.DependencyResolver.GetService<AttributeProvider>().ClearCache();
        }

        [Fact]
        public void Can_hide_navigation_property_and_ignore_it_if_bases_mapped()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<StaffAccount>().Ignore(c => c.Domain);
            modelBuilder.Entity<Domain>().Ignore(d => d.Accounts);
            modelBuilder.Entity<CustomStaffAccount>();
            modelBuilder.Entity<CustomDomain>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Empty(databaseMapping.GetAssociationSetMappings());
        }

        private DbContext CreateContext(DbModelBuilder modelBuilder)
        {
            return new DbContext("Bug2616Test", modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo).Compile());
        }

        public abstract class Domain
        {
            protected Domain()
            {
                Accounts = new List<StaffAccount>();
            }

            public int Id { get; private set; }

            public virtual ICollection<StaffAccount> Accounts { get; set; }
        }
        
        public class CustomDomain
            : Domain
        {
            public CustomDomain()
            {
                Accounts = new List<CustomStaffAccount>();
            }

            public new virtual ICollection<CustomStaffAccount> Accounts { get; set; }
        }

        public class CustomDomainNonVirtual
            : Domain
        {
            public CustomDomainNonVirtual()
            {
                Accounts = new List<CustomStaffAccountSameField>();
            }

            public new ICollection<CustomStaffAccountSameField> Accounts { get; set; }
        }

        public abstract class StaffAccount
        {
            public int Id { get; private set; }

            public virtual Domain Domain { get; set; }

            public virtual int? Name { get; set; }
        }

        public class CustomStaffAccount
            : StaffAccount
        {
            public new virtual CustomDomain Domain { get; set; }

            public new virtual string Name { get; set; }
        }

        public class CustomStaffAccountSameField
            : StaffAccount
        {
            public new virtual CustomDomainNonVirtual Domain
            {
                get { return base.Domain as CustomDomainNonVirtual; }
                set { base.Domain = value; }
            }

            public new virtual string Name
            {
                get { return base.Name == null ? null : base.Name.ToString(); }
                set { base.Name = value == null ? null : (int?)Int32.Parse(value); }
            }
        }
    }

    #region Bug165027

    namespace Bug165027
    {
        public class Dependent1
        {
            public DateTimeOffset key1 { get; set; }
            public Principal1 PrincipalNavigation { get; set; }
        }

        public abstract class Principal1 : BasePrincipal1
        {
            public Dependent1 DependentNavigation { get; set; }
        }

        public class BasePrincipal1
        {
            public short BaseProperty { get; set; }
            public DateTimeOffset? Key1 { get; set; }
        }

        public class DerivedPrincipal1 : Principal1
        {
            public decimal PrincipalDerivedProperty1 { get; set; }
        }

        public sealed class Bug165027Repro : FunctionalTestBase
        {
            [Fact]
            public void Bug165027Test()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<BasePrincipal1>().ToTable("BasePrincipal");
                modelBuilder.Entity<BasePrincipal1>().HasKey(e => e.Key1);
                modelBuilder.Entity<Principal1>().HasOptional(e => e.DependentNavigation).WithRequired(
                    e => e.PrincipalNavigation);

                modelBuilder.Entity<DerivedPrincipal1>().Map(
                    mapping =>
                        {
                            mapping.MapInheritedProperties();
                            mapping.ToTable("DerivedPrincipal");
                        });

                modelBuilder.Entity<Dependent1>().HasKey(e => e.key1);

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();
            }
        }
    }

    #endregion

    #region Bug178568

    namespace Bug178568
    {
        using System.Data.Entity.Core.Metadata.Edm;

        public abstract class A
        {
            public virtual int Id { get; set; }
            public virtual int? X { get; set; }
        }

        public class B : A
        {
            public virtual int? Y { get; set; }
        }

        public class C
        {
            public virtual int Id { get; set; }
            public virtual int? X { get; set; }
            public virtual int? Y { get; set; }
        }

        public sealed class Bug178568Repro : TestBase
        {
            [Fact]
            public void TPC_Identity_ShouldPropagate()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder
                    .Entity<A>()
                    .HasKey(a => a.Id);

                modelBuilder
                    .Entity<A>()
                    .Property(a => a.Id)
                    .IsRequired()
                    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

                modelBuilder
                    .Entity<B>()
                    .Map(mapping => mapping.MapInheritedProperties())
                    .ToTable("B", "dbo");

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                databaseMapping.Assert<B>("B")
                               .Column("Id").DbEqual(
                                   StoreGeneratedPattern.Identity,
                                   c => c.StoreGeneratedPattern);
            }

            [Fact]
            public void TPC_IdentityNotExplicit_ShouldNotPropagate()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<A>();

                modelBuilder
                    .Entity<B>()
                    .Map(mapping => mapping.MapInheritedProperties())
                    .ToTable("B", "dbo");

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                databaseMapping.Assert<B>("B")
                               .Column("Id").DbEqual(
                                   StoreGeneratedPattern.None,
                                   c => c.StoreGeneratedPattern);
            }

            [Fact]
            public void TPT_Identity_ShouldNotPropagate()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder
                    .Entity<A>()
                    .HasKey(a => a.Id);

                modelBuilder
                    .Entity<A>()
                    .Property(a => a.Id)
                    .IsRequired()
                    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

                modelBuilder
                    .Entity<B>()
                    .ToTable("B", "dbo");

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                databaseMapping.Assert<A>("A")
                               .Column("Id").DbEqual(
                                   StoreGeneratedPattern.Identity,
                                   c => c.StoreGeneratedPattern);
                databaseMapping.Assert<B>("B")
                               .Column("Id").DbEqual(
                                   StoreGeneratedPattern.None,
                                   c => c.StoreGeneratedPattern);
            }

            [Fact]
            public void EntitySplitting_Identity_ShouldNotPropagate()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder
                    .Entity<C>()
                    .HasKey(a => a.Id);

                modelBuilder
                    .Entity<C>()
                    .Property(a => a.Id)
                    .IsRequired()
                    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

                modelBuilder.Entity<C>()
                            .Map(
                                m =>
                                    {
                                        m.Properties(
                                            c => new
                                                {
                                                    c.Id,
                                                    c.X
                                                });
                                        m.ToTable("CX");
                                    })
                            .Map(
                                m =>
                                    {
                                        m.Properties(
                                            c => new
                                                {
                                                    c.Id,
                                                    c.Y
                                                });
                                        m.ToTable("CY");
                                    });

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                databaseMapping.Assert<C>("CX")
                               .Column("Id").DbEqual(
                                   StoreGeneratedPattern.Identity,
                                   c => c.StoreGeneratedPattern);
                databaseMapping.Assert<C>("CY")
                               .Column("Id").DbEqual(
                                   StoreGeneratedPattern.None,
                                   c => c.StoreGeneratedPattern);
            }
        }
    }

    #endregion

    #region Bug336566

    namespace Bug336566
    {
        using System.Data.Entity.Core.Metadata.Edm;

        public class A
        {
            public virtual int Id { get; set; }
            public virtual int? X { get; set; }
        }

        public class B : A
        {
            public virtual int? Y { get; set; }
        }

        public class C
        {
            public virtual int Id { get; set; }
            public virtual int? X { get; set; }
            public virtual int? Y { get; set; }
        }

        public sealed class Bug336566Repro : TestBase
        {
            [Fact]
            public void TPC_IdentityNotExplicit_ShouldNotPropagate()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<A>();

                modelBuilder
                    .Entity<B>()
                    .Map(mapping => mapping.MapInheritedProperties())
                    .ToTable("B", "dbo");

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                databaseMapping.Assert<A>("A")
                               .Column("Id").DbEqual(
                                   StoreGeneratedPattern.None,
                                   c => c.StoreGeneratedPattern);

                databaseMapping.Assert<B>("B")
                               .Column("Id").DbEqual(
                                   StoreGeneratedPattern.None,
                                   c => c.StoreGeneratedPattern);
            }

            [Fact]
            public void NoIdentityExplicit()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder
                    .Entity<A>()
                    .Property(a => a.Id)
                    .IsRequired()
                    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                databaseMapping.Assert<A>("A")
                               .Column("Id").DbEqual(
                                   StoreGeneratedPattern.None,
                                   c => c.StoreGeneratedPattern);
            }

            [Fact]
            public void TPT_Identity_ShouldKickIn()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder
                    .Entity<A>();

                modelBuilder
                    .Entity<B>()
                    .ToTable("B", "dbo");

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                databaseMapping.Assert<A>("A")
                               .Column("Id").DbEqual(
                                   StoreGeneratedPattern.Identity,
                                   c => c.StoreGeneratedPattern);
                databaseMapping.Assert<B>("B")
                               .Column("Id").DbEqual(
                                   StoreGeneratedPattern.None,
                                   c => c.StoreGeneratedPattern);
            }

            [Fact]
            public void EntitySplitting_Identity_ShouldKickIn()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder
                    .Entity<C>()
                    .HasKey(a => a.Id);

                modelBuilder.Entity<C>()
                            .Map(
                                m =>
                                    {
                                        m.Properties(
                                            c => new
                                                {
                                                    c.Id,
                                                    c.X
                                                });
                                        m.ToTable("CX");
                                    })
                            .Map(
                                m =>
                                    {
                                        m.Properties(
                                            c => new
                                                {
                                                    c.Id,
                                                    c.Y
                                                });
                                        m.ToTable("CY");
                                    });

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                databaseMapping.Assert<C>("CX")
                               .Column("Id").DbEqual(
                                   StoreGeneratedPattern.Identity,
                                   c => c.StoreGeneratedPattern);
                databaseMapping.Assert<C>("CY")
                               .Column("Id").DbEqual(
                                   StoreGeneratedPattern.None,
                                   c => c.StoreGeneratedPattern);
            }
        }
    }

    #endregion

    #region Contexts for TPT/TPC with different PK column names

    public abstract class BaseContextForPkNaming : DbContext
    {
        public DbSet<BaseForPKNaming> Bases { get; set; }
        public DbSet<DerivedForPKNaming> Deriveds { get; set; }
    }

    public class ContextForPkNamingTPC : BaseContextForPkNaming
    {
        public ContextForPkNamingTPC()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<ContextForPkNamingTPC>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BaseForPKNaming>().Map(
                m =>
                    {
                        m.MapInheritedProperties();
                        m.ToTable("base_table");
                    });
            modelBuilder.Entity<DerivedForPKNaming>().Map(
                m =>
                    {
                        m.MapInheritedProperties();
                        m.ToTable("derived_table");
                    });

            modelBuilder.Entity<BaseForPKNaming>().Property(e => e.Id).HasColumnName("base_id");
            modelBuilder.Entity<DerivedForPKNaming>().Property(e => e.Id).HasColumnName("derived_id");
            modelBuilder.Entity<BaseForPKNaming>().Property(e => e.Foo).HasColumnName("base_foo");
            modelBuilder.Entity<DerivedForPKNaming>().Property(e => e.Foo).HasColumnName("derived_foo");
        }
    }

    public class ContextForPkNamingTPT : BaseContextForPkNaming
    {
        public ContextForPkNamingTPT()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<ContextForPkNamingTPT>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BaseForPKNaming>().ToTable("base_table");
            modelBuilder.Entity<DerivedForPKNaming>().ToTable("derived_table");

            modelBuilder.Entity<BaseForPKNaming>().Property(e => e.Id).HasColumnName("base_id");
            modelBuilder.Entity<DerivedForPKNaming>().Property(e => e.Id).HasColumnName("derived_id");
        }
    }

    public class BaseForPKNaming
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Foo { get; set; }
    }

    public class DerivedForPKNaming : BaseForPKNaming
    {
        public string Bar { get; set; }
    }

    [Table("Horses")]
    public class TPTHorse
    {
        public int Id { get; set; }
    }

    [Table("Unicorns")]
    public class TPTUnicorn : TPTHorse
    {
        public int HornLength { get; set; }
    }

    [Table("HornedPegasuses")]
    public class TPTHornedPegasus : TPTUnicorn
    {
        public int Wingspan { get; set; }
    }

    #endregion

    #region Bug335965

    namespace Bug335965
    {
        public class A
        {
            public int Id { get; set; }
            public string X { get; set; }
        }

        public abstract class B : A
        {
            public string Y { get; set; }
        }

        public class C : B
        {
            public string Z { get; set; }
        }

        public class A2
        {
            public int Id { get; set; }
            public string X { get; set; }
        }

        public abstract class B2 : A2
        {
            public string Y { get; set; }
        }

        public class C2 : B2
        {
            public string Z { get; set; }
        }

        public class D2 : B2
        {
            public string W { get; set; }
        }

        public sealed class Bug335965Repro : TestBase
        {
            [Fact]
            public void ExplicitDiscriminatorShouldNotBeNullable()
            {
                var modelBuilder = new DbModelBuilder();

                // Adding this configuration makes the discriminator nullable.
                modelBuilder.Entity<B>();

                modelBuilder.Entity<A>().Map(m => m.Requires("Disc").HasValue(17));
                modelBuilder.Entity<C>().Map(m => m.Requires("Disc").HasValue(7));

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                databaseMapping
                    .Assert<A>("A")
                    .Column("Disc")
                    .DbEqual(false, c => c.Nullable);
            }

            [Fact]
            public void InvalidMappingSubtypeHasNoDiscriminatorCondition()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<A2>().Map(m => m.Requires("Disc").HasValue(17));
                modelBuilder.Entity<C2>().Map(m => m.Requires("Disc").HasValue(7));

                modelBuilder.Entity<B2>();
                modelBuilder.Entity<D2>();

                var databaseMapping = BuildMapping(modelBuilder);

                Assert.Throws<MappingException>(() => databaseMapping.AssertValid(true));
            }

            [Fact]
            public void ImplicitDiscriminatorShouldNotBeNullable()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<A>();
                modelBuilder.Entity<C>();
                modelBuilder.Entity<B>();

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                databaseMapping.Assert<A>("A")
                               .Column("Discriminator")
                               .DbEqual(false, c => c.Nullable);
            }
        }
    }

    #endregion

    #region Bug339467

    namespace Bug339467
    {
        public class A
        {
            public int Id { set; get; }
            public string Name { set; get; }
        }

        public abstract class B : A
        {
        }

        public class C : B
        {
            public string CName { set; get; }
        }

        public sealed class Bug339467Repro : TestBase
        {
            [Fact]
            public void SimpleTPTWithAbstractWithNoPropertiesInBetween()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<A>();
                modelBuilder.Entity<C>().ToTable("C");

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();
            }
        }
    }

    #endregion

    #region Bug336706

    namespace Bug336706
    {
        public class Dependent : BaseDependent
        {
            public int PrincipalNavigationId { get; set; }
            public BaseDependent PrincipalNavigation { get; set; }
        }

        public abstract class BaseDependent
        {
            public int Id { get; set; }
            public ICollection<Dependent> DependentNavigation { get; set; }

            public BaseDependent()
            {
                DependentNavigation = new List<Dependent>();
            }
        }

        public sealed class Bug336706Repro : TestBase
        {
            [Fact]
            public void TPH_with_self_ref_FK_on_derived_type_has_non_nullable_FK_when_base_type_is_abstract()
            {
                var modelBuilder = new DbModelBuilder();
                modelBuilder.Entity<Dependent>().HasRequired(e => e.PrincipalNavigation).WithMany(
                    e => e.DependentNavigation)
                            .WillCascadeOnDelete(false);
                modelBuilder.Entity<Dependent>().Map(mapping => { mapping.Requires("DiscriminatorValue").HasValue(1); });

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.Assert<Dependent>("BaseDependents")
                               .HasForeignKeyColumn("PrincipalNavigationId")
                               .DbEqual(false, t => t.Properties.Single(c => c.Name == "PrincipalNavigationId").Nullable);

                databaseMapping.AssertValid();
            }
        }
    }

    #endregion
}
